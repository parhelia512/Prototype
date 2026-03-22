using System.Collections.Generic;
using C7.Textures;
using C7GameData;
using Godot;

public abstract class MiniMapLayer {
	public virtual void Configure(GameData gD) { }
	public virtual void DrawTile(Image mapImage, Tile tile, int x, int y) { }

	public bool visible = true;
}

public class BaseLandMiniLayer : MiniMapLayer {
	public override void DrawTile(Image mapImage, Tile tile, int x, int y) {
		if (visible && tile.IsLand())
			mapImage.SetPixel(x, y, Colors.DarkSeaGreen);
	}
}

public class TerrainMiniLayer : MiniMapLayer {
	public static readonly Dictionary<string, Color> TerrainColorMap = new()
	{
		{ "desert",      Colors.DarkGray },
		{ "plains",      Colors.DarkKhaki },
		{ "grassland",   Colors.DarkSeaGreen },
		{ "tundra",      Colors.LightGray },
		{ "flood plain", Colors.LightBlue },
		{ "hills",       Colors.Silver },
		{ "mountains",   Colors.RosyBrown },
		{ "forest",      Colors.DarkSeaGreen },
		{ "jungle",      Colors.CadetBlue },
		{ "marsh",       Colors.LightSlateGray },
		{ "volcano",     Colors.DimGray },
		{ "coast",       Colors.LightSteelBlue },
		{ "sea",         Colors.SteelBlue },
		{ "ocean",       Colors.RoyalBlue },
	};

	public override void DrawTile(Image mapImage, Tile tile, int x, int y) {
		if (visible && TerrainColorMap.TryGetValue(tile.overlayTerrainType.Key, out Color value))
			mapImage.SetPixel(x, y, value);
	}
}

public class PlayerColorMiniLayer : MiniMapLayer {
	/// Returns the fully saturated version of the colour.
	public static Color Intensify(Color colour) => Color.FromHsv(colour.H, 1, colour.V, colour.A);

	public override void DrawTile(Image mapImage, Tile tile, int x, int y) {
		if (visible && tile.OwningPlayer() != null) {
			var civColor = TextureLoader.LoadColor(tile.OwningPlayer().GetPlayerColor());
			mapImage.SetPixel(x, y, Intensify(civColor));
		}
	}
}

public class CityMiniLayer : MiniMapLayer {
	public override void DrawTile(Image mapImage, Tile tile, int x, int y) {
		if (visible && tile.HasCity)
			mapImage.SetPixel(x, y, Colors.White);
	}
}

public class WaterMiniLayer : MiniMapLayer {
	public override void DrawTile(Image mapImage, Tile tile, int x, int y) {
		if (visible && tile.IsWater())
			mapImage.SetPixel(x, y, Colors.SteelBlue);
	}
}

public class FogOfWarMiniLayer : MiniMapLayer {
	private bool _observerMode;
	private HashSet<Tile> _knownTiles;

	public override void Configure(GameData gD) {
		_observerMode = gD.observerMode;
		_knownTiles = gD.GetFirstHumanPlayer().tileKnowledge.knownTiles;
	}

	public override void DrawTile(Image mapImage, Tile tile, int x, int y) {
		if (visible && !_observerMode && !_knownTiles.Contains(tile))
			mapImage.SetPixel(x, y, Colors.Black);
	}
}
