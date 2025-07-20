using System;
using System.Collections.Generic;
using System.Linq;
using C7GameData.Save;
using C7Engine;

namespace C7GameData;

public class Terraform {
	public ID Id;
	public string Name;
	public string CivilopediaEntry;
	public int TurnsToComplete;
	public ID RequiredTech;
	public UnitAction Action;

	public List<Resource> RequiredResources = [];

	public TerrainImprovement Improvement;

	private Action<Player, Tile> Effect;
	private List<Func<Player, Tile, bool>> ActionValidators = [];

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

		if (saveTerraform.Improvement != null)
			Improvement = TerrainImprovement.FromKey(saveTerraform.Improvement);

		SetRules(gameData.luaRulesEngine);
	}

	public string ToString() {
		return Name;
	}

	private void SetRules(LuaRulesEngine engine) {
		foreach (string functionPath in dataSource.Effects) {
			Effect += engine.ImportFunc<Action<Player, Tile>>(functionPath);
		}

		foreach (string functionPath in dataSource.Validators) {
			ActionValidators.Add(engine.ImportFunc<Func<Player, Tile, bool>>(functionPath));
		}
	}

	public bool MeetsRequirements(Player player, Tile tile) {
		bool hasTech = RequiredTech == null || player.knownTechs.Contains(RequiredTech);

		bool hasResources = RequiredResources.All(
			res => EngineStorage.gameData.GetTradeNetwork().HasTradeAccess(tile, player, res)
		);

		bool canAddImprovement = Improvement == null || tile.overlays.CanAdd(Improvement);

		return hasTech && hasResources
				&& canAddImprovement && ActionValidators.All(func => func(player, tile));
	}

	public void OnComplete(Player player, Tile tile) {
		Effect?.Invoke(player, tile);

		if (Improvement != null) {
			tile.overlays.Add(Improvement);
		}
	}

	public SaveTerraform ToSaveTerraform() {
		return dataSource;
	}
}
