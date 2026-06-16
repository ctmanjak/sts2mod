using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Models;

namespace IntegratedStrategyEvents.Events;

internal static partial class IntegratedStrategyEventRewards
{
	public static RelicModel PullRandomRelic(Player owner)
	{
		return RelicFactory.PullNextRelicFromFront(owner).ToMutable();
	}

	public static RelicModel PullRandomRelic(Player owner, RelicRarity rarity)
	{
		return RelicFactory.PullNextRelicFromFront(owner, rarity).ToMutable();
	}
}
