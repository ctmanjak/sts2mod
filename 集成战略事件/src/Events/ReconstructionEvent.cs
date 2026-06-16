using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace IntegratedStrategyEvents.Events;

public sealed partial class ReconstructionEvent : IntegratedStrategyEventModel
{
	private const int InitialTransformCardCount = 2;
	private const int InitialRemoveCardCount = 2;
	private const int MaxHpGain = 30;
	private const string CollapseClearedPage = "COLLAPSE_CLEARED";
	private const string RepairedPage = "SITE_REPAIRED";
	private const string RestartedPage = "FACILITY_RESTARTED";

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		Player owner = OwnerOrThrow;
		return
		[
			HasTransformableDeckCards(owner, InitialTransformCardCount)
				? Choice(IgnoreCollapsePollution, "IGNORE_COLLAPSE_POLLUTION")
				: LockedChoice("IGNORE_COLLAPSE_POLLUTION_LOCKED"),
			HasRemovableDeckCards(InitialRemoveCardCount)
				? Choice(OverloadDetectors, "OVERLOAD_DETECTORS")
				: LockedChoice("OVERLOAD_DETECTORS_LOCKED"),
			Choice(Endure, "ENDURE")
		];
	}

	private async Task IgnoreCollapsePollution()
	{
		await TransformDeckCards(InitialTransformCardCount);
		ShowCollapseClearedPage();
	}

	private async Task OverloadDetectors()
	{
		await RemoveDeckCards(InitialRemoveCardCount);
		ShowCollapseClearedPage();
	}

	private async Task Endure()
	{
		await FillEmptyPotionSlots();
		ShowCollapseClearedPage();
	}

	private void ShowCollapseClearedPage()
	{
		ShowPage(
			CollapseClearedPage,
			[
				RelicPreviewChoice<BeatingRemnant, TungstenRod>(ForceRepair, "FORCE_REPAIR", CollapseClearedPage),
				Choice(BuyMaterials, "BUY_MATERIALS", CollapseClearedPage)
			]);
	}

	private async Task ForceRepair()
	{
		await ObtainRelic<BeatingRemnant>();
		await ObtainRelic<TungstenRod>();
		ShowRepairedPage();
	}

	private async Task BuyMaterials()
	{
		Player owner = OwnerOrThrow;
		await SpendGold(owner.Gold);
		await UpgradeAllDeckCards(owner);
		ShowRepairedPage();
	}

	private void ShowRepairedPage()
	{
		ShowPage(
			RepairedPage,
			[
				Choice(BruteForcePower, "BRUTE_FORCE_POWER", RepairedPage),
				RelicChoice<LizardTail>(CarefulHope, "CAREFUL_HOPE", RepairedPage)
			]);
	}

	private async Task BruteForcePower()
	{
		await GainMaxHp(MaxHpGain);
		Finish(RestartedPage);
	}

	private async Task CarefulHope()
	{
		await ObtainRelic<LizardTail>();
		Finish(RestartedPage);
	}

	private static async Task UpgradeAllDeckCards(Player owner)
	{
		List<CardModel> upgradableCards = PileType.Deck.GetPile(owner).Cards
			.Where(static card => card.IsUpgradable)
			.ToList();
		if (upgradableCards.Count > 0)
		{
			CardCmd.Upgrade(upgradableCards, CardPreviewStyle.MessyLayout);
			await Cmd.CustomScaledWait(0.4f, 0.8f);
		}
	}

	private async Task FillEmptyPotionSlots()
	{
		Player owner = OwnerOrThrow;
		int emptyPotionSlots = owner.PotionSlots.Count(static potion => potion == null);
		for (int i = 0; i < emptyPotionSlots; i++)
		{
			await OfferRandomPotionReward();
		}
	}
}
