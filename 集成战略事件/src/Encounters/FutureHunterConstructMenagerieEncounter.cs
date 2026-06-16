using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;

namespace IntegratedStrategyEvents.Encounters;

public sealed class FutureHunterConstructMenagerieEncounter : IntegratedStrategyEliteEncounter
{
	public override IEnumerable<MonsterModel> AllPossibleMonsters =>
	[
		Monster<PunchConstruct>(),
		Monster<CubexConstruct>()
	];

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		return
		[
			(MutableMonster<PunchConstruct>(), null),
			(MutableMonster<CubexConstruct>(), null),
			(MutableMonster<CubexConstruct>(), null)
		];
	}
}
