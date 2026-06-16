using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;

namespace IntegratedStrategyEvents.Events;

public sealed partial class DepartedGardenEvent : IntegratedStrategyEventModel
{
	private const int UncommonCardRewardOptionCount = 3;
	private const int HealAmount = 18;
	private const int GoldReward = 80;

	private static IReadOnlyList<CardModel> CreateAmiyaAncientCardPool()
	{
		List<CardModel> pool =
		[
			ModelDb.Card<NeowsFury>(),
			ModelDb.Card<Whistle>(),
			ModelDb.Card<Wish>(),
			ModelDb.Card<BrightestFlame>(),
			ModelDb.Card<Apparition>(),
			ModelDb.Card<Apotheosis>(),
			ModelDb.Card<Relax>()
		];

		List<CardModel> ancientCards = pool
			.Where(static card => card.Rarity == CardRarity.Ancient)
			.ToList();
		if (ancientCards.Count == 0)
		{
			throw new InvalidOperationException("No Ancient cards were configured for the Departed Garden Amiya option.");
		}

		return ancientCards;
	}

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		return
		[
			Choice(ChooseAscalon, "ASCALON"),
			Choice(ChooseTouch, "TOUCH"),
			Choice(ChooseFitzroy, "FITZROY"),
			Choice(ChooseErikson, "ERIKSON"),
			Choice(ChooseAmiya, "AMIYA")
		];
	}

	private async Task ChooseAscalon()
	{
		Finish("ASCALON");
		await OfferRarityCardReward(UncommonCardRewardOptionCount, CardRarity.Uncommon);
	}

	private async Task ChooseTouch()
	{
		await Heal(HealAmount);
		Finish("TOUCH");
	}

	private async Task ChooseFitzroy()
	{
		await GainGold(GoldReward);
		Finish("FITZROY");
	}

	private async Task ChooseErikson()
	{
		await ObtainRandomRelic(RelicRarity.Common);
		Finish("ERIKSON");
	}

	private async Task ChooseAmiya()
	{
		Finish("AMIYA");
		await GrantRandomSpecificCard(CreateAmiyaAncientCardPool());
		await GrantRandomPoolCard<CurseCardPool>(CardRarity.Curse);
	}
}
