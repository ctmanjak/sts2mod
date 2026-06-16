using HarmonyLib;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Events;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace IntegratedStrategyEvents.TreeHoles;

[HarmonyPatch(typeof(RunManager), nameof(RunManager.EnterNextAct))]
internal static class IntegratedStrategyEndlessFinaleEnterNextActPatch
{
	[HarmonyPriority(Priority.First)]
	[HarmonyBefore("Act4Placeholder")]
	private static bool Prefix(RunManager __instance, ref Task __result)
	{
		return IntegratedStrategyTreeHoleController.HandleEnterNextAct(__instance, ref __result);
	}
}

[HarmonyPatch(typeof(EventModel), "SetEventState")]
internal static class IntegratedStrategyEndlessFinaleArchitectOptionsPatch
{
	[HarmonyPriority(Priority.Last)]
	[HarmonyAfter("Act4Placeholder")]
	private static void Postfix(EventModel __instance)
	{
		IntegratedStrategyTreeHoleController.SuppressArchitectActChangeOptions(__instance);
	}
}

[HarmonyPatch(typeof(NEventLayout), nameof(NEventLayout.AddOptions))]
internal static class IntegratedStrategyEndlessFinaleArchitectOptionDisplayPatch
{
	[HarmonyPriority(Priority.Last)]
	[HarmonyAfter("Act4Placeholder")]
	private static void Prefix(EventModel ____event, ref IEnumerable<EventOption> options)
	{
		options = IntegratedStrategyTreeHoleController.FilterArchitectActChangeOptionsForDisplay(
			____event,
			options);
	}
}

[HarmonyPatch(typeof(NEventRoom), nameof(NEventRoom.OptionButtonClicked))]
internal static class IntegratedStrategyEndlessFinaleArchitectOptionClickPatch
{
	[HarmonyPriority(Priority.First)]
	[HarmonyBefore("Act4Placeholder")]
	private static bool Prefix(EventModel ____event, EventOption option)
	{
		return IntegratedStrategyTreeHoleController.ShouldChooseArchitectOption(____event, option);
	}
}

[HarmonyPatch(typeof(RunManager), "CreateRoom")]
internal static class IntegratedStrategyEndlessFinaleCreateRoomPatch
{
	private static bool Prefix(
		ref RoomType roomType,
		MapPointType mapPointType,
		AbstractModel? model,
		ref AbstractRoom __result)
	{
		return IntegratedStrategyTreeHoleController.HandleCreateRoom(roomType, model, ref __result);
	}

	[HarmonyPriority(Priority.Last)]
	private static void Postfix(
		RoomType roomType,
		AbstractModel? model,
		ref AbstractRoom __result)
	{
		IntegratedStrategyTreeHoleController.EnsureCreatedRoomIsEndlessFinaleBoss(roomType, model, ref __result);
	}
}

[HarmonyPatch(typeof(NBossMapPoint), nameof(NBossMapPoint._Ready))]
internal static class IntegratedStrategyEndlessFinaleBossMapPointPatch
{
	private static void Prefix(NBossMapPoint __instance, out BossNodeRenderSwap? __state)
	{
		__state = IntegratedStrategyTreeHoleController.BeginEndlessFinaleBossNodeRender(__instance.Point);
	}

	private static void Postfix(BossNodeRenderSwap? __state)
	{
		IntegratedStrategyTreeHoleController.EndEndlessFinaleBossNodeRender(__state);
	}
}
