using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;

namespace IntegratedStrategyEvents.Events;

public sealed partial class ForSurvivalEvent : IntegratedStrategyEventModel
{
	private const int JoinGoldReward = 30;
	private const int SmallWorkCost = 45;
	private const int SmallWorkReward = 120;
	private const int SmallWorkSuccessPercent = 70;
	private const int BigWorkCost = 150;
	private const int BigWorkReward = 300;

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		return
		[
			Choice(Join, "JOIN"),
			Choice(Leave, "LEAVE")
		];
	}

	private async Task Join()
	{
		await GainGold(JoinGoldReward);
		ShowPage(
			"JOIN",
			[
				CreateSmallWorkOption(OwnerOrThrow),
				Choice(Leave, "LEAVE", "JOIN")
			]);
	}

	private EventOption CreateSmallWorkOption(Player owner)
	{
		return owner.Gold >= SmallWorkCost
			? Choice(WorkSmall, "WORK_SMALL", "JOIN")
			: LockedChoice("WORK_SMALL_LOCKED", "JOIN");
	}

	private EventOption CreateBigWorkOption(Player owner)
	{
		return owner.Gold >= BigWorkCost
			? Choice(WorkBig, "WORK_BIG", "WORK_SMALL")
			: LockedChoice("WORK_BIG_LOCKED", "WORK_SMALL");
	}

	private async Task WorkSmall()
	{
		await SpendGold(SmallWorkCost);
		if (OwnerOrThrow.PlayerRng.Rewards.NextInt(100) < SmallWorkSuccessPercent)
		{
			await GainGold(SmallWorkReward);
		}

		ShowPage(
			"WORK_SMALL",
			[
				CreateBigWorkOption(OwnerOrThrow),
				Choice(FleeAfterSignal, "FLEE_AFTER_SIGNAL", "WORK_SMALL")
			]);
	}

	private async Task WorkBig()
	{
		await SpendGold(BigWorkCost);
		if (OwnerOrThrow.PlayerRng.Rewards.NextBool())
		{
			await GainGold(BigWorkReward);
		}

		Finish("WORK_BIG");
	}

	private Task Leave()
	{
		Finish("LEAVE");
		return Task.CompletedTask;
	}

	private Task FleeAfterSignal()
	{
		Finish("FISH_ATTACK");
		return Task.CompletedTask;
	}
}
