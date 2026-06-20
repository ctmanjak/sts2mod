using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Illaoi;

public abstract class IllaoiPowerBase : PowerModel
{
	public virtual Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, ICombatState combatState)
	{
		return Task.CompletedTask;
	}

	public sealed override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		return BeforeSideTurnStart(choiceContext, side, combatState);
	}

	public virtual Task AfterSideTurnStart(CombatSide side, ICombatState combatState)
	{
		return Task.CompletedTask;
	}

	public sealed override Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		return AfterSideTurnStart(side, combatState);
	}

	public virtual Task BeforeTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
	{
		return Task.CompletedTask;
	}

	public sealed override Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		return BeforeTurnEnd(choiceContext, side);
	}

	public virtual Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
	{
		return Task.CompletedTask;
	}

	public sealed override Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		return AfterTurnEnd(choiceContext, side);
	}

	public virtual Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
	{
		return Task.CompletedTask;
	}

	public sealed override Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
	{
		return AfterPowerAmountChanged(power, amount, applier, cardSource);
	}
}

public abstract class IllaoiRelicBase : RelicModel
{
	public virtual Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, ICombatState combatState)
	{
		return Task.CompletedTask;
	}

	public sealed override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		return BeforeSideTurnStart(choiceContext, side, combatState);
	}

	public virtual Task AfterSideTurnStart(CombatSide side, ICombatState combatState)
	{
		return Task.CompletedTask;
	}

	public sealed override Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		return AfterSideTurnStart(side, combatState);
	}

	public virtual Task BeforeTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
	{
		return Task.CompletedTask;
	}

	public sealed override Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		return BeforeTurnEnd(choiceContext, side);
	}

	public virtual Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
	{
		return Task.CompletedTask;
	}

	public sealed override Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		return AfterTurnEnd(choiceContext, side);
	}
}
