using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves;

namespace IntegratedStrategyEvents.Relics;

public sealed partial class RhodesDoorRelic
{
	public override async Task AfterObtained()
	{
		Player? owner = Owner;
		if (owner == null)
		{
			return;
		}

		CardModel?[] templateCandidates =
		[
			FindStartingDeckCard(owner, CardTag.Strike, CardType.Attack),
			FindStartingDeckCard(owner, CardTag.Defend, CardType.Skill)
		];
		List<CardModel> templates = templateCandidates
			.Where(static card => card != null)
			.Cast<CardModel>()
			.ToList();
		if (templates.Count == 0)
		{
			return;
		}

		List<CardPileAddResult> results = new(templates.Count);
		foreach (CardModel template in templates)
		{
			CardModel card = owner.RunState.CreateCard(template, owner);
			CardPileAddResult added = await CardPileCmd.Add(card, PileType.Deck);
			if (!added.success)
			{
				continue;
			}

			SaveManager.Instance.MarkCardAsSeen(added.cardAdded);
			results.Add(added);
		}

		if (results.Count == 0)
		{
			return;
		}

		Flash();
		CardCmd.PreviewCardPileAdd(results, 2f);
	}

	private static CardModel? FindStartingDeckCard(Player owner, CardTag tag, CardType type)
	{
		return owner.Character.StartingDeck.FirstOrDefault(card =>
				card.IsBasicStrikeOrDefend
				&& card.Type == type
				&& card.Tags.Contains(tag))
			?? owner.Character.StartingDeck.FirstOrDefault(card =>
				card.Type == type
				&& card.Tags.Contains(tag))
			?? owner.Deck.Cards.FirstOrDefault(card =>
				card.IsBasicStrikeOrDefend
				&& card.Type == type
				&& card.Tags.Contains(tag));
	}
}
