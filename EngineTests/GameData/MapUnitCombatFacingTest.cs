using System.Collections.Generic;
using System.Linq;
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

		HashSet<string> rotatingUnits = GameModeLoader.Load(PathUtils.gameModesDir, new GameModeConfig("base-ruleset.json"))
			.UnitPrototypes
			.Where(proto => proto.rotateBeforeAttack)
			.Select(proto => proto.name)
			.ToHashSet();

		foreach (string unitName in expectedRotatingUnits) {
			Assert.True(rotatingUnits.Contains(unitName), $"{unitName} should rotate before attack.");
		}
	}

	private static MapUnit MakeUnit(bool rotateBeforeAttack) {
		return new(ID.None("unit")) {
			unitType = new() {
				rotateBeforeAttack = rotateBeforeAttack,
			}
		};
	}
}
