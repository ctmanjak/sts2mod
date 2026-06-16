using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Monsters;

namespace IntegratedStrategyEvents.Encounters;

public sealed class NorthWindWitchSpinyToadsEncounterHook :
	IntegratedStrategyEncounterHook<NorthWindWitchSpinyToadsEncounter>
{
	private const string TongueLashMoveId = "TONGUE_LASH_MOVE";

	protected override async Task BeforeIntegratedStrategyCombatStart(CombatState combatState)
	{
		if (!IntegratedStrategyEncounterSetup.TryFindEnemyPairBySlots(
			combatState,
			NorthWindWitchSpinyToadsEncounter.LeftSlot,
			NorthWindWitchSpinyToadsEncounter.RightSlot,
			out Creature leftToad,
			out Creature rightToad))
		{
			return;
		}

		IntegratedStrategyEncounterSetup.ForceMoveStart<SpinyToad>(leftToad, TongueLashMoveId);
		await IntegratedStrategyEncounterSetup.ApplyTwoSidedBackAttackPowers(combatState, leftToad, rightToad);
	}
}
