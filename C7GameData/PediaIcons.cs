
using System;
using System.Collections.Generic;
using System.IO;
using Serilog;

namespace C7GameData {

	// A class for interacting with the PediaIcons.txt file used in scenarios.
	class PediaIcons {
		private static readonly ILogger log = Log.ForContext<PediaIcons>();

		// A mapping from the civilopedia entry name (like PRTO_Spearman or
		// PRTO_Legionary_II) to the name used in the art directory (Spearman
		// and Legionary)
		private readonly Dictionary<string, string> artMapping = new();

		private readonly string pediaIconsPath;

		public PediaIcons(string path) {
			pediaIconsPath = path;
			string[] lines = File.ReadAllLines(path);

			string animNamePrefix = "#ANIMNAME_";
			for (int i = 0; i < lines.Length - 1; ++i) {
				if (!lines[i].StartsWith(animNamePrefix)) {
					continue;
				}

				string civilopediaName = lines[i].Substring(animNamePrefix.Length);
				if (civilopediaName.Contains("_ERAS_")) {
					// HACK: The civilopedia name for leaders differs by era, but the current
					// approach for resolving the artwork is era independent. Given a line like
					// #ANIMNAME_PRTO_Leader_ERAS_Ancient_Times we want to end up with
					// PRTO_Leader to match the biq file.
					civilopediaName = civilopediaName.Substring(0, civilopediaName.IndexOf("_ERAS_"));
				}

				string artName = lines[i + 1];
				artMapping[civilopediaName] = artName;
			}
		}

		public string GetArtName(string civilopediaEntry) {
			string artName = artMapping[civilopediaEntry];
			if (artName == null) {
				log.Error($"Could not find #ANIMNAME_{civilopediaEntry} in PediaIcons file '{pediaIconsPath}");
				return "Warrior";
			}
			return artName;
		}
	}

}
