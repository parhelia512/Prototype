local CITY_WIDTH = 166
local CITY_HEIGHT = 95

local cities = {
  extra_data = {
    path = "Art/Cities/rMIDEAST.pcx",
  },
}

local function validate_input(pair)
  local size = pair.size
  local era = pair.era

  if type(size) ~= "number" then
    error "Expected a number for the size"
  end
  if type(era) ~= "number" then
    error "Expected a number for the era"
  end

  return size, era
end

function cities:map_object_to_sprite(pair)
  local size, era = validate_input(pair)

  return {
    path = self.extra_data.path,
    crop_region = { size * CITY_WIDTH, era * CITY_HEIGHT, CITY_WIDTH, CITY_HEIGHT },
    shadows = false,
  }
end
  
return cities