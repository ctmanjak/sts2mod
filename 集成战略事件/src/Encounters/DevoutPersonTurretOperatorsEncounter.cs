using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;

namespace IntegratedStrategyEvents.Encounters;

public sealed class DevoutPersonTurretOperatorsEncounter : IntegratedStrategyEliteEncounter
{
	public override IEnumerable<MonsterModel> AllPossibleMonsters => [Monster<TurretOperator>()];

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		return
		[
			(MutableMonster<TurretOperator>(), null),
			(MutableMonster<TurretOperator>(), null)
		];
	}
}
