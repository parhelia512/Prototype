
using System.Collections.Generic;

namespace C7GameData {
	// A class representing one side of a diplomatic agreement between two civs.
	public class TradeOffer {
		public int? gold = null;
		public List<Tech> techs = new();

		// Calculate how much this trade offer is worth for a given player. This
		// has to be per-player because tech costs vary based on how many civs
		// a player have already researched the tech.
		public int GoldEquivalentFor(Player p) {
			int result = 0;
			if (gold.HasValue) {
				result += gold.Value;
			}
			foreach (Tech t in techs) {
				result += t.TechCostFor(p);
			}

			return result;
		}
	}
}
