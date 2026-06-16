using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Rooms;

namespace IntegratedStrategyEvents.Encounters;

public abstract class IntegratedStrategyEliteEncounter : CustomEncounterModel
{
	protected IntegratedStrategyEliteEncounter()
		: base(RoomType.Elite, autoAdd: false)
	{
	}

	public override bool IsValidForAct(ActModel act)
	{
		return false;
	}

	protected static MonsterModel Monster<TMonster>()
		where TMonster : MonsterModel
	{
		return ModelDb.Monster<TMonster>();
	}

	protected static TMonster MutableMonster<TMonster>()
		where TMonster : MonsterModel
	{
		return (TMonster)ModelDb.Monster<TMonster>().ToMutable();
	}
}
