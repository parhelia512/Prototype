using System;
using System.Collections.Generic;

namespace C7GameData {
	/**
	 * Represents a civilization, such as the French, which can be
	 * assigned to a player.
	 */
	public enum Gender {
		Male,
		Female,
	}

	public class Civilization {
		public Civilization() {}
		// ReSharper disable once UnusedMember.Global
		public Civilization(string name) {
			Name = name;
		}
		public string Name;
		// `noun` is "Americans" for "America", or "Spanish" for "Spain", etc.
		public string Noun;
		// ReSharper disable once NotAccessedField.Global
		public string Leader;
		public int ColorIndex;
		// ReSharper disable once NotAccessedField.Global
		public Gender LeaderGender;
		// ReSharper disable once FieldCanBeMadeReadOnly.Global
		public List<string> CityNames = new();
		// The IDs of all the techs that this civ starts with.
		// ReSharper disable once FieldCanBeMadeReadOnly.Global
		public HashSet<ID> StartingTechs = new();

		public class SettlerTileAdjustments {
			private const int DefaultDistancePenaltyRadius = 4;
			private const float DefaultCommerceYieldBonus = 2;
			private const float DefaultDistancePenalty = -2;
			private const float DefaultFoodYieldBonus = 5;
			private const float DefaultHillsBonus = 10;
			private const float DefaultLuxuryResourceBonus = 15;
			private const float DefaultProductionYieldBonus = 3;
			private const float DefaultStrategicResourceBonus = 20;
			private const float DefaultWaterBonus = 10;

			public int DistancePenaltyRadius = DefaultDistancePenaltyRadius;
			public Func<float, float> CommerceYieldBonus = yield => yield * DefaultCommerceYieldBonus;
			public Func<int, float> DistancePenalty = distance => distance * DefaultDistancePenalty;
			public Func<float, float> FoodYieldBonus = yield => yield * DefaultFoodYieldBonus;
			public float HillsBonus = DefaultHillsBonus;
			public float LuxuryResourceBonus = DefaultLuxuryResourceBonus;
			public Func<float, float> ProductionYieldBonus = yield => yield * DefaultProductionYieldBonus;
			public float StrategicResourceBonus = DefaultStrategicResourceBonus;
			public float WaterBonus = DefaultWaterBonus;
		}
		// ReSharper disable once FieldCanBeMadeReadOnly.Global
		public SettlerTileAdjustments Adjustments = new();
	}
}
