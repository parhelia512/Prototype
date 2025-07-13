using Godot;
using System.Collections.Generic;
using C7GameData;
using Serilog;

/*
 UnitButtons contains the buttons at the bottom of the game UI when viewing the
 map, and control unit actions. Clicking buttons triggers Input.ActionPress
 calls which are checked and handled in Game.processActions.
*/

public partial class UnitButtons : VBoxContainer {
	[Signal] public delegate void ActionRequestedEventHandler(string action);

	private ILogger log = LogManager.ForContext<UnitButtons>();

	private Dictionary<string, TextureButton> buttonMap = new();

	[Export]
	HBoxContainer primaryControls;

	[Export]
	HBoxContainer specializedControls;

	[Export]
	HBoxContainer advancedControls;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		// You can hide buttons like this.  This will come in handy later!
		// Remember to re-calc the margin after hiding/unhiding buttons, as that may affect the width.
		// this.GetNode<FortifyButton>("PrimaryUnitControls/FortifyButton").Hide();

		AddNewButton(primaryControls, C7Action.UnitHold);
		AddNewButton(primaryControls, C7Action.UnitWait);
		AddNewButton(primaryControls, C7Action.UnitFortify);
		AddNewButton(primaryControls, C7Action.UnitDisband);
		AddNewButton(primaryControls, C7Action.UnitGoto);
		AddNewButton(primaryControls, C7Action.UnitExplore);
		// AddNewButton(primaryControls, C7Action.UnitSentry);
		// AddNewButton(primaryControls, C7Action.UnitSentryEnemyOnly);

		//   ******* SPECIALIZED CONTROLS *************
		// AddNewButton(specializedControls, "load");
		// AddNewButton(specializedControls, "unload");
		// AddNewButton(specializedControls, "pillage");
		// AddNewButton(specializedControls, "bombard");
		// AddNewButton(specializedControls, "autobombard");
		// AddNewButton(specializedControls, "paradrop");
		//superfortify?
		// AddNewButton(specializedControls, "hurryBuilding");
		// AddNewButton(specializedControls, "upgrade");

		//TODO: The first two buttons in row index 2, and validate science age/colony are correct
		// AddNewButton(specializedControls, "sacrifice");
		// AddNewButton(specializedControls, "scienceAge");  //validate
		// AddNewButton(specializedControls, "buildColony"); //validate
		AddNewButton(specializedControls, C7Action.UnitBuildCity);
		AddNewButton(specializedControls, C7Action.UnitBuildRoad);
		AddNewButton(specializedControls, C7Action.UnitBuildRailroad);

		// AddNewButton(specializedControls, C7Action.UnitBuildFortress);
		// AddNewButton(specializedControls, C7Action.UnitBuildBarricade);
		AddNewButton(specializedControls, C7Action.UnitBuildMine);
		AddNewButton(specializedControls, C7Action.UnitIrrigate);
		AddNewButton(specializedControls, C7Action.UnitClearForest);
		AddNewButton(specializedControls, C7Action.UnitClearWetlands);
		// AddNewButton(specializedControls, "plantForest");
		// AddNewButton(specializedControls, "clearDamage");
		AddNewButton(specializedControls, C7Action.UnitAutomate);

		// Row index 4 and later not yet added
	}

	private void AddNewButton(HBoxContainer row, string action) {
		TextureButton button = new();
		TextureLoader.SetButtonTextures(button, "ui.unit_control." + action);
		button.Pressed += () => { EmitSignal(SignalName.ActionRequested, action); };

		row.AddChild(button);
		buttonMap[action] = button;
	}

	private void OnNoMoreAutoselectableUnits() {
		this.Visible = false;
	}

	private void OnNewUnitSelected(ParameterWrapper<MapUnit> wrappedMapUnit) {
		MapUnit unit = wrappedMapUnit.Value;
		foreach (TextureButton button in buttonMap.Values) {
			button.Visible = false;
		}

		// Mark all the buttons corresponding to the unit's available actions
		// as visible. We do this rather than using the unit prototype's actions
		// so that we don't display buttons that do nothing - we don't want to
		// show the "road" button if we can't build a road, etc.
		foreach (UnitAction action in unit.availableActions) {
			string actionKey = C7Action.ToActionString(action);
			if (buttonMap.ContainsKey(actionKey)) {
				buttonMap[actionKey].Visible = true;
			} else {
				log.Warning("Could not find button " + action);
			}
		}

		this.Visible = true;
	}
}
