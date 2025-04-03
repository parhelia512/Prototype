using System;
using System.Collections.Generic;
using System.Linq;
using C7Engine.AI.StrategicAI;
using C7GameData.Save;
using Serilog;

namespace C7GameData {
	public class Player {
		private static ILogger log = Log.ForContext<Player>();

		public ID id { get; internal set; }
		public int colorIndex;
		public bool isBarbarians = false;
		//TODO: Refactor front-end so it sends player GUID with requests.
		//We should allow multiple humans, this is a temporary measure.
		public bool isHuman = false;
		public bool hasPlayedThisTurn = false;

		// Has this player been defeated?
		public bool defeated = false;

		public Civilization civilization;
		internal int cityNameIndex = 0;

		public List<MapUnit> units = new List<MapUnit>();
		public List<City> cities = new List<City>();
		public TileKnowledge tileKnowledge { get; private set; }

		//Ordered list of priority data.  First is most important.
		public List<StrategicPriority> strategicPriorityData = new List<StrategicPriority>();

		// A map from player id to the relationship this player has with the other player.
		public Dictionary<ID, PlayerRelationship> playerRelationships = new();

		// The list of techs known by this player.
		public HashSet<ID> knownTechs = new();

		// The tech the player is currently researching.
		public ID currentlyResearchedTech { get; private set; }

		// The civilopedia name of the era this player is in.
		//
		// The civilopedia name is what is used for art lookups, not the actual
		// name.
		public string eraCivilopediaName;

		public int turnsUntilPriorityReevaluation = 0;

		// The values of the science/happiness/tax sliders (tax is implicit)
		// A value of 1 => 10%, a value of 10 => 100%.
		//
		// INVARIANT: LuxuryRate + ScienceRate + TaxRate = 10
		public int luxuryRate = 0;
		public int scienceRate = 5;
		public int taxRate = 5;

		// The amount of gold this player has.
		private int _gold = 0;
		public int gold {
			get => _gold;
			set {
				if (value < 0) {
					throw new Exception($"bad gold value of {value} for {this}");
				}
				_gold = value;
			}
		}

		// The number of "beakers" (gold) spent on the currently researched
		// tech.
		public int beakers = 0;

		// The number of turns the player has been researching the current tech.
		public int turnsResearched = 0;

		// If the government is anarchy (or a govt with the transition bool set
		// to true), the number of turns left before switching is allowed.
		public int anarchyTurnsLeft = 0;

		// The current government of the player.
		public Government government;

		// The rules of this game.
		public Rules rules;

		public int EraIndex() {
			return EraIndex(eraCivilopediaName);
		}

		public static int EraIndex(string era) {
			if (era == "ERAS_Ancient_Times") {
				return 0;
			} else if (era == "ERAS_Middle_Ages") {
				return 1;
			} else if (era == "ERAS_Industrial_Age") {
				return 2;
			} else if (era == "ERAS_Modern_Era") {
				return 3;
			}
			return -1;
		}

		public static string EraIndexToEra(int index) {
			if (index <= 0) {
				return "ERAS_Ancient_Times";
			} else if (index == 1) {
				return "ERAS_Middle_Ages";
			} else if (index == 2) {
				return "ERAS_Industrial_Age";
			} else {
				return "ERAS_Modern_Era";
			}
		}

		public void AddUnit(MapUnit unit) {
			this.units.Add(unit);
		}

		public void SetCurrentlyResearchedTech(ID id) {
			currentlyResearchedTech = id;

			// Clear out previous progress.
			beakers = 0;
			turnsResearched = 0;
		}

		public string GetNextCityName() {
			string name = civilization.cityNames[cityNameIndex % civilization.cityNames.Count];
			int bonusLoops = cityNameIndex / civilization.cityNames.Count;
			if (bonusLoops % 2 == 1) {
				name = "New " + name;
			}
			int suffix = (bonusLoops / 2) + 1;
			if (suffix > 1) {
				name = name + " " + suffix; //e.g. for bonusLoops = 2, we'll have "Athens 2"
			}
			cityNameIndex++;
			return name;
		}

