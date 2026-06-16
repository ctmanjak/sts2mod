using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace IntegratedStrategyEvents.Events;

public abstract partial class IntegratedStrategyEventModel
{
	protected Task UpgradeDeckCards(int count)
	{
		return IntegratedStrategyEventEffects.UpgradeDeckCards(OwnerOrThrow, count);
	}

	protected static bool HasUpgradableDeckCards(Player owner)
	{
		return PileType.Deck.GetPile(owner).Cards.Any(static card => card.IsUpgradable);
	}

	protected Task TransformDeckCards(int count)
	{
		return IntegratedStrategyEventEffects.TransformDeckCards(OwnerOrThrow, count);
	}

	protected bool HasTransformableDeckCards(int count)
	{
		return HasTransformableDeckCards(OwnerOrThrow, count);
	}

	protected static bool HasTransformableDeckCards(Player owner, int count)
	{
		return IntegratedStrategyEventEffects.CountTransformableDeckCards(owner) >= count;
	}

	protected static bool HasTransformableBasicDeckCard(Player owner, CardTag tag)
	{
		return IntegratedStrategyEventEffects.CountTransformableBasicDeckCards(owner, tag) > 0;
	}

	protected Task TransformBasicDeckCard<TReplacement>(CardTag tag)
		where TReplacement : CardModel
	{
		return IntegratedStrategyEventEffects.TransformBasicDeckCard<TReplacement>(OwnerOrThrow, tag);
	}

	protected Task RemoveDeckCards(int count)
	{
		return IntegratedStrategyEventEffects.RemoveDeckCards(OwnerOrThrow, count);
	}

	protected bool HasRemovableDeckCards(int count)
	{
		return IntegratedStrategyEventEffects.CountRemovableDeckCards(OwnerOrThrow) >= count;
	}

	protected Task RemoveRandomDeckCards(int count)
	{
		return IntegratedStrategyEventEffects.RemoveRandomDeckCards(OwnerOrThrow, count);
	}

	protected bool HasRemovableDeckCards(CardType type, int count)
	{
		return IntegratedStrategyEventEffects.CountRemovableDeckCards(OwnerOrThrow, type) >= count;
	}

	protected Task RemoveDeckCards(int count, CardType type)
	{
		return IntegratedStrategyEventEffects.RemoveDeckCards(OwnerOrThrow, count, type);
	}
}
