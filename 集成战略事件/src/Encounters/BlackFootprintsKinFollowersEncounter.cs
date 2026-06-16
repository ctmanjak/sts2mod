using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;

namespace IntegratedStrategyEvents.Encounters;

public sealed class BlackFootprintsKinFollowersEncounter : IntegratedStrategyEliteEncounter
{
	public override IEnumerable<MonsterModel> AllPossibleMonsters => [Monster<KinFollower>()];

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		return
		[
			(MutableMonster<KinFollower>(), null),
			(MutableMonster<KinFollower>(), null),
			(MutableMonster<KinFollower>(), null)
		];
	}
}
