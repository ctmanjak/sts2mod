using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace IntegratedStrategyEvents.Events;

public abstract partial class IntegratedStrategyEventModel
{
	protected EventOption RelicChoice<TRelic>(
		Func<Task> onChosen,
		string optionKey,
		string pageKey = InitialPage)
		where TRelic : RelicModel
	{
		return RelicChoice<TRelic>(OwnerOrThrow, onChosen, optionKey, pageKey);
	}

	protected EventOption RelicChoice<TRelic>(
		Player owner,
		Func<Task> onChosen,
		string optionKey,
		string pageKey = InitialPage)
		where TRelic : RelicModel
	{
		EventOption option = Choice(onChosen, optionKey, pageKey).WithRelic<TRelic>(owner);
		option.HoverTips = HoverTipFactory.FromRelic<TRelic>();
		return option;
	}

	protected EventOption RelicPreviewChoice<TFirstRelic, TSecondRelic>(
		Func<Task> onChosen,
		string optionKey,
		string pageKey = InitialPage)
		where TFirstRelic : RelicModel
		where TSecondRelic : RelicModel
	{
		EventOption option = Choice(onChosen, optionKey, pageKey);
		option.HoverTips =
		[
			.. HoverTipFactory.FromRelic<TFirstRelic>(),
			.. HoverTipFactory.FromRelic<TSecondRelic>()
		];
		return option;
	}

	protected EventOption RelicCostChoice(
		RelicModel relic,
		Func<Task> onChosen,
		string optionKey,
		string pageKey = InitialPage)
	{
		EventOption option = Choice(onChosen, optionKey, pageKey).WithRelic(relic);
		option.HoverTips = relic.HoverTips;
		option.Description.Add("Relic", relic.Title.GetFormattedText());
		return option;
	}

	protected EventOption PotionCostChoice(
		PotionModel potion,
		Func<Task> onChosen,
		string optionKey,
		string pageKey = InitialPage)
	{
		EventOption option = Choice(onChosen, optionKey, pageKey);
		option.HoverTips = potion.HoverTips;
		option.Description.Add("Potion", potion.Title.GetFormattedText());
		return option;
	}

	protected EventOption CardPreviewChoice<TCard>(
		Func<Task> onChosen,
		string optionKey,
		string pageKey = InitialPage)
		where TCard : CardModel
	{
		EventOption option = Choice(onChosen, optionKey, pageKey);
		option.HoverTips = [HoverTipFactory.FromCard<TCard>()];
		return option;
	}

	protected EventOption PotionPreviewChoice<TPotion>(
		Func<Task> onChosen,
		string optionKey,
		string pageKey = InitialPage)
		where TPotion : PotionModel
	{
		EventOption option = Choice(onChosen, optionKey, pageKey);
		option.HoverTips = [HoverTipFactory.FromPotion<TPotion>()];
		return option;
	}
}
