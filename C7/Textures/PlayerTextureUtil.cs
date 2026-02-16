using System.Collections.Generic;
using C7Engine;
using C7GameData;
using System.Linq;
using Godot;

namespace C7.Textures;

public static class PlayerTextureUtil {
	private static Dictionary<int, ShaderMaterial> materialCache = [];
	private static Dictionary<ID, int> colorsInUseCache = [];

	public static int GetPlayerColor(this Player player) {
		if (!colorsInUseCache.ContainsKey(player.id)) {
			InitializeCivColors(EngineStorage.gameData);
		}
		return colorsInUseCache[player.id];
	}

	private static int LoadCivColor(Player player) {
		if (colorsInUseCache.TryGetValue(player.id, out int colorIndex))
			return colorIndex;

		if (colorsInUseCache.ContainsValue(player.primaryColorIndex)) {
			colorsInUseCache.Add(player.id, player.secondaryColorIndex);
			return player.secondaryColorIndex;
		}

		colorsInUseCache.Add(player.id, player.primaryColorIndex);
		return player.primaryColorIndex;
	}

	// This will only run the first time we ask for any civ's color, for all the civs
	private static void InitializeCivColors(GameData gameData) {
		// a first basic run to assign the colors
		foreach (var player in gameData.players) {
			LoadCivColor(player);
		}

		// a second run to avoid duplicate colors if possible
		foreach (var player in gameData.players) {
			if (colorsInUseCache.Values.Count(p => p == player.GetPlayerColor()) > 1) {
				colorsInUseCache.Remove(player.id);
				LoadCivColor(player);
			}
		}
	}

	public static ShaderMaterial GetShaderMaterialForUnit(int civIndex) {
		if (materialCache.TryGetValue(civIndex, out ShaderMaterial material)) {
			return material;
		}
		material = new();
		material.Shader = GD.Load<Shader>("res://UnitTint.gdshader");
		Color civColor = TextureLoader.LoadColor(civIndex);
		material.SetShaderParameter("tintColor", new Vector3(civColor.R, civColor.G, civColor.B));
		materialCache[civIndex] = material;
		return material;
	}

	public static void ClearCache() {
		materialCache.Clear();
		colorsInUseCache.Clear();
	}
}
