using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using C7Engine.Lua;
using C7GameData;
using Xunit;

namespace EngineTests.GameData;

public class MapUnitCombatFacingTest {
	[Fact]
	public void UnitsTurnTowardTargetWhenRotateBeforeAttackIsDisabled() {
		MapUnit unit = MakeUnit(rotateBeforeAttack: false);

		Assert.Equal(TileDirection.EAST, unit.GetAttackAnimationDirection(TileDirection.EAST));
		Assert.Equal(TileDirection.WEST, unit.GetDefenseAnimationDirection(TileDirection.EAST));
	}

	[Fact]
	public void UnitsUseBroadsideFacingWhenRotateBeforeAttackIsEnabled() {
		MapUnit unit = MakeUnit(rotateBeforeAttack: true);

		Assert.Equal(TileDirection.NORTH, unit.GetAttackAnimationDirection(TileDirection.EAST));
		Assert.Equal(TileDirection.SOUTH, unit.GetDefenseAnimationDirection(TileDirection.EAST));
	}

	[Fact]
	public void UnitPrototypePreservesRotateBeforeAttackFromSavedPrototype() {
		UnitPrototype unit = new(new() {
			name = "Galley",
			rotateBeforeAttack = true,
		}, []);

		Assert.True(unit.rotateBeforeAttack);
	}

	[Fact]
	public void BaseRulesetMarksBroadsideUnitsAsRotateBeforeAttack() {
		string[] expectedRotatingUnits = [
			"Radar Artillery",
			"Galley",
			"Caravel",
			"Frigate",
			"Galleon",
			"Battleship",
			"Man-O-War",
			"Privateer",
			"Carrack",
			"Curragh",
		];

		using JsonDocument ruleset = JsonDocument.Parse(File.ReadAllText(Path.Combine(PathUtils.gameModesDir, "base-ruleset.json")));
		JsonElement unitPrototypes = ruleset.RootElement.GetProperty("unitPrototypes");
		foreach (JsonElement unitPrototype in unitPrototypes.EnumerateArray()) {
			string unitName = unitPrototype.GetProperty("name").GetString();
			Assert.True(unitPrototype.TryGetProperty("rotateBeforeAttack", out _), $"{unitName} should declare rotateBeforeAttack.");
		}

		HashSet<string> rotatingUnits = unitPrototypes.EnumerateArray()
			.Where(unitPrototype => unitPrototype.GetProperty("rotateBeforeAttack").GetBoolean())
			.Select(unitPrototype => unitPrototype.GetProperty("name").GetString())
			.ToHashSet();

		Assert.Equal(expectedRotatingUnits.OrderBy(unitName => unitName), rotatingUnits.OrderBy(unitName => unitName));
	}

	private static MapUnit MakeUnit(bool rotateBeforeAttack) {
		return new(ID.None("unit")) {
			unitType = new() {
				rotateBeforeAttack = rotateBeforeAttack,
			}
		};
	}
}
