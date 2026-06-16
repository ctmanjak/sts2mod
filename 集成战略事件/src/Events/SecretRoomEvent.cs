using IntegratedStrategyEvents.Relics;
using MegaCrit.Sts2.Core.Events;

namespace IntegratedStrategyEvents.Events;

public sealed partial class SecretRoomEvent : IntegratedStrategyEventModel
{
	private const int HealAmount = 15;

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		return
		[
			CreateTakeSculptureOption(),
			Choice(Rest, "REST")
		];
	}

	private EventOption CreateTakeSculptureOption()
	{
		return RelicChoice<NumbnessAndVulgarityRelic>(TakeSculpture, "TAKE_SCULPTURE");
	}

	private async Task TakeSculpture()
	{
		await ObtainRelic<NumbnessAndVulgarityRelic>();
		Finish("TAKE_SCULPTURE");
	}

	private async Task Rest()
	{
		await Heal(HealAmount);
		Finish("REST");
	}
}
