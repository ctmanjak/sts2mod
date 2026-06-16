using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;

namespace IntegratedStrategyEvents.Events;

public sealed partial class FatefulMeetingEvent : IntegratedStrategyEventModel
{
	private const int ExamineHpLoss = 8;
	private const int AidHpLoss = 16;
	private const int GoldReward = 120;
	private const int MaxHpGain = 8;

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		Player owner = OwnerOrThrow;
		return
		[
			HpChoice(owner, ExamineHpLoss, Examine, "EXAMINE", "EXAMINE_LOCKED"),
			HpChoice(owner, AidHpLoss, Aid, "AID", "AID_LOCKED"),
			Choice(Leave, "LEAVE")
		];
	}

	private async Task Examine()
	{
		await LoseHp(ExamineHpLoss);
		await GainGold(GoldReward);
		Finish("EXAMINE");
	}

	private async Task Aid()
	{
		await LoseHpAndGainMaxHp(AidHpLoss, MaxHpGain);
		Finish("AID");
	}

	private Task Leave()
	{
		Finish("LEAVE");
		return Task.CompletedTask;
	}
}
