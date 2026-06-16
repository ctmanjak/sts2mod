using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;

namespace IntegratedStrategyEvents.Events;

public sealed partial class FortuneFlowsEvent : IntegratedStrategyEventModel
{
	private const int SmallOfferingCost = 150;
	private const int SmallOfferingRelicCount = 2;
	private const int AllOfferingRelicCount = 3;

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		Player owner = OwnerOrThrow;
		return
		[
			GoldChoice(owner, SmallOfferingCost, OfferSomeWealth, "OFFER_SOME", "OFFER_SOME_LOCKED"),
			Choice(OfferAllWealth, "OFFER_ALL"),
			Choice(Leave, "LEAVE")
		];
	}

	private async Task OfferSomeWealth()
	{
		await SpendGold(SmallOfferingCost);
		await ObtainRandomRelics(SmallOfferingRelicCount);
		Finish("OFFER");
	}

	private async Task OfferAllWealth()
	{
		await SpendGold(OwnerOrThrow.Gold);
		await ObtainRandomRelics(AllOfferingRelicCount);
		Finish("OFFER");
	}

	private Task Leave()
	{
		Finish("LEAVE");
		return Task.CompletedTask;
	}
}
