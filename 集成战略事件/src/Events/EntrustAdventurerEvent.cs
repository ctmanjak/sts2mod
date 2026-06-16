using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;

namespace IntegratedStrategyEvents.Events;

public sealed partial class EntrustAdventurerEvent : IntegratedStrategyEventModel
{
	private const int MeagerTipCost = 20;
	private const int HeavyRewardCost = 50;
	private const int MeagerTipCardOptionCount = 2;
	private const int HeavyRewardCardOptionCount = 5;

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		Player owner = OwnerOrThrow;
		return
		[
			GoldChoice(owner, MeagerTipCost, ChooseMeagerTip, "MEAGER_TIP", "MEAGER_TIP_LOCKED"),
			GoldChoice(owner, HeavyRewardCost, ChooseHeavyReward, "HEAVY_REWARD", "HEAVY_REWARD_LOCKED"),
			Choice(ChooseSincereRequest, "SINCERE_REQUEST")
		];
	}

	private async Task ChooseMeagerTip()
	{
		await SpendGold(MeagerTipCost);
		Finish("MEAGER_TIP");
		await OfferRegularCardReward(MeagerTipCardOptionCount);
	}

	private async Task ChooseHeavyReward()
	{
		await SpendGold(HeavyRewardCost);
		Finish("HEAVY_REWARD");
		await OfferRegularCardReward(HeavyRewardCardOptionCount);
	}

	private Task ChooseSincereRequest()
	{
		Finish("SINCERE_REQUEST");
		return Task.CompletedTask;
	}
}
