using Godot;
using System;
using System.Threading;
using C7GameData;
using C7Engine;
using C7GameData.Save;
using Serilog;

[Tool]
public partial class WorldSetup : Control {
	private static ILogger log = LogManager.ForContext<MainMenu>();

	[Export] TextureRect background;

	[Export] Label pangaeaLabel;
	[Export] Label continentsLabel;
	[Export] Label archipelagoLabel;

	[Export] TextureButton pangaea60;
	[Export] TextureButton pangaea70;
	[Export] TextureButton pangaea80;

	[Export] TextureButton continents60;
	[Export] TextureButton continents70;
	[Export] TextureButton continents80;

	[Export] TextureButton archipelago60;
	[Export] TextureButton archipelago70;
	[Export] TextureButton archipelago80;

	[Export] TextureRect pangaea60Large;
	[Export] TextureRect pangaea70Large;
	[Export] TextureRect pangaea80Large;
	[Export] TextureRect continents60Large;
	[Export] TextureRect continents70Large;
	[Export] TextureRect continents80Large;
	[Export] TextureRect archipelago60Large;
	[Export] TextureRect archipelago70Large;
	[Export] TextureRect archipelago80Large;

	[Export] TextureButton arid;
	[Export] TextureButton normal;
	[Export] TextureButton wet;
	[Export] TextureButton cool;
	[Export] TextureButton temperate;
	[Export] TextureButton warm;
	[Export] TextureButton billion3;
	[Export] TextureButton billion4;
	[Export] TextureButton billion5;

	[Export] TextureRect aridLarge;
	[Export] TextureRect normalLarge;
	[Export] TextureRect wetLarge;
	[Export] TextureRect coolLarge;
	[Export] TextureRect temperateLarge;
	[Export] TextureRect warmLarge;
	[Export] TextureRect billion3Large;
	[Export] TextureRect billion4Large;
	[Export] TextureRect billion5Large;

	[Export] CheckButton tinySize;
	[Export] CheckButton smallSize;
	[Export] CheckButton standardSize;
	[Export] CheckButton largeSize;
	[Export] CheckButton hugeSize;
	[Export] CheckButton randomSize;

	[Export] TextureButton confirm;
	[Export] TextureButton cancel;

	[Export] LineEdit seedInput;

	[Export] Label loadingLabel;

	WorldCharacteristics.Landform landform = WorldCharacteristics.Landform.Pangaea;
	WorldCharacteristics.OceanCoverage ocean = WorldCharacteristics.OceanCoverage.Percent_70;
	WorldCharacteristics.Age age = WorldCharacteristics.Age.Billion_4;
	WorldCharacteristics.Temperature temp = WorldCharacteristics.Temperature.Temperate;
	WorldCharacteristics.Climate clim = WorldCharacteristics.Climate.Normal;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		background.Texture = Util.LoadTextureFromPCX("Art/WorldSetup/background.pcx");

		pangaea80.TextureNormal = Util.LoadTextureFromPCX("Art/WorldSetup/landmassWaterSMALL.pcx", 76 * 1 + 1, 1, 75, 50);
		pangaea80.TextureHover = Util.LoadTextureFromPCX("Art/WorldSetup/landmassWaterSMALLrollovers.pcx", 76 * 1 + 1, 1, 75, 50);
		pangaea80.TexturePressed = Util.LoadTextureFromPCX("Art/WorldSetup/landmassWaterSMALLdepress.pcx", 76 * 1 + 1, 1, 75, 50);
		pangaea80.Pressed += () => {
			landform = WorldCharacteristics.Landform.Pangaea;
			ocean = WorldCharacteristics.OceanCoverage.Percent_80;
			ResetLandformGraphics();
			pangaea80Large.Visible = true;
			pangaeaLabel.Text = "Pangaea (80% water)";
		};

