using IntegratedStrategyEvents.Relics;
using MegaCrit.Sts2.Core.Events;

namespace IntegratedStrategyEvents.Events;

public sealed partial class VoidPortentEvent : IntegratedStrategyEventModel
{
	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		return
		[
			RelicChoice<ProphecyProjectionRelic>(AcceptIt, "ACCEPT_IT"),
			Choice(ThinkAgain, "THINK_AGAIN")
		];
	}

	private async Task AcceptIt()
	{
		await ObtainRelic<ProphecyProjectionRelic>();
		Finish("ACCEPT_IT");
	}

	private async Task ThinkAgain()
	{
		await ObtainRandomRelic();
		Finish("THINK_AGAIN");
	}
}
