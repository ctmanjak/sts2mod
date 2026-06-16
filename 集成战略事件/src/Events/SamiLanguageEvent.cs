using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;

namespace IntegratedStrategyEvents.Events;

public sealed partial class SamiLanguageEvent : IntegratedStrategyEventModel
{
	private const int CardsToRandomRemove = 2;
	private const int TransformCardCount = 2;
	private const int MaxHpLoss = 8;

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		Player owner = OwnerOrThrow;
		return
		[
			CreatePeelOffOption(owner),
			CreateDeclareOption(owner),
			Choice(Leave, "LEAVE")
		];
	}

	private EventOption CreatePeelOffOption(Player owner)
	{
		return HasRemovableDeckCards(CardsToRandomRemove)
			? Choice(PeelOff, "PEEL_OFF")
			: LockedChoice("PEEL_OFF_LOCKED");
	}

	private EventOption CreateDeclareOption(Player owner)
	{
		return CanDeclare(owner)
			? Choice(DeclareIt, "DECLARE")
			: LockedChoice("DECLARE_LOCKED");
	}

	private async Task PeelOff()
	{
		await RemoveRandomDeckCards(CardsToRandomRemove);
		Finish("PEEL_OFF");
	}

	private async Task DeclareIt()
	{
		await LoseMaxHp(MaxHpLoss);
		await TransformDeckCards(TransformCardCount);
		Finish("DECLARE");
	}

	private Task Leave()
	{
		Finish("LEAVE");
		return Task.CompletedTask;
	}

	private static bool CanDeclare(Player owner)
	{
		return CanLoseMaxHp(owner, MaxHpLoss)
			&& HasTransformableDeckCards(owner, TransformCardCount);
	}
}
