using C7GameData;
using C7GameData.AIData;
using C7Engine.Pathing;
using System.Collections.Generic;
using System.Linq;
using Serilog;

namespace C7Engine.AI.UnitAI {
	class DefenderAI : C7GameData.UnitAI {
		private static ILogger log = Log.ForContext<DefenderAI>();
		public DefenderAIData data;

		public static DefenderAIData MakeAiDataForDefendInPlace(MapUnit unit, Player player) {
			DefenderAIData ai = new DefenderAIData();
			ai.goal = DefenderAIData.DefenderGoal.DEFEND_CITY;
			ai.destination = unit.location;
			log.Information("Set defender AI for " + unit + " with destination of " + ai.destination);
			return ai;
		}

		public static DefenderAIData MakeAiDataForDefendAtRiskCity(MapUnit unit, Player player) {
			City cityToDefend = FindAtRiskCityToDefend(unit, player);

			DefenderAIData ai = new DefenderAIData();
			ai.destination = cityToDefend.location;
			ai.goal = DefenderAIData.DefenderGoal.DEFEND_CITY;

			PathingAlgorithm algorithm = PathingAlgorithmChooser.GetAlgorithm(unit);
			ai.pathToDestination = algorithm.PathFrom(unit.location, ai.destination);

			log.Information($"Unit {unit} tasked with defending {cityToDefend.name}");
			return ai;
		}

		public DefenderAI(DefenderAIData d) {
			data = d;
		}

		C7GameData.UnitAI.Result C7GameData.UnitAI.PlayTurnImpl(Player player, MapUnit unit) {
			if (data.destination == unit.location) {
				if (!unit.isFortified) {
					unit.fortify();
					log.Information("Fortifying " + unit + " at " + data.destination);
				}
				return C7GameData.UnitAI.Result.Done;
			} else {
				log.Debug("Moving defender towards " + data.destination);
				return this.TryToMoveAlongPath(unit, data.pathToDestination, /*allowCombat=*/false);
			}
		}

		public string SummarizePlan() {
			return "DefenderAI: " + data.ToString();
		}

		/**
		 * Finds a nearby city that could use extra defenders.
		 *
		 * This is not a brilliant method, with many flaws such as whether the 
		 * city needs more defenders, or if the units present are defenders.
		 */
		private static City FindAtRiskCityToDefend(MapUnit unit, Player player) {
			if (player.cities.Count == 0) {
				return City.NONE;
			}

			// Assign a score to each city, where the highest score is the city
			// we want to send our unit to.
			Dictionary<City, float> cityScores = new();
			foreach (City c in player.cities) {
				int score = 0;
				if (c.location.unitsOnTile.Count(u => u.CanDefendOnLand()) < 3) {
					// Add to the score if there aren't many defenders.
					score += 10;
				}

				// Penalize cities for being far away.
				score -= c.location.distanceTo(unit.location);

				cityScores.Add(c, score);
			}

			// Make the city less important if there are already units on the
			// way.
			foreach (MapUnit u in player.units) {
				if (u.currentAI is DefenderAI defenderAi) {
					if (defenderAi.data.destination.cityAtTile != null) {
						cityScores[defenderAi.data.destination.cityAtTile] -= 3;
					}
				}
			}

			return cityScores.MaxBy(t => t.Value).Key;
		}
	}
}
