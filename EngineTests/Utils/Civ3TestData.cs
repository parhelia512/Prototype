using System;
using System.IO;
using System.Linq;
using QueryCiv3;

namespace EngineTests.Utils;

public static class Civ3TestData {
	private static readonly string[] DefaultRequiredFiles = {
		"Conquests/conquests.biq",
		"Conquests/Text/PediaIcons.txt",
	};

	public static bool ShouldSkipCiv3DependentTests(params string[] requiredRelativePaths) {
		if (Environment.GetEnvironmentVariable("CI") != null) {
			return true;
		}

		string[] requiredFiles = DefaultRequiredFiles.Concat(requiredRelativePaths).ToArray();
		return requiredFiles.Any(relativePath => !File.Exists(GetCiv3Path(relativePath)));
	}

	private static string GetCiv3Path(string relativePath) {
		string normalizedPath = relativePath
			.Replace('\\', Path.DirectorySeparatorChar)
			.Replace('/', Path.DirectorySeparatorChar);

		return Path.Combine(Civ3Location.GetCiv3Path(), normalizedPath);
	}
}
