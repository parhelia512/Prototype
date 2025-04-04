using System.Collections.Generic;
using C7GameData.Save;

namespace C7GameData {
	// The in-game representation of a tech that can be researched.
	public class Tech {
		public ID id;
		public string Name { get; set; }
		public string CivilopediaEntry { get; set; }
		public int Cost;
		public bool RequiredForEraAdvancement;

		// The civilopedia name of the era this tech is part of
		// (like ERA_Ancient_Times). This is what art lookups are based on.
		public string EraCivilopediaName { get; set; }

		// The path, like "Art\tech chooser\Icons\39-Mapmaking-small.pcx", of
		// the small icon for this tech.
		public string SmallIconPath;

		// The position of this tech within the tech advisor UI.
		public int X;
		public int Y;

		public List<Tech> Prerequisites = new();

		public int TechCostFor(Player player) {
			// Cost formula from https://forums.civfanatics.com/threads/research-cost-formula-v1-29f.29485/.
			// Research Cost = [MM * [10*COST * (1 - N/[CL*1.75])]/(CF * 10)] - progress
			//
			// MM = map modifier (tiny=160, small=200, standard=240, large=320, huge=400)
			// COST = tech cost
			// CF = difficulty factor, range 10 (easy) to 6 (hard)
			// N = number of known civs that have discovered the tech
			// CL = civs left in game
			//
			// We also have the min/max turns to research of 4 and 50.
			// TODO: the min/max costs are in the biq, we should load them.
			// TODO: implement the civ-related parts of the equation
			// TODO: figure out what map size we are
			// TODO: See this this whole equation can be configurable
			int mapModifier = 160;  // small, to make testing faster
			int difficultyFactor = 10; // easy difficulty
			int researchCost = mapModifier * 10 * Cost / (difficultyFactor * 10);

			return researchCost;
		}

		public C7GameData.Save.SaveTech ToSaveTech() {
			C7GameData.Save.SaveTech result = new() {
				id = this.id,
				Name = this.Name,
				CivilopediaEntry = this.CivilopediaEntry,
				Cost = this.Cost,
				RequiredForEraAdvancement = this.RequiredForEraAdvancement,
				EraCivilopediaName = this.EraCivilopediaName,
				SmallIconPath = this.SmallIconPath,
				X = this.X,
				Y = this.Y
			};

			foreach (Tech t in Prerequisites) {
				result.Prerequisites.Add(t.id);
			}

			return result;
		}

		public void FillInPrereqs(List<SaveTech> saveTechs, List<Tech> techs) {
			SaveTech st = saveTechs.Find(st => st.id == this.id);

			foreach (ID prereq in st.Prerequisites) {
				this.Prerequisites.Add(techs.Find(t => t.id == prereq));
			}
		}
	}
}
