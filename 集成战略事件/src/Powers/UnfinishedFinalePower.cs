using BaseLib.Abstracts;
using IntegratedStrategyEvents.Encounters;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace IntegratedStrategyEvents.Powers;

public sealed class UnfinishedFinalePower : PowerModel, ICustomPower
{
	private const string CustomPowerIconPath = "res://IntegratedStrategyEvents/images/powers/unfinished_finale.png";

	private bool _isReviving;

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

	public string? CustomPackedIconPath => CustomPowerIconPath;

	public string? CustomBigIconPath => CustomPowerIconPath;

	public void DoRevive()
	{
		AssertMutable();
		_isReviving = false;
	}

	public override async Task AfterDeath(
		PlayerChoiceContext choiceContext,
		Creature creature,
		bool wasRemovalPrevented,
		float deathAnimLength)
	{
		_ = choiceContext;
		_ = deathAnimLength;
		if (wasRemovalPrevented || creature != Owner || creature.Monster is not FurnaceFinaleAmiya amiya || amiya.HasRevived)
		{
			return;
		}

		_isReviving = true;
		Flash();
		await amiya.TriggerReviveWaitingState();
	}

	public override bool ShouldAllowHitting(Creature creature)
	{
		if (creature != Owner)
		{
			return true;
		}

		return !_isReviving;
	}

	public override bool ShouldStopCombatFromEnding()
	{
		return Owner?.Monster is FurnaceFinaleAmiya amiya && (_isReviving || !amiya.HasRevived);
	}

	public override bool ShouldCreatureBeRemovedFromCombatAfterDeath(Creature creature)
	{
		if (creature != Owner || creature.Monster is not FurnaceFinaleAmiya amiya)
		{
			return true;
		}

		return amiya.HasRevived;
	}

	public override bool ShouldPowerBeRemovedAfterOwnerDeath()
	{
		return Owner?.Monster is not FurnaceFinaleAmiya amiya || amiya.HasRevived;
	}
}
