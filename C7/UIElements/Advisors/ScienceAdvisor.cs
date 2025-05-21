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
	private TextureRect advisorHead = new();

	// Stored separately so we can modify this without mutating the player.
	private string eraName;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		this.CreateUI();

		using UIGameDataAccess gameDataAccess = new();
		List<Tech> techs = gameDataAccess.gameData.techs;
		Player player = gameDataAccess.gameData.GetHumanPlayers()[0];
		eraName = player.eraCivilopediaName;
		this.DrawTechTree(eraName, player, techs, player.GetAvailableTechsToResearch(gameDataAccess.gameData));
	}

	private void CreateUI() {
		// science_industrial_new is used as the industrial tech tree is
		// different from vanilla civ3.
		AncientBackground = TextureLoader.Load("advisors.science.background.ancient");
		MiddleBackground = TextureLoader.Load("advisors.science.background.middle");
		IndustrialBackground = TextureLoader.Load("advisors.science.background.industrial");
		ModernBackground = TextureLoader.Load("advisors.science.background.modern");

		advisorHead.Texture = AdvisorHead.GetPopupImage(AdvisorHead.Advisor.Science, AdvisorHead.Mood.Happy, eraIndex: 0);
		advisorHead.SetPosition(new Vector2(851, 0));
		AddChild(advisorHead);

		ImageTexture DialogBoxTexture = TextureLoader.Load("advisors.dialog_box");
		TextureButton DialogBox = new TextureButton();
		DialogBox.TextureNormal = DialogBoxTexture;
		DialogBox.SetPosition(new Vector2(806, 110));
		AddChild(DialogBox);

		//TODO: Multi-line capabilities
		Label DialogBoxAdvise = new Label();
		DialogBoxAdvise.Text = "You are running C7!";
		DialogBoxAdvise.SetPosition(new Vector2(815, 119));
		AddChild(DialogBoxAdvise);

		ImageTexture GoBackTexture = TextureLoader.Load("ui.exit.normal");
		TextureButton GoBackButton = new TextureButton();
		GoBackButton.TextureNormal = GoBackTexture;
		GoBackButton.SetPosition(new Vector2(952, 720));
		AddChild(GoBackButton);
		GoBackButton.Pressed += ReturnToMenu;

		previousEra = new();
		TextureLoader.SetButtonTextures(previousEra, "advisors.science.navigation.button");
		previousEra.SetPosition(new Vector2(512 - 128 - 100, 720));
		AddChild(previousEra);
		previousEra.Pressed += () => { ChangeEraAndDrawTree(-1); };

		TextureRect leftArrow = new() {
			Texture = TextureLoader.Load("advisors.science.navigation.arrow_previous")
		};
		previousEra.AddChild(leftArrow);
		leftArrow.SetPosition(new Vector2(-44, 13));

		Label previousEraLabel = new();
		previousEra.AddChild(previousEraLabel);
		previousEraLabel.SetTextAndCenterLabel("Previous Era");
		previousEraLabel.Position += new Vector2(0, 7);

		nextEra = new();
		TextureLoader.SetButtonTextures(nextEra, "advisors.science.navigation.button");
		nextEra.SetPosition(new Vector2(512 + 100, 720));
		AddChild(nextEra);
		nextEra.Pressed += () => { ChangeEraAndDrawTree(1); };

		TextureRect rightArrow = new() {
			Texture = TextureLoader.Load("advisors.science.navigation.arrow_next")
		};
		nextEra.AddChild(rightArrow);
		rightArrow.SetPosition(new Vector2(129, 13));

		Label nextEraLabel = new();
		nextEra.AddChild(nextEraLabel);
		nextEraLabel.SetTextAndCenterLabel("Next Era");
		nextEraLabel.Position += new Vector2(0, 7);
	}

	void DrawTechTree(string eraName, Player player, List<Tech> allTechs, HashSet<Tech> availableTechsToResearch) {
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
		advisorHead.Texture = AdvisorHead.GetPopupImage(AdvisorHead.Advisor.Science, AdvisorHead.Mood.Happy, player.EraIndex());

		foreach (Tech tech in allTechs) {
			if (tech.EraCivilopediaName != eraName) {
				continue;
			}

			TechBox.TechState techState = TechBox.TechState.kBlocked;
			if (knownTechs.Contains(tech.id)) {
				techState = TechBox.TechState.kKnown;
			} else if (player.currentlyResearchedTech == tech.id) {
				techState = TechBox.TechState.kInProgress;
			} else if (availableTechsToResearch.Contains(tech)) {
				techState = TechBox.TechState.kPossible;
			} else {
				techState = TechBox.TechState.kBlocked;
			}

			TechBox techButton = new(tech, techState);
			techButton.SetPosition(new Vector2(tech.X, tech.Y));
			techButton.Pressed += () => {
				new MsgChooseResearch(tech.id, showAdvisor: true).send();
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
		DrawTechTree(eraName, player, techs, player.GetAvailableTechsToResearch(gameDataAccess.gameData));
	}

	private void ReturnToMenu() {
		GetParent<Advisors>().Hide();
	}
}
