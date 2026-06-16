using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;

namespace IntegratedStrategyEvents.Events;

internal static partial class IntegratedStrategyEventEffects
{
	public static Task OfferPotionReward(Player owner, PotionModel potion)
	{
		return OfferRewards(owner, new PotionReward(potion.ToMutable(), owner));
	}

	public static Task ObtainRandomRelic(Player owner)
	{
		return RelicCmd.Obtain(IntegratedStrategyEventRewards.PullRandomRelic(owner), owner);
	}

	public static Task ObtainRandomRelic(Player owner, RelicRarity rarity)
	{
		return RelicCmd.Obtain(IntegratedStrategyEventRewards.PullRandomRelic(owner, rarity), owner);
	}

	public static async Task ObtainRandomRelics(Player owner, int count)
	{
		for (int i = 0; i < count; i++)
		{
			await ObtainRandomRelic(owner);
		}
	}

	public static Task ObtainRelic<TRelic>(Player owner)
		where TRelic : RelicModel
	{
		return RelicCmd.Obtain<TRelic>(owner);
	}

	public static RelicModel? GetMostRecentlyObtainedRelic(Player owner)
	{
		for (int i = owner.Relics.Count - 1; i >= 0; i--)
		{
			RelicModel relic = owner.Relics[i];
			if (!relic.HasBeenRemovedFromState)
			{
				return relic;
			}
		}

		return null;
	}

	public static PotionModel? GetMostRecentlyObtainedPotion(Player owner)
	{
		return IntegratedStrategyPotionTracker.GetMostRecentlyObtainedPotion(owner);
	}

	public static Task DiscardPotion(PotionModel potion)
	{
		return PotionCmd.Discard(potion);
	}

	public static async Task DiscardPotionAndRemoveSlot(Player owner, PotionModel potion)
	{
		int slotIndex = owner.GetPotionSlotIndex(potion);
		if (slotIndex < 0)
		{
			return;
		}

		await DiscardPotion(potion);
		MoveEmptyPotionSlotToEnd(owner, slotIndex);

		if (owner.MaxPotionCount > 0)
		{
			await PlayerCmd.LoseMaxPotionCount(1, owner);
		}
	}

	private static void MoveEmptyPotionSlotToEnd(Player owner, int emptySlotIndex)
	{
		if (owner.PotionSlots is not List<PotionModel?> slots ||
			emptySlotIndex < 0 ||
			emptySlotIndex >= slots.Count)
		{
			return;
		}

		for (int i = emptySlotIndex; i < slots.Count - 1; i++)
		{
			slots[i] = slots[i + 1];
		}

		slots[^1] = null;
	}

	public static async Task ReplaceRelicWithRandomRelic(Player owner, RelicModel relic)
	{
		await RelicCmd.Remove(relic);
		await ObtainRandomRelic(owner);
	}

	public static Task OfferRewards(Player owner, params Reward[] rewards)
	{
		return RewardsCmd.OfferCustom(owner, rewards.ToList());
	}
}
