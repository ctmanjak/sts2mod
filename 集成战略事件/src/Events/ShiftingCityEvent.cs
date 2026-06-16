using IntegratedStrategyEvents.TreeHoles;
using MegaCrit.Sts2.Core.Events;

namespace IntegratedStrategyEvents.Events;

public sealed partial class ShiftingCityEvent : IntegratedStrategyEventModel
{
	private const int UnderstandCardCount = 2;
	private const int PleaseCardCount = 1;
	private const int PleaseGoldReward = 100;
	private const string StrangeFragmentActName = "诡谲断章";
	private const string StrangeFragmentStageLabel = "阶段∅";

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		return
		[
			Choice(UnderstandCity, "UNDERSTAND_CITY"),
			Choice(PleaseCity, "PLEASE_CITY"),
			Choice(AskEarthspirit, "ASK_EARTHSPIRIT")
		];
	}

	private async Task UnderstandCity()
	{
		await GrantRandomCards(UnderstandCardCount);
		ShowPage("UNDERSTAND_CITY", [Choice(EnterStrangeFragment, "ENTER_FRAGMENT", "UNDERSTAND_CITY")]);
	}

	private async Task PleaseCity()
	{
		await GrantRandomCards(PleaseCardCount);
		await GainGold(PleaseGoldReward);
		Finish("PLEASE_CITY");
	}

	private Task AskEarthspirit()
	{
		Finish("ASK_EARTHSPIRIT");
		return Task.CompletedTask;
	}

	private Task EnterStrangeFragment()
	{
		return IntegratedStrategyTreeHoleController.EnterFromEvent(
			OwnerOrThrow,
			StrangeFragmentActName,
			StrangeFragmentStageLabel);
	}
}
