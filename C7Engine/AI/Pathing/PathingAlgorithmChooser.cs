using C7GameData;

namespace C7Engine.Pathing {
	/**
	 * Returns a pathing algorithm to use.
	 * Eventually, this will depend on some map considerations.
	 * For now, just return the first one.
	 */
	public class PathingAlgorithmChooser {
		private static PathingAlgorithm landAlgorithm = new AStarAlgorithm(new WalkerOnLand(), (Tile from, Tile to) => {
			return from.distanceTo(to);
		});
		private static PathingAlgorithm waterAlgorithm = new AStarAlgorithm(new WalkerOnWater(), (Tile from, Tile to) => {
			return from.distanceTo(to);
		});

		public static PathingAlgorithm GetAlgorithm(bool isLandUnit) {
			return isLandUnit ? landAlgorithm : waterAlgorithm;
		}
	}
}
