using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace IntegratedStrategyEvents.Events;

public sealed partial class SeabornScholarEvent : IntegratedStrategyEventModel
{
	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		return
		[
			Choice(TryPrototype, "TRY_PROTOTYPE"),
			Choice(Leave, "LEAVE")
		];
	}

	private async Task TryPrototype()
	{
		await ObtainRandomRelic();
		if (OwnerOrThrow.PlayerRng.Rewards.NextBool())
		{
			await GrantRandomPoolCard<CurseCardPool>(CardRarity.Curse);
		}

		Finish("TRY_PROTOTYPE");
	}

	private Task Leave()
	{
		Finish("LEAVE");
		return Task.CompletedTask;
	}
}
