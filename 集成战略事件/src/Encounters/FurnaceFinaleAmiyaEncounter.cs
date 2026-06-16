using Godot;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Models;

namespace IntegratedStrategyEvents.Encounters;

public sealed class FurnaceFinaleAmiyaEncounter : IntegratedStrategyBossEncounter
{
	public const string BossNodePathBase = $"res://{ModInfo.ModId}/images/map/the_root_boss_icon";

	public override string? CustomScenePath =>
		"res://IntegratedStrategyEvents/scenes/encounters/furnace_finale_amiya.tscn";

	public override string BossNodePath => BossNodePathBase;

	public override MegaSkeletonDataResource? BossNodeSpineResource => null;

	public override IEnumerable<string> ExtraAssetPaths =>
	[
		BossNodePathBase + ".png",
		BossNodePathBase + "_outline.png"
	];

	public override IEnumerable<MonsterModel> AllPossibleMonsters =>
	[
		Monster<FurnaceFinaleAmiya>(),
		Monster<SarkazCasterLeader>(),
		Monster<SarkazCursebearer>(),
		Monster<RemainingCreativity>()
	];

	public override IReadOnlyList<string> Slots =>
	[
		FurnaceFinaleAmiya.SummonLeftSlot,
		FurnaceFinaleAmiya.SummonRightSlot,
		FurnaceFinaleAmiya.BossSlot
	];

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		return [(MutableMonster<FurnaceFinaleAmiya>(), FurnaceFinaleAmiya.BossSlot)];
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
