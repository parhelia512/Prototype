using IniParser;

namespace C7Engine;

public static class Util {
	private static FileIniDataParser fileIniDataParser;

	public static FileIniDataParser GetFileIniDataParser() {
		if (fileIniDataParser != null) return fileIniDataParser;

		FileIniDataParser parser = new FileIniDataParser();
		// The default behaviour of the parser is to throw an exception
		// when it finds duplicate keys (e.x. Tank.ini has 'DEAD' two times in [Sound Effects])
		// so, we want to allow it so that it doesn't crash
		parser.Parser.Configuration.AllowDuplicateKeys = true;
		// but we only want to keep the last value we encounter
		parser.Parser.Configuration.OverrideDuplicateKeys = true;

		fileIniDataParser = parser;
		return parser;
	}
}
