using Godot;
using MegaCrit.Sts2.Core.Models;

namespace IntegratedStrategyEvents.Encounters;

public sealed class CalendarKingsPincerBossEncounter :
	IntegratedStrategyTwoSidedBossEncounter<LugalszargusCalendarKing>
{
	public override string BossNodePath => CalendarKingsPincerEncounter.BossNodePathBase;

	public override IEnumerable<string> ExtraAssetPaths =>
	[
		CalendarKingsPincerEncounter.BossNodePathBase + ".png",
		CalendarKingsPincerEncounter.BossNodePathBase + "_outline.png"
	];

	public override IEnumerable<MonsterModel> AllPossibleMonsters =>
	[
		Monster<LugalszargusCalendarKing>(),
		Monster<HaranduhEarthwhip>()
	];

	public override float GetCameraScaling()
	{
		return 0.82f;
	}

	public override Vector2 GetCameraOffset()
	{
		return Vector2.Down * 42f;
	}

	protected override MonsterModel CreateRightMonster()
	{
		return MutableMonster<HaranduhEarthwhip>();
	}
}
