using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace IntegratedStrategyEvents.Relics;

public sealed class NumbnessAndVulgarityRelic : IntegratedStrategyEventRelic
{
	private const decimal ArtifactAmount = 1m;

	public NumbnessAndVulgarityRelic()
		: base("numbness_and_vulgarity.png")
	{
	}

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		HoverTipFactory.FromPower(ModelDb.Power<ArtifactPower>())
	];

	public override Task BeforeCombatStart()
	{
		Player? owner = Owner;
		if (owner == null || owner.Creature.IsDead)
		{
			return Task.CompletedTask;
		}

		Flash();
		return PowerCmd.Apply<ArtifactPower>(owner.Creature, ArtifactAmount, owner.Creature, null);
	}
}
