using System.Collections.Generic;
using C7GameData;
using Godot;
using ConvertCiv3Media;

namespace C7.Map {
	// The layer responsible for drawing civilization borders.
	public class BorderLayer : LooseLayer {
		private readonly string texturePath = "Art/Terrain/Territory.pcx";
		private readonly ImageTexture[] borderGraphics = new ImageTexture[8];

		private readonly Dictionary<TileDirection, ImageTexture> directionToTexture = new();
		private readonly Dictionary<(TileDirection, Color), ImageTexture> textureCache = new();

		public BorderLayer() {
			directionToTexture[TileDirection.NORTHWEST] = TextureLoader.Load("terrain.borders.northwest_flat");
			directionToTexture[TileDirection.NORTHEAST] = TextureLoader.Load("terrain.borders.northeast_flat");
			directionToTexture[TileDirection.SOUTHWEST] = TextureLoader.Load("terrain.borders.southwest_flat");
			directionToTexture[TileDirection.SOUTHEAST] = TextureLoader.Load("terrain.borders.southeast_flat");
		}

		// TODO: This method doesn't precisely mirror Civ3 coloring
		private static Color CalcSecondaryColor(Color mainColor) {
			return new Color(mainColor.R * 0.8f, mainColor.G * 0.8f, mainColor.B * 0.8f);
		}

		/// This method retrieves a border texture of a specific index and color.
		/// It works by replacing two base colors in a border texture template with new colors:
		/// - The color of civilization, passed as an argument to this method.
		/// - The secondary color, which is derived from the civilization color.
		/// The resulting texture is cached.
		private ImageTexture GetBorderTexture(TileDirection dir, Color borderColor) {
			if (textureCache.TryGetValue((dir, borderColor), out ImageTexture res)) {
				return res;
			}

			ImageTexture texture = directionToTexture[dir];

			Color secondaryColor = CalcSecondaryColor(borderColor);

			Dictionary<Color, Color> colorReplacements = new Dictionary<Color, Color>
			{
				{ Color.Color8(236, 255, 0), borderColor },
				{ Color.Color8(255, 0, 0), secondaryColor}
			};

			Image image = Util.TransformColors(texture.GetImage(), colorReplacements);

			var newTexture = ImageTexture.CreateFromImage(image);
			textureCache[(dir, borderColor)] = newTexture;

			return newTexture;
		}

		public override void drawObject(LooseView looseView, GameData gameData, Tile tile, Vector2 tileCenter) {
			if (tile.owningCity is null) {
				return;
			}

			Color borderColor = TextureLoader.LoadColor(tile.owningCity.owner.colorIndex);

			foreach (TileDirection dir in directionToTexture.Keys) {
				if (tile.neighbors[dir].owningCity?.owner != tile.owningCity?.owner) {
					ImageTexture texture = GetBorderTexture(dir, borderColor);
					Vector2 size = texture.GetSize();
					Vector2 offset = size/2;
					// this value were found experimentally to improve alignment with the grid
					offset.Y += size.Y * 0.055f;

					looseView.DrawTexture(texture, tileCenter - offset);
				}
			}
		}
	}
}
