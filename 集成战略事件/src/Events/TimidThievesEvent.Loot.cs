namespace IntegratedStrategyEvents.Events;

public sealed partial class TimidThievesEvent
{
	private const int DeepSearchLootCount = 3;
	private const int CasualSearchLootCount = 6;
	private const int BigHealAmount = 8;
	private const int SmallHealAmount = 4;
	private const int BigGoldAmount = 100;
	private const int SmallGoldAmount = 30;

	private static readonly IReadOnlyList<LootOutcome> LootOutcomes =
	[
		new("BIG_FOOD", "TAKE_FOOD", static eventModel => eventModel.Heal(BigHealAmount), true),
		new("RELIC", "TAKE_RELIC", static eventModel => eventModel.ObtainRandomRelic(), true),
		new("BIG_GOLD", "TAKE_GOLD", static eventModel => eventModel.GainGold(BigGoldAmount), true),
		new("SMALL_GOLD", "TAKE_GOLD", static eventModel => eventModel.GainGold(SmallGoldAmount), true),
		new("SMALL_FOOD", "TAKE_FOOD", static eventModel => eventModel.Heal(SmallHealAmount), true),
		new("NOTHING", "CONTINUE", static _ => Task.CompletedTask, false)
	];

	private LootBranch DrawLootBranch(int outcomeCount)
	{
		if (outcomeCount < 1 || outcomeCount > LootOutcomes.Count)
		{
			throw new ArgumentOutOfRangeException(nameof(outcomeCount), outcomeCount, null);
		}

		return (LootBranch)OwnerOrThrow.PlayerRng.Rewards.NextInt(outcomeCount);
	}

	private void ShowLootPage(LootBranch branch)
	{
		LootOutcome outcome = GetLootOutcome(branch);
		ShowPage(outcome.PageKey, [Choice(() => CollectLoot(outcome), outcome.OptionKey, outcome.PageKey)]);
	}

	private async Task CollectLoot(LootOutcome outcome)
	{
		await outcome.Apply(this);
		_hasTakenReward |= outcome.CountsAsReward;
		ShowSearchPage();
	}

	private static LootOutcome GetLootOutcome(LootBranch branch)
	{
		int index = (int)branch;
		if ((uint)index >= LootOutcomes.Count)
		{
			throw new ArgumentOutOfRangeException(nameof(branch), branch, null);
		}

		return LootOutcomes[index];
	}

	private sealed record LootOutcome(
		string PageKey,
		string OptionKey,
		Func<TimidThievesEvent, Task> Apply,
		bool CountsAsReward);

	private enum LootBranch
	{
		BigFood,
		Relic,
		BigGold,
		SmallGold,
		SmallFood,
		Nothing
	}
}
