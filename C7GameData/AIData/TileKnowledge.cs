using System.Collections.Generic;
using System.Linq;

namespace C7GameData {
	public class TileKnowledge {
		private readonly Player _player;

		public TileKnowledge(Player player) {
			_player = player;
		}

		public HashSet<Tile> knownTiles { get; private set; } = new();
		public HashSet<Tile> borderTiles { get; private set; } = new();
		private HashSet<Tile> activeTiles = new HashSet<Tile>();

		// Has this player explored all known ocean tiles?
		// TODO: this should be split out for coast/ocean
		public bool fullyExploredOceans = false;

		// The set of tiles this player currently has explorers headed towards.
		public HashSet<Tile> aiExplorationTargets = new();

		public void AddTilesToKnown(Tile unitLocation) {
			knownTiles.Add(unitLocation);
			borderTiles.Remove(unitLocation);

			foreach (Tile t in unitLocation.neighbors.Values) {
				if (t == Tile.NONE) {
					continue;
				}

				knownTiles.Add(t);
				borderTiles.Remove(t);

				foreach (Tile border in t.neighbors.Values) {
					if (border == Tile.NONE) {
						continue;
					}
					if (!knownTiles.Contains(border)) {
						borderTiles.Add(border);
					}
				}
			}

			RecomputeActiveTiles();
		}

		// neighboring tiles should not be added when loading tile knowledge
		// from a .sav file
		internal bool AddTileToKnown(Tile unitLocation) {
			bool added = knownTiles.Add(unitLocation);
			borderTiles.Remove(unitLocation);

			foreach (Tile border in unitLocation.neighbors.Values) {
				if (border == Tile.NONE) {
					continue;
				}
				if (!knownTiles.Contains(border)) {
					borderTiles.Add(border);
				}
			}

			return added;
		}

		public bool isTileKnown(Tile t) {
			return knownTiles.Contains(t);
		}

		public bool isActiveTile(Tile t) {
			return activeTiles.Contains(t);
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

		public void RecomputeActiveTiles() {
			activeTiles.Clear();
			foreach (Tile t in knownTiles) {
				// A tile within a city's borders and all of its neighbors are active.
				if (t.owningCity != null && t.owningCity.owner == _player) {
					activeTiles.Add(t);

					foreach (Tile neighbor in t.neighbors.Values) {
						activeTiles.Add(neighbor);
					}
					continue;
				}

				// A tile with a unit on it and all of its neighbors are active.
				if (t.unitsOnTile.Count > 0 && t.unitsOnTile[0].owner == _player) {
					activeTiles.Add(t);

					foreach (Tile neighbor in t.neighbors.Values) {
						activeTiles.Add(neighbor);
					}
					continue;
				}
			}
		}
	}
}
