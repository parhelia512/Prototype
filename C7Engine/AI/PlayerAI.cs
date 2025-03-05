using System;
using System.Collections.Generic;
using System.Linq;
using C7Engine.Pathing;
using C7GameData;
using C7GameData.AIData;
using C7Engine.AI;
using C7Engine.AI.StrategicAI;
using C7Engine.AI.UnitAI;
using Serilog;
using System.Diagnostics;

namespace C7Engine {
	public class PlayerAI {
		private static ILogger log = Log.ForContext<PlayerAI>();

		public static void PlayTurn(Player player, Random rng, List<Tech> techs) {
			if (player.isHuman || player.isBarbarians) {
				return;
			}
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			log.Information("-> Begin " + player.civilization.cityNames[0] + " turn");

			if (player.turnsUntilPriorityReevaluation == 0) {
				log.Information("Re-evaluating strategic priorities for " + player);
				List<StrategicPriority> priorities = StrategicPriorityArbitrator.Arbitrate(player);
				player.strategicPriorityData.Clear();
				foreach (StrategicPriority priority in priorities) {
					player.strategicPriorityData.Add(priority);
				}
				player.turnsUntilPriorityReevaluation = 15 + GameData.rng.Next(10);

				string summary = string.Join(',', player.strategicPriorityData);
				log.Information($"Player {player.civilization.name} now has priorities of: {summary}. {player.turnsUntilPriorityReevaluation} turns until next re-evaluation");
			} else {
				player.turnsUntilPriorityReevaluation--;
			}

			if (player.currentlyResearchedTech == null) {
				Tech toResearch = PickTechToResearch(player, techs);
				if (toResearch == null) {
					log.Information($"Player {player.civilization.name} has no techs available to research.");
					player.SetCurrentlyResearchedTech(null);
				} else {
					log.Information($"Player {player.civilization.name} is researching {toResearch.Name}.");
					player.SetCurrentlyResearchedTech(toResearch.id);
				}
			}

			// Reorder the list so that settlers are last, giving our escorts a
			// chance to configure themselves without wasting a turn.
			player.units.Sort((x, y) => (x.unitType.name == "Settler").CompareTo(y.unitType.name == "Settler"));

			// Any time we have an unescorted settler, wake up any other units
			// on the same tile to force us to re-evaluate whether they should
			// be an escort. Otherwise can have perfectly fine escorts sitting
			// there fortified.
			foreach (MapUnit u in player.units) {
				if (u.currentAI is SettlerAI settlerAi && settlerAi.data.escort == null) {
					foreach (MapUnit uu in u.location.unitsOnTile) {
						uu.wake();
					}
				}
			}

			//Do things with units.  Copy into an array first to avoid collection-was-modified exception
			foreach (MapUnit unit in player.units.ToArray()) {
				// Don't waste time recalculating behaviors for fortified units.
				// This means we'll have to unfortify all our units after
				// interesting events like war declarations, but this seems like
				// a good tradeoff for faster turns.
				if (unit.isFortified) {
					continue;
				}

				// For each unit, if there's already an AI task assigned, it will attempt to complete its goal.
				// It may fail due to conditions having changed since that goal was assigned; in that case it will
				// get a new task to try to complete.
				//
				// Cap our attempts at 2 to avoid getting stuck in bad situations.
				for (int attempt = 0; attempt < 2; ++attempt) {
					if (unit.currentAI == null) {
						unit.currentAI = GetAIForUnit(unit, player);
					}

					// If the unit is still the process of doing its plan, allow
					// it to continue next turn.
					UnitAI.Result result = unit.currentAI.PlayTurn(player, unit);
					if (result == UnitAI.Result.InProgress) {
						break;
					}

					if (result == UnitAI.Result.Error) {
						unit.currentAI = null;
						break;
					}

					if (unit.hitPointsRemaining <= 0 || unit.isFortified) {
						unit.currentAI = null;
						break;
					}

					// Otherwise we need a new plan for next turn. Pick it now
					// to avoid things like new units being preferred for
					// exploration instead of units already far away from home
					// for exploration.
					unit.currentAI = GetAIForUnit(unit, player);
				}

				player.tileKnowledge.AddTilesToKnown(unit.location);
			}

			log.Information("-> End " + player.civilization.cityNames[0] + $" turn {stopwatch.ElapsedMilliseconds} milliseconds");
		}

