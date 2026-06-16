using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace IntegratedStrategyEvents.TreeHoles;

[HarmonyPatch(typeof(RunManager), nameof(RunManager.ToSave))]
internal static class IntegratedStrategyTreeHoleSavePatch
{
	private static void Postfix(SerializableRun __result)
	{
		RunState? state = RunManager.Instance.DebugOnlyGetState();
		TreeHoleSaveSnapshot? snapshot = IntegratedStrategyTreeHoleController.GetSaveSnapshot(state);
		if (state == null || snapshot == null)
		{
			IntegratedStrategyTreeHoleSaveStateStore.Clear();
			return;
		}

		int currentActIndex = __result.CurrentActIndex;
		if (currentActIndex >= 0 && currentActIndex < __result.Acts.Count)
		{
			__result.Acts[currentActIndex].SavedMap = SerializableActMap.FromActMap(snapshot.CurrentMap);
		}

		__result.VisitedMapCoords = snapshot.CurrentVisitedMapCoords.ToList();
		__result.MapPointHistory = snapshot.CurrentMapPointHistory
			.Select(static history => history.ToList())
			.ToList();
		__result.MapDrawings = null;
		IntegratedStrategyTreeHoleSaveStateStore.Save(__result, snapshot);

		Log.Info($"{ModInfo.LogPrefix} Saved active tree-hole run at the temporary map location.");
	}
}

[HarmonyPatch(typeof(RunState), nameof(RunState.FromSerializable))]
internal static class IntegratedStrategyTreeHoleLoadPatch
{
	private static void Postfix(SerializableRun save, RunState __result)
	{
		IntegratedStrategyTreeHoleController.QueueRestoreFromSave(save, __result);
	}
}
