using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Illaoi;

public sealed class IllaoiSoulLinkPower : IllaoiPowerBase
{
	private bool _suppressShatterReward;
	[NonSerialized]
	private Creature? _subscribedOwner;
	[NonSerialized]
	private Creature? _subscribedTarget;

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override bool OwnerIsSecondaryEnemy => true;

	public override bool ShouldPlayVfx => false;

	public override Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
	{
		if (target != Owner)
		{
			return Task.CompletedTask;
		}

		EnsureDeathCleanupRegistered();
		// STS2 skips AfterDamageReceived for lethal hits, so transfer from the damage result hook instead.
		return IllaoiMechanics.TransferSoulDamage(choiceContext, this, result, props, dealer, cardSource);
	}

	public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
	{
		if (side != CombatSide.Player || Owner.IsDead)
		{
			return;
		}

		EnsureDeathCleanupRegistered();
		await PowerCmd.TickDownDuration(this);
	}

	public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
	{
		if (creature == Owner)
		{
			IllaoiCombatVisuals.RemoveSoulLink(Owner);
			ClearDeathCleanupHandlers();
			if (!_suppressShatterReward && Target is { IsAlive: true } body)
			{
				_suppressShatterReward = true;
				await IllaoiMechanics.ApplyHuskFromShatteredSoul(choiceContext, body, Applier);
			}
		}

		if (creature == Target && Owner.IsAlive)
		{
			_suppressShatterReward = true;
			IllaoiCombatVisuals.RemoveSoulLink(Owner);
			ClearDeathCleanupHandlers();
			await CreatureCmd.Kill(Owner, force: true);
		}
	}

	public override Task AfterRemoved(Creature oldOwner)
	{
		_suppressShatterReward = true;
		IllaoiCombatVisuals.RemoveSoulLink(oldOwner);
		ClearDeathCleanupHandlers();
		if (oldOwner.IsAlive)
		{
			return CreatureCmd.Kill(oldOwner, force: true);
		}

		return Task.CompletedTask;
	}

	public void SuppressShatterReward()
	{
		_suppressShatterReward = true;
	}

	public void SetBodyTarget(Creature body)
	{
		Target = body;
		EnsureDeathCleanupRegistered();
	}

	public void EnsureDeathCleanupRegistered()
	{
		if (_subscribedOwner != Owner)
		{
			if (_subscribedOwner != null)
			{
				_subscribedOwner.Died -= OnLinkedCreatureDied;
			}

			_subscribedOwner = Owner;
			_subscribedOwner.Died += OnLinkedCreatureDied;
		}

		Creature? target = Target;
		if (_subscribedTarget != target)
		{
			if (_subscribedTarget != null)
			{
				_subscribedTarget.Died -= OnLinkedCreatureDied;
			}

			_subscribedTarget = target;
			if (_subscribedTarget != null)
			{
				_subscribedTarget.Died += OnLinkedCreatureDied;
			}
		}
	}

	private void OnLinkedCreatureDied(Creature creature)
	{
		if (creature == Owner)
		{
			IllaoiCombatVisuals.RemoveSoulLink(Owner);
			ClearDeathCleanupHandlers();
			return;
		}

		if (creature == Target)
		{
			_suppressShatterReward = true;
			IllaoiCombatVisuals.RemoveSoulLinksForBody(creature);
			ClearDeathCleanupHandlers();
		}
	}

	private void ClearDeathCleanupHandlers()
	{
		if (_subscribedOwner != null)
		{
			_subscribedOwner.Died -= OnLinkedCreatureDied;
			_subscribedOwner = null;
		}

		if (_subscribedTarget != null)
		{
			_subscribedTarget.Died -= OnLinkedCreatureDied;
			_subscribedTarget = null;
		}
	}

	public override bool ShouldPowerBeRemovedAfterOwnerDeath()
	{
		return false;
	}

	public override bool ShouldOwnerDeathTriggerFatal()
	{
		return false;
	}
}