		pangaea70.TextureNormal = Util.LoadTextureFromPCX("Art/WorldSetup/landmassWaterSMALL.pcx", 76 * 3 + 1, 1, 75, 50);
		pangaea70.TextureHover = Util.LoadTextureFromPCX("Art/WorldSetup/landmassWaterSMALLrollovers.pcx", 76 * 3 + 1, 1, 75, 50);
		pangaea70.TexturePressed = Util.LoadTextureFromPCX("Art/WorldSetup/landmassWaterSMALLdepress.pcx", 76 * 3 + 1, 1, 75, 50);
		pangaea70.Pressed += () => {
			landform = WorldCharacteristics.Landform.Pangaea;
			ocean = WorldCharacteristics.OceanCoverage.Percent_70;
			ResetLandformGraphics();
			pangaea70Large.Visible = true;
			pangaeaLabel.Text = "Pangaea (70% water)";
		};

		pangaea60.TextureNormal = Util.LoadTextureFromPCX("Art/WorldSetup/landmassWaterSMALL.pcx", 76 * 6 + 1, 1, 75, 50);
		pangaea60.TextureHover = Util.LoadTextureFromPCX("Art/WorldSetup/landmassWaterSMALLrollovers.pcx", 76 * 6 + 1, 1, 75, 50);
		pangaea60.TexturePressed = Util.LoadTextureFromPCX("Art/WorldSetup/landmassWaterSMALLdepress.pcx", 76 * 6 + 1, 1, 75, 50);
		pangaea60.Pressed += () => {
			landform = WorldCharacteristics.Landform.Pangaea;
			ocean = WorldCharacteristics.OceanCoverage.Percent_60;
			ResetLandformGraphics();
			pangaea60Large.Visible = true;
			pangaeaLabel.Text = "Pangaea (60% water)";
		};

		continents80.TextureNormal = Util.LoadTextureFromPCX("Art/WorldSetup/landmassWaterSMALL.pcx", 76 * 1 + 1, 1, 75, 50);
		continents80.TextureHover = Util.LoadTextureFromPCX("Art/WorldSetup/landmassWaterSMALLrollovers.pcx", 76 * 1 + 1, 1, 75, 50);
		continents80.TexturePressed = Util.LoadTextureFromPCX("Art/WorldSetup/landmassWaterSMALLdepress.pcx", 76 * 1 + 1, 1, 75, 50);
		continents80.Pressed += () => {
			landform = WorldCharacteristics.Landform.Continents;
			ocean = WorldCharacteristics.OceanCoverage.Percent_80;
			ResetLandformGraphics();
			continents80Large.Visible = true;
			continentsLabel.Text = "Continents (80% water)";
		};

		continents70.TextureNormal = Util.LoadTextureFromPCX("Art/WorldSetup/landmassWaterSMALL.pcx", 76 * 4 + 1, 1, 75, 50);
		continents70.TextureHover = Util.LoadTextureFromPCX("Art/WorldSetup/landmassWaterSMALLrollovers.pcx", 76 * 4 + 1, 1, 75, 50);
		continents70.TexturePressed = Util.LoadTextureFromPCX("Art/WorldSetup/landmassWaterSMALLdepress.pcx", 76 * 4 + 1, 1, 75, 50);
		continents70.Pressed += () => {
			landform = WorldCharacteristics.Landform.Continents;
			ocean = WorldCharacteristics.OceanCoverage.Percent_70;
			ResetLandformGraphics();
			continents70Large.Visible = true;
			continentsLabel.Text = "Continents (70% water)";
		};

		continents60.TextureNormal = Util.LoadTextureFromPCX("Art/WorldSetup/landmassWaterSMALL.pcx", 76 * 7 + 1, 1, 75, 50);
		continents60.TextureHover = Util.LoadTextureFromPCX("Art/WorldSetup/landmassWaterSMALLrollovers.pcx", 76 * 7 + 1, 1, 75, 50);
		continents60.TexturePressed = Util.LoadTextureFromPCX("Art/WorldSetup/landmassWaterSMALLdepress.pcx", 76 * 7 + 1, 1, 75, 50);
		continents60.Pressed += () => {
			landform = WorldCharacteristics.Landform.Continents;
			ocean = WorldCharacteristics.OceanCoverage.Percent_60;
			ResetLandformGraphics();
			continents60Large.Visible = true;
			continentsLabel.Text = "Continents (60% water)";
		};

