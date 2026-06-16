using Godot;
using MegaCrit.Sts2.Core.Models;

namespace IntegratedStrategyEvents.Encounters;

public sealed class BozhokastiSaintguardGunnerBossEncounter : IntegratedStrategyBossEncounter
{
	public const string BossNodePathBase = $"res://{ModInfo.ModId}/images/map/antlered_guard_lancer_boss_icon";

	public override string? CustomScenePath =>
		"res://IntegratedStrategyEvents/scenes/encounters/bozhokasti_saintguard_gunner.tscn";

	public override string BossNodePath => BossNodePathBase;

	public override IEnumerable<string> ExtraAssetPaths =>
	[
		BossNodePathBase + ".png",
		BossNodePathBase + "_outline.png",
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
