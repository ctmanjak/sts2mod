using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace IntegratedStrategyEvents.Encounters;

public sealed class TreasureChestDancePunchConstructsEncounterHook :
	IntegratedStrategyEncounterHook<TreasureChestDancePunchConstructsEncounter>
{
	protected override async Task BeforeIntegratedStrategyCombatStart(CombatState combatState)
	{
		if (!IntegratedStrategyEncounterSetup.TryFindEnemyPairBySlots(
			combatState,
			TreasureChestDancePunchConstructsEncounter.LeftSlot,
			TreasureChestDancePunchConstructsEncounter.RightSlot,
			out Creature leftConstruct,
			out Creature rightConstruct))
		{
			return;
		}

		await IntegratedStrategyEncounterSetup.ApplyTwoSidedBackAttackPowers(combatState, leftConstruct, rightConstruct);
	}
}
