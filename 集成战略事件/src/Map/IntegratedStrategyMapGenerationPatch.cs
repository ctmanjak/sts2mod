using System.Reflection;
using System.Runtime.CompilerServices;
using IntegratedStrategyEvents.Events;
using IntegratedStrategyEvents.TreeHoles;
using HarmonyLib;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Odds;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace IntegratedStrategyEvents.Map;

[HarmonyPatch(typeof(ActModel), nameof(ActModel.GetNumberOfRooms))]
internal static class IntegratedStrategyMapLengthPatch
{
	private const int ExtraRooms = 1;

	private static void Postfix(ref int __result)
	{
		__result += ExtraRooms;
	}
}

[HarmonyPatch(typeof(RoomSet), nameof(RoomSet.EnsureNextEventIsValid))]
internal static class IntegratedStrategyFirstEventPatch
{
	private const int SecondActIndex = 1;
	private const int FirstEventBranchCount = 3;
	private static readonly ConditionalWeakTable<RunState, FirstEventChoice> FirstEventChoices = new();

	private static void Prefix(RoomSet __instance, RunState runState)
	{
		if (__instance.events.Count == 0)
		{
			return;
		}

		if (IntegratedStrategyTreeHoleController.IsAtEternalDustFirstEventPoint(runState))
		{
			int reconstructionIndex = __instance.eventsVisited % __instance.events.Count;
			RoomSet.SwapToOrCreateAtIndex<EventModel, ReconstructionEvent>(__instance.events, reconstructionIndex);
			return;
		}

		if (IntegratedStrategyTreeHoleController.IsAtEternalDustSecondEventPoint(runState))
		{
			int explorerStepIndex = __instance.eventsVisited % __instance.events.Count;
			RoomSet.SwapToOrCreateAtIndex<EventModel, ExplorerSmallStepEvent>(__instance.events, explorerStepIndex);
			return;
		}

		if (runState.CurrentActIndex != SecondActIndex ||
			IntegratedStrategyTreeHoleController.IsActive(runState) ||
			HasVisitedOrdinaryEventInCurrentAct(runState))
		{
			return;
		}

		int desiredIndex = __instance.eventsVisited % __instance.events.Count;
		FirstEventChoice choice = FirstEventChoices.GetValue(
			runState,
			ChooseFirstEvent);

		if (choice.Branch == FirstEventBranch.Change)
		{
			RoomSet.SwapToOrCreateAtIndex<EventModel, ChangeEvent>(__instance.events, desiredIndex);
			return;
		}

		if (choice.Branch == FirstEventBranch.AnomalousReport)
		{
			RoomSet.SwapToOrCreateAtIndex<EventModel, AnomalousReportEvent>(__instance.events, desiredIndex);
			return;
		}

		RoomSet.SwapToOrCreateAtIndex<EventModel, VoidPortentEvent>(__instance.events, desiredIndex);
	}

	private static FirstEventChoice ChooseFirstEvent(RunState runState)
	{
		return new FirstEventChoice((FirstEventBranch)runState.Rng.UpFront.NextInt(FirstEventBranchCount));
	}

	private static bool HasVisitedOrdinaryEventInCurrentAct(RunState runState)
	{
		return runState.MapPointHistory.Count > runState.CurrentActIndex &&
			runState.MapPointHistory[runState.CurrentActIndex].Any(static entry =>
				entry.MapPointType == MapPointType.Unknown &&
				entry.Rooms.Any(static room => room.RoomType == RoomType.Event));
	}

	private enum FirstEventBranch
	{
		VoidPortent,
		Change,
		AnomalousReport
	}

	private sealed record FirstEventChoice(FirstEventBranch Branch);
}

[HarmonyPatch]
internal static class IntegratedStrategyMapPointTypeCountsPatch
{
	private const int ExtraUnknownNodes = 2;

	private static IEnumerable<MethodBase> TargetMethods()
	{
		Type[] actTypes =
		[
			typeof(Overgrowth),
			typeof(Underdocks),
			typeof(Hive),
			typeof(Glory),
			typeof(DeprecatedAct)
		];

		foreach (Type actType in actTypes)
		{
			MethodInfo? method = AccessTools.Method(actType, nameof(ActModel.GetMapPointTypes), [typeof(MegaCrit.Sts2.Core.Random.Rng)]);
			if (method != null)
			{
				yield return method;
			}
		}
	}

	private static void Postfix(ref MapPointTypeCounts __result)
	{
		__result = new MapPointTypeCounts(__result.NumOfUnknowns + ExtraUnknownNodes, __result.NumOfRests)
		{
			NumOfElites = __result.NumOfElites,
			PointTypesThatIgnoreRules = [.. __result.PointTypesThatIgnoreRules]
		};
	}
}

[HarmonyPatch(typeof(UnknownMapPointOdds), nameof(UnknownMapPointOdds.Roll))]
internal static class IntegratedStrategyUnknownRoomOddsPatch
{
	private const float VanillaMonsterOdds = 0.10f;
	private const float VanillaTreasureOdds = 0.02f;
	private const float VanillaShopOdds = 0.03f;

	private const float ModdedMonsterOdds = 0.06f;
	private const float ModdedTreasureOdds = 0.01f;
	private const float ModdedShopOdds = 0.02f;

	private static readonly ConditionalWeakTable<UnknownMapPointOdds, Marker> ConfiguredOdds = new();

	private static void Prefix(UnknownMapPointOdds __instance)
	{
		if (ConfiguredOdds.TryGetValue(__instance, out _))
		{
			return;
		}

		__instance.MonsterOdds = RescaleCurrentOdds(__instance.MonsterOdds, VanillaMonsterOdds, ModdedMonsterOdds);
		__instance.TreasureOdds = RescaleCurrentOdds(__instance.TreasureOdds, VanillaTreasureOdds, ModdedTreasureOdds);
		__instance.ShopOdds = RescaleCurrentOdds(__instance.ShopOdds, VanillaShopOdds, ModdedShopOdds);

		__instance.SetBaseOdds(RoomType.Monster, ModdedMonsterOdds);
		__instance.SetBaseOdds(RoomType.Treasure, ModdedTreasureOdds);
		__instance.SetBaseOdds(RoomType.Shop, ModdedShopOdds);

		ConfiguredOdds.Add(__instance, new Marker());
	}

	private static float RescaleCurrentOdds(float currentOdds, float vanillaBaseOdds, float moddedBaseOdds)
	{
		if (currentOdds < 0f)
		{
			return currentOdds;
		}

		return currentOdds * (moddedBaseOdds / vanillaBaseOdds);
	}

	private sealed class Marker;
}
