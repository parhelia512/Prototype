namespace C7GameData {
	using System;
	using System.Linq;
	using System.Collections.Generic;
	using C7GameData.Save;

	/**
	 * The game map, at the top level.
	 */
	public class GameMap {
		// TODO : protect setters while still allowing JSON deserialization
		public int numTilesWide { get; set; }
		public int numTilesTall { get; set; }
		public bool wrapHorizontally, wrapVertically;
		public int techRate;
		public int optimalNumberOfCities;

		public List<Tile> tiles { get; set; }
		public List<Tile> barbarianCamps = new List<Tile>();

		// The list of continents, with continents[0] being the largest continent.
		//
		// A "water" continent is an ocean.
		public List<HashSet<Tile>> continents = new();

		// The list of starting locations.
		//
		// TODO: How is this saved in editor generated scenarios?
		public List<Tile> startingLocations = new();

		public GameMap() {
			this.tiles = new List<Tile>();
		}

		public int tileCoordsToIndex(int X, int Y) {
			return Y * numTilesWide / 2 + (Y % 2 == 0 ? X / 2 : (X - 1) / 2);
		}

		public void tileIndexToCoords(int index, out int X, out int Y) {
			int doubleRow = index / numTilesWide;
			int doubleRowRem = index % numTilesWide;
			if (doubleRowRem < numTilesWide / 2) {
				X = 2 * doubleRowRem;
				Y = 2 * doubleRow;
			} else {
				X = 1 + 2 * (doubleRowRem - numTilesWide / 2);
				Y = 2 * doubleRow + 1;
			}
		}

		public void computeNeighbors() {
			foreach (Tile tile in tiles) {
				Dictionary<TileDirection, Tile> neighbors = new Dictionary<TileDirection, Tile>();
				foreach (TileDirection direction in Enum.GetValues(typeof(TileDirection))) {
					neighbors[direction] = tileNeighbor(tile, direction);
				}
				tile.neighbors = neighbors;
				tile.map = this;
			}
		}

		// This method verifies that the conversion between tile index and coords is consistent for all possible valid inputs. It's not called
		// anywhere but I'm keeping it around in case we ever need to work on the conversion methods again.
		public void testTileIndexComputation() {
			for (int Y = 0; Y < numTilesTall; Y++)
				for (int X = Y % 2; X < numTilesWide; X += 2) {
					int rX, rY;
					int index = tileCoordsToIndex(X, Y);
					tileIndexToCoords(index, out rX, out rY);
					if ((rX != X) || (rY != Y))
						throw new Exception(String.Format("Error computing tile index/coords: ({0}, {1}) -> {2} -> ({3}, {4})", X, Y, index, rX, rY));
				}

			for (int i = 0; i < numTilesWide * numTilesTall / 2; i++) {
				int X, Y;
				tileIndexToCoords(i, out X, out Y);
				int ri = tileCoordsToIndex(X, Y);
				if (ri != i)
					throw new Exception(String.Format("Error computing tile index/coords: {0} -> ({1}, {2}) -> {3}", i, X, Y, ri));
			}
		}

		public bool isRowAt(int Y) {
			return wrapVertically || ((Y >= 0) && (Y < numTilesTall));
		}

		public bool isTileAt(int X, int Y) {
			bool evenRow = Y%2 == 0;
			bool XInBounds;
			{
				if (wrapHorizontally)
					XInBounds = true;
				else if (evenRow)
					XInBounds = (X >= 0) && (X <= numTilesWide - 2);
				else
					XInBounds = (X >= 1) && (X <= numTilesWide - 1);
			}
			return XInBounds && isRowAt(Y) && (evenRow ? (X % 2 == 0) : (X % 2 != 0));
		}

		public int wrapTileX(int X) {
			if (wrapHorizontally) {
				int wrappedX = X % numTilesWide;
				return (wrappedX >= 0) ? wrappedX : wrappedX + numTilesWide;
			} else
				return X;
		}

		public int wrapTileY(int Y) {
			if (wrapVertically) {
				int wrappedY = Y % numTilesTall;
				return (wrappedY >= 0) ? wrappedY : wrappedY + numTilesTall;
			} else
				return Y;
		}

		public Tile tileAt(int X, int Y) {
			if (isTileAt(X, Y))
				return tiles[tileCoordsToIndex(wrapTileX(X), wrapTileY(Y))];
			else
				return Tile.NONE;
		}

		/**
		 * Returns the Tile that neighbors the given Tile in a certain direction,
		 * or the NONE tile if there is no neighbor in said direction.
		 **/
		public Tile tileNeighbor(Tile center, TileDirection direction) {
			TileLocation neighbor = Tile.NeighborCoordinate(new TileLocation(center), direction);
			return tileAt(neighbor.X, neighbor.Y);
		}

		// Calculates X1 - X2, handling world wrap.
		public int CalculateXDelta(int X1, int X2) {
			if (!wrapHorizontally) {
				return X1 - X2;
			}

			int rawDelta = X1 - X2;
			if (rawDelta > numTilesWide / 2) {
				return rawDelta - numTilesWide;
			} else if (rawDelta < -numTilesWide / 2) {
				return rawDelta + numTilesWide;
			}
			return rawDelta;
		}

		// Calculates Y1 - Y2, handling world wrap.
		public int CalculateYDelta(int Y1, int Y2) {
			if (!wrapVertically) {
				return Y1 - Y2;
			}

			int rawDelta = Y1 - Y2;
			if (rawDelta > numTilesTall / 2) {
				return rawDelta - numTilesTall;
			} else if (rawDelta < -numTilesTall / 2) {
				return rawDelta + numTilesTall;
			}
			return rawDelta;
		}

		public void recomputeContinents() {
			int nextContinent = 0;
			HashSet<Tile> currentContinent = new();
			HashSet<Tile> seen = new();

			foreach (Tile t in tiles) {
				if (seen.Contains(t)) {
					continue;
				}
				seen.Add(t);

				Queue<Tile> toCheck = new();
				toCheck.Enqueue(t);

				while (toCheck.Count > 0) {
					Tile x = toCheck.Dequeue();
					x.continent = nextContinent;
					currentContinent.Add(x);

					foreach (Tile n in x.neighbors.Values) {
						if (!seen.Contains(n) && n.IsLand() == x.IsLand()) {
							seen.Add(n);
							toCheck.Enqueue(n);
						}
					}
				}
				++nextContinent;
				continents.Add(currentContinent);
				currentContinent = new();
			}

			// Sort by size, descending.
			continents.Sort((x, y) => y.Count.CompareTo(x.Count));

			// For water tiles, determine if they are ocean or inland seas.
			foreach (HashSet<Tile> continent in continents) {
				if (continent.First().IsLand()) {
					continue;
				}

				// Bodies of water under 20 tiles are fresh water
				// (https://civilization.fandom.com/wiki/Fresh_Water_Lake_(Civ3)).
				//
				// TODO: consider making this part of the rules.
				if (continent.Count <= 20) {
					foreach (Tile t in continent) {
						t.isFreshWater = true;
					}
				}
			}
		}
	}
}
