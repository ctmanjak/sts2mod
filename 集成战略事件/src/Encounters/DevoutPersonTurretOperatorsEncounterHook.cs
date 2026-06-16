using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Monsters;

namespace IntegratedStrategyEvents.Encounters;

public sealed class DevoutPersonTurretOperatorsEncounterHook :
	IntegratedStrategyEncounterHook<DevoutPersonTurretOperatorsEncounter>
{
	private const string UnloadBeforeReloadMoveId = "UNLOAD_MOVE_2";
	private const string ReloadMoveId = "RELOAD_MOVE";

	protected override Task BeforeIntegratedStrategyCombatStart(CombatState combatState)
	{
		if (!IntegratedStrategyEncounterSetup.TryFindEnemies<TurretOperator>(
			combatState,
			2,
			out Creature[] turretOperators))
		{
			return Task.CompletedTask;
		}

		IntegratedStrategyEncounterSetup.ForceMoveStart<TurretOperator>(turretOperators[0], ReloadMoveId);
		IntegratedStrategyEncounterSetup.ForceMoveStart<TurretOperator>(turretOperators[1], UnloadBeforeReloadMoveId);
		return Task.CompletedTask;
	}
}
