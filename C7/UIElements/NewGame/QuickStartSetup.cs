using Godot;
using System;
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
			landform = WorldCharacteristics.Landform.Pangaea,
			oceanCoverage = WorldCharacteristics.OceanCoverage.Percent_70,
			age = WorldCharacteristics.Age.Billion_4,
			climate = WorldCharacteristics.Climate.Normal,
			temperature = WorldCharacteristics.Temperature.Temperate,
			worldSize = WorldSize.Generic(),
			mapSeed = new Random().Next(),
		};

		globalState.SaveGame = save;

		Civilization player =  save.Civilizations.FirstOrDefault(x => x.name == "Netherlands") ?? save.Civilizations[1];

		Difficulty difficulty = save.Difficulties.FirstOrDefault(x => x.Name == "Regent") ?? save.Difficulties[0];

		var opponents = save.Civilizations
			.Select(x => new SelectedOpponent { isRandom = true })
			.Take(globalState.WorldCharacteristics.worldSize.numberOfCivs - 1)
			.ToList();

		GameSetup gameSetup = new() {
			playerCivilization = player,
			difficulty = difficulty,
			worldCharacteristics = globalState.WorldCharacteristics,
			opponents = opponents
		};

		gameSetup.Populate(save);
	}
}
