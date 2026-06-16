using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace IntegratedStrategyEvents.Relics;

public sealed class SwordHammerRelic : IntegratedStrategyEventRelic
{
	private const decimal EnergyIncrease = 1m;
	private const int AdditionalPlayCount = 1;

	public SwordHammerRelic()
		: base("sword_hammer.png")
	{
	}

	public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
	{
		modifiedCost = originalCost;
		if (!ShouldAffect(card))
		{
			return false;
		}

		modifiedCost = originalCost + EnergyIncrease;
		return true;
	}

	public override int ModifyCardPlayCount(CardModel card, Creature? target, int playCount)
	{
		if (!ShouldAffect(card))
		{
			return playCount;
		}

		Flash();
		return playCount + AdditionalPlayCount;
	}

	private bool ShouldAffect(CardModel card)
	{
		return IsOwnedCard(card)
			&& IsNonXEnergyCard(card)
			&& card.EnergyCost.Canonical == 0m;
	}
}
