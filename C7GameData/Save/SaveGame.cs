using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace C7GameData.Save {

	public static class TypeInfoResolver {
		public static void IgnoreDefaultValues(JsonTypeInfo jsonTypeInfo) {
			foreach (JsonPropertyInfo pi in jsonTypeInfo.Properties) {
				if (pi.PropertyType == typeof(string)) {
					pi.ShouldSerialize = (_, value) => ((string)value)?.Length > 0;
				} else if (typeof(ICollection).IsAssignableFrom(pi.PropertyType)) {
					pi.ShouldSerialize = (_, value) => ((ICollection)value)?.Count > 0;
				} else if (typeof(IEnumerable).IsAssignableFrom(pi.PropertyType)) {
					pi.ShouldSerialize = (_, value) => value is not null && ((IEnumerable)value).GetEnumerator().MoveNext();
				} else {
					pi.ShouldSerialize = (_, value) => value is not null;
				}
			}
		}
	}

	public class SaveGame {

		private static JsonSerializerOptions JsonOptions {
			get => new JsonSerializerOptions {
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
				// Pretty print during development; may change this for production
				WriteIndented = true,
				// By default it only serializes getters, this makes it serialize fields, too
				IncludeFields = true,
				Converters = {
					new Json2DArrayConverter(),
					new IDJsonConverter(),
					new JsonStringEnumConverter(JsonNamingPolicy.CamelCase),
				},
				TypeInfoResolver = new DefaultJsonTypeInfoResolver {
					Modifiers = { TypeInfoResolver.IgnoreDefaultValues },
				},
			};
		}

		public SaveGame() { }

		public static SaveGame FromGameData(GameData data) {
			SaveGame save = new SaveGame{
				Seed = data.seed,
				TurnNumber = data.turn,
				Civilizations = data.civilizations,
				Map = new SaveMap(data.map),
				TerrainTypes = data.terrainTypes,
				Resources = data.Resources,
				Buildings = data.Buildings.ConvertAll(building => new SaveBuilding(building)),
				BarbarianInfo = data.barbarianInfo,
				Units = data.mapUnits.ConvertAll(unit => new SaveUnit(unit, data.map)),
				UnitPrototypes = data.unitPrototypes.ConvertAll(proto => new SaveUnitPrototype(proto)),
				Players = data.players.ConvertAll(player => new SavePlayer(player)),
				Cities = data.cities.ConvertAll(city => new SaveCity(city)),
				ExperienceLevels = data.experienceLevels,
			};
			save.StrengthBonuses.Add(data.fortificationBonus);
			save.StrengthBonuses.Add(data.riverCrossingBonus);
			save.StrengthBonuses.Add(data.cityLevel1DefenseBonus);
			save.StrengthBonuses.Add(data.cityLevel2DefenseBonus);
			save.StrengthBonuses.Add(data.cityLevel3DefenseBonus);
			save.HealRates["friendly_field"] = data.healRateInFriendlyField;
			save.HealRates["neutral_field"] = data.healRateInNeutralField;
			save.HealRates["hostile_field"] = data.healRateInHostileField;
			save.HealRates["city"] = data.healRateInCity;
			save.ScenarioSearchPath = data.scenarioSearchPath;
			save.DefaultExperienceLevel = data.defaultExperienceLevelKey;
			save.Techs = data.techs.ConvertAll(t => t.ToSaveTech());
			save.CitizenTypes = data.citizenTypes;
			save.TerraForms = data.Terraforms;
			save.Governments = data.governments;
			return save;
		}

		private void populateGameDataTileUnitsAndCities(GameData data) {
			foreach (Tile tile in data.map.tiles) {
				tile.unitsOnTile = data.mapUnits.Where(unit => unit.location == tile).ToList();
				tile.cityAtTile = data.cities.Find(city => city.location == tile);
			}
		}

		public GameData ToGameData(Action<City, CitizenType> assignScenarioResidents) {
			GameData data = InitializeGameData();
			ConvertMapAndPlayers(data);
			ConvertTechnologies(data);
			ConvertBuildings(data);
			ConvertUnits(data);
			ConvertCities(data, assignScenarioResidents);
			ConvertBarbarianInfo(data);
			ConvertStrengthBonuses(data);
			ConvertHealRates(data);

			data.defaultExperienceLevelKey = DefaultExperienceLevel;
			data.defaultExperienceLevel = data.experienceLevels.Find(el => el.key == DefaultExperienceLevel);

			data.UpdateTileOwners();

			return data;
		}

		private GameData InitializeGameData() {
			// copy data without references
			return new GameData {
				seed = Seed,
				turn = TurnNumber,
				terrainTypes = TerrainTypes,
				Resources = Resources,
				scenarioSearchPath = ScenarioSearchPath,
				civilizations = Civilizations,
				citizenTypes = CitizenTypes,
				Terraforms = TerraForms,
				governments = Governments,
				ids = new ID.Factory(this),
				experienceLevels = ExperienceLevels,
			};
		}

		private void ConvertMapAndPlayers(GameData data) {
			// units and cities are empty
			data.map = Map.ToGameMap(data);

			// players need game map to populate tile knowledge
			data.players = Players.ConvertAll(player => player.ToPlayer(data.map, Civilizations));
		}

		private void ConvertTechnologies(GameData data) {
			// Fill in the list of techs and then backfill the prereqs.
			//
			// This is an N^2 approach, but doing a topological sort of the
			// prereqs feels like overkill given how many techs are in a game.

			foreach (SaveTech st in this.Techs) {
				data.techs.Add(st.ToTechWithoutPrereqs());
			}

			foreach (Tech t in data.techs) {
				t.FillInPrereqs(this.Techs, data.techs);
			}
		}

		private void ConvertBuildings(GameData data) {
			data.Buildings = Buildings.ConvertAll(building => new Building(building));

			var buildingDict = data.Buildings.ToDictionary(b => b.name);
			var techDict = data.techs.ToDictionary(t => t.id);

			foreach (SaveBuilding saveBuilding in Buildings) {
				Building building = buildingDict[saveBuilding.name];

				if (saveBuilding.requiredBuilding != null) {
					building.requiredBuilding = buildingDict[saveBuilding.requiredBuilding];
				}

				if (saveBuilding.requiredTech != null) {
					building.requiredTech = techDict[saveBuilding.requiredTech];
				}
			}
		}

		private void ConvertUnits(GameData data) {
			data.unitPrototypes = UnitPrototypes.ConvertAll(prototype => new UnitPrototype(prototype));

			var techDict = data.techs.ToDictionary(t => t.id);
			var unitPrototypeDict = data.unitPrototypes.ToDictionary(b => b.name);
			var civDict = data.civilizations.ToDictionary(c => c.name);

			foreach (SaveUnitPrototype saveProto in UnitPrototypes) {
				UnitPrototype proto = unitPrototypeDict[saveProto.name];

				if (saveProto.upgradeTo != null) {
					proto.upgradeTo = unitPrototypeDict[saveProto.upgradeTo];
				}

				if (saveProto.requiredTech != null) {
					proto.requiredTech = techDict[saveProto.requiredTech];
				}

				if (saveProto.unique != null) {
					Civilization civ = civDict[saveProto.unique.civilization];

					proto.unique = new() {
						civilization = civ
					};

					if (saveProto.unique.replace != null) {
						proto.unique.replace = unitPrototypeDict[saveProto.unique.replace];
					}

					civ.uniqueUnit = proto;
				}
			}

			// map units need game map and players to populate location and owner
			data.mapUnits = Units.ConvertAll(unit =>
				unit.ToMapUnit(data.unitPrototypes, ExperienceLevels, data.players, data.Terraforms, data.map)
			);


			// once unit owners are known, players can reference units
			data.players.ForEach(player => {
				player.units = data.mapUnits.Where(unit => unit.owner.id == player.id).ToList();
			});
		}

		private void ConvertCities(GameData data, Action<City, CitizenType> assignScenarioResidents) {
			// cities require game map for location and players for city owner
			data.cities = Cities.ConvertAll(city =>
				city.ToCity(data.map, data.players, data.unitPrototypes, Civilizations, data.Buildings, CitizenTypes, assignScenarioResidents)
			);

			// add references to map tiles after units and cities are defined
			populateGameDataTileUnitsAndCities(data);

			// Once cities are known, players can reference cities.
			data.players.ForEach(player => {
				player.cities = data.cities.Where(city => city.owner.id == player.id).ToList();
			});

			foreach (City city in data.cities) {
				data.map.tileAt(city.location.XCoordinate, city.location.YCoordinate).cityAtTile = city;
			}
		}

		private void ConvertBarbarianInfo(GameData data) {
			data.barbarianInfo = BarbarianInfo;

			if (BarbarianInfo.basicBarbarianIndex != -1) {
				data.barbarianInfo.basicBarbarian = data.unitPrototypes[data.barbarianInfo.basicBarbarianIndex];
			}
			if (BarbarianInfo.advancedBarbarianIndex != -1) {
				data.barbarianInfo.advancedBarbarian = data.unitPrototypes[data.barbarianInfo.advancedBarbarianIndex];
			}
			if (BarbarianInfo.barbarianSeaUnitIndex != -1) {
				data.barbarianInfo.barbarianSeaUnit = data.unitPrototypes[data.barbarianInfo.barbarianSeaUnitIndex];
			}
		}

		private void ConvertStrengthBonuses(GameData data) {
			foreach (StrengthBonus sb in StrengthBonuses) {
				switch (sb.description) {
					case "Fortified":
						data.fortificationBonus = sb;
						break;
					case "Behind river":
						data.riverCrossingBonus = sb;
						break;
					case "Town":
						data.cityLevel1DefenseBonus = sb;
						break;
					case "City":
						data.cityLevel2DefenseBonus = sb;
						break;
					case "Metropolis":
						data.cityLevel3DefenseBonus = sb;
						break;
				}
			}
		}

		private void ConvertHealRates(GameData data) {
			data.healRateInFriendlyField = HealRates["friendly_field"];
			data.healRateInNeutralField = HealRates["neutral_field"];
			data.healRateInHostileField = HealRates["hostile_field"];
			data.healRateInCity = HealRates["city"];
		}

		public string Version = "0.0.0";
		public int Seed = -1;
		public int TurnNumber = 0;
		public SaveMap Map = new SaveMap();
		public List<TerrainType> TerrainTypes = new List<TerrainType>();
		public List<Resource> Resources = new List<Resource>();
		public List<SaveUnit> Units = new List<SaveUnit>();
		public List<SaveUnitPrototype> UnitPrototypes = [];
		public List<SaveBuilding> Buildings = [];
		public List<SavePlayer> Players = new List<SavePlayer>();
		public List<SaveCity> Cities = new List<SaveCity>();
		public BarbarianInfo BarbarianInfo = new BarbarianInfo();
		public List<ExperienceLevel> ExperienceLevels = new List<ExperienceLevel>();
		public string DefaultExperienceLevel; // key
		public List<Civilization> Civilizations = new List<Civilization>();
		public List<StrengthBonus> StrengthBonuses = new List<StrengthBonus>();
		public Dictionary<string, int> HealRates = new Dictionary<string, int>();
		public List<SaveTech> Techs = new();
		public List<CitizenType> CitizenTypes = new();
		public List<Terraform> TerraForms = new();
		public List<Government> Governments = new();
		public string ScenarioSearchPath; // TODO: what is this
		public void Save(string path) {
			byte[] json = JsonSerializer.SerializeToUtf8Bytes(this, JsonOptions);
			File.WriteAllBytes(path, json);
		}

		public static SaveGame Load(string path) {
			return JsonSerializer.Deserialize<SaveGame>(File.ReadAllText(path), JsonOptions);
		}
	}
}
