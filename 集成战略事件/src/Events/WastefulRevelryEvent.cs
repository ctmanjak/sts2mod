using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;

namespace IntegratedStrategyEvents.Events;

public sealed partial class WastefulRevelryEvent : IntegratedStrategyEventModel
{
	private const int OneThirdHpLoss = 6;
	private const int HalfHpLoss = 9;
	private const int WholeHpLoss = 18;
	private const int OneThirdGoldReward = 80;
	private const int RareCardRewardOptionCount = 3;

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		Player owner = OwnerOrThrow;
		return
		[
			HpChoice(owner, OneThirdHpLoss, DrinkOneThird, "ONE_THIRD", "ONE_THIRD_LOCKED"),
			HpChoice(owner, HalfHpLoss, DrinkHalf, "HALF", "HALF_LOCKED"),
			HpChoice(owner, WholeHpLoss, DrinkWhole, "WHOLE", "WHOLE_LOCKED"),
			Choice(Leave, "LEAVE")
		];
	}

	private async Task DrinkOneThird()
	{
		await LoseHp(OneThirdHpLoss);
		await GainGold(OneThirdGoldReward);
		Finish("ONE_THIRD");
	}

	private async Task DrinkHalf()
	{
		await LoseHp(HalfHpLoss);
		Finish("HALF");
		await OfferRareCardReward(RareCardRewardOptionCount);
	}

	private async Task DrinkWhole()
	{
		await LoseHp(WholeHpLoss);
		await ObtainRandomRelic();
		Finish("WHOLE");
	}

	private Task Leave()
	{
		Finish("LEAVE");
		return Task.CompletedTask;
	}
}
