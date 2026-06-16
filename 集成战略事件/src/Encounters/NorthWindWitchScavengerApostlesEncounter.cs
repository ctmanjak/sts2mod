using MegaCrit.Sts2.Core.Models;

namespace IntegratedStrategyEvents.Encounters;

public class NorthWindWitchScavengerApostlesEncounter : IntegratedStrategyTwoSidedEliteEncounter<ScavengerApostle>
{
	protected override MonsterModel CreateLeftMonster()
	{
		ScavengerApostle pollution = MutableMonster<ScavengerApostle>();
		pollution.InitialMove = ScavengerApostle.OpeningMove.Pollution;
		return pollution;
	}

	protected override MonsterModel CreateRightMonster()
	{
		ScavengerApostle erosion = MutableMonster<ScavengerApostle>();
		erosion.InitialMove = ScavengerApostle.OpeningMove.Erosion;
		return erosion;
	}
}
