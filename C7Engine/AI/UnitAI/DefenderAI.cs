using C7GameData;
using C7GameData.AIData;
using Serilog;

namespace C7Engine.AI.UnitAI {
	class DefenderAI : C7GameData.UnitAI {
		private static ILogger log = Log.ForContext<DefenderAI>();
		private DefenderAIData defenderAI;

		public static DefenderAIData MakeAiData(MapUnit unit, Player player) {
			DefenderAIData ai = new DefenderAIData();
			ai.goal = DefenderAIData.DefenderGoal.DEFEND_CITY;
			ai.destination = unit.location;
			log.Information("Set defender AI for " + unit + " with destination of " + ai.destination);
			return ai;
		}

		public DefenderAI(DefenderAIData d) {
			defenderAI = d;
		}

		C7GameData.UnitAI.Result C7GameData.UnitAI.PlayTurnImpl(Player player, MapUnit unit) {
			if (defenderAI.destination == unit.location) {
				if (!unit.isFortified) {
					unit.fortify();
					log.Information("Fortifying " + unit + " at " + defenderAI.destination);
				}
				return C7GameData.UnitAI.Result.Done;
			} else {
				log.Debug("Moving defender towards " + defenderAI.destination);
				return this.TryToMoveAlongPath(unit, defenderAI.pathToDestination, /*allowCombat=*/true);
			}
		}

		public string SummarizePlan() {
			return "DefenderAI: " + defenderAI.ToString();
		}
	}
}
