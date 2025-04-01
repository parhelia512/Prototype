using C7Engine;
using C7GameData;
using Godot;
using System;
using System.Collections.Generic;

public partial class ScienceAdvisor : TextureRect {
	private ImageTexture AncientBackground;
	private ImageTexture MiddleBackground;
	private ImageTexture IndustrialBackground;
	private ImageTexture ModernBackground;
	private TextureButton nextEra;
	private TextureButton previousEra;
	private List<TechBox> techBoxes = new();

	// Stored separately so we can modify this without mutating the player.
	private string eraName;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		this.CreateUI();

		using UIGameDataAccess gameDataAccess = new();
		List<Tech> techs = gameDataAccess.gameData.techs;
		Player player = gameDataAccess.gameData.GetHumanPlayers()[0];
		eraName = player.eraCivilopediaName;
		this.DrawTechTree(eraName, player, techs);
	}

	private void CreateUI() {
		// science_industrial_new is used as the industrial tech tree is
		// different from vanilla civ3.
		AncientBackground = Util.LoadTextureFromPCX("Art/Advisors/science_ancient.pcx");
		MiddleBackground = Util.LoadTextureFromPCX("Art/Advisors/science_middle.pcx");
		IndustrialBackground = Util.LoadTextureFromPCX("Art/Advisors/science_industrial_new.pcx");
		ModernBackground = Util.LoadTextureFromPCX("Art/Advisors/science_modern.pcx");

		// TODO: Age-based background.  Only use Ancient for now.
		// TODO: Consider moving this to an advisor utility, since we're copying
		// these X,Y coordinates in multiple places.
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

		previousEra = new();
		previousEra.TextureNormal = Util.LoadTextureFromPCX("Art/Tech Chooser/scienceNAV.pcx", 0, 1, 129, 33);
		previousEra.TextureHover = Util.LoadTextureFromPCX("Art/Tech Chooser/scienceNAV.pcx", 0, 35, 129, 33);
		previousEra.TexturePressed = Util.LoadTextureFromPCX("Art/Tech Chooser/scienceNAV.pcx", 0, 69, 129, 33);
		previousEra.SetPosition(new Vector2(512 - 128 - 100, 720));
		AddChild(previousEra);
		previousEra.Pressed += () => { ChangeEraAndDrawTree(-1); };

		TextureRect leftArrow = new();
		leftArrow.Texture = Util.LoadTextureFromPCX("Art/Tech Chooser/scienceNAV.pcx", 0, 103, 44, 9);
		previousEra.AddChild(leftArrow);
		leftArrow.SetPosition(new Vector2(-44, 13));

		Label previousEraLabel = new();
		previousEra.AddChild(previousEraLabel);
		previousEraLabel.SetTextAndCenterLabel("Previous Era");
		previousEraLabel.Position += new Vector2(0, 7);

		nextEra = new();
		nextEra.TextureNormal = Util.LoadTextureFromPCX("Art/Tech Chooser/scienceNAV.pcx", 0, 1, 129, 33);
		nextEra.TextureHover = Util.LoadTextureFromPCX("Art/Tech Chooser/scienceNAV.pcx", 0, 35, 129, 33);
		nextEra.TexturePressed = Util.LoadTextureFromPCX("Art/Tech Chooser/scienceNAV.pcx", 0, 69, 129, 33);
		nextEra.SetPosition(new Vector2(512 + 100, 720));
		AddChild(nextEra);
		nextEra.Pressed += () => { ChangeEraAndDrawTree(1); };

		TextureRect rightArrow = new();
		rightArrow.Texture = Util.LoadTextureFromPCX("Art/Tech Chooser/scienceNAV.pcx", 46, 103, 44, 9);
		nextEra.AddChild(rightArrow);
		rightArrow.SetPosition(new Vector2(129, 13));

		Label nextEraLabel = new();
		nextEra.AddChild(nextEraLabel);
		nextEraLabel.SetTextAndCenterLabel("Next Era");
		nextEraLabel.Position += new Vector2(0, 7);
	}

	void DrawTechTree(string eraName, Player player, List<Tech> allTechs) {
		HashSet<ID> knownTechs = player.knownTechs;
		previousEra.Show();
		nextEra.Show();

		// Set the tech background based on the player's era.
		if (eraName == "ERAS_Ancient_Times") {
			previousEra.Hide();
			this.Texture = AncientBackground;
		} else if (eraName == "ERAS_Middle_Ages") {
			this.Texture = MiddleBackground;
		} else if (eraName == "ERAS_Industrial_Age") {
			this.Texture = IndustrialBackground;
		} else if (eraName == "ERAS_Modern_Era") {
			this.Texture = ModernBackground;
			nextEra.Hide();
		}

		foreach (Tech tech in allTechs) {
			if (tech.EraCivilopediaName != eraName) {
				continue;
			}

			TechBox.TechState techState = TechBox.TechState.kBlocked;
			if (knownTechs.Contains(tech.id)) {
				techState = TechBox.TechState.kKnown;
			} else if (player.currentlyResearchedTech == tech.id) {
				techState = TechBox.TechState.kInProgress;
			} else {
				bool prereqsKnown = true;
				foreach (Tech prereq in tech.Prerequisites) {
					if (!knownTechs.Contains(prereq.id)) {
						prereqsKnown = false;
						break;
					}
				}
				techState = prereqsKnown ? TechBox.TechState.kPossible : TechBox.TechState.kBlocked;
			}

			TechBox techButton = new(tech, techState);
			techButton.SetPosition(new Vector2(tech.X, tech.Y));
			techButton.Pressed += () => {
				new MsgChooseResearch(tech.id).send();
			};
			AddChild(techButton);
			techBoxes.Add(techButton);
		}
	}

	private void ChangeEraAndDrawTree(int delta) {
		foreach (TechBox tb in techBoxes) {
			RemoveChild(tb);
			tb.QueueFree();
		}
		techBoxes.Clear();

		using UIGameDataAccess gameDataAccess = new();
		List<Tech> techs = gameDataAccess.gameData.techs;
		Player player = gameDataAccess.gameData.GetHumanPlayers()[0];
		eraName = Player.EraIndexToEra(Player.EraIndex(eraName) + delta);
		DrawTechTree(eraName, player, techs);
	}

	private void ReturnToMenu() {
		GetParent<Advisors>().Hide();
	}
}
