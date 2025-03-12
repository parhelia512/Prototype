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
				knownTiles.Add(t);
				borderTiles.Remove(t);

				foreach (Tile border in t.neighbors.Values) {
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

		private void RecomputeActiveTiles() {
			activeTiles.Clear();

			activeTiles = _player.cities
				.SelectMany(x => x.GetTilesWithinBorders().SelectMany(y => y.neighbors.Values).Append(x.location))
				.Concat(_player.units.SelectMany(x => x.location.neighbors.Values.Append(x.location)))
				.ToHashSet();
		}
	}
}
