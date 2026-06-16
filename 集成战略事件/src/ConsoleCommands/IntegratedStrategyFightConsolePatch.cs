using HarmonyLib;
using MegaCrit.Sts2.Core.DevConsole;
using MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;
using MegaCrit.Sts2.Core.Models;

namespace IntegratedStrategyEvents.ConsoleCommands;

[HarmonyPatch(typeof(FightConsoleCmd), nameof(FightConsoleCmd.Process))]
internal static class IntegratedStrategyFightConsoleProcessPatch
{
	[HarmonyPrefix]
	private static void AddIntegratedStrategyPrefix(string[] args)
	{
		if (args.Length == 0 || string.IsNullOrWhiteSpace(args[0]))
		{
			return;
		}

		args[0] = IntegratedStrategyFightConsoleAliases.Normalize(args[0]);
	}
}

[HarmonyPatch(typeof(FightConsoleCmd), nameof(FightConsoleCmd.GetArgumentCompletions))]
internal static class IntegratedStrategyFightConsoleCompletionPatch
{
	[HarmonyPostfix]
	private static void AddIntegratedStrategyEntries(string[] args, ref CompletionResult __result)
	{
		if (args.Length > 1)
		{
			return;
		}

		string partial = args.FirstOrDefault() ?? string.Empty;
		HashSet<string> candidates = new(__result.Candidates, StringComparer.OrdinalIgnoreCase);
		foreach (string entry in IntegratedStrategyFightConsoleAliases.GetCompletionEntries())
		{
			if (IsMatch(entry, partial))
			{
				candidates.Add(entry);
			}
		}

		__result.Candidates = candidates
			.OrderBy(entry => GetRelevance(entry, partial))
			.ThenBy(entry => entry, StringComparer.Ordinal)
			.ToList();
		__result.CommonPrefix = CalculateCommonCompletion(__result.Candidates, __result.CommandPrefix);
	}

	private static bool IsMatch(string entry, string partial)
	{
		return string.IsNullOrWhiteSpace(partial) ||
			entry.Contains(partial, StringComparison.OrdinalIgnoreCase);
	}

	private static int GetRelevance(string entry, string partial)
	{
		if (string.IsNullOrWhiteSpace(partial))
		{
			return 0;
		}

		if (entry.StartsWith(partial, StringComparison.OrdinalIgnoreCase))
		{
			return 0;
		}

		string unprefixed = IntegratedStrategyFightConsoleAliases.RemoveModPrefix(entry);
		return unprefixed.StartsWith(partial, StringComparison.OrdinalIgnoreCase) ? 1 : 2;
	}

	private static string CalculateCommonCompletion(IReadOnlyList<string> entries, string prefix)
	{
		if (entries.Count == 0)
		{
			return string.Empty;
		}

		if (entries.Count == 1)
		{
			return prefix + entries[0] + " ";
		}

		int maxLength = entries.Min(entry => entry.Length);
		string first = entries[0];
		int matchingLength = 0;
		for (int i = 0; i < maxLength; i++)
		{
			char c = first[i];
			if (!entries.All(entry => char.ToUpperInvariant(entry[i]) == char.ToUpperInvariant(c)))
			{
				break;
			}

			matchingLength = i + 1;
		}

		return matchingLength == 0 ? string.Empty : prefix + first[..matchingLength];
	}
}

internal static class IntegratedStrategyFightConsoleAliases
{
	private const string ModPrefix = "INTEGRATEDSTRATEGYEVENTS-";
	private static readonly ModelId EncounterCategory = new(ModelId.SlugifyCategory<EncounterModel>(), string.Empty);

	public static string Normalize(string entry)
	{
		string normalized = entry.ToUpperInvariant();
		if (normalized.StartsWith(ModPrefix, StringComparison.Ordinal))
		{
			return normalized;
		}

		if (ModelDb.GetByIdOrNull<EncounterModel>(WithEntry(normalized)) != null)
		{
			return normalized;
		}

		string prefixed = ModPrefix + normalized;
		return ModelDb.GetByIdOrNull<EncounterModel>(WithEntry(prefixed)) == null ? normalized : prefixed;
	}

	public static IEnumerable<string> GetCompletionEntries()
	{
		HashSet<string> entries = new(StringComparer.OrdinalIgnoreCase);
		foreach (Type type in IntegratedStrategyContentCatalog.EncounterTypes)
		{
			if (!type.IsSubclassOf(typeof(EncounterModel)))
			{
				continue;
			}

			string entry = ModelDb.GetId(type).Entry;
			if (entries.Add(entry))
			{
				yield return entry;
			}

			string unprefixed = RemoveModPrefix(entry);
			if (unprefixed != entry && entries.Add(unprefixed))
			{
				yield return unprefixed;
			}
		}
	}

	public static string RemoveModPrefix(string entry)
	{
		return entry.StartsWith(ModPrefix, StringComparison.OrdinalIgnoreCase)
			? entry[ModPrefix.Length..]
			: entry;
	}

	private static ModelId WithEntry(string entry)
	{
		return new ModelId(EncounterCategory.Category, entry);
	}
}