		public Player() {
			tileKnowledge = new TileKnowledge(this);
		}

		public bool IsAtPeaceWith(Player other) {
			// Evaluate this before checking for barbarians so barbarians don't
			// attack themselves.
			if (other == this) {
				return true;
			}

			if (other.isBarbarians || this.isBarbarians) {
				return false;
			}

			if (playerRelationships.ContainsKey(other.id)) {
				return !playerRelationships[other.id].atWar;
			}
			return true;
		}

		public void EnsureRelationshipExists(Player other) {
			if (!playerRelationships.ContainsKey(other.id)) {
				playerRelationships.Add(other.id, new PlayerRelationship());
			}
			if (!other.playerRelationships.ContainsKey(this.id)) {
				other.playerRelationships.Add(this.id, new PlayerRelationship());
			}
		}

		public void DeclareWarOn(Player other, int currentTurn) {
			EnsureRelationshipExists(other);

			playerRelationships[other.id].atWar = true;

			PlayerRelationship pr = other.playerRelationships[this.id];
			pr.atWar = true;
			pr.warDeclarationCount += 1;

			// Check to see if there was a sneak attack - we consider a sneak
			// attack any attack where the player's units were inside the
			// borders of the civ they're declaring war on.
			foreach (Tile t in other.tileKnowledge.knownTiles) {
				if (t.owningCity == null || t.owningCity.owner != other) {
					continue;
				}
				if (t.unitsOnTile.Count == 0) {
					continue;
				}
				if (t.unitsOnTile[0].owner == this) {
					pr.wasSneakAttacked = true;
					break;
				}
			}


			// Refuse contact from the aggressor civ until enough turns have
			// elapsed. The exact civ3 mechanism here is unknown, so we just
			// pick some reasonable random number. To penalize sneak attacks we
			// use a higher upper bound.
			pr.refuseContactUntilTurn = currentTurn + new Random().Next(5, pr.wasSneakAttacked ? 16 : 12);

			// Whenever war is declared, re-evaluate priorities.
			turnsUntilPriorityReevaluation = 0;
			other.turnsUntilPriorityReevaluation = 0;
		}

		public bool WillAcceptCommunicationFrom(Player other, int currentTurn) {
			EnsureRelationshipExists(other);

			PlayerRelationship pr = playerRelationships[other.id];
			if (!pr.atWar) {
				return true;
			}
			return currentTurn >= pr.refuseContactUntilTurn;
		}

		public bool SitsOutFirstTurn() {
			// TODO: Scenarios can also specify that certain players sit out the first turn. E.g. WW2 in the Pacific
			return isBarbarians;
		}

		// Once we have technologies, not all resources will be known at the start.
		// Eventually, perhaps there will be other gates around resource access as well
		// For now, just always return true, but have this method so we have that structure
		// in place.
		public bool KnowsAboutResource(Resource resource) {
			return true;
		}

		public int RemainingCities() {
			int result = 0;
			foreach (City city in cities) {
				// Destroyed cities have a size of zero.
				if (city.size > 0) {
					++result;
				}
			}
			return result;
		}

		public override string ToString() {
			if (civilization != null)
				return civilization.cityNames.First();
			return "";
		}

		public int CalculateGoldPerTurn() {
			int result = 0;
			foreach (City city in cities) {
				result += city.CurrentCommerceYield().taxes;
			}

			// Subtract unit support costs, if any.
			var (_, _, unitSupportCost) = TotalUnitsAllowedUnitsAndSupportCost();
			result -= unitSupportCost;

			return result;
		}

		public bool WouldAcceptDealFrom(GameData gameData, Player other, TradeOffer theirOffer, TradeOffer ourOffer) {
			// TODO: consider any factors like trade reputations here
			// TODO: figure out when peace is acceptable
			int theirGoldValue = theirOffer.GoldEquivalentFor(gameData, this);
			int ourGoldValue = ourOffer.GoldEquivalentFor(gameData, this);
			return theirGoldValue >= ourGoldValue;
		}

