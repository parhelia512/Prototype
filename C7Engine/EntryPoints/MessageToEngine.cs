using Serilog;

namespace C7Engine {
	using System;
	using C7GameData;

	public abstract class MessageToEngine {
		public abstract void process();

		public void send() {
			EngineStorage.pendingMessages.Enqueue(this);
			EngineStorage.actionAddedToQueue.Set();
		}
	}

	public class MsgShutdownEngine : MessageToEngine {
		private ILogger log = Log.ForContext<MsgShutdownEngine>();

		public override void process() {
			log.Information("Engine received shutdown message.");
		}
	}

	public class MsgSetFortification : MessageToEngine {
		private ID unitID;
		private bool fortifyElseWake;

		public MsgSetFortification(ID unitID, bool fortifyElseWake) {
			this.unitID = unitID;
			this.fortifyElseWake = fortifyElseWake;
		}

		public override void process() {
			MapUnit unit = EngineStorage.gameData.GetUnit(unitID);

			// Simply do nothing if we weren't given a valid GUID. TODO: Maybe this is an error we need to handle? In an MP game, we should reject
			// invalid actions at the server level but at the client level an invalid action received from the server indicates a desync.
			if (unit != null) {
				if (fortifyElseWake)
					unit.fortify();
				else
					unit.wake();
			}
		}
	}

	public class MsgMoveUnit : MessageToEngine {
		private ID unitID;
		private TileDirection dir;

		public MsgMoveUnit(ID unitID, TileDirection dir) {
			this.unitID = unitID;
			this.dir = dir;
		}

		public override void process() {
			MapUnit unit = EngineStorage.gameData.GetUnit(unitID);
			unit?.move(dir);

			// The unit moved to a new tile - if it still has movement points,
			// update the UI to reflect this new position and movement points.
			if (unit?.movementPoints.canMove == true) {
				new MsgUpdateUiAfterMove().send();
			}
		}
	}

	public class MsgSetUnitPath : MessageToEngine {
		private ID unitID;
		private TilePath path;

		public MsgSetUnitPath(ID unitID, TilePath path) {
			this.unitID = unitID;
			this.path = path;
		}

		public override void process() {
			MapUnit unit = EngineStorage.gameData.GetUnit(unitID);
			unit?.setUnitPath(path);

			// The unit moved to a new tile - if it still has movement points,
			// update the UI to reflect this new position and movement points.
			if (unit?.movementPoints.canMove == true) {
				new MsgUpdateUiAfterMove().send();
			}
		}
	}

	// A generic class that allows the UI to have the game engine run some
	// action, assumed to be on a unit.
	//
	// Actions that require more than a 1 or 2 line lambda should probably use
	// a custom subclass.
	public class ActionToEngineMsg : MessageToEngine {
		private Action action;
		public ActionToEngineMsg(Action action) {
			this.action = action;
		}

		public override void process() {
			action();
		}
	}

	// Switches the player government to anarchy and determines when the player
	// can exit anarchy.
	public class StartGovernmentTransitionMsg : MessageToEngine {
		private Player player;

		public StartGovernmentTransitionMsg(Player p) {
			player = p;
		}

		public override void process() {
			GameData gD = EngineStorage.gameData;
			Government transitionGovt = gD.governments.Find(x => x.transitionType);
			player.government = transitionGovt;
			player.inAnarchyUntilTurn = gD.turn + player.GetTurnsOfAnarchyForTransition(gD);

			// Update the domestic advisor once we know how long the anarchy is.
			new MsgUpdateUiAfterDomesticChange().send();
		}
	}

	public class SelectGovernmentMsg : MessageToEngine {
		private Player player;
		private Government government;

		public SelectGovernmentMsg(Player player, Government government) {
			this.player = player;
			this.government = government;
		}

		public override void process() {
			player.government = government;
		}
	}

	// A Class that allows the UI to have the game engine run some
	// terraform action.
	public class MsgStartWorkerJob : MessageToEngine {
		private ID UnitID;
		private Terraform Action;
		public MsgStartWorkerJob(ID unitID, Terraform action) {
			this.UnitID = unitID;
			this.Action = action;
		}

		public override void process() {
			MapUnit unit = EngineStorage.gameData.GetUnit(UnitID);
			unit?.PerformTerraformAction(Action);
		}
	}

	public class MsgChooseProduction : MessageToEngine {
		private ID cityID;
		private string producibleName;

		public MsgChooseProduction(ID cityID, string producibleName) {
			this.cityID = cityID;
			this.producibleName = producibleName;
		}

