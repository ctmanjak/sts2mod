using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Saves;

namespace IntegratedStrategyEvents.Events;

internal static partial class IntegratedStrategyEventEffects
{
	public static async Task AddRandomRareCardToDeck(Player owner, CardType type, string sourceName)
	{
		CardCreationResult reward = IntegratedStrategyEventRewards.RollRareCard(owner, type, sourceName);
		await AddCreatedCardToDeckAndPreview(reward.Card);
	}

	public static async Task AddRandomOffColorCardToDeck(Player owner, string sourceName)
	{
		CardCreationResult reward = IntegratedStrategyEventRewards.RollOffColorCard(owner, sourceName);
		await AddCreatedCardToDeckAndPreview(reward.Card);
	}

	public static async Task AddRandomPoolCardToDeck<TCardPool>(
		Player owner,
		CardRarity? rarity,
		string sourceName)
		where TCardPool : CardPoolModel
	{
		CardModel template = IntegratedStrategyEventRewards.RollPoolCard<TCardPool>(owner, rarity, sourceName);
		CardModel card = owner.RunState.CreateCard(template, owner);
		await AddCreatedCardToDeckAndPreview(card);
	}

	public static async Task AddRandomSpecificCardToDeck(
		Player owner,
		IReadOnlyList<CardModel> templates,
		string sourceName)
	{
		CardModel template = IntegratedStrategyEventRewards.RollSpecificCard(owner, templates, sourceName);
		CardModel card = owner.RunState.CreateCard(template, owner);
		await AddCreatedCardToDeckAndPreview(card);
	}

	public static async Task AddRandomCardsToDeck(Player owner, int count, string sourceName)
	{
		IReadOnlyList<CardCreationResult> rewards = IntegratedStrategyEventRewards.RollRegularCards(
			owner,
			count,
			sourceName);
		List<CardPileAddResult> results = new(rewards.Count);
		foreach (CardCreationResult reward in rewards)
		{
			CardPileAddResult? added = await AddCreatedCardToDeck(reward.Card);
			if (added != null)
			{
				results.Add(added.Value);
			}
		}

		PreviewAddedCards(results);
	}

	public static async Task AddCardToDeck<TCard>(Player owner)
		where TCard : CardModel
	{
		CardModel card = owner.RunState.CreateCard(ModelDb.Card<TCard>(), owner);
		await AddCreatedCardToDeckAndPreview(card);
	}

	public static Task AddCurseToDeck<TCard>(Player owner)
		where TCard : CardModel
	{
		return CardPileCmd.AddCurseToDeck<TCard>(owner);
	}

	private static async Task AddCreatedCardToDeckAndPreview(CardModel card)
	{
		CardPileAddResult? added = await AddCreatedCardToDeck(card);
		if (added != null)
		{
			PreviewAddedCards([added.Value]);
		}
	}

	private static async Task<CardPileAddResult?> AddCreatedCardToDeck(CardModel card)
	{
		CardPileAddResult added = await CardPileCmd.Add(card, PileType.Deck);
		if (!added.success)
		{
			return null;
		}

		SaveManager.Instance.MarkCardAsSeen(added.cardAdded);
		return added;
	}

	private static void PreviewAddedCards(IReadOnlyList<CardPileAddResult> results)
	{
		if (results.Count > 0)
		{
			CardCmd.PreviewCardPileAdd(results, 2f);
		}
	}
}
