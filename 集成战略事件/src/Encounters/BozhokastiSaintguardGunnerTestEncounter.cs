using Godot;
using MegaCrit.Sts2.Core.Models;

namespace IntegratedStrategyEvents.Encounters;

public sealed class BozhokastiSaintguardGunnerTestEncounter : IntegratedStrategyBossEncounter
{
	public override bool IsDebugEncounter => true;

	public override string? CustomScenePath =>
		"res://IntegratedStrategyEvents/scenes/encounters/bozhokasti_saintguard_gunner.tscn";

	public override IEnumerable<string> ExtraAssetPaths =>
	[
		BozhokastiSaintguardGunnerMusicController.TrackPath
	];

	public override IEnumerable<MonsterModel> AllPossibleMonsters =>
	[
		Monster<BozhokastiSaintguardGunner>(),
		Monster<SaintguardAutomaton>()
	];

	public override IReadOnlyList<string> Slots =>
	[
		BozhokastiSaintguardGunner.SummonLeftSlot,
		BozhokastiSaintguardGunner.SummonRightSlot,
		BozhokastiSaintguardGunner.BossSlot
	];

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		return [(MutableMonster<BozhokastiSaintguardGunner>(), BozhokastiSaintguardGunner.BossSlot)];
	}

	public override float GetCameraScaling()
	{
		return 0.88f;
	}

	public override Vector2 GetCameraOffset()
	{
		return Vector2.Down * 45f;
	}
}
