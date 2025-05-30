using Godot;
using System;

[Tool]
public partial class MenuButtonContainer : VBoxContainer {
	public partial class MenuButton : HBoxContainer {
		private TextureButton textureButton;
		private Button labelButton;

		private TextureButton TextureButton => textureButton;
		private Button LabelButton => labelButton;

		public string Text {
			get => labelButton?.Text ?? string.Empty;
			set {
				if (labelButton != null)
					labelButton.Text = value;
			}
		}

		public Action Pressed;

		public MenuButton() : this("") {
		}

		public MenuButton(string text) {
			textureButton = new TextureButton();
			labelButton = new Button();

			Theme theme = new();
			theme.SetFontSize("font_size", "Button", 14);
			labelButton.Theme = theme;

			labelButton.Text = text;

			textureButton.TextureNormal = TextureLoader.Load("ui.button.inactive");
			textureButton.TextureHover = TextureLoader.Load("ui.button.hover");

			labelButton.Pressed += () => { Pressed?.Invoke(); };
			textureButton.Pressed += () => { Pressed?.Invoke(); };
		}

		public override void _Ready() {
			AddChild(textureButton);
			AddChild(labelButton);
		}
	}

	public MenuButton NewGame { get; private set; }
	public MenuButton QuickStart { get; private set; }
	public MenuButton Tutorial { get; private set; }
	public MenuButton LoadGame { get; private set; }
	public MenuButton LoadScenario { get; private set; }
	public MenuButton HallOfFame { get; private set; }
	public MenuButton ToggleGraphics { get; private set; }
	public MenuButton Preferences { get; private set; }
	public MenuButton AudioPreferences { get; private set; }
	public MenuButton Credits { get; private set; }
	public MenuButton Exit { get; private set; }

	public override void _Ready() {
		CreateButtons();
	}

	private void CreateButtons() {
		NewGame = new MenuButton("New Game");
		AddChild(NewGame);

		QuickStart = new MenuButton("Quick Start");
		AddChild(QuickStart);

		Tutorial = new MenuButton("Tutorial");
		AddChild(Tutorial);

		LoadGame = new MenuButton("Load Game");
		AddChild(LoadGame);

		LoadScenario = new MenuButton("Load Scenario");
		AddChild(LoadScenario);

		HallOfFame = new MenuButton("Hall of Fame");
		AddChild(HallOfFame);

		ToggleGraphics = new MenuButton("Turn on C7 Graphics");
		AddChild(ToggleGraphics);

		Preferences = new MenuButton("Preferences");
		AddChild(Preferences);

		AudioPreferences = new MenuButton("Audio Preferences");
		AddChild(AudioPreferences);

		Credits = new MenuButton("Credits");
		AddChild(Credits);

		Exit = new MenuButton("Exit");
		AddChild(Exit);
	}
}
