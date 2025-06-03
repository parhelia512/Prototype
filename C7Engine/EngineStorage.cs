namespace C7Engine {
	using System;
	using System.Threading;
	using System.Collections.Concurrent;
	using C7GameData;

	/**
	 * This class stores references to data that the engine needs between calls from the player.
	 * Most obviously this includes a reference to the C7GameData, but it might eventually
	 * also include things like keeping track of which networked players are up to date.
	 *
	 * Note that we should NOT store pointers to pieces of the game data here; that will
	 * all be handled within C7GameData.  We just need a pointer to the main, top level
	 * so we don't forget the state of the game after we create it.
	 **/
	public static class EngineStorage {
		private static readonly object gameDataLock = new object();
		internal static GameData gameData { get; set; }

		public static ID uiControllerID;
		internal static bool animationsEnabled = true;

		public static bool isWaitingForUi = false;

		public static ConcurrentQueue<MessageToUI> messagesToUI = new ConcurrentQueue<MessageToUI>();

		private static Thread engineThread = null;

		internal static BlockingCollection<MessageToEngine> pendingMessages = new BlockingCollection<MessageToEngine>(new ConcurrentQueue<MessageToEngine>());

		private static void processActions() {
			foreach (MessageToEngine msg in pendingMessages.GetConsumingEnumerable()) {
				lock (gameDataLock) {
					msg.process();
				}

				if (msg is MsgShutdownEngine) {
					break;
				}
			}
		}

		internal static void createThread() {
			if (engineThread == null) {
				// TODO: What if engineThread is not null, i.e. if the thread has already been created? Should we join() it? Does it matter?
				engineThread = new Thread(processActions);
				engineThread.Start();
			}
		}

		public static void ReadGameData(Action<GameData> accessor) {
			lock (gameDataLock) {
				accessor(gameData);
			}
		}

		internal static void WaitForUiEvent() {
			lock (gameDataLock) {
				isWaitingForUi = true;
				while (isWaitingForUi) {
					Monitor.Wait(gameDataLock);
				}
			}
		}

		public static void FinishUiEvent() {
			lock (gameDataLock) {
				isWaitingForUi = false;
				Monitor.Pulse(gameDataLock);
			}
		}
	}
}
