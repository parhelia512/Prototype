using System;
using System.Collections.Generic;
using System.Linq;
using C7GameData.Save;

namespace C7GameData;

public static class TerraformRules {
	public static readonly Dictionary<UnitAction, Action<Player, Tile>> ActionEffects = new() {
		{UnitAction.BuildMine, (_, tile) => tile.overlays.Add(TerrainImprovement.mine)},
		{UnitAction.Irrigate, (_, tile) => tile.overlays.Add(TerrainImprovement.irrigation)},
		{UnitAction.BuildRoad, (player, tile) => {
				tile.overlays.Add(TerrainImprovement.road);

				// The trade network needs to be recomputed whenever roads are added.
				player.InvalidateCachedTradeNetwork();
			}
		},
		{UnitAction.BuildRailroad, (_, tile) => tile.overlays.Add(TerrainImprovement.railroad)},
		{UnitAction.ClearWetlands, (_, tile) => tile.ClearTerrainOverlay()},
		// TODO: add bonus shields to the nearest city - should only happen the first time a forest is cleared
		{UnitAction.ClearForest, (_, tile) => tile.ClearTerrainOverlay()},
	};

	public static readonly Dictionary<UnitAction, Func<Player, Tile, bool>> ActionValidators = new() {
		{UnitAction.BuildMine, (_, tile) => tile.CanBeMined()},
		{UnitAction.Irrigate, (player, tile) => tile.CanBeIrrigated(player)},
		{UnitAction.BuildRoad, (_, tile) => tile.overlays.CanAdd(TerrainImprovement.road)},
		{UnitAction.BuildRailroad, (_, tile) => tile.overlays.CanAdd(TerrainImprovement.railroad)},
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
	public UnitAction Action;

	public List<Resource> RequiredResources = [];

	public Action<Player, Tile> OnComplete;
	private Func<Player, Tile, bool> ActionValidator;

	private SaveTerraform dataSource;

	public Terraform(SaveTerraform saveTerraform, GameData gameData) {
		Id = saveTerraform.Id;
		Name = saveTerraform.Name;
		CivilopediaEntry = saveTerraform.CivilopediaEntry;
		TurnsToComplete = saveTerraform.TurnsToComplete;
		RequiredTech = saveTerraform.RequiredTech;
		Action = saveTerraform.Action;
		dataSource = saveTerraform;
		RequiredResources = saveTerraform.RequiredResources.ConvertAll(resKey => gameData.Resources.Find(res => res.Key == resKey));

		SetRules();
	}

	private void SetRules() {
		if (TerraformRules.ActionEffects.TryGetValue(Action, out var onComplete)) {
			OnComplete = onComplete;
		} else {
			OnComplete = (_, _) => { };
		}

		if (TerraformRules.ActionValidators.TryGetValue(Action, out var prerequisite)) {
			ActionValidator = prerequisite;
		} else {
			ActionValidator = (_, _) => false;
		}
	}

	public bool MeetsRequirements(Player player, Tile tile) {
		bool hasTech = RequiredTech == null || player.knownTechs.Contains(RequiredTech);

		bool hasResources = RequiredResources.All(
			res => player.GetTradeNetwork().HasTradeAccess(tile, res)
		);

		return hasTech && hasResources && ActionValidator(player, tile);
	}

	public SaveTerraform ToSaveTerraform() {
		return dataSource;
	}
}
