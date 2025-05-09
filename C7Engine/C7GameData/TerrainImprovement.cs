using System.Collections.Generic;

namespace C7GameData {
	public class TerrainImprovement {
		public enum Layer {
			Roads,
			ResourceDevelopment, // mine, irrigation
			Holdings, // outpost, radar tower, fortress, barricade
		}

		private static Dictionary<string, TerrainImprovement> improvementByKey = [];

		public static readonly TerrainImprovement road = new(nameof(road), Layer.Roads, movementCost: 1.0f / 3);
		public static readonly TerrainImprovement railroad = new(nameof(railroad), Layer.Roads, road, movementCost: 0);
		public static readonly TerrainImprovement mine = new(nameof(mine), Layer.ResourceDevelopment);
		public static readonly TerrainImprovement irrigation = new(nameof(irrigation), Layer.ResourceDevelopment);
		public static readonly TerrainImprovement fortress = new(nameof(fortress), Layer.Holdings, defenseBonus: new(nameof(fortress), 0.5));
		public static readonly TerrainImprovement barricade = new(nameof(barricade), Layer.Holdings, fortress, defenseBonus: new(nameof(barricade), 1));

		public readonly string key;

		public readonly Layer layer;
		public readonly StrengthBonus? defenseBonus;

		// A terrain improvement with negative movement cost shouldn't affect the tile movement cost
		public readonly float movementCost = -1;

		// In the default ruleset, Road upgrades to Railroad and Fortress upgrades to Barricade.
		// The upgrade relationship affects:
		// 1) confirmation dialog when replacing improvements (if an improvement is replaced with its upgrade, there is no need for a confirmation)
		// 2) availability of an improvement (a player can't build an upgraded improvement without building a base improvement)
		// 3) result of pillaging (after pillaging, an upgraded improvement will downgrade)
		public readonly TerrainImprovement upgradesFrom;

		public TerrainImprovement(
			string key,
			Layer layer,
			TerrainImprovement upgradesFrom = null,
			StrengthBonus? defenseBonus = null,
			float movementCost = -1
		) {
			this.key = key;
			this.layer = layer;
			this.defenseBonus = defenseBonus;
			this.upgradesFrom = upgradesFrom;
			this.movementCost = movementCost;

			improvementByKey[key] = this;
		}

		public static TerrainImprovement FromKey(string key) {
			return improvementByKey[key];
		}
	}
}
