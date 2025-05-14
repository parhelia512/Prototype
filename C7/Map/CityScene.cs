using C7GameData;
using ConvertCiv3Media;
using Godot;
using Serilog;
using System;

namespace C7.Map {
	public partial class CityScene : Node2D {
		private ILogger log = LogManager.ForContext<CityScene>();

		private readonly Vector2 citySpriteSize;

		private ImageTexture cityTexture;
		private TextureRect cityGraphics = new TextureRect();
		private CityLabelScene cityLabelScene;
		private AnimatedSprite2D disorderSprite;
		private City city;

		public CityScene(City city, Vector2I tileCenter) {
			cityLabelScene = new CityLabelScene(city, tileCenter);
			this.city = city;

			//TODO: Generalize, support multiple city types, etc.
			Pcx pcx = TextureLoader.LoadPCX("Art/Cities/rMIDEAST.PCX");
			int height = pcx.Height/4;
			int width = pcx.Width/3;
			cityTexture = TextureLoader.LoadFromPCX("Art/Cities/rMIDEAST.PCX", new(0, 0, width, height), false);
			citySpriteSize = new Vector2(width, height);

			cityGraphics.OffsetLeft = tileCenter.X - (float)0.5 * citySpriteSize.X;
			cityGraphics.OffsetTop = tileCenter.Y - (float)0.5 * citySpriteSize.Y;
			cityGraphics.MouseFilter = Control.MouseFilterEnum.Ignore;
			cityGraphics.Texture = cityTexture;

			AddChild(cityGraphics);
			AddChild(cityLabelScene);

			disorderSprite = new();
			SpriteFrames frames = new();
			disorderSprite.SpriteFrames = frames;
			AnimationManager.loadNonTintedAnimation("Art/Animations/Disorder/DisorderDefault.flc", "disorder", ref frames);
			disorderSprite.Animation = "disorder";
			disorderSprite.Position = tileCenter + new Vector2(0, -32);
			AddChild(disorderSprite);
			disorderSprite.Play("disorder");
		}

		public override void _Draw() {
			base._Draw();
			cityLabelScene._Draw();

			if (city.isInCivilDisorder) {
				disorderSprite.Show();
			} else {
				disorderSprite.Hide();
			}
		}
	}
}
