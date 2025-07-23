local YieldType = ENUMS.Tile_YieldType

local mine
local irrigation

local function get_terrain_improvement(key)
  local improvements = GAME_DATA().terrainImprovements
  for i = 0, improvements.Count - 1 do
    local ti = myList:get_Item(i)

    if ti.key == key then
      return ti
    end
  end
end

local terrain_improvements = {
  railroad = {
    tile_modifier = function(yield)
      if not mine or not irrigation then
        mine = get_terrain_improvement "mine"
        irrigation = get_terrain_improvement "irrigation"
      end

      -- bonus production for a mined tile
      if yield.tile:HasImprovement(mine) and yield.type == YieldType.Production then
        yield.bonus = yield.bonus + 1
      end

      -- bonus food for an irrigated tile
      if yield.tile:HasImprovement(irrigation) and yield.type == YieldType.Food then
        yield.bonus = yield.bonus + 1
      end
    end,
  },
}

return terrain_improvements
