using System;
using System.Collections.Generic;
using System.Linq;
using Serilog;

namespace C7GameData;

// A class holding all the state of the relationship between two civs.
// If a relationship between 2 civs doesn't exit it means that they haven't met yet.
// If a relationship exists but doesn't have any multi-turn deals on either side,
// it means that these 2 civs are at war, because Peace itself is a multi-turn deal.
// A civ can't (and shouldn't) have a relationship with themselves, barbarians or defeated players.
public class PlayerRelationship {
	private static ILogger log = Log.ForContext<PlayerRelationship>();

	// p1.playerRelationships[p2].warDeclarationCount is the number of times
	// p2 declared war on p1.
	// TODO: contribute towards reputation
	public int warDeclarationCount = 0;

	// p1.playerRelationships[p2].warDeclarationWithRoPActiveCount is the number of times
	// p2 declared war on p1 while having an active RoP.
	// TODO: contribute towards reputation
	public int warDeclarationWithRoPActiveCount = 0;

	// true if a war declaration happened with units inside the player's
	// borders.
	public bool wasSneakAttacked = false;

	// If at war, refuse contact with the relevant player until this turn
	// has been reached.
	public int refuseContactUntilTurn = -1;

	public List<MultiTurnDeal> multiTurnDeals = new List<MultiTurnDeal>();

	public bool declaredWarWithActiveRightOfPassage = false;

	public bool AtWar() {
		return multiTurnDeals.Count == 0;
	}

	public static bool AtWar(Player left, Player right) {
		if (left.id == right.id) return false;

		if (right.defeated || left.defeated) return false;

		if ((left.isBarbarians && !right.isBarbarians) || (right.isBarbarians && !left.isBarbarians)) return true;

		if (!left.playerRelationships.TryGetValue(right.id, out var pr)) return false;

		return pr.multiTurnDeals.Count == 0;
	}

	public static bool IsInAnyWar(Player player, List<Player> players) {
		if (players
			// any player that is not barbarians, is not us,
			.Any(other => !other.isBarbarians && other.id != player.id
				// that we have a relationship with,
				&& player.playerRelationships.TryGetValue(other.id, out var playerRelationship)
				// but we don't have any multi turn deals, which means we don't have peace
				&& playerRelationship != null && playerRelationship.multiTurnDeals.Count == 0)) {
			return true;
		}

		return false;
	}

	public static bool HaveActiveRightOfPassage(Player left, Player right) {
		if (left == null || right == null)
			throw new Exception("Player(s) should not be null");
		return left.playerRelationships.TryGetValue(right.id, out var pr) &&
		pr.multiTurnDeals.Any(d => d.dealSubType == DealSubType.RightOfPassage);
	}

	// Breaks peace and all other multiturn deals when war is declared 
	public static void DeclareWar(Player aggressor, Player defender, bool sneakAttack, int refuseContactUntilTurn) {
		// increment the times the aggressor has declared war on the defender
		defender.playerRelationships[aggressor.id].warDeclarationCount++;

		// increment the times the aggressor has declared war on the defender while there is an active RoP
		if (HaveActiveRightOfPassage(aggressor, defender)) {
			defender.playerRelationships[aggressor.id].warDeclarationWithRoPActiveCount++;
		}

		// update whether the defender was sneak attacked
		defender.playerRelationships[aggressor.id].wasSneakAttacked = sneakAttack;

		// TODO: do we need the same for the aggressor? Perhaps a few turns less what the defender's is?
		// The thinking is that if AI attacks a human, the human might try to talk to them
		// earlier than the AI is programmed to do.
		//
		// Set for how many turns the defender will refuse contact from the aggressor
		defender.playerRelationships[aggressor.id].refuseContactUntilTurn = refuseContactUntilTurn;

		// Finally clear all multi-turn deals, including Peace
		aggressor.playerRelationships[defender.id].multiTurnDeals = new List<MultiTurnDeal>();
		defender.playerRelationships[aggressor.id].multiTurnDeals = new List<MultiTurnDeal>();

		log.Information($"{aggressor} declared war on {defender}{(sneakAttack ? $" in a sneak attack" : "")}! " +
						$"Defender is refusing contact for at least up to turn {refuseContactUntilTurn}!");
	}

