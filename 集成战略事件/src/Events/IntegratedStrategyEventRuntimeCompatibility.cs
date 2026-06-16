using Godot;
using HarmonyLib;
using System.Reflection;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.TestSupport;

namespace IntegratedStrategyEvents.Events;

internal static class IntegratedStrategyEventRuntimeCompatibility
{
	private static bool localeChangeSubscribed;

	public static void Install()
	{
		TrySubscribeToLocaleChanges();
		MergeCurrentEventLocalization();
	}

	private static void TrySubscribeToLocaleChanges()
	{
		if (localeChangeSubscribed || LocManager.Instance == null)
		{
			return;
		}
		LocManager.Instance.SubscribeToLocaleChange(MergeCurrentEventLocalization);
		localeChangeSubscribed = true;
	}

	internal static void MergeCurrentEventLocalization()
	{
		if (LocManager.Instance == null)
		{
			return;
		}
		TrySubscribeToLocaleChanges();

		try
		{
			Dictionary<string, string> entries = BuildEventLocalization();
			if (entries.Count == 0)
			{
				return;
			}

			LocManager.Instance.GetTable("events").MergeWith(entries);
			Log.Info($"{ModInfo.LogPrefix} Merged {entries.Count} event localization entries for {LocManager.Instance.Language}.");
		}
		catch (Exception ex)
		{
			Log.Warn($"{ModInfo.LogPrefix} Failed to merge event localization compatibility entries: {ex}");
		}
	}

	private static Dictionary<string, string> BuildEventLocalization()
	{
		Dictionary<string, string> entries = new(StringComparer.Ordinal);
		foreach (Type eventType in IntegratedStrategyContentCatalog.EventTypes)
		{
			MethodInfo? createLocalization = eventType.GetMethod(
				"CreateLocalization",
				BindingFlags.Static | BindingFlags.NonPublic);
			if (createLocalization == null)
			{
				continue;
			}

			string eventKey = StringHelper.Slugify(eventType.Name);
			List<(string, string)>? localization = createLocalization.Invoke(null, null) as List<(string, string)>;
			foreach ((string relativeKey, string value) in IntegratedStrategyRichText.ApplyFontSizes(localization) ?? [])
			{
				entries[$"{eventKey}.{relativeKey}"] = value;
			}
		}

		return entries;
	}
}

[HarmonyPatch(typeof(LocManager), nameof(LocManager.Initialize))]
internal static class IntegratedStrategyEventLocManagerInitializePatch
{
	private static void Postfix()
	{
		IntegratedStrategyEventRuntimeCompatibility.Install();
	}
}

[HarmonyPatch(typeof(EventModel), nameof(EventModel.GetAssetPaths))]
internal static class IntegratedStrategyEventAssetPathsPatch
{
	private static void Postfix(EventModel __instance, IRunState runState, ref IEnumerable<string> __result)
	{
		if (TestMode.IsOn || __instance is not IntegratedStrategyEventModel eventModel)
		{
			return;
		}

		string? portraitPath = eventModel.CustomInitialPortraitPath;
		if (string.IsNullOrWhiteSpace(portraitPath))
		{
			return;
		}

		string defaultPortraitPath = $"res://images/events/{__instance.Id.Entry.ToLowerInvariant()}.png";
		List<string> paths = __result
			.Where(path => !string.Equals(path, defaultPortraitPath, StringComparison.Ordinal))
			.ToList();
		if (!paths.Contains(portraitPath, StringComparer.Ordinal))
		{
			paths.Add(portraitPath);
		}

		__result = paths;
	}
}

[HarmonyPatch(typeof(EventModel), nameof(EventModel.CreateInitialPortrait))]
internal static class IntegratedStrategyEventCreateInitialPortraitPatch
{
	private static bool Prefix(EventModel __instance, ref Texture2D __result)
	{
		if (__instance is not IntegratedStrategyEventModel eventModel)
		{
			return true;
		}

		string? portraitPath = eventModel.CustomInitialPortraitPath;
		if (string.IsNullOrWhiteSpace(portraitPath))
		{
			return true;
		}

		__result = PreloadManager.Cache.GetTexture2D(portraitPath);
		return false;
	}
}

[HarmonyPatch(typeof(EventOption), "AddLocVars")]
internal static class IntegratedStrategyEventOptionAddLocVarsPatch
{
	private static bool Prefix(EventOption __instance, EventModel eventModel)
	{
		if (eventModel is not IntegratedStrategyEventModel)
		{
			return true;
		}

		if (__instance.Description != null)
		{
			eventModel.Owner?.Character?.AddDetailsTo(__instance.Description);
			__instance.Description.Add("IsMultiplayer", eventModel.Owner != null && eventModel.Owner.RunState.Players.Count > 1);
		}

		return false;
	}
}
