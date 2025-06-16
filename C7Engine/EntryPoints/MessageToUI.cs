namespace C7Engine {
	using System.Threading;
	using C7GameData;
	using System;

	public class MessageToUI {
		public void send() {
			EngineStorage.messagesToUI.Enqueue(this);
		}
	}

	public class MsgStartUnitAnimation : MessageToUI {
		public ID unitID;
		public MapUnit.AnimatedAction action;
		public Action completionEvent;
		public AnimationEnding ending;

		public MsgStartUnitAnimation(MapUnit unit, MapUnit.AnimatedAction action, Action completionEvent, AnimationEnding ending) {
			this.unitID = unit.id;
			this.action = action;
			this.completionEvent = completionEvent;
			this.ending = ending;
		}
	}

	public class MsgStartEffectAnimation : MessageToUI {
		public int tileIndex;
		public AnimatedEffect effect;
		public Action completionEvent;
		public AnimationEnding ending;

		public MsgStartEffectAnimation(Tile tile, AnimatedEffect effect, Action completionEvent, AnimationEnding ending) {
			this.tileIndex = EngineStorage.gameData.map.tileCoordsToIndex(tile.XCoordinate, tile.YCoordinate);
			this.effect = effect;
			this.completionEvent = completionEvent;
			this.ending = ending;
		}
	}

	public class MsgStartTurn : MessageToUI { }

	public class MsgUpdateUiAfterMove : MessageToUI { }

	public class MsgShowScienceAdvisor : MessageToUI { }

	public class MsgUpdateUiAfterDomesticChange : MessageToUI { }

	public class MsgWarDeclaration : MessageToUI {
		public Player aggressor;
		public Player opponent;

		public MsgWarDeclaration(Player aggressor, Player opponent) {
			this.aggressor = aggressor;
			this.opponent = opponent;
		}
	}

	public class MsgCityDestroyed : MessageToUI {
		public City city;

		public MsgCityDestroyed(City city) {
			this.city = city;
		}
	}

	public class MsgCivilizationDestroyed : MessageToUI {
		public Civilization civilization;

		public MsgCivilizationDestroyed(Civilization civ) {
			this.civilization = civ;
		}
	}

	public class MsgCityCreated : MessageToUI {
		public City city;

		public MsgCityCreated(City city) {
			this.city = city;
		}
	}

	public class MsgDisplayHurryProductionPopup : MessageToUI {
		public City city;
		public City.HurryProductionDetails details;

		public MsgDisplayHurryProductionPopup(City c, City.HurryProductionDetails d) {
			city = c;
			details = d;
		}
	}

	public class MsgShowCityScreen : MessageToUI {
		public City city;

		public MsgShowCityScreen(City city) {
			this.city = city;
		}
	}

	public class MsgShowMilitaryAdvisorPopup : MessageToUI {
		public string message;
		public bool happy;
		public MsgShowMilitaryAdvisorPopup(string message, bool happy) {
			this.message = message;
			this.happy = happy;
		}
	}

	public class MsgShowTemporaryPopup : MessageToUI {
		public string message;
		public Tile location;

		public MsgShowTemporaryPopup(string message, Tile location) {
			this.message = message;
			this.location = location;
		}
	}

	public class MsgShowTradeOffer : MessageToUI {
		public Player aiPlayer;
		public Player humanPlayer;
		public TradeOffer aiWant;
		public TradeOffer aiGive;

		public MsgShowTradeOffer(Player aiPlayer, Player humanPlayer, TradeOffer aiWant, TradeOffer aiGive) {
			this.aiPlayer = aiPlayer;
			this.humanPlayer = humanPlayer;
			this.aiWant = aiWant;
			this.aiGive = aiGive;
		}
	}
}
