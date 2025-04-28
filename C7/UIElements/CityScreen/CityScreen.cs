using Godot;
using System;
using Serilog;
using System.Collections.Generic;
using C7GameData;
using C7.Map;
using C7Engine;
using C7Engine.AI;


// Handles the city screen, where citizens can be assigned and other details of
// the city can bee seen.
[Tool]
public partial class CityScreen : Control {
	private ILogger log = LogManager.ForContext<CityScreen>();
	public TileAssignmentLayer tileAssignmentLayer;
	public MapView mapView;
	public List<CitizenType> citizenTypes;
	private List<TextureButton> popHeads = new();

	[Export] private TextureRect background;
	[Export] private VBoxContainer existingBuildings;
	[Export] private Label culturePerTurn;
	[Export] private Label totalCulture;
	[Export] private Label cityName;

	[Export] private TextureButton productionButton;
	[Export] private TextureButton close;
	[Export] private TextureButton previousCity;
	[Export] private TextureButton nextCity;

	[Export] private ProductionMenu productionMenu;

	[Export] private HBoxContainer strategicResources;
	[Export] private VBoxContainer luxuriesContainer;

	Theme yieldDetailsFontTheme = new();
	FontFile yieldDetailsFont = new();

	private Label foodDetails;
	private Label productionDetails;
	private Label commerceTaxesDetails;
	private Label commerceScienceDetails;
	private Label commerceHappinessDetails;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		background.Texture = Util.LoadTextureFromPCX("Art/city screen/background.pcx");

		// The close button.
		close.TextureNormal = Util.LoadTextureFromPCX("Art/city screen/cityMgmtButtons.pcx", 155, 1, 32, 48);
		close.TextureHover = Util.LoadTextureFromPCX("Art/city screen/cityMgmtButtons.pcx", 155, 50, 32, 48);
		close.TexturePressed = Util.LoadTextureFromPCX("Art/city screen/cityMgmtButtons.pcx", 155, 99, 32, 48);

		close.Pressed += Hide;

		previousCity.TextureNormal = Util.LoadTextureFromPCX("Art/city screen/cityMgmtButtons.pcx", 1, 1, 40, 48);
		previousCity.TextureHover = Util.LoadTextureFromPCX("Art/city screen/cityMgmtButtons.pcx", 1, 50, 40, 48);
		previousCity.TexturePressed = Util.LoadTextureFromPCX("Art/city screen/cityMgmtButtons.pcx", 1, 99, 40, 48);
		previousCity.Pressed += SwitchToPreviousCity;

		nextCity.TextureNormal = Util.LoadTextureFromPCX("Art/city screen/cityMgmtButtons.pcx", 42, 1, 40, 48);
		nextCity.TextureHover = Util.LoadTextureFromPCX("Art/city screen/cityMgmtButtons.pcx", 42, 50, 40, 48);
		nextCity.TexturePressed = Util.LoadTextureFromPCX("Art/city screen/cityMgmtButtons.pcx", 42, 99, 40, 48);
		nextCity.Pressed += SwitchToNextCity;

		// Load the font we'll use for the details.
		//
		// We skip the cache so that we can change the size without affecting other
		// code using the same font.
		yieldDetailsFont = ResourceLoader.Load<FontFile>("res://Fonts/NotoSans-Regular.ttf", null, ResourceLoader.CacheMode.Ignore);
		yieldDetailsFont.FixedSize = 20;

		yieldDetailsFontTheme.DefaultFont = yieldDetailsFont;
		yieldDetailsFontTheme.SetColor("font_color", "Label", Colors.Black);
		yieldDetailsFontTheme.SetFontSize("font_size", "Label", 20);

		foodDetails = new Label() {
			OffsetLeft = 290,
			OffsetTop = 567,
			Theme = yieldDetailsFontTheme,
		};
		background.AddChild(foodDetails);

		productionDetails = new Label() {
			OffsetLeft = 290,
			OffsetTop = 520,
			Theme = yieldDetailsFontTheme,
		};
		background.AddChild(productionDetails);

		commerceTaxesDetails = new Label() {
			OffsetLeft = 290,
			OffsetTop = 620,
			Theme = yieldDetailsFontTheme,
		};
		background.AddChild(commerceTaxesDetails);

