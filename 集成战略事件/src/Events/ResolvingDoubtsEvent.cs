using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.Events;

namespace IntegratedStrategyEvents.Events;

public sealed partial class ResolvingDoubtsEvent : IntegratedStrategyEventModel
{
	private const int GoldReward = 60;
	private const int CardRewardOptionCount = 4;

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		return
		[
			Choice(ChooseSanktaWins, "SANKTA_WINS"),
			Choice(ChooseSarkazWins, "SARKAZ_WINS"),
			Choice(ChooseHalos, "HALOS")
		];
	}

	private async Task ChooseSanktaWins()
	{
		await GainGold(GoldReward);
		Finish("SANKTA_WINS");
	}

	private async Task ChooseSarkazWins()
	{
		await OfferRandomPotionReward(PotionRarity.Rare);
		Finish("SARKAZ_WINS");
	}

	private async Task ChooseHalos()
	{
		Finish("HALOS");
		await OfferRegularCardReward(CardRewardOptionCount);
	}
}
