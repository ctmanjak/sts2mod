using IntegratedStrategyEvents.Encounters;
using MegaCrit.Sts2.Core.Events;

namespace IntegratedStrategyEvents.Events;

public sealed partial class NorthWindWitchEvent : IntegratedStrategyEventModel
{
	public override bool IsShared => true;

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		return
		[
			Choice(TouchThings, "TOUCH"),
			Choice(Leave, "LEAVE")
		];
	}

	private Task TouchThings()
	{
		ShowFightPage<NorthWindWitchScavengerApostlesEncounter>("TOUCH");
		return Task.CompletedTask;
	}

	private Task Leave()
	{
		Finish("LEAVE");
		return Task.CompletedTask;
	}
}
