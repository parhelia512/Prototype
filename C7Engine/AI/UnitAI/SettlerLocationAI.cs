using System.Collections.Generic;
using C7GameData;
using System.Linq;
using C7GameData.AIData;
using Serilog;
// ReSharper disable ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
// ReSharper disable CheckNamespace

namespace C7Engine {
	public class SettlerLocationAi {
		private static readonly ILogger Log = Serilog.Log.ForContext<SettlerLocationAi>();

		//Figures out where to plant Settlers
		public static Tile FindSettlerLocation(Tile start, Player player) {
			Dictionary<Tile, float> scores = GetScoredSettlerCandidates(start, player);
			if (scores.Count == 0 || scores.Values.Max() <= 0) {
				return Tile.NONE;   //nowhere to settle
			}

			Tile result = scores.MaxBy(t => t.Value).Key;
			return result;
		}

		public static Dictionary<Tile, float> GetScoredSettlerCandidates(Tile start, Player player) {
			List<MapUnit> playerUnits = player.units;
			// TODO: handle settling other continents
			IEnumerable<Tile> candidates = player.tileKnowledge.AllKnownTiles().Where(t => !IsInvalidCityLocation(t) && t.continent == start.continent);
			Dictionary<Tile, float> scores = AssignTileScores(start, player, candidates, playerUnits.FindAll(u => u.unitType.name == "Settler"));
			return scores;
		}

		private static Dictionary<Tile, float> AssignTileScores(Tile startTile, Player player, IEnumerable<Tile> candidates, List<MapUnit> playerSettlers) {
			Dictionary<Tile, float> scores = new();
			candidates = candidates.Where(t => !SettlerAlreadyMovingTowardsTile(t, playerSettlers) && t.IsAllowCities());
			foreach (Tile t in candidates) {
				float score = GetTileYieldScore(t, player);
				//For simplicity's sake, I'm only going to look at immediate neighbors here, but
				//a lot more things should be considered over time.
				foreach (Tile nt in t.neighbors.Values) {
					score += GetTileYieldScore(nt, player);
				}
				//TODO: Also look at the next ring out, with lower weights.

				//Prefer hills for defense, and coast for boats and such.
				if (t.baseTerrainType.Key == "hills") {
					score += player.civilization.Adjustments.HillsBonus;
				}
				if (t.NeighborsWater()) {
					score += player.civilization.Adjustments.WaterBonus;
				}

				//Lower scores if they are far away
				float preDistanceScore = score;
				int distance = startTile.distanceTo(t);
				if (distance > player.civilization.Adjustments.DistancePenaltyRadius) {
					score += player.civilization.Adjustments.DistancePenalty(distance);
				}
				if (distance > 8) {
					score -= distance * 4;
				}
				//Distance can never lower score beyond 1; the AI will always try to settle those worthless tundras.
				//(This could actually be modified in the future, but for now is also a safety rail)
				if (preDistanceScore > 0 && score <= 0) {
					score = 1;
				}
				if (score > 0)
					scores[t] = score;
			}
			return scores;
		}

		private static float GetTileYieldScore(Tile t, Player owner) {
			float score = owner.civilization.Adjustments.FoodYieldBonus(t.foodYield(owner).yield);
			score += owner.civilization.Adjustments.ProductionYieldBonus(t.productionYield(owner).yield);
			score += owner.civilization.Adjustments.CommerceYieldBonus(t.commerceYield(owner).yield);
			if (owner.KnowsAboutResource(t.Resource)) {
				if (t.Resource.Category == ResourceCategory.STRATEGIC) {
					score += owner.civilization.Adjustments.StrategicResourceBonus;
				} else if (t.Resource.Category == ResourceCategory.LUXURY) {
					score += owner.civilization.Adjustments.LuxuryResourceBonus;
				}
			}
			return score;
		}

		private static bool IsInvalidCityLocation(Tile tile) {
			if (tile.HasCity) {
				return true;
			}
			foreach (Tile neighbor in tile.neighbors.Values) {
				if (neighbor.HasCity) {
					return true;
				}
				foreach (Tile neighborOfNeighbor in neighbor.neighbors.Values) {
					if (neighborOfNeighbor.HasCity) {
						return true;
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Returns true if one of the settlers in the list (which should be the list of the current AI's settlers) is
		/// already heading to a tile near the requested tile.
		/// Does not return true if only another AI's settlers are headed there, as the AI shouldn't know the other
		/// AI's plans.
		/// </summary>
		/// <param name="tile">The tile under consideration for a future city.</param>
		/// <param name="playerSettlers">The settlers owned by the AI considering building a city.</param>
		/// <returns></returns>
		private static bool SettlerAlreadyMovingTowardsTile(Tile tile, List<MapUnit> playerSettlers) {
			foreach (MapUnit otherSettler in playerSettlers) {
				if (otherSettler.currentAI is SettlerAI otherSettlerAI) {
					Tile otherDestination = ((SettlerAI)(otherSettler.currentAI)).data.destination;
					if (otherDestination == tile) {
						return true;
					}
					if (otherDestination.GetLandNeighbors().Exists(innerRingTile => innerRingTile == tile)) {
						return true;
					}
					foreach (Tile innerRingTile in otherDestination.GetLandNeighbors()) {
						if (innerRingTile.GetLandNeighbors().Exists(outerRingTile => outerRingTile == tile)) {
							return true;
						}
					}
				}
			}
			return false;
		}
	}
}
