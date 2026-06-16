using IntegratedStrategyEvents.Encounters;
using MegaCrit.Sts2.Core.Events;

namespace IntegratedStrategyEvents.Events;

public sealed partial class BoundBloodEvent : IntegratedStrategyEventModel
{
	public override bool IsShared => true;

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		return
		[
			Choice(HelpHer, "HELP"),
			Choice(Leave, "LEAVE")
		];
	}

	private Task HelpHer()
	{
		ShowFightPage<BoundBloodAxeRubyRaidersEncounter>("HELP");
		return Task.CompletedTask;
	}

	private Task Leave()
	{
		Finish("LEAVE");
		return Task.CompletedTask;
	}
}
