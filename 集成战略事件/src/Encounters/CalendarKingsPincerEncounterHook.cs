using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace IntegratedStrategyEvents.Encounters;

public sealed class CalendarKingsPincerEncounterHook :
	IntegratedStrategyEncounterHook<CalendarKingsPincerEncounter>
{
	protected override async Task BeforeIntegratedStrategyCombatStart(CombatState combatState)
	{
		await CalendarKingsPincerEncounterSetup.ApplyToCombat(combatState);
	}
}

public sealed class CalendarKingsPincerBossEncounterHook :
	IntegratedStrategyEncounterHook<CalendarKingsPincerBossEncounter>
{
	protected override async Task BeforeIntegratedStrategyCombatStart(CombatState combatState)
	{
		await CalendarKingsPincerEncounterSetup.ApplyToCombat(combatState);
	}
}

internal static class CalendarKingsPincerEncounterSetup
{
	public static async Task ApplyToCombat(CombatState combatState)
	{
		CalendarKingsPincerMusicController.Play();
		if (!IntegratedStrategyEncounterSetup.TryFindEnemyPairBySlots(
			combatState,
			CalendarKingsPincerEncounter.LeftSlot,
			CalendarKingsPincerEncounter.RightSlot,
			out Creature leftBoss,
			out Creature rightBoss))
		{
			return;
		}

		await IntegratedStrategyEncounterSetup.ApplyTwoSidedBackAttackPowers(combatState, leftBoss, rightBoss);
		IntegratedStrategyEncounterSetup.FaceCreatureBodyRight(leftBoss);
	}
}
