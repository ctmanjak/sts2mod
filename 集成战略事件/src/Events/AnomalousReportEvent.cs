using IntegratedStrategyEvents.Relics;
using MegaCrit.Sts2.Core.Events;

namespace IntegratedStrategyEvents.Events;

public sealed partial class AnomalousReportEvent : IntegratedStrategyEventModel
{
	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		return
		[
			RelicChoice<TimeAndLightRelic>(PullTrigger, "PULL_TRIGGER"),
			Choice(DestroyFirearm, "DESTROY_FIREARM")
		];
	}

	private async Task PullTrigger()
	{
		await ObtainRelic<TimeAndLightRelic>();
		Finish("PULL_TRIGGER");
	}

	private Task DestroyFirearm()
	{
		Finish("DESTROY_FIREARM");
		return Task.CompletedTask;
	}
}
