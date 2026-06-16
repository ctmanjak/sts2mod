using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Runs.History;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace IntegratedStrategyEvents.TreeHoles;

internal static class TreeHoleRunAccessor
{
	private static readonly MethodInfo? RunManagerClearScreensMethod =
		AccessTools.Method(typeof(RunManager), "ClearScreens");
	private static readonly MethodInfo? RunManagerFadeInMethod =
		AccessTools.Method(typeof(RunManager), "FadeIn", [typeof(bool)]);
	private static readonly MethodInfo? RunManagerWinRunMethod =
		AccessTools.Method(typeof(RunManager), "WinRun");
	private static readonly FieldInfo? ActRoomsField =
		AccessTools.Field(typeof(ActModel), "_rooms");
	private static readonly FieldInfo? MapPointHistoryField =
		AccessTools.Field(typeof(RunState), "_mapPointHistory");

	public static void ClearScreens(RunManager runManager)
	{
		RunManagerClearScreensMethod?.Invoke(runManager, null);
	}

	public static async Task FadeIn(RunManager runManager, bool showTransition)
	{
		if (RunManagerFadeInMethod?.Invoke(runManager, [showTransition]) is Task task)
		{
			await task;
		}
	}

	public static bool TryWinRun(RunManager runManager, out Task task)
	{
		if (RunManagerWinRunMethod?.Invoke(runManager, null) is Task winRunTask)
		{
			task = winRunTask;
			return true;
		}

		task = Task.CompletedTask;
		return false;
	}

	public static void RestoreActRooms(RunState state, SerializableActModel originalActSave)
	{
		if (ActRoomsField == null)
		{
			Log.Warn($"{ModInfo.LogPrefix} Could not restore act room set.");
			return;
		}

		ActRoomsField.SetValue(state.Act, RoomSet.FromSave(originalActSave.SerializableRooms));
	}

	public static bool TryGetMapPointHistory(
		RunState state,
		out List<List<MapPointHistoryEntry>> mapPointHistory)
	{
		if (MapPointHistoryField?.GetValue(state) is List<List<MapPointHistoryEntry>> history)
		{
			mapPointHistory = history;
			return true;
		}

		mapPointHistory = [];
		return false;
	}
}
