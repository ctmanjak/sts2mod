using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Models.Relics;

namespace IntegratedStrategyEvents.Events;

public sealed partial class KindlingSparkEvent : IntegratedStrategyEventModel
{
	private const int HpLoss = 12;
	private const int GoldReward = 60;

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		Player owner = OwnerOrThrow;
		return
		[
			CreateTakeLampOption(owner),
			Choice(TakeSupplies, "TAKE_SUPPLIES")
		];
	}

	private EventOption CreateTakeLampOption(Player owner)
	{
		if (!CanLoseHp(owner, HpLoss))
		{
			return LockedChoice("TAKE_LAMP_LOCKED");
		}

		return RelicChoice<LavaLamp>(owner, TakeLamp, "TAKE_LAMP")
			.ThatDoesDamage(HpLoss);
	}

	private async Task TakeLamp()
	{
		await LoseHp(HpLoss);
		await ObtainRelic<LavaLamp>();
		Finish("TAKE_LAMP");
	}

	private async Task TakeSupplies()
	{
		await GainGold(GoldReward);
		Finish("TAKE_SUPPLIES");
	}
}
