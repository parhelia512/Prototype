using System;
using System.Collections.Generic;
using C7.Textures;
using Godot;
using C7GameData;
using Serilog;
using C7Engine;

[GlobalClass]
[Tool]
public partial class MiniMap : TextureRect {
	private ILogger log = LogManager.ForContext<MiniMap>();

	private TextureRect mapFrameRect;

	private MapView mapView;
	private TextureRect mapTextureRect;
	private ImageTexture mapTexture;
	private Image mapImage;

	private Vector2I miniMapFrameSize = new (280, 130);
	private Vector2I miniMapSize = new (229, 105);
	private Vector2I frameOffset = new Vector2I(7, -12 + -10); // overall control has 10px boundary, adjust for VP
	private Vector2I mapOffset = new Vector2I(25, -13); // offset inside the frame
	private Vector2I clickOffset = new Vector2I(15, 0); // offset delta?

	public MiniMap(MapView mapView) {
		this.mapView = mapView;
	}

	public override void _Ready() {
		CreateMiniMap();
		MouseFilter = MouseFilterEnum.Stop;
	}

	private void CreateMiniMap() {
		// Draw frame
		// TODO: Draw frame on top of the map texture (figure out stencil alpha)
		ImageTexture boxLeft = TextureLoader.Load("lower_left_infobox.box");
		mapFrameRect = new TextureRect();
		mapFrameRect.Texture = boxLeft;
		AddChild(mapFrameRect);

		// Draw the map inside the frame
		mapTexture = new ImageTexture();
		mapTextureRect = new TextureRect();

		mapTextureRect.Texture = mapTexture;
		mapTextureRect.SetSize(miniMapSize);

		mapTextureRect.SetExpandMode(ExpandModeEnum.KeepSize);
		AddChild(mapTextureRect);
	}

	// TODO: Enable/disable via INI
	// TODO: Dynamic colours
	// TODO: Configurable colours
	// TODO: Resizing minimap
	// TODO: Configurable size via INI (absolute/relative)

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

	/// Returns the fully saturated version of the colour.
	public static Color Intensify(Color colour) => Color.FromHsv(colour.H, 1, colour.V, colour.A);

	public override void _Process(double delta) {
		// Position frame and map relative to viewport
		var vp = GetViewportRect().Size;
		mapFrameRect.SetPosition(frameOffset + new Vector2(0, vp.Y - miniMapFrameSize.Y));
		mapTextureRect.SetPosition(frameOffset + new Vector2(0, vp.Y - miniMapSize.Y) + mapOffset);

		EngineStorage.ReadGameData((GameData gD) => {
			var map = gD.map;
			var knowledge = gD.GetFirstHumanPlayer().tileKnowledge;
			mapImage = Image.CreateEmpty(map.numTilesWide, map.numTilesTall / 2, true, Image.Format.Rgb8);

			// Render tiles as pixels
			foreach (var t in map.tiles) {
				var (x, y) = ComputeIsoCoordinates(t);
				if (!gD.observerMode && !knowledge.knownTiles.Contains(t))
					mapImage.SetPixel(x, y, Colors.Black);
				else if (t.IsWater())
					mapImage.SetPixel(x, y, Colors.SteelBlue);				
				else if (t.HasCity)
					mapImage.SetPixel(x, y, Colors.White);
				else if (t.OwningPlayer() != null) {
					var civColor = TextureLoader.LoadColor(t.OwningPlayer().GetPlayerColor());
					mapImage.SetPixel(x, y, Intensify(civColor));
				}
				// Terrain colors
				else if (TerrainColorMap.TryGetValue(t.overlayTerrainType.Key, out Color value))
					mapImage.SetPixel(x, y, value);
				// Fallbacks
				else if (t.IsLand())
					mapImage.SetPixel(x, y, Colors.DarkSeaGreen);
			}

			// Render the viewport bounds as a rectangle
			// TODO: Handle draws over map edges, maybe with Mathf.Wrap
			if (mapView != null) {
				var vr = mapView.getVisibleRegion();

				// Clamp coordinates to draw space
				var ax = Math.Clamp(vr.upperLeftX, 0, map.numTilesWide - 2);
				var ay = Math.Clamp(vr.upperLeftY / 2, 0, (map.numTilesTall - 1) / 2);
				var bx = Math.Clamp(vr.lowerRightX, 0, map.numTilesWide - 2);
				var by = Math.Clamp(vr.lowerRightY / 2, 0, (map.numTilesTall - 1) / 2);

				// Render
				DrawLineOnImage(mapImage, ax, ay, ax, by, Colors.White);
				DrawLineOnImage(mapImage, ax, by, bx, by, Colors.White);
				DrawLineOnImage(mapImage, bx, by, bx, ay, Colors.White);
				DrawLineOnImage(mapImage, bx, ay, ax, ay, Colors.White);
			}

			mapTexture.SetImage(mapImage);
			mapTexture.SetSizeOverride(miniMapSize);
		});
	}

	private (int x, int y) ComputeIsoCoordinates(Tile tile) {
		// Isometric tile dimensions - wider than tall for rhombus shape
		var x = tile.XCoordinate;
		var y = tile.YCoordinate / 2;
		return (x, y);
	}

	/// <summary>
	/// Draw a line on an Image using the SetPixel primitive.<br/>
	/// https://en.wikipedia.org/wiki/Bresenham%27s_line_algorithm
	/// </summary>
	private void DrawLineOnImage(Image image, int x0, int y0, int x1, int y1, Color color) {
		int dx = Math.Abs(x1 - x0);
		int dy = Math.Abs(y1 - y0);
		int sx = x0 < x1 ? 1 : -1;
		int sy = y0 < y1 ? 1 : -1;
		int err = dx - dy;

		while (true) {
			image.SetPixel(x0, y0, color);
			if (x0 == x1 && y0 == y1) break;
			int e2 = err * 2;
			if (e2 > -dy) { err -= dy; x0 += sx; }
			if (e2 < dx) { err += dx; y0 += sy; }
		}
	}

	public override void _GuiInput(InputEvent @event) {
		if (@event is InputEventMouseButton eventMouseButton) {
			Control uiHover = GetViewport().GuiGetHoveredControl();
			if (eventMouseButton.IsPressed() && uiHover is TextureRect) {
				switch (eventMouseButton.ButtonIndex) {
					case MouseButton.Left:
						HandleLeftMouseButton(eventMouseButton);
						break;
				}
			}
		}
	}

	private void HandleLeftMouseButton(InputEventMouseButton eventMouseButton) {
		var mapPos = eventMouseButton.Position - mapFrameRect.GlobalPosition - clickOffset;
		var relativeMapPos = mapPos / mapTextureRect.Size;
		CenterToPosition(mapPos, relativeMapPos);
	}

	private void CenterToPosition(Vector2 mapPos, Vector2 relativeMapPos) {
		EngineStorage.ReadGameData((GameData gameData) => {
			if (mapView == null)
				return;

			var mapSize = new Vector2(gameData.map.numTilesWide, gameData.map.numTilesTall);
			var mapLocation = relativeMapPos * mapSize;
			var (x, y) = mapView.tileCoordsForMapLocation(mapLocation);
			var tile = gameData.map.tileAt(x, y);
			mapView.centerCameraOnTile(tile);
		});
	}
}
