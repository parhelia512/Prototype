using System.Collections.Generic;
using System.Linq;
using System;
using C7GameData;

namespace C7Engine.Pathing {
	public class TileAndCost : IComparable<TileAndCost> {
		public Tile tile;
		public double cost = 0;

		public int CompareTo(TileAndCost other) {
			return cost.CompareTo(other.cost);
		}
	}

	// An implementation fo the A* path searching algorithm.
	//
	// See https://en.wikipedia.org/wiki/A*_search_algorithm and
	// https://github.com/yairm210/Unciv/blob/master/core/src/com/unciv/logic/map/AStar.kt
	// for some useful details.
	public class AStarAlgorithm : PathingAlgorithm {
		private readonly EdgeWalker<Tile> edgeWalker;

		// An estimate of the cost between two tiles, usually between a tile in
		// the search and the destination tile. This allows targeting the search
		// by doing things like focusing the search in the cardinal direction of
		// the destination instead of expanding outwards in all directions.
		private Func<Tile, Tile, double> costHeuristic;

		// A delegate that determines if a tile is passable during pathfinding.
		// Receives the tile being evaluated and the final destination tile.
		private Func<Tile, Tile, bool> isPassable;

		public AStarAlgorithm(EdgeWalker<Tile> edgeWalker, Func<Tile, Tile, double> costHeuristic, Func<Tile, Tile, bool> isPassable) {
			this.edgeWalker = edgeWalker;
			this.costHeuristic = costHeuristic;
			this.isPassable = isPassable;
		}

		public override TilePath PathFrom(Tile start, Tile destination, MapUnit unit) {
			// Exit early if we're starting and ending on land, and we're on
			// different continents. Don't waste time checking every tile on the
			// continent just to discover the path is impossible.
			if (!unit.owner.isHuman || unit.owner.HasExploredTile(destination)) {
				if (unit.IsLandUnit() && start.IsLand() && destination.IsLand() && start.continent != destination.continent) {
					return TilePath.EmptyPath(destination);
				}
				// Avoid trying to calculate an impossible path, for example, from the sea to a lake
				if (unit.IsWaterUnit() && start.IsWater() && destination.IsWater() && start.continent != destination.continent) {
					return TilePath.EmptyPath(destination);
				}
			}
			// The set of tiles to explore next, ordered by their cumulative cost
			// so far and the estimate of the cost to the goal.
			BinaryMinHeap<TileAndCost> openSet = new();
			openSet.insert(new TileAndCost() {
				tile = start,
				cost = 0,
			});

			// For a given tile, cameFrom[tile] is the tile preceeding it on the
			// cheapest path from start->tile.
			Dictionary<Tile, Tile> cameFrom = new();

			// For a given tile, knownCosts[tile] is the currently known cheapest
			// path from start->tile.
			Dictionary<Tile, double> knownCosts = new();
			knownCosts.Add(start, 0);

			while (openSet.count > 0) {
				// Get the tile with the lowest estimated cost.
				TileAndCost currentTileAndCost = openSet.extract();
				Tile current = currentTileAndCost.tile;

				// If this is the destination, we're done.
				if (current == destination) {
					return makePath(cameFrom, current);
				}

				// Otherwise check each neighbor that we can use.
				foreach (Edge<Tile> neighborEdge in edgeWalker.getEdges(current)) {
					Tile neighbor = neighborEdge.current;

					// TODO: the possibility here is to create a building/improvement/tech effect like "canal"
					// (much like what the Panama canal is irl) that allows that kind of movement
					if (unit.IsWaterUnit() && IsLandStrip(current, neighbor)) continue;

					if (!isPassable(neighbor, destination)) {
						continue;
					}

					double newCost = neighborEdge.distanceToCurrent + knownCosts[current];

					// If we didn't know the cost to get to this neighbor, or
					// this new path is now cheaper, update our data structures.
					if (!knownCosts.ContainsKey(neighbor) || newCost < knownCosts[neighbor]) {
						UpdateDictionary(knownCosts, neighbor, newCost);
						double estimatedCost = newCost + costHeuristic(neighbor, destination);
						openSet.insert(new TileAndCost() {
							tile = neighbor,
							cost = estimatedCost,
						});
						UpdateDictionary(cameFrom, neighbor, current);
					}
				}
			}

			return TilePath.EmptyPath(destination);
		}

		private bool IsLandStrip(Tile current, Tile neighbor) {
			// Account for small strips of land(visually) that water units must go around.
			// These are coast tiles that bridge land tiles, and that a sea unit couldn't go through
			// because otherwise that would make them islands of sorts.
			// But in reality they are sea tiles with the coast texture, right next to each other,
			// and nothing was preventing a sea unit to just cross from one to the other.
			//
			// Because of the isometric nature of the tiles, it seems that
			// tiles that have a different relationship other than W<->E and N<->S
			// like NW<->SE and NE<->SW can't possibly have the same issues
			// because the never meet in a corner, but rather an edge.
			//
			//  east<->west case       north<->south case
			//
			//        <L>                    <S>
			//      <S> <S>       or       <L> <L>
			//        <L>                    <S>
			//
			if (current.IsWater() && neighbor.IsWater()) {
				// current:east -> neighbor:west case
				if (current.neighbors[TileDirection.WEST] == neighbor
					&& current.neighbors[TileDirection.SOUTHWEST].IsLand()
					&& current.neighbors[TileDirection.NORTHWEST].IsLand()) {
					return true;
				}
				// current:west -> neighbor:east case
				if (current.neighbors[TileDirection.EAST] == neighbor
					&& current.neighbors[TileDirection.SOUTHEAST].IsLand()
					&& current.neighbors[TileDirection.NORTHEAST].IsLand()) {
					return true;
				}
				// current:south -> neighbor:north case
				if (current.neighbors[TileDirection.NORTH] == neighbor
					&& current.neighbors[TileDirection.NORTHWEST].IsLand()
					&& current.neighbors[TileDirection.NORTHEAST].IsLand()) {
					return true;
				}
				// current:north -> neighbor:south case
				if (current.neighbors[TileDirection.SOUTH] == neighbor
					&& current.neighbors[TileDirection.SOUTHWEST].IsLand()
					&& current.neighbors[TileDirection.SOUTHEAST].IsLand()) {
					return true;
				}
			}
			return false;
		}

		private static TilePath makePath(Dictionary<Tile, Tile> cameFrom, Tile current) {
			List<Tile> tilesInPath = new List<Tile>() { current };
			Tile tile = current;
			while (cameFrom.ContainsKey(tile)) {
				tile = cameFrom[tile];
				tilesInPath.Add(tile);
			}

			tilesInPath.Reverse();
			Queue<Tile> path = new Queue<Tile>();
			foreach (Tile t in tilesInPath.Skip(1)) {
				path.Enqueue(t);
			}
			return new TilePath(current, path);
		}

		private static void UpdateDictionary<Key, Value>(Dictionary<Key, Value> dictionary, Key key, Value value) {
			if (dictionary.ContainsKey(key)) {
				dictionary[key] = value;
			} else {
				dictionary.Add(key, value);
			}
		}
	}
}
