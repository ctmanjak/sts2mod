using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Events;

namespace IntegratedStrategyEvents.Events;

public sealed partial class TurningPointEvent : IntegratedStrategyEventModel
{
	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		return
		[
			Choice(StandUp, "STAND_UP"),
			Choice(LookAgain, "LOOK_AGAIN"),
			Choice(Surrender, "SURRENDER")
		];
	}

	private async Task StandUp()
	{
		Finish("STAND_UP");
		await GrantRandomRareCard(CardType.Attack);
	}

	private async Task LookAgain()
	{
		Finish("LOOK_AGAIN");
		await GrantRandomRareCard(CardType.Skill);
	}

	private Task Surrender()
	{
		Finish("SURRENDER");
		return Task.CompletedTask;
	}
}
