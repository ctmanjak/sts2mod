using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace IntegratedStrategyEvents.Events;

internal static partial class IntegratedStrategyEventEffects
{
	public static async Task UpgradeDeckCards(Player owner, int count)
	{
		List<CardModel> selectedCards = (await CardSelectCmd.FromDeckForUpgrade(
				owner,
				new CardSelectorPrefs(CardSelectorPrefs.UpgradeSelectionPrompt, count)))
			.ToList();
		CardCmd.Upgrade(selectedCards, CardPreviewStyle.MessyLayout);
		if (selectedCards.Count > 0)
		{
			await Cmd.CustomScaledWait(0.4f, 0.8f);
		}
	}

	public static int CountTransformableDeckCards(Player owner)
	{
		return PileType.Deck.GetPile(owner).Cards.Count(static card =>
			card.Type != CardType.Quest && card.IsTransformable);
	}

	public static int CountTransformableBasicDeckCards(Player owner, CardTag tag)
	{
		return PileType.Deck.GetPile(owner).Cards.Count(card => IsTransformableBasicDeckCard(card, tag));
	}

	public static async Task TransformDeckCards(Player owner, int count)
	{
		List<CardModel> selectedCards = (await CardSelectCmd.FromDeckForTransformation(
				owner,
				new CardSelectorPrefs(CardSelectorPrefs.TransformSelectionPrompt, count)))
			.ToList();

		foreach (CardModel card in selectedCards)
		{
			await CardCmd.TransformToRandom(card, owner.RunState.Rng.Niche, CardPreviewStyle.EventLayout);
		}
	}

	public static async Task TransformBasicDeckCard<TReplacement>(Player owner, CardTag tag)
		where TReplacement : CardModel
	{
		List<CardModel> deck = PileType.Deck.GetPile(owner).Cards.ToList();
		List<CardModel> selectedCards = (await CardSelectCmd.FromDeckGeneric(
				owner,
				new CardSelectorPrefs(CardSelectorPrefs.TransformSelectionPrompt, 1),
				card => IsTransformableBasicDeckCard(card, tag),
				card => deck.IndexOf(card)))
			.ToList();

		foreach (CardModel card in selectedCards)
		{
			await CardCmd.TransformTo<TReplacement>(card, CardPreviewStyle.EventLayout);
		}
	}

	private static bool IsTransformableBasicDeckCard(CardModel card, CardTag tag)
	{
		return card.IsBasicStrikeOrDefend
			&& card.Type != CardType.Quest
			&& card.IsTransformable
			&& card.Tags.Contains(tag);
	}

	public static async Task RemoveDeckCards(Player owner, int count)
	{
		List<CardModel> selectedCards = (await CardSelectCmd.FromDeckForRemoval(
				owner,
				new CardSelectorPrefs(CardSelectorPrefs.RemoveSelectionPrompt, count)
				{
					RequireManualConfirmation = true
				}))
			.ToList();

		foreach (CardModel card in selectedCards)
		{
			await CardPileCmd.RemoveFromDeck(card);
		}
	}

	public static int CountRemovableDeckCards(Player owner)
	{
		return PileType.Deck.GetPile(owner).Cards.Count(static card => card.IsRemovable);
	}

	public static async Task RemoveRandomDeckCards(Player owner, int count)
	{
		List<CardModel> candidates = PileType.Deck.GetPile(owner).Cards
			.Where(static card => card.IsRemovable)
			.ToList();
		List<CardModel> selectedCards = new(Math.Min(count, candidates.Count));
		for (int i = 0; i < count && candidates.Count > 0; i++)
		{
			CardModel card = owner.PlayerRng.Rewards.NextItem(candidates)
				?? throw new InvalidOperationException($"Failed to roll a random removable card for {nameof(RemoveRandomDeckCards)}.");
			selectedCards.Add(card);
			candidates.Remove(card);
		}

		await CardPileCmd.RemoveFromDeck(selectedCards);
	}

	public static int CountRemovableDeckCards(Player owner, CardType type)
	{
		return PileType.Deck.GetPile(owner).Cards.Count(card => card.Type == type && card.IsRemovable);
	}

	public static async Task RemoveDeckCards(Player owner, int count, CardType type)
	{
		List<CardModel> selectedCards = (await CardSelectCmd.FromDeckForRemoval(
				owner,
				new CardSelectorPrefs(CardSelectorPrefs.RemoveSelectionPrompt, count)
				{
					RequireManualConfirmation = true
				},
				card => card.Type == type))
			.ToList();

		foreach (CardModel card in selectedCards)
		{
			await CardPileCmd.RemoveFromDeck(card);
		}
	}
}
