using System.Collections.Generic;
using System.Linq;

namespace C7GameData.Save {
	public class SaveUnitPrototype {
		public string name { get; set; }
		public Art art { get; set; }
		public int shieldCost { get; set; }
		public int populationCost { get; set; }
		public ID requiredTech { get; set; }
		public int attack { get; set; }
		public int defense { get; set; }
		public int bombard { get; set; }
		public int movement { get; set; }

		public HashSet<string> producibleBy = [];

		public string upgradeTo;
		public bool unproducible;
		public bool rotateBeforeAttack { get; set; }

		public HashSet<string> categories = new HashSet<string>();

		public HashSet<UnitAction> actions = [];

		public HashSet<string> attributes = new HashSet<string>();

		public HashSet<string> requiredResources = [];

		public HashSet<ID> terraformActions = [];

		public SaveUnitPrototype() { }

		public SaveUnitPrototype(UnitPrototype proto) {
			(name, art, shieldCost, populationCost, unproducible, rotateBeforeAttack,
			attack, defense, bombard, movement) =
			(proto.name, proto.art, proto.shieldCost, proto.populationCost, proto.unproducible, proto.rotateBeforeAttack,
			 proto.attack, proto.defense, proto.bombard, proto.movement);

			if (proto.requiredTech != null)
				requiredTech = proto.requiredTech.id;

			if (proto.upgradeTo != null)
				upgradeTo = proto.upgradeTo.name;

			categories = new HashSet<string>(proto.categories);
			actions = proto.actions;
			attributes = new HashSet<string>(proto.attributes);

			requiredResources = proto.requiredResources.Select(r => r.Key).ToHashSet();
			terraformActions = proto.terraformActions.Select(r => r.Id).ToHashSet();
			producibleBy = proto.producibleBy.Select(r => r.name).ToHashSet();
		}
	}
}
