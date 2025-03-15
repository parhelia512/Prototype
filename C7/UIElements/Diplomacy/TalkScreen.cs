using C7Engine;
using C7GameData;
using Godot;
using System;
using System.Collections.Generic;
using ConvertCiv3Media;

public partial class TalkScreen : TextureRect {
	private ID humanPlayerId;
	private ID opponentPlayerId;

	Theme fontTheme = new();
	FontFile font = new();

	public TalkScreen(ID humanPlayer, ID opponentPlayer) {
		this.humanPlayerId = humanPlayer;
		this.opponentPlayerId = opponentPlayer;
	}

	public override void _Ready() {
		this.CreateUI();
	}

	private void CreateUI() {
		// Load the font we'll use.
		//
		// We skip the cache so that we can change the size without affecting other
		// code using the same font.
		font = ResourceLoader.Load<FontFile>("res://Fonts/NotoSans-Regular.ttf", null, ResourceLoader.CacheMode.Ignore);
		font.FixedSize = 14;
		fontTheme.DefaultFont = font;

		this.Texture = Util.LoadTextureFromPCX("Art/Diplomacy/talk_offer.pcx");

		ColorRect headBackground = new();
		headBackground.Color = Colors.Black;
		headBackground.Size = new Vector2(200, 240);
		headBackground.SetPosition(new Vector2(512 - 100, 59));
		AddChild(headBackground);

		TextureRect leaderHead = new();
		string civNameText;
		string leaderName;
		using (UIGameDataAccess gameDataAccess = new()) {
			GameData gD = gameDataAccess.gameData;
			Player opponentPlayer = gD.players.Find(x => x.id == opponentPlayerId);
			civNameText = $"{opponentPlayer.civilization.name} (Cautious)";
			leaderName = opponentPlayer.civilization.leader;

			int xOffset = opponentPlayer.EraIndex() * 115;

			// TODO: track mood in the player relationship data structure.
			int yOffset = 115;  // 0 is annoyed, 115*2 is mad.

			Pcx headPcx = Util.LoadPCX(opponentPlayer.civilization.leaderArtFile);
			leaderHead.Texture = PCXToGodot.getImageTextureFromPCX(
						headPcx,
						new(xOffset, yOffset, 115, 115),
						new(false, []));
			leaderHead.Scale = new Vector2(1.7f, 1.7f);
		}

		leaderHead.SetPosition(new Vector2(512 - (115 * 1.7f) / 2, 59 + 120 - (115 * 1.7f) / 2));
		AddChild(leaderHead);

		Label civName = new();
		civName.SetPosition(new Vector2(0, 330));
		AddChild(civName);
		civName.Text = civNameText;
		civName.HorizontalAlignment = HorizontalAlignment.Center;
		civName.AnchorLeft = 0.5f;
		civName.AnchorRight = 0.5f;
		civName.OffsetLeft = -1 * (civName.Size.X / 2.0f);
		civName.Theme = fontTheme;

		Button proposeDeal = new();
		proposeDeal.Text = "We would like to propose a deal...";
		proposeDeal.SetPosition(new Vector2(512 - 205, 500));
		proposeDeal.Theme = fontTheme;
		// TODO: Advance to the next screen.
		AddChild(proposeDeal);

		Button declareWar = new();
		declareWar.Text = "That's it! Prepare for WAR!";
		declareWar.SetPosition(new Vector2(512 - 205, 520));
		declareWar.Theme = fontTheme;
		declareWar.Pressed += DeclareWar;
		AddChild(declareWar);

		Button goodbye = new();
		goodbye.Text = $"That's it. Goodbye, {leaderName}";
		goodbye.SetPosition(new Vector2(512 - 205, 540));
		goodbye.Theme = fontTheme;
		goodbye.Pressed += () => { GetParent<Diplomacy>().Hide(); };
		AddChild(goodbye);
	}

	private void DeclareWar() {
		using UIGameDataAccess gameDataAccess = new();
		GameData gD = gameDataAccess.gameData;
		Player humanPlayer = gD.players.Find(x => x.id == humanPlayerId);
		Player opponentPlayer = gD.players.Find(x => x.id == opponentPlayerId);
		GetParent<Diplomacy>().popupOverlay.ShowPopup(new WarConfirmation(opponentPlayer,
				() => {
					humanPlayer.DeclareWarOn(opponentPlayer, gD.turn);
					GetParent<Diplomacy>().Hide();
				}), PopupOverlay.PopupCategory.Advisor);
	}
}
