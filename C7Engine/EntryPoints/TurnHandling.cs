using System.Diagnostics;
using C7Engine.AI;
using Serilog;

namespace C7Engine {
	using System;
	using C7GameData;

	public class TurnHandling {
		private static ILogger log = Log.ForContext<TurnHandling>();

		internal static void OnBeginTurn() {
			GameData gameData = EngineStorage.gameData;
			log.Information("\n*** Beginning turn " + gameData.turn + " ***");

			foreach (MapUnit mapUnit in gameData.mapUnits)
				mapUnit.OnBeginTurn();
		}

		// Implements the game loop. This method is called when the game is started and when the player signals that they're done moving.
		internal static void AdvanceTurn() {
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			GameData gameData = EngineStorage.gameData;
			while (true) { // Loop ends with a function return once we reach the UI controller during the movement phase
				bool firstTurn = GetTurnNumber() == 0;

				// Movement phase
				if (PlayPlayerTurns(gameData, firstTurn)) {
					stopwatch.Stop();
					log.Information("Turn time took " + stopwatch.ElapsedMilliseconds + " milliseconds");
					return;
				}

				//Clear all wait queue, so if a player ended the turn without handling all waited units, they are selected
				//at the same place in the order.  Confirmed this is what Civ3 does.
				UnitInteractions.ClearWaitQueue();

				SpawnBarbarians(gameData);

				gameData.turn++;
				foreach (Player player in gameData.players) {
					player.RecalculateCitizenMoods(gameData, goIntoDisorderIfUnhappy: true);
					player.DoCorruptionCalculations(gameData);

					// Note that we do growth after calculating citizen moods,
					// to ensure that the player has a chance to deal with the
					// unhappiness of a new citizen during their turn.
					player.HandleCityUpdates(gameData);
					HandleCityResults(gameData, player);

					player.DoPerTurnFinanceUpdates(gameData);
					player.DoPerTurnScienceUpdates(gameData);
				}

				// Now that the turn is ending, do all the bookkeeping for the
				// start of the next turn. We don't put the "hasPlayedThisTurn"
				// logic in OnBeginTurn because OnBeginTurn is called when a
				// save game is loaded, and that would erase the saved information
				// about which players have played.
				OnBeginTurn();
				foreach (Player player in gameData.players) {
					player.hasPlayedThisTurn = false;
				}
			}
		}

		/// <summary>
		/// Plays the turns for all the players in the game (including barbarians).
		/// </summary>
		/// <param name="gameData"></param>
		/// <param name="firstTurn"></param>
		/// <returns>true when it is time for the human to take control again</returns>
		private static bool PlayPlayerTurns(GameData gameData, bool firstTurn) {
			foreach (Player player in gameData.players) {
				if (player.hasPlayedThisTurn || player.defeated) {
					continue;
				}

				if (firstTurn && player.SitsOutFirstTurn()) {
					continue;
				}

				if (player.isBarbarians) {
					//Call the barbarian AI
					//TODO: The AIs should be stored somewhere on the game state as some of them will store state (plans,
					//strategy, etc.) For now, we only have a random AI, so that will be in a future commit
					new BarbarianAI().PlayTurn(player, gameData);
					player.hasPlayedThisTurn = true;
				} else if (!player.isHuman) {
					PlayerAI.PlayTurn(player, GameData.rng, gameData.techs);
					player.hasPlayedThisTurn = true;
				} else if (player.id != EngineStorage.uiControllerID) {
					player.hasPlayedThisTurn = true;
				}
				//Human player check.  Let the human see what's going on even if they are in observer mode.
				if (player.id == EngineStorage.uiControllerID) {
					new MsgStartTurn().send();
					return true;
				}
			}
			return false;
		}

		private static void SpawnBarbarians(GameData gameData) {
			//Generate new barbarian units.
			Player barbPlayer = gameData.players.Find(player => player.isBarbarians);
			foreach (Tile tile in gameData.map.barbarianCamps) {
				//7% chance of a new barbarian.  Probably should scale based on barbarian activity.
				int result = GameData.rng.Next(100);
				log.Verbose("Random barb result = " + result);
				if (result < 4) {
					MapUnit newUnit = new MapUnit(gameData.ids.CreateID("barbarian"));
					newUnit.location = tile;
					newUnit.owner = gameData.players[0];
					newUnit.unitType = gameData.barbarianInfo.basicBarbarian;
					newUnit.experienceLevelKey = gameData.defaultExperienceLevelKey;
					newUnit.experienceLevel = gameData.defaultExperienceLevel;
					newUnit.hitPointsRemaining = 3;
					newUnit.isFortified = true; //todo: hack for unit selection

					tile.unitsOnTile.Add(newUnit);
					gameData.mapUnits.Add(newUnit);
					barbPlayer.units.Add(newUnit);
					log.Debug("New barbarian added at " + tile);
				} else if (tile.NeighborsWater() && result < 6) {
					MapUnit newUnit = new MapUnit(gameData.ids.CreateID(gameData.barbarianInfo.barbarianSeaUnit.name));
					newUnit.location = tile;
					newUnit.owner = gameData.players[0]; //todo: make this reliably point to the barbs
					newUnit.unitType = gameData.barbarianInfo.barbarianSeaUnit;
					newUnit.experienceLevelKey = gameData.defaultExperienceLevelKey;
					newUnit.experienceLevel = gameData.defaultExperienceLevel;
					newUnit.hitPointsRemaining = 3;
					newUnit.isFortified = true; //todo: hack for unit selection

					tile.unitsOnTile.Add(newUnit);
					gameData.mapUnits.Add(newUnit);
					barbPlayer.units.Add(newUnit);
					log.Debug("New barbarian galley added at " + tile);
				}
			}
		}

		private static void HandleCityResults(GameData gameData, Player player) {
			log.Information($"\n*** City production for turn {gameData.turn}, player {player} ***");

			foreach (City city in player.cities) {
				IProducible producedItem = city.ComputeTurnProduction();
				if (producedItem != null) {
					log.Debug($"Produced {producedItem} in {city}");
					if (producedItem is UnitPrototype prototype) {
						city.AddUnit(prototype, gameData);
					} else if (producedItem is Building building) {
						city.AddBuilding(building);
					}

					city.SetItemBeingProduced(CityProductionAI.GetNextItemToBeProduced(city, producedItem));
				}
			}
		}

		///Eventually we'll have a game year or month or whatever, but for now this provides feedback on our progression
		public static int GetTurnNumber() {
			return EngineStorage.gameData.turn;
		}
	}
}
