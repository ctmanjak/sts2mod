using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Potions;

namespace IntegratedStrategyEvents.Events;

public sealed partial class UnfreezingRiverEvent : IntegratedStrategyEventModel
{
	private const int DetourHealAmount = 10;
	private const int CrossRiverHpLoss = 8;

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		Player owner = OwnerOrThrow;
		List<EventOption> options =
		[
			CardPreviewChoice<Shame>(TakeDetour, "DETOUR"),
			CanLoseHp(owner, CrossRiverHpLoss)
				? PotionPreviewChoice<EntropicBrew>(CrossRiver, "CROSS_RIVER").ThatDoesDamage(CrossRiverHpLoss)
				: LockedChoice("CROSS_RIVER_LOCKED")
		];

		RelicModel? latestRelic = GetMostRecentlyObtainedRelic();
		options.Add(latestRelic != null
			? RelicCostChoice(latestRelic, SearchTools, "SEARCH_TOOLS")
			: LockedChoice("SEARCH_TOOLS_LOCKED"));
		return options;
	}

	private async Task TakeDetour()
	{
		await Heal(DetourHealAmount);
		Finish("DETOUR");
		await GrantCurse<Shame>();
	}

	private async Task CrossRiver()
	{
		await LoseHp(CrossRiverHpLoss);
		Finish("CROSS_RIVER");
		await OfferPotionReward<EntropicBrew>();
	}

	private async Task SearchTools()
	{
		RelicModel? latestRelic = GetMostRecentlyObtainedRelic();
		if (latestRelic != null)
		{
			await ReplaceRelicWithRandomRelic(latestRelic);
		}

		Finish("SEARCH_TOOLS");
	}
}
