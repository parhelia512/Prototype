using System.Collections.Generic;

namespace C7GameData {
	using System;
	using C7GameData.Save;

	/**
	 * The prototype for a unit, which defines the characteristics of a unit.
	 * For example, a Spearman might have 1 attack, 2 defense, and 1 movement.
	 **/
	public class UnitPrototype : IProducible {
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

		public HashSet<string> categories = new HashSet<string>();

		public HashSet<string> actions = new HashSet<string>();

		public HashSet<string> attributes = new HashSet<string>();

		public UnitPrototype() { }

		public UnitPrototype(SaveUnitPrototype proto, Tech requiredTech) {
			(name, artName, shieldCost, populationCost, attack, defense, bombard, movement, iconIndex) =
			(proto.name, proto.artName, proto.shieldCost, proto.populationCost,
			 proto.attack, proto.defense, proto.bombard, proto.movement, proto.iconIndex);

			this.requiredTech = requiredTech;

			categories = new HashSet<string>(proto.categories);
			actions = new HashSet<string>(proto.actions);
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
	}
}
