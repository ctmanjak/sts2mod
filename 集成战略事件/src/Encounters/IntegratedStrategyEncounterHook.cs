using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;

namespace IntegratedStrategyEvents.Encounters;

public abstract class IntegratedStrategyEncounterHook<TEncounter> : CustomSingletonModel
	where TEncounter : class
{
	protected IntegratedStrategyEncounterHook()
		: base(HookType.Combat)
	{
	}

	public sealed override Task BeforeCombatStart()
	{
		if (!IntegratedStrategyEncounterSetup.TryGetCombatState<TEncounter>(out CombatState combatState))
		{
			return Task.CompletedTask;
		}

		return BeforeIntegratedStrategyCombatStart(combatState);
	}

	protected abstract Task BeforeIntegratedStrategyCombatStart(CombatState combatState);
}
