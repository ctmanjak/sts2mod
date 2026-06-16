using IntegratedStrategyEvents.Relics;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;

namespace IntegratedStrategyEvents.Events;

public sealed partial class TrappedPersonEvent : IntegratedStrategyEventModel
{
	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		return
		[
			RelicChoice<FamiliarStatueRelic>(Trade, "TRADE"),
			Choice(CoaxOut, "COAX_OUT")
		];
	}

	private async Task Trade()
	{
		Player owner = OwnerOrThrow;
		await SpendGold(owner.Gold);
		await ObtainRelic<FamiliarStatueRelic>();
		Finish("TRADE");
	}

	private Task CoaxOut()
	{
		Finish("COAX_OUT");
		return Task.CompletedTask;
	}
}
