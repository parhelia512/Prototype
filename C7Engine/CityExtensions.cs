namespace C7Engine {

	using System.Linq;
	using System.Collections.Generic;
	using C7GameData;
	using C7Engine.Pathing;

	public static class CityExtensions {
		public static IEnumerable<IProducible> ListProductionOptions(this City city) {
			return EngineStorage.gameData.unitPrototypes.Where(u => city.CanBuildUnit(u));
		}

		private static Dictionary<Resource, int> ListResourceAccess(this City city, ResourceCategory category, GameData gd) {
			PathingAlgorithm pathing = PathingAlgorithmChooser.GetTradeNetworkAlgorithm();

			return gd.map.tiles
				.Where(t => t.OwningPlayer() == city.owner && t.Resource?.Category == category)
				.GroupBy(t => t.Resource)
				.Select(tg => (tg.Key, tg.Where(t => pathing.PathFrom(city.location, t).path.Count > 0).Count()))
				.Where(rc => rc.Item2 > 0)
				.ToDictionary();
		}

		public static Dictionary<Resource, int> GetStrategicResources(this City city, GameData gd) {
			return ListResourceAccess(city, ResourceCategory.STRATEGIC, gd);
		}

		public static Dictionary<Resource, int> GetLuxuries(this City city, GameData gd) {
			return ListResourceAccess(city, ResourceCategory.LUXURY, gd);
		}
	}
}
