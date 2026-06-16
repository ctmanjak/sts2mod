using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Models.Relics;

namespace IntegratedStrategyEvents.Events;

public sealed partial class LiuerEvent : IntegratedStrategyEventModel
{
	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		return
		[
			RelicChoice<Driftwood>(Accept, "ACCEPT"),
			Choice(Decline, "DECLINE")
		];
	}

	private async Task Accept()
	{
		Player owner = OwnerOrThrow;
		await SpendGold(owner.Gold / 2);
		await ObtainRelic<Driftwood>();
		Finish("ACCEPT");
	}

	private Task Decline()
	{
		Finish("DECLINE");
		return Task.CompletedTask;
	}
}
