using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace IntegratedStrategyEvents.Events;

internal static partial class IntegratedStrategyEventRewards
{
	private static List<CardModel> CreateRarityCardPool(
		IEnumerable<CardModel> cards,
		CardRarity rarity,
		CardType? type,
		string sourceName)
	{
		List<CardModel> pool = cards
			.Where(card => card.Rarity == rarity
				&& (!type.HasValue || card.Type == type.Value)
				&& card.CanBeGeneratedByModifiers)
			.ToList();
		if (pool.Count == 0)
		{
			pool = cards
				.Where(card => card.Rarity == rarity
					&& (!type.HasValue || card.Type == type.Value))
				.ToList();
		}

		if (pool.Count == 0)
		{
			string typeLabel = type.HasValue ? $" {type.Value}" : "";
			throw new InvalidOperationException($"No {rarity}{typeLabel} cards were available for {sourceName}.");
		}

		return pool;
	}

	private static List<CardPoolModel> CreateOffColorCardPools(Player owner, string sourceName)
	{
		ModelId currentPoolId = owner.Character.CardPool.Id;
		List<CardPoolModel> pools = owner.UnlockState.CharacterCardPools
			.Where(pool => !pool.IsColorless
				&& !pool.Id.Equals(currentPoolId)
				&& pool.AllCards.Any(static card => card.CanBeGeneratedByModifiers))
			.ToList();
		if (pools.Count == 0)
		{
			pools = owner.UnlockState.CharacterCardPools
				.Where(pool => !pool.IsColorless && !pool.Id.Equals(currentPoolId))
				.ToList();
		}

		if (pools.Count == 0)
		{
			throw new InvalidOperationException($"No off-color card pools were available for {sourceName}.");
		}

		return pools;
	}
}
