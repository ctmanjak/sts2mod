using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace IntegratedStrategyEvents.Relics;

public sealed class BrokenSwordRelic : IntegratedStrategyEventRelic
{
	private const decimal EnergyReduction = 1m;
	private const decimal MinimumAffectedCost = 2m;

	public BrokenSwordRelic()
		: base("broken_sword.png")
	{
	}

	public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
	{
		modifiedCost = originalCost;
		if (!ShouldAffect(card))
		{
			return false;
		}

		modifiedCost = Math.Max(0m, originalCost - EnergyReduction);
		return true;
	}

	public override (PileType, CardPilePosition) ModifyCardPlayResultPileTypeAndPosition(
		CardModel card,
		bool isAutoPlay,
		ResourceInfo resources,
		PileType pileType,
		CardPilePosition position)
	{
		if (!ShouldAffect(card) || pileType == PileType.None)
		{
			return (pileType, position);
		}

		Flash();
		return (PileType.Exhaust, position);
	}

	private bool ShouldAffect(CardModel card)
	{
		return IsOwnedCard(card)
			&& IsNonXEnergyCard(card)
			&& card.EnergyCost.Canonical >= MinimumAffectedCost;
	}
}
