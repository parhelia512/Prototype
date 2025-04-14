using System;
using System.Collections.Generic;
using System.Linq;
using Serilog;
using C7Engine;
using C7Engine.Pathing;

namespace C7GameData {
	public class CityBuilding {
		public Building building;
		public Player builtByPlayer;
		public int year;
		public int totalCulture; // This represents the total culture produced by the building. 
								 // In Civ3, this value is displayed in the cultural advisor tab
	}

	public struct CommerceBreakdown {
		public int corrupted;
		public int taxes;
		public int beakers;
		public int happiness;
	}

	public struct CorruptableValue {
		public CorruptableValue(int value, float corruption) {
			// Apply the corruption amount, ensuring that there is always at
			// least one shield/commerce if we started with at least one.
			useful = (int)Math.Round(value * (1 - corruption));
			if (value > 0) {
				useful = Math.Max(1, useful);
			}

			// Whatever is left over is corrupt.
			corrupt = value - useful;
		}

		public int useful;
		public int corrupt;
	}

	public class City {
		public ID id { get; set; }
		public Tile location { get; internal set; }
		public string name;
		public int size = 1;
		public Dictionary<Player, int> perPlayerCulture = new();

		//Temporary production code because production is fun.
		public IProducible itemBeingProduced;
		public int shieldsStored = 0;

		public int foodStored = 0;
		public int foodNeededToGrow = 20;

		public bool capital = false;
		public Player owner { get; set; }
		public List<CityResident> residents = new List<CityResident>();
		public List<CityBuilding> buildings = [];

		// The order of this city within all the cities of a player for the
		// purposes of rank corruption calculations.
		//
		// This is updated each turn to avoid each city needing to do an O(n)
		// scan of the list, which would cause an O(n^2) overall calcuation.
		public int rankIndex = -1;

		// The amount of corruption, between 0 and 1.
		public float corruption = 0;

		public static City NONE = new City(Tile.NONE, null, "Dummy City", ID.None("city"));

		public City(Tile location, Player owner, string name, ID id) {
			this.id = id;
			this.location = location;
			this.owner = owner;
			this.name = name;
			if (owner != null) {
				this.perPlayerCulture.Add(owner, 0);
			}
		}

		internal City() { }

		public void SetItemBeingProduced(IProducible producible) {
			this.itemBeingProduced = producible;
		}

		public bool IsCapital() {
			return capital;
		}

		public bool CanBuildUnit(UnitPrototype proto) {
			return MeetsProductionRequirements(proto) && !IsUnitObsolete(proto);
		}

		// TODO: Consider golden ages when determining whether a unit is obsolete.
		// If a golden age has not yet been triggered and a unit can trigger one,  
		// it shouldn't be marked as obsolete, even if its upgrade is available.
		private bool IsUnitObsolete(UnitPrototype proto) {
			List<UnitPrototype> upgradeChain = owner.civilization.GetUpgradeChain(proto);

			return upgradeChain.Any(MeetsProductionRequirements);
		}

		private bool HasRequiredTechnology(IProducible producible) {
			return producible.requiredTech == null ||
				   owner.knownTechs.Contains(producible.requiredTech.id);
		}

		private bool MeetsProductionRequirements(UnitPrototype proto) {
			if (!owner.civilization.IsUnitAvailable(proto)) {
				return false;
			}

			if (!HasRequiredTechnology(proto)) {
				return false;
			}

			if (proto.categories.Contains("Sea") && !location.NeighborsWater()) {
				return false;
			}

			return true;
		}

		public int TurnsUntilGrowth() {
			if (FoodGrowthPerTurn() == 0) {
				return int.MaxValue;
			}
			int additionalFoodNeeded = foodNeededToGrow - foodStored;
			int turnsRoundedDown = additionalFoodNeeded / FoodGrowthPerTurn();
			if (additionalFoodNeeded % FoodGrowthPerTurn() != 0) {
				return turnsRoundedDown + 1;
			}
			return turnsRoundedDown;
		}