		public static UnitAI GetAIForUnit(MapUnit unit, Player player) {
			//figure out an AI behavior
			//TODO: Use strategies, not names
			if (unit.unitType.name == "Settler") {
				return new SettlerAI(SettlerAI.MakeAiData(unit, player));
			} else if (unit.unitType.name == "Worker") {
				return new WorkerAI(WorkerAI.MakeAiData(unit, player));
			} else if (unit.location.cityAtTile != null && unit.CanDefendOnLand() && unit.location.unitsOnTile.Count(u => u.CanDefendOnLand() && u != unit) == 0) {
				return new DefenderAI(DefenderAI.MakeAiDataForDefendInPlace(unit, player));
			} else if (GetCombatAIIfUnitCanAttackNearbyBarbCamp(unit, player) is UnitAI unitAI && unitAI != null) {
				log.Information("Set unit " + unit + " to take out barb camp");
				return unitAI;
			} else if (unit.unitType.name == "Catapult") {
				//For now tell catapults to sit tight.  It's getting really annoying watching them pointlessly bombard barb camps forever
				return new DefenderAI(DefenderAI.MakeAiDataForDefendInPlace(unit, player));
			} else {
				// If there's an unescorted settler, escort it.
				EscortAIData? maybeEscortData = EscortAI.MaybeMakeAiData(unit, player);
				if (maybeEscortData != null) {
					return new EscortAI(maybeEscortData);
				}

				// As long as we don't have too many explorers yet of this unit's
				// type (land vs sea), start a new exploring unit.
				int numRelevantExplorers = 0;
				foreach (MapUnit u in player.units) {
					if (u.currentAI is ExplorerAI && u.IsLandUnit() == unit.IsLandUnit()) {
						++numRelevantExplorers;
					}
				}

				if (numRelevantExplorers < 10) {
					ExplorerAIData? maybeAiData = ExplorerAI.MaybeMakeAiData(unit, player);
					if (maybeAiData != null) {
						return new ExplorerAI(maybeAiData);
					}
				}

				//Nowhere to explore or too many explorers.  What to do now?
				//Priority 1: Adequate defense of cities.
				//Priority 2: Clearing out barbs
				//Priority 3: Defending chokepoints
				//Priority 4: ???
				//Priority 5: Profit!
				//(Realistically, as we evolve there will be a lot of options, such as defending borders from barbs, preparing attackers on other civs, defending
				//resources.  I expect we'll have some sort of arbiter that decides between competing priorities, with each being given a score as to how important
				//they are, including a weight by how far away the task is.  But this will evolve gradually over a long time)

				//As of today (4/7/2022), let's tackle just one of those - adequate defense of cities.  The AI is really good at losing cities to barbs right now,
				//and that's a problem.
				return new DefenderAI(DefenderAI.MakeAiDataForDefendAtRiskCity(unit, player));
			}
		}

		private static UnitAI GetCombatAIIfUnitCanAttackNearbyBarbCamp(MapUnit unit, Player player) {
			if (unit.unitType.attack <= 0) {
				return null;
			}

			List<Tile> reachableBarbCampsTiles = player.tileKnowledge.AllKnownTiles()
				.Where(t => unit.CanEnterTile(t, true) && t.hasBarbarianCamp).ToList();

			Tile closestBarbCamp = Tile.NONE;
			int closestBarbDistance = int.MaxValue;
			foreach (Tile t in reachableBarbCampsTiles) {
				int crowDistance = t.distanceTo(unit.location);
				if (crowDistance < closestBarbDistance) {
					closestBarbCamp = t;
					closestBarbDistance = crowDistance;
				}
			}

			if (closestBarbDistance <= 3) {
				CombatAIData caid = new CombatAIData();
				caid.destination = closestBarbCamp;

				PathingAlgorithm algorithm = PathingAlgorithmChooser.GetAlgorithm(unit);
				caid.path = algorithm.PathFrom(unit.location, closestBarbCamp);
				return new CombatAI(caid);
			}
			return null;
		}

		private static Tech PickTechToResearch(Player player, List<Tech> techs) {
			List<Tech> possibleTechs = new();

			// Figure out what techs we're allowed to research.
			foreach (Tech tech in techs) {
				if (tech.EraCivilopediaName != player.eraCivilopediaName) {
					continue;
				}
				if (player.knownTechs.Contains(tech.id)) {
					continue;
				}

				bool prereqsKnown = true;
				foreach (Tech prereq in tech.Prerequisites) {
					if (!player.knownTechs.Contains(prereq.id)) {
						prereqsKnown = false;
						break;
					}
				}
				if (!prereqsKnown) {
					continue;
				}
				possibleTechs.Add(tech);
			}

			if (possibleTechs.Count == 0) {
				return null;
			}

			// Details on how Civ3 does it: https://forums.civfanatics.com/threads/what-will-the-ai-research-next.45559/
			return possibleTechs[(int)GameData.rng.NextInt64(possibleTechs.Count)];
		}
	}
}
