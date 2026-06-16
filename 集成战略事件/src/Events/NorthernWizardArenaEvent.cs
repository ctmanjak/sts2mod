using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;

namespace IntegratedStrategyEvents.Events;

public sealed partial class NorthernWizardArenaEvent : IntegratedStrategyEventModel
{
	private const int EntryGoldCost = 40;
	private const int HornRockGoldReward = 60;
	private const int TreeEyebrowsHealAmount = 4;

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		return InitialOptions(OwnerOrThrow, InitialPage);
	}

	private IReadOnlyList<EventOption> InitialOptions(Player owner, string pageKey)
	{
		return
		[
			GoldChoice(owner, EntryGoldCost, StartMatch, "CHOOSE_FIGHTER", "CHOOSE_FIGHTER_LOCKED", pageKey),
			Choice(Leave, "LEAVE_EARLY", pageKey)
		];
	}

	private IReadOnlyList<EventOption> RepeatOptions(Player owner)
	{
		return
		[
			GoldChoice(owner, EntryGoldCost, StartMatch, "RETRY", "RETRY_LOCKED", "REPEAT"),
			Choice(Leave, "LEAVE_REPEAT", "REPEAT")
		];
	}

	private async Task StartMatch()
	{
		await SpendGold(EntryGoldCost);
		ShowOutcomePage(DrawOutcome());
	}

	private Task Leave()
	{
		Finish("LEAVE");
		return Task.CompletedTask;
	}

	private ArenaOutcome DrawOutcome()
	{
		int roll = OwnerOrThrow.PlayerRng.Rewards.NextInt(100);
		if (roll < 20)
		{
			return ArenaOutcome.HornRock;
		}

		if (roll < 40)
		{
			return ArenaOutcome.TreeEyebrows;
		}

		if (roll < 55)
		{
			return ArenaOutcome.HotStan;
		}

		if (roll < 60)
		{
			return ArenaOutcome.OldBjorn;
		}

		return ArenaOutcome.Helma;
	}

	private void ShowOutcomePage(ArenaOutcome branch)
	{
		ArenaPrize prize = GetPrize(branch);
		ShowPage(prize.PageKey, [Choice(() => CollectPrize(prize), prize.OptionKey, prize.PageKey)]);
	}

	private async Task CollectPrize(ArenaPrize prize)
	{
		await prize.Apply(this);
		ShowPage("REPEAT", RepeatOptions(OwnerOrThrow));
	}

	private static ArenaPrize GetPrize(ArenaOutcome branch)
	{
		return branch switch
		{
			ArenaOutcome.HornRock => new ArenaPrize(
				"HORN_ROCK",
				"CLAIM_HORN_ROCK",
				static eventModel => eventModel.GainGold(HornRockGoldReward)),
			ArenaOutcome.TreeEyebrows => new ArenaPrize(
				"TREE_EYEBROWS",
				"CLAIM_TREE_EYEBROWS",
				static eventModel => eventModel.Heal(TreeEyebrowsHealAmount)),
			ArenaOutcome.HotStan => new ArenaPrize(
				"HOT_STAN",
				"CLAIM_HOT_STAN",
				static eventModel => eventModel.OfferRandomPotionReward()),
			ArenaOutcome.OldBjorn => new ArenaPrize(
				"OLD_BJORN",
				"CLAIM_OLD_BJORN",
				static eventModel => eventModel.ObtainRandomRelic()),
			ArenaOutcome.Helma => new ArenaPrize(
				"HELMA",
				"ACCEPT_LOSS",
				static _ => Task.CompletedTask),
			_ => throw new ArgumentOutOfRangeException(nameof(branch), branch, null)
		};
	}

	private sealed record ArenaPrize(
		string PageKey,
		string OptionKey,
		Func<NorthernWizardArenaEvent, Task> Apply);

	private enum ArenaOutcome
	{
		HornRock,
		TreeEyebrows,
		HotStan,
		OldBjorn,
		Helma
	}
}
