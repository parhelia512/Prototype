using System.Collections.Generic;

namespace C7GameData.Save {

	public class SavePlayer {
		public ID id;
		public int colorIndex;
		public bool barbarian;
		public bool human = false;
		public bool hasPlayedCurrentTurn = false;

		public string civilization;
		public int cityNameIndex = 0;

		public List<TileLocation> tileKnowledge = new List<TileLocation>();

		// The list of techs known by this player.
		public HashSet<ID> knownTechs = new();

		// The tech the player is currently researching.
		public ID currentlyResearchedTech;

		// The civilopedia name of the era this player is in.
		//
		// The civilopedia name is what is used for art lookups, not the actual
		// name.
		public string eraCivilopediaName;

		public int turnsUntilPriorityReevaluation = 0;

		// The amount of gold this player has.
		public int gold = 0;

		public Player ToPlayer(GameMap map, List<Civilization> civilizations) {
			Player player = new Player{
				Id = id,
				IsBarbarians = barbarian,
				IsHuman = human,
				HasPlayedThisTurn = hasPlayedCurrentTurn,
				ColorIndex = colorIndex,
				Civilization = civilization is not null ? civilizations.Find(civ => civ.Name == civilization) : null,
				CityNameIndex = cityNameIndex,
				TileKnowledge = new TileKnowledge(),
				KnownTechs = knownTechs,
				CurrentlyResearchedTech = currentlyResearchedTech,
				EraCivilopediaName = eraCivilopediaName,
				Gold = gold,
			};
			foreach (TileLocation tile in tileKnowledge) {
				player.TileKnowledge.AddTileToKnown(map.tileAt(tile.x, tile.y));
			}
			foreach (ID techId in player.Civilization.StartingTechs) {
				if (!player.KnownTechs.Contains(techId)) {
					player.KnownTechs.Add(techId);
				}
			}
			return player;
		}

		public SavePlayer() { }

		public SavePlayer(Player player) {
			id = player.Id;
			colorIndex = player.ColorIndex;
			barbarian = player.IsBarbarians;
			human = player.IsHuman;
			hasPlayedCurrentTurn = player.HasPlayedThisTurn;
			civilization = player.Civilization?.Name;
			// TODO: this should be computed by looking at cities defined in the save
			// so that adding cities in the save structure doesn't require updating this value
			cityNameIndex = player.CityNameIndex;
			tileKnowledge = player.TileKnowledge.AllKnownTiles().ConvertAll(tile => new TileLocation(tile));
			turnsUntilPriorityReevaluation = player.TurnsUntilPriorityReevaluation;
			knownTechs = player.KnownTechs;
			currentlyResearchedTech = player.CurrentlyResearchedTech;
			eraCivilopediaName = player.EraCivilopediaName;
			gold = player.Gold;
		}
	}
}
