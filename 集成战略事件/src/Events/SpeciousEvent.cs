using IntegratedStrategyEvents.Relics;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;

namespace IntegratedStrategyEvents.Events;

public sealed partial class SpeciousEvent : IntegratedStrategyEventModel
{
	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		Player owner = OwnerOrThrow;
		return
		[
			CreateLearnStoryOption(owner),
			CreateRequestAidOption(owner),
			Choice(Leave, "LEAVE")
		];
	}

	private EventOption CreateLearnStoryOption(Player owner)
	{
		return RelicChoice<RhodesDoorRelic>(owner, LearnStories, "LEARN_STORIES");
	}

	private EventOption CreateRequestAidOption(Player owner)
	{
		return HasUpgradableDeckCards(owner)
			? Choice(RequestAid, "REQUEST_AID")
			: LockedChoice("REQUEST_AID_LOCKED");
	}

	private async Task LearnStories()
	{
		await ObtainRelic<RhodesDoorRelic>();
		Finish("LEARN_STORIES");
	}

	private async Task RequestAid()
	{
		await UpgradeDeckCards(1);
		Finish("REQUEST_AID");
	}

	private Task Leave()
	{
		Finish("LEAVE");
		return Task.CompletedTask;
	}
}
