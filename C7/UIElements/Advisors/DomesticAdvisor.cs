using C7Engine;
using C7GameData;
using Godot;
using System;

[GlobalClass]
[Tool]
public partial class DomesticAdvisor : Control {
	[Export] TextureRect background;
	[Export] TextureButton close;
	[Export] TextureButton changeGovernment;
	[Export] Label governmentLabel;
	[Export] Label scienceStatus;
	[Export] Label treasury;
	[Export] Label incomeDetails;
	[Export] Label expenseDetails;
	[Export] Label incomeSummary;
	[Export] Label expenseSummary;
	[Export] Label sumSummary;
	[Export] Label growth;

	TextureRect scienceSliderIcon = new();
	Label scienceSliderLabel = new();
	TextureRect luxurySliderIcon = new();
	Label luxurySliderLabel = new();

	TextureRect advisorHead = new();

	// The Y position of the two sliders.
	private int scienceSliderY = 84;
	private int luxurySliderY = 130;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		this.CreateUI();
	}

	private void CreateUI() {
		ImageTexture DomesticBackground = Util.LoadTextureFromPCX("Art/Advisors/domestic.pcx");
		background.Texture = DomesticBackground;

		advisorHead.Texture = AdvisorHead.GetPopupImage(AdvisorHead.Advisor.Domestic, AdvisorHead.Mood.Happy, /*eraIndex=*/0);
		advisorHead.SetPosition(new Vector2(851, 0));
		background.AddChild(advisorHead);

		ImageTexture DialogBoxTexture = Util.LoadTextureFromPCX("Art/Advisors/dialogbox.pcx");
		TextureButton DialogBox = new TextureButton();
		DialogBox.TextureNormal = DialogBoxTexture;
		DialogBox.SetPosition(new Vector2(806, 110));
		background.AddChild(DialogBox);

		//TODO: Multi-line capabilities
		Label DialogBoxAdvise = new Label();
		DialogBoxAdvise.Text = "You are running C7!";
		DialogBoxAdvise.SetPosition(new Vector2(815, 119));
		background.AddChild(DialogBoxAdvise);

		close.TextureNormal = Util.LoadTextureFromPCX("Art/exitBox-backgroundStates.pcx", 0, 0, 72, 48);
		close.TextureHover = Util.LoadTextureFromPCX("Art/exitBox-backgroundStates.pcx", 72, 0, 72, 48);
		close.TexturePressed = Util.LoadTextureFromPCX("Art/exitBox-backgroundStates.pcx", 144, 0, 72, 48);
		close.Pressed += Hide;

		changeGovernment.TextureNormal = Util.LoadTextureFromPCX("Art/Advisors/domesticBUTTON.pcx", 1, 1, 145, 24);
		changeGovernment.TextureHover = Util.LoadTextureFromPCX("Art/Advisors/domesticBUTTON.pcx", 1, 26, 145, 24);
		changeGovernment.TexturePressed = Util.LoadTextureFromPCX("Art/Advisors/domesticBUTTON.pcx", 1, 52, 145, 24);
		// TODO: implement changing governments

		ImageTexture scienceSliderTexture = Util.LoadTextureFromPCX("Art/city screen/CityIcons.pcx", 34, 2, 30, 30);
		ImageTexture luxurySliderTexture = Util.LoadTextureFromPCX("Art/city screen/CityIcons.pcx", 376, 2, 30, 30);

		// Placeholder values
		int scienceRate = 5;
		int luxuryRate = 5;

		scienceSliderIcon.Texture = scienceSliderTexture;
		scienceSliderIcon.SetPosition(new Vector2(CalculateSliderXPos(scienceRate), scienceSliderY));
		background.AddChild(scienceSliderIcon);

		luxurySliderIcon.Texture = luxurySliderTexture;
		luxurySliderIcon.SetPosition(new Vector2(CalculateSliderXPos(luxuryRate), luxurySliderY));
		background.AddChild(luxurySliderIcon);

		scienceSliderLabel.Text = $"{scienceRate * 10}%";
		scienceSliderLabel.SetPosition(new Vector2(760, scienceSliderY + 6));
		background.AddChild(scienceSliderLabel);

		luxurySliderLabel.Text = $"{luxuryRate * 10}%";
		luxurySliderLabel.SetPosition(new Vector2(760, luxurySliderY + 4));
		background.AddChild(luxurySliderLabel);

		ImageTexture plusTexture = Util.LoadTextureFromPCX("Art/Advisors/domestic_icons_aux.pcx", 75, 1, 22, 22);
		ImageTexture minusTexture = Util.LoadTextureFromPCX("Art/Advisors/domestic_icons_aux.pcx", 51, 1, 22, 22);

		TextureButton moreScience = new();
		moreScience.TextureNormal = plusTexture;
		moreScience.SetPosition(new Vector2(725, scienceSliderY + 25));
		moreScience.Pressed += () => { new MsgChangeSliders(true, false, false, false).send(); };
		background.AddChild(moreScience);

		TextureButton lessScience = new();
		lessScience.TextureNormal = minusTexture;
		lessScience.SetPosition(new Vector2(575, scienceSliderY + 25));
		lessScience.Pressed += () => { new MsgChangeSliders(false, true, false, false).send(); };
		background.AddChild(lessScience);

		TextureButton moreLuxury = new();
		moreLuxury.TextureNormal = plusTexture;
		moreLuxury.SetPosition(new Vector2(725, luxurySliderY - 5));
		moreLuxury.Pressed += () => { new MsgChangeSliders(false, false, true, false).send(); };
		background.AddChild(moreLuxury);

		TextureButton lessLuxury = new();
		lessLuxury.TextureNormal = minusTexture;
		lessLuxury.SetPosition(new Vector2(575, luxurySliderY - 5));
		lessLuxury.Pressed += () => { new MsgChangeSliders(false, false, false, true).send(); };
		background.AddChild(lessLuxury);
	}

	public void ShowAdvisor() {
		Show();

		using UIGameDataAccess gameDataAccess = new();
		Player player = gameDataAccess.gameData.GetHumanPlayers()[0];

		int scienceRate = player.scienceRate;
		int luxuryRate = player.luxuryRate;

		scienceSliderIcon.SetPosition(new Vector2(CalculateSliderXPos(scienceRate), scienceSliderY));
		luxurySliderIcon.SetPosition(new Vector2(CalculateSliderXPos(luxuryRate), luxurySliderY));
		scienceSliderLabel.Text = $"{scienceRate * 10}%";
		luxurySliderLabel.Text = $"{luxuryRate * 10}%";

		governmentLabel.Text = $"{player.government.name}";
		scienceStatus.Text = player.SummarizeScience(gameDataAccess.gameData);
		treasury.Text = $"Treasury: {player.gold}";

		// TODO: fill these in.
		incomeDetails.Text = "From cities: +??\nFrom taxmen: +??\nFrom other civs: +??\nFrom interest: +??";
		expenseDetails.Text = "-??: Science\n-??: Entertainment\n-??: Corruption\n-??: Maintenance\n-??: Unit costs\n-??: To other civs";
		incomeSummary.Text = "??";
		expenseSummary.Text = "??";

		int goldPerTurn = player.CalculateGoldPerTurn();
		if (goldPerTurn > 0) {
			sumSummary.Text = $"Net gain: {goldPerTurn}";
			growth.Text = "Growing!";
		} else if (goldPerTurn < 0) {
			sumSummary.Text = $"Net loss: {goldPerTurn}";
			growth.Text = "Shrinking!";
		} else {
			sumSummary.Text = $"Neutral: {goldPerTurn}";
			growth.Text = "Balanced";
		}

		//TODO: Randomize or set logically
		advisorHead.Texture = AdvisorHead.GetPopupImage(AdvisorHead.Advisor.Domestic, AdvisorHead.Mood.Happy, player.EraIndex());
	}

	private int CalculateSliderXPos(int sliderRate) {
		int minX = 570;
		int maxX = 725;

		return minX + (int)((maxX - minX) * (sliderRate / 10.0));
	}
}
