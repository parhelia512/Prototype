using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace C7GameData;

public static class TerraformRules {
	public static readonly Dictionary<string, Action<Tile>> OnCompleteActions = new() {
		{C7Action.UnitBuildMine, tile => tile.overlays.mine = true},
		{C7Action.UnitIrrigate, tile => tile.overlays.irrigation = true},
		{C7Action.UnitBuildRoad, tile => tile.overlays.road = true},
		{C7Action.UnitClearWetlands, tile => tile.ClearTerrainOverlay()},
		// TODO: add bonus shields to the nearest city - should only happen the first time a forest is cleared
		{C7Action.UnitClearForest, tile => tile.ClearTerrainOverlay()},
	};

	public static readonly Dictionary<string, Func<Player, Tile, bool>> ActionValidators = new() {
		{C7Action.UnitBuildMine, (_, tile) => tile.CanBeMined()},
		{C7Action.UnitIrrigate, (player, tile) => tile.CanBeIrrigated(player)},
		{C7Action.UnitBuildRoad, (_, tile) => tile.CanBeRoaded()},
		{C7Action.UnitClearWetlands, (_, tile) => tile.overlayTerrainType.allowedWorkerActions.Contains(C7Action.UnitClearWetlands)},
		{C7Action.UnitClearForest, (_, tile) =>  tile.overlayTerrainType.allowedWorkerActions.Contains(C7Action.UnitClearForest)}
	};
}

public class Terraform {
	public ID Id;

	public string Name;

	public string CivilopediaEntry;

	public int TurnsToComplete;

	public ID RequiredTech;

	public List<ID> RequiredResources = new();

	[JsonIgnore]
	public Action<Tile> OnComplete;

	[JsonIgnore]
	public Func<Player, Tile, bool> MeetsRequirements;

	public string Action {
		get => _action;
		set {
			_action = value;

			if (value == null) {
				return;
			}

			if (TerraformRules.OnCompleteActions.TryGetValue(value, out var onComplete)) {
				OnComplete = onComplete;
			}

			if (TerraformRules.ActionValidators.TryGetValue(value, out var prerequisite)) {
				MeetsRequirements = prerequisite;
			} else {
				MeetsRequirements = (_, _) => false;
			}
		}
	}
	private string _action;
}
