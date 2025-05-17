using System;
using NLua;
using Godot;
using ConvertCiv3Media;
using System.Collections.Generic;
using System.IO;


public readonly record struct CropRegion(int LeftStart, int TopStart, int CroppedWidth, int CroppedHeight);

public static class TextureLoader {
	private struct ConfigEntry {
		public string Path;
		public CropRegion? CropRegion = null;

		// PCX-specific settings
		public string AlphaPath = null;
		public int AlphaRowOffset = 0;
		public PCXToGodot.ColorOptions ColorOptions = true;

		public ConfigEntry() {
		}

		public readonly bool UseAlpha => AlphaPath != null;
	}

	private static Lua lua;
	private static LuaTable civ3TextureConfig;
	private static LuaTable c7TextureConfig;

	private static LuaTable textureConfig;
	private static bool modernGraphics;

	private static Dictionary<string, ImageTexture> textureCache = [];
	private static Dictionary<string, Pcx> PcxCache = [];

	static TextureLoader() {
		lua = new Lua();
		lua.DoString($"package.path = './Text/TextureConfigs/?.lua;./Text/TextureConfigs/*/?.lua'");

		civ3TextureConfig = (LuaTable)lua.DoFile("./Text/TextureConfigs/civ3.lua")[0];
		c7TextureConfig = (LuaTable)lua.DoFile("./Text/TextureConfigs/c7.lua")[0];

		textureConfig = c7TextureConfig;
		modernGraphics = true;
	}

	public static ImageTexture Load(string keyPath) {
		object entry = GetEntryByPath(keyPath);

		if (entry == null)
			throw new Exception($"Texture config not found for key: {keyPath}");

		return LoadFromLuaObject(entry);
	}

	public static ImageTexture Load(string keyPath, object obj) {
		object entry = GetEntryByPath(keyPath);

		if (entry is not LuaTable table)
			throw new Exception($"Table expected for key: {keyPath}");

		var func = table["map_object_to_sprite"] as LuaFunction
			?? throw new Exception("Custom mapping function expected");

		return LoadFromLuaObject(func.Call(table, obj)[0]);
	}

	public static void SetButtonTextures(TextureButton button, string keyPath) {
		var entry = (LuaTable)GetEntryByPath(keyPath);

		button.TextureNormal = LoadFromLuaObject(entry["normal"]);
		button.TexturePressed = LoadFromLuaObject(entry["pressed"]);
		button.TextureHover = LoadFromLuaObject(entry["hover"]);
	}

	private static ImageTexture LoadFromLuaObject(object entry) {
		return LoadFromConfigEntry(ParseConfigEntry(entry));
	}

	private static ConfigEntry ParseConfigEntry(object entry) {
		if (entry is string simplePath) {
			return new() {
				Path = simplePath,
			};
		}

		if (entry is LuaTable table) {
			if (table["path"] == null) {
				throw new ArgumentException("Texture configuration missing required 'path' property");
			}

			return new() {
				Path = table["path"].ToString(),
				AlphaPath = table["alpha"]?.ToString(),
				CropRegion = ExtractCropRegion(table),
				ColorOptions = Convert.ToBoolean(table["shadows"] ?? true),
				AlphaRowOffset = Convert.ToInt32(table["alpha_row_offset"] ?? 0)
			};
		}

		throw new ArgumentException($"Invalid texture config format: {entry?.GetType().Name ?? "null"}");
	}

	private static ImageTexture LoadFromConfigEntry(ConfigEntry config) {
		if (config.UseAlpha) {
			return LoadWithAlphaBlend(config.Path, config.AlphaPath!, config.CropRegion, config.AlphaRowOffset);
		}

		string ext = Path.GetExtension(config.Path).ToLowerInvariant();
		return ext switch {
			".png" => LoadFromPNG(config.Path, config.CropRegion),
			".pcx" => LoadFromPCX(config.Path, config.CropRegion, config.ColorOptions),
			_ => throw new FormatException($"Unknown texture format: {config.Path}"),
		};
	}

	// Helper method to extract crop region from a table
	private static CropRegion? ExtractCropRegion(LuaTable table) {
		object cropRegionObj = table["crop_region"];
		if (cropRegionObj is LuaTable cropRegion) {
			try {
				int x = Convert.ToInt32(cropRegion[1]);
				int y = Convert.ToInt32(cropRegion[2]);
				int w = Convert.ToInt32(cropRegion[3]);
				int h = Convert.ToInt32(cropRegion[4]);

				return new CropRegion(x, y, w, h);
			} catch (Exception ex) {
				throw new FormatException($"Invalid crop_region format: {ex.Message}", ex);
			}
		}

		return null;
	}

	// Helper method to handle alpha blend loading
	private static ImageTexture LoadWithAlphaBlend(string path, string alphaPath, CropRegion? cropRegion, int alphaRowOffset) {
		Pcx pcx = LoadPCX(path);
		Pcx alphaPcx = LoadPCX(alphaPath);

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
	public static ImageTexture LoadFromPCX(string relPath, CropRegion cropRegion, bool shadows = true) {
		return LoadFromPCX(relPath, cropRegion, new PCXToGodot.ColorOptions(shadows));
	}

	private static ImageTexture LoadFromPCX(string relPath, CropRegion? cropRegion, PCXToGodot.ColorOptions colorOptions) {
		return GetOrAddTexture(relPath, cropRegion, () => {
			Pcx pcx = LoadPCX(relPath);
			return cropRegion is null
				? PCXToGodot.getImageTextureFromPCX(pcx)
				: PCXToGodot.getImageTextureFromPCX(pcx, cropRegion.Value, colorOptions);
		});
	}

	private static ImageTexture LoadFromPNG(string relPath, CropRegion? cropRegion) {
		return GetOrAddTexture(relPath, cropRegion, () => {
			Image image = Image.LoadFromFile(Util.Civ3MediaPath(relPath));
			if (cropRegion != null) {
				var region = cropRegion.Value;
				return ImageTexture.CreateFromImage(
					image.GetRegion(new Rect2I(region.LeftStart, region.TopStart, region.CroppedWidth, region.CroppedHeight))
				);
			}
			return ImageTexture.CreateFromImage(image);
		});
	}

	private static string MakeCacheKey(string relPath, CropRegion? cropRegion) {
		if (cropRegion is null) return relPath;

		var region = cropRegion.Value;
		return $"{relPath}-{region.LeftStart}-{region.TopStart}-{region.CroppedWidth}-{region.CroppedHeight}";
	}

	private static ImageTexture GetOrAddTexture(string relPath, CropRegion? cropRegion, Func<ImageTexture> loader) {
		string key = MakeCacheKey(relPath, cropRegion);

		if (textureCache.TryGetValue(key, out ImageTexture cached))
			return cached;

		ImageTexture texture = loader();
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

	public static void ToggleModernGraphics() {
		if (modernGraphics) {
			modernGraphics = false;
			textureConfig = civ3TextureConfig;
		} else {
			modernGraphics = true;
			textureConfig = c7TextureConfig;
		}
	}
}
