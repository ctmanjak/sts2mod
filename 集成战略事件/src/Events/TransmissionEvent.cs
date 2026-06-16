using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models.Cards;

namespace IntegratedStrategyEvents.Events;

public sealed partial class TransmissionEvent : IntegratedStrategyEventModel
{
	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		Player owner = OwnerOrThrow;
		return
		[
			CreateArmorOption(owner),
			CreateFlagOption(owner),
			Choice(Leave, "LEAVE")
		];
	}

	private EventOption CreateArmorOption(Player owner)
	{
		return HasTransformableBasicDeckCard(owner, CardTag.Defend)
			? Choice(TakeArmor, "TAKE_ARMOR")
			: LockedChoice("TAKE_ARMOR_LOCKED");
	}

	private EventOption CreateFlagOption(Player owner)
	{
		return HasTransformableBasicDeckCard(owner, CardTag.Strike)
			? Choice(TakeFlag, "TAKE_FLAG")
			: LockedChoice("TAKE_FLAG_LOCKED");
	}

	private async Task TakeArmor()
	{
		await TransformBasicDeckCard<Finesse>(CardTag.Defend);
		Finish("TAKE_ARMOR");
	}

	private async Task TakeFlag()
	{
		await TransformBasicDeckCard<FlashOfSteel>(CardTag.Strike);
		Finish("TAKE_FLAG");
	}

	private Task Leave()
	{
		Finish("LEAVE");
		return Task.CompletedTask;
	}
}
