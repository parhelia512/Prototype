-- Base paths
local ADVISORS = "Art/Advisors/"

local BUTTONS = "Art/buttonsFINAL.pcx"
local EXIT_BOX = "Art/exitBox-backgroundStates.pcx"
local INTERFACE = "Art/interface/"
local X_O = "Art/X-o_ALLstates-sprite.pcx"
local SCIENCE_NAV = "Art/Tech Chooser/scienceNAV.pcx"

local CITY_SCREEN = "Art/city screen/"
local CITY_BUTTONS = "Art/city screen/cityMgmtButtons.pcx"
local CITY_PRODUCTION = "Art/city screen/ProdButton.pcx"
local CITY_SCREEN_ICONS = "Art/city screen/CityIcons.pcx"

local CITY_ICONS = "Art/Cities/city icons.pcx"

local TERRAIN = "Art/Terrain/"
local RESOURCES = "Art/resources.pcx"

local CREDITS = "Art/Credits/"
local PALACE = "Art/PalaceView/"

local WORLD_SETUP = "Art/WorldSetup/"

-- Texture definitions
local textures = {}

textures.advisors = {
  dialog_box = ADVISORS .. "dialogbox.pcx",
  science = {
    background = {
      ancient = ADVISORS .. "science_ancient.pcx",
      middle = ADVISORS .. "science_middle.pcx",
      industrial = ADVISORS .. "science_industrial_new.pcx",
      modern = ADVISORS .. "science_modern.pcx",
    },
    navigation = {
      button = {
        normal = {
          path = SCIENCE_NAV,
          crop_region = { 0, 1, 129, 33 },
        },
        hover = {
          path = SCIENCE_NAV,
          crop_region = { 0, 35, 129, 33 },
        },
        pressed = {
          path = SCIENCE_NAV,
          crop_region = { 0, 69, 129, 33 },
        },
      },
      arrow_previous = {
        path = SCIENCE_NAV,
        crop_region = { 0, 103, 44, 9 },
      },
      arrow_next = {
        path = SCIENCE_NAV,
        crop_region = { 46, 103, 44, 9 },
      },
    },
  },
  military = {
    background = ADVISORS .. "military.pcx",
  },
  domestic = {
    background = ADVISORS .. "domestic.pcx",
    button = {
      normal = {
        path = ADVISORS .. "domesticBUTTON.pcx",
        crop_region = { 1, 1, 145, 24 },
      },
      hover = {
        path = ADVISORS .. "domesticBUTTON.pcx",
        crop_region = { 1, 26, 145, 24 },
      },
      pressed = {
        path = ADVISORS .. "domesticBUTTON.pcx",
        crop_region = { 1, 52, 145, 24 },
      },
    },
  },
}

textures.tech_box = {
  known = {
    path = ADVISORS .. "techboxes.pcx",
    crop_region = { 1, 1, 180, 80 },
  },
  in_progress = {
    path = ADVISORS .. "techboxes.pcx",
    crop_region = { 192, 1, 180, 80 },
  },
  possible = {
    path = ADVISORS .. "techboxes.pcx",
    crop_region = { 381, 1, 180, 80 },
  },
  blocked = {
    path = ADVISORS .. "techboxes.pcx",
    crop_region = { 568, 1, 180, 80 },
  },
  non_required = ADVISORS .. "non_required.pcx",
}

