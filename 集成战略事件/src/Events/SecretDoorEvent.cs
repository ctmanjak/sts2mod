using IntegratedStrategyEvents.Relics;
using MegaCrit.Sts2.Core.Events;

namespace IntegratedStrategyEvents.Events;

public sealed partial class SecretDoorEvent : IntegratedStrategyEventModel
{
	private const int MaxHpGain = 10;

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		return
		[
			CreateTakeTabletOption(),
			Choice(StudyMechanism, "STUDY_MECHANISM")
		];
	}

	private EventOption CreateTakeTabletOption()
	{
		return RelicChoice<FlowerOfCandeRelic>(TakeTablet, "TAKE_TABLET");
	}

	private async Task TakeTablet()
	{
		await ObtainRelic<FlowerOfCandeRelic>();
		Finish("TAKE_TABLET");
	}

	private async Task StudyMechanism()
	{
		await GainMaxHp(MaxHpGain);
		Finish("STUDY_MECHANISM");
	}
}
