using Godot;
using System;
using C7GameData;
using C7Engine;
using Serilog;
using System.Collections.Generic;


// A container for all the diplomacy-related screens that aren't simple popups.
public partial class Diplomacy : CenterContainer {
	private ILogger log = LogManager.ForContext<Diplomacy>();

	[Export]
	public PopupOverlay popupOverlay;

	private TalkScreen talkScreen;

	public override void _Ready() {
		this.Hide();
	}

	public void ShowTalkScreenForPlayer(ID humanPlayer, ID opponentPlayer) {
		if (talkScreen != null) {
			RemoveChild(talkScreen);
			talkScreen = null;
		}

		using (UIGameDataAccess gameDataAccess = new()) {
			GameData gd = gameDataAccess.gameData;
			Player opponent = gd.players.Find(x => x.id == opponentPlayer);
			Player human = gd.players.Find(x => x.id == humanPlayer);
			if (!opponent.WillAcceptCommunicationFrom(human, gd.turn)) {
				popupOverlay.ShowPopup(
					new InformationalPopup(
						$"The {opponent.civilization.noun} refused to acknowledge our envoy!"),
						PopupOverlay.PopupCategory.Advisor);
				return;
			}
		}

		talkScreen = new TalkScreen(humanPlayer, opponentPlayer);
		AddChild(talkScreen);
		this.Show();
	}
}
