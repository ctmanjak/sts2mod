using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.TestSupport;
using CorePowerCmd = MegaCrit.Sts2.Core.Commands.PowerCmd;

namespace Illaoi;

internal static class PowerCmd
{
	public static Task<IReadOnlyList<T>> Apply<T>(IEnumerable<Creature> targets, decimal amount, Creature? applier, CardModel? cardSource, bool silent = false)
		where T : PowerModel
	{
		return CorePowerCmd.Apply<T>(new BlockingPlayerChoiceContext(), targets, amount, applier, cardSource, silent);
	}

	public static Task<IReadOnlyList<T>> Apply<T>(PlayerChoiceContext choiceContext, IEnumerable<Creature> targets, decimal amount, Creature? applier, CardModel? cardSource, bool silent = false)
		where T : PowerModel
	{
		return CorePowerCmd.Apply<T>(choiceContext, targets, amount, applier, cardSource, silent);
	}

	public static Task<T?> Apply<T>(Creature target, decimal amount, Creature? applier, CardModel? cardSource, bool silent = false)
		where T : PowerModel
	{
		return CorePowerCmd.Apply<T>(new BlockingPlayerChoiceContext(), target, amount, applier, cardSource, silent);
	}

	public static Task<T?> Apply<T>(PlayerChoiceContext choiceContext, Creature target, decimal amount, Creature? applier, CardModel? cardSource, bool silent = false)
		where T : PowerModel
	{
		return CorePowerCmd.Apply<T>(choiceContext, target, amount, applier, cardSource, silent);
	}

	public static Task Apply(PowerModel power, Creature target, decimal amount, Creature? applier, CardModel? cardSource, bool silent = false)
	{
		return CorePowerCmd.Apply(new BlockingPlayerChoiceContext(), power, target, amount, applier, cardSource, silent);
	}

	public static Task Apply(PlayerChoiceContext choiceContext, PowerModel power, Creature target, decimal amount, Creature? applier, CardModel? cardSource, bool silent = false)
	{
		return CorePowerCmd.Apply(choiceContext, power, target, amount, applier, cardSource, silent);
	}

	public static Task<int> ModifyAmount(PowerModel power, decimal offset, Creature? applier, CardModel? cardSource, bool silent = false)
	{
		return CorePowerCmd.ModifyAmount(new BlockingPlayerChoiceContext(), power, offset, applier, cardSource, silent);
	}

	public static Task<int> ModifyAmount(PlayerChoiceContext choiceContext, PowerModel power, decimal offset, Creature? applier, CardModel? cardSource, bool silent = false)
	{
		return CorePowerCmd.ModifyAmount(choiceContext, power, offset, applier, cardSource, silent);
	}

	public static Task Decrement(PowerModel power)
	{
		return CorePowerCmd.Decrement(power);
	}

	public static Task TickDownDuration(PowerModel power)
	{
		return CorePowerCmd.TickDownDuration(power);
	}

	public static Task Remove<T>(Creature creature) where T : PowerModel
	{
		return CorePowerCmd.Remove<T>(creature);
	}

	public static Task Remove(PowerModel? power)
	{
		return CorePowerCmd.Remove(power);
	}
}
