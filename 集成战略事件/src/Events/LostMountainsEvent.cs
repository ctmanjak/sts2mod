using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Models.Relics;

namespace IntegratedStrategyEvents.Events;

public sealed partial class LostMountainsEvent : IntegratedStrategyEventModel
{
	private const int LeftCardMaxHpGain = 15;
	private const int RightCardMaxHpLoss = 6;

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		Player owner = OwnerOrThrow;
		return
		[
			CreateLeftCardOption(owner),
			CreateRightCardOption(owner),
			Choice(Decline, "DECLINE")
		];
	}

	private EventOption CreateLeftCardOption(Player owner)
	{
		return RelicChoice<RoyalPoison>(owner, ChooseLeftCard, "LEFT_CARD");
	}

	private EventOption CreateRightCardOption(Player owner)
	{
		if (!CanLoseMaxHp(owner, RightCardMaxHpLoss))
		{
			return LockedChoice("RIGHT_CARD_LOCKED");
		}

		return RelicChoice<BloodVial>(owner, ChooseRightCard, "RIGHT_CARD");
	}

	private async Task ChooseLeftCard()
	{
		await GainMaxHp(LeftCardMaxHpGain);
		await ObtainRelic<RoyalPoison>();
		Finish("LEFT_CARD");
	}

	private async Task ChooseRightCard()
	{
		await LoseMaxHp(RightCardMaxHpLoss);
		await ObtainRelic<BloodVial>();
		Finish("RIGHT_CARD");
	}

	private Task Decline()
	{
		Finish("DECLINE");
		return Task.CompletedTask;
	}
}
