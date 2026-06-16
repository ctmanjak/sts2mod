using IntegratedStrategyEvents.Relics;
using MegaCrit.Sts2.Core.Events;

namespace IntegratedStrategyEvents.Events;

public sealed partial class CompletionCeremonyEvent : IntegratedStrategyEventModel
{
	private const int CardRewardOptionCount = 3;

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		return
		[
			Choice(DemandBlueprints, "DEMAND_BLUEPRINTS"),
			CreateAttendSeminarOption()
		];
	}

	private EventOption CreateAttendSeminarOption()
	{
		return RelicChoice<OldGaulishPlaceNamesRelic>(AttendSeminar, "ATTEND_SEMINAR");
	}

	private async Task DemandBlueprints()
	{
		Finish("DEMAND_BLUEPRINTS");
		await OfferRegularCardReward(CardRewardOptionCount);
	}

	private async Task AttendSeminar()
	{
		await ObtainRelic<OldGaulishPlaceNamesRelic>();
		Finish("ATTEND_SEMINAR");
	}
}
