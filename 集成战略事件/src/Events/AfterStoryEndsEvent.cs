using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Models.Cards;

namespace IntegratedStrategyEvents.Events;

public sealed partial class AfterStoryEndsEvent : IntegratedStrategyEventModel
{
	private const int CardsToUpgrade = 2;
	private const int MaxHpGain = 12;
	private const int RandomCardCount = 2;

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		Player owner = OwnerOrThrow;
		return
		[
			CreateCrumbleToAshOption(owner),
			Choice(RootAndSprout, "ROOT_AND_SPROUT"),
			Choice(EternalStillness, "ETERNAL_STILLNESS")
		];
	}

	private EventOption CreateCrumbleToAshOption(Player owner)
	{
		if (!HasUpgradableDeckCards(owner))
		{
			return LockedChoice("CRUMBLE_TO_ASH_LOCKED");
		}

		return CardPreviewChoice<Decay>(CrumbleToAsh, "CRUMBLE_TO_ASH");
	}

	private async Task CrumbleToAsh()
	{
		await UpgradeDeckCards(CardsToUpgrade);
		await GrantCard<Decay>();
		Finish("CRUMBLE_TO_ASH");
	}

	private async Task RootAndSprout()
	{
		await GainMaxHp(MaxHpGain);
		await GrantRandomCards(RandomCardCount);
		Finish("ROOT_AND_SPROUT");
	}

	private Task EternalStillness()
	{
		Finish("ETERNAL_STILLNESS");
		return Task.CompletedTask;
	}
}
