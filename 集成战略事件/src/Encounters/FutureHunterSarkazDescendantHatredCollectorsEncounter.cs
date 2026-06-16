using Godot;
using MegaCrit.Sts2.Core.Models;

namespace IntegratedStrategyEvents.Encounters;

public class FutureHunterSarkazDescendantHatredCollectorsEncounter : IntegratedStrategyEliteEncounter
{
	public override IEnumerable<MonsterModel> AllPossibleMonsters => [Monster<SarkazDescendantHatredCollector>()];

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		SarkazDescendantHatredCollector attack = MutableMonster<SarkazDescendantHatredCollector>();
		SarkazDescendantHatredCollector rally = MutableMonster<SarkazDescendantHatredCollector>();
		SarkazDescendantHatredCollector sweep = MutableMonster<SarkazDescendantHatredCollector>();

		attack.InitialMove = SarkazDescendantHatredCollector.OpeningMove.Attack;
		rally.InitialMove = SarkazDescendantHatredCollector.OpeningMove.Rally;
		sweep.InitialMove = SarkazDescendantHatredCollector.OpeningMove.Sweep;

		return
		[
			(attack, null),
			(rally, null),
			(sweep, null)
		];
	}

	public override float GetCameraScaling()
	{
		return 0.92f;
	}

	public override Vector2 GetCameraOffset()
	{
		return Vector2.Down * 30f;
	}
}
