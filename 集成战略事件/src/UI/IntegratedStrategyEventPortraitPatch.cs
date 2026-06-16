using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Events;

namespace IntegratedStrategyEvents.UI;

[HarmonyPatch(typeof(NEventLayout), nameof(NEventLayout.SetPortrait))]
internal static class IntegratedStrategyEventPortraitPatch
{
	private static void Postfix(NEventLayout __instance)
	{
		if (!IntegratedStrategyEventLayout.IsIntegratedStrategyEvent(__instance))
		{
			return;
		}

		Callable.From(() => IntegratedStrategyEventPortraitFitter.ApplyWithDriver(__instance)).CallDeferred();
	}
}
