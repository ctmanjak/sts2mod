using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;

namespace IntegratedStrategyEvents.Events;

public sealed partial class AllComersWelcomeEvent : IntegratedStrategyEventModel
{
	private const int SmallPouchGoldCost = 80;
	private const int LargePouchGoldCost = 200;
	private const int LargePouchRelicCount = 3;

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		Player owner = OwnerOrThrow;
		return
		[
			GoldChoice(owner, SmallPouchGoldCost, ChooseSmallPouch, "SMALL_POUCH", "SMALL_POUCH_LOCKED"),
			GoldChoice(owner, LargePouchGoldCost, ChooseLargePouch, "LARGE_POUCH", "LARGE_POUCH_LOCKED"),
			Choice(Leave, "LEAVE")
		];
	}

	private async Task ChooseSmallPouch()
	{
		await SpendGold(SmallPouchGoldCost);
		await ObtainRandomRelic(RelicRarity.Common);
		Finish("SMALL_POUCH");
	}

	private async Task ChooseLargePouch()
	{
		await SpendGold(LargePouchGoldCost);
		for (int i = 0; i < LargePouchRelicCount; i++)
		{
			await ObtainRandomRelic(RelicRarity.Common);
		}

		Finish("LARGE_POUCH");
	}

	private Task Leave()
	{
		Finish("LEAVE");
		return Task.CompletedTask;
	}
}
