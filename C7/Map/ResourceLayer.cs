using C7GameData;
using Godot;
using Resource = C7GameData.Resource;
using Serilog;

namespace C7.Map {
	public partial class ResourceLayer : LooseLayer {
		private ILogger log = LogManager.ForContext<ResourceLayer>();

		private static readonly Vector2 resourceSize = new Vector2(50, 50);
		private ImageTexture resourceTexture;

		public ResourceLayer() {
			resourceTexture = TextureLoader.Load("resources");
		}

		public override void drawObject(LooseView looseView, GameData gameData, Tile tile, Vector2 tileCenter) {
			Resource resource = tile.Resource;
			if (resource == Resource.NONE) {
				return;
			}

			if (!ResourceVisible(gameData, tile)) {
				return;
			}

			Rect2 resourceRectangle = Util.GetResourceRect(resource);

			Rect2 screenTarget = new Rect2(tileCenter - 0.5f * resourceSize, resourceSize);
			looseView.DrawTextureRectRegion(resourceTexture, screenTarget, resourceRectangle);
		}

		private bool ResourceVisible(GameData gameData, Tile t) {
			if (gameData.observerMode) {
				return true;
			}
			return gameData.GetHumanPlayers()[0].KnowsAboutResource(t.Resource);
		}
	}
}
