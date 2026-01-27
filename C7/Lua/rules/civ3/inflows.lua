local function rules()
  return GAME_DATA().rules
end

local inflows = {}

-- to match an item in a list based on a predicate
local function any(list, predicate)
    for _, v in ipairs(list) do
        if predicate(v) then
            return true
        end
    end
    return false
end
    
-- context is [ int input, List<Tech> techs ]
-- this is the actual implementation we would do for Wealth, for conquests
local function extra_commerce_calculation(context)
    local useful_shields = context.input
    local known_techs = context.techs
    local double_effect = any(known_techs,
    function(x) 
                return x.DoublesWealthProduction == true
            end
            )
    local ratio = rules().ShieldCostPerGold
    return math.max(1, useful_shields / (double_effect and (ratio / 2) or ratio))
end

-- example
local function extra_culture_calculation(context)
    local city_culture = context.input
    return math.max(1, city_culture/2)
end

-- example
local function extra_science_calculation(context)
    local beakers = context.input
    return math.max(0, beakers/10)
end
    
inflows.result = {
  wealth = {
      commerce = function(context)
          return extra_commerce_calculation(context)
      end,
      culture = function(context)
          return extra_culture_calculation(context)
      end,
      science = function(context)
          return extra_science_calculation(context)
      end,
  },
  -- example
  cultivation = {
      commerce = function(context)
          return extra_commerce_calculation(context) * 2
      end,
      culture = function(context)
          return extra_culture_calculation(context) * 2
      end,
      science = function(context)
          return extra_science_calculation(context) * 2
      end,
  },
  -- example
  expertise = {
      commerce = function(context)
          return extra_commerce_calculation(context) * 3
      end,
      culture = function(context)
          return extra_culture_calculation(context) * 3
      end,
      science = function(context)
          return extra_science_calculation(context) * 3
      end,
  },
}

return inflows