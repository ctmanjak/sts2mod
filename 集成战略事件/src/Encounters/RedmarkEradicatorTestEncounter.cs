using Godot;
using MegaCrit.Sts2.Core.Models;

namespace IntegratedStrategyEvents.Encounters;

public sealed class RedmarkEradicatorTestEncounter : IntegratedStrategyEliteEncounter
{
	public override IEnumerable<MonsterModel> AllPossibleMonsters =>
	[
		Monster<RedmarkEradicator>(),
		Monster<RedmarkInfiltrator>()
	];

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		return
		[
			(MutableMonster<RedmarkInfiltrator>(), null),
			(MutableMonster<RedmarkEradicator>(), null)
		];
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
