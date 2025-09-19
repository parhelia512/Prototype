local UnitAction = ENUMS.UnitAction
local YieldType = ENUMS.Tile_YieldType
local Civ3FoliageAction = ENUMS.TerrainType_Civ3FoliageAction

local terraforms = {
  mine = {
    validator = function(context)
      local mine = context.terraform.Improvement
      return mine:GetYieldBonus(context.tile.overlayTerrainType, YieldType.Production) > 0
    end,
  },

  irrigate = {
    validator = function(context)
      local irrigation = context.terraform.Improvement
      return context.tile:CanBeIrrigated(irrigation, context.player)
    end,
  },

  clear_wetlands = {
    validator = function(context)
      return context.tile.overlayTerrainType.allowedFoliageAction == Civ3FoliageAction.ClearWetlands
    end,
    effect = function(context)
      context.tile:ClearTerrainOverlay()
    end,
  },

  clear_forest = {
    validator = function(context)
      return context.tile.overlayTerrainType.allowedFoliageAction == Civ3FoliageAction.ClearForest
    end,
    effect = function(context)
      context.tile:MaybeAwardForestClearingShields()
      context.tile:ClearTerrainOverlay()
    end,
  },
}

return terraforms
