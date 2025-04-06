using Godot;
using System;
using C7GameData;
using Serilog;

public partial class GameStatus : Control {

	private ILogger log = LogManager.ForContext<GameStatus>();

	[Export] LowerRightInfoBox lowerRightInfoBox;

	[Signal] public delegate void BlinkyEndTurnButtonPressedEventHandler();

	public void OnNewUnitSelected(ParameterWrapper<MapUnit> wrappedMapUnit) {
		MapUnit newUnit = wrappedMapUnit.Value;
		log.Information("Selected unit: " + newUnit + " at " + newUnit.location);
		lowerRightInfoBox.UpdateUnitInfo(newUnit, newUnit.location.overlayTerrainType);
	}

	private void OnTurnEnded() {
		lowerRightInfoBox.StopToggling();
	}

	private void OnNoMoreAutoselectableUnits() {
		lowerRightInfoBox.SetEndOfTurnStatus();
	}
}
