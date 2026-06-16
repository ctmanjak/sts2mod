using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace IntegratedStrategyEvents.Encounters;

public sealed class TreasureChestDanceSarkazCursebearersEncounterHook :
	IntegratedStrategyEncounterHook<TreasureChestDanceSarkazCursebearersEncounter>
{
	protected override async Task BeforeIntegratedStrategyCombatStart(CombatState combatState)
	{
		if (!IntegratedStrategyEncounterSetup.TryFindEnemyPairBySlots(
			combatState,
			TreasureChestDanceSarkazCursebearersEncounter.LeftSlot,
			TreasureChestDanceSarkazCursebearersEncounter.RightSlot,
			out Creature leftCursebearer,
			out Creature rightCursebearer))
		{
			return;
		}

		await IntegratedStrategyEncounterSetup.ApplyTwoSidedBackAttackPowers(
			combatState,
			leftCursebearer,
			rightCursebearer);
		IntegratedStrategyEncounterSetup.FaceCreatureBodyRight(leftCursebearer);
	}
}
