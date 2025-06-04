using System.Collections.Generic;
using System.Linq;
using System;
using C7GameData;

namespace C7Engine.Pathing {
	public class TradeNetworkSegment {
		public Dictionary<Resource, int> resourceCounts = new();
		public HashSet<Tile> tiles = new();

		public void AddTile(Tile t, Player p) {
			tiles.Add(t);
			if (t.Resource != Resource.NONE && t.OwningPlayer() == p && p.KnowsAboutResource(t.Resource)) {
				if (resourceCounts.TryGetValue(t.Resource, out int currentCount)) {
					resourceCounts[t.Resource] = currentCount + 1;
				} else {
					resourceCounts[t.Resource] = 1;
				}
			}
		}
	}

	// A class for calculating the trade networks of a particular player.
	//
	// The main idea is that calculating the entire empire's trade network once
	// is faster than doing it repeatedly for each city. By doing it once we can
	// do a simple flood fill of the road network, rather than needing to do
	// actual pathfinding between different tiles.
	//
	// TODO: Handle harbors and airports
	// TODO: Account for passing through the borders of civs we're at war with
	public class TradeNetwork {
		public Dictionary<City, TradeNetworkSegment> segments = new();

		public TradeNetwork(Player player) {
			HashSet<Tile> seen = new();

			foreach (City c in player.cities) {
				if (segments.ContainsKey(c)) {
					continue;
				}

				// If we don't know about this city yet, start a new network
				// segment and do a flood fill for all roads coming from the
				// city.
				TradeNetworkSegment segment = new();
				segments[c] = segment;

				Queue<Tile> toCheck = new();
				toCheck.Enqueue(c.location);
				seen.Add(c.location);

				while (toCheck.Count > 0) {
					Tile x = toCheck.Dequeue();
					segment.AddTile(x, player);

					if (x.cityAtTile != null) {
						segments[x.cityAtTile] = segment;
					}

					foreach (Tile n in x.neighbors.Values) {
						if (n.overlays.HasRoad() && !seen.Contains(n)) {
							seen.Add(n);
							toCheck.Enqueue(n);
						}
					}
				}
			}
		}

		public bool HasTradeAccess(Tile t, Resource r) {
			foreach (TradeNetworkSegment segment in segments.Values) {
				if (segment.tiles.Contains(t) && segment.resourceCounts.TryGetValue(r, out int count) && count > 0) {
					return true;
				}
			}
			return false;
		}
	}
}