textures.ui = {
  button = {
    inactive = {
      path = BUTTONS,
      crop_region = { 1, 1, 20, 20 },
      shadows = false,
    },
    hover = {
      path = BUTTONS,
      crop_region = { 22, 1, 20, 20 },
      shadows = false,
    },
  },
  confirm = {
    normal = {
      path = X_O,
      crop_region = { 1, 1, 19, 19 },
    },
    hover = {
      path = X_O,
      crop_region = { 37, 1, 19, 19 },
    },
    pressed = {
      path = X_O,
      crop_region = { 73, 1, 19, 19 },
    },
  },
  cancel = {
    normal = {
      path = X_O,
      crop_region = { 21, 1, 15, 19 },
    },
    hover = {
      path = X_O,
      crop_region = { 57, 1, 15, 19 },
    },
    pressed = {
      path = X_O,
      crop_region = { 93, 1, 15, 19 },
    },
  },
  console = {
    normal = {
      path = INTERFACE .. "consoleButtons.pcx",
      crop_region = { 1, 1, 16, 16 },
    },
    hover = {
      path = INTERFACE .. "consoleButtons.pcx",
      crop_region = { 17, 1, 16, 16 },
    },
    pressed = {
      path = INTERFACE .. "consoleButtons.pcx",
      crop_region = { 33, 1, 16, 16 },
    },
  },
  exit = {
    normal = {
      path = EXIT_BOX,
      crop_region = { 0, 0, 72, 48 },
    },
    hover = {
      path = EXIT_BOX,
      crop_region = { 72, 0, 72, 48 },
    },
    pressed = {
      path = EXIT_BOX,
      crop_region = { 144, 0, 72, 48 },
    },
  },
  rename = {
    path = INTERFACE .. "NormButtons.PCX",
    alpha = INTERFACE .. "ButtonAlpha.pcx",
    crop_region = { 64, 224, 32, 32 },
  },
}

textures.terrain = {
  mountain = {
    base = TERRAIN .. "Mountains.pcx",
    jungle = TERRAIN .. "mountain jungles.pcx",
    snow = TERRAIN .. "Mountains-snow.pcx",
    forest = TERRAIN .. "mountain forests.pcx",
  },
  hill = {
    base = TERRAIN .. "xhills.pcx",
    forest = TERRAIN .. "hill forests.pcx",
    jungle = TERRAIN .. "hill jungle.pcx",
  },
  volcano = {
    base = TERRAIN .. "Volcanos.pcx",
    forest = TERRAIN .. "Volcanos forests.pcx",
    jungle = TERRAIN .. "Volcanos jungles.pcx",
  },
  irrigation = {
    grass = TERRAIN .. "irrigation.pcx",
    desert = TERRAIN .. "irrigation DESETT.pcx",
    plains = TERRAIN .. "irrigation PLAINS.pcx",
    tundra = TERRAIN .. "irrigation TUNDRA.pcx",
  },
  mine = {
    path = TERRAIN .. "TerrainBuildings.pcx",
    crop_region = { 256, 64, 128, 64 },
  },
  marsh = {
    large = {
      path = TERRAIN .. "marsh.pcx",
      crop_region = { 0, 0, 512, 176 },
    },
    small = {
      path = TERRAIN .. "marsh.pcx",
      crop_region = { 0, 176, 640, 176 },
    },
  },
  river = TERRAIN .. "mtnRivers.pcx",
  tnt = TERRAIN .. "tnt.pcx",
  railroad = TERRAIN .. "railroads.pcx",
  road = TERRAIN .. "roads.pcx",
  jungle = {
    large = {
      path = TERRAIN .. "grassland forests.pcx",
      crop_region = { 0, 0, 512, 176 },
    },
    small = {
      path = TERRAIN .. "grassland forests.pcx",
      crop_region = { 0, 176, 768, 176 },
    },
  },
  forest = {
    large = {
      path = TERRAIN .. "grassland forests.pcx",
      crop_region = { 0, 352, 512, 176 },
    },
    small = {
      path = TERRAIN .. "grassland forests.pcx",
      crop_region = { 0, 528, 640, 176 },
    },
    plains = {
      large = {
        path = TERRAIN .. "plains forests.pcx",
        crop_region = { 0, 352, 512, 176 },
      },
      small = {
        path = TERRAIN .. "plains forests.pcx",
        crop_region = { 0, 528, 640, 176 },
      },
    },
    tundra = {
      large = {
        path = TERRAIN .. "tundra forests.pcx",
        crop_region = { 0, 352, 512, 176 },
      },
      small = {
        path = TERRAIN .. "tundra forests.pcx",
        crop_region = { 0, 528, 640, 176 },
      },
    },
  },
  pine = {
    forest = {
      path = TERRAIN .. "grassland forests.pcx",
      crop_region = { 0, 704, 768, 176 },
    },
    plains = {
      path = TERRAIN .. "plains forests.pcx",
      crop_region = { 0, 704, 768, 176 },
    },
    tundra = {
      path = TERRAIN .. "tundra forests.pcx",
      crop_region = { 0, 704, 768, 176 },
    },
  },
}

