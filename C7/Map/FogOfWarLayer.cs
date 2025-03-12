using System.Collections.Generic;
using System.Linq;
using C7GameData;
using ConvertCiv3Media;
using Godot;

namespace C7.Map {
	public partial class FogOfWarLayer : LooseLayer {

		private readonly ImageTexture fogOfWarTexture;
		private readonly Vector2 tileSize;
		private HashSet<Tile> activeTiles = new HashSet<Tile>();

		public FogOfWarLayer() {
			Pcx fogOfWarPcx = new Pcx(Util.Civ3MediaPath("Art/Terrain/FogOfWar.pcx"));
			fogOfWarTexture = PCXToGodot.getPureAlphaFromPCX(fogOfWarPcx);
			tileSize = fogOfWarTexture.GetSize() / 9;
		}

		public void ComputeTileKnowledge(GameData gameData) {
			activeTiles.Clear();

			var player = gameData.GetHumanPlayers()[0];
			activeTiles = player.cities
				.SelectMany(x => x.GetTilesWithinBorders().SelectMany(y => y.neighbors.Values).Append(x.location))
				.Concat(player.units.SelectMany(x => x.location.neighbors.Values.Append(x.location)))
				.ToHashSet();
		}

		public override void drawObject(LooseView looseView, GameData gameData, Tile tile, Vector2 tileCenter) {

			// Fog of war is computed from the squares' intersections.
			// Thus, for each iteration, we have to consider that we are at the north intersection of the iterated tile

			Tile north = tile.neighbors[TileDirection.NORTH];
			Tile south = tile;
			Tile east = tile.neighbors[TileDirection.NORTHEAST];
			Tile west = tile.neighbors[TileDirection.NORTHWEST];

			TileKnowledge tileKnowledge = gameData.GetHumanPlayers()[0].tileKnowledge;
			int column = 0;
			int row = 0;

			if (activeTiles.Contains(north)) {
				column += 2;
			} else if (tileKnowledge.isTileKnown(north)) {
				column += 1;
			}

			if (activeTiles.Contains(west)) {
				column += 6;
			} else if (tileKnowledge.isTileKnown(west)) {
				column += 3;
			}

			if (activeTiles.Contains(east)) {
				row += 2;
			} else if (tileKnowledge.isTileKnown(east)) {
				row += 1;
			}

			if (activeTiles.Contains(south)) {
				row += 6;
			} else if (tileKnowledge.isTileKnown(south)) {
				row += 3;
			}

			var fogOrigin = new Vector2(tileCenter.X - tileSize.X/2, tileCenter.Y - tileSize.Y);
			var fogRect = new Rect2(fogOrigin, tileSize);
			var spriteRect = new Rect2(column * tileSize.X, row * tileSize.Y, tileSize * 0.999f);
			looseView.DrawTextureRectRegion(fogOfWarTexture, fogRect, spriteRect);
		}
	}
}