		public int TurnsToProduce(IProducible item) {
			int additionalProductionNeeded = item.shieldCost - shieldsStored;
			int usefulShields = CurrentProductionYield().useful;
			if (usefulShields == 0) {
				return int.MaxValue;
			}

			int turnsRoundedDown = additionalProductionNeeded / usefulShields;
			if (additionalProductionNeeded % usefulShields != 0) {
				return Math.Max(turnsRoundedDown + 1, 1);
			}
			return Math.Max(turnsRoundedDown, 1);
		}

		public int TurnsUntilProductionFinished() {
			return TurnsToProduce(itemBeingProduced);
		}

		public void ComputeCityGrowth() {
			foodStored += FoodGrowthPerTurn();
			if (foodStored >= foodNeededToGrow) {
				size++;
				foodStored = 0;
			} else if (foodStored < 0) {
				size--;
				foodStored = 0;
			}
		}

		/**
		 * Computes turn production.  If the production queue finishes,
		 * returns the item that is built.  Otherwise, returns null.
		 */
		public IProducible ComputeTurnProduction() {
			shieldsStored += CurrentProductionYield().useful;
			if (shieldsStored >= itemBeingProduced.shieldCost && size > itemBeingProduced.populationCost) {
				shieldsStored = 0;
				size -= itemBeingProduced.populationCost;
				return itemBeingProduced;
			}

			shieldsStored = Math.Min(shieldsStored, itemBeingProduced.shieldCost);
			return null;
		}

		public int CurrentFoodYield() {
			int yield = location.foodYield(owner).yield;
			foreach (CityResident r in residents) {
				yield += r.tileWorked.foodYield(owner).yield;
			}
			return yield;
		}

		public CorruptableValue CurrentProductionYield() {
			int yield = location.productionYield(owner).yield;
			foreach (CityResident r in residents) {
				yield += r.tileWorked.productionYield(owner).yield;
			}
			CorruptableValue result = new(yield, corruption);

			// Using our value of corruption, figure out how much useful
			// production we have to work with. Special case anarchy, where no
			// useful production is available. We do this here rather than 
			// setting corruption to 100% because CorruptableValue would give us
			// one useful commerce in that situation.
			if (owner.government.transitionType) {
				result.useful = 0;
				result.corrupt = yield;
			}

			// TODO: add specialist shields here.

			return result;
		}

		public CommerceBreakdown CurrentCommerceYield() {
			int uncorruptedCommerce = location.commerceYield(owner).yield;
			foreach (CityResident r in residents) {
				uncorruptedCommerce += r.tileWorked.commerceYield(owner).yield;
			}

			// Using our value of corruption, figure out how much useful
			// commerce we have to work with. Special case anarchy, where no
			// useful commerce is available. We do this here rather than setting
			// corruption to 100% because CorruptableValue would give us one
			// useful commerce in that situation.
			CorruptableValue commerce = new CorruptableValue(uncorruptedCommerce, corruption);
			if (owner.government.transitionType) {
				commerce.useful = 0;
				commerce.corrupt = uncorruptedCommerce;
			}

			// TODO: Add specialist income, which is unaffected by corruption.
			CommerceBreakdown result = new();
			result.corrupted = commerce.corrupt;
			result.beakers = (int)Math.Floor(commerce.useful * owner.scienceRate / 10.0);
			result.happiness = (int)Math.Floor(commerce.useful * owner.luxuryRate / 10.0);
			result.taxes = commerce.useful - result.beakers - result.happiness;

			return result;
		}

		public int FoodGrowthPerTurn() {
			return CurrentFoodYield() - size * 2;
		}

		private void RemoveCitizen() {
			residents[residents.Count - 1].tileWorked.personWorkingTile = null;
			residents.RemoveAt(residents.Count - 1);
		}

		public void RemoveCitizens(int number) {
			for (int i = 0; i < number; i++) {
				if (residents.Count > 0) {
					RemoveCitizen();
				} else {
					Log.Warning("Trying to remove last citizen from " + name);
					break;
				}
			}
		}

