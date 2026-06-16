using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Monsters;

namespace IntegratedStrategyEvents.Encounters;

public sealed class PopularAttractionGremlinMercsEncounterHook :
	IntegratedStrategyEncounterHook<PopularAttractionGremlinMercsEncounter>
{
	private const string TackleMoveId = "TACKLE_MOVE";

	protected override Task BeforeIntegratedStrategyCombatStart(CombatState combatState)
	{
		if (!IntegratedStrategyEncounterSetup.TryFindEnemyBySlot(
			combatState,
			PopularAttractionGremlinMercsEncounter.StarterSneakySlot,
			out Creature starterSneaky))
		{
			return Task.CompletedTask;
		}

		IntegratedStrategyEncounterSetup.ForceMoveStart<SneakyGremlin>(starterSneaky, TackleMoveId);
		return Task.CompletedTask;
	}
}
