using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;

namespace IntegratedStrategyEvents.Events;

public sealed partial class UrsusEvent : IntegratedStrategyEventModel
{
	private const int MaxHpGain = 8;
	private const int BrawlHpLoss = 8;
	private const int BrawlGoldReward = 120;

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		Player owner = OwnerOrThrow;
		return
		[
			Choice(Ask, "ASK"),
			HpChoice(owner, BrawlHpLoss, Brawl, "BRAWL", "BRAWL_LOCKED"),
			Choice(Leave, "LEAVE")
		];
	}

	private async Task Ask()
	{
		await GainMaxHp(MaxHpGain);
		Finish("ASK");
	}

	private async Task Brawl()
	{
		await LoseHp(BrawlHpLoss);
		await GainGold(BrawlGoldReward);
		Finish("BRAWL");
	}

	private Task Leave()
	{
		Finish("LEAVE");
		return Task.CompletedTask;
	}
}