		public void RemoveAllCitizens() {
			while (residents.Count > 0) {
				RemoveCitizen();
			}
		}

		public override string ToString() {
			return $"{name} ({size})";
		}

		public int GetCulture() {
			return perPlayerCulture[owner];
		}

		public int GetCulturePerTurn() {
			int result = 0;
			foreach (CityBuilding cb in buildings) {
				result += cb.building.culturePerTurn;
			}
			return result;
		}

		public int GetBorderExpansionLevel() {
			// Give ourselves a minimum of 1 culture to avoid taking the log of 0
			int culture = Math.Max(1, GetCulture());

			// Take the log10 of culture, rounding down (so a culture of 123
			// would be 2, a culture of 5 would be 0, etc) and then add one to
			// get the expansion level. With 0-9 culture our culture goal is 10^1
			// and we have one tile of borders, with 10-99 our culture goal is
			// 10^2 and we have two tiles of borders.
			return (int)Math.Floor(Math.Log10(culture)) + 1;
		}

		public void AddBuilding(Building building) {
			buildings.Add(new CityBuilding {
				building = building,
				builtByPlayer = owner,
				year = 1, // TODO: Implement in-game year tracking
				totalCulture = 0
			});
		}

		// The list of tiles that could be worked by this city.
		// This isn't necessarily a subset of our borders, because we're allowed
		// to work tiles owned by our civ in our big fat cross, even if our
		// borders haven't expanded yet.
		//
		// TODO: we should make this configurable to allow for the bigger cross
		// if a mod wants it.
		public HashSet<Tile> GetWorkableTiles() {
			HashSet<Tile> result = new();
			foreach (Tile t in GetTilesOfRank(2)) {
				// Skip tiles not owned by this player.
				if (t.owningCity == null || t.owningCity.owner != this.owner) {
					continue;
				}

				// Skip tiles with cities on them.
				if (t.HasCity) {
					continue;
				}

				result.Add(t);
			}
			return result;
		}

		// The list of tiles that are within the borders of this city, without
		// taking into account border collisions with other cities.
		public HashSet<Tile> GetTilesWithinBorders() {
			return GetTilesOfRank(GetBorderExpansionLevel());
		}

		private HashSet<Tile> GetTilesOfRank(int rank) {
			HashSet<Tile> result = new();
			HashSet<Tile> knownTiles = new();

			Stack<Tile> toCheck = new();
			toCheck.Push(location);
			knownTiles.Add(location);

			while (toCheck.Count > 0) {
				Tile t = toCheck.Pop();

				// Skip tiles that are too far away.
				if (location.rankDistanceTo(t) > rank) {
					continue;
				}

				// Ocean tiles may only hold claims of rank 2.
				if (t.baseTerrainTypeKey == "ocean" && rank > 2) {
					continue;
				}

				// Otherwise this tile is close enough. Check its neighbors next.
				result.Add(t);
				foreach (Tile neighbor in t.neighbors.Values) {
					if (!knownTiles.Contains(neighbor)) {
						toCheck.Push(neighbor);
						knownTiles.Add(t);
					}
				}
			}

			return result;
		}

		// See https://forums.civfanatics.com/threads/everything-about-corruption-c3c-edition.76619/
		private float CalculateDistanceCorruption(int numAntiCorruptionBuildings) {
			float maxD = (location.map.numTilesWide + location.map.numTilesTall) / 4;

			// TODO: This should also look for the forbidden palace and secret
			// police headquarters.
			City capital = owner.cities.Find(x => x.IsCapital());
			if (capital == null) {
				// TODO: Ensure we always have a capital, even in scenarios
				// without palaces added.
				capital = owner.cities[0];
			}
			float distanceToPalace = location.rankDistanceTo(capital.location);
			if (owner.government.corruptionType == Government.CorruptionType.Communal) {
				distanceToPalace = maxD / 4;
			}

			// TODO: Update this once we track trade networks.
			bool connectedTocapital = false;
			float tradeFactor = connectedTocapital ? 1.0f : 5.0f/4.0f;

			float govtFactor = owner.government.corruptionType switch {
				Government.CorruptionType.Minimal => 3.0f/4.0f,
				Government.CorruptionType.Nuisance => 1f,
				Government.CorruptionType.Problematic => 1f,
				Government.CorruptionType.Rampant => 3.0f/2.0f,
				Government.CorruptionType.Catastrophic => 1f, // anarchy, special cased
				Government.CorruptionType.Communal => 1f,
				Government.CorruptionType.Off => 0f
			};


			float adjustedDistance =
					(float)Math.Pow(0.5f, numAntiCorruptionBuildings)
					* Math.Min(govtFactor * tradeFactor * distanceToPalace, maxD);
			return adjustedDistance / maxD;
		}