	public static void SignPeaceAfterWar(Player left, Player right, GameData gameData) {
		if (left.isBarbarians || left.defeated || right.isBarbarians || right.defeated || left.id == right.id)
			throw new Exception($"Can't sign peace between {left} and {right}");

		if (!AtWar(left, right))
			throw new Exception($"This is not the proper method to use if the two players are not at war");

		MultiTurnDeal mtd = new MultiTurnDeal(DealType.DiplomaticAgreement, DealSubType.Peace, DealDetails.Exchange,
			0, null, gameData.rules.DefaultDealDuration, gameData.turn, null);

		RegisterMultiTurnDeal(left, right, mtd);

		left.playerRelationships[right.id].refuseContactUntilTurn = -1;
		right.playerRelationships[left.id].refuseContactUntilTurn = -1;

		log.Information($"{left} signed a peace treaty with {right}");
	}

	public static void RegisterMultiTurnDeal(Player left, Player right, MultiTurnDeal mtd) {
		if (mtd == null || mtd.dealDetails == DealDetails.None)
			throw new Exception("Not a valid deal");

		if (mtd.dealDetails == DealDetails.Exchange) {
			RegisterTwoWayDeal(left, right, mtd);
			return;
		}
		RegisterOneWayDeal(left, right, mtd);
	}

	private static void RegisterOneWayDeal(Player left, Player right, MultiTurnDeal mtd) {
		if (mtd.dealDetails == DealDetails.None || mtd.dealDetails == DealDetails.Exchange)
			throw new Exception("This is not a valid one way deal. Perhaps you intended to use RegisterTwoWayDeal() instead.");

		// add the deal for this player
		left.playerRelationships[right.id].multiTurnDeals.Add(mtd);

		// add the deal for the other player
		right.playerRelationships[left.id].multiTurnDeals.Add(new MultiTurnDeal(mtd.dealType, mtd.dealSubType,
			mtd.dealDetails == DealDetails.Inbound ? DealDetails.Outbound : DealDetails.Inbound,
			mtd.goldPerTurn, mtd.resourcePerTurn, mtd.dealDuration, mtd.goldPerTurn, mtd.againstPlayer));
	}

	private static void RegisterTwoWayDeal(Player left, Player right, MultiTurnDeal mtd) {
		if (mtd.dealDetails != DealDetails.Exchange)
			throw new Exception("This is not a valid two way deal. Perhaps you intended to use RegisterOneWayDeal() instead.");

		// add the deal for this player
		left.playerRelationships[right.id].multiTurnDeals.Add(mtd);

		// add the deal for the other player
		right.playerRelationships[left.id].multiTurnDeals.Add(mtd);
		// right.playerRelationships[left.id].multiTurnDeals.Add(new MultiTurnDeal(mtd.dealType, mtd.dealSubType,
		// 	mtd.dealDetails, mtd.goldPerTurn, mtd.resourcePerTurn, mtd.dealDuration, mtd.turnStartDeal, mtd.againstPlayer));
	}

