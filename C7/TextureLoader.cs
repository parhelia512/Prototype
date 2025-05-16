using System;
using NLua;
using Godot;
using ConvertCiv3Media;
using System.Collections.Generic;

public static class TextureLoader {
	private static Lua lua;
	private static LuaTable textureConfig;

	private static Dictionary<string, ImageTexture> textureCache = [];
	private static Dictionary<string, Pcx> PcxCache = [];

	static TextureLoader() {
		lua = new Lua();
		textureConfig = (LuaTable)lua.DoFile("./Text/texture_config.lua")[0];
	}

	public static ImageTexture Load(string keyPath) {
		object entry = GetEntryByPath(keyPath);

		if (entry == null)
			throw new Exception($"Texture config not found for key: {keyPath}");

		return LoadFromLuaObject(entry);
	}

	public static void SetButtonTextures(TextureButton button, string keyPath) {
		var entry = (LuaTable)GetEntryByPath(keyPath);

		button.TextureNormal = LoadFromLuaObject(entry["normal"]);
		button.TexturePressed = LoadFromLuaObject(entry["pressed"]);
		button.TextureHover = LoadFromLuaObject(entry["hover"]);
	}

	private static ImageTexture LoadFromLuaObject(object entry) {
		// Handle string paths directly
		if (entry is string simplePath) {
			return LoadFromPCX(simplePath);
		}

		// Handle configurations via table
		if (entry is LuaTable table) {
			if (table["path"] == null) {
				throw new ArgumentException("Texture configuration missing required 'path' property");
			}

			string path = table["path"].ToString();

			// Process textures with alpha blending
			string? alpha = table["alpha"]?.ToString();
			if (alpha != null) {
				return LoadWithAlphaBlend(path, alpha, table);
			}

			// Process standard texture with optional cropping
			PCXToGodot.CropRegion? cropRegion = ExtractCropRegion(table);
			bool shadows = Convert.ToBoolean(table["shadows"] ?? true);

			return LoadFromPCX(path, cropRegion, shadows);
		}

		throw new ArgumentException($"Invalid texture config format: {entry?.GetType().Name ?? "null"}");
	}

	// Helper method to extract crop region from a table
	private static PCXToGodot.CropRegion? ExtractCropRegion(LuaTable table) {
		object cropRegionObj = table["crop_region"];
		if (cropRegionObj is LuaTable cropRegion) {
			try {
				int x = Convert.ToInt32(cropRegion[1]);
				int y = Convert.ToInt32(cropRegion[2]);
				int w = Convert.ToInt32(cropRegion[3]);
				int h = Convert.ToInt32(cropRegion[4]);

				return new PCXToGodot.CropRegion(x, y, w, h);
			} catch (Exception ex) {
				throw new FormatException($"Invalid crop_region format: {ex.Message}", ex);
			}
		}

		return null;
	}

	// Helper method to handle alpha blend loading
	private static ImageTexture LoadWithAlphaBlend(string path, string alphaPath, LuaTable table) {
		Pcx pcx = LoadPCX(path);
		Pcx alphaPcx = LoadPCX(alphaPath);
		int alphaRowOffset = Convert.ToInt32(table["alpha_row_offset"] ?? 0);

		PCXToGodot.CropRegion? cropRegion = ExtractCropRegion(table);

		return cropRegion.HasValue
			? PCXToGodot.getImageFromPCXWithAlphaBlend(pcx, alphaPcx, cropRegion.Value, alphaRowOffset)
			: PCXToGodot.getImageFromPCXWithAlphaBlend(pcx, alphaPcx);
	}

	private static object GetEntryByPath(string keyPath) {
		string[] parts = keyPath.Split('.');
		object current = textureConfig;

		foreach (string part in parts) {
			if (current is LuaTable table && table[part] != null) {
				current = table[part];
			} else {
				return null;
			}
		}

		return current;
	}

	//Send this function a path (e.g. Art/title.pcx) and it will load it up and convert it to a texture for you.
	public static ImageTexture LoadFromPCX(string relPath) {
		return LoadFromPCX(relPath, null, true);
	}

	//Send this function a path (e.g. Art/exitBox-backgroundStates.pcx), and the coordinates of the extracted image you need from that PCX
	//file, and it'll load it up and return you what you need.
	public static ImageTexture LoadFromPCX(string relPath, PCXToGodot.CropRegion cropRegion, bool shadows = true) {
		return LoadFromPCX(relPath, cropRegion, new PCXToGodot.ColorOptions(shadows));
	}

	private static ImageTexture LoadFromPCX(string relPath, PCXToGodot.CropRegion? cropRegion, PCXToGodot.ColorOptions colorOptions) {
		string key = cropRegion is null
			? relPath
			: $"{relPath}-{cropRegion.Value.LeftStart}-{cropRegion.Value.TopStart}-{cropRegion.Value.CroppedWidth}-{cropRegion.Value.CroppedHeight}";

		if (textureCache.TryGetValue(key, out ImageTexture cached)) {
			return cached;
		}

		Pcx pcx = LoadPCX(relPath);
		ImageTexture texture = cropRegion is null
			? PCXToGodot.getImageTextureFromPCX(pcx)
			: PCXToGodot.getImageTextureFromPCX(pcx, cropRegion.Value, colorOptions);

		textureCache[key] = texture;
		return texture;
	}

	/**
	 * Utility method for loading PCX files that will cache them, so we don't have to load them from disk so often.
	 **/
	public static Pcx LoadPCX(string relPath) {
		if (PcxCache.TryGetValue(relPath, out Pcx value)) {
			return value;
		}
		Pcx thePcx = new(Util.Civ3MediaPath(relPath));
		PcxCache[relPath] = thePcx;
		return thePcx;
	}

	public static void ClearCache() {
		PcxCache.Clear();
		textureCache.Clear();
	}
}
