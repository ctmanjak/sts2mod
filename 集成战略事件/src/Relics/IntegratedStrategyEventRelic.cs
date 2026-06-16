using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;

namespace IntegratedStrategyEvents.Relics;

public abstract class IntegratedStrategyEventRelic : RelicModel
{
	private readonly string _iconFileName;

	protected IntegratedStrategyEventRelic(string iconFileName)
	{
		_iconFileName = iconFileName;
	}

	public sealed override RelicRarity Rarity => RelicRarity.Event;

	public override string PackedIconPath => $"res://{ModInfo.ModId}/images/relics/{_iconFileName}";

	protected override string PackedIconOutlinePath => PackedIconPath;

	protected override string BigIconPath => PackedIconPath;

	protected bool IsOwnedCard(CardModel card)
	{
		return Owner != null && card.Owner == Owner;
	}

	protected static bool IsNonXEnergyCard(CardModel card)
	{
		return !card.EnergyCost.CostsX;
	}
}
