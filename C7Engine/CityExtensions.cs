namespace C7Engine {

	using System.Linq;
	using System.Collections.Generic;
	using C7GameData;
	using C7Engine.Pathing;

	public static class CityExtensions {
		public static IEnumerable<IProducible> ListProductionOptions(this City city) {
			return EngineStorage.gameData.unitPrototypes.Where(city.CanBuildUnit);
		}

		private static Dictionary<Resource, int> ListResourceAccess(this City city, ResourceCategory category) {
			PathingAlgorithm pathing = PathingAlgorithmChooser.GetTradeNetworkAlgorithm();

			return city.owner.resourcesInBorders
				.Where(kv => kv.Key.Category == category && city.HasResourcePrerequisite(kv.Key))
				.Select(kv => (kv.Key, kv.Value.Where(t => city.HasTradeAccess(t, pathing)).Count()))
				.Where(rc => rc.Item2 > 0)
				.ToDictionary();
		}

		private static bool HasTradeAccess(this City city, Tile tile, PathingAlgorithm pathing) {
			return city.location == tile || pathing.PathFrom(city.location, tile).path.Count > 0;
		}

		private static bool HasResourcePrerequisite(this City city, Resource resource) {
			return resource.Prerequisite == null || city.owner.knownTechs.Contains(resource.Prerequisite);
		}

		public static bool HasResource(this City city, Resource resource) {
			PathingAlgorithm pathing = PathingAlgorithmChooser.GetTradeNetworkAlgorithm();

			if (!city.HasResourcePrerequisite(resource)) {
				return false;
			}

			if (city.owner.resourcesInBorders.TryGetValue(resource, out var resTiles)) {
				return resTiles.Where(t => city.HasTradeAccess(t, pathing)).Any();
			}

			return false;
		}

		public static Dictionary<Resource, int> GetStrategicResources(this City city) {
			return ListResourceAccess(city, ResourceCategory.STRATEGIC);
		}

		public static Dictionary<Resource, int> GetLuxuries(this City city) {
			return ListResourceAccess(city, ResourceCategory.LUXURY);
		}
	}
}
