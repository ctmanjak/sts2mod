using BaseLib.Abstracts;
using IntegratedStrategyEvents.Encounters;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace IntegratedStrategyEvents.Powers;

public sealed class SaintguardShieldPower : PowerModel, ICustomPower
{
	private const string RampartPowerPackedIconPath =
		"res://images/atlases/power_atlas.sprites/rampart_power.tres";
	private const string RampartPowerBigIconPath = "res://images/powers/rampart_power.png";

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public string? CustomPackedIconPath => RampartPowerPackedIconPath;

	public string? CustomBigIconPath => RampartPowerBigIconPath;

	public override decimal ModifyDamageAdditive(
		Creature? target,
		decimal amount,
		ValueProp props,
		Creature? dealer,
		CardModel? cardSource)
	{
		_ = dealer;
		_ = cardSource;
		if (target != Owner || amount <= 0m || Amount <= 0 || props.HasFlag(ValueProp.Unpowered))
		{
			return 0m;
		}

		return -Math.Min(amount, Amount);
	}

	public override bool ShouldPowerBeRemovedAfterOwnerDeath()
	{
		return Owner?.Monster is not BozhokastiSaintguardGunner boss || boss.HasRevived;
	}
}
