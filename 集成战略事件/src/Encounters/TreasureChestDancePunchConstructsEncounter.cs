using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;

namespace IntegratedStrategyEvents.Encounters;

public sealed class TreasureChestDancePunchConstructsEncounter :
	IntegratedStrategyTwoSidedEliteEncounter<PunchConstruct>
{
	protected override MonsterModel CreateLeftMonster()
	{
		PunchConstruct leftConstruct = MutableMonster<PunchConstruct>();
		return leftConstruct;
	}
}