		public void ExecuteDeal(GameData gameData, Player other, TradeOffer theirOffer, TradeOffer ourOffer) {
			log.Information($"Executing trade between {this} and {other}");
			log.Information($"  {this} gives {ourOffer.ToString()}, worth {ourOffer.GoldEquivalentFor(gameData, other)} gold");
			log.Information($"  {other} gives {theirOffer.ToString()}, worth {theirOffer.GoldEquivalentFor(gameData, this)} gold)");
			if (theirOffer.partOfPeaceTreaty) {
				log.Information($"  {this} is now at peace with {other}");
				this.playerRelationships[other.id].atWar = false;
				other.playerRelationships[this.id].atWar = false;
			}

			if (ourOffer.gold.HasValue) {
				other.gold += ourOffer.gold.Value;
				this.gold -= ourOffer.gold.Value;
			}
			if (theirOffer.gold.HasValue) {
				this.gold += theirOffer.gold.Value;
				other.gold -= theirOffer.gold.Value;
			}

			foreach (Tech t in ourOffer.techs) {
				other.knownTechs.Add(t.id);
			}

			foreach (Tech t in theirOffer.techs) {
				this.knownTechs.Add(t.id);
			}
		}

		public int EstimateTurnsToResearch(GameData gameData, Tech tech) {
			int beakersPerTurn = 0;
			foreach (City city in cities) {
				beakersPerTurn += city.CurrentCommerceYield().beakers;
			}

			if (beakersPerTurn == 0) {
				// No research is happening.
				return int.MaxValue;
			}

			int remainingCost = gameData.TechCostFor(tech, this);
			int turnsRemaining = (int)Math.Ceiling((double)remainingCost / beakersPerTurn);

			int maxTurnsRemaining = rules.MaximumResearchTime - turnsResearched;
			int minTurnsRemaining = rules.MinimumResearchTime - turnsResearched;

			int result = Math.Min(turnsRemaining, maxTurnsRemaining);
			result = Math.Max(result, minTurnsRemaining);

			return result;
		}

		public void DoPerTurnFinanceUpdates(GameData gameData) {
			if (isBarbarians) {
				return;
			}

			// Process per-city contributions.
			//
			// TODO: consider making this return a tuple too. Or maybe return all
			// the gold accounting stuff in a struct, for one pass over the cities.
			foreach (City city in cities) {
				beakers += city.CurrentCommerceYield().beakers;
			}

			// Ensure we never go below 0 gold.
			while (gold + CalculateGoldPerTurn() < 0) {
				// Start by disbanding units to get things under control.
				var (_, _, unitSupportCost) = TotalUnitsAllowedUnitsAndSupportCost();
				if (unitSupportCost > 0) {
					for (int i = 0; i < unitSupportCost / government.unitCost; ++i) {
						MapUnit unitToDisband = units[GameData.rng.Next(units.Count)];
						log.Information($"{this} is out of gold, disbanding {unitToDisband} at {unitToDisband.location} to being unit support costs under control");
						gameData.DisbandUnit(unitToDisband);
					}
					continue;
				}

				// If we're under the unit support cap, try lowering our science
				// budget.
				if (scienceRate > 0) {
					--scienceRate;
					++taxRate;
					continue;
				}

				// If that wasn't sufficient, go after luxuries.
				if (scienceRate > 0) {
					--luxuryRate;
					++taxRate;
					continue;
				}

				// If the budget still isn't under control, something is wrong.
				throw new Exception($"{this} was unable to get the budget under control despite being under the unit support cap and zeroing out the sliders (gold={gold}, gpt={CalculateGoldPerTurn()})");
			}

			gold += CalculateGoldPerTurn();
		}

