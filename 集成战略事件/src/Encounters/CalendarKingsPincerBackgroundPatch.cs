using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace IntegratedStrategyEvents.Encounters;

[HarmonyPatch(typeof(EncounterModel), nameof(EncounterModel.CreateBackground))]
internal static class CalendarKingsPincerCreateBackgroundPatch
{
	private const string TheInsatiableBackgroundId = "the_insatiable_boss";

	[HarmonyPrefix]
	private static bool UseTheInsatiableBackground(
		EncounterModel __instance,
		Rng rng,
		ref NCombatBackground __result)
	{
		if (!IsCalendarKingsPincerEncounter(__instance))
		{
			return true;
		}

		__result = NCombatBackground.Create(new BackgroundAssets(TheInsatiableBackgroundId, rng));
		return false;
	}

	internal static bool IsCalendarKingsPincerEncounter(EncounterModel encounter)
	{
		return encounter is CalendarKingsPincerEncounter ||
			encounter.CanonicalInstance is CalendarKingsPincerEncounter ||
			encounter is CalendarKingsPincerBossEncounter ||
			encounter.CanonicalInstance is CalendarKingsPincerBossEncounter;
	}
}

[HarmonyPatch(typeof(EncounterModel), nameof(EncounterModel.GetAssetPaths))]
internal static class CalendarKingsPincerGetAssetPathsPatch
{
	[HarmonyPostfix]
	private static void AddTheInsatiableBackgroundAssetPaths(
		EncounterModel __instance,
		IRunState runState,
		ref IEnumerable<string> __result)
	{
		if (!CalendarKingsPincerCreateBackgroundPatch.IsCalendarKingsPincerEncounter(__instance))
		{
			return;
		}

		Rng backgroundRng = NCombatRoom.GenerateBackgroundRngForCurrentPoint(runState);
		IEnumerable<string> backgroundAssetPaths =
			new BackgroundAssets("the_insatiable_boss", backgroundRng).AssetPaths;
		__result = __result
			.Concat(backgroundAssetPaths)
			.Append(CalendarKingsPincerMusicController.TrackPath)
			.Distinct()
			.ToArray();
	}
}
