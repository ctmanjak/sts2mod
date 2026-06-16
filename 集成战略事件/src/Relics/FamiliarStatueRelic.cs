using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace IntegratedStrategyEvents.Relics;

public sealed class FamiliarStatueRelic : IntegratedStrategyEventRelic
{
	private const decimal StrengthLoss = 1m;

	private bool _appliedThisCombat;

	public FamiliarStatueRelic()
		: base("familiar_statue.png")
	{
	}

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		HoverTipFactory.FromPower(ModelDb.Power<StrengthPower>())
	];

	public override Task BeforeCombatStart()
	{
		_appliedThisCombat = false;
		return Task.CompletedTask;
	}

	public override Task AfterSideTurnStart(CombatSide side, CombatState combatState)
	{
		Player? owner = Owner;
		if (_appliedThisCombat
				|| owner == null
				|| owner.Creature.IsDead
				|| side != CombatSide.Player
				|| combatState.RoundNumber != 1)
		{
			return Task.CompletedTask;
		}

		IReadOnlyList<Creature> enemies = combatState.Enemies
			.Where(static enemy => enemy.IsAlive)
			.ToList();
		if (enemies.Count == 0)
		{
			return Task.CompletedTask;
		}

		_appliedThisCombat = true;
		Flash();
		return PowerCmd.Apply<StrengthPower>(enemies, -StrengthLoss, owner.Creature, null);
	}
}
