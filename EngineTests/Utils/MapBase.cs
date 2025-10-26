using C7GameData;

namespace EngineTests.Utils;

public class MapBase {
	protected static MapUnit MakeWaterUnit(int movementPoints) {
		MapUnit result = new(ID.None("")) {
			unitType = new UnitPrototype() {
				movement = movementPoints,
			},
			owner = new Player()
		};
		result.unitType.categories.Add("Sea");
		return result;
	}

	protected static MapUnit MakeLandUnit(int movementPoints) {
		MapUnit result = new(ID.None("")) {
			unitType = new UnitPrototype() {
				movement = movementPoints,
			},
			owner = new Player(),
		};
		result.unitType.categories.Add("Land");
		return result;
	}

	protected TerrainImprovement road = new("road", TerrainImprovement.Layer.Roads, movementCost: 1.0f / 3);


	protected Tile mountain  = new(ID.None("")) {
		baseTerrainType = new() {
			Key = "mountains"
		},
		overlayTerrainType = new() {
			Key = "mountains",
			movementCost = 3
		}
	};
	protected Tile hill  = new(ID.None("")) {
		baseTerrainType = new() {
			Key = "hills"
		},
		overlayTerrainType = new() {
			Key = "hills",
			movementCost = 2
		}
	};
	protected Tile plains  = new(ID.None("")) {
		baseTerrainType = new() {
			Key = "plains"
		},
		overlayTerrainType = new() {
			Key = "plains",
			movementCost = 1
		}
	};
	protected Tile coast  = new(ID.None("")) {
		baseTerrainType = new() {
			Key = "coast"
		},
		overlayTerrainType = new() {
			Key = "coast",
			movementCost = 1
		}
	};
	protected Tile sea  = new(ID.None("")) {
		baseTerrainType = new() {
			Key = "sea"
		},
		overlayTerrainType = new() {
			Key = "sea",
			movementCost = 1
		}
	};
}
