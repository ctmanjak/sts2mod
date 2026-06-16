using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;

namespace IntegratedStrategyEvents.Events;

public sealed partial class DustDevouringSpreadEvent : IntegratedStrategyEventModel
{
	private const int TransformCardCount = 1;
	private const int HealAmount = 6;

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		Player owner = OwnerOrThrow;
		return
		[
			CreateDeclineSuitOption(owner),
			Choice(WearSuit, "WEAR_SUIT")
		];
	}

	private EventOption CreateDeclineSuitOption(Player owner)
	{
		return HasTransformableDeckCards(owner, TransformCardCount)
			? Choice(DeclineSuit, "DECLINE_SUIT")
			: LockedChoice("DECLINE_SUIT_LOCKED");
	}

	private async Task DeclineSuit()
	{
		await TransformDeckCards(TransformCardCount);
		Finish("DECLINE_SUIT");
	}

	private async Task WearSuit()
	{
		await Heal(HealAmount);
		Finish("WEAR_SUIT");
	}
}
