using System.Collections.Generic;

namespace C7GameData {
	public class TileKnowledge {
		HashSet<Tile> knownTiles = new();
		public HashSet<Tile> borderTiles { get; private set; } = new();
		HashSet<Tile> visibleTiles = new();

		// Has this player explored all known ocean tiles?
		// TODO: this should be split out for coast/ocean
		public bool fullyExploredOceans = false;

		// The set of tiles this player currently has explorers headed towards.
		public HashSet<Tile> aiExplorationTargets = new();

		public void AddTilesToKnown(Tile unitLocation) {
			knownTiles.Add(unitLocation);
			borderTiles.Remove(unitLocation);

			foreach (Tile t in unitLocation.neighbors.Values) {
				knownTiles.Add(t);
				borderTiles.Remove(t);

				foreach (Tile border in t.neighbors.Values) {
					if (!knownTiles.Contains(border)) {
						borderTiles.Add(border);
					}
				}
			}
		}

		// neighboring tiles should not be added when loading tile knowledge
		// from a .sav file
		internal bool AddTileToKnown(Tile unitLocation) {
			bool added = knownTiles.Add(unitLocation);
			borderTiles.Remove(unitLocation);

			foreach (Tile border in unitLocation.neighbors.Values) {
				if (!knownTiles.Contains(border)) {
					borderTiles.Add(border);
				}
			}

			return added;
		}

		public bool isTileKnown(Tile t) {
			return knownTiles.Contains(t);
		}

		public bool isBorderOfTileKnowlege(Tile t) {
			return borderTiles.Contains(t);
		}

		/**
		 * Returns a copy of the list of known tiles.
		 * This prevents external modifications.
		 **/
		public List<Tile> AllKnownTiles() {
			List<Tile> list = new List<Tile>();
			foreach (Tile t in knownTiles) {
				list.Add(t);
			}
			return list;
		}
	}
}
