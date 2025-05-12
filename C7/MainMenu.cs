using Godot;
using System;
using C7Engine;
using C7GameData;
using C7GameData.Save;
using Serilog;

public partial class MainMenu : Node2D {
	private ILogger log;

	readonly int BUTTON_LABEL_OFFSET = 0;

	ImageTexture InactiveButton;
	ImageTexture HoverButton;
	TextureRect MainMenuBackground;
	[Export]
	Civ3FileDialog LoadDialog;
	[Export]
	Button SetCiv3Home;
	[Export]
	FileDialog SetCiv3HomeDialog;
	[Export]
	Civ3FileDialog LoadScenarioDialog;
	GlobalSingleton Global;

	readonly int MENU_OFFSET_FROM_TOP = 180;
	readonly int MENU_OFFSET_FROM_LEFT = 180;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		log = LogManager.ForContext<MainMenu>();
		log.Debug("enter MainMenu._Ready");

		DisplayServer.WindowSetTitle("C7 - Godot 4");

		// To pass data between scenes, putting path string in a global singleton and reading it later in createGame
		Global = GetNode<GlobalSingleton>("/root/GlobalSingleton");
		Global.ResetLoadGamePath();

		LoadDialog.SetDirectoryForLoading(@"Conquests/Saves");
		LoadScenarioDialog.SetDirectoryForLoading(@"Conquests/Scenarios");

		DisplayTitleScreen();
	}

	private void DisplayTitleScreen() {
		try {
			SetMainMenuBackground();

			InactiveButton = Util.LoadTextureFromPCX("Art/buttonsFINAL.pcx", 1, 1, 20, 20, false);
			HoverButton = Util.LoadTextureFromPCX("Art/buttonsFINAL.pcx", 22, 1, 20, 20, false);

			AddButton("New Game", 0, GenerateNewGame);
			AddButton("Quick Start", 35, GenerateNewGame);
			AddButton("Tutorial", 70, StartGame);
			AddButton("Load Game", 105, LoadGame);
			AddButton("Load Scenario", 140, LoadScenario);
			AddButton("Hall of Fame", 175, HallOfFame);
			AddButton("Preferences", 210, Preferences);
			AddButton("Audio Preferences", 245, Preferences);
			AddButton("Credits", 280, showCredits);
			AddButton("Exit", 315, _on_Exit_pressed);

			// Hide select home folder if valid path is present as proven by reaching this point in code
			SetCiv3Home.Visible = false;
		} catch (Exception ex) {
			log.Error(ex, "Could not set up the main menu");
			GetNode<Label>("CanvasLayer/Label").Visible = true;
			GetNode<ColorRect>("CanvasLayer/ColorRect").Visible = true;
		}
	}

	private void SetMainMenuBackground() {
		ImageTexture TitleScreenTexture = Util.LoadTextureFromC7JPG("Art/Title_Screen.jpg");
		MainMenuBackground = GetNode<TextureRect>("CanvasLayer/MainMenuBackground");
		MainMenuBackground.StretchMode = TextureRect.StretchModeEnum.Scale;
		MainMenuBackground.Texture = TitleScreenTexture;
	}

	private void AddButton(string label, int verticalPosition, Action action) {
		TextureButton newButton = new TextureButton();
		newButton.TextureNormal = InactiveButton;
		newButton.TextureHover = HoverButton;
		newButton.SetPosition(new Vector2(MENU_OFFSET_FROM_LEFT, MENU_OFFSET_FROM_TOP + verticalPosition));
		MainMenuBackground.AddChild(newButton);
		newButton.Pressed += action;

		Theme theme = new Theme();
		theme.SetFontSize("font_size", "Button", 14);
		Button newButtonLabel = new Button();
		newButtonLabel.Theme = theme;
		newButtonLabel.Text = label;

		newButtonLabel.SetPosition(new Vector2(MENU_OFFSET_FROM_LEFT + 25, MENU_OFFSET_FROM_TOP + verticalPosition + BUTTON_LABEL_OFFSET));
		MainMenuBackground.AddChild(newButtonLabel);
		newButtonLabel.Pressed += action;
	}

	public void GenerateNewGame() {
		log.Information("generating new map");
		PlayButtonPressedSound();

		int mapSeed = new Random().Next(int.MaxValue);
		Random rand = new(mapSeed + 0x987);

		WorldCharacteristics.OceanCoverage[] oceans = {
			WorldCharacteristics.OceanCoverage.Percent_60,
			WorldCharacteristics.OceanCoverage.Percent_70,
			WorldCharacteristics.OceanCoverage.Percent_80,
		};
		WorldCharacteristics.Age[] ages = {
			WorldCharacteristics.Age.Billion_3,
			WorldCharacteristics.Age.Billion_4,
			WorldCharacteristics.Age.Billion_5,
		};
		WorldCharacteristics.Temperature[] temps = {
			WorldCharacteristics.Temperature.Cool,
			WorldCharacteristics.Temperature.Temperate,
			WorldCharacteristics.Temperature.Warm,
		};
		WorldCharacteristics.Climate[] climates = {
			WorldCharacteristics.Climate.Wet,
			WorldCharacteristics.Climate.Normal,
			WorldCharacteristics.Climate.Arid,
		};

		SaveGame save = SaveManager.LoadSave(Global.DefaultGamePath, Global.DefaultBicPath, (string unused) => { return unused; });
		save.Map = new SaveMap(MapGenerator.GenerateMap(new WorldCharacteristics() {
			// Use pangaea to start until we implement more boat logic
			landform = WorldCharacteristics.Landform.Pangaea,
			oceanCoverage = oceans[rand.Next(3)],
			age = ages[rand.Next(3)],
			climate = climates[rand.Next(3)],
			temperature = temps[rand.Next(3)],
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
			mapSeed = mapSeed,
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
		GetTree().ChangeSceneToFile("res://C7Game.tscn");
	}

	public void StartGame() {
		log.Information("start game button pressed");
		PlayButtonPressedSound();
		GetTree().ChangeSceneToFile("res://C7Game.tscn");
	}

	public void LoadGame() {
		log.Information("load game button pressed");
		PlayButtonPressedSound();
		LoadDialog.Popup();
	}

	public void LoadScenario() {
		log.Information("load scenario button pressed");
		PlayButtonPressedSound();
		LoadScenarioDialog.Popup();
	}

	public void showCredits() {
		log.Information("credits button pressed");
		GetTree().ChangeSceneToFile("res://Credits.tscn");
	}

	public void HallOfFame() {
		PlayButtonPressedSound();
	}

	public void Preferences() {
		PlayButtonPressedSound();
	}

	public void _on_Exit_pressed() {
		GetTree().Quit(); // no need to notify the scene tree
	}

	private void PlayButtonPressedSound() {
		AudioStreamWav wav = Util.LoadWAVFromDisk(Util.Civ3MediaPath("Sounds/Button1.wav"));
		AudioStreamPlayer player = GetNode<AudioStreamPlayer>("CanvasLayer/SoundEffectPlayer");
		player.Stream = wav;
		player.Play();
	}

	private void _on_SetCiv3Home_pressed() {
		SetCiv3HomeDialog.Popup();
	}

	private void _on_SetCiv3HomeDialog_dir_selected(string path) {
		Util.Civ3Root = path;
		C7Settings.SetValue("locations", "civ3InstallDir", path);
		C7Settings.SaveSettings();
		// This function should only be reachable if DisplayTitleScreen failed on previous runs, so should be OK to run here
		DisplayTitleScreen();
	}
}
