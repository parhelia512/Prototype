using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using C7GameData;
using C7Engine;
using ConvertCiv3Media;
using C7GameData.Save;
using Serilog;

[Tool]
public partial class PlayerSetup : Control {
	private static ILogger log = LogManager.ForContext<PlayerSetup>();

	[Export] TextureRect background;

	[Export] GridContainer playerListContainer;
	List<Civilization> civilizations = new();
	Civilization civilization = null;
	ButtonGroup playerListButtonGroup = new();
	TextureRect leaderHead = new();
	[Export] Label civLabel;

	[Export] GridContainer opponentListContainer;
	List<OptionButton> opponentSelectors = new();

	[Export] GridContainer difficultyContainer;

	Difficulty difficulty = null;
	ButtonGroup difficultyButtonGroup = new();

	[Export] TextureButton confirm;
	[Export] TextureButton cancel;

	[Export] Label loadingLabel;

	ID.Factory ids;

	// TODO: read this from the rules based on the world size
	const int NUM_OPPONENTS = 7;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		background.Texture = TextureLoader.Load("player_setup.background");

		SaveGame save = GetSave();

		// Set up buttons for the civs the player can play as.
		//
		// TODO: Bump this to Dutch once we bump the release.
		civilizations = save.Civilizations;
		playerListContainer.Columns = (int)Math.Ceiling(save.Civilizations.Count / 12.0);
		string initiallySelectedCiv = save.Civilizations.Any(x => x.name == "Carthage") ? "Carthage" : save.Civilizations[1].name;
		foreach (Civilization civ in save.Civilizations) {
			if (civ.name == "A Barbarian Chiefdom") {
				continue;
			}

			Civ3MenuButton button = new() {
				Text = civ.name,
				FontSize = 12,
				textPosition = Civ3MenuButton.TextPosition.TextLeftOfIcon,
				ButtonGroup = playerListButtonGroup,
				ToggleMode = true,
			};
			button.Pressed += () => {
				this.civilization = civ;
				UpdateOpponentSelectors();
				DisplaySelectedLeader();
			};
			playerListContainer.AddChild(button);

			if (civ.name == initiallySelectedCiv) {
				button.ButtonPressed = true;
				this.civilization = civ;
			}
		}
		background.AddChild(leaderHead);
		DisplaySelectedLeader();

		// Set up the options for opponents.
		AddOpponentSelectors();

		// Set up the difficulty buttons
		difficultyContainer.Columns = save.Difficulties.Count;
		string initiallySelectedDifficulty = save.Difficulties.Any(x => x.Name == "Regent") ? "Regent" : save.Difficulties[0].Name;
		foreach (Difficulty difficulty in save.Difficulties) {
			CenterContainer container = new();
			difficultyContainer.AddChild(container);

			Civ3MenuButton button = new(Civ3MenuButton.TextPosition.TextAboveIcon) {
				Text = difficulty.Name,
				ButtonGroup = difficultyButtonGroup,
				ToggleMode = true,
			};
			button.Pressed += () => { this.difficulty = difficulty; };
			container.AddChild(button);
			if (difficulty.Name == initiallySelectedDifficulty) {
				button.ButtonPressed = true;
				this.difficulty = difficulty;
			}
			container.CustomMinimumSize = new Vector2(843.0f / difficultyContainer.Columns, 0);
		}

		TextureLoader.SetButtonTextures(confirm, "ui.confirm");
		confirm.Pressed += CreateGame;

