using MegaCrit.Sts2.Core.Combat;

namespace IntegratedStrategyEvents.Encounters;

public sealed class BozhokastiSaintguardGunnerBossEncounterHook :
	IntegratedStrategyEncounterHook<BozhokastiSaintguardGunnerBossEncounter>
{
	protected override Task BeforeIntegratedStrategyCombatStart(CombatState combatState)
	{
		_ = combatState;
		BozhokastiSaintguardGunnerMusicController.Play();
		return Task.CompletedTask;
	}
}

public sealed class BozhokastiSaintguardGunnerTestEncounterHook :
	IntegratedStrategyEncounterHook<BozhokastiSaintguardGunnerTestEncounter>
{
	protected override Task BeforeIntegratedStrategyCombatStart(CombatState combatState)
	{
		_ = combatState;
		BozhokastiSaintguardGunnerMusicController.Play();
		return Task.CompletedTask;
	}
}
