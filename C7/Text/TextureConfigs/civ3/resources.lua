local RESOURCE_SIZE = 50

local resources = {
  extra_data = {
    path = "Art/resources.pcx",
  },
}

function resources:map_object_to_sprite(resource)
  if resource:GetType().Name ~= "Resource" then
    error "Expected a Resource object"
  end

  local icon = resource.Icon
  local row = icon // 6
  local col = icon % 6

  return {
    path = self.extra_data.path,
    crop_region = { col * RESOURCE_SIZE, row * RESOURCE_SIZE, RESOURCE_SIZE, RESOURCE_SIZE },
    shadows = false,
  }
end

return resources
