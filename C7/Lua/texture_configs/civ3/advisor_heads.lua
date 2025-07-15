local IMAGE_SIZE = 150
local CROP_HEIGHT = 110

local advisor_heads = {
  domestic = { path = "Art/SmallHeads/popupDOMESTIC.pcx", },
  trade = { path = "Art/SmallHeads/popupTRADE.pcx", },
  military = { path = "Art/SmallHeads/popupMILITARY.pcx", },
  foreign = { path = "Art/SmallHeads/popupFOREIGN.pcx", },
  culture = { path = "Art/SmallHeads/popupCULTURE.pcx", },
  science = { path = "Art/SmallHeads/popupSCIENCE.pcx", },
}

function advisor_heads:map_object_to_sprite(details)
  if (details:GetType().Name ~= "AdvisorGraphicsDetails") then
    error "Expected a AdvisorGraphicsDetails object"
  end

  -- Note: we need to use self.<advisor> for the c7.lua
  -- conversion to work.
  local path = nil
  if details.advisor == details.advisor.Domestic then
    path = self.domestic.path
  elseif details.advisor == details.advisor.Trade then
    path = self.trade.path
  elseif details.advisor == details.advisor.Military then
    path = self.military.path
  elseif details.advisor == details.advisor.Foreign then
    path = self.foreign.path
  elseif details.advisor == details.advisor.Culture then
    path = self.culture.path
  elseif details.advisor == details.advisor.Science then
    path = self.science.path
  end

  local mood_col = nil
  if details.mood == details.mood.Happy then
    mood_col = 0
  elseif detmood == details.mood.Angry then
    mood_col = 1
  elseif detmood == details.mood.Sad then
    mood_col = 2
  elseif detmood == details.mood.Surprised then
    mood_col = 3
  end

  return {
    path = path,
    crop_region = { 
        1 + mood_col * IMAGE_SIZE, 
        (details.eraIndex + 1) * IMAGE_SIZE - CROP_HEIGHT,
        IMAGE_SIZE - 1,
        CROP_HEIGHT,
    },
  }
end

return advisor_heads
