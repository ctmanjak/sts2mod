using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Illaoi;

public sealed class IllaoiHuskPower : IllaoiPowerBase
{
	public bool SkipNextPlayerTurnEndTick { get; set; }

	public override PowerType Type => PowerType.Debuff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override bool OwnerIsSecondaryEnemy => Owner?.Monster is IllaoiSoulMonster;

	public override bool ShouldPlayVfx => false;

	public override Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
	{
		if (side != CombatSide.Player || Owner.IsDead)
		{
			return Task.CompletedTask;
		}

		if (SkipNextPlayerTurnEndTick)
		{
			SkipNextPlayerTurnEndTick = false;
			return Task.CompletedTask;
		}

		return PowerCmd.TickDownDuration(this);
	}
}
