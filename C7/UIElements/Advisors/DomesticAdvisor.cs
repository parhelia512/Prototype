using C7Engine;
using C7GameData;
using Godot;
using System;

[GlobalClass]
[Tool]
public partial class DomesticAdvisor : Control {
	[Export] TextureRect background;
	[Export] TextureButton close;

	TextureRect scienceSliderIcon = new();
	Label scienceSliderLabel = new();
	TextureRect luxurySliderIcon = new();
	Label luxurySliderLabel = new();

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

		//TODO: Age-based background.  Only use Ancient for now.
		ImageTexture AdvisorHappy = Util.LoadTextureFromPCX("Art/SmallHeads/popupDOMESTIC.pcx", 1, 40, 149, 110);
		ImageTexture AdvisorAngry = Util.LoadTextureFromPCX("Art/SmallHeads/popupDOMESTIC.pcx", 151, 40, 149, 110);
		ImageTexture AdvisorSad = Util.LoadTextureFromPCX("Art/SmallHeads/popupDOMESTIC.pcx", 301, 40, 149, 110);
		ImageTexture AdvisorSurprised = Util.LoadTextureFromPCX("Art/SmallHeads/popupDOMESTIC.pcx", 451, 40, 149, 110);

		TextureRect AdvisorHead = new TextureRect();
		//TODO: Randomize or set logically
		AdvisorHead.Texture = AdvisorSurprised;
		AdvisorHead.SetPosition(new Vector2(851, 0));
		background.AddChild(AdvisorHead);

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

		int scienceRate = 5;
		int luxuryRate = 5;
		using (UIGameDataAccess gameDataAccess = new()) {
			Player player = gameDataAccess.gameData.GetHumanPlayers()[0];
			scienceRate = player.scienceRate;
			luxuryRate = player.luxuryRate;
		}

		scienceSliderIcon.SetPosition(new Vector2(CalculateSliderXPos(scienceRate), scienceSliderY));
		luxurySliderIcon.SetPosition(new Vector2(CalculateSliderXPos(luxuryRate), luxurySliderY));
		scienceSliderLabel.Text = $"{scienceRate * 10}%";
		luxurySliderLabel.Text = $"{luxuryRate * 10}%";
	}

	private int CalculateSliderXPos(int sliderRate) {
		int minX = 570;
		int maxX = 725;

		return minX + (int)((maxX - minX) * (sliderRate / 10.0));
	}
}
