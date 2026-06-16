using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace IntegratedStrategyEvents.Relics;

public sealed class TimeAndLightRelic : IntegratedStrategyEventRelic
{
	private const decimal StrengthAmount = 1m;

	public TimeAndLightRelic()
		: base("time_and_light.png")
	{
	}

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		HoverTipFactory.FromPower(ModelDb.Power<StrengthPower>())
	];

	public override Task BeforeCombatStart()
	{
		Player? owner = Owner;
		if (owner == null || owner.Creature.IsDead)
		{
			return Task.CompletedTask;
		}

		Flash();
		return PowerCmd.Apply<StrengthPower>(owner.Creature, StrengthAmount, owner.Creature, null);
	}
}