		TextureLoader.SetButtonTextures(cancel, "ui.cancel");
		cancel.Pressed += BackToMainMenu;
	}

	private void BackToMainMenu() {
		GetTree().ChangeSceneToFile("res://UIElements/MainMenu/main_menu.tscn");
	}

	private void AddOpponentSelectors() {
		// TODO: The number of opponents should come from the rule set.
		for (int i = 0; i < NUM_OPPONENTS; ++i) {
			OptionButton optionButton = new();
			StyleBoxFlat styleBox = new() {
				BorderColor = Color.Color8(150, 150, 150, 220),
				BgColor = Color.Color8(255, 255, 255, 0),
				BorderWidthBottom = 2,
				BorderWidthLeft = 2,
				BorderWidthRight = 2,
				BorderWidthTop = 2,
				ContentMarginLeft = 4,
				ContentMarginRight = 4,
				ContentMarginTop = 4,
				ContentMarginBottom = 4
			};
			optionButton.AddThemeStyleboxOverride("normal", styleBox);
			optionButton.AddThemeStyleboxOverride("hover", styleBox); ;
			optionButton.AddThemeStyleboxOverride("pressed", styleBox);

			PopupMenu popup = optionButton.GetPopup();
			popup.AddThemeStyleboxOverride("panel", styleBox);
			popup.MaxSize = new Vector2I(popup.MaxSize.X, 300);
			opponentSelectors.Add(optionButton);

			CenterContainer container = new();
			opponentListContainer.AddChild(container);
			container.AddChild(optionButton);

			container.CustomMinimumSize = new Vector2(312.0f / opponentListContainer.Columns, 315.0f / NUM_OPPONENTS);
			optionButton.CustomMinimumSize = new Vector2(290.0f / opponentListContainer.Columns, optionButton.CustomMinimumSize.Y);

			foreach (Civilization civ in civilizations) {
				if (civ.name == "A Barbarian Chiefdom") {
					continue;
				}
				optionButton.AddItem(civ.name);
			}

			// Add (and default to) random for each opponent.
			optionButton.AddItem("Random");
			optionButton.Select(popup.ItemCount - 1);

			for (int k = 0; k < popup.ItemCount; ++k) {
				popup.SetItemAsRadioCheckable(k, false);
				popup.SetItemAsCheckable(k, false);
			}

			optionButton.ItemSelected += (long i) => { UpdateOpponentSelectors(); };
		}

		UpdateOpponentSelectors();
	}

	private void UpdateOpponentSelectors() {
		HashSet<string> civsTaken = new();
		civsTaken.Add(civilization.name);

		foreach (OptionButton ob in opponentSelectors) {
			string selection = ob.GetItemText(ob.Selected);

			// If the player decides to play as civ X and one of the opponent
			// selectors has X selected, change it to random. Similarly if one
			// of the previous option buttons has selected this civ.
			if (civsTaken.Contains(selection)) {
				ob.Select(ob.GetPopup().ItemCount - 1);
			} else if (selection != "Random") {
				civsTaken.Add(selection);
			}

			for (int i = 0; i < ob.GetPopup().ItemCount; ++i) {
				ob.SetItemDisabled(i, civsTaken.Contains(ob.GetItemText(i)));
			}
		}
	}

	private void DisplaySelectedLeader() {
		Pcx headPcx = TextureLoader.LoadPCX(civilization.leaderArtFile);
		leaderHead.Texture = PCXToGodot.getImageTextureFromPCX(
					headPcx,
					new(0, 115, 115, 115),
					new(false, [255]));
		leaderHead.Scale = new Vector2(1.7f, 1.7f);
		leaderHead.SetPosition(new Vector2(414, 46));

		string traits = string.Join(", ", civilization.traits);
		civLabel.Text = $"{civilization.leader} of the {civilization.noun}\n({traits})";
	}

	private SaveGame GetSave() {
		if (!Engine.IsEditorHint()) {
			GlobalSingleton Global = GetNode<GlobalSingleton>("/root/GlobalSingleton");
			return SaveManager.LoadSave(Global.DefaultGamePath, Global.DefaultBicPath, (string unused) => { return unused; });
		} else {
			// Hardcoded fallback for the godot editor, which doesn't handle the
			// global.
			return SaveManager.LoadSave(@"./Text/c7-static-map-save.json", "", (string unused) => { return unused; });
		}
	}

	private void CreateGame() {
		loadingLabel.Visible = true;
		GlobalSingleton global = GetNode<GlobalSingleton>("/root/GlobalSingleton");
		SaveGame save = GetSave();

		// World generation can take a bit of time if multiple attempts are
		// needed, so we don't want to tie up the UI thread.
		Thread thread = new(() => { DoWorldGenerationAndstartGame(save, global); });
		thread.Start();
	}

	private void DoWorldGenerationAndstartGame(SaveGame save, GlobalSingleton Global) {
		log.Information("Starting map generation");
		save.Map = new SaveMap(MapGenerator.GenerateMap(Global.WorldCharacteristics));
		log.Information("Done with map generation");
		Random rand = new(Global.WorldCharacteristics.mapSeed + 0x531);
		ids = new(save);

		// Hack: reuse the save but clear out the non-barbarian players.
		// 
		// Longer term we'll need to split out our own
		// "conquests.bic" type file and load that - until then we'll use this
		// hack of reusing the static save.
		//
		// Start at index 1 to skip the barbarians.
		save.Players.RemoveRange(1, save.Players.Count - 1);

		// Clear out the units, we'll add new ones.
		save.Units.Clear();

		// Add the human player.
		AddPlayer(save, this.civilization,
				  Global.WorldCharacteristics.defaultGovernment.id,
				  isHuman: true);

		// Add the opponents.
		HashSet<string> taken = new();
		taken.Add(this.civilization.name);

		foreach (OptionButton ob in opponentSelectors) {
			string selectedName = ob.GetItemText(ob.Selected);
			if (taken.Contains(selectedName)) {
				selectedName = "Random";
			}

			if (selectedName == "Random") {
				do {
					selectedName = civilizations[rand.Next(1, civilizations.Count)].name;
				} while (taken.Contains(selectedName));
			}
			taken.Add(selectedName);

			Civilization civ = civilizations.Find(x => x.name == selectedName);
			AddPlayer(save, civ,
					  Global.WorldCharacteristics.defaultGovernment.id,
					  isHuman: false);
		}

		log.Information("saving generated map");
		save.Save(Global.DefaultGeneratedGamePath);
		Global.LoadGamePath = Global.DefaultGeneratedGamePath;

		log.Information("opening map");
		CallDeferred("StartGame");
	}

	private void AddPlayer(SaveGame save, Civilization civ, ID defaultGovernment, bool isHuman) {
		SavePlayer player = new() {
			human = isHuman,
			id = ids.CreateID("Player"),
			colorIndex = civ.colorIndex,
			barbarian = false,
			civilization = civ.name,
			knownTechs = civ.startingTechs,
			// TODO: stop hardcoding this
			eraCivilopediaName = "ERAS_Ancient_Times",
			// TODO: load this from the rules
			gold = 10,
			governmentId = defaultGovernment,
		};
		save.Players.Add(player);

		SaveTile startingTile = save.Map.startingLocations[save.Players.Count - 2];
		TileLocation startingLocation = new TileLocation(startingTile.X, startingTile.Y);

		AddUnit(save, player, save.Rules.StartUnitType1, startingLocation);
		AddUnit(save, player, save.Rules.StartUnitType2, startingLocation);
		if (civ.traits.Contains(Civilization.Trait.Expansionist)) {
			AddUnit(save, player, save.Rules.ScoutUnitType, startingLocation);
		}
	}

	private void AddUnit(SaveGame save, SavePlayer player, string unitType, TileLocation location) {
		SaveUnit unit = new() {
			id = ids.CreateID(unitType),
			prototype = unitType,
			owner = player.id,
			previousLocation = new TileLocation(-1, -1),
			currentLocation = location,

			// TODO: stop hardcoding these
			hitPointsRemaining = 3,
			movePointsRemaining = 1,
			experience = "Regular",

			facingDirection = TileDirection.NORTH,
		};
		save.Units.Add(unit);
	}

	private void StartGame() {
		GetTree().ChangeSceneToFile("res://C7Game.tscn");
	}
}
