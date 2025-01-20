using System;
using Godot;
using Serilog;

public partial class GameMenu : Popup {
	private ILogger log;

	Civ3FileDialog GameMenuLoadDialog;

	// An object for passing information (like save file paths) between scenes.
	GlobalSingleton Global;

	public GameMenu() {
		alignment = BoxContainer.AlignmentMode.Center;
		margins = new Margins(top: 100);
	}

	public override void _Ready() {
		base._Ready();
		log = LogManager.ForContext<GameMenu>();

		AddTexture(370, 300);
		AddBackground(370, 300);

		AddHeader("Main Menu", 10);

		AddButton("Map", 60, map);
		AddButton("Load Game (Ctrl-L)", 85, load);
		AddButton("New Game (Ctrl-Shift-Q)", 110, newGame);
		AddButton("Preferences (Ctrl-P)", 135, preferences);
		AddButton("Retire (Ctrl-Q)", 160, retire);
		AddButton("Save Game (Ctrl-S)", 185, save);
		AddButton("Quit Game (ESC)", 210, quit);

		Global = GetNode<GlobalSingleton>("/root/GlobalSingleton");

		GameMenuLoadDialog = new Civ3FileDialog();
		AddChild(GameMenuLoadDialog);
		GameMenuLoadDialog.SetDirectory(@"Conquests/Saves");
		GameMenuLoadDialog.FileSelected += OnFileSelected;
	}

	private void save() {
		throw new NotImplementedException();
	}

	private void preferences() {
		throw new NotImplementedException();
	}

	private void newGame() {
		throw new NotImplementedException();
	}

	private void load() {
		log.Information("load game button pressed");
		// TODO: The main menu does sound playing but we don't know our path in
		// the scene, which makes this hard.
		// PlayButtonPressedSound();
		GameMenuLoadDialog.Popup();
	}

	private void quit() {
		GetParent().EmitSignal(PopupOverlay.SignalName.Quit);
	}

	private void retire() {
		GetParent().EmitSignal(PopupOverlay.SignalName.Retire);
	}

	private void map() {
		GetParent().EmitSignal(PopupOverlay.SignalName.HidePopup);
	}

	private void OnFileSelected(string path) {
		log.Information($"loading {path}");
		Global.LoadGamePath = path;
		GetTree().ChangeSceneToFile("res://C7Game.tscn");
	}
}