		archipelago80.TextureNormal = Util.LoadTextureFromPCX("Art/WorldSetup/landmassWaterSMALL.pcx", 76 * 2 + 1, 1, 75, 50);
		archipelago80.TextureHover = Util.LoadTextureFromPCX("Art/WorldSetup/landmassWaterSMALLrollovers.pcx", 76 * 2 + 1, 1, 75, 50);
		archipelago80.TexturePressed = Util.LoadTextureFromPCX("Art/WorldSetup/landmassWaterSMALLdepress.pcx", 76 * 2 + 1, 1, 75, 50);
		archipelago80.Pressed += () => {
			landform = WorldCharacteristics.Landform.Archipelago;
			ocean = WorldCharacteristics.OceanCoverage.Percent_80;
			ResetLandformGraphics();
			archipelago80Large.Visible = true;
			archipelagoLabel.Text = "Archipelago (80% water)";
		};

		archipelago70.TextureNormal = Util.LoadTextureFromPCX("Art/WorldSetup/landmassWaterSMALL.pcx", 76 * 5 + 1, 1, 75, 50);
		archipelago70.TextureHover = Util.LoadTextureFromPCX("Art/WorldSetup/landmassWaterSMALLrollovers.pcx", 76 * 5 + 1, 1, 75, 50);
		archipelago70.TexturePressed = Util.LoadTextureFromPCX("Art/WorldSetup/landmassWaterSMALLdepress.pcx", 76 * 5 + 1, 1, 75, 50);
		archipelago70.Pressed += () => {
			landform = WorldCharacteristics.Landform.Archipelago;
			ocean = WorldCharacteristics.OceanCoverage.Percent_70;
			ResetLandformGraphics();
			archipelago70Large.Visible = true;
			archipelagoLabel.Text = "Archipelago (70% water)";
		};

		archipelago60.TextureNormal = Util.LoadTextureFromPCX("Art/WorldSetup/landmassWaterSMALL.pcx", 76 * 8 + 1, 1, 75, 50);
		archipelago60.TextureHover = Util.LoadTextureFromPCX("Art/WorldSetup/landmassWaterSMALLrollovers.pcx", 76 * 8 + 1, 1, 75, 50);
		archipelago60.TexturePressed = Util.LoadTextureFromPCX("Art/WorldSetup/landmassWaterSMALLdepress.pcx", 76 * 8 + 1, 1, 75, 50);
		archipelago60.Pressed += () => {
			landform = WorldCharacteristics.Landform.Archipelago;
			ocean = WorldCharacteristics.OceanCoverage.Percent_60;
			ResetLandformGraphics();
			archipelago60Large.Visible = true;
			archipelagoLabel.Text = "Archipelago (60% water)";
		};

		arid.TextureNormal = Util.LoadTextureFromPCX("Art/WorldSetup/climate.pcx", 1, 339, 75, 50);
		arid.TexturePressed = Util.LoadTextureFromPCX("Art/WorldSetup/CLIMTEMPAGEDepress.pcx", 1, 1, 75, 50);
		arid.TextureHover = Util.LoadTextureFromPCX("Art/WorldSetup/CLIMTEMPAGERollovers.pcx", 1, 1, 75, 50);
		arid.Pressed += () => {
			clim = WorldCharacteristics.Climate.Arid;
			ResetClimateGraphics();
			aridLarge.Visible = true;
		};

		normal.TextureNormal = Util.LoadTextureFromPCX("Art/WorldSetup/climate.pcx", 77, 339, 75, 50);
		normal.TexturePressed = Util.LoadTextureFromPCX("Art/WorldSetup/CLIMTEMPAGEDepress.pcx", 77, 1, 75, 50);
		normal.TextureHover = Util.LoadTextureFromPCX("Art/WorldSetup/CLIMTEMPAGEDepress.pcx", 77, 1, 75, 50);
		normal.Pressed += () => {
			clim = WorldCharacteristics.Climate.Normal;
			ResetClimateGraphics();
			normalLarge.Visible = true;
		};

