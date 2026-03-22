using C7Engine;
using C7GameData;
using Godot;

public partial class MiniMapFrame : TextureRect {
	private TextureRect mapFrameRect;

	private MapView mapView;
	private TextureRect mapTextureRect;
	private ImageTexture mapTexture;

	private Vector2I miniMapFrameSize = new (280, 130);
	private Vector2I miniMapSize = new (229, 105);
	private Vector2I frameOffset = new (7, -12 + -10); // overall control has 10px boundary, adjust for VP
	private Vector2I mapOffset = new (25, -13); // offset inside the frame
	private Vector2I clickOffset = new (15, 0); // offset delta?

	public MiniMapFrame(MapView mapView) {
		this.mapView = mapView;

		// TODO: Draw frame on top of the map texture (figure out stencil alpha)

		// Draw frame
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

	public override void _Ready() {
		MouseFilter = MouseFilterEnum.Pass;
	}

	public void SetViewportPosition() {
		// Position frame and map relative to viewport
		var vp = GetViewportRect().Size;
		mapFrameRect.SetPosition(frameOffset + new Vector2(0, vp.Y - miniMapFrameSize.Y));
		mapTextureRect.SetPosition(frameOffset + new Vector2(0, vp.Y - miniMapSize.Y) + mapOffset);
	}

	public void RenderImage(Image mapImage) {
		mapTexture.SetImage(mapImage);
		mapTexture.SetSizeOverride(miniMapSize);
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
