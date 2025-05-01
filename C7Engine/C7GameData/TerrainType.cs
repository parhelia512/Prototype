namespace C7GameData {
	using QueryCiv3;
	using QueryCiv3.Biq;
	using System.Collections.Generic;
	using System.Linq;

	public class TerrainType {
		//The "key" is a language-independent name for the terrain.  Civ3 relies on their index in the list to know
		//what they are; we'll use the key.  This allows adding custom terrain types in the future, including having
		//different custom terrains in Mod A than Mod B, while still allowing internationalized versions of their
		//names that don't break the scenario.  E.g. "ocean"
		public string Key { get; set; } = "";
		//The name is the display name.  E.g. "Ocean" in English scenarios, "Hochsee" in German scenarios.
		public string DisplayName { get; set; } = "";
		public int baseFoodProduction { get; set; }
		public int baseShieldProduction { get; set; }
		public int baseCommerceProduction { get; set; }
		public int movementCost { get; set; }
		public bool allowCities { get; set; } = true;
		public int miningBonus { get; set; }
		public int irrigationBonus { get; set; }
		public int roadBonus { get; set; }
		public HashSet<string> allowedWorkerActions;
		public StrengthBonus defenseBonus;

		//some stuff about graphics would probably make sense, too

		public bool isHilly() {
			if (Key.Equals("mountains") || Key.Equals("hills") || Key.Equals("volcano")) {
				return true;
			}
			return false;
		}

		public bool isVolcano() {
			return Key.Equals("volcano");
		}

		//TODO: Once we have IDs, this should *not* rely on the display name.
		//That will be after issue 58, which will be after PR 70.
		public bool isWater() {
			return Key.Equals("coast") || Key.Equals("sea") || Key.Equals("ocean");
		}

		public override string ToString() {
			return DisplayName + "(" + baseFoodProduction + ", " + baseShieldProduction + ", " + baseCommerceProduction + ")";
		}

		public static TerrainType NONE = new TerrainType();


		public static TerrainType ImportFromCiv3(int civ3Index, TERR civ3Terrain) {
			TerrainType c7Terrain = new() {
				Key = civTerrainKeyLookup[civ3Index],
				DisplayName = civ3Terrain.Name,
				baseFoodProduction = civ3Terrain.Food,
				baseShieldProduction = civ3Terrain.Shields,
				baseCommerceProduction = civ3Terrain.Commerce,
				movementCost = civ3Terrain.MovementCost,
				allowCities = civ3Terrain.AllowCities != 0,
				defenseBonus = new StrengthBonus {
					description = civ3Terrain.Name,
					amount = civ3Terrain.DefenseBonus / 100.0
				},
				miningBonus = civ3Terrain.MiningBonus,
				roadBonus = civ3Terrain.RoadBonus,
				irrigationBonus = civ3Terrain.IrrigationBonus,
				allowedWorkerActions = LoadWorkerActions(civ3Terrain).ToHashSet(),
			};

			return c7Terrain;
		}

		private static IEnumerable<string> LoadWorkerActions(TERR civ3Terrain) {
			if (civ3Terrain.CanChopForest) yield return C7Action.UnitClearForest;
			if (civ3Terrain.CanClearWetlands) yield return C7Action.UnitClearWetlands;
			if (civ3Terrain.CanPlantForest) yield return C7Action.UnitPlantForest;
		}

		//This only works for Conquests due to the new terrains being added in the middle of the list.
		private static Dictionary<int, string> civTerrainKeyLookup = new Dictionary<int, string>() {
			{ 0,  "desert"},
			{ 1,  "plains"},
			{ 2,  "grassland"},
			{ 3,  "tundra"},
			{ 4,  "flood plain"},
			{ 5,  "hills"},
			{ 6,  "mountains"},
			{ 7,  "forest"},
			{ 8,  "jungle"},
			{ 9,  "marsh"},
			{ 10, "volcano"},
			{ 11, "coast"},
			{ 12, "sea"},
			{ 13, "ocean"}
		};
	}
}