textures.resources = RESOURCES

textures.credits = {
  background = CREDITS .. "credits_background.pcx",
}

textures.icons = {
  science = {
    path = CITY_SCREEN_ICONS,
    crop_region = { 34, 2, 30, 30 },
  },
  luxury = {
    path = CITY_SCREEN_ICONS,
    crop_region = { 376, 2, 30, 30 },
  },
  food = {
    path = CITY_SCREEN_ICONS,
    crop_region = { 195, 1, 21, 30 },
  },
  shield = {
    path = CITY_SCREEN_ICONS,
    crop_region = { 133, 1, 16, 30 },
  },
  commerce = {
    path = CITY_SCREEN_ICONS,
    crop_region = { 67, 1, 21, 30 },
  },
  plus = {
    path = ADVISORS .. "domestic_icons_aux.pcx",
    crop_region = { 75, 1, 22, 22 },
  },
  minus = {
    path = ADVISORS .. "domestic_icons_aux.pcx",
    crop_region = { 51, 1, 22, 22 },
  },
  eaten_food = {
    path = CITY_SCREEN_ICONS,
    crop_region = { 218, 2, 29, 29 },
  },
  full_food = {
    path = CITY_SCREEN_ICONS,
    crop_region = { 188, 2, 29, 29 },
  },
  wasted_shield = {
    path = CITY_SCREEN_ICONS,
    crop_region = { 157, 2, 29, 29 },
  },
  good_shield = {
    path = CITY_SCREEN_ICONS,
    crop_region = { 125, 2, 29, 29 },
  },
  wasted_gold = {
    path = CITY_SCREEN_ICONS,
    crop_region = { 95, 2, 29, 29 },
  },
  good_gold = {
    path = CITY_SCREEN_ICONS,
    crop_region = { 64, 2, 29, 29 },
  },
  happy_face = {
    path = CITY_SCREEN_ICONS,
    crop_region = { 373, 2, 29, 29 },
  },
  content_face = {
    path = CITY_SCREEN_ICONS,
    crop_region = { 591, 2, 29, 29 },
  },
  beaker = {
    path = CITY_SCREEN_ICONS,
    crop_region = { 34, 2, 29, 29 },
  },
  treasury = {
    path = CITY_SCREEN_ICONS,
    crop_region = { 746, 2, 29, 29 },
  },
  capital_star = {
    path = CITY_ICONS,
    crop_region = { 20, 1, 18, 18 },
  },
}

textures.city_screen = {
  background = "Art/city screen/background.pcx",
  buttons = {
    close = {
      normal = {
        path = CITY_BUTTONS,
        crop_region = { 155, 1, 38, 48 },
      },
      hover = {
        path = CITY_BUTTONS,
        crop_region = { 155, 50, 38, 48 },
      },
      pressed = {
        path = CITY_BUTTONS,
        crop_region = { 155, 99, 38, 48 },
      },
    },
    previous = {
      normal = {
        path = CITY_BUTTONS,
        crop_region = { 1, 1, 48, 48 },
      },
      hover = {
        path = CITY_BUTTONS,
        crop_region = { 1, 50, 48, 48 },
      },
      pressed = {
        path = CITY_BUTTONS,
        crop_region = { 1, 99, 48, 48 },
      },
    },
    next = {
      normal = {
        path = CITY_BUTTONS,
        crop_region = { 42, 1, 48, 48 },
      },
      hover = {
        path = CITY_BUTTONS,
        crop_region = { 42, 50, 48, 48 },
      },
      pressed = {
        path = CITY_BUTTONS,
        crop_region = { 42, 99, 48, 48 },
      },
    },
    production = {
      normal = {
        path = CITY_PRODUCTION,
        crop_region = { 1, 0, 114, 95 },
      },
      hover = {
        path = CITY_PRODUCTION,
        crop_region = { 116, 0, 115, 95 },
      },
      pressed = {
        path = CITY_PRODUCTION,
        crop_region = { 231, 0, 115, 95 },
      },
    },
  },
  production_queue = CITY_SCREEN .. "ProductionQueueBox.pcx",
}

