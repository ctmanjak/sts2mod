using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;

namespace IntegratedStrategyEvents.Encounters;

public sealed class InviteToPlayLostAndForgottenEncounter : IntegratedStrategyEliteEncounter
{
	public override IEnumerable<MonsterModel> AllPossibleMonsters =>
	[
		Monster<TheLost>(),
		Monster<TheForgotten>()
	];

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		return
		[
			(MutableMonster<TheLost>(), null),
			(MutableMonster<TheForgotten>(), null)
		];
	}
}
