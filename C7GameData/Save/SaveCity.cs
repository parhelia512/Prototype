using System.Collections.Generic;

namespace C7GameData.Save {
	public class SaveCityResident {
		public string nationality;
		public TileLocation tileWorked;
	}

	public class SaveCity {
		public ID id;
		public ID owner;
		public bool capital;
		public TileLocation location;
		public string producible;
		public string name;
		public int size;
		public int shieldsStored;
		public int foodStored;
		public int foodNeededToGrow;
		public List<SaveCityResident> residents = new List<SaveCityResident>();

		public SaveCity() { }

		public SaveCity(City city) {
			id = city.id;
			owner = city.owner.Id;
			capital = city.capital;
			location = new TileLocation(city.location);
			name = city.name;
			size = city.size;
			producible = city.itemBeingProduced.name;
			shieldsStored = city.shieldsStored;
			foodStored = city.foodStored;
			foodNeededToGrow = city.foodNeededToGrow;
			residents = city.residents.ConvertAll(resident => {
				return new SaveCityResident {
					nationality = resident.nationality?.Name,
					tileWorked = new TileLocation(resident.tileWorked),
				};
			});
		}

		public City ToCity(GameMap gameMap, List<Player> players, List<UnitPrototype> unitPrototypes, List<Civilization> civilizations) {
			City city = new City{
				id = id,
				location = gameMap.tileAt(location.x, location.y),
				owner = players.Find(p => p.Id == owner),
				name = name,
				size = size,
				itemBeingProduced = unitPrototypes.Find(proto => proto.name == producible),
				shieldsStored = shieldsStored,
				foodStored = foodStored,
				foodNeededToGrow = foodNeededToGrow,
				capital = capital,
				residents = residents.ConvertAll(resident =>{
					return new CityResident{
						tileWorked = gameMap.tileAt(resident.tileWorked.x, resident.tileWorked.y),
						nationality = civilizations.Find(civ => civ.Name == resident.nationality),
					};
				}),
			};
			return city;
		}
	}
}
