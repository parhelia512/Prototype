using System.Linq;
using C7Engine.AI;

namespace C7Engine {
	using System;
	using C7GameData;

	public class CityInteractions {
		public static City BuildCity(int X, int Y, ID playerID, string name) {
			GameData gameData = EngineStorage.gameData;
			Player owner = gameData.GetPlayer(playerID);
			Tile tileWithNewCity = gameData.map.tileAt(X, Y);
			City newCity = new City(tileWithNewCity, owner, name, gameData.ids.CreateID("city"));
			if (owner.cities.Count == 0) {
				newCity.capital = true;
				newCity.AddBuilding(gameData.Buildings.Find(x => x.isCenterOfEmpire));
			}
			gameData.cities.Add(newCity);
			owner.cities.Add(newCity);
			tileWithNewCity.cityAtTile = newCity;

			CityResident firstResident = new CityResident();
			firstResident.city = newCity;
			firstResident.citizenType = gameData.citizenTypes.Find(x => x.IsDefaultCitizen);
			newCity.AddCitizen(firstResident);

			// Update owners before we assign the citizen so the tile owners are
			// accurate. We do this after adding the resident though, because
			// cities with zero residents are considered destroyed.
			gameData.UpdateTileOwners();
			CityTileAssignmentAI.AssignNewCitizenToTile(firstResident);

			newCity.SetItemBeingProduced(CityProductionAI.GetNextItemToBeProduced(newCity, null));

			// Redo corruption calculations after a city is created, since it
			// may change rank corruption values.
			owner.DoCorruptionCalculations(EngineStorage.gameData);

			return newCity;
		}

		public static void DestroyCity(int X, int Y) {
			Tile tile = EngineStorage.gameData.map.tileAt(X, Y);
			tile.DisbandNonDefendingUnits();
			Player owner = tile.cityAtTile.owner;
			tile.cityAtTile.RemoveAllCitizens();
			tile.cityAtTile.owner.cities.Remove(tile.cityAtTile);
			EngineStorage.gameData.cities.Remove(tile.cityAtTile);
			EngineStorage.gameData.UpdateTileOwnersOnCityDestruction(tile.cityAtTile);
			new MsgCityDestroyed(tile.cityAtTile).send();
			if (EngineStorage.gameData.CheckForCivDestruction(tile.cityAtTile.owner)) {
				// Let the UI know about the civ destruction.
				new MsgCivilizationDestroyed(tile.cityAtTile.owner.civilization).send();
			}
			tile.cityAtTile = null;

			// Redo corruption calculations after a city is destroyed, since it
			// may change rank corruption values.
			owner.DoCorruptionCalculations(EngineStorage.gameData);
		}
	}
}
