using System.Collections.Generic;
using System.Linq;

namespace C7GameData.Save {
	public class SaveBuilding {
		public const string IS_CENTER_OF_EMPIRE = "isCenterOfEmpire";

		public string name;
		public int shieldCost;
		public int populationCost;
		public ID requiredTech;
		public string requiredBuilding;
		public bool isGreatWonder;
		public bool isSmallWonder;
		public int culturePerTurn;

		// Assorted boolean flags for the building. They're stored in this set
		// rather than as booleans to avoid bloating the json file.
		public HashSet<string> flags = new();

		public HashSet<string> requiredResources = [];

		public SaveBuilding() { }

		public SaveBuilding(Building b) {
			(name, shieldCost, populationCost, isGreatWonder, isSmallWonder, culturePerTurn) =
			(b.name, b.shieldCost, b.populationCost, b.isGreatWonder, b.isSmallWonder, b.culturePerTurn);

			if (b.isCenterOfEmpire) {
				flags.Add(IS_CENTER_OF_EMPIRE);
			}

			if (b.requiredTech != null)
				requiredTech = b.requiredTech.id;

			if (b.requiredBuilding != null)
				requiredBuilding = b.requiredBuilding.name;

			requiredResources = b.requiredResources.Select(r => r.Key).ToHashSet();
		}
	}
}