		public void DoPerTurnScienceUpdates(GameData gameData) {
			if (currentlyResearchedTech == null) {
				return;
			}

			// TODO: This isn't quite accurate. This should only be
			// incremented if the player is actually spending money on
			// research, or has a science specialist.
			turnsResearched++;

			// Check to see if the player has finished researching their
			// tech, and if they have, add it to the list of known techs
			Tech tech = gameData.techs.Find(x => x.id == currentlyResearchedTech);
			if (EstimateTurnsToResearch(gameData, tech) > 0) {
				return;
			}

			knownTechs.Add(currentlyResearchedTech);
			SetCurrentlyResearchedTech(null);

			if (CanAdvanceToNextEra(gameData)) {
				eraCivilopediaName = EraIndexToEra(EraIndex() + 1);
			}
		}

		private bool CanAdvanceToNextEra(GameData gameData) {
			foreach (Tech t in gameData.techs) {
				if (t.EraCivilopediaName != eraCivilopediaName) {
					continue;
				}

				if (knownTechs.Contains(t.id)) {
					continue;
				}

				// This is a tech in our era that we don't know. If it is
				// required for era advancement we can't go to the next era yet.
				if (t.RequiredForEraAdvancement) {
					return false;
				}
			}

			return true;
		}

		public (int, int, int) TotalUnitsAllowedUnitsAndSupportCost() {
			int freeUnits = 0;

			foreach (City city in cities) {
				// TODO: Import these sizes from Rule.cs in the biq. Maybe have
				// them live in the city class?
				if (city.size <= 6) {
					freeUnits += government.freeUnitsPerTown;
				} else if (city.size <= 12) {
					freeUnits += government.freeUnitsPerCity;
				} else {
					freeUnits += government.freeUnitsPerMetropolis;
				}
			}
			if (government.allUnitsFree) {
				freeUnits = units.Count;
			}

			int totalUnits = units.Count;
			int allowedUnits = freeUnits;
			int unitSupportCost = Math.Max(0, (totalUnits - allowedUnits) * government.unitCost);
			return (totalUnits, allowedUnits, unitSupportCost);
		}

		// See https://forums.civfanatics.com/threads/military-advisor-relative-strength-assessment-definition.62980/post-1211499 and
		// https://forums.civfanatics.com/threads/study-of-inner-workings-of-military-advisor.83599/
		public float CalculateMilitaryStrength() {
			float result = 4; // The +4 base is hypothesized to avoid div by 0 bugs.
			foreach (MapUnit unit in units) {
				UnitPrototype up = unit.unitType;
				result += unit.maxHitPoints * (up.attack * 3 + up.defense * 2) + up.bombard;
			}
			return result;
		}

		public enum MilitaryStrength {
			WeakTo,
			EquivalentTo,
			StrongTo,
		};

		// See https://forums.civfanatics.com/threads/study-of-inner-workings-of-military-advisor.83599/
		public MilitaryStrength CompareMilitaryStrengthTo(Player other) {
			float us = CalculateMilitaryStrength();
			float them = other.CalculateMilitaryStrength();

			if (us < 0.8 * them) {
				return MilitaryStrength.WeakTo;
			} else if (us > 1.2 * them) {
				return MilitaryStrength.StrongTo;
			} else {
				return MilitaryStrength.EquivalentTo;
			}
		}

		public void DoCorruptionCalculations(GameData gameData) {
			if (cities.Count == 0) {
				return;
			}

			// Order the cities by distance to the capitol, using OrderBy to get
			// a stable sort (https://stackoverflow.com/a/148123). We want a
			// stable sort, because if two cities are the same distance from the
			// capitol, the tiebreaker is city age (which we don't track yet) and
			// then order in the database, which a stable sort gives us.
			//
			// TODO: track city age.
			City capitol = cities.Find(x => x.IsCapital());
			if (capitol == null) {
				capitol = cities[0];
			}
			List<City> citiesInRankOrdering = cities.OrderBy(x => x.location.rankDistanceTo(capitol.location)).ToList();
			for (int i = 0; i < citiesInRankOrdering.Count; ++i) {
				citiesInRankOrdering[i].rankIndex = i;

				// For each city, calculate its corruption level so this
				// calculation doesn't have to be done on the fly.
				citiesInRankOrdering[i].CalculateCorruption(gameData);
			}
		}
	}

}
