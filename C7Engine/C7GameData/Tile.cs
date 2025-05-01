namespace C7GameData {
	using System;
	using System.Text.Json.Serialization;
	using System.Collections.Generic;
	using System.Linq;
	using C7GameData.Save;
	using C7Engine;

	public class Tile {
		public enum YieldType {
			Commerce,
			Food,
			Production
		}

		public class Yield {
			public static Yield CalculateForCity(Tile tile, int yield, YieldType type, City city) {
				return new Yield(tile, yield, type).ApplyPlayerModifiers(city.owner).ApplyCityModifiers(city);
			}

			public static Yield CalculateForPlayer(Tile tile, int yield, YieldType type, Player player) {
				return new Yield(tile, yield, type).ApplyPlayerModifiers(player);
			}

			public Yield(Tile tile, int baseYield, YieldType type) {
				this.tile = tile;
				this.baseYield = baseYield;
				this.type = type;
			}

			private Yield ApplyPlayerModifiers(Player player) {
				player.government.tileModifier?.Invoke(this);
				return this;
			}

			private Yield ApplyCityModifiers(City city) {
				city.buildings.ForEach(b => b.building.tileModifier?.Invoke(this));
				return this;
			}

			public readonly Tile tile;
			public readonly YieldType type;
			public int penalty = 0;
			public int bonus = 0;
			public readonly int baseYield = 0;
			public int yield { get => baseYield + bonus - penalty; }
		}

		public ID Id { get; internal set; }
		public Civ3ExtraInfo ExtraInfo;
		public int XCoordinate;
		public int YCoordinate;

		// Needed for coordinate wrapping.
		[JsonIgnore]
		public GameMap map;

		// An arbitrary number indicating which landmass this tile is part of,
		// for land-based tiles, or -1 for water.
		//
		// This is used to avoid the expensive process of pathfinding between
		// two land tiles just to discover they have no land connection.
		public int continent;

		public City owningCity; // The city whose border contains this tile
		public string baseTerrainTypeKey { get; set; }
		[JsonIgnore]
		public TerrainType baseTerrainType = TerrainType.NONE;
		public string overlayTerrainTypeKey { get; set; }
		[JsonIgnore]
		public TerrainType overlayTerrainType = TerrainType.NONE;
		public City cityAtTile;
		[JsonIgnore]
		public bool HasCity => cityAtTile != null && cityAtTile != City.NONE;
		public CityResident personWorkingTile = null;   //allows us to see if another city is working this tile
		public bool hasBarbarianCamp = false;
		//One thing to decide is do we want to have a tile have a list of units on it,
		//or a unit have reference to the tile it is on, or both?
		//The downside of both is that both have to be updated (and it uses a miniscule amount
		//of memory for pointers), but I'm inclined to go with both since it makes it easy and
		//efficient to perform calculations, whether you need to know which unit on a tile
		//has the best defense, or which tile a unit is on when viewing the Military Advisor.
		public List<MapUnit> unitsOnTile = new List<MapUnit>();
		public string ResourceKey { get; set; }
		[JsonIgnore]
		public Resource Resource { get; set; }

		public Dictionary<TileDirection, Tile> neighbors { get; set; } = new Dictionary<TileDirection, Tile>();

		//See discussion on page 4 of the "Babylon" thread (https://forums.civfanatics.com/threads/0-1-babylon-progress-thread.673959) about sub-terrain type and Civ3 properties.
		//We may well move these properties somewhere, whether that's Civ3ExtraInfo, a Civ3Tile child class, a Dictionary property, or something else, in the future.
		public bool isBonusShield;
		public bool isSnowCapped;
		public bool isPineForest;

		public bool riverNorth;
		public bool riverNortheast;
		public bool riverEast;
		public bool riverSoutheast;
		public bool riverSouth;
		public bool riverSouthwest;
		public bool riverWest;
		public bool riverNorthwest;

		public TileOverlays overlays = new TileOverlays();

		public Tile(ID id) {
			this.Id = id;
			unitsOnTile = new List<MapUnit>();
			Resource = Resource.NONE;
		}

		internal Tile() { }

		// TODO: this should be either an extension in C7Engine, or otherwise
		// calculated somewhere else, but it's not obvious to someone unfamiliar
		// with the save format that it's the overaly terrain that has actual
		// movement cost
		public int MovementCost() {
			return overlayTerrainType.movementCost;
		}

		public static Tile NONE = new Tile(ID.None("tile")) {
			XCoordinate = -1,
			YCoordinate = -1,
		};

		//This should be used when we want to check if land tiles are next to water tiles.
		//Usually this is coast, but it could be Sea - see the "Deepwater Harbours" topics at CFC.
		//Sometimes we care *specifically* about the Coast terrain, e.g. galleys can only move on that terrain, not Sea or Ocean
		//Those cases should not use this method.
		public bool NeighborsWater() {
			foreach (Tile neighbor in neighbors.Values) {
				if (neighbor.baseTerrainType.isWater()) {
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Returns neighbors along edges only.
		/// This is used by some graphics algorithms.
		/// </summary>
		/// <returns></returns>
		public Tile[] getEdgeNeighbors() {
			Tile[] edgeNeighbors =  { neighbors[TileDirection.NORTHEAST], neighbors[TileDirection.NORTHWEST], neighbors[TileDirection.SOUTHEAST], neighbors[TileDirection.SOUTHWEST]};
			return edgeNeighbors;
		}

		public override string ToString() {
			return "[" + XCoordinate + ", " + YCoordinate + "] (" + overlayTerrainType.DisplayName + " on " + baseTerrainType.DisplayName + ")";
		}

		public List<Tile> GetLandNeighbors() {
			return neighbors.Values.Where(tile => tile != NONE && !tile.baseTerrainType.isWater()).ToList();
		}

		/**
		 * Returns neighbors of the "Coast" type, not including Sea or Ocean.  This is used e.g. for Galley movement.
		 * Eventually, this should be refactored into a more general "get valid neighbors to move to" type of method,
		 * which could work e.g. for units that can move anywhere except desert.
		 **/
		public List<Tile> GetCoastNeighbors() {
			return neighbors.Values.Where(tile => tile.baseTerrainType.Key == "coast").ToList();
		}

		public bool HasRiverCrossing(TileDirection dir) {
			switch (dir) {
				case TileDirection.NORTH: return riverNorth;
				case TileDirection.NORTHEAST: return riverNortheast;
				case TileDirection.EAST: return riverEast;
				case TileDirection.SOUTHEAST: return riverSoutheast;
				case TileDirection.SOUTH: return riverSouth;
				case TileDirection.SOUTHWEST: return riverSouthwest;
				case TileDirection.WEST: return riverWest;
				case TileDirection.NORTHWEST: return riverNorthwest;
				default: throw new ArgumentOutOfRangeException("Invalid TileDirection");
			}
		}

		public bool IsLand() {
			return !baseTerrainType.isWater();
		}

		public bool IsWater() {
			return baseTerrainType.isWater();
		}

		public bool IsAllowCities() {
			return overlayTerrainType.allowCities;
		}

		public bool IsVolcano() {
			return overlayTerrainType.isVolcano();
		}

		public TileDirection directionTo(Tile other) {
			if ((this == NONE) || (other == NONE))
				throw new System.Exception("Can't get direction toward NONE Tile since it doesn't have a meaningful location");

			// We have to use the map helper functions to handle edge wrapping
			// correctly.
			//
			// y calculation is reversed so dy is in typical Cartesian coords
			// instead of tile coords, where y is inverted
			int dx = map.CalculateXDelta(other.XCoordinate, this.XCoordinate);
			int dy = map.CalculateYDelta(this.YCoordinate, other.YCoordinate);
			double angle = Math.Atan2(dy, dx); // angle is in interval [-pi, pi]

			if (angle > 7.0 / 8.0 * Math.PI) return TileDirection.WEST;
			else if (angle > 5.0 / 8.0 * Math.PI) return TileDirection.NORTHWEST;
			else if (angle > 3.0 / 8.0 * Math.PI) return TileDirection.NORTH;
			else if (angle > 1.0 / 8.0 * Math.PI) return TileDirection.NORTHEAST;
			else if (angle > -1.0 / 8.0 * Math.PI) return TileDirection.EAST;
			else if (angle > -3.0 / 8.0 * Math.PI) return TileDirection.SOUTHEAST;
			else if (angle > -5.0 / 8.0 * Math.PI) return TileDirection.SOUTH;
			else if (angle > -7.0 / 8.0 * Math.PI) return TileDirection.SOUTHWEST;
			else return TileDirection.WEST;
		}

		/**
		 * Distance as the raven flies to another tile.
		 * This is a rough metric only.
		 */
		public int distanceTo(Tile other) {
			if (this == Tile.NONE || other == Tile.NONE) {
				// We can't path to tiles that don't exist.
				return int.MaxValue;
			}
			return (Math.Abs(map.CalculateXDelta(other.XCoordinate, this.XCoordinate)) + Math.Abs(map.CalculateYDelta(other.YCoordinate, this.YCoordinate))) / 2;
		}

		// Returns the number of "ranks" to another tile, where each rank is a
		// border expansion due to culture. So rank 1 is immediate neighbors,
		// rank 2 is the "big fat cross", etc.
		public int rankDistanceTo(Tile other) {
			// Get the x and y deltas in the standard grid coordinates.
			int dx = Math.Abs(map.CalculateXDelta(other.XCoordinate, this.XCoordinate));
			int dy = Math.Abs(map.CalculateYDelta(other.YCoordinate, this.YCoordinate));

			// Transform that to the rank distance using the formula from
			// https://forums.civfanatics.com/threads/everything-about-corruption-c3c-edition.76619/post-1551201
			return (dx + dy) / 2 + Math.Abs(dx - dy) / 4;
		}

		private int baseFoodYield(Player player) {
			int yield = overlayTerrainType.baseFoodProduction;
			if (this.Resource != Resource.NONE && player.KnowsAboutResource(Resource)) {
				yield += this.Resource.FoodBonus;
			}

			if (this.overlays.irrigation) {
				yield += this.overlayTerrainType.irrigationBonus;
				if (this.overlays.railroad) {
					yield += 1;
				}
			}

			if (HasCity) {
				// All city centers have a food yield of 2, regardless of bonus
				// food. See https://wiki.civforum.de/wiki/Stadtfeldertrag_(Civ3).
				yield = 2;

				// TODO: For agricultural civilizations, the city field produces
				// a food yield of three food, but this is reduced to two by the
				// despotism penalty, unless the city is located on a fresh
				// water source or has already reached city size (≥ 7)
			}

			return yield;
		}

		public Yield foodYield(Player player) {
			int yield = baseFoodYield(player);
			return Yield.CalculateForPlayer(this, yield, YieldType.Food, player);
		}

		public Yield foodYield(City city) {
			int yield = baseFoodYield(city.owner);
			return Yield.CalculateForCity(this, yield, YieldType.Food, city);
		}

		private int baseProductionYield(Player player) {
			int yield = overlayTerrainType.baseShieldProduction;
			if (overlayTerrainType.Key == "grassland" && this.isBonusShield) {
				yield++;
			}

			if (HasCity) {
				// City centers always have 1 shield prior to any bonuses
				// resources, regardless of the terrain.
				// See https://wiki.civforum.de/wiki/Stadtfeldertrag_(Civ3).
				yield = 1;

				// There is a size bonus for larger cities.
				if (cityAtTile.size >= 7 && cityAtTile.size < 13) {
					yield += 1;
				} else if (cityAtTile.size >= 13) {
					yield += 2;

					// TODO: +1 more for industrial civs.
				}
			}

			// Bonus resources provide a boost in yield regardless of whether
			// there is a city.
			if (Resource != Resource.NONE && player.KnowsAboutResource(Resource)) {
				yield += this.Resource.ShieldsBonus;
			}

			if (this.overlays.mine) {
				yield += this.overlayTerrainType.miningBonus;
				if (this.overlays.railroad) {
					yield += 1;
				}
			}

			return yield;
		}

		public Yield productionYield(Player player) {
			int yield = baseProductionYield(player);
			return Yield.CalculateForPlayer(this, yield, YieldType.Production, player);
		}

		public Yield productionYield(City city) {
			int yield = baseProductionYield(city.owner);
			return Yield.CalculateForCity(this, yield, YieldType.Production, city);
		}

		private int baseCommerceYield(Player player) {
			int yield = overlayTerrainType.baseCommerceProduction;
			if (this.Resource != Resource.NONE && player.KnowsAboutResource(Resource)) {
				yield += this.Resource.CommerceBonus;
			}
			if (BordersRiver()) {
				yield += 1;
			}
			if (overlays.road) {
				yield += overlayTerrainType.roadBonus;
			}

			// See https://wiki.civforum.de/wiki/Stadtfeldertrag_(Civ3)
			if (HasCity) {
				int regularCityYield;
				if (cityAtTile.size < 7) {
					regularCityYield = 1;
				} else if (cityAtTile.size < 13) {
					regularCityYield = 2;
				} else {
					regularCityYield = 3;
				}
				if (BordersRiver()) {
					regularCityYield += 1;
				}
				if (this.Resource != Resource.NONE && player.KnowsAboutResource(Resource)) {
					regularCityYield += this.Resource.CommerceBonus;
				}

				int capitalCityYield = 0;
				if (cityAtTile.IsCapital()) {
					capitalCityYield = 4;
				}

				yield = Math.Max(regularCityYield, capitalCityYield);
			}

			return yield;
		}

		public Yield commerceYield(Player player) {
			int yield = baseCommerceYield(player);

			// TODO: handle the commerce bonus for costal cities+seafaring
			// TODO: handle the commerce bonus for commerial civs
			return Yield.CalculateForPlayer(this, yield, YieldType.Commerce, player);
		}

		public Yield commerceYield(City city) {
			int yield = baseCommerceYield(city.owner);

			return Yield.CalculateForCity(this, yield, YieldType.Commerce, city);
		}

		public bool BordersRiver() {
			return riverNorth || riverNortheast || riverEast || riverSoutheast || riverSouth || riverSouthwest || riverWest || riverNorthwest;
		}

		public bool CanBeMined() {
			return overlayTerrainType.miningBonus > 0 && !overlays.mine && !overlays.irrigation && cityAtTile == null;
		}

		public bool CanBeRoaded() {
			return IsLand() && !overlays.road;
		}

		// TODO: This method doesn't handle two important irrigation cases:
		//  - inland lakes/seas: we need to figure out what is fresh/salt water
		//  - Electricity tech, to allow irrigating w/o fresh water access
		public bool CanBeIrrigated(Player player) {
			// Irrigation can't be done if there is no irrigation bonus for the
			// tile or if there's already an improvement or city on the tile.
			if (overlayTerrainType.irrigationBonus == 0 ||
				overlays.mine ||
				overlays.irrigation ||
				cityAtTile != null) {
				return false;
			}

			// If a tile borders a river, it has fresh water access.
			if (BordersRiver()) {
				return true;
			}

			foreach (KeyValuePair<TileDirection, Tile> dirToTile in neighbors) {
				// If a neighboring tile is irrigated, this tile has fresh water access.
				if (dirToTile.Value.overlays.irrigation) {
					return true;
				}

				// Special case, if we are neighboring a city, check
				// if the city can act as part of an irrigation chain.
				if (dirToTile.Value.cityAtTile != null) {
					if (dirToTile.Value.BordersRiver()) {
						return true;
					}

					foreach (var (dir, tile) in dirToTile.Value.neighbors) {
						if (tile.overlays.irrigation) {
							return true;
						}
					}
				}
			}

			return false;
		}

		//Convenience method for printing the yield
		public string YieldString(Player player) {
			return $"{foodYield(player).yield}/{productionYield(player).yield}/{commerceYield(player).yield})";
		}

		public Player? OwningPlayer() {
			if (cityAtTile != null) {
				return cityAtTile.owner;
			}
			if (owningCity != null) {
				return owningCity.owner;
			}
			return null;
		}

		// Returns the X and Y coordinates of the neighbor in the specified direction.
		public static TileLocation NeighborCoordinate(TileLocation location, TileDirection direction) {
			switch (direction) {
				case TileDirection.NORTH:
					location.Y -= 2;
					break;
				case TileDirection.NORTHEAST:
					location.Y--;
					location.X++;
					break;
				case TileDirection.EAST:
					location.X += 2;
					break;
				case TileDirection.SOUTHEAST:
					location.Y++;
					location.X++;
					break;
				case TileDirection.SOUTH:
					location.Y += 2;
					break;
				case TileDirection.SOUTHWEST:
					location.Y++;
					location.X--;
					break;
				case TileDirection.WEST:
					location.X -= 2;
					break;
				case TileDirection.NORTHWEST:
					location.X--;
					location.Y--;
					break;
			}
			return location;
		}
		public MapUnit FindTopDefender(MapUnit opponent) {
			if (unitsOnTile.Count > 0) {
				IEnumerable<MapUnit> potentialDefenders = unitsOnTile.Where(u => u.CanDefendAgainst(opponent));
				if (potentialDefenders.Count() == 0) {
					return MapUnit.NONE;
				}

				MapUnit leadingCandidate = unitsOnTile[0];
				foreach (MapUnit u in unitsOnTile)
					if (u.HasPriorityAsDefender(leadingCandidate, opponent))
						leadingCandidate = u;
				return leadingCandidate;
			} else
				return MapUnit.NONE;
		}

		/// <summary>
		/// Disbands non-defending units on a tile.  This should only be called when all defending units have been destroyed,
		/// hence its name.  E.g. if only air/sea units remain after a land battle, this should be called.
		///
		/// Eventually, we should also have a method to make relevant units (workers, artillery, etc.) be captured.
		/// </summary>
		/// <param name="tile"></param>
		public void DisbandNonDefendingUnits() {
			//There may have been naval units, if so, disband them
			if (unitsOnTile.Count > 0) {
				//Copy to a separate array so we don't crash due to concurrent modification exceptions
				MapUnit[] unitsOnTile = new MapUnit[this.unitsOnTile.Count];
				this.unitsOnTile.CopyTo(unitsOnTile);
				foreach (MapUnit destroyedUnit in unitsOnTile) {
					destroyedUnit.disband();
				}
			}
		}

		/// <summary>
		/// After a WorkerJob has finished, Cclean up all the WorkerJobs and set the correct overlay
		/// </summary>
		/// <param name="tile">the current tile</param>
		/// <param name="currentWorkerJob">the worker job currently finished, must not be null</param>
		public void FinishWorkerJob(Terraform currentWorkerJob) {
			// Reset All Workers working on the finished Job
			foreach (MapUnit unit in unitsOnTile) {
				if (currentWorkerJob == unit.WorkerJob) {
					unit.resetWorkerJob();
				}
			}

			currentWorkerJob.OnComplete(this);
		}

		public void Animate(AnimatedEffect effect, bool wait) {
			if (EngineStorage.animationsEnabled) {
				new MsgStartEffectAnimation(this, effect, wait ? EngineStorage.uiEvent : null, AnimationEnding.Stop).send();
				if (wait) {
					EngineStorage.gameDataMutex.ReleaseMutex();
					EngineStorage.uiEvent.WaitOne();
					EngineStorage.gameDataMutex.WaitOne();
				}
			}
		}

		public void ClearTerrainOverlay() {
			overlayTerrainType = baseTerrainType;
			overlayTerrainTypeKey = baseTerrainTypeKey;
		}
	}

	public enum TileDirection {
		NORTH,
		NORTHEAST,
		EAST,
		SOUTHEAST,
		SOUTH,
		SOUTHWEST,
		WEST,
		NORTHWEST,
	}

	public static class TileDirectionExtensions {
		public static TileDirection reversed(this TileDirection dir) {
			switch (dir) {
				case TileDirection.NORTH: return TileDirection.SOUTH;
				case TileDirection.NORTHEAST: return TileDirection.SOUTHWEST;
				case TileDirection.EAST: return TileDirection.WEST;
				case TileDirection.SOUTHEAST: return TileDirection.NORTHWEST;
				case TileDirection.SOUTH: return TileDirection.NORTH;
				case TileDirection.SOUTHWEST: return TileDirection.NORTHEAST;
				case TileDirection.WEST: return TileDirection.EAST;
				case TileDirection.NORTHWEST: return TileDirection.SOUTHEAST;
				default: throw new ArgumentOutOfRangeException("Invalid TileDirection");
			}
		}

		public static (int, int) toCoordDiff(this TileDirection dir) {
			switch (dir) {
				case TileDirection.NORTH: return (0, -2);
				case TileDirection.NORTHEAST: return (1, -1);
				case TileDirection.EAST: return (2, 0);
				case TileDirection.SOUTHEAST: return (1, 1);
				case TileDirection.SOUTH: return (0, 2);
				case TileDirection.SOUTHWEST: return (-1, 1);
				case TileDirection.WEST: return (-2, 0);
				case TileDirection.NORTHWEST: return (-1, -1);
				default: throw new ArgumentOutOfRangeException("Invalid TileDirection");
			}
		}
	}

	public class TileOverlays {
		public bool road = false;
		// assume that railroad contains road too
		public bool railroad = false;
		public bool mine = false;
		public bool irrigation = false;
	}
}