textures.palace = {
  background = PALACE .. "bkgr.pcx",
}

textures.world_setup = {
  background = WORLD_SETUP .. "background.pcx",

  -- Pangaea buttons
  pangaea80 = {
    normal = {
      path = WORLD_SETUP .. "landmassWaterSMALL.pcx",
      crop_region = { 76 * 1 + 1, 1, 75, 50 },
    },
    hover = {
      path = WORLD_SETUP .. "landmassWaterSMALLrollovers.pcx",
      crop_region = { 76 * 1 + 1, 1, 75, 50 },
    },
    pressed = {
      path = WORLD_SETUP .. "landmassWaterSMALLdepress.pcx",
      crop_region = { 76 * 1 + 1, 1, 75, 50 },
    },
  },
  pangaea70 = {
    normal = {
      path = WORLD_SETUP .. "landmassWaterSMALL.pcx",
      crop_region = { 76 * 3 + 1, 1, 75, 50 },
    },
    hover = {
      path = WORLD_SETUP .. "landmassWaterSMALLrollovers.pcx",
      crop_region = { 76 * 3 + 1, 1, 75, 50 },
    },
    pressed = {
      path = WORLD_SETUP .. "landmassWaterSMALLdepress.pcx",
      crop_region = { 76 * 3 + 1, 1, 75, 50 },
    },
  },
  pangaea60 = {
    normal = {
      path = WORLD_SETUP .. "landmassWaterSMALL.pcx",
      crop_region = { 76 * 6 + 1, 1, 75, 50 },
    },
    hover = {
      path = WORLD_SETUP .. "landmassWaterSMALLrollovers.pcx",
      crop_region = { 76 * 6 + 1, 1, 75, 50 },
    },
    pressed = {
      path = WORLD_SETUP .. "landmassWaterSMALLdepress.pcx",
      crop_region = { 76 * 6 + 1, 1, 75, 50 },
    },
  },

  -- Continents buttons
  continents80 = {
    normal = {
      path = WORLD_SETUP .. "landmassWaterSMALL.pcx",
      crop_region = { 76 * 1 + 1, 1, 75, 50 },
    },
    hover = {
      path = WORLD_SETUP .. "landmassWaterSMALLrollovers.pcx",
      crop_region = { 76 * 1 + 1, 1, 75, 50 },
    },
    pressed = {
      path = WORLD_SETUP .. "landmassWaterSMALLdepress.pcx",
      crop_region = { 76 * 1 + 1, 1, 75, 50 },
    },
  },
  continents70 = {
    normal = {
      path = WORLD_SETUP .. "landmassWaterSMALL.pcx",
      crop_region = { 76 * 4 + 1, 1, 75, 50 },
    },
    hover = {
      path = WORLD_SETUP .. "landmassWaterSMALLrollovers.pcx",
      crop_region = { 76 * 4 + 1, 1, 75, 50 },
    },
    pressed = {
      path = WORLD_SETUP .. "landmassWaterSMALLdepress.pcx",
      crop_region = { 76 * 4 + 1, 1, 75, 50 },
    },
  },
  continents60 = {
    normal = {
      path = WORLD_SETUP .. "landmassWaterSMALL.pcx",
      crop_region = { 76 * 7 + 1, 1, 75, 50 },
    },
    hover = {
      path = WORLD_SETUP .. "landmassWaterSMALLrollovers.pcx",
      crop_region = { 76 * 7 + 1, 1, 75, 50 },
    },
    pressed = {
      path = WORLD_SETUP .. "landmassWaterSMALLdepress.pcx",
      crop_region = { 76 * 7 + 1, 1, 75, 50 },
    },
  },

  -- Archipelago buttons
  archipelago80 = {
    normal = {
      path = WORLD_SETUP .. "landmassWaterSMALL.pcx",
      crop_region = { 76 * 2 + 1, 1, 75, 50 },
    },
    hover = {
      path = WORLD_SETUP .. "landmassWaterSMALLrollovers.pcx",
      crop_region = { 76 * 2 + 1, 1, 75, 50 },
    },
    pressed = {
      path = WORLD_SETUP .. "landmassWaterSMALLdepress.pcx",
      crop_region = { 76 * 2 + 1, 1, 75, 50 },
    },
  },
  archipelago70 = {
    normal = {
      path = WORLD_SETUP .. "landmassWaterSMALL.pcx",
      crop_region = { 76 * 5 + 1, 1, 75, 50 },
    },
    hover = {
      path = WORLD_SETUP .. "landmassWaterSMALLrollovers.pcx",
      crop_region = { 76 * 5 + 1, 1, 75, 50 },
    },
    pressed = {
      path = WORLD_SETUP .. "landmassWaterSMALLdepress.pcx",
      crop_region = { 76 * 5 + 1, 1, 75, 50 },
    },
  },
  archipelago60 = {
    normal = {
      path = WORLD_SETUP .. "landmassWaterSMALL.pcx",
      crop_region = { 76 * 8 + 1, 1, 75, 50 },
    },
    hover = {
      path = WORLD_SETUP .. "landmassWaterSMALLrollovers.pcx",
      crop_region = { 76 * 8 + 1, 1, 75, 50 },
    },
    pressed = {
      path = WORLD_SETUP .. "landmassWaterSMALLdepress.pcx",
      crop_region = { 76 * 8 + 1, 1, 75, 50 },
    },
  },

  -- Climate buttons
  arid = {
    normal = {
      path = WORLD_SETUP .. "climate.pcx",
      crop_region = { 1, 339, 75, 50 },
    },
    hover = {
      path = WORLD_SETUP .. "CLIMTEMPAGERollovers.pcx",
      crop_region = { 1, 1, 75, 50 },
    },
    pressed = {
      path = WORLD_SETUP .. "CLIMTEMPAGEDepress.pcx",
      crop_region = { 1, 1, 75, 50 },
    },
  },
  normal = {
    normal = {
      path = WORLD_SETUP .. "climate.pcx",
      crop_region = { 77, 339, 75, 50 },
    },
    hover = {
      path = WORLD_SETUP .. "CLIMTEMPAGEDepress.pcx",
      crop_region = { 77, 1, 75, 50 },
    },
    pressed = {
      path = WORLD_SETUP .. "CLIMTEMPAGEDepress.pcx",
      crop_region = { 77, 1, 75, 50 },
    },
  },
  wet = {
    normal = {
      path = WORLD_SETUP .. "climate.pcx",
      crop_region = { 153, 339, 75, 50 },
    },
    hover = {
      path = WORLD_SETUP .. "CLIMTEMPAGEDepress.pcx",
      crop_region = { 153, 1, 75, 50 },
    },
    pressed = {
      path = WORLD_SETUP .. "CLIMTEMPAGEDepress.pcx",
      crop_region = { 153, 1, 75, 50 },
    },
  },

  -- Temperature buttons
  warm = {
    normal = {
      path = WORLD_SETUP .. "temperature.pcx",
      crop_region = { 1, 339, 75, 50 },
    },
    hover = {
      path = WORLD_SETUP .. "CLIMTEMPAGERollovers.pcx",
      crop_region = { 1, 124, 75, 50 },
    },
    pressed = {
      path = WORLD_SETUP .. "CLIMTEMPAGEDepress.pcx",
      crop_region = { 1, 124, 75, 50 },
    },
  },
  temperate = {
    normal = {
      path = WORLD_SETUP .. "temperature.pcx",
      crop_region = { 77, 339, 75, 50 },
    },
    hover = {
      path = WORLD_SETUP .. "CLIMTEMPAGEDepress.pcx",
      crop_region = { 77, 124, 75, 50 },
    },
    pressed = {
      path = WORLD_SETUP .. "CLIMTEMPAGEDepress.pcx",
      crop_region = { 77, 124, 75, 50 },
    },
  },
  cool = {
    normal = {
      path = WORLD_SETUP .. "temperature.pcx",
      crop_region = { 153, 339, 75, 50 },
    },
    hover = {
      path = WORLD_SETUP .. "CLIMTEMPAGEDepress.pcx",
      crop_region = { 153, 124, 75, 50 },
    },
    pressed = {
      path = WORLD_SETUP .. "CLIMTEMPAGEDepress.pcx",
      crop_region = { 153, 124, 75, 50 },
    },
  },

  -- Age buttons
  billion3 = {
    normal = {
      path = WORLD_SETUP .. "age.pcx",
      crop_region = { 1, 339, 75, 50 },
    },
    hover = {
      path = WORLD_SETUP .. "CLIMTEMPAGERollovers.pcx",
      crop_region = { 1, 281, 75, 50 },
    },
    pressed = {
      path = WORLD_SETUP .. "CLIMTEMPAGEDepress.pcx",
      crop_region = { 1, 281, 75, 50 },
    },
  },
  billion4 = {
    normal = {
      path = WORLD_SETUP .. "age.pcx",
      crop_region = { 77, 339, 75, 50 },
    },
    hover = {
      path = WORLD_SETUP .. "CLIMTEMPAGEDepress.pcx",
      crop_region = { 77, 281, 75, 50 },
    },
    pressed = {
      path = WORLD_SETUP .. "CLIMTEMPAGEDepress.pcx",
      crop_region = { 77, 281, 75, 50 },
    },
  },
  billion5 = {
    normal = {
      path = WORLD_SETUP .. "age.pcx",
      crop_region = { 153, 339, 75, 50 },
    },
    hover = {
      path = WORLD_SETUP .. "CLIMTEMPAGEDepress.pcx",
      crop_region = { 153, 281, 75, 50 },
    },
    pressed = {
      path = WORLD_SETUP .. "CLIMTEMPAGEDepress.pcx",
      crop_region = { 153, 281, 75, 50 },
    },
  },
}

