using IntegratedStrategyEvents.Encounters;
using MegaCrit.Sts2.Core.Events;

namespace IntegratedStrategyEvents.Events;

public sealed partial class BusinessEmpireEvent : IntegratedStrategyEventModel
{
	public override bool IsShared => true;

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		return
		[
			Choice(Challenge, "CHALLENGE"),
			Choice(Leave, "LEAVE")
		];
	}

	private Task Challenge()
	{
		ShowFightPage<BusinessEmpireGopnikEncounter>("CHALLENGE");
		return Task.CompletedTask;
	}

	private Task Leave()
	{
		Finish("LEAVE");
		return Task.CompletedTask;
	}
}
