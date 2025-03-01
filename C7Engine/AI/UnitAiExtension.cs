using C7GameData;

namespace C7Engine {
	// This goofy class only exists because of the arbitrary separation between
	// C7GameData and C7Engine.
	public static class UnitAiExtension {
		public static UnitAI.Result TryToMoveAlongPath(this UnitAI unitAi, MapUnit unit, TilePath path, bool allowCombat) {
			Tile nextTile = path.Next();
			if (nextTile == Tile.NONE) {
				return UnitAI.Result.Error;
			}
			if (!unit.CanEnterTile(nextTile, /*allowCombat=*/allowCombat)) {
				return UnitAI.Result.Error;
			}
			bool stillAlive = unit.move(unit.location.directionTo(nextTile));
			if (!stillAlive) {
				return UnitAI.Result.Error;
			}
			return UnitAI.Result.InProgress;
		}
	}
}
