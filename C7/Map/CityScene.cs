using C7GameData;
using ConvertCiv3Media;
using Godot;
using Serilog;
using System;

namespace C7.Map {
	public partial class CityScene : Node2D {
		private ILogger log = LogManager.ForContext<CityScene>();

		private ImageTexture cityTexture;
		private TextureRect cityGraphics = new TextureRect();
		private CityLabelScene cityLabelScene;
		private AnimatedSprite2D disorderSprite;
		private City city;
		private Rules rules;
		private int cachedCitySize = -1;
		private int cachedEraIndex = -1;
		private Vector2I tileCenter;

		public CityScene(City city, Vector2I tileCenter) {
			cityLabelScene = new CityLabelScene(city, tileCenter);
			this.city = city;
			this.rules = city.owner.rules;
			this.tileCenter = tileCenter;

			cachedCitySize = city.residents.Count;
			cachedEraIndex = city.owner.EraIndex();
			ConfigureCityGraphics(cachedCitySize, cachedEraIndex);

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

			if (city.residents.Count != cachedCitySize || city.owner.EraIndex() != cachedEraIndex) {
				cachedCitySize = city.residents.Count;
				cachedEraIndex = city.owner.EraIndex();
				ConfigureCityGraphics(cachedCitySize, cachedEraIndex);
			}

			if (city.isInCivilDisorder) {
				disorderSprite.Show();
			} else {
				disorderSprite.Hide();
			}
		}

		//TODO: Support multiple city flavors and walls.
		private void ConfigureCityGraphics(int citySize, int era) {
			int size = 0;
			if (citySize > rules.MaximumLevel1CitySize) {
				++size;
			}
			if (citySize > rules.MaximumLevel2CitySize) {
				++size;
			}

			cityTexture = TextureLoader.Load("cities", new { size, era });

			cityGraphics.OffsetLeft = tileCenter.X - (float)0.5 * cityTexture.GetWidth();
			cityGraphics.OffsetTop = tileCenter.Y - (float)0.5 * cityTexture.GetHeight();
			cityGraphics.MouseFilter = Control.MouseFilterEnum.Ignore;
			cityGraphics.Texture = cityTexture;
		}
	}
}
