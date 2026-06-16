using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace IntegratedStrategyEvents.Events;

public abstract partial class IntegratedStrategyEventModel
{
	protected Task OfferRandomPotionReward(PotionRarity? preferredRarity = null)
	{
		Player owner = OwnerOrThrow;
		PotionModel potion = IntegratedStrategyEventRewards.RollPotion(owner, preferredRarity, GetType().Name);
		return IntegratedStrategyEventEffects.OfferPotionReward(owner, potion);
	}

	protected Task OfferPotionReward<TPotion>()
		where TPotion : PotionModel
	{
		return IntegratedStrategyEventEffects.OfferPotionReward(OwnerOrThrow, ModelDb.Potion<TPotion>());
	}

	protected Task ObtainRandomRelic()
	{
		return IntegratedStrategyEventEffects.ObtainRandomRelic(OwnerOrThrow);
	}

	protected Task ObtainRandomRelic(RelicRarity rarity)
	{
		return IntegratedStrategyEventEffects.ObtainRandomRelic(OwnerOrThrow, rarity);
	}

	protected Task ObtainRandomRelics(int count)
	{
		return IntegratedStrategyEventEffects.ObtainRandomRelics(OwnerOrThrow, count);
	}

	protected Task ObtainRelic<TRelic>()
		where TRelic : RelicModel
	{
		return IntegratedStrategyEventEffects.ObtainRelic<TRelic>(OwnerOrThrow);
	}

	protected RelicModel? GetMostRecentlyObtainedRelic()
	{
		return IntegratedStrategyEventEffects.GetMostRecentlyObtainedRelic(OwnerOrThrow);
	}

	protected PotionModel? GetMostRecentlyObtainedPotion()
	{
		return IntegratedStrategyEventEffects.GetMostRecentlyObtainedPotion(OwnerOrThrow);
	}

	protected Task DiscardPotion(PotionModel potion)
	{
		return IntegratedStrategyEventEffects.DiscardPotion(potion);
	}

	protected Task DiscardPotionAndRemoveSlot(PotionModel potion)
	{
		return IntegratedStrategyEventEffects.DiscardPotionAndRemoveSlot(OwnerOrThrow, potion);
	}

	protected Task ReplaceRelicWithRandomRelic(RelicModel relic)
	{
		return IntegratedStrategyEventEffects.ReplaceRelicWithRandomRelic(OwnerOrThrow, relic);
	}

	protected Task OfferRegularCardReward(int optionCount)
	{
		Player owner = OwnerOrThrow;
		return IntegratedStrategyEventEffects.OfferRewards(
			owner,
			IntegratedStrategyEventRewards.CreateRegularCardReward(owner, optionCount));
	}

	protected Task OfferRareCardReward(int optionCount, CardType? type = null)
	{
		Player owner = OwnerOrThrow;
		return IntegratedStrategyEventEffects.OfferRewards(
			owner,
			IntegratedStrategyEventRewards.CreateRareCardReward(owner, optionCount, type, GetType().Name));
	}

	protected Task OfferRarityCardReward(int optionCount, CardRarity rarity, CardType? type = null)
	{
		Player owner = OwnerOrThrow;
		return IntegratedStrategyEventEffects.OfferRewards(
			owner,
			IntegratedStrategyEventRewards.CreateRarityCardReward(owner, optionCount, rarity, type, GetType().Name));
	}

	protected Task OfferColorlessCardReward(int optionCount)
	{
		Player owner = OwnerOrThrow;
		return IntegratedStrategyEventEffects.OfferRewards(
			owner,
			IntegratedStrategyEventRewards.CreateColorlessCardReward(owner, optionCount, GetType().Name));
	}

	protected Task OfferOffColorCardReward(int optionCount)
	{
		Player owner = OwnerOrThrow;
		return IntegratedStrategyEventEffects.OfferRewards(
			owner,
			IntegratedStrategyEventRewards.CreateOffColorCardReward(owner, optionCount, GetType().Name));
	}

	protected Task GrantRandomRareCard(CardType type)
	{
		return IntegratedStrategyEventEffects.AddRandomRareCardToDeck(OwnerOrThrow, type, GetType().Name);
	}

	protected Task GrantRandomOffColorCard()
	{
		return IntegratedStrategyEventEffects.AddRandomOffColorCardToDeck(OwnerOrThrow, GetType().Name);
	}

	protected Task GrantRandomPoolCard<TCardPool>(CardRarity? rarity = null)
		where TCardPool : CardPoolModel
	{
		return IntegratedStrategyEventEffects.AddRandomPoolCardToDeck<TCardPool>(
			OwnerOrThrow,
			rarity,
			GetType().Name);
	}

	protected Task GrantRandomSpecificCard(IReadOnlyList<CardModel> templates)
	{
		return IntegratedStrategyEventEffects.AddRandomSpecificCardToDeck(
			OwnerOrThrow,
			templates,
			GetType().Name);
	}

	protected Task GrantRandomCards(int count)
	{
		return IntegratedStrategyEventEffects.AddRandomCardsToDeck(OwnerOrThrow, count, GetType().Name);
	}

	protected Task GrantCard<TCard>()
		where TCard : CardModel
	{
		return IntegratedStrategyEventEffects.AddCardToDeck<TCard>(OwnerOrThrow);
	}

	protected Task GrantCurse<TCard>()
		where TCard : CardModel
	{
		return IntegratedStrategyEventEffects.AddCurseToDeck<TCard>(OwnerOrThrow);
	}
}
