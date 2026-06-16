using IntegratedStrategyEvents.Encounters;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;

namespace IntegratedStrategyEvents.Events;

public sealed partial class HundredMileEncampmentEvent : IntegratedStrategyEventModel
{
	private const int ApproachHpLoss = 8;

	public override bool IsShared => true;

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		Player owner = OwnerOrThrow;
		return
		[
			HpChoice(owner, ApproachHpLoss, ApproachCamp, "APPROACH", "APPROACH_LOCKED"),
			Choice(Leave, "LEAVE")
		];
	}

	private async Task ApproachCamp()
	{
		await LoseHp(ApproachHpLoss);
		ShowPage(
			"APPROACH",
			[
				FightChoice<HundredMileEncampmentCultistsEncounter>("APPROACH"),
				Choice(Leave, "FLEE", "APPROACH")
			]);
	}

	private Task Leave()
	{
		Finish("LEAVE");
		return Task.CompletedTask;
	}
}