		wet.TextureNormal = Util.LoadTextureFromPCX("Art/WorldSetup/climate.pcx", 153, 339, 75, 50);
		wet.TexturePressed = Util.LoadTextureFromPCX("Art/WorldSetup/CLIMTEMPAGEDepress.pcx", 153, 1, 75, 50);
		wet.TextureHover = Util.LoadTextureFromPCX("Art/WorldSetup/CLIMTEMPAGEDepress.pcx", 153, 1, 75, 50);
		wet.Pressed += () => {
			clim = WorldCharacteristics.Climate.Wet;
			ResetClimateGraphics();
			wetLarge.Visible = true;
		};

		warm.TextureNormal = Util.LoadTextureFromPCX("Art/WorldSetup/temperature.pcx", 1, 339, 75, 50);
		warm.TexturePressed = Util.LoadTextureFromPCX("Art/WorldSetup/CLIMTEMPAGEDepress.pcx", 1, 124, 75, 50);
		warm.TextureHover = Util.LoadTextureFromPCX("Art/WorldSetup/CLIMTEMPAGERollovers.pcx", 1, 124, 75, 50);
		warm.Pressed += () => {
			temp = WorldCharacteristics.Temperature.Warm;
			ResetTemperatureGraphics();
			warmLarge.Visible = true;
		};

		temperate.TextureNormal = Util.LoadTextureFromPCX("Art/WorldSetup/temperature.pcx", 77, 339, 75, 50);
		temperate.TexturePressed = Util.LoadTextureFromPCX("Art/WorldSetup/CLIMTEMPAGEDepress.pcx", 77, 124, 75, 50);
		temperate.TextureHover = Util.LoadTextureFromPCX("Art/WorldSetup/CLIMTEMPAGEDepress.pcx", 77, 124, 75, 50);
		temperate.Pressed += () => {
			temp = WorldCharacteristics.Temperature.Temperate;
			ResetTemperatureGraphics();
			temperateLarge.Visible = true;
		};

		cool.TextureNormal = Util.LoadTextureFromPCX("Art/WorldSetup/temperature.pcx", 153, 339, 75, 50);
		cool.TexturePressed = Util.LoadTextureFromPCX("Art/WorldSetup/CLIMTEMPAGEDepress.pcx", 153, 124, 75, 50);
		cool.TextureHover = Util.LoadTextureFromPCX("Art/WorldSetup/CLIMTEMPAGEDepress.pcx", 153, 124, 75, 50);
		cool.Pressed += () => {
			temp = WorldCharacteristics.Temperature.Cool;
			ResetTemperatureGraphics();
			coolLarge.Visible = true;
		};

		billion3.TextureNormal = Util.LoadTextureFromPCX("Art/WorldSetup/age.pcx", 1, 339, 75, 50);
		billion3.TexturePressed = Util.LoadTextureFromPCX("Art/WorldSetup/CLIMTEMPAGEDepress.pcx", 1, 281, 75, 50);
		billion3.TextureHover = Util.LoadTextureFromPCX("Art/WorldSetup/CLIMTEMPAGERollovers.pcx", 1, 281, 75, 50);
		billion3.Pressed += () => {
			age = WorldCharacteristics.Age.Billion_3;
			ResetAgeGraphics();
			billion3Large.Visible = true;
		};

		billion4.TextureNormal = Util.LoadTextureFromPCX("Art/WorldSetup/age.pcx", 77, 339, 75, 50);
		billion4.TexturePressed = Util.LoadTextureFromPCX("Art/WorldSetup/CLIMTEMPAGEDepress.pcx", 77, 281, 75, 50);
		billion4.TextureHover = Util.LoadTextureFromPCX("Art/WorldSetup/CLIMTEMPAGEDepress.pcx", 77, 281, 75, 50);
		billion4.Pressed += () => {
			age = WorldCharacteristics.Age.Billion_4;
			ResetAgeGraphics();
			billion4Large.Visible = true;
		};

		billion5.TextureNormal = Util.LoadTextureFromPCX("Art/WorldSetup/age.pcx", 153, 339, 75, 50);
		billion5.TexturePressed = Util.LoadTextureFromPCX("Art/WorldSetup/CLIMTEMPAGEDepress.pcx", 153, 281, 75, 50);
		billion5.TextureHover = Util.LoadTextureFromPCX("Art/WorldSetup/CLIMTEMPAGEDepress.pcx", 153, 281, 75, 50);
		billion5.Pressed += () => {
			age = WorldCharacteristics.Age.Billion_5;
			ResetAgeGraphics();
			billion5Large.Visible = true;
		};

