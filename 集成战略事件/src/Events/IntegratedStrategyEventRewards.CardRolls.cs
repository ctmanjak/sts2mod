using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;

namespace IntegratedStrategyEvents.Events;

internal static partial class IntegratedStrategyEventRewards
{
	public static CardCreationResult RollRareCard(
		Player owner,
		CardType type,
		string sourceName = "integrated strategy event")
	{
		CardCreationOptions options = new CardCreationOptions(
				CreateRarityCardPool(owner.Character.CardPool.AllCards, CardRarity.Rare, type, sourceName),
				CardCreationSource.Other,
				CardRarityOddsType.Uniform)
			.WithFlags(CardCreationFlags.NoModifyHooks);

		return CardFactory.CreateForReward(owner, 1, options).FirstOrDefault()
			?? throw new InvalidOperationException($"Failed to roll a rare {type} card for {sourceName}.");
	}

	public static CardCreationResult RollOffColorCard(
		Player owner,
		string sourceName = "integrated strategy event")
	{
		CardCreationOptions options = new CardCreationOptions(
				CreateOffColorCardPools(owner, sourceName),
				CardCreationSource.Other,
				CardRarityOddsType.RegularEncounter)
			.WithFlags(CardCreationFlags.NoModifyHooks);

		return CardFactory.CreateForReward(owner, 1, options).FirstOrDefault()
			?? throw new InvalidOperationException($"Failed to roll an off-color card for {sourceName}.");
	}

	public static IReadOnlyList<CardCreationResult> RollRegularCards(
		Player owner,
		int count,
		string sourceName = "integrated strategy event")
	{
		CardCreationOptions options = new CardCreationOptions(
				[owner.Character.CardPool],
				CardCreationSource.Other,
				CardRarityOddsType.RegularEncounter)
			.WithFlags(CardCreationFlags.NoModifyHooks);

		List<CardCreationResult> results = CardFactory.CreateForReward(owner, count, options).ToList();
		if (results.Count == 0)
		{
			throw new InvalidOperationException($"Failed to roll random cards for {sourceName}.");
		}

		return results;
	}

	public static CardModel RollPoolCard<TCardPool>(
		Player owner,
		CardRarity? rarity = null,
		string sourceName = "integrated strategy event")
		where TCardPool : CardPoolModel
	{
		List<CardModel> pool = ModelDb.CardPool<TCardPool>().AllCards
			.Where(card => (!rarity.HasValue || card.Rarity == rarity.Value)
				&& card.CanBeGeneratedByModifiers)
			.ToList();
		if (pool.Count == 0)
		{
			pool = ModelDb.CardPool<TCardPool>().AllCards
				.Where(card => !rarity.HasValue || card.Rarity == rarity.Value)
				.ToList();
		}

		if (pool.Count == 0)
		{
			string rarityLabel = rarity.HasValue ? $" {rarity.Value}" : "";
			throw new InvalidOperationException($"No{rarityLabel} cards were available in {typeof(TCardPool).Name} for {sourceName}.");
		}

		return owner.PlayerRng.Rewards.NextItem(pool)
			?? throw new InvalidOperationException($"Failed to roll a card from {typeof(TCardPool).Name} for {sourceName}.");
	}

	public static CardModel RollSpecificCard(
		Player owner,
		IReadOnlyList<CardModel> pool,
		string sourceName = "integrated strategy event")
	{
		if (pool.Count == 0)
		{
			throw new InvalidOperationException($"No specific cards were configured for {sourceName}.");
		}

		return owner.PlayerRng.Rewards.NextItem(pool)
			?? throw new InvalidOperationException($"Failed to roll a specific card for {sourceName}.");
	}
}
