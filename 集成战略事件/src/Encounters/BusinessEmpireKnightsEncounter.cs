using Godot;
using MegaCrit.Sts2.Core.Entities.Encounters;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Models.Monsters;

namespace IntegratedStrategyEvents.Encounters;

public sealed class BusinessEmpireKnightsEncounter : IntegratedStrategyEliteEncounter
{
	public override IEnumerable<EncounterTag> Tags => [EncounterTag.Knights];

	public override string? CustomScenePath => SceneHelper.GetScenePath("encounters/knights_elite");

	public override IEnumerable<string> ExtraAssetPaths => [ModelDb.Affliction<Hexed>().OverlayPath];

	public override IEnumerable<MonsterModel> AllPossibleMonsters =>
	[
		Monster<FlailKnight>(),
		Monster<SpectralKnight>(),
		Monster<MagiKnight>()
	];

	public override float GetCameraScaling()
	{
		return 0.87f;
	}

	public override Vector2 GetCameraOffset()
	{
		return Vector2.Down * 50f;
	}

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		return
		[
			(MutableMonster<FlailKnight>(), "first"),
			(MutableMonster<SpectralKnight>(), "second"),
			(MutableMonster<MagiKnight>(), "third")
		];
	}
}
