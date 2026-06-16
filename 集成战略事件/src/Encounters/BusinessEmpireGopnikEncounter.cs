using Godot;
using MegaCrit.Sts2.Core.Models;

namespace IntegratedStrategyEvents.Encounters;

public class BusinessEmpireGopnikEncounter : IntegratedStrategyEliteEncounter
{
	public override IEnumerable<MonsterModel> AllPossibleMonsters => [Monster<Gopnik>()];

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		return [(MutableMonster<Gopnik>(), null)];
	}

	public override float GetCameraScaling()
	{
		return 0.95f;
	}

	public override Vector2 GetCameraOffset()
	{
		return Vector2.Down * 35f;
	}
}
