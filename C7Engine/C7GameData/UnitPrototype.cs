using System.Collections.Generic;

namespace C7GameData {
	using System;
	using System.Linq;
	using C7Engine;
	using C7GameData.Save;

	public enum UnitAction {
		BuildCity,
		Bombard,
		Hold,
		Wait,
		Fortify,
		Disband,
		Goto,
		Explore,
		Automate
	}

	public struct ItemContext(UnitPrototype proto, Player player) {
		public UnitPrototype proto = proto;
		public Player player = player;
	}

	// A container for all the art for this unit
	public struct Art {
		public MainArt mainArt;
		public ThumbNailArt thumbnailArt;
		public PediaArt pediaArt;
	}

	// the main art contains the Animation art folder path for the unit
	public struct MainArt {
		public string defaultName;
		public Dictionary<string, string> variations;
	}
	// the thumbnail art contains the index to the small unit icons used in city screen production, etc
	public struct ThumbNailArt {
		public int defaultIndex;
		public Dictionary<string, int> variations;
	}
	// the icons being used by the civilopedia and also other places like the Science Advisor
	public struct PediaArt {
		public string small;
		public string large;
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
		public Art art { get; set; }
		public int shieldCost { get; set; }
		public int populationCost { get; set; }
		public Tech requiredTech { get; set; }
		public int attack { get; set; }
		public int defense { get; set; }
		public int bombard { get; set; }
		public int movement { get; set; }
		public UnitPrototype upgradeTo;
		public Unique unique;
		public bool unproducible;

		public HashSet<string> categories = new HashSet<string>();

		public HashSet<UnitAction> actions = [];

		public HashSet<string> attributes = new HashSet<string>();

		public HashSet<Resource> requiredResources { get; set; } = [];

		public HashSet<Terraform> terraformActions = [];
		public bool isWorker => terraformActions.Count > 0;

		public UnitPrototype() { }

		public UnitPrototype(SaveUnitPrototype proto, IEnumerable<Terraform> terraforms) {
			(name, art, shieldCost, populationCost, attack, defense, bombard, movement, unproducible) =
			(proto.name, proto.art, proto.shieldCost, proto.populationCost,
			 proto.attack, proto.defense, proto.bombard, proto.movement, proto.unproducible);

			categories = new HashSet<string>(proto.categories);
			actions = proto.actions;
			attributes = new HashSet<string>(proto.attributes);

			terraformActions = proto.terraformActions.Select(id => terraforms.First(t => t.Id == id)).ToHashSet();
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

		public MapUnit GetInstance(ID id, UnitPrototype proto, Player owner, Civilization nationality = null, Tile location = null, TileDirection facingDirection = TileDirection.SOUTHWEST, int hitPoints = 3) {
			MapUnit instance = new MapUnit(id);
			instance.unitType = proto;
			instance.name = proto.name;
			instance.hitPointsRemaining = hitPoints;    //todo: make this configurable
			instance.owner = owner;
			if (nationality != null)
				instance.nationality = nationality;
			else
				instance.nationality = owner.civilization;
			instance.location = location;

			instance.movementPoints.reset(movement);
			return instance;
		}

		public bool CanProduce(City city, HashSet<Resource> accessibleResources) {
			return MeetsProductionRequirements(city, accessibleResources) && !IsUnitObsolete(city, accessibleResources);
		}

		public int ShieldCost(HashSet<Civilization.Trait> civTraits, float costFactor) {
			return (int)(shieldCost * costFactor);
		}

		public bool IsLandUnit() {
			return categories.Contains("Land");
		}

		public bool IsSeaUnit() {
			return categories.Contains("Sea");
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
}
