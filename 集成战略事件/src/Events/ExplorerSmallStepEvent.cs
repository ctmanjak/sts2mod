using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;

namespace IntegratedStrategyEvents.Events;

public sealed partial class ExplorerSmallStepEvent : IntegratedStrategyEventModel
{
	private const string ResponsePage = "RESPONSE_ARRIVED";
	private const string SurvivalPage = "SURVIVE_THE_WAR";

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		return
		[
			Choice(WaitForResponse, "WAIT_RESPONSE")
		];
	}

	private Task WaitForResponse()
	{
		ShowPage(
			ResponsePage,
			[
				Choice(Advance, "ADVANCE", ResponsePage)
			]);
		return Task.CompletedTask;
	}

	private async Task Advance()
	{
		Player owner = OwnerOrThrow;
		await Heal(owner.Creature.MaxHp);
		Finish(SurvivalPage);
	}
}
