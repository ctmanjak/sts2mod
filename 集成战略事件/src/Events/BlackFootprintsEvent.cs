using IntegratedStrategyEvents.Encounters;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;

namespace IntegratedStrategyEvents.Events;

public sealed partial class BlackFootprintsEvent : IntegratedStrategyEventModel
{
	private const int EscapeHpLoss = 8;

	public override bool IsShared => true;

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		Player owner = OwnerOrThrow;
		return
		[
			Choice(StandOff, "STAND_OFF"),
			HpChoice(owner, EscapeHpLoss, Escape, "ESCAPE", "ESCAPE_LOCKED")
		];
	}

	private Task StandOff()
	{
		ShowFightPage<BlackFootprintsKinFollowersEncounter>("STAND_OFF");
		return Task.CompletedTask;
	}

	private async Task Escape()
	{
		await LoseHp(EscapeHpLoss);
		Finish("ESCAPE");
	}
}
