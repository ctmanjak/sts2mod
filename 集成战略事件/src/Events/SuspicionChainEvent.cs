using IntegratedStrategyEvents.Encounters;
using MegaCrit.Sts2.Core.Events;

namespace IntegratedStrategyEvents.Events;

public sealed partial class SuspicionChainEvent : IntegratedStrategyEventModel
{
	public override bool IsShared => true;

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		return
		[
			Choice(MaintainOrder, "MAINTAIN_ORDER"),
			Choice(AvoidConflict, "AVOID_CONFLICT")
		];
	}

	private Task MaintainOrder()
	{
		ShowFightPage<SuspicionChainChomperScrollEncounter>("MAINTAIN_ORDER");
		return Task.CompletedTask;
	}

	private Task AvoidConflict()
	{
		Finish("AVOID_CONFLICT");
		return Task.CompletedTask;
	}
}