		confirm.TextureNormal = Util.LoadTextureFromPCX("Art/X-o_ALLstates-sprite.pcx", 1, 1, 19, 19);
		confirm.TextureHover = Util.LoadTextureFromPCX("Art/X-o_ALLstates-sprite.pcx", 37, 1, 19, 19);
		confirm.TexturePressed = Util.LoadTextureFromPCX("Art/X-o_ALLstates-sprite.pcx", 73, 1, 19, 19);
		confirm.Pressed += CreateGame;

		cancel.TextureNormal = Util.LoadTextureFromPCX("Art/X-o_ALLstates-sprite.pcx", 21, 1, 15, 19);
		cancel.TextureHover = Util.LoadTextureFromPCX("Art/X-o_ALLstates-sprite.pcx", 57, 1, 15, 19);
		cancel.TexturePressed = Util.LoadTextureFromPCX("Art/X-o_ALLstates-sprite.pcx", 93, 1, 15, 19);
		cancel.Pressed += BackToMainMenu;

		pangaea60Large.Texture = Util.LoadTextureFromPCX("Art/WorldSetup/landmassWaterlarge.pcx", 1, 551, 300, 200);
		pangaea70Large.Texture = Util.LoadTextureFromPCX("Art/WorldSetup/landmassWaterlarge.pcx", 1, 276, 300, 200);
		pangaea80Large.Texture = Util.LoadTextureFromPCX("Art/WorldSetup/landmassWaterlarge.pcx", 1, 1, 300, 200);
		continents60Large.Texture = Util.LoadTextureFromPCX("Art/WorldSetup/landmassWaterlarge.pcx", 301 + 1, 551, 300, 200);
		continents70Large.Texture = Util.LoadTextureFromPCX("Art/WorldSetup/landmassWaterlarge.pcx", 301 + 1, 276, 300, 200);
		continents80Large.Texture = Util.LoadTextureFromPCX("Art/WorldSetup/landmassWaterlarge.pcx", 301 + 1, 1, 300, 200);
		archipelago60Large.Texture = Util.LoadTextureFromPCX("Art/WorldSetup/landmassWaterlarge.pcx", 301 * 2 + 1, 551, 300, 200);
		archipelago70Large.Texture = Util.LoadTextureFromPCX("Art/WorldSetup/landmassWaterlarge.pcx", 301 * 2 + 1, 276, 300, 200);
		archipelago80Large.Texture = Util.LoadTextureFromPCX("Art/WorldSetup/landmassWaterlarge.pcx", 301 * 2 + 1, 1, 300, 200);

		aridLarge.Texture = Util.LoadTextureFromPCX("Art/WorldSetup/climate.pcx", 1, 1, 300, 200);
		normalLarge.Texture = Util.LoadTextureFromPCX("Art/WorldSetup/climate.pcx", 302, 1, 300, 200);
		wetLarge.Texture = Util.LoadTextureFromPCX("Art/WorldSetup/climate.pcx", 603, 1, 300, 200);
		coolLarge.Texture = Util.LoadTextureFromPCX("Art/WorldSetup/temperature.pcx", 603, 1, 300, 200);
		temperateLarge.Texture = Util.LoadTextureFromPCX("Art/WorldSetup/temperature.pcx", 302, 1, 300, 200);
		warmLarge.Texture = Util.LoadTextureFromPCX("Art/WorldSetup/temperature.pcx", 1, 1, 300, 200);
		billion3Large.Texture = Util.LoadTextureFromPCX("Art/WorldSetup/age.pcx", 1, 1, 300, 200);
		billion4Large.Texture = Util.LoadTextureFromPCX("Art/WorldSetup/age.pcx", 302, 1, 300, 200);
		billion5Large.Texture = Util.LoadTextureFromPCX("Art/WorldSetup/age.pcx", 603, 1, 300, 200);

		ResetLandformGraphics();
		pangaea70Large.Visible = true;
		pangaea70.ButtonPressed = true;
		pangaeaLabel.Text = "Pangaea (70% water)";

