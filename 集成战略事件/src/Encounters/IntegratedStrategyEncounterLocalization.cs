using System.Text.Json;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;

namespace IntegratedStrategyEvents.Encounters;

internal static class IntegratedStrategyEncounterLocalization
{
	private const string LocTable = "encounters";
	private static bool localeChangeSubscribed;

	public static void Install()
	{
		TrySubscribeToLocaleChanges();
		MergeCurrentEncounterLocalization();
	}

	private static void TrySubscribeToLocaleChanges()
	{
		if (localeChangeSubscribed || LocManager.Instance == null)
		{
			return;
		}

		LocManager.Instance.SubscribeToLocaleChange(MergeCurrentEncounterLocalization);
		localeChangeSubscribed = true;
	}

	internal static void MergeCurrentEncounterLocalization()
	{
		if (LocManager.Instance == null)
		{
			return;
		}

		TrySubscribeToLocaleChanges();

		try
		{
			Dictionary<string, string> entries = BuildEncounterLocalization();
			if (entries.Count == 0)
			{
				return;
			}

			LocManager.Instance.GetTable(LocTable).MergeWith(entries);
			Log.Info($"{ModInfo.LogPrefix} Merged {entries.Count} encounter localization entries for {LocManager.Instance.Language}.");
		}
		catch (Exception ex)
		{
			Log.Warn($"{ModInfo.LogPrefix} Failed to merge encounter localization compatibility entries: {ex}");
		}
	}

	private static Dictionary<string, string> BuildEncounterLocalization()
	{
		Dictionary<string, string> entries = LoadEncounterLocalizationForCurrentLanguage();
		AddFallbackEntries(entries);
		return entries;
	}

	private static Dictionary<string, string> LoadEncounterLocalizationForCurrentLanguage()
	{
		return LoadEncounterLocalization(LocManager.Instance.Language) ??
			LoadEncounterLocalization("eng") ??
			new Dictionary<string, string>(StringComparer.Ordinal);
	}

	private static Dictionary<string, string>? LoadEncounterLocalization(string language)
	{
		string path = $"res://{ModInfo.ModId}/localization/{language}/encounters.json";
		using Godot.FileAccess? file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Read);
		if (file == null)
		{
			return null;
		}

		string json = file.GetAsText();
		Dictionary<string, string>? entries = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
		return entries == null
			? new Dictionary<string, string>(StringComparer.Ordinal)
			: new Dictionary<string, string>(entries, StringComparer.Ordinal);
	}

	private static void AddFallbackEntries(Dictionary<string, string> entries)
	{
		foreach (Type encounterType in IntegratedStrategyContentCatalog.EncounterTypes)
		{
			if (!typeof(EncounterModel).IsAssignableFrom(encounterType))
			{
				continue;
			}

			string slug = StringHelper.Slugify(encounterType.Name);
			AddFallbackEntrySet(entries, slug);
			AddFallbackEntrySet(entries, $"{ModInfo.ModId.ToUpperInvariant()}-{slug}");
		}
	}

	private static void AddFallbackEntrySet(Dictionary<string, string> entries, string encounterKey)
	{
		bool isChinese = LocManager.Instance.Language == "zhs";
		entries.TryAdd($"{encounterKey}.title", isChinese ? "特殊战斗" : "Special Encounter");
		entries.TryAdd(
			$"{encounterKey}.loss",
			isChinese
				? "{character}倒在了[gold]{encounter}[/gold]之中。"
				: "{character} fell in [gold]{encounter}[/gold].");
	}
}

[HarmonyPatch(typeof(LocManager), nameof(LocManager.Initialize))]
internal static class IntegratedStrategyEncounterLocManagerInitializePatch
{
	private static void Postfix()
	{
		IntegratedStrategyEncounterLocalization.Install();
	}
}
