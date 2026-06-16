using MegaCrit.Sts2.Core.Entities.Players;

namespace IntegratedStrategyEvents.Events;

public abstract partial class IntegratedStrategyEventModel
{
	protected bool CanLoseHp(int amount)
	{
		return CanLoseHp(OwnerOrThrow, amount);
	}

	protected static bool CanLoseHp(Player owner, int amount)
	{
		return IntegratedStrategyEventEffects.CanLoseHp(owner, amount);
	}

	protected bool CanLoseMaxHp(int amount)
	{
		return CanLoseMaxHp(OwnerOrThrow, amount);
	}

	protected static bool CanLoseMaxHp(Player owner, int amount)
	{
		return IntegratedStrategyEventEffects.CanLoseMaxHp(owner, amount);
	}

	protected Task LoseHp(int amount)
	{
		return IntegratedStrategyEventEffects.LoseHp(OwnerOrThrow, amount);
	}

	protected Task LoseMaxHp(int amount)
	{
		return IntegratedStrategyEventEffects.LoseMaxHp(OwnerOrThrow, amount);
	}

	protected Task Heal(int amount)
	{
		return IntegratedStrategyEventEffects.Heal(OwnerOrThrow, amount);
	}

	protected static Task LoseHp(Player owner, int amount)
	{
		return IntegratedStrategyEventEffects.LoseHp(owner, amount);
	}

	protected Task LoseHpAndGainMaxHp(int hpLoss, int maxHpGain)
	{
		return IntegratedStrategyEventEffects.LoseHpAndGainMaxHp(OwnerOrThrow, hpLoss, maxHpGain);
	}

	protected Task GainMaxHp(int maxHpGain)
	{
		return IntegratedStrategyEventEffects.GainMaxHp(OwnerOrThrow, maxHpGain);
	}

	protected Task SpendGold(int amount)
	{
		return IntegratedStrategyEventEffects.SpendGold(OwnerOrThrow, amount);
	}

	protected Task LoseGold(int amount)
	{
		return IntegratedStrategyEventEffects.LoseGold(OwnerOrThrow, amount);
	}

	protected Task GainGold(int amount)
	{
		return IntegratedStrategyEventEffects.GainGold(OwnerOrThrow, amount);
	}
}
