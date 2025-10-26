using System.Collections.Generic;
using System.Linq;
using C7Engine.Pathing;
using C7GameData;
using EngineTests.Utils;
using Xunit;

namespace EngineTests.AI.Pathing {
	public sealed class WalkerOnLandTest : MapBase {
		[Fact]
		private void TestHumanPlayerLandUnitIgnoresKnownWater() {
			Tile start = hill;

			// Add 3 neighbors, one of which is water.
			start.neighbors[TileDirection.NORTH] = coast;
			start.neighbors[TileDirection.SOUTH] = mountain;
			start.neighbors[TileDirection.WEST] = plains;

			float movementPoints = 2.0f;
			MapUnit landUnit =  MakeLandUnit((int)movementPoints);

			// make player human, so that they can't see unknown tiles
			landUnit.owner.isHuman = true;

			// Add tiles to Player's known tiles
			landUnit.owner.tileKnowledge.knownTiles.Add(start);
			landUnit.owner.tileKnowledge.knownTiles.Add(coast);
			landUnit.owner.tileKnowledge.knownTiles.Add(mountain);
			landUnit.owner.tileKnowledge.knownTiles.Add(plains);

			UnitWalker unitWalker = new(landUnit);

			// The water tile should be ignored, and the costs should be correct.
			IEnumerable<Edge<Tile>> edges = unitWalker.getEdges(start);
			Assert.Equal(2, edges.Count());

			Assert.Contains(edges, item => item.current == mountain && item.distanceToCurrent == 1);
			Assert.Contains(edges, item => item.current == plains && item.distanceToCurrent == 1 / movementPoints);
		}

		[Fact]
		private void TestHumanPlayerLandUnitIncludesUnknownWater() {
			Tile start = hill;

			// Add 3 neighbors, one of which is water.
			start.neighbors[TileDirection.NORTH] = coast;
			start.neighbors[TileDirection.SOUTH] = mountain;
			start.neighbors[TileDirection.WEST] = plains;

			float movementPoints = 2.0f;
			MapUnit landUnit =  MakeLandUnit((int)movementPoints);
			landUnit.owner.isHuman = true;

			// Add tiles to Player's known tiles
			landUnit.owner.tileKnowledge.knownTiles.Add(start);
			landUnit.owner.tileKnowledge.knownTiles.Add(mountain);
			landUnit.owner.tileKnowledge.knownTiles.Add(plains);

			UnitWalker unitWalker = new(landUnit);

			// The water tile should not be ignored, and the costs should be correct.
			IEnumerable<Edge<Tile>> edges = unitWalker.getEdges(start);
			Assert.Equal(3, edges.Count());

			Assert.Contains(edges, item => item.current == mountain && item.distanceToCurrent == 1);
			Assert.Contains(edges, item => item.current == plains && item.distanceToCurrent == 1 / movementPoints);
			// 1 cost because the human player does not know the nature of the unexplored tile
			Assert.Contains(edges, item => item.current == coast && item.distanceToCurrent == 1 / movementPoints);
		}

		[Fact]
		private void TestAiPlayerLandUnitIgnoresUnknownWater() {
			Tile start = hill;

			// Add 3 neighbors, one of which is water.
			start.neighbors[TileDirection.NORTH] = coast;
			start.neighbors[TileDirection.SOUTH] = mountain;
			start.neighbors[TileDirection.WEST] = plains;

			float movementPoints = 2.0f;
			MapUnit landUnit =  MakeLandUnit((int)movementPoints);

			// make player human, so that they can't see unknown tiles
			landUnit.owner.isHuman = false;

			UnitWalker unitWalker = new(landUnit);

			// The water tile should be ignored, even if it's unexplored because player is AI, and the costs should be correct.
			IEnumerable<Edge<Tile>> edges = unitWalker.getEdges(start);
			Assert.Equal(2, edges.Count());

			Assert.Contains(edges, item => item.current == mountain && item.distanceToCurrent == 1);
			Assert.Contains(edges, item => item.current == plains && item.distanceToCurrent == 1 / movementPoints);
		}

		[Fact]
		private void TestRoadOnDestinationNotOnStart() {
			Tile start = hill;

			// Set up a neighbor with a road.
			Tile end = plains;
			end.overlays.Add(road);
			start.neighbors[TileDirection.NORTH] = end;

			float movementPoints = 2.0f;
			MapUnit landUnit =  MakeLandUnit((int)movementPoints);

			UnitWalker unitWalker = new(landUnit);

			// The road shouldn't matter, since we don't have a road.
			IEnumerable<Edge<Tile>> edges = unitWalker.getEdges(start);
			Assert.Single(edges);
			Assert.Contains(edges, item => item.current == plains && item.distanceToCurrent == 1 / 2.0f);
		}

		[Fact]
		private void TestRoadOnStartNotOnDestination() {
			Tile start = hill;
			start.overlays.Add(road);

			// Set up a neighbor without a road.
			Tile end = plains;
			start.neighbors[TileDirection.NORTH] = end;

			float movementPoints = 2.0f;
			MapUnit landUnit =  MakeLandUnit((int)movementPoints);

			UnitWalker unitWalker = new(landUnit);

			// The road shouldn't matter, since the destination doesn't have a road.
			IEnumerable<Edge<Tile>> edges = unitWalker.getEdges(start);
			Assert.Single(edges);
			Assert.Contains(edges, item => item.current == plains && item.distanceToCurrent == 1 / 2.0f);
		}

