using System.Collections.Generic;

namespace C7GameData.Save {
	public class SaveUnitPrototype {
		public string name { get; set; }
		public string artName { get; set; }
		public int shieldCost { get; set; }
		public int populationCost { get; set; }
		public ID requiredTech { get; set; }
		public int attack { get; set; }
		public int defense { get; set; }
		public int bombard { get; set; }
		public int movement { get; set; }
		public int iconIndex { get; set; }

		public HashSet<string> categories = new HashSet<string>();

		public HashSet<string> actions = new HashSet<string>();

		public HashSet<string> attributes = new HashSet<string>();

		public SaveUnitPrototype() { }

		public SaveUnitPrototype(UnitPrototype proto) {
			(name, artName, shieldCost, populationCost,
			attack, defense, bombard, movement, iconIndex) =
			(proto.name, proto.artName, proto.shieldCost, proto.populationCost,
			 proto.attack, proto.defense, proto.bombard, proto.movement, proto.iconIndex);

			if (proto.requiredTech != null)
				requiredTech = proto.requiredTech.id;

			categories = new HashSet<string>(proto.categories);
			actions = new HashSet<string>(proto.actions);
			attributes = new HashSet<string>(proto.attributes);
		}
	}
}