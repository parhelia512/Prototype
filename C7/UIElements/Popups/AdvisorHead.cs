using Godot;
using System;

public class AdvisorHead {
	public enum Mood {
		Happy,
		Angry,
		Sad,
		Surprised
	};

	public enum Advisor {
		Domestic,
		Trade,
		Military,
		Foreign,
		Culture,
		Science
	};

	public static ImageTexture GetPopupImage(Advisor advisor, Mood mood, int eraIndex) {
		return Util.LoadTextureFromPCX(GetFilename(advisor), 1 + (int)mood * 150, 150 * (eraIndex + 1) - 110, 149, 110, false);
	}

	private static string GetFilename(Advisor advisor) {
		switch (advisor) {
			case Advisor.Domestic: return "Art/SmallHeads/popupDOMESTIC.pcx";
			case Advisor.Trade: return "Art/SmallHeads/popupTRADE.pcx";
			case Advisor.Military: return "Art/SmallHeads/popupMILITARY.pcx";
			case Advisor.Foreign: return "Art/SmallHeads/popupFOREIGN.pcx";
			case Advisor.Culture: return "Art/SmallHeads/popupCULTURE.pcx";
			case Advisor.Science: return "Art/SmallHeads/popupSCIENCE.pcx";
		}
		throw new Exception($"Unknown advisor type: {advisor}");
	}
}
