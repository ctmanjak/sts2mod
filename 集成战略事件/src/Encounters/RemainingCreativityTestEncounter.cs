using Godot;
using MegaCrit.Sts2.Core.Models;

namespace IntegratedStrategyEvents.Encounters;

public sealed class RemainingCreativityTestEncounter : IntegratedStrategyEliteEncounter
{
	public override IEnumerable<MonsterModel> AllPossibleMonsters => [Monster<RemainingCreativity>()];

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		RemainingCreativity creativity = MutableMonster<RemainingCreativity>();
		creativity.AppliesMinionPower = false;
		return [(creativity, null)];
	}

	public override float GetCameraScaling()
	{
		return 0.95f;
	}

	public override Vector2 GetCameraOffset()
	{
		return Vector2.Down * 20f;
	}
}
