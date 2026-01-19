using IniParser;

namespace C7Engine;

public static class Util {
	private static FileIniDataParser fileIniDataParser;

	public static FileIniDataParser GetFileIniDataParser() {
		if (fileIniDataParser != null) return fileIniDataParser;

		FileIniDataParser parser = new FileIniDataParser();
		parser.Parser.Configuration.AllowDuplicateKeys = true;
		parser.Parser.Configuration.OverrideDuplicateKeys = true;

		fileIniDataParser = parser;
		return parser;
	}
}
