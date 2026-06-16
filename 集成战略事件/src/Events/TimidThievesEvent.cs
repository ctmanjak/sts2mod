using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;

namespace IntegratedStrategyEvents.Events;

public sealed partial class TimidThievesEvent : IntegratedStrategyEventModel
{
	private const int DeepSearchMaxHpLoss = 4;
	private const int CasualSearchMaxHpLoss = 2;

	private bool _hasTakenReward;

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		return SearchOptions(OwnerOrThrow, InitialPage);
	}

	private IReadOnlyList<EventOption> SearchOptions(Player owner, string pageKey)
	{
		return
		[
			CanLoseMaxHp(owner, DeepSearchMaxHpLoss)
				? Choice(DeepSearch, "DEEP_SEARCH", pageKey)
				: LockedChoice("DEEP_SEARCH_LOCKED", pageKey),
			CanLoseMaxHp(owner, CasualSearchMaxHpLoss)
				? Choice(CasualSearch, "CASUAL_SEARCH", pageKey)
				: LockedChoice("CASUAL_SEARCH_LOCKED", pageKey),
			Choice(Leave, "LEAVE", pageKey)
		];
	}

	private async Task DeepSearch()
	{
		await LoseMaxHp(DeepSearchMaxHpLoss);
		ShowLootPage(DrawLootBranch(DeepSearchLootCount));
	}

	private async Task CasualSearch()
	{
		await LoseMaxHp(CasualSearchMaxHpLoss);
		ShowLootPage(DrawLootBranch(CasualSearchLootCount));
	}

	private Task Leave()
	{
		Finish("LEAVE");
		return Task.CompletedTask;
	}

	private void ShowSearchPage()
	{
		string pageKey = _hasTakenReward ? "REPEAT" : InitialPage;
		ShowPage(pageKey, SearchOptions(OwnerOrThrow, pageKey));
	}
}
