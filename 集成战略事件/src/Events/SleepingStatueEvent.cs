using IntegratedStrategyEvents.Encounters;
using MegaCrit.Sts2.Core.Events;

namespace IntegratedStrategyEvents.Events;

public sealed partial class SleepingStatueEvent : IntegratedStrategyEventModel
{
	public override bool IsShared => true;

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		return
		[
			Choice(AwakenStatues, "AWAKEN"),
			Choice(LeaveQuietly, "LEAVE")
		];
	}

	private Task AwakenStatues()
	{
		ShowFightPage<SleepingStatueBygoneEffigyEncounter>("AWAKEN");
		return Task.CompletedTask;
	}

	private Task LeaveQuietly()
	{
		Finish("LEAVE");
		return Task.CompletedTask;
	}
}
