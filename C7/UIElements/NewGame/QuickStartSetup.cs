using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using C7GameData;
using C7Engine;
using C7Engine.Lua;
using Serilog;

public partial class QuickStartSetup : Node {
	private static ILogger log = LogManager.ForContext<ScenarioSetup>();

	public static void Init(GlobalSingleton globalState) {
		log.Information("Setting up a QuickStart game");

		globalState.ResetLoadGameFields();

		var save = GameModeLoader.Load(GamePaths.GameModesDir, GamePaths.GameMode);

		globalState.WorldCharacteristics = new WorldCharacteristics(save) {
			barbarianActivity = GetSetting("barbarianActivity", BarbarianActivity.Roaming),
			landform = GetSetting("landform", WorldCharacteristics.Landform.Pangaea),
			oceanCoverage = GetSetting("oceanCoverage", WorldCharacteristics.OceanCoverage.Percent_70),
			age = GetSetting("age", WorldCharacteristics.Age.Billion_4),
			climate = GetSetting("climate", WorldCharacteristics.Climate.Normal),
			temperature = GetSetting("temperature", WorldCharacteristics.Temperature.Temperate),
			worldSize = GetWorldSize(save.WorldSizes),
			mapSeed = new Random().Next(),
		};

		globalState.SaveGame = save;

		string lastCivilization = C7Settings.GetSettingsValueOrDefault("lastGame", "civilization", "Netherlands");
		Civilization player = save.Civilizations.FirstOrDefault(civ => civ.name == lastCivilization) ?? save.Civilizations[1];

		string lastDifficulty = C7Settings.GetSettingsValueOrDefault("lastGame", "difficulty", "Regent");
		Difficulty difficulty = save.Difficulties.FirstOrDefault(diff => diff.Name == lastDifficulty) ?? save.Difficulties[0];

		int numOpponents = globalState.WorldCharacteristics.worldSize.numberOfCivs - 1;
		List<SelectedOpponent> opponents = GetOpponents(numOpponents);

		GameSetup gameSetup = new() {
			playerCivilization = player,
			difficulty = difficulty,
			worldCharacteristics = globalState.WorldCharacteristics,
			opponents = opponents
		};

		gameSetup.Populate(save);
	}

	private static T GetSetting<T>(string key, T defaultValue) where T : struct, Enum {
		string value = C7Settings.GetSettingValue("lastGame", key);
		return Enum.TryParse(value, true, out T result) ? result : defaultValue;
	}

	private static WorldSize GetWorldSize(List<WorldSize> availableSizes) {
		string lastWorldSize = C7Settings.GetSettingValue("lastGame", "worldSize");
		return availableSizes.FirstOrDefault(ws =>
				   string.Equals(ws.name, lastWorldSize, StringComparison.OrdinalIgnoreCase))
			   ?? availableSizes.FirstOrDefault(ws => ws.isDefault)
			   ?? WorldSize.Generic();
	}

	private static List<SelectedOpponent> GetOpponents(int expectedCount) {
		string opsRaw = C7Settings.GetSettingsValueOrDefault("lastGame", "opponents", "");
		List<SelectedOpponent> opponents = [];

		if (!string.IsNullOrEmpty(opsRaw)) {
			foreach (string name in opsRaw
						 .Split(',').Select(n => n.Trim())
						 .Where(n => !string.IsNullOrEmpty(n))
					) {
				opponents.Add(name == "Random"
					? new SelectedOpponent { isRandom = true }
					: new SelectedOpponent { isRandom = false, Name = name });
			}
		}

		if (opponents.Count == 0) {
			for (int i = 0; i < expectedCount; i++) {
				opponents.Add(new SelectedOpponent { isRandom = true });
			}
		}

		return opponents.Take(expectedCount).ToList();
	}
}