		public override void process() {
			City city = EngineStorage.gameData.cities.Find(c => c.id == cityID);
			if (city != null) {
				foreach (IProducible producible in city.ListProductionOptions()) {
					if (producible.name == producibleName) {
						city.SetItemBeingProduced(producible);
						break;
					}
				}
			}
		}
	}

	public class MsgChooseResearch : MessageToEngine {
		private ID techId;
		private bool showAdvisor;
		public MsgChooseResearch(ID techId, bool showAdvisor) {
			this.techId = techId;
			this.showAdvisor = showAdvisor;
		}

		public override void process() {
			Player player = EngineStorage.gameData.GetHumanPlayers()[0];
			if (player.currentlyResearchedTech == techId) {
				return;
			}
			Tech requestedTech = EngineStorage.gameData.techs.Find(t => t.id == techId);

			// Ensure this is an eligible tech to research.
			//
			// TODO: do a topological sort to allow a queue of techs to study.
			foreach (Tech prereq in requestedTech.Prerequisites) {
				if (!player.knownTechs.Contains(prereq.id)) {
					return;
				}
			}

			// Start researching this tech and update the UI.
			player.SetCurrentlyResearchedTech(requestedTech.id);
			if (showAdvisor) {
				new MsgShowScienceAdvisor().send();
			}
		}
	}

	public class MsgChangeSliders : MessageToEngine {
		private bool moreScience;
		private bool lessScience;
		private bool moreLuxury;
		private bool lessLuxury;

		public MsgChangeSliders(bool moreScience, bool lessScience, bool moreLuxury, bool lessLuxury) {
			this.moreScience = moreScience;
			this.lessScience = lessScience;
			this.moreLuxury = moreLuxury;
			this.lessLuxury = lessLuxury;
		}

		public override void process() {
			Player player = EngineStorage.gameData.GetHumanPlayers()[0];

			if (moreScience && player.scienceRate == 10 || lessScience && player.scienceRate == 0) {
				return;
			}
			if (moreLuxury && player.luxuryRate == 10 || lessLuxury && player.luxuryRate == 0) {
				return;
			}

			// Increase our science rate, taking away from tax rate if we can,
			// otherwise decrease the luxury rate.
			if (moreScience) {
				player.scienceRate++;
				if (player.taxRate > 0) {
					player.taxRate--;
				} else {
					player.luxuryRate--;
				}
			}

			// Ditto for luxury.
			if (moreLuxury) {
				player.luxuryRate++;
				if (player.taxRate > 0) {
					player.taxRate--;
				} else {
					player.scienceRate--;
				}
			}

			// Decreasing is easier, we decrease the requested slider and bump
			// up the tax rate.
			if (lessScience) {
				player.scienceRate--;
				player.taxRate++;
			}

			if (lessLuxury) {
				player.luxuryRate--;
				player.taxRate++;
			}

			// Update citizen moods in all cities, as changing the sliders can
			// change moods.
			foreach (City city in player.cities) {
				city.RecalculateCitizenMoods(EngineStorage.gameData);
			}

			// Update the ui to reflect our changes.
			new MsgUpdateUiAfterDomesticChange().send();
		}
	}

	public class MsgEndTurn : MessageToEngine {
		private ILogger log = Log.ForContext<MsgEndTurn>();

		public override void process() {
			Player controller = EngineStorage.gameData.GetPlayer(EngineStorage.uiControllerID);

			// Reorder the unit list so that non-busy units will be selected
			// first.
			controller.units.Sort((x, y) => x.IsBusy().CompareTo(y.IsBusy()));

			controller.hasPlayedThisTurn = true;
			TurnHandling.AdvanceTurn();
		}
	}

	public class MsgPerformUnitAction : MessageToEngine {
		private MapUnit unit;
		public MsgPerformUnitAction(MapUnit unit) {
			this.unit = unit;
		}

		public override void process() {
			unit.PerformBusyAction();
		}
	}

	public class MsgDoHurryProduction : MessageToEngine {
		private City city;
		public MsgDoHurryProduction(City c) {
			city = c;
		}

		public override void process() {
			city.HurryProduction();
		}
	}

	public class MsgSetAnimationsEnabled : MessageToEngine {
		private bool enabled;

		public MsgSetAnimationsEnabled(bool enabled) {
			this.enabled = enabled;
		}

		public override void process() {
			EngineStorage.animationsEnabled = enabled;
		}
	}
}
