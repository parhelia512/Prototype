using System;
using C7Engine.Pathing;
using C7GameData;
using System.Collections.Generic;
using System.Linq;
using C7GameData.AIData;
using C7Engine.AI;
using Serilog;

namespace C7Engine {
	public class WorkerAI : UnitAI {
		private static ILogger log = Log.ForContext<WorkerAI>();
		private WorkerAIData data;

		public WorkerAI(WorkerAIData d) {
			data = d;
		}

		public static WorkerAIData MakeAiData(MapUnit unit, Player player) {
			// First, check if there are any tiles our civ works that we can
			// improve.
			List<Tile> workedTiles = new();
			foreach (City city in player.cities) {
				foreach (CityResident cr in city.residents) {
					if (cr.tileWorked != Tile.NONE) {
						workedTiles.Add(cr.tileWorked);
					}
				}
			}
			WorkerAIData? result = GetPlanToImproveNearestUnimproved(unit, player, workedTiles);
			if (result != null) {
				return result;
			}

			// If all our worked tiles are improved, improve the nearest owned
			// tile.
			List<Tile> ownedTiles = new();
			foreach (City city in player.cities) {
				foreach (Tile tile in city.GetTilesWithinBorders()) {
					ownedTiles.Add(tile);
				}
			}
			return GetPlanToImproveNearestUnimproved(unit, player, ownedTiles);
		}

		public string SummarizePlan() {
			return "WorkerAI: " + data.ToString();
		}

		public void UpdateOnDeath() { }

		UnitAI.Result UnitAI.PlayTurnImpl(Player player, MapUnit unit) {
			if (data == null) {
				return UnitAI.Result.Error;
			}

			// If we're at the destination, do our planned move.
			if (unit.location == data.destination) {
				return PerformWorkerMove(unit, data.workerMove);
			}

			// Otherwise we're moving towards our actual goal. To avoid wasting
			// worker moves, see if there's anything we can do on this tile
			// before moving to the next.
			//
			// This also functions as a way to build a road network, since we
			// will eventually road between all worked tiles of all cities.
			Terraform? improvement = GetTileImprovement(unit.location, unit);
			if (improvement != null) {
				return PerformWorkerMove(unit, improvement);
			}

			return this.TryToMoveAlongPath(unit, ref data.pathToDestination, allowCombat: false);
		}

		private static Terraform? GetTileImprovement(Tile t, MapUnit unit) {
			if (!t.IsLand()) {
				return null;
			}

			HashSet<Terraform> accessibleTerraforms = EngineStorage.gameData.Terraforms
													.Where(terr => unit.canPerformTerraformAction(terr, t))
													.ToHashSet();

			if (accessibleTerraforms.Count == 0) {
				return null;
			}

			Player player = unit.owner;

			// Don't waste worker moves on unowned tiles. We do allow roading
			// unowned tiles though.
			if (t.owningCity == null || t.owningCity.owner != player) {
				Terraform buildRoad = UnitAction.BuildRoad.ToTerraform();

				if (accessibleTerraforms.Contains(buildRoad)) {
					return buildRoad;
				}
				return null;
			}

			int initialCommerce = t.commerceYield(player).yield;
			int initialShields = t.productionYield(player).yield;
			int initialFood = t.foodYield(player).yield;

			int bestScore = 0;
			Terraform? bestTerraform = null;

			const int CommercePoints = 1;
			const int ShieldPoints = 3;
			const int FoodPoints = 5;

			foreach (Terraform terraform in accessibleTerraforms) {
				Tile tAfterImprovement = t.Copy();
				terraform.OnComplete(tAfterImprovement);

				int newCommerce = tAfterImprovement.commerceYield(player).yield;
				int newShields = tAfterImprovement.productionYield(player).yield;
				int newFood = tAfterImprovement.foodYield(player).yield;

				int commerceDiff = newCommerce - initialCommerce;
				int shieldDiff = newShields - initialShields;
				int foodDiff = newFood - initialFood;

				int currentScore = commerceDiff * CommercePoints +
						  shieldDiff * ShieldPoints +
						  foodDiff * FoodPoints;

				if (currentScore > bestScore) {
					bestScore = currentScore;
					bestTerraform = terraform;
				}
			}

			return bestTerraform;
		}

		private UnitAI.Result PerformWorkerMove(MapUnit unit, Terraform workerMove) {
			if (unit.canPerformTerraformAction(workerMove)) {
				unit.PerformTerraformAction(workerMove);
				return UnitAI.Result.InProgress;
			}

			return UnitAI.Result.Done;
		}

		// Given a list of tile candidates, return a plan to improve the nearest
		// unimproved tile.
		private static WorkerAIData? GetPlanToImproveNearestUnimproved(MapUnit unit, Player player, List<Tile> tiles) {
			List<Tile> nearestWorkedTiles =
				tiles.OrderBy(x => x.distanceTo(unit.location))
					.ThenByDescending(x => CityTileAssignmentAI.CalculateTileYieldScore(x, 2, player))
					.ToList();
			foreach (Tile t in nearestWorkedTiles) {
				Terraform? improvement = GetTileImprovement(t, unit);
				if (improvement != null) {
					WorkerAIData result = new () {
						workerMove = improvement,
						destination = t,
					};
					log.Information($"Set AI for unit at {unit.location} to {improvement} with destination of " + result.destination);

					PathingAlgorithm algorithm = PathingAlgorithmChooser.GetAlgorithm(unit);
					result.pathToDestination = algorithm.PathFrom(unit.location, result.destination);

					return result;
				}
			}
			return null;
		}
	}
}
