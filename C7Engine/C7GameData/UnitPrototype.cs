using System.Collections.Generic;

namespace C7GameData {
	using System;
	using System.Linq;
	using C7Engine;
	using C7GameData.Save;

	public enum UnitAction {
		BuildCity,
		BuildRoad,
		BuildRailroad,
		BuildMine,
		BuildFortress,
		ClearDamage,
		BuildAirfield,
		BuildRadarTower,
		BuildOutpost,
		BuildBarricade,
		Irrigate,
		ClearWetlands,
		ClearForest,
		PlantForest,
		Bombard,
		Hold,
		Wait,
		Fortify,
		Disband,
		Goto,
		Explore,
		Automate
	}

	/**
	 * The prototype for a unit, which defines the characteristics of a unit.
	 * For example, a Spearman might have 1 attack, 2 defense, and 1 movement.
	 **/
	public class UnitPrototype : IProducible {
		public class Unique {
			public Civilization civilization;
			public UnitPrototype replace;
		}

		public string name { get; set; }
		// The name to use when searching for animations for this unit.
		public string artName { get; set; }
		public int shieldCost { get; set; }
		public int populationCost { get; set; }
		public Tech requiredTech { get; set; }
		public int attack { get; set; }
		public int defense { get; set; }
		public int bombard { get; set; }
		public int movement { get; set; }
		public int iconIndex { get; set; }
		public UnitPrototype upgradeTo;
		public Unique unique;
		public bool unproducible;

		public HashSet<string> categories = new HashSet<string>();

		public HashSet<UnitAction> actions = [];

		public HashSet<string> attributes = new HashSet<string>();

		public HashSet<Resource> requiredResources { get; set; } = [];

		public UnitPrototype() { }

		public UnitPrototype(SaveUnitPrototype proto) {
			(name, artName, shieldCost, populationCost, attack, defense, bombard, movement, iconIndex, unproducible) =
			(proto.name, proto.artName, proto.shieldCost, proto.populationCost,
			 proto.attack, proto.defense, proto.bombard, proto.movement, proto.iconIndex, proto.unproducible);

			categories = new HashSet<string>(proto.categories);
			actions = proto.actions;
			attributes = new HashSet<string>(proto.attributes);
		}

		public override string ToString() {
			return $"{name} ({attack}/{defense}/{movement})";
		}

		public double BaseStrength(CombatRole role) {
			switch (role) {
				case CombatRole.Attack: return attack;
				case CombatRole.Defense: return defense;
				case CombatRole.Bombard: return bombard;
				case CombatRole.BombardDefense: return defense;
				case CombatRole.DefensiveBombard: return bombard;
				case CombatRole.DefensiveBombardDefense: return defense;
				default: throw new ArgumentOutOfRangeException("Invalid CombatRole");
			}
		}

		public MapUnit GetInstance(GameData gameData) {
			MapUnit instance = new MapUnit(gameData.ids.CreateID(this.name));
			instance.unitType = this;
			instance.hitPointsRemaining = 3;    //todo: make this configurable
			instance.movementPoints.reset(movement);
			return instance;
		}

		public bool CanProduce(City city, HashSet<Resource> accessibleResources) {
			return MeetsProductionRequirements(city, accessibleResources) && !IsUnitObsolete(city, accessibleResources);
		}

		// TODO: Consider golden ages when determining whether a unit is obsolete.
		// If a golden age has not yet been triggered and a unit can trigger one,
		// it shouldn't be marked as obsolete, even if its upgrade is available.
		private bool IsUnitObsolete(City city, HashSet<Resource> accessibleResources) {
			List<UnitPrototype> upgradeChain = city.owner.civilization.GetUpgradeChain(this);

			return upgradeChain.Any(upgrade => upgrade.MeetsProductionRequirements(city, accessibleResources));
		}

		private bool MeetsProductionRequirements(City city, HashSet<Resource> accessibleResources) {
			if (!city.owner.civilization.IsUnitAvailable(this)) {
				return false;
			}

			if (!city.owner.HasRequiredTechnology(this)) {
				return false;
			}

			if (categories.Contains("Sea") && !city.location.NeighborsWater()) {
				return false;
			}

			if (!requiredResources.All(accessibleResources.Contains)) {
				return false;
			}

			return true;
		}
	}

	public static class UnitActionExtension {
		public static Terraform? ToTerraform(this UnitAction action) {
			return EngineStorage.gameData.Terraforms.Find(t => t.Action == action);
		}
	}
}
