using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;

namespace IntegratedStrategyEvents.Encounters;

public sealed class HundredMileEncampmentCultistsEncounter : IntegratedStrategyEliteEncounter
{
	public override IEnumerable<MonsterModel> AllPossibleMonsters =>
	[
		Monster<CalcifiedCultist>(),
		Monster<DampCultist>(),
		Monster<DevotedSculptor>()
	];

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		return
		[
			(MutableMonster<CalcifiedCultist>(), null),
			(MutableMonster<DampCultist>(), null),
			(MutableMonster<DevotedSculptor>(), null)
		];
	}
}
