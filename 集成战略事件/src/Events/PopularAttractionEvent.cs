using IntegratedStrategyEvents.Encounters;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Models;

namespace IntegratedStrategyEvents.Events;

public sealed partial class PopularAttractionEvent : IntegratedStrategyEventModel
{
	public override bool IsShared => true;

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		RelicModel? latestRelic = GetMostRecentlyObtainedRelic();
		return
		[
			latestRelic != null
				? RelicCostChoice(latestRelic, BuyTicket, "BUY_TICKET")
				: LockedChoice("BUY_TICKET_LOCKED"),
			Choice(SneakIn, "SNEAK_IN"),
			Choice(LeaveConfused, "LEAVE")
		];
	}

	private async Task BuyTicket()
	{
		RelicModel? latestRelic = GetMostRecentlyObtainedRelic();
		if (latestRelic != null)
		{
			await ReplaceRelicWithRandomRelic(latestRelic);
		}

		ShowPlatform();
	}

	private Task SneakIn()
	{
		ShowPlatform();
		return Task.CompletedTask;
	}

	private Task LeaveConfused()
	{
		Finish("CONFUSED_LEAVE");
		return Task.CompletedTask;
	}

	private void ShowPlatform()
	{
		ShowPage(
			"PLATFORM",
			[
				Choice(Complain, "COMPLAIN", "PLATFORM"),
				Choice(WalkAway, "WALK_AWAY", "PLATFORM")
			]);
	}

	private Task Complain()
	{
		ShowFightPage<PopularAttractionGremlinMercsEncounter>("COMPLAINT");
		return Task.CompletedTask;
	}

	private Task WalkAway()
	{
		Finish("WALK_AWAY");
		return Task.CompletedTask;
	}
}
