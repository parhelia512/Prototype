using C7Engine;
using C7GameData;
using Godot;
using System;

public partial class MilitaryAdvisor : TextureRect {
	Label totalUnitsLabel = new();
	Label allowedUnitsLabel = new();
	Label unitSupportCostLabel = new();

	public override void _Ready() {
		this.CreateUI();
	}

	private void CreateUI() {
		this.Texture = Util.LoadTextureFromPCX("Art/Advisors/military.pcx");

		ImageTexture DialogBoxTexture = Util.LoadTextureFromPCX("Art/Advisors/dialogbox.pcx");
		TextureButton DialogBox = new TextureButton();
		DialogBox.TextureNormal = DialogBoxTexture;
		DialogBox.SetPosition(new Vector2(806, 110));
		AddChild(DialogBox);

		//TODO: Multi-line capabilities
		Label DialogBoxAdvise = new Label();
		DialogBoxAdvise.Text = "You are running C7!";
		DialogBoxAdvise.SetPosition(new Vector2(815, 119));
		AddChild(DialogBoxAdvise);

		ImageTexture GoBackTexture = Util.LoadTextureFromPCX("Art/exitBox-backgroundStates.pcx", 0, 0, 72, 48);
		TextureButton GoBackButton = new TextureButton();
		GoBackButton.TextureNormal = GoBackTexture;
		GoBackButton.SetPosition(new Vector2(952, 720));
		AddChild(GoBackButton);
		GoBackButton.Pressed += ReturnToMenu;

		using (UIGameDataAccess gameDataAccess = new()) {
			Player player = gameDataAccess.gameData.GetHumanPlayers()[0];
			var (totalUnits, allowedUnits, unitSupportCost) = player.TotalUnitsAllowedUnitsAndSupportCost();

			AddChild(totalUnitsLabel);
			totalUnitsLabel.SetPosition(new Vector2(0, 90));
			totalUnitsLabel.SetTextAndCenterLabel($"Total Units\n{totalUnits}");
			totalUnitsLabel.Position += new Vector2(-50, 0);

			AddChild(allowedUnitsLabel);
			allowedUnitsLabel.SetPosition(new Vector2(0, 139));
			allowedUnitsLabel.SetTextAndCenterLabel($"Allowed Units\n{allowedUnits}");
			allowedUnitsLabel.Position += new Vector2(-50, 0);

			AddChild(unitSupportCostLabel);
			unitSupportCostLabel.SetPosition(new Vector2(0, 188));
			unitSupportCostLabel.SetTextAndCenterLabel($"Unit Support Cost\n{unitSupportCost} gold/turn");
			unitSupportCostLabel.Position += new Vector2(-50, 0);

			TextureRect advisorHead = new();
			//TODO: Randomize or set logically
			advisorHead.Texture = AdvisorHead.GetPopupImage(AdvisorHead.Advisor.Military, AdvisorHead.Mood.Happy, player.EraIndex());
			advisorHead.SetPosition(new Vector2(851, 0));
			AddChild(advisorHead);
		}

	}

	private void ReturnToMenu() {
		GetParent<Advisors>().Hide();
	}
}
