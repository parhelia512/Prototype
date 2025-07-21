using System.Collections.Generic;
using C7GameData;
using Godot;
using ConvertCiv3Media;

namespace C7.Map {
	// The layer responsible for drawing civilization borders.
	public class BorderLayer : LooseLayer {
		private readonly string texturePath = "Art/Terrain/Territory.pcx";
		private readonly ImageTexture[] borderGraphics = new ImageTexture[8];
		private readonly Dictionary<(int, Color), ImageTexture> textureCache = new();

		public BorderLayer() {
			borderGraphics[0] = TextureLoader.Load("terrain.borders.northwest_flat");
			borderGraphics[2] = TextureLoader.Load("terrain.borders.northeast_flat");
			borderGraphics[4] = TextureLoader.Load("terrain.borders.southwest_flat");
			borderGraphics[6] = TextureLoader.Load("terrain.borders.southeast_flat");
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
		private ImageTexture GetBorderTexture(int textureIndex, Color borderColor) {
			if (textureCache.TryGetValue((textureIndex, borderColor), out ImageTexture res)) {
				return res;
			}

			ImageTexture texture = borderGraphics[textureIndex];

			Color secondaryColor = CalcSecondaryColor(borderColor);

			Dictionary<Color, Color> colorReplacements = new Dictionary<Color, Color>
			{
				{ Color.Color8(236, 255, 0), borderColor },
				{ Color.Color8(255, 0, 0), secondaryColor}
			};

			Image image = Util.TransformColors(texture.GetImage(), colorReplacements);

			var newTexture = ImageTexture.CreateFromImage(image);
			textureCache[(textureIndex, borderColor)] = newTexture;

			return newTexture;
		}

		public override void drawObject(LooseView looseView, GameData gameData, Tile tile, Vector2 tileCenter) {
			if (tile.owningCity is null) {
				return;
			}

			var directionToTextureIdx = new Dictionary<TileDirection, int>
				{
					{ TileDirection.NORTHWEST, 0 },
					{ TileDirection.NORTHEAST, 2 },
					{ TileDirection.SOUTHWEST, 4 },
					{ TileDirection.SOUTHEAST, 6 }
				};

			Color borderColor = Util.LoadColor(tile.owningCity.owner.colorIndex);

			foreach (var entry in directionToTextureIdx) {
				if (tile.neighbors[entry.Key].owningCity?.owner != tile.owningCity?.owner) {
					ImageTexture texture = GetBorderTexture(entry.Value, borderColor);
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
