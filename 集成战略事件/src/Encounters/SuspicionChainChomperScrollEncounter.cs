using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;

namespace IntegratedStrategyEvents.Encounters;

public sealed class SuspicionChainChomperScrollEncounter : IntegratedStrategyEliteEncounter
{
	public override IEnumerable<MonsterModel> AllPossibleMonsters =>
	[
		Monster<Chomper>(),
		Monster<ScrollOfBiting>()
	];

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		Chomper chomper = MutableMonster<Chomper>();
		chomper.ScreamFirst = false;

		ScrollOfBiting scroll = MutableMonster<ScrollOfBiting>();
		scroll.StarterMoveIdx = 2;

		return
		[
			(chomper, null),
			(scroll, null)
		];
	}
}
