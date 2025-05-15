using System;
using NLua;
using Godot;

public static class TextureLoader {
	private static Lua lua;
	private static LuaTable textureConfig;

	static TextureLoader() {
		lua = new Lua();
		textureConfig = (LuaTable)lua.DoFile("./Text/texture_config.lua")[0];
	}

	public static ImageTexture Load(string keyPath) {
		var entry = GetEntryByPath(keyPath);

		if (entry == null)
			throw new Exception($"Texture config not found for key: {keyPath}");

		string path;

		int x, y, w, h;
		bool shadows;

		if (entry is string simplePath) {
			path = simplePath;
			return Util.LoadTextureFromPCX(path);
		} else if (entry is LuaTable table) {
			path = table["path"]?.ToString();
			shadows = Convert.ToBoolean(table["shadows"] ?? true);

			LuaTable cropRegion = (LuaTable)table["crop_region"];

			x = Convert.ToInt32(cropRegion[1]);
			y = Convert.ToInt32(cropRegion[2]);
			w = Convert.ToInt32(cropRegion[3]);
			h = Convert.ToInt32(cropRegion[4]);
		} else {
			throw new Exception($"Invalid texture config format for key: {keyPath}");
		}

		return Util.LoadTextureFromPCX(path, x, y, w, h, shadows);
	}

	private static object GetEntryByPath(string keyPath) {
		string[] parts = keyPath.Split('.');
		object current = textureConfig;

		foreach (var part in parts) {
			if (current is LuaTable table && table[part] != null) {
				current = table[part];
			} else {
				return null;
			}
		}

		return current;
	}
}
