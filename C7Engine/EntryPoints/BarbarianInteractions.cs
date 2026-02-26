using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using C7GameData;

namespace C7Engine;

public class BarbarianInteractions {
	public static int SpawnBarbarians(GameData gameData) {
		Player barbPlayer = gameData.players.Find(player => player.isBarbarians);

		// A random 5% of camps will spawn a unit each turn. Shuffle the
		// camps to make this random.
		int barbariansToSpawn = (int)Math.Ceiling(GameData.rng.Next(gameData.map.barbarianCamps.Count) / 20.0);
		List<int> tileIndicies = Enumerable.Range(0, gameData.map.barbarianCamps.Count).ToList();
		GameData.rng.Shuffle<int>(CollectionsMarshal.AsSpan(tileIndicies));

		// Sample barbarian camps
		var spawningCamps = tileIndicies
			.Select(i => gameData.map.barbarianCamps[i])
			.Take(barbariansToSpawn);

		// Spawn a unit
		foreach (Tile camp in spawningCamps) {
			UnitPrototype unitType = SelectBarbarianUnitType(gameData.barbarianInfo, camp);
			Tile tile = SelectSpawnTile(barbPlayer, camp, unitType);
			if (tile != null) {
				gameData.SpawnUnit(barbPlayer, unitType, tile);
			}
		}

		return barbariansToSpawn;
	}

	public static UnitPrototype SelectBarbarianUnitType(BarbarianInfo barbInfo, Tile tile) {
		// Coastal camps have a 20% chance of spawning a sea unit
		if (tile.NeighborsWater() && GameData.rng.Next(100) < 20) {
			return barbInfo.barbarianSeaUnitProto;
		}

		// Land units are generated in a 3:1 ratio, three advanced units for every basic barbarian
		return GameData.rng.Next(100) < 25 ? barbInfo.advancedBarbarian : barbInfo.basicBarbarian;
	}

	public static Tile SelectSpawnTile(Player player, Tile camp, UnitPrototype unitType) {
		// Spawn land units at the camp
		if (unitType.IsLandUnit())
			return camp;

		// Spawn sea units on a sea tile, but not in a lake or on a tile occupied by another player
		bool CanSpawnSeaUnits(Tile t) =>
			t.IsWater() && !t.isFreshWater && t.unitsOnTile.TrueForAll(u => u.owner == player);

		if (unitType.IsSeaUnit()) {
			// Check first two ranks around camp for a suitable tile, or bail with a null 
			return camp.FindInRing(1, CanSpawnSeaUnits)
				?? camp.FindInRing(2, CanSpawnSeaUnits);
		}

		// Default to camp 
		return camp;
	}
}
