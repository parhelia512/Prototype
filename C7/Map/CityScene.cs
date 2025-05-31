using C7GameData;
using ConvertCiv3Media;
using Godot;
using Serilog;
using System;

namespace C7.Map {
	public record struct CityGraphicsDetails(
		// A size rank of 0 is a town, 1 a city, etc.
		int sizeRank,
		int eraIndex
	);

	public partial class CityScene : Node2D {
		private ILogger log = LogManager.ForContext<CityScene>();

		private ImageTexture cityTexture;
		private TextureRect cityGraphics = new TextureRect();
		private CityLabelScene cityLabelScene;
		private AnimatedSprite2D disorderSprite;
		private City city;
		private Rules rules;
		private CityGraphicsDetails cachedDetails;
		private Vector2I tileCenter;

		public CityScene(City city, Vector2I tileCenter) {
			cityLabelScene = new CityLabelScene(city, tileCenter);
			this.city = city;
			this.rules = city.owner.rules;
			this.tileCenter = tileCenter;

			cachedDetails = GetCityGraphicsDetails(city);
			ConfigureCityGraphics(cachedDetails);

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

			if (cachedDetails != GetCityGraphicsDetails(city)) {
				cachedDetails = GetCityGraphicsDetails(city);
				ConfigureCityGraphics(cachedDetails);
			}

			if (city.isInCivilDisorder) {
				disorderSprite.Show();
			} else {
				disorderSprite.Hide();
			}
		}

		private CityGraphicsDetails GetCityGraphicsDetails(City c) {
			CityGraphicsDetails result = new() { sizeRank = 0 };
			if (c.residents.Count > rules.MaximumLevel1CitySize) {
				++result.sizeRank;
			}
			if (c.residents.Count > rules.MaximumLevel2CitySize) {
				++result.sizeRank;
			}
			result.eraIndex = city.owner.EraIndex();
			return result;
		}

		//TODO: Support multiple city flavors and walls.
		private void ConfigureCityGraphics(CityGraphicsDetails details) {
			cityTexture = TextureLoader.Load("cities", details);

			cityGraphics.OffsetLeft = tileCenter.X - (float)0.5 * cityTexture.GetWidth();
			cityGraphics.OffsetTop = tileCenter.Y - (float)0.5 * cityTexture.GetHeight();
			cityGraphics.MouseFilter = Control.MouseFilterEnum.Ignore;
			cityGraphics.Texture = cityTexture;
		}
	}
}
