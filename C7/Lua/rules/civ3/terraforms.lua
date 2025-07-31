local UnitAction = ENUMS.UnitAction
local YieldType = ENUMS.Tile_YieldType

local terraforms = {
  mine_validator = function(context)
    local mine = context.terraform.Improvement
    return mine:GetYieldBonus(context.tile.overlayTerrainType, YieldType.Production) > 0
  end,

  irrigate_validator = function(context)
    local irrigation = context.terraform.Improvement
    return context.tile:CanBeIrrigated(irrigation, context.player)
  end,

  clear_foliage_validator = function(context)
    return context.tile.overlayTerrainType.allowedWorkerActions:Contains(context.terraform.Action)
  end,

  clear_wetlands_effect = function(context)
    context.tile:ClearTerrainOverlay()
  end,

  clear_forest_effect = function(context)
    context.tile:MaybeAwardForestClearingShields()
    context.tile:ClearTerrainOverlay()
  end,
}

return terraforms
