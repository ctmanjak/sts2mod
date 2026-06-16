using IntegratedStrategyEvents.TreeHoles;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Models;

namespace IntegratedStrategyEvents.Events;

public sealed partial class GlimpseEvent : IntegratedStrategyEventModel
{
	private const string TimeSliceActName = "绀碧摇篮";

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		PotionModel? latestPotion = GetMostRecentlyObtainedPotion();
		return
		[
			latestPotion != null
				? PotionCostChoice(latestPotion, ClearSeabornTrace, "CLEAR_SEABORN")
				: LockedChoice("CLEAR_SEABORN_LOCKED"),
			Choice(ListenToMizuki, "LISTEN_TO_MIZUKI")
		];
	}

	private async Task ClearSeabornTrace()
	{
		PotionModel? latestPotion = GetMostRecentlyObtainedPotion();
		if (latestPotion != null)
		{
			await DiscardPotionAndRemoveSlot(latestPotion);
		}

		ShowPage("CLEAR_SEABORN", [Choice(EnterMysteriousTime, "ENTER_TIME_SPACE", "CLEAR_SEABORN")]);
	}

	private async Task ListenToMizuki()
	{
		await ObtainRandomRelic();
		Finish("LISTEN_TO_MIZUKI");
	}

	private Task EnterMysteriousTime()
	{
		return IntegratedStrategyTreeHoleController.EnterFromEvent(OwnerOrThrow, TimeSliceActName);
	}
}
