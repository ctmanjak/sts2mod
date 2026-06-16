using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Rooms;

namespace IntegratedStrategyEvents.Encounters;

public abstract class IntegratedStrategyTwoSidedEliteEncounter<TMonster> :
	IntegratedStrategyTwoSidedEncounter<TMonster>
	where TMonster : MonsterModel
{
	protected IntegratedStrategyTwoSidedEliteEncounter()
		: base(RoomType.Elite)
	{
	}
}

public abstract class IntegratedStrategyTwoSidedBossEncounter<TMonster> :
	IntegratedStrategyTwoSidedEncounter<TMonster>
	where TMonster : MonsterModel
{
	protected IntegratedStrategyTwoSidedBossEncounter()
		: base(RoomType.Boss)
	{
	}
}

public abstract class IntegratedStrategyTwoSidedEncounter<TMonster> : CustomEncounterModel
	where TMonster : MonsterModel
{
	public const string LeftSlot = "crusher";
	public const string RightSlot = "rocket";

	protected IntegratedStrategyTwoSidedEncounter(RoomType roomType)
		: base(roomType, autoAdd: false)
	{
	}

	public override bool FullyCenterPlayers => true;

	public override IReadOnlyList<string> Slots => [LeftSlot, RightSlot];

	public override string? CustomScenePath => SceneHelper.GetScenePath("encounters/kaiser_crab_boss");

	public override IEnumerable<MonsterModel> AllPossibleMonsters => [Monster<TMonster>()];

	public override bool IsValidForAct(ActModel act)
	{
		return false;
	}

	public override float GetCameraScaling()
	{
		return 0.84f;
	}

	public override Vector2 GetCameraOffset()
	{
		return Vector2.Down * 35f;
	}

	protected static MonsterModel Monster<TSpecificMonster>()
		where TSpecificMonster : MonsterModel
	{
		return ModelDb.Monster<TSpecificMonster>();
	}

	protected static TSpecificMonster MutableMonster<TSpecificMonster>()
		where TSpecificMonster : MonsterModel
	{
		return (TSpecificMonster)ModelDb.Monster<TSpecificMonster>().ToMutable();
	}

	protected virtual MonsterModel CreateLeftMonster()
	{
		return MutableMonster<TMonster>();
	}

	protected virtual MonsterModel CreateRightMonster()
	{
		return MutableMonster<TMonster>();
	}

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		return
		[
			(CreateLeftMonster(), LeftSlot),
			(CreateRightMonster(), RightSlot)
		];
	}
}
