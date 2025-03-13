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
			string? improvement = GetTileImprovement(unit.location, unit.owner);
			if (improvement != null) {
				return PerformWorkerMove(unit, improvement);
			}

			return this.TryToMoveAlongPath(unit, ref data.pathToDestination, /*allowCombat=*/false);
		}

		private static string? GetTileImprovement(Tile t, Player player) {
			if (!t.IsLand()) {
				return null;
			}

			// Don't waste worker moves on unowned tiles. We do allow roading
			// unowned tiles though.
			if (t.owningCity == null || t.owningCity.owner != player) {
				if (t.CanBeRoaded()) {
					return C7Action.UnitBuildRoad;
				}
				return null;
			}

			// "Mine green, irrigate brown"
			if (t.overlayTerrainType.Key == "grassland" || t.overlayTerrainType.Key == "hills" || t.overlayTerrainType.Key == "mountains") {
				if (t.CanBeMined()) {
					return C7Action.UnitBuildMine;
				}
				if (t.CanBeRoaded()) {
					return C7Action.UnitBuildRoad;
				}
				return null;
			}

			if (t.overlayTerrainType.Key == "desert" || t.overlayTerrainType.Key == "plains" || t.overlayTerrainType.Key == "flood plain") {
				if (t.CanBeIrrigated(player)) {
					return C7Action.UnitIrrigate;
				} else {
					// TODO: We should check to see if we can chain irrigate.
				}
				if (t.CanBeRoaded()) {
					return C7Action.UnitBuildRoad;
				}
				if (t.CanBeMined()) {
					return C7Action.UnitBuildMine;
				}
				return null;
			}

			// TODO: handle clearing forest/jungle/marsh
			if (t.CanBeRoaded()) {
				return C7Action.UnitBuildRoad;
			}
			return null;
		}

		private UnitAI.Result PerformWorkerMove(MapUnit unit, string workerMove) {
			if (workerMove == C7Action.UnitBuildRoad) {
				if (unit.canBuildRoad()) {
					unit.PerformTerraformAction(C7Action.UnitBuildRoad);
					return UnitAI.Result.InProgress;
				}
				if (unit.location.overlays.road) {
					return UnitAI.Result.Done;
				}
				return UnitAI.Result.Error;
			} else if (workerMove == C7Action.UnitBuildMine) {
				if (unit.canBuildMine()) {
					unit.PerformTerraformAction(C7Action.UnitBuildMine);
					return UnitAI.Result.InProgress;
				}
				if (unit.location.overlays.mine) {
					return UnitAI.Result.Done;
				}
				return UnitAI.Result.Error;
			} else if (workerMove == C7Action.UnitIrrigate) {
				if (unit.canIrrigate()) {
					unit.PerformTerraformAction(C7Action.UnitIrrigate);
					return UnitAI.Result.InProgress;
				}
				if (unit.location.overlays.irrigation) {
					return UnitAI.Result.Done;
				}
				return UnitAI.Result.Error;
			} else {
				throw new ArgumentOutOfRangeException("Invalid worker move" + data.workerMove);
			}
		}

		// Given a list of tile candidates, return a plan to improve the nearest
		// unimproved tile.
		private static WorkerAIData? GetPlanToImproveNearestUnimproved(MapUnit unit, Player player, List<Tile> tiles) {
			List<Tile> nearestWorkedTiles =
				tiles.OrderBy(x => x.distanceTo(unit.location))
					.ThenByDescending(x => CityTileAssignmentAI.CalculateTileYieldScore(x, 2, player))
					.ToList();
			foreach (Tile t in nearestWorkedTiles) {
				string? improvement = GetTileImprovement(t, unit.owner);
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
