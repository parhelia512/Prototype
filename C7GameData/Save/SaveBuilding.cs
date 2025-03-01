namespace C7GameData.Save {
	public class SaveBuilding {
		public string name;
		public int shieldCost;
		public int populationCost;
		public ID requiredTech;
		public string requiredBuilding;
		public bool isGreatWonder;
		public bool isSmallWonder;

		public SaveBuilding() { }

		public SaveBuilding(Building building) {
			(name, shieldCost, populationCost, isGreatWonder, isSmallWonder) =
			(building.name, building.shieldCost, building.populationCost, building.isGreatWonder, building.isSmallWonder);

			if (building.requiredTech != null)
				requiredTech = building.requiredTech.id;

			if (building.requiredBuilding != null)
				requiredBuilding = building.requiredBuilding.name;
		}
	}
}
