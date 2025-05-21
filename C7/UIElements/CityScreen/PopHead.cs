using Godot;
using System;
using C7GameData;

// A utility class for rendering pop heads.
public class PopHeads {
	public const int HEAD_SIZE = 48;
	private const int HEAD_SIZE_WITH_BORDER = 50;
	private const int NUM_ERAS = 4;
	private const int MOODS_PER_ERA = 4;

	public static ImageTexture GetPopHead(CityResident cr, int eraNum) {
		if (cr.citizenType.IsDefaultCitizen) {
			return GetLaborerPopHead(cr, eraNum);
		}
		return GetSpecialistPopHead(cr, eraNum);
	}

	private static ImageTexture GetSpecialistPopHead(CityResident cr, int eraNum) {
		int X = HEAD_SIZE_WITH_BORDER * eraNum;
		int numRowsOfLaborers = NUM_ERAS * MOODS_PER_ERA;
		int Y = HEAD_SIZE_WITH_BORDER * numRowsOfLaborers
				 + HEAD_SIZE_WITH_BORDER * (cr.citizenType.SpecialistIndex - 1);
		return TextureLoader.LoadFromPCX("Art/SmallHeads/popHeads.pcx",
										new(X + 1, Y + 1, HEAD_SIZE, HEAD_SIZE));
	}

	private static ImageTexture GetLaborerPopHead(CityResident cr, int eraNum) {
		int column = cr.mood switch {
			CityResident.Mood.Content => 0,
			CityResident.Mood.Happy => 1,
			CityResident.Mood.Unhappy => 3,
		};
		int headWithBorderSize = HEAD_SIZE + 2;

		return TextureLoader.LoadFromPCX("Art/SmallHeads/popHeads.pcx",
										new(0 + 1,
										MOODS_PER_ERA * HEAD_SIZE_WITH_BORDER * eraNum
											+ HEAD_SIZE_WITH_BORDER * column + 1,
										HEAD_SIZE, HEAD_SIZE));
	}
}
