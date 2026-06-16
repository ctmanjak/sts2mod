using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace IntegratedStrategyEvents.Encounters;

public sealed class NorthWindWitchScavengerApostlesEncounterHook :
	IntegratedStrategyEncounterHook<NorthWindWitchScavengerApostlesEncounter>
{
	protected override async Task BeforeIntegratedStrategyCombatStart(CombatState combatState)
	{
		if (!IntegratedStrategyEncounterSetup.TryFindEnemyPairBySlots(
			combatState,
			NorthWindWitchScavengerApostlesEncounter.LeftSlot,
			NorthWindWitchScavengerApostlesEncounter.RightSlot,
			out Creature leftApostle,
			out Creature rightApostle))
		{
			return;
		}

		IntegratedStrategyEncounterSetup.ForceMoveStart<ScavengerApostle>(
			leftApostle,
			ScavengerApostle.PollutionMoveId);
		IntegratedStrategyEncounterSetup.ForceMoveStart<ScavengerApostle>(
			rightApostle,
			ScavengerApostle.ErosionMoveId);
		await IntegratedStrategyEncounterSetup.ApplyTwoSidedBackAttackPowers(combatState, leftApostle, rightApostle);
		IntegratedStrategyEncounterSetup.FaceCreatureBodyRight(leftApostle);
	}
}
