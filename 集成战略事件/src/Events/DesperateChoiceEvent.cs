using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Events;

namespace IntegratedStrategyEvents.Events;

public sealed partial class DesperateChoiceEvent : IntegratedStrategyEventModel
{
	private const int LichHpLoss = 10;
	private const int CyclopsHealAmount = 10;
	private const int CardRewardOptionCount = 3;

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		Player owner = OwnerOrThrow;
		return
		[
			Choice(FollowBanshee, "FOLLOW_BANSHEE"),
			HpChoice(owner, LichHpLoss, FollowLich, "FOLLOW_LICH", "FOLLOW_LICH_LOCKED"),
			Choice(FollowCyclops, "FOLLOW_CYCLOPS"),
			Choice(FollowHeart, "FOLLOW_HEART")
		];
	}

	private Task FollowBanshee()
	{
		ShowPage("BANSHEE", [Choice(ClaimHomeGift, "HOME_GIFT", "BANSHEE")]);
		return Task.CompletedTask;
	}

	private async Task FollowLich()
	{
		await LoseHp(LichHpLoss);
		ShowPage("LICH", [Choice(ClaimCollectiveGift, "COLLECTIVE_GIFT", "LICH")]);
	}

	private async Task FollowCyclops()
	{
		await Heal(CyclopsHealAmount);
		ShowPage("CYCLOPS", [Choice(ClaimAlliesGift, "ALLIES_GIFT", "CYCLOPS")]);
	}

	private Task FollowHeart()
	{
		Finish("WANDER");
		return Task.CompletedTask;
	}

	private async Task ClaimHomeGift()
	{
		await ObtainRandomRelic(RelicRarity.Uncommon);
		Finish("WANDER");
	}

	private async Task ClaimCollectiveGift()
	{
		await RemoveDeckCards(1);
		Finish("WANDER");
	}

	private async Task ClaimAlliesGift()
	{
		Finish("WANDER");
		await OfferRegularCardReward(CardRewardOptionCount);
	}
}
