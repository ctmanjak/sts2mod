using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace IntegratedStrategyEvents.Events;

internal static partial class IntegratedStrategyEventEffects
{
	public static bool CanLoseHp(Player owner, int amount)
	{
		return owner.Creature.CurrentHp >= amount + 1;
	}

	public static bool CanLoseMaxHp(Player owner, int amount)
	{
		return owner.Creature.MaxHp >= amount + 1;
	}

	public static Task LoseHp(Player owner, int amount)
	{
		return CreatureCmd.Damage(
			new ThrowingPlayerChoiceContext(),
			owner.Creature,
			amount,
			ValueProp.Unblockable | ValueProp.Unpowered,
			null,
			null);
	}

	public static Task LoseMaxHp(Player owner, int amount)
	{
		return CreatureCmd.LoseMaxHp(
			new ThrowingPlayerChoiceContext(),
			owner.Creature,
			amount,
			isFromCard: false);
	}

	public static Task Heal(Player owner, int amount)
	{
		return CreatureCmd.Heal(owner.Creature, amount);
	}

	public static async Task LoseHpAndGainMaxHp(Player owner, int hpLoss, int maxHpGain)
	{
		await CreatureCmd.GainMaxHp(owner.Creature, maxHpGain);
		await LoseHp(owner, hpLoss);
	}

	public static Task GainMaxHp(Player owner, int maxHpGain)
	{
		return CreatureCmd.GainMaxHp(owner.Creature, maxHpGain);
	}

	public static Task SpendGold(Player owner, int amount)
	{
		return PlayerCmd.LoseGold(amount, owner, GoldLossType.Spent);
	}

	public static Task LoseGold(Player owner, int amount)
	{
		return PlayerCmd.LoseGold(amount, owner, GoldLossType.Lost);
	}

	public static Task GainGold(Player owner, int amount)
	{
		return PlayerCmd.GainGold(amount, owner);
	}
}
