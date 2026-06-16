using IntegratedStrategyEvents.Encounters;
using MegaCrit.Sts2.Core.Events;

namespace IntegratedStrategyEvents.Events;

public sealed partial class TreasureChestDanceEvent : IntegratedStrategyEventModel
{
	public override bool IsShared => true;

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		return
		[
			Choice(Dance, "DANCE"),
			Choice(Leave, "LEAVE")
		];
	}

	private Task Dance()
	{
		ShowFightPage<TreasureChestDanceSarkazCursebearersEncounter>("DANCE");
		return Task.CompletedTask;
	}

	private Task Leave()
	{
		Finish("LEAVE");
		return Task.CompletedTask;
	}
}
