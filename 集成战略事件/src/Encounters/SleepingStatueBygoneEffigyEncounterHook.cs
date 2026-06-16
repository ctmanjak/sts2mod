using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Monsters;

namespace IntegratedStrategyEvents.Encounters;

public sealed class SleepingStatueBygoneEffigyEncounterHook :
	IntegratedStrategyEncounterHook<SleepingStatueBygoneEffigyEncounter>
{
	private const string WakeMoveId = "WAKE_MOVE";

	protected override Task BeforeIntegratedStrategyCombatStart(CombatState combatState)
	{
		if (IntegratedStrategyEncounterSetup.TryFindEnemies<BygoneEffigy>(
			combatState,
			1,
			out Creature[] effigies))
		{
			IntegratedStrategyEncounterSetup.ForceMoveStart<BygoneEffigy>(effigies[0], WakeMoveId);
		}

		return Task.CompletedTask;
	}
}
