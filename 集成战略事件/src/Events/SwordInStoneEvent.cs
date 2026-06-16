using IntegratedStrategyEvents.Relics;
using MegaCrit.Sts2.Core.Events;

namespace IntegratedStrategyEvents.Events;

public sealed partial class SwordInStoneEvent : IntegratedStrategyEventModel
{
	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		return
		[
			CreateBrokenSwordOption(),
			CreateSwordHammerOption(),
			Choice(QuestionNecessity, "QUESTION_NECESSITY")
		];
	}

	private EventOption CreateBrokenSwordOption()
	{
		return RelicChoice<BrokenSwordRelic>(BreakSword, "BREAK_SWORD");
	}

	private EventOption CreateSwordHammerOption()
	{
		return RelicChoice<SwordHammerRelic>(LiftStone, "LIFT_STONE");
	}

	private async Task BreakSword()
	{
		await ObtainRelic<BrokenSwordRelic>();
		Finish("BREAK_SWORD");
	}

	private async Task LiftStone()
	{
		await ObtainRelic<SwordHammerRelic>();
		Finish("LIFT_STONE");
	}

	private Task QuestionNecessity()
	{
		Finish("QUESTION_NECESSITY");
		return Task.CompletedTask;
	}
}
