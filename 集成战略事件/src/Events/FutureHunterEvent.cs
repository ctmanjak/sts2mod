using IntegratedStrategyEvents.Encounters;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Events;

namespace IntegratedStrategyEvents.Events;

public sealed partial class FutureHunterEvent : IntegratedStrategyEventModel
{
	private const int CardsToRemove = 2;

	public override bool IsShared => true;

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		return
		[
			Choice(Refuse, "REFUSE"),
			HasRemovableDeckCards(CardType.Skill, CardsToRemove)
				? Choice(OfferHope, "OFFER_HOPE")
				: LockedChoice("OFFER_HOPE_LOCKED"),
			HasRemovableDeckCards(CardType.Attack, CardsToRemove)
				? Choice(OfferHatred, "OFFER_HATRED")
				: LockedChoice("OFFER_HATRED_LOCKED"),
			Choice(Leave, "LEAVE")
		];
	}

	private Task Refuse()
	{
		ShowFightPage<FutureHunterSarkazDescendantHatredCollectorsEncounter>("REFUSE");
		return Task.CompletedTask;
	}

	private async Task OfferHope()
	{
		await RemoveDeckCards(CardsToRemove, CardType.Skill);
		Finish("OFFER_HOPE");
	}

	private async Task OfferHatred()
	{
		await RemoveDeckCards(CardsToRemove, CardType.Attack);
		Finish("OFFER_HATRED");
	}

	private Task Leave()
	{
		Finish("LEAVE");
		return Task.CompletedTask;
	}
}
