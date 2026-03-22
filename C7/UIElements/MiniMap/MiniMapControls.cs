using System;
using C7GameData;
using Godot;

public partial class MiniMapControls : Control {
	public override void _Ready() {
		MouseFilter = MouseFilterEnum.Stop;
	}

	/// Render the viewport bounds as a rectangle on the minimap image
	public void DrawBounds(Image mapImage, GameMap map, MapView.VisibleRegion vr) {
		// TODO: Handle draws over map edges, maybe with Mathf.Wrap

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
}
