local UnitAction = ENUMS.UnitAction

local terraforms = {
  build_mine = {
    validator = function(_, tile)
      return tile:CanBeMined()
    end,
  },

  irrigate = {
    validator = function(player, tile)
      return tile:CanBeIrrigated(player)
    end,
  },

  clear_wetlands = {
    validator = function(_, tile)
      return tile.overlayTerrainType.allowedWorkerActions:Contains(UnitAction.ClearWetlands)
    end,
    effect = function(_, tile)
      tile:ClearTerrainOverlay()
    end,
  },

  clear_forest = {
    validator = function(_, tile)
      return tile.overlayTerrainType.allowedWorkerActions:Contains(UnitAction.ClearForest)
    end,
    effect = function(_, tile)
      tile:MaybeAwardForestClearingShields()
      tile:ClearTerrainOverlay()
    end,
  },
}

return terraforms
