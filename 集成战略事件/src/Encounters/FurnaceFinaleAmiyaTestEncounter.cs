using Godot;
using MegaCrit.Sts2.Core.Models;

namespace IntegratedStrategyEvents.Encounters;

public sealed class FurnaceFinaleAmiyaTestEncounter : IntegratedStrategyBossEncounter
{
	public override bool IsDebugEncounter => true;

	public override string? CustomScenePath =>
		"res://IntegratedStrategyEvents/scenes/encounters/furnace_finale_amiya.tscn";

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
