--[[
	This configuration file holds a table with texture definitions for "modern" graphics.
	It produces this table by copying the config of the civ3 assets and replaces the paths to the original Civilization 3 PCX textures with modern PNG versions.
	To add new replacement texture to the config modify the "c7_texture_list" table.
--]]
local civ3_textures = require "civ3"

local c7_texture_list = {
  "Art/SmallHeads/popHeads.png",
  "Art/Terrain/Mountains-snow.png",
  "Art/Terrain/Mountains.png",
  "Art/Terrain/TerrainBuildings.png",
  "Art/Terrain/Volcanos forests.png",
  "Art/Terrain/Volcanos jungles.png",
  "Art/Terrain/Volcanos.png",
  "Art/Terrain/grassland forests.png",
  "Art/Terrain/hill forests.png",
  "Art/Terrain/hill jungle.png",
  "Art/Terrain/irrigation DESETT.png",
  "Art/Terrain/irrigation PLAINS.png",
  "Art/Terrain/irrigation TUNDRA.png",
  "Art/Terrain/irrigation.png",
  "Art/Terrain/marsh.png",
  "Art/Terrain/mountain forests.png",
  "Art/Terrain/mountain jungles.png",
  "Art/Terrain/mtnRivers.png",
  "Art/Terrain/plains forests.png",
  "Art/Terrain/tnt.png",
  "Art/Terrain/tundra forests.png",
  "Art/Terrain/wCSO.png",
  "Art/Terrain/wOOO.png",
  "Art/Terrain/wSSS.png",
  "Art/Terrain/xdgc.png",
  "Art/Terrain/xdgp.png",
  "Art/Terrain/xdpc.png",
  "Art/Terrain/xggc.png",
  "Art/Terrain/xhills.png",
  "Art/Terrain/xpgc.png",
  "Art/Terrain/xtgc.png",
  "Art/city screen/CityIcons.png",
  "Art/city screen/ProdButton.png",
  "Art/city screen/background.png",
  "Art/city screen/cityMgmtButtons.png",
  "Art/resources.png",
}

-- Helper: Strip file extension from path
local function strip_extension(path) return path:match "^(.-)%.[^%.]+$" or path end

local output = {}

-- Build lookup table from c7_texture_list without extensions
local lookup = {}
for _, path in ipairs(c7_texture_list) do
  lookup[strip_extension(path)] = path
end

-- Recursive traversal
local function traverse(value)
  -- string leaf
  if type(value) == "string" then
    local stripped = strip_extension(value)
    return lookup[stripped] or value

  -- Table with "path" key
  elseif type(value) == "table" and type(value.path) == "string" then
    local stripped = strip_extension(value.path)
    local png_path = lookup[stripped]

    -- Copy rest of the table (in case it has extra keys)
    local result = {}
    for k, v in pairs(value) do
      result[k] = traverse(v)
    end

    result.path = png_path or value.path

    return result

  -- General nested table
  elseif type(value) == "table" then
    local result = {}
    for k, v in pairs(value) do
      result[k] = traverse(v)
    end
    return result
  end

  -- Return any other primitive as-is
  return value
end

return traverse(civ3_textures)
