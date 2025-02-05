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

			IOrderedEnumerable<KeyValuePair<Tile, float> > orderedScores = scores.OrderByDescending(t => t.Value);
			Log.Debug("Top city location candidates from " + start + ":");
			Tile returnValue = null;
			foreach (KeyValuePair<Tile, float> kvp in orderedScores.Take(5)) {
				returnValue ??= kvp.Key;
				if (kvp.Value > 0) {
					Log.Debug("  Tile " + kvp.Key + " scored " + kvp.Value);
				}
			}
			return returnValue;
		}

		public static Dictionary<Tile, float> GetScoredSettlerCandidates(Tile start, Player player) {
			List<MapUnit> playerUnits = player.Units;
			IEnumerable<Tile> candidates = player.TileKnowledge.AllKnownTiles().Where(t => !IsInvalidCityLocation(t));
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
					score += player.Civilization.Adjustments.HillsBonus;
				}
				if (t.NeighborsWater()) {
					score += player.Civilization.Adjustments.WaterBonus;
				}

				//Lower scores if they are far away
				float preDistanceScore = score;
				int distance = startTile.distanceTo(t);
				if (distance > player.Civilization.Adjustments.DistancePenaltyRadius) {
					score += player.Civilization.Adjustments.DistancePenalty(distance);
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
			float score = owner.Civilization.Adjustments.FoodYieldBonus(t.foodYield(owner));
			score += owner.Civilization.Adjustments.ProductionYieldBonus(t.productionYield(owner));
			score += owner.Civilization.Adjustments.CommerceYieldBonus(t.commerceYield(owner));
			switch (t.Resource.Category)
			{
				case ResourceCategory.STRATEGIC:
					// TODO: increase bonus if this civ doesn't have this strategic resource yet
					score += owner.Civilization.Adjustments.StrategicResourceBonus;
					break;
				case ResourceCategory.LUXURY:
					score += owner.Civilization.Adjustments.LuxuryResourceBonus;
					break;
			}
			return score;
		}

		private static bool IsInvalidCityLocation(Tile tile) {
			if (tile.HasCity) {
				Log.Verbose("Tile " + tile + " is invalid due to existing city of " + tile.cityAtTile.name);
				return true;
			}
			foreach (Tile neighbor in tile.neighbors.Values) {
				if (neighbor.HasCity) {
					Log.Verbose("Tile " + tile + " is invalid due to neighboring city of " + neighbor.cityAtTile.name);
					return true;
				}
				foreach (Tile neighborOfNeighbor in neighbor.neighbors.Values) {
					if (neighborOfNeighbor.HasCity) {
						Log.Verbose("Tile " + tile + " is invalid due to nearby city of " + neighborOfNeighbor.cityAtTile.name);
						return true;
					}
				}
			}

			Log.Debug("Tile " + tile + " is a valid city location ");
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
				// ReSharper disable once InvertIf
				if (otherSettler.currentAIData is SettlerAiData otherSettlerAi) {
					if (otherSettlerAi.Destination == tile) {
						return true;
					}
					if (otherSettlerAi.Destination.GetLandNeighbors().Exists(innerRingTile => innerRingTile == tile)) {
						return true;
					}
					foreach (Tile innerRingTile in otherSettlerAi.Destination.GetLandNeighbors()) {
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
