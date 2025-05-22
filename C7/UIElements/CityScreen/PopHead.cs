using Godot;
using System;
using C7GameData;

// A utility class for rendering pop heads.
public class PopHead {
	public const int HEAD_SIZE = 48;

	public static ImageTexture GetTexture(CityResident cityResident, int eraNum) {
		return TextureLoader.Load("popheads", new { cityResident, eraNum });
	}
}