		ResetClimateGraphics();
		normalLarge.Visible = true;
		normal.ButtonPressed = true;

		ResetTemperatureGraphics();
		temperateLarge.Visible = true;
		temperate.ButtonPressed = true;

		ResetAgeGraphics();
		billion4Large.Visible = true;
		billion4.ButtonPressed = true;

		// TODO: handle different map sizes properly (including loading the
		// optimal city number, etc)
		tinySize.Visible = false;
		smallSize.Visible = false;
		standardSize.Visible = false;
		largeSize.Visible = false;
		hugeSize.Visible = false;
		randomSize.Visible = false;
	}

	private void ResetLandformGraphics() {
		pangaea60Large.Visible = false;
		pangaea70Large.Visible = false;
		pangaea80Large.Visible = false;
		continents60Large.Visible = false;
		continents70Large.Visible = false;
		continents80Large.Visible = false;
		archipelago60Large.Visible = false;
		archipelago70Large.Visible = false;
		archipelago80Large.Visible = false;

		pangaeaLabel.Text = "Pangaea";
		continentsLabel.Text = "Continents";
		archipelagoLabel.Text = "Archipelago";
	}

	private void ResetClimateGraphics() {
		aridLarge.Visible = false;
		normalLarge.Visible = false;
		wetLarge.Visible = false;
	}

	private void ResetTemperatureGraphics() {
		coolLarge.Visible = false;
		temperateLarge.Visible = false;
		warmLarge.Visible = false;
	}

	private void ResetAgeGraphics() {
		billion3Large.Visible = false;
		billion4Large.Visible = false;
		billion5Large.Visible = false;
	}

	private void CreateGame() {
		loadingLabel.Visible = true;
		GlobalSingleton Global = GetNode<GlobalSingleton>("/root/GlobalSingleton");

		// World generation can take a bit of time if multiple attempts are 
		// needed, so we don't want to tie up the UI thread. 
		Thread thread = new(() => { DoWorldGenerationAndstartGame(Global); });
		thread.Start();
	}

	private void DoWorldGenerationAndstartGame(GlobalSingleton Global) {
		Global.ResetLoadGamePath();

		SaveGame save = SaveManager.LoadSave(Global.DefaultGamePath, Global.DefaultBicPath, (string unused) => { return unused; });
		save.Map = new SaveMap(MapGenerator.GenerateMap(new WorldCharacteristics() {
			landform = landform,
			oceanCoverage = ocean,
			age = age,
			climate = clim,
			temperature = temp,
			worldSize = new WorldSize() {
				width = 100,
				height = 100,
				numberOfCivs = 8,
				distanceBetweenCivs = 12,
				techRate = 240,
				optimalNumberOfCities = 20,
			},
			terrainTypes = save.TerrainTypes,
			resources = save.Resources,
			defaultGovernment = save.Governments.Find(x => x.defaultType),
			mapSeed = Int32.Parse(seedInput.Text),
		}));

		// Hack: reposition the initial units to the starting locations from the
		// generated map. Longer term we'll need to split out our own 
		// "conquests.bic" type file and load that - until then we'll use this
		// hack of grabbing it from the static save.
		//
		// Start at index 1 to skip the barbarians.
		for (int i = 1; i < save.Players.Count; ++i) {
			SaveTile startingTile = save.Map.startingLocations[i - 1];
			TileLocation startingLocation = new TileLocation(startingTile.X, startingTile.Y);
			SavePlayer player = save.Players[i];

			foreach (SaveUnit unit in save.Units) {
				if (unit.owner == player.id) {
					unit.currentLocation = startingLocation;
				}
			}
			player.tileKnowledge.Clear();
		}

		log.Information("saving generated map");
		save.Save(Global.DefaultGeneratedGamePath);
		Global.LoadGamePath = Global.DefaultGeneratedGamePath;

		log.Information("opening map");
		CallDeferred("StartGame");
	}

	private void StartGame() {
		GetTree().ChangeSceneToFile("res://C7Game.tscn");
	}

	private void BackToMainMenu() {
		GetTree().ChangeSceneToFile("res://MainMenu.tscn");
	}
}
