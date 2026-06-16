using MegaCrit.Sts2.Core.Localization;

namespace IntegratedStrategyEvents.Events;

internal static class IntegratedStrategyEventLocalization
{
	public static T ForCurrentLanguage<T>(T zhs, T fallback)
	{
		return LocManager.Instance.Language == "zhs" ? zhs : fallback;
	}
}
