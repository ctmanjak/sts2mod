using Godot;
using MegaCrit.Sts2.Core.Models;

namespace IntegratedStrategyEvents.Encounters;

public sealed class ChangeRedmarkEncounter : IntegratedStrategyEliteEncounter
{
	public override IEnumerable<MonsterModel> AllPossibleMonsters =>
	[
		Monster<RedmarkInfiltrator>(),
		Monster<RedmarkEradicator>()
	];

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		return
		[
			(MutableMonster<RedmarkInfiltrator>(), null),
			(MutableMonster<RedmarkEradicator>(), null),
			(MutableMonster<RedmarkEradicator>(), null)
		];
	}

	public override float GetCameraScaling()
	{
		return 0.92f;
	}

	public override Vector2 GetCameraOffset()
	{
		return Vector2.Down * 24f;
	}
}
