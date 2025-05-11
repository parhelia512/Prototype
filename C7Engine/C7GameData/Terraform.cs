using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace C7GameData;

public static class TerraformRules {
	public static readonly Dictionary<UnitAction, Action<Tile>> OnCompleteActions = new() {
		{UnitAction.BuildMine, tile => tile.overlays.Add(TerrainImprovement.mine)},
		{UnitAction.Irrigate, tile => tile.overlays.Add(TerrainImprovement.irrigation)},
		{UnitAction.BuildRoad, tile => tile.overlays.Add(TerrainImprovement.road)},
		{UnitAction.BuildRailroad, tile => tile.overlays.Add(TerrainImprovement.railroad)},
		{UnitAction.ClearWetlands, tile => tile.ClearTerrainOverlay()},
		// TODO: add bonus shields to the nearest city - should only happen the first time a forest is cleared
		{UnitAction.ClearForest, tile => tile.ClearTerrainOverlay()},
	};

	public static readonly Dictionary<UnitAction, Func<Player, Tile, bool>> ActionValidators = new() {
		{UnitAction.BuildMine, (_, tile) => tile.CanBeMined()},
		{UnitAction.Irrigate, (player, tile) => tile.CanBeIrrigated(player)},
		{UnitAction.BuildRoad, (_, tile) => tile.overlays.CanAdd(TerrainImprovement.road)},
		{UnitAction.ClearWetlands, (_, tile) => tile.overlayTerrainType.allowedWorkerActions.Contains(UnitAction.ClearWetlands)},
		{UnitAction.ClearForest, (_, tile) =>  tile.overlayTerrainType.allowedWorkerActions.Contains(UnitAction.ClearForest)}
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

	public UnitAction Action {
		get => _action;
		set {
			_action = value;

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
	private UnitAction _action;
}