		// See https://forums.civfanatics.com/threads/everything-about-corruption-c3c-edition.76619/
		private float CalculateRankCorruption(GameData gameData, int numAntiCorruptionBuildings) {
			int rank = rankIndex;
			if (owner.government.corruptionType == Government.CorruptionType.Communal) {
				rank = owner.cities.Count / 2;
			}

			float nOpt = Math.Max(
				1,
				owner.GetAdjustedOptimalCityNumber(gameData) + .25f * numAntiCorruptionBuildings);

			if (rank < nOpt) {
				return rank / (2 * nOpt);
			} else {
				return (2 * rank - nOpt) / (2 * nOpt);
			}
		}

		public void CalculateCorruption(GameData gameData) {
			// TODO: Once we import ReducesCorruption from Bldg.cs, count the
			// number of buildings with this set.
			int numAntiCorruptionBuildings = 0;

			// TODO: Handle the forbidden palace and SPHQ.
			int numCorruptionReducingSmallWondersInCity = 0;

			corruption = CalculateDistanceCorruption(numAntiCorruptionBuildings)
					+ CalculateRankCorruption(gameData, numAntiCorruptionBuildings);
			// TODO: apply policeman modifiers, before applying the max

			// Corruption maxes out at 90%, and this max can be reduced further
			// via courthouses/police stations, and the forbidden palace/SPHQ.
			float maxCorruption = Math.Max(
				0,
				.9f - (.1f * numAntiCorruptionBuildings + .7f * numCorruptionReducingSmallWondersInCity));
			corruption = Math.Max(corruption, 0);
			corruption = Math.Min(corruption, maxCorruption);
		}

		// Does the per turn culture updating for the city and returns whether
		// the borders need to be updated.
		public bool UpdateCultureAndCheckForExpansion() {
			int start = GetBorderExpansionLevel();
			foreach (CityBuilding cb in buildings) {
				cb.totalCulture += cb.building.culturePerTurn;
				perPlayerCulture[owner] += cb.building.culturePerTurn;
			}
			return start != GetBorderExpansionLevel();
		}

		public IEnumerable<IProducible> ListProductionOptions() {
			return EngineStorage.gameData.unitPrototypes.Where(CanBuildUnit);
		}

		private Dictionary<Resource, int> ListResourceAccess(ResourceCategory category) {
			PathingAlgorithm pathing = PathingAlgorithmChooser.GetTradeNetworkAlgorithm();

			return owner.resourcesInBorders
				.Where(kv => kv.Key.Category == category && owner.KnowsAboutResource(kv.Key))
				.Select(kv => (kv.Key, kv.Value.Where(t => HasTradeAccess(t, pathing)).Count()))
				.Where(rc => rc.Item2 > 0)
				.ToDictionary();
		}

		private bool HasTradeAccess(Tile tile, PathingAlgorithm pathing) {
			return location == tile || pathing.PathFrom(location, tile).path.Count > 0;
		}

		public Dictionary<Resource, int> GetStrategicResources() {
			return ListResourceAccess(ResourceCategory.STRATEGIC);
		}

		public Dictionary<Resource, int> GetLuxuries() {
			return ListResourceAccess(ResourceCategory.LUXURY);
		}
	}
}
