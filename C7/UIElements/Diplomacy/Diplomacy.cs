using Godot;
using System;
using C7GameData;
using C7Engine;
using Serilog;
using ConvertCiv3Media;
using System.Collections.Generic;


// A container for all the diplomacy-related screens that aren't simple popups.
public partial class Diplomacy : CenterContainer {
	private ILogger log = LogManager.ForContext<Diplomacy>();

	[Export]
	public PopupOverlay popupOverlay;

	private TalkScreen talkScreen;
	private DealScreen dealScreen;

	public override void _Ready() {
		this.Hide();
	}

	private void RemoveOtherScreens() {
		if (talkScreen != null) {
			RemoveChild(talkScreen);
			talkScreen = null;
		}
		if (dealScreen != null) {
			RemoveChild(dealScreen);
			dealScreen = null;
		}
	}

	public void ShowDealScreenForPlayer(ID humanPlayer, ID opponentPlayer) {
		RemoveOtherScreens();

		dealScreen = new DealScreen(humanPlayer, opponentPlayer);
		AddChild(dealScreen);
		this.Show();
	}

	public void ShowTalkScreenForPlayer(ID humanPlayer, ID opponentPlayer) {
		RemoveOtherScreens();

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

	public void AddLeaderHeadAndLabel(TextureRect node, Player player, Theme fontTheme) {
		ColorRect headBackground = new();
		headBackground.Color = Colors.Black;
		headBackground.Size = new Vector2(200, 240);
		headBackground.SetPosition(new Vector2(512 - 100, 59));
		node.AddChild(headBackground);

		TextureRect leaderHead = new();
		{
			int xOffset = player.EraIndex() * 115;

			// TODO: track mood in the player relationship data structure.
			int yOffset = 115;  // 0 is annoyed, 115*2 is mad.

			Pcx headPcx = TextureLoader.LoadPCX(player.civilization.leaderArtFile);
			leaderHead.Texture = PCXToGodot.getImageTextureFromPCX(
						headPcx,
						new(xOffset, yOffset, 115, 115),
						new(false, [255]));
			leaderHead.Scale = new Vector2(1.7f, 1.7f);
		}
		leaderHead.SetPosition(new Vector2(512 - (115 * 1.7f) / 2, 59 + 120 - (115 * 1.7f) / 2));
		node.AddChild(leaderHead);

		string civNameText = $"{player.civilization.name} (Cautious)";
		Label civName = new();
		civName.SetPosition(new Vector2(0, 330));
		node.AddChild(civName);
		civName.Theme = fontTheme;
		civName.SetTextAndCenterLabel(civNameText);
	}
}
