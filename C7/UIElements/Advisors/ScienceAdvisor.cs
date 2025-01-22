using C7Engine;
using C7GameData;
using Godot;
using System;
using System.Collections.Generic;

public partial class ScienceAdvisor : TextureRect {
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		this.CreateUI();
	}

	private void CreateUI() {
		// TODO: support other eras. Amusingly the dependency arrows are drawn
		// on this texture, so to render prereqs we need to update this per era.
		ImageTexture DomesticBackground = Util.LoadTextureFromPCX("Art/Advisors/science_ancient.pcx");
		this.Texture = DomesticBackground;

		// TODO: Age-based background.  Only use Ancient for now.
		// TODO: Consider moving this to an advisor utility, since we're copying
		// these x,y coordinates in multiple places.
		ImageTexture AdvisorHappy = Util.LoadTextureFromPCX("Art/SmallHeads/popupSCIENCE.pcx", 1, 40, 149, 110);
		ImageTexture AdvisorAngry = Util.LoadTextureFromPCX("Art/SmallHeads/popupSCIENCE.pcx", 151, 40, 149, 110);
		ImageTexture AdvisorSad = Util.LoadTextureFromPCX("Art/SmallHeads/popupSCIENCE.pcx", 301, 40, 149, 110);
		ImageTexture AdvisorSurprised = Util.LoadTextureFromPCX("Art/SmallHeads/popupSCIENCE.pcx", 451, 40, 149, 110);

		TextureRect AdvisorHead = new();
		//TODO: Randomize or set logically
		AdvisorHead.Texture = AdvisorSurprised;
		AdvisorHead.SetPosition(new Vector2(851, 0));
		AddChild(AdvisorHead);

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

		// TODO: Figure out how to pick which of the different sized tech boxes
		// we should use for a given tech.
		//
		// NOTE: this pcx has 4 columns (discovered, planned, possible,
		// not yet researchable), and 16 rows, 4 per era, with different sizes.
		ImageTexture techBox = Util.LoadTextureFromPCX("Art/Advisors/techboxes.pcx", 1, 1, 180, 80);
		using (UIGameDataAccess gameDataAccess = new()) {
			List<Tech> techs = gameDataAccess.gameData.techs;

			foreach (Tech tech in techs) {
				// TODO: handle other eras.
				if (tech.Era != "Ancient Times") {
					continue;
				}

				// TODO: draw the icon, name, etc. Consider sticking that in
				// its own class.
				TextureButton techButton = new();
				techButton.TextureNormal = techBox;
				techButton.SetPosition(new Vector2(tech.X, tech.Y));
				AddChild(techButton);
			}
		}
	}

	private void ReturnToMenu() {
		GetParent<Advisors>().Hide();
	}
}
