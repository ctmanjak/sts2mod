using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;

namespace IntegratedStrategyEvents.Events;

public sealed partial class RoyalDisputeEvent : IntegratedStrategyEventModel
{
	private const int PraiseGoldCost = 40;
	private const int OffColorCardRewardOptionCount = 3;

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		Player owner = OwnerOrThrow;
		return
		[
			Choice(DenounceCorsica, "DENOUNCE"),
			GoldChoice(owner, PraiseGoldCost, PraiseCorsica, "PRAISE", "PRAISE_LOCKED")
		];
	}

	private async Task DenounceCorsica()
	{
		await GrantRandomOffColorCard();
		Finish("DENOUNCE");
	}

	private async Task PraiseCorsica()
	{
		await SpendGold(PraiseGoldCost);
		Finish("PRAISE");
		await OfferOffColorCardReward(OffColorCardRewardOptionCount);
	}
}
