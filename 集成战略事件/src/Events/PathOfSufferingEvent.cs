using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;

namespace IntegratedStrategyEvents.Events;

public sealed partial class PathOfSufferingEvent : IntegratedStrategyEventModel
{
	private const int WorkHpLoss = 6;
	private const int FieldMaxHpGain = 2;
	private const int ColorlessCardRewardOptionCount = 3;

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		Player owner = OwnerOrThrow;
		return
		[
			HpChoice(owner, WorkHpLoss, HaulFood, "HAUL_FOOD", "HAUL_FOOD_LOCKED"),
			Choice(Leave, "LEAVE")
		];
	}

	private async Task HaulFood()
	{
		await LoseHp(WorkHpLoss);
		await OfferRandomPotionReward();
		ShowPage("FOOD", FoodOptions(OwnerOrThrow));
	}

	private async Task FieldWork()
	{
		await LoseHpAndGainMaxHp(WorkHpLoss, FieldMaxHpGain);
		ShowPage("FIELD", FieldOptions(OwnerOrThrow));
	}

	private async Task SwingBell()
	{
		await LoseHp(WorkHpLoss);
		ShowPage("BELL", BellOptions(OwnerOrThrow));
		await OfferColorlessCardReward(ColorlessCardRewardOptionCount);
	}

	private async Task Watchtower()
	{
		await LoseHp(WorkHpLoss);
		await ObtainRandomRelic();
		Finish("COMPLETE");
	}

	private Task Leave()
	{
		Finish("LEAVE");
		return Task.CompletedTask;
	}

	private IReadOnlyList<EventOption> FoodOptions(Player owner)
	{
		return
		[
			HpChoice(owner, WorkHpLoss, FieldWork, "FIELD_WORK", "FIELD_WORK_LOCKED", "FOOD"),
			Choice(Leave, "EARLY_LEAVE", "FOOD")
		];
	}

	private IReadOnlyList<EventOption> FieldOptions(Player owner)
	{
		return
		[
			HpChoice(owner, WorkHpLoss, SwingBell, "SWING_BELL", "SWING_BELL_LOCKED", "FIELD"),
			Choice(Leave, "EARLY_LEAVE", "FIELD")
		];
	}

	private IReadOnlyList<EventOption> BellOptions(Player owner)
	{
		return
		[
			HpChoice(owner, WorkHpLoss, Watchtower, "WATCHTOWER", "WATCHTOWER_LOCKED", "BELL"),
			Choice(Leave, "EARLY_LEAVE", "BELL")
		];
	}
}
