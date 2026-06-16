using MegaCrit.Sts2.Core.Events;

namespace IntegratedStrategyEvents.Events;

public sealed partial class ForesightEvent : IntegratedStrategyEventModel
{
	private const int GoldReward = 75;

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		return
		[
			Choice(DispatchPersonnel, "DISPATCH"),
			Choice(FocusOnPresent, "PRESENT")
		];
	}

	private async Task DispatchPersonnel()
	{
		await RemoveDeckCards(1);
		Finish("DISPATCH");
	}

	private async Task FocusOnPresent()
	{
		await GainGold(GoldReward);
		Finish("PRESENT");
	}
}
