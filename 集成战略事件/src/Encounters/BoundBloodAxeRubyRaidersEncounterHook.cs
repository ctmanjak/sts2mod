using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace IntegratedStrategyEvents.Encounters;

public sealed class BoundBloodAxeRubyRaidersEncounterHook :
	IntegratedStrategyEncounterHook<BoundBloodAxeRubyRaidersEncounter>
{
	protected override async Task BeforeIntegratedStrategyCombatStart(CombatState combatState)
	{
		if (!IntegratedStrategyEncounterSetup.TryFindEnemyPairBySlots(
			combatState,
			BoundBloodAxeRubyRaidersEncounter.LeftSlot,
			BoundBloodAxeRubyRaidersEncounter.RightSlot,
			out Creature leftRaider,
			out Creature rightRaider))
		{
			return;
		}

		await IntegratedStrategyEncounterSetup.ApplyTwoSidedBackAttackPowers(combatState, leftRaider, rightRaider);
	}
}