textures.world_setup_large = {
  pangaea60 = {
    path = WORLD_SETUP .. "landmassWaterlarge.pcx",
    crop_region = { 1, 551, 300, 200 },
  },
  pangaea70 = {
    path = WORLD_SETUP .. "landmassWaterlarge.pcx",
    crop_region = { 1, 276, 300, 200 },
  },
  pangaea80 = {
    path = WORLD_SETUP .. "landmassWaterlarge.pcx",
    crop_region = { 1, 1, 300, 200 },
  },

  continents60 = {
    path = WORLD_SETUP .. "landmassWaterlarge.pcx",
    crop_region = { 301 + 1, 551, 300, 200 },
  },
  continents70 = {
    path = WORLD_SETUP .. "landmassWaterlarge.pcx",
    crop_region = { 301 + 1, 276, 300, 200 },
  },
  continents80 = {
    path = WORLD_SETUP .. "landmassWaterlarge.pcx",
    crop_region = { 301 + 1, 1, 300, 200 },
  },

  archipelago60 = {
    path = WORLD_SETUP .. "landmassWaterlarge.pcx",
    crop_region = { 301 * 2 + 1, 551, 300, 200 },
  },
  archipelago70 = {
    path = WORLD_SETUP .. "landmassWaterlarge.pcx",
    crop_region = { 301 * 2 + 1, 276, 300, 200 },
  },
  archipelago80 = {
    path = WORLD_SETUP .. "landmassWaterlarge.pcx",
    crop_region = { 301 * 2 + 1, 1, 300, 200 },
  },

  arid = {
    path = WORLD_SETUP .. "climate.pcx",
    crop_region = { 1, 1, 300, 200 },
  },
  normal = {
    path = WORLD_SETUP .. "climate.pcx",
    crop_region = { 302, 1, 300, 200 },
  },
  wet = {
    path = WORLD_SETUP .. "climate.pcx",
    crop_region = { 603, 1, 300, 200 },
  },

  cool = {
    path = WORLD_SETUP .. "temperature.pcx",
    crop_region = { 603, 1, 300, 200 },
  },
  temperate = {
    path = WORLD_SETUP .. "temperature.pcx",
    crop_region = { 302, 1, 300, 200 },
  },
  warm = {
    path = WORLD_SETUP .. "temperature.pcx",
    crop_region = { 1, 1, 300, 200 },
  },

  -- Age large images
  billion3 = {
    path = WORLD_SETUP .. "age.pcx",
    crop_region = { 1, 1, 300, 200 },
  },
  billion4 = {
    path = WORLD_SETUP .. "age.pcx",
    crop_region = { 302, 1, 300, 200 },
  },
  billion5 = {
    path = WORLD_SETUP .. "age.pcx",
    crop_region = { 603, 1, 300, 200 },
  },
}