		commerceScienceDetails = new Label() {
			OffsetLeft = 290,
			OffsetTop = 653,
			Theme = yieldDetailsFontTheme,
		};
		background.AddChild(commerceScienceDetails);

		commerceHappinessDetails = new Label() {
			OffsetLeft = 290,
			OffsetTop = 685,
			Theme = yieldDetailsFontTheme,
		};
		background.AddChild(commerceHappinessDetails);

		productionButton.TextureNormal = Util.LoadTextureFromPCX("Art/city screen/ProdButton.pcx", 1, 0, 114, 95);
		productionButton.TextureHover = Util.LoadTextureFromPCX("Art/city screen/ProdButton.pcx", 116, 0, 115, 95);
		productionButton.TexturePressed = Util.LoadTextureFromPCX("Art/city screen/ProdButton.pcx", 231, 0, 115, 95);

		productionButton.Pressed += () => { this.productionMenu.Visible = !this.productionMenu.Visible; };

		Hidden += OnExit;
	}

	public override void _UnhandledInput(InputEvent @event) {
		// Only capture mouse events if we're visible.
		if (!this.Visible) {
			return;
		}

		// If we left clicked on a tile, handle reassigning a citizen accordingly.
		if (@event is InputEventMouseButton eventMouseButton) {
			if (eventMouseButton.ButtonIndex == MouseButton.Left) {
				GetViewport().SetInputAsHandled();
				if (eventMouseButton.IsPressed()) {
					using UIGameDataAccess gameDataAccess = new();
					if (productionMenu != null) {
						productionMenu.Visible = false;
					}
					Tile tile = mapView.tileOnScreenAt(gameDataAccess.gameData.map, eventMouseButton.Position);
					if (tile != null) {
						HandleReassignment(tile);

						// Recalculate moods after changing tile assignments.
						tileAssignmentLayer.city.RecalculateCitizenMoods(gameDataAccess.gameData);

						RenderPopHeads(tileAssignmentLayer.city);
						RenderFoodDetails(tileAssignmentLayer.city);
						RenderCommerceDetails(tileAssignmentLayer.city);
						RenderProductionDetails(tileAssignmentLayer.city);
					}
				}
			}
		}
	}

	// Returns a list of specialists that this player can use.
	private List<CitizenType> GetKnownSpecialists(Player player) {
		return citizenTypes.FindAll(x => {
			return !x.IsDefaultCitizen && (x.PrerequisiteTech == null || player.knownTechs.Contains(x.PrerequisiteTech));
		});
	}

	private void HandleReassignment(Tile tile) {
		City city = tileAssignmentLayer.city;

		// We only support clicking on workable tiles or the city center.
		if (!city.GetWorkableTiles().Contains(tile) && tile != city.location) {
			return;
		}

		// We can't assign citizens to other cities.
		if (tile.cityAtTile != null && tile.cityAtTile != city) {
			return;
		}

		// We can't assign citizens to tiles worked by other cities.
		if (tile.personWorkingTile != null && tile.personWorkingTile.city != city) {
			return;
		}

		// If we're already working a tile and click on it, turn the citizen
		// into a specialist.
		if (tile.personWorkingTile != null && tile.personWorkingTile.city == city) {
			CityResident resident = tile.personWorkingTile;
			tile.personWorkingTile = null;
			resident.tileWorked = Tile.NONE;
			resident.citizenType = GetKnownSpecialists(city.owner)[0];
			return;
		}

		// We've clicked on an unworked tile, move the "worst" citizen to that
		// tile.
		if (tile.cityAtTile == null) {
			int worstYield = int.MaxValue;
			CityResident worst = null;

			foreach (CityResident cr in city.residents) {
				int tileYield = cr.tileWorked.foodYield(city.owner).yield +
								cr.tileWorked.productionYield(city.owner).yield +
								cr.tileWorked.commerceYield(city.owner).yield;
				if (tileYield < worstYield) {
					worstYield = tileYield;
					worst = cr;
				}
			}

			// Move the worst citizen to our new tile, being sure to update
			// backpointers from the tile.
			worst.tileWorked.personWorkingTile = null;
			worst.tileWorked = tile;
			tile.personWorkingTile = worst;
			worst.citizenType = citizenTypes.Find(x => x.IsDefaultCitizen);
			return;
		}

		// If we've clicked the city center, re-assign all the citizens using
		// the basic AI.
		//
		// TODO: This throws away existing nationalities, fix that.
		if (tile.cityAtTile == city) {
			int numResidents = city.residents.Count;
			city.RemoveAllCitizens();

			for (int i = 0; i < numResidents; ++i) {
				CityResident newResident = new() {
					citizenType = citizenTypes.Find(x => x.IsDefaultCitizen),
					nationality = city.owner.civilization,
					city = city
				};
				city.AddCitizen(newResident);
				CityTileAssignmentAI.AssignNewCitizenToTile(newResident);
			}
		}
	}

	private void OnShowCityScreen(ParameterWrapper<City> city) {
		this.Show();
		mapView.centerCameraOnTile(city.Value.location.neighbors[TileDirection.SOUTH]);
		tileAssignmentLayer.city = city.Value;
		cityName.Text = city.Value.name;
		RenderPopHeads(city.Value);
		RenderCulture(city.Value);
		RenderFoodDetails(city.Value);
		RenderCommerceDetails(city.Value);
		RenderProductionDetails(city.Value);
		RenderExistingBuildings(city.Value.buildings);
		RenderStrategicResources(city.Value);
		RenderLuxuries(city.Value);
	}

	private void OnExit() {
		productionMenu.Hide();
		tileAssignmentLayer.city = null;
	}

	private void RenderStrategicResources(City city) {
		Dictionary<C7GameData.Resource, int> resourceCounter = city.GetStrategicResources();

		foreach (var child in strategicResources.GetChildren()) {
			strategicResources.RemoveChild(child);
		}

		foreach ((C7GameData.Resource resource, int count) in resourceCounter) {
			VBoxContainer resourceContainer = new();
			resourceContainer.AddThemeConstantOverride("separation", 0);

			var texture = Util.GetResourceTexture(resource);
			texture.SetSizeOverride(new(45, 45));

			TextureRect resourceRect = new() {
				Texture = texture,
			};

			Label resourceLabel = new() {
				Text = count.ToString(),
				HorizontalAlignment = HorizontalAlignment.Center
			};

			resourceContainer.AddChild(resourceRect);
			resourceContainer.AddChild(resourceLabel);

			strategicResources.AddChild(resourceContainer);
		}
	}

	private void RenderLuxuries(City city) {
		Dictionary<C7GameData.Resource, int> resourceCounter = city.GetLuxuries();

		foreach (var child in luxuriesContainer.GetChildren()) {
			luxuriesContainer.RemoveChild(child);
		}

		foreach ((C7GameData.Resource resource, int count) in resourceCounter) {
			HBoxContainer resourceContainer = new();

			Label resourceCount = new() {
				Text = "(" + count.ToString() + ")"
			};

			Label resourceName = new() {
				Text = resource.Name
			};

			resourceContainer.AddChild(resourceCount);
			resourceContainer.AddChild(resourceName);

			luxuriesContainer.AddChild(resourceContainer);
		}
	}

	private void RenderExistingBuildings(List<CityBuilding> buildings) {
		foreach (var node in existingBuildings.GetChildren()) {
			existingBuildings.RemoveChild(node);
		}

		foreach (CityBuilding building in buildings) {
			Label label = new() {
				Text = building.building.name
			};
			existingBuildings.AddChild(label);
		}
	}

	private void RenderFoodDetails(City city) {
		foodDetails.Text = $"{city.CurrentFoodYield()} food/turn, {city.FoodGrowthPerTurn()} surplus. {city.foodStored} stored. Growth in {city.TurnsUntilGrowth()} turns.";
	}

	private void RenderCommerceDetails(City city) {
		CommerceBreakdown breakdown = city.CurrentCommerceYield();
		commerceTaxesDetails.Text = $"{breakdown.taxes} gold/turn to taxes";
		commerceScienceDetails.Text = $"{breakdown.beakers} gold/turn to science  ({breakdown.corrupted} corrupt)";
		commerceHappinessDetails.Text = $"{breakdown.happiness} gold/turn to happiness";
	}

	private void RenderProductionDetails(City city) {
		CorruptableValue shields = city.CurrentProductionYield();
		string turnsLeft = city.TurnsUntilProductionFinished() == int.MaxValue ? "--" : $"{city.TurnsUntilProductionFinished()} turns left";
		productionDetails.Text = $"{shields.useful + shields.corrupt} shields/turn ({shields.useful} usable, {shields.corrupt} corrupt). {city.shieldsStored} of {city.itemBeingProduced.shieldCost} stored. {turnsLeft}";

		foreach (Node child in productionButton.GetChildren()) {
			child.QueueFree();
		}

		if (city.itemBeingProduced is UnitPrototype up) {
			// Get the flic data for the unit we're producing.
			string path = new AnimationManager(null).getUnitFlicFilepath(up, MapUnit.AnimatedAction.DEFAULT);
			ConvertCiv3Media.Flic flic = Util.LoadFlic(path);

			// Set up a shader we can use to color the "tint" portion of the
			// animation frame below.
			ShaderMaterial material = new();
			material.Shader = GD.Load<Shader>("res://UnitTint.gdshader");
			Color civColor = Util.LoadColor(city.owner.colorIndex);
			material.SetShaderParameter("tintColor", new Vector3(civColor.R, civColor.G, civColor.B));

			// See flicRowToAnimationDirection for the mapping, row 2 is facing
			// southeast, and we're just grabbing frame 0.
			byte[] frame = flic.Images[2, 0];

			// Each frame is split in two parts, the base image, and the "tint"
			// of the image, which is the part of the unit that has civ-specific
			// colors.
			(ImageTexture baseImage, ImageTexture imageTint) = Util.LoadTextureFromFlicData(frame, flic.Palette, flic.Width, flic.Height);

			// Add the base sprite.
			Sprite2D baseImageSprite = new();
			baseImageSprite.Texture = baseImage;
			baseImageSprite.Position = new Vector2(productionButton.TextureNormal.GetWidth() / 2, 35);
			productionButton.AddChild(baseImageSprite);

			// Add the tint sprite, hooking up the shader.
			Sprite2D imageTintSprite = new Sprite2D();
			imageTintSprite.Texture = imageTint;
			imageTintSprite.Material = material;
			imageTintSprite.Position = baseImageSprite.Position;
			productionButton.AddChild(imageTintSprite);

			Label productionButtonLabel = new();
			productionButton.AddChild(productionButtonLabel);
			productionButtonLabel.SetPosition(new Vector2(0, 65));
			productionButtonLabel.SetTextAndCenterLabel($"{city.itemBeingProduced.name}");
		}

		productionMenu.AddItems(city, (IProducible p) => {
			using UIGameDataAccess gameDataAccess = new();
			city.SetItemBeingProduced(p);
			RenderProductionDetails(city);
		});
	}

	private void RenderCulture(City city) {
		culturePerTurn.Text = $"{city.GetCulturePerTurn()}/turn";

		int nextCultureExpansion = (int)Math.Pow(10, city.GetBorderExpansionLevel());
		totalCulture.Text = $"Total: {city.GetCulture()}/{nextCultureExpansion}";
	}

	private void RenderPopHeads(City city) {
		// Reset any old heads.
		foreach (TextureButton head in popHeads) {
			background.RemoveChild(head);
			head.QueueFree();
		}
		popHeads.Clear();

		int eraNum = city.owner.EraIndex();

		// The pop head textures are 50 x 50, but have a 1px border on all sides
		//
		// The texture file has 16 rows of the default citizen, in groups of 4
		// per era (content, happy, resisting, unhappy). There are 10 columns,
		// the first 5 are male heads of different regions for civs, the other 5
		// are female heads.
		//
		// After the 16 rows of default citizens there is one row per specialist
		// type, and again 10 columns per row. This time they are
		// (ancient, middle, industrial, modern, blank) for male and female heads
		//
		// TODO: handle per-civ regions
		// TODO: handle male/female citizens

		// Start by splitting the default residents from the specialists, since
		// they are spaced apart in the UI.
		List<CityResident> happyResidents =
			city.residents.FindAll(x => x.citizenType.IsDefaultCitizen && x.mood == CityResident.Mood.Happy);
		List<CityResident> contentResidents =
			city.residents.FindAll(x => x.citizenType.IsDefaultCitizen && x.mood == CityResident.Mood.Content);
		List<CityResident> unhappyResidents =
			city.residents.FindAll(x => x.citizenType.IsDefaultCitizen && x.mood == CityResident.Mood.Unhappy);
		List<CityResident> specialists = city.residents.FindAll(x => !x.citizenType.IsDefaultCitizen);

		// Leave a 1 head gap if we have specialists.
		int width = city.residents.Count * PopHeads.HEAD_SIZE;
		if (specialists.Count > 0) {
			width += PopHeads.HEAD_SIZE;
		}

		// Leave a 1 head gap between each section of moods.
		int numMoodsPresent = (happyResidents.Count > 0 ? 1 : 0)
			+ (contentResidents.Count > 0 ? 1: 0)
			+ (unhappyResidents.Count > 0 ? 1: 0);
		width += (numMoodsPresent - 1) * PopHeads.HEAD_SIZE;

		// Track the x position of each head so that we're centered in the screen
		int xPos = background.Texture.GetWidth() / 2 + -width / 2;

		// Add each of the default citizens. These are buttons with the idea that
		// we can eventually support clicking on the heads to view details, such
		// as the reason for unhappiness.
		foreach (CityResident cr in happyResidents) {
			xPos = AddDefaultCitizen(cr, xPos, eraNum);
		}
		if (happyResidents.Count > 0 && (contentResidents.Count > 0 || unhappyResidents.Count > 0)) {
			xPos += PopHeads.HEAD_SIZE;
		}
		foreach (CityResident cr in contentResidents) {
			xPos = AddDefaultCitizen(cr, xPos, eraNum);
		}
		if (contentResidents.Count > 0 && unhappyResidents.Count > 0) {
			xPos += PopHeads.HEAD_SIZE;
		}
		foreach (CityResident cr in unhappyResidents) {
			xPos = AddDefaultCitizen(cr, xPos, eraNum);
		}

		// Add space before specialists.
		xPos += PopHeads.HEAD_SIZE;

		// Add each of the specialists.
		//
		// TODO: Render the specialist effect (like a smiley for entertainers)
		// in the corner of the head.
		foreach (CityResident cr in specialists) {
			TextureButton tb = new();
			tb.TextureNormal = PopHeads.GetPopHead(cr, eraNum);
			tb.SetPosition(new Vector2(xPos, 440));

			List<CitizenType> specialistTypes = GetKnownSpecialists(city.owner);
			int index = specialistTypes.FindIndex(x => x.Id == cr.citizenType.Id);
			tb.Pressed += () => {
				cr.citizenType = specialistTypes[(index + 1) % specialistTypes.Count];
				++index;
				RenderPopHeads(city);
			};

			background.AddChild(tb);
			popHeads.Add(tb);
			xPos += PopHeads.HEAD_SIZE;
		}
	}

	private void SwitchToNextCity() {
		using UIGameDataAccess gameDataAccess = new();
		City currentCity = tileAssignmentLayer.city;
		List<City> cities = currentCity.owner.cities;
		City nextCity = cities[(cities.IndexOf(currentCity) + 1) % cities.Count];
		nextCity.RecalculateCitizenMoods(gameDataAccess.gameData);
		OnShowCityScreen(new ParameterWrapper<City>(nextCity));
	}

	private void SwitchToPreviousCity() {
		using UIGameDataAccess gameDataAccess = new();
		City currentCity = tileAssignmentLayer.city;
		List<City> cities = currentCity.owner.cities;
		City previousCity = cities[(cities.IndexOf(currentCity) + cities.Count - 1) % cities.Count];
		previousCity.RecalculateCitizenMoods(gameDataAccess.gameData);
		OnShowCityScreen(new ParameterWrapper<City>(previousCity));
	}

	private int AddDefaultCitizen(CityResident cr, int xPos, int eraNum) {
		TextureButton tb = new();
		tb.TextureNormal = PopHeads.GetPopHead(cr, eraNum);
		tb.SetPosition(new Vector2(xPos, 440));
		background.AddChild(tb);
		popHeads.Add(tb);
		return xPos + PopHeads.HEAD_SIZE;
	}
}
