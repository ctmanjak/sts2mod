using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Runs;

namespace IntegratedStrategyEvents.Events;

internal static partial class IntegratedStrategyEventSpawnRules
{
	public static ActModel[] GetActs(Type eventType)
	{
		return ActRules.TryGetValue(eventType, out Func<ActModel[]>? createActs)
			? createActs()
			: [];
	}

	public static bool IsAllowed(Type eventType, IRunState runState)
	{
		return !AllowRules.TryGetValue(eventType, out Func<IRunState, bool>? isAllowed)
			|| isAllowed(runState);
	}
}
