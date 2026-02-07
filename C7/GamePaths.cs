using Godot;
using System.IO;

public static class GamePaths {
	// Base directory for finding data files (Lua/, Text/, Assets/).
	// In the editor this is "res://". In exports, it's the directory
	// containing the executable — on macOS, navigated out of the .app bundle.
	private static string _baseDir;
	public static string BaseDir {
		get {
			if (_baseDir == null) {
				if (OS.HasFeature("editor")) {
					_baseDir = "";
				} else {
					string exeDir = OS.GetExecutablePath().GetBaseDir();
					// On macOS the exe is inside *.app/Contents/MacOS/;
					// data files live alongside the .app bundle.
					if (exeDir.Contains(".app/Contents/MacOS")) {
						_baseDir = Path.GetFullPath(Path.Combine(exeDir, "..", "..", "..")) + "/";
					} else {
						_baseDir = exeDir + "/";
					}
				}
			}
			return _baseDir;
		}
	}

	// This is the 'static map' used in lieu of terrain generation
	public static string DefaultGamePath {
		get =>
			C7Engine.C7Settings.UseStandaloneMode()
				? BaseDir + "Text/c7-static-map-save-standalone.json"
				: BaseDir + "Text/c7-static-map-save.json";
	}

	public static string LuaRulesDir => BaseDir + "Lua/rules/";
	public static string TextureConfigsDir => BaseDir + "Lua/texture_configs/";

	public const string ModernGraphicsConfig = "c7.lua";
	public const string ClassicGraphicsConfig = "civ3.lua";

	// For now this needs to get passed to QueryCiv3 when importing.
	public static string DefaultBicPath { get => Util.GetCiv3Path() + "/Conquests/conquests.biq"; }
}
