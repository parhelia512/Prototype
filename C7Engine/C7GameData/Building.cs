using System;
using System.Collections.Generic;
using System.Linq;
using C7GameData.Save;

namespace C7GameData {
	public static class BuildingRules {
		public static Dictionary<SaveBuilding.Flag, Func<City, bool>> productionRules = new()
		{
			{ SaveBuilding.Flag.MustBeCoastal, MustBeCoastal },
			{ SaveBuilding.Flag.MustBeNearRiver, MustBeNearRiver }
		};

		private static bool MustBeCoastal(City city) {
			return city.location.NeighborsWater();
		}

		private static bool MustBeNearRiver(City city) {
			return city.location.BordersRiver();
		}
	}

	public class Building : IProducible {
		public string name { get; set; }
		public int shieldCost { get; set; }
		public int populationCost { get; set; } // Will always be equal to 0 in the Civ3 rule set
		public Tech requiredTech { get; set; }
		public Building requiredBuilding;

		List<Func<City, bool>> productionPrerequisites = [];
		public bool isGreatWonder;
		public bool isSmallWonder;
		public bool isCenterOfEmpire;

		public int culturePerTurn = 0;

		public HashSet<Resource> requiredResources { get; set; } = [];

		SaveBuilding dataSource;

		public Building(SaveBuilding building) {
			name = building.name;
			shieldCost = building.shieldCost;
			populationCost = building.populationCost;
			isGreatWonder = building.isGreatWonder;
			isSmallWonder = building.isSmallWonder;
			culturePerTurn = building.culturePerTurn;
			isCenterOfEmpire = building.flags.Contains(SaveBuilding.Flag.IsCenterOfEmpire);
			dataSource = building;

			foreach (var kvp in BuildingRules.productionRules) {
				if (building.flags.Contains(kvp.Key)) {
					productionPrerequisites.Add(kvp.Value);
				}
			}

		}

		public bool CanProduce(City city, HashSet<Resource> accessibleResources) {
			if (!city.owner.HasRequiredTechnology(this)) {
				return false;
			}

			if (city.buildings.Exists(cityBuilding => cityBuilding.building == this)) {
				return false;
			}

			// TODO: Add logic for wonders and the palace
			if (isGreatWonder || isSmallWonder || isCenterOfEmpire) {
				return false;
			}

			if (requiredBuilding != null &&
				!city.buildings.Exists(cityBuilding => cityBuilding.building == this)) {
				return false;
			}

			if (!requiredResources.All(accessibleResources.Contains)) {
				return false;
			}

			if (!productionPrerequisites.All(func => func(city))) {
				return false;
			}

			return true;
		}

		public SaveBuilding ToSaveBuilding() {
			return dataSource;
		}
	}
}
