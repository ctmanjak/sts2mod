using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;

namespace IntegratedStrategyEvents.Events;

internal static class IntegratedStrategyPotionTracker
{
	private static readonly HashSet<Player> HookedPlayers = [];
	private static readonly Dictionary<Player, List<PotionModel>> ProcuredPotions = [];
	private static bool _installed;

	public static void Install()
	{
		if (_installed)
		{
			return;
		}

		RunManager.Instance.RunStarted += OnRunStarted;
		_installed = true;
	}

	public static PotionModel? GetMostRecentlyObtainedPotion(Player owner)
	{
		EnsurePlayerHooked(owner);
		if (ProcuredPotions.TryGetValue(owner, out List<PotionModel>? trackedPotions))
		{
			for (int i = trackedPotions.Count - 1; i >= 0; i--)
			{
				PotionModel potion = trackedPotions[i];
				if (IsOwnedPotion(owner, potion))
				{
					return potion;
				}
			}
		}

		return owner.Potions.Where(potion => IsOwnedPotion(owner, potion)).LastOrDefault();
	}

	private static void OnRunStarted(RunState runState)
	{
		ProcuredPotions.Clear();
		HookedPlayers.Clear();
		foreach (Player player in runState.Players)
		{
			EnsurePlayerHooked(player);
		}
	}

	private static void EnsurePlayerHooked(Player player)
	{
		if (!HookedPlayers.Add(player))
		{
			return;
		}

		player.PotionProcured += potion => Track(player, potion);
		player.PotionDiscarded += potion => Untrack(player, potion);
		player.UsedPotionRemoved += potion => Untrack(player, potion);
	}

	private static void Track(Player owner, PotionModel potion)
	{
		List<PotionModel> potions = ProcuredPotions.GetValueOrDefault(owner) ?? [];
		potions.RemoveAll(existing => ReferenceEquals(existing, potion));
		potions.Add(potion);
		ProcuredPotions[owner] = potions;
	}

	private static void Untrack(Player owner, PotionModel potion)
	{
		if (ProcuredPotions.TryGetValue(owner, out List<PotionModel>? potions))
		{
			potions.RemoveAll(existing => ReferenceEquals(existing, potion));
		}
	}

	private static bool IsOwnedPotion(Player owner, PotionModel potion)
	{
		return !potion.HasBeenRemovedFromState
			&& ReferenceEquals(potion.Owner, owner)
			&& owner.Potions.Any(current => ReferenceEquals(current, potion));
	}
}
