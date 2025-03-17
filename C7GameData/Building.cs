using System.Collections.Generic;
using C7GameData.Save;

namespace C7GameData {
	public class Building : IProducible {
		public string name { get; set; }
		public int shieldCost { get; set; }
		public int populationCost { get; set; } // Will always be equal to 0 in the Civ3 rule set
		public Tech requiredTech { get; set; }
		public Building requiredBuilding;
		public bool isGreatWonder;
		public bool isSmallWonder;

		public Building(SaveBuilding building) {
			name = building.name;
			shieldCost = building.shieldCost;
			populationCost = building.populationCost;
			isGreatWonder = building.isGreatWonder;
			isSmallWonder = building.isSmallWonder;
		}
	}
}
