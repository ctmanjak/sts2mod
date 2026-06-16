using Godot;
using MegaCrit.Sts2.Core.Models;

namespace IntegratedStrategyEvents.Encounters;

public sealed class CalendarKingsPincerEncounter : IntegratedStrategyTwoSidedEliteEncounter<LugalszargusCalendarKing>
{
	public const string BossNodePathBase = $"res://{ModInfo.ModId}/images/map/two_rivals_boss_icon";

	public override string BossNodePath => BossNodePathBase;

	public override IEnumerable<string> ExtraAssetPaths =>
	[
		BossNodePathBase + ".png",
		BossNodePathBase + "_outline.png"
	];

	public override IEnumerable<MonsterModel> AllPossibleMonsters =>
	[
		Monster<LugalszargusCalendarKing>(),
		Monster<HaranduhEarthwhip>()
	];

	protected override MonsterModel CreateRightMonster()
	{
		return MutableMonster<HaranduhEarthwhip>();
	}

	public override float GetCameraScaling()
	{
		return 0.82f;
	}

	public override Vector2 GetCameraOffset()
	{
		return Vector2.Down * 42f;
	}
}
