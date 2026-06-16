using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Rewards;

namespace IntegratedStrategyEvents.Events;

public abstract partial class IntegratedStrategyEventModel
{
	protected EventOption Choice(Func<Task> onChosen, string optionKey, string pageKey = InitialPage)
	{
		return new EventOption(this, onChosen, $"{Id.Entry}.pages.{pageKey}.options.{optionKey}");
	}

	protected EventOption LockedChoice(string optionKey, string pageKey = InitialPage)
	{
		return LockedOption(optionKey, pageKey);
	}

	protected EventOption GoldChoice(
		Player owner,
		int cost,
		Func<Task> onChosen,
		string optionKey,
		string lockedOptionKey,
		string pageKey = InitialPage)
	{
		return owner.Gold >= cost
			? Choice(onChosen, optionKey, pageKey)
			: LockedChoice(lockedOptionKey, pageKey);
	}

	protected EventOption HpChoice(
		Player owner,
		int hpLoss,
		Func<Task> onChosen,
		string optionKey,
		string lockedOptionKey,
		string pageKey = InitialPage)
	{
		return CanLoseHp(owner, hpLoss)
			? Choice(onChosen, optionKey, pageKey).ThatDoesDamage(hpLoss)
			: LockedChoice(lockedOptionKey, pageKey);
	}

	protected void ShowPage(string pageKey, IReadOnlyList<EventOption> options)
	{
		SetEventState(PageDescription(pageKey), options);
	}

	protected EventOption FightChoice(Func<Task> startFight, string pageKey)
	{
		return Choice(startFight, "FIGHT", pageKey);
	}

	protected EventOption FightChoice<TEncounter>(string pageKey)
		where TEncounter : CustomEncounterModel
	{
		return FightChoice(EnterEventCombat<TEncounter>, pageKey);
	}

	protected void ShowFightPage(string pageKey, Func<Task> startFight)
	{
		ShowPage(pageKey, [FightChoice(startFight, pageKey)]);
	}

	protected void ShowFightPage<TEncounter>(string pageKey)
		where TEncounter : CustomEncounterModel
	{
		ShowFightPage(pageKey, EnterEventCombat<TEncounter>);
	}

	protected Task EnterEventCombat<TEncounter>()
		where TEncounter : CustomEncounterModel
	{
		return EnterEventCombat<TEncounter>(Array.Empty<Reward>());
	}

	protected Task EnterEventCombat<TEncounter>(IReadOnlyList<Reward> rewards)
		where TEncounter : CustomEncounterModel
	{
		EnterCombatWithoutExitingEvent<TEncounter>(
			rewards,
			shouldResumeAfterCombat: false);
		return Task.CompletedTask;
	}

	protected void Finish(string pageKey)
	{
		SetEventFinished(PageDescription(pageKey));
	}
}
