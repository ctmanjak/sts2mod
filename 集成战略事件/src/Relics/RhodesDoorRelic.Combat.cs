using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Rooms;

namespace IntegratedStrategyEvents.Relics;

public sealed partial class RhodesDoorRelic
{
	private CardModel? _lastPlayedDeckCard;

	public override Task BeforeCombatStart()
	{
		_lastPlayedDeckCard = null;
		return Task.CompletedTask;
	}

	public override Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
	{
		if (TryGetPlayedDeckCard(cardPlay.Card, out CardModel? deckCard))
		{
			_lastPlayedDeckCard = deckCard;
		}

		return Task.CompletedTask;
	}

	public override Task AfterCombatEnd(CombatRoom room)
	{
		CardModel? card = _lastPlayedDeckCard;
		_lastPlayedDeckCard = null;
		if (Owner == null
			|| card == null
			|| !card.IsUpgradable
			|| card.Owner != Owner
			|| !Owner.Deck.Cards.Contains(card))
		{
			return Task.CompletedTask;
		}

		Flash();
		CardCmd.Upgrade(card, CardPreviewStyle.MessyLayout);
		return Task.CompletedTask;
	}

	private bool TryGetPlayedDeckCard(CardModel combatCard, out CardModel? deckCard)
	{
		deckCard = combatCard.DeckVersion;
		return Owner != null
			&& deckCard != null
			&& deckCard.Owner == Owner
			&& deckCard.Pile?.Type == PileType.Deck
			&& Owner.Deck.Cards.Contains(deckCard);
	}
}
