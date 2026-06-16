using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace IntegratedStrategyEvents.Relics;

public sealed class OldGaulishPlaceNamesRelic : IntegratedStrategyEventRelic
{
	private const decimal RegenAmount = 3m;

	public OldGaulishPlaceNamesRelic()
		: base("old_gaulish_place_names.png")
	{
	}

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		HoverTipFactory.FromPower(ModelDb.Power<RegenPower>())
	];

	public override Task BeforeCombatStart()
	{
		Player? owner = Owner;
		if (owner == null || owner.Creature.IsDead)
		{
			return Task.CompletedTask;
		}

		Flash();
		return PowerCmd.Apply<RegenPower>(owner.Creature, RegenAmount, owner.Creature, null);
	}
}