		[Fact]
		private void TestRoadOnStartAndDestination() {
			Tile start = hill;
			start.overlays.Add(road);

			// Set up a neighbor with a road.
			Tile end = plains;
			end.overlays.Add(road);
			start.neighbors[TileDirection.NORTH] = end;

			float movementPoints = 2.0f;
			MapUnit landUnit =  MakeLandUnit((int)movementPoints);

			UnitWalker unitWalker = new(landUnit);

			// The cost should be adjusted because we both have a road.
			IEnumerable<Edge<Tile>> edges = unitWalker.getEdges(start);
			Assert.Single(edges);
			Assert.Contains(edges, item => item.current == plains && item.distanceToCurrent == 1.0f / 3.0f / 2.0f);
		}
	}

	public sealed class WalkerOnWaterTest : MapBase {
		[Fact]
		private void TestHumanWaterUnitIgnoresKnownLand() {
			Tile start = coast;

			// Add 2 neighbors, one of which is land.
			start.neighbors[TileDirection.NORTH] = hill;
			start.neighbors[TileDirection.SOUTH] = sea;

			float movementPoints = 2.0f;
			MapUnit nonLandUnit =  MakeWaterUnit((int)movementPoints);

			// make player human, so that they can't see unknown tiles
			nonLandUnit.owner.isHuman = true;

			// Add tiles to Player's known tiles
			nonLandUnit.owner.tileKnowledge.knownTiles.Add(start);
			nonLandUnit.owner.tileKnowledge.knownTiles.Add(hill);
			nonLandUnit.owner.tileKnowledge.knownTiles.Add(sea);

			UnitWalker unitWalker = new(nonLandUnit);

			// The land tile should be ignored, and the costs should be correct.
			IEnumerable<Edge<Tile>> edges = unitWalker.getEdges(start);
			Assert.Single(edges);

			Assert.Contains(edges, item => item.current == sea && item.distanceToCurrent == 1 / movementPoints);
		}

		[Fact]
		private void TestHumanWaterUnitDoesNotIgnoreUnknownLand() {
			Tile start = coast;

			// Add 2 neighbors, one of which is land.
			start.neighbors[TileDirection.NORTH] = hill;
			start.neighbors[TileDirection.SOUTH] = sea;

			float movementPoints = 2.0f;
			MapUnit nonLandUnit =  MakeWaterUnit((int)movementPoints);

			// make player human, so that they can't see unknown tiles
			nonLandUnit.owner.isHuman = true;

			// Add tiles to Player's known tiles
			nonLandUnit.owner.tileKnowledge.knownTiles.Add(start);
			nonLandUnit.owner.tileKnowledge.knownTiles.Add(sea);

			UnitWalker unitWalker = new(nonLandUnit);

			// The land tile should be ignored, and the costs should be correct.
			IEnumerable<Edge<Tile>> edges = unitWalker.getEdges(start);
			Assert.Equal(2, edges.Count());

			Assert.Contains(edges, item => item.current == sea && item.distanceToCurrent == 1 / movementPoints);
			Assert.Contains(edges, item => item.current == hill && item.distanceToCurrent == 1 / movementPoints);
		}

		[Fact]
		private void TestLandIncludedIfItHasCityWithSameOwner() {
			Tile start = coast;

			float movementPoints = 2.0f;
			MapUnit nonLandUnit =  MakeWaterUnit((int)movementPoints);

			// Set up a neighbor on land with a city of the same owner.
			Tile end = hill;
			end.cityAtTile = new City(Tile.NONE, nonLandUnit.owner, "", ID.None(""));
			start.neighbors[TileDirection.NORTH] = end;

			// Add tiles to Player's known tiles
			nonLandUnit.owner.tileKnowledge.knownTiles.Add(start);
			nonLandUnit.owner.tileKnowledge.knownTiles.Add(end);

			UnitWalker walker = new(nonLandUnit);

			// The city tile should be included, to allow for canals, and so
			// that ships can go back into harbors.
			//
			// The cost should be 1, despite the city being on a hill. Land
			// movement costs don't make sense to apply to ships.
			IEnumerable<Edge<Tile>> edges = walker.getEdges(start);
			Assert.Single(edges);
			Assert.Contains(edges, item => item.current == hill && item.distanceToCurrent == 1 / movementPoints);
		}

		[Fact]
		private void TestCityWithDifferentOwnerNotIncluded() {
			Tile start = coast;

			float movementPoints = 2.0f;
			MapUnit nonLandUnit =  MakeWaterUnit((int)movementPoints);

			Player otherPlayer = new Player();

			// Set up a neighbor on land with a city of the same owner.
			Tile end = hill;
			end.cityAtTile = new City(Tile.NONE, otherPlayer, "", ID.None(""));
			start.neighbors[TileDirection.NORTH] = end;

			// Add tiles to Player's known tiles
			nonLandUnit.owner.tileKnowledge.knownTiles.Add(start);
			nonLandUnit.owner.tileKnowledge.knownTiles.Add(end);

			UnitWalker walker = new(nonLandUnit);

			// The city tile should be included, to allow for canals, and so
			// that ships can go back into harbors.
			//
			// The cost should be 1, despite the city being on a hill. Land
			// movement costs don't make sense to apply to ships.
			IEnumerable<Edge<Tile>> edges = walker.getEdges(start);
			Assert.Empty(edges);
		}
	}
}