	/// <summary>
	/// This automatically ends all deals except peace when they go over the initial agreed upon duration
	/// </summary>
	/// <param name="players"></param>
	/// <param name="currentTurn"></param>
	public static void CheckForObsoleteDeals(List<Player> players, int currentTurn) {
		var activeNotBarbPlayers = players.Where(p => !p.isBarbarians && !p.defeated).ToList();
		var playerIds = activeNotBarbPlayers.Select(x => x.id).ToList();

		// check each player's relationship with the other players
		foreach (Player player in activeNotBarbPlayers) {
			foreach (var playerId in playerIds) {
				// we don't have a relationship with ourselves
				if (playerId == player.id) continue;
				// if the player has a relationship with the other civ, and are at peace
				if (player.playerRelationships.TryGetValue(playerId, out var pr) && pr.multiTurnDeals != null && pr.multiTurnDeals.Count > 0) {
					// we don't want to cancel peace
					List<MultiTurnDeal> deadDeals = pr.multiTurnDeals.Where(mtd => mtd != null && mtd.dealSubType != DealSubType.Peace && mtd.TurnsRemaining(currentTurn) <= 0).ToList();
					foreach (MultiTurnDeal deadDeal in deadDeals) {
						log.Information($"Cancelling multi turn deal: {player} -- {activeNotBarbPlayers.First(p => p.id == playerId)}");
						UnRegisterMultiTurnDeal(player, activeNotBarbPlayers.First(p => p.id == playerId), deadDeal);
					}
				}
			}
		}
	}

	// Removes a multi turn deal from both parties
	private static void UnRegisterMultiTurnDeal(Player left, Player right, MultiTurnDeal mtd) {
		left.playerRelationships[right.id].multiTurnDeals.Remove(mtd);
		right.playerRelationships[left.id].multiTurnDeals.Remove(mtd);
	}
}

public class MultiTurnDeal {
	private const int DEFAULT_DEAL_DURATION = 20;
	public DealType dealType { get; private set; }
	public DealSubType dealSubType { get; private set; }
	public DealDetails dealDetails { get; private set; }
	public int goldPerTurn { get; private set; }
	public string resourcePerTurn { get; private set; }
	public int dealDuration { get; private set; }
	public int turnStartDeal { get; private set; }

	public int turnEndDeal { get; private set; }

	// only applicable for Military Alliances and Trade Embargoes
	public ID againstPlayer { get; private set; }

	public MultiTurnDeal(DealType dealType, DealSubType dealSubType, DealDetails dealDetails, int goldPerTurn = 0,
		string resourcePerTurn = null, int dealDuration = DEFAULT_DEAL_DURATION, int turnStartDeal = 0, ID againstPlayer = null) {
		this.dealSubType = dealSubType;
		this.dealType = dealType;
		this.dealDetails = dealDetails;
		this.goldPerTurn = goldPerTurn;
		this.resourcePerTurn = resourcePerTurn;
		this.dealDuration = dealDuration;
		this.turnStartDeal = turnStartDeal;
		// basically peace from the start, don't need to know when it ends
		if (dealSubType == DealSubType.Peace && turnStartDeal == 0)
			this.turnEndDeal = 0;
		else
			this.turnEndDeal = turnStartDeal + dealDuration;
		this.againstPlayer = againstPlayer;
	}

	public int TurnsRemaining(int currentTurn) {
		return turnEndDeal - currentTurn <= 0 ? 0 : turnEndDeal - currentTurn;
	}

	// A multi turn peace deal that is from the start of the game without end.
	// Used for when a civ meets another civ to establish the initial peace.
	public static MultiTurnDeal DEFAULT_PEACE => new MultiTurnDeal(DealType.DiplomaticAgreement, DealSubType.Peace,
			DealDetails.Exchange, 0, null, 0, 0, null);

}

// https://github.com/maxpetul/C3X/blob/064c8307c5085205c0dc8f2ee5b61ad2c2606523/Civ3Conquests.h#L1399
public enum DealType {
	DiplomaticAgreement,
	Alliance,
	Embargo,
	Map,
	Communication,
	Resource,
	Luxury,
	Gold,
	Technology,
	City,
	Unit,
	None,
}

public enum DealSubType {
	Peace,
	MutualProtectionPact,
	RightOfPassage,
	MilitaryAlliance,
	TradeEmbargo,
	GoldPerTurn,
	ResourcePerTurn,
	LuxuryPerTurn,
	None,
}

public enum DealDetails {
	Inbound,
	Outbound,
	Exchange,
	None,
}
