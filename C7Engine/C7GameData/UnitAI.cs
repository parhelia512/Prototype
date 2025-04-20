using C7GameData;
using C7GameData.AIData;
using System;

namespace C7GameData {
	//Not-fully-fleshed out player unit AI.
	//Right now it's kind of random to just add some appearance
	//of stuff being done.  I.e. it kinda sucks.  But that's okay.
	//It has to start somewhere, right?
	public interface UnitAI {
		enum Result {
			Done,
			InProgress,
			Error,
		};

		public Result PlayTurn(Player player, MapUnit unit) {
			while (unit.movementPoints.canMove && !unit.isFortified) {
				Result result = PlayTurnImpl(player, unit);
				if (result == Result.Error || result == Result.Done) {
					return result;
				}
			}
			return Result.InProgress;
		}

		// To be implemented by each AI subclass.
		protected abstract Result PlayTurnImpl(Player player, MapUnit unit);

		// Provide a string representation of the current AI plan.
		string SummarizePlan();

		// Do any bookkeeping required when the unit is destroyed. For example,
		// if an escort is destroyed, update the unit being escorted to reflect
		// that it no longer has an escort.
		void UpdateOnDeath();
	}
}
