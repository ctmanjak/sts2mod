using IntegratedStrategyEvents.Encounters;
using MegaCrit.Sts2.Core.Events;

namespace IntegratedStrategyEvents.Events;

public sealed partial class DevoutPersonEvent : IntegratedStrategyEventModel
{
	public override bool IsShared => true;

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		return
		[
			Choice(ApproachSister, "APPROACH"),
			Choice(ObserveCarefully, "OBSERVE")
		];
	}

	private Task ApproachSister()
	{
		ShowFightPage<DevoutPersonTurretOperatorsEncounter>("APPROACH");
		return Task.CompletedTask;
	}

	private Task ObserveCarefully()
	{
		Finish("OBSERVE");
		return Task.CompletedTask;
	}
}
