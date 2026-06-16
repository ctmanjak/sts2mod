using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;

namespace IntegratedStrategyEvents.Events;

internal static partial class IntegratedStrategyEventRewards
{
	public static CardReward CreateRegularCardReward(Player owner, int optionCount)
	{
		CardCreationOptions options = new(
			[owner.Character.CardPool],
			CardCreationSource.Other,
			CardRarityOddsType.RegularEncounter);
		return new CardReward(options, optionCount, owner);
	}

	public static CardReward CreateRareCardReward(
		Player owner,
		int optionCount,
		CardType? type = null,
		string sourceName = "integrated strategy event")
	{
		return CreateRarityCardReward(owner, optionCount, CardRarity.Rare, type, sourceName);
	}

	public static CardReward CreateRarityCardReward(
		Player owner,
		int optionCount,
		CardRarity rarity,
		CardType? type = null,
		string sourceName = "integrated strategy event")
	{
		List<CardModel> pool = CreateRarityCardPool(owner.Character.CardPool.AllCards, rarity, type, sourceName);
		CardCreationOptions options = new(
			pool,
			CardCreationSource.Other,
			CardRarityOddsType.Uniform);
		return new CardReward(options, optionCount, owner);
	}

	public static CardReward CreateColorlessCardReward(
		Player owner,
		int optionCount,
		string sourceName = "integrated strategy event")
	{
		List<CardModel> pool = ModelDb.CardPool<ColorlessCardPool>().AllCards
			.Where(static card => card.CanBeGeneratedByModifiers)
			.ToList();
		if (pool.Count == 0)
		{
			pool = ModelDb.CardPool<ColorlessCardPool>().AllCards.ToList();
		}

		if (pool.Count == 0)
		{
			throw new InvalidOperationException($"No colorless cards were available for {sourceName}.");
		}

		CardCreationOptions options = new(
			pool,
			CardCreationSource.Other,
			CardRarityOddsType.Uniform);
		return new CardReward(options, optionCount, owner);
	}

	public static CardReward CreateOffColorCardReward(
		Player owner,
		int optionCount,
		string sourceName = "integrated strategy event")
	{
		CardCreationOptions options = new(
			CreateOffColorCardPools(owner, sourceName),
			CardCreationSource.Other,
			CardRarityOddsType.RegularEncounter);
		return new CardReward(options, optionCount, owner);
	}
}
