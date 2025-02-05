using System.Collections.Generic;
using System.Linq;
using C7Engine.AI.StrategicAI;
// ReSharper disable ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator

namespace C7GameData {

	public class Player {
		// ReSharper disable once PropertyCanBeMadeInitOnly.Global
		public ID Id { get; internal set; }
		public int ColorIndex;
		public bool IsBarbarians = false;
		// TODO: Refactor front-end so it sends player GUID with requests.
		// We should allow multiple humans, this is a temporary measure.
		public bool IsHuman = false;
		public bool HasPlayedThisTurn = false;

		public Civilization Civilization;
		internal int CityNameIndex;

		public List<MapUnit> Units = new();
		public List<City> Cities = new();
		public TileKnowledge TileKnowledge = new();

		//Ordered list of priority data.  First is most important.
		// ReSharper disable once FieldCanBeMadeReadOnly.Global
		public List<StrategicPriority> StrategicPriorityData = new();

		// The list of techs known by this player.
		public HashSet<ID> KnownTechs = new();

		// The tech the player is currently researching.
		public ID CurrentlyResearchedTech;

		// The civilopedia name of the era this player is in.
		//
		// The civilopedia name is what is used for art lookups, not the actual
		// name.
		public string EraCivilopediaName;

		public int TurnsUntilPriorityReevaluation = 0;

		// The amount of gold this player has.
		public int Gold = 0;

		public void AddUnit(MapUnit unit) {
			Units.Add(unit);
		}

		public string GetNextCityName() {
			string name = Civilization.CityNames[CityNameIndex % Civilization.CityNames.Count];
			int bonusLoops = CityNameIndex / Civilization.CityNames.Count;
			if (bonusLoops % 2 == 1) {
				name = "New " + name;
			}
			int suffix = (bonusLoops / 2) + 1;
			if (suffix > 1) {
				name = name + " " + suffix; //e.g. for bonusLoops = 2, we'll have "Athens 2"
			}
			CityNameIndex++;
			return name;
		}

		public bool IsAtPeaceWith(Player other) {
			// Right now it's a free-for-all, but eventually we'll implement peace treaties and alliances
			return other == this;
		}

		public bool SitsOutFirstTurn() {
			// TODO: Scenarios can also specify that certain players sit out the first turn. E.g. WW2 in the Pacific
			return IsBarbarians;
		}

		// Once we have technologies, not all resources will be known at the start.
		// Eventually, perhaps there will be other gates around resource access as well
		// For now, just always return true, but have this method so we have that structure
		// in place.
		public bool KnowsAboutResource(Resource resource) {
			return true;
		}

		public int RemainingCities() {
			int result = 0;
			foreach (City city in Cities) {
				// Destroyed cities have a size of zero.
				if (city.size > 0) {
					++result;
				}
			}
			return result;
		}

		public override string ToString() {
			return Civilization != null ? Civilization.CityNames.First() : "";
		}
	}

}