textures.diplomacy = {
  deal = "Art/Diplomacy/counter.pcx",
  offer = "Art/Diplomacy/talk_offer.pcx",
}

textures.upper_left_navigation = {
  menu = {
    path = INTERFACE .. "menuButtons.pcx",
    alpha = INTERFACE .. "menuButtonsAlpha.pcx",
    crop_region = { 0, 1, 35, 29 },
  },
  civilopedia = {
    path = INTERFACE .. "menuButtons.pcx",
    alpha = INTERFACE .. "menuButtonsAlpha.pcx",
    crop_region = { 36, 1, 35, 29 },
  },
  advisor = {
    normal = {
      path = INTERFACE .. "menuButtons.pcx",
      alpha = INTERFACE .. "menuButtonsAlpha.pcx",
      crop_region = { 73, 1, 35, 29 },
    },
    hover = {
      path = INTERFACE .. "menuButtons.pcx",
      alpha = INTERFACE .. "menuButtonsAlpha.pcx",
      alpha_row_offset = 60,
      crop_region = { 73, 61, 35, 29 },
    },
    pressed = {
      path = INTERFACE .. "menuButtons.pcx",
      alpha = INTERFACE .. "menuButtonsAlpha.pcx",
      alpha_row_offset = 120,
      crop_region = { 73, 121, 35, 29 },
    },
  },
}

textures.lower_right_infobox = {
  box = {
    path = INTERFACE .. "box right color.pcx",
    alpha = INTERFACE .. "box right alpha.pcx",
  },
  next_turn = {
    off = {
      path = INTERFACE .. "nextturn states color.pcx",
      alpha = INTERFACE .. "nextturn states alpha.pcx",
      crop_region = { 0, 0, 47, 28 },
    },
    on = {
      path = INTERFACE .. "nextturn states color.pcx",
      alpha = INTERFACE .. "nextturn states alpha.pcx",
      crop_region = { 47, 0, 47, 28 },
    },
    blink = {
      path = INTERFACE .. "nextturn states color.pcx",
      alpha = INTERFACE .. "nextturn states alpha.pcx",
      crop_region = { 94, 0, 47, 28 },
    },
  },
}

return textures
