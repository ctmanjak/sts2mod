using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Powers;

namespace IntegratedStrategyEvents.Encounters;

public sealed class BlackFootprintsKinFollowersEncounterHook :
	IntegratedStrategyEncounterHook<BlackFootprintsKinFollowersEncounter>
{
	private const string BoomerangMoveId = "BOOMERANG_MOVE";
	private const string PowerDanceMoveId = "POWER_DANCE_MOVE";

	protected override async Task BeforeIntegratedStrategyCombatStart(CombatState combatState)
	{
		if (!IntegratedStrategyEncounterSetup.TryFindEnemies<KinFollower>(combatState, 3, out Creature[] kinFollowers))
		{
			return;
		}

		foreach (Creature kinFollower in kinFollowers)
		{
			await PowerCmd.Remove<MinionPower>(kinFollower);
		}

		IntegratedStrategyEncounterSetup.ForceMoveStart<KinFollower>(kinFollowers[0], BoomerangMoveId);
		IntegratedStrategyEncounterSetup.ForceMoveStart<KinFollower>(kinFollowers[1], PowerDanceMoveId);
	}
}
