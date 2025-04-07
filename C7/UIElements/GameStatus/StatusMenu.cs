using Godot;
using System;
using C7GameData;
using C7Engine;

[GlobalClass]
public partial class StatusMenu : Control {
	[Export] ConsoleButton openDiplomacy;
	[Export] ConsoleButton openPalaceScreen;

	[Export] PopupOverlay popupOverlay;
	[Export] PalaceScreen palaceScreen;

	public override void _Ready() {
		openDiplomacy.Pressed += OpenDiplomacyPopup;
		openPalaceScreen.Pressed += palaceScreen.Show;
	}

	public override void _Process(double delta) {
		using UIGameDataAccess gameDataAccess = new();
		GameData gD = gameDataAccess.gameData;
		Player player = gD.GetHumanPlayers()[0];

		// Only show the diplomacy button if we have civs to talk to.
		if (player.playerRelationships.Count > 0) {
			openDiplomacy.ShowButton();
		}

		// TODO: Don't show the palace button if the player can't start building the palace
		openPalaceScreen.ShowButton();
	}

	private void OpenDiplomacyPopup() {
		using (UIGameDataAccess gameDataAccess = new()) {
			GameData gD = gameDataAccess.gameData;
			Player player = gD.GetHumanPlayers()[0];

			popupOverlay.ShowPopup(new DiplomacySelection(player, gD.players), PopupOverlay.PopupCategory.Info);
		}
	}
}
