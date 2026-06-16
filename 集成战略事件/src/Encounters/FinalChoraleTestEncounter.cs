using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;

namespace IntegratedStrategyEvents.Encounters;

public sealed class FinalChoraleTestEncounter : IntegratedStrategyEliteEncounter
{
	public override IEnumerable<MonsterModel> AllPossibleMonsters => [Monster<FinalChorale>()];

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		return [(MutableMonster<FinalChorale>(), null)];
	}

	public override float GetCameraScaling()
	{
		return 0.92f;
	}

	public override Vector2 GetCameraOffset()
	{
		return Vector2.Down * 35f;
	}
}
