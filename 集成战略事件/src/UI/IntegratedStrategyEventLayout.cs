using HarmonyLib;
using IntegratedStrategyEvents.Events;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Events;

namespace IntegratedStrategyEvents.UI;

internal static class IntegratedStrategyEventLayout
{
	private static readonly AccessTools.FieldRef<NEventLayout, EventModel?> EventRef =
		AccessTools.FieldRefAccess<NEventLayout, EventModel?>("_event");

	public static bool IsIntegratedStrategyEvent(NEventLayout layout)
	{
		return GetIntegratedStrategyEvent(layout) != null;
	}

	public static IntegratedStrategyEventModel? GetIntegratedStrategyEvent(NEventLayout layout)
	{
		return EventRef(layout) as IntegratedStrategyEventModel;
	}
}
