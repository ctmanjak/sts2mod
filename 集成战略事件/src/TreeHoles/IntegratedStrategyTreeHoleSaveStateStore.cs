using System.Text.Json;
using Godot;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace IntegratedStrategyEvents.TreeHoles;

internal static class IntegratedStrategyTreeHoleSaveStateStore
{
	private const int CurrentVersion = 1;
	private const string StateFileName = "integrated_strategy_tree_hole_state.json";
	private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
	{
		WriteIndented = true,
		PropertyNameCaseInsensitive = true
	};

	public static void Save(
		SerializableRun run,
		TreeHoleSaveSnapshot snapshot)
	{
		try
		{
			PersistedState state = new()
			{
				Version = CurrentVersion,
				StartTime = run.StartTime,
				Kind = snapshot.Kind.ToString(),
				CurrentActIndex = snapshot.CurrentActIndex,
				CurrentActFloor = snapshot.CurrentActFloor,
				OriginalMap = SerializableActMap.FromActMap(snapshot.OriginalMap),
				OriginalVisitedMapCoords = snapshot.OriginalVisitedMapCoords.ToList(),
				OriginalMapPointHistoryCounts = snapshot.OriginalMapPointHistory
					.Select(static history => history.Count)
					.ToList(),
				OriginalActFloor = snapshot.OriginalActFloor,
				StageLabel = snapshot.StageLabel,
				DestinationActName = snapshot.DestinationActName,
				TerminalCoord = snapshot.TerminalCoord
			};

			string path = GetStatePath();
			Directory.CreateDirectory(Path.GetDirectoryName(path)!);
			File.WriteAllText(path, JsonSerializer.Serialize(state, JsonOptions));
		}
		catch (Exception ex)
		{
			Log.Warn($"{ModInfo.LogPrefix} Failed to persist tree-hole restore state: {ex}");
		}
	}

	public static TreeHoleSaveSnapshot? CreateSnapshot(RunState? state, TreeHoleSessionStore sessions)
	{
		if (state == null || !sessions.TryGetTreeHoleSession(state, out TreeHoleSession session))
		{
			if (state == null || !sessions.TryGetFinaleSession(state, out EndlessFinaleSession finaleSession))
			{
				return null;
			}

			TreeHoleSaveKind kind = finaleSession.Kind switch
			{
				SpecialFinaleKind.EternalDust => TreeHoleSaveKind.EternalDustFinale,
				SpecialFinaleKind.RadiantApex => TreeHoleSaveKind.RadiantApexFinale,
				_ => TreeHoleSaveKind.EndlessFinale
			};

			return new TreeHoleSaveSnapshot(
				kind,
				state.CurrentActIndex,
				state.Map,
				state.VisitedMapCoords.ToList(),
				state.MapPointHistory.Select(static history => history.ToList()).ToList(),
				state.ActFloor,
				finaleSession.OriginalMap,
				finaleSession.OriginalVisitedMapCoords,
				finaleSession.OriginalMapPointHistory,
				finaleSession.OriginalActFloor,
				finaleSession.StageLabel,
				finaleSession.DestinationActName,
				finaleSession.FinaleMap.BossMapPoint.coord);
		}

		return new TreeHoleSaveSnapshot(
			TreeHoleSaveKind.TreeHole,
			state.CurrentActIndex,
			state.Map,
			state.VisitedMapCoords.ToList(),
			state.MapPointHistory.Select(static history => history.ToList()).ToList(),
			state.ActFloor,
			session.OriginalMap,
			session.OriginalVisitedMapCoords,
			session.OriginalMapPointHistory,
			session.OriginalActFloor,
			session.StageLabel,
			session.DestinationActName,
			session.TerminalCoord);
	}

	public static TreeHoleRestoreSnapshot? Load(SerializableRun save)
	{
		try
		{
			string path = GetStatePath();
			if (!File.Exists(path))
			{
				return null;
			}

			PersistedState? state = JsonSerializer.Deserialize<PersistedState>(
				File.ReadAllText(path),
				JsonOptions);
			if (state == null ||
				state.Version != CurrentVersion ||
				state.StartTime != save.StartTime ||
				state.CurrentActIndex != save.CurrentActIndex ||
				state.OriginalMap == null ||
				!Enum.TryParse(state.Kind, out TreeHoleSaveKind kind))
			{
				return null;
			}

			return new TreeHoleRestoreSnapshot(
				kind,
				state.CurrentActIndex,
				state.CurrentActFloor,
				state.OriginalMap,
				state.OriginalVisitedMapCoords ?? [],
				state.OriginalMapPointHistoryCounts ?? [],
				state.OriginalActFloor,
				state.StageLabel ?? string.Empty,
				state.DestinationActName ?? string.Empty,
				state.TerminalCoord);
		}
		catch (Exception ex)
		{
			Log.Warn($"{ModInfo.LogPrefix} Failed to load tree-hole restore state: {ex}");
			return null;
		}
	}

	public static void Clear()
	{
		try
		{
			string path = GetStatePath();
			if (File.Exists(path))
			{
				File.Delete(path);
			}
		}
		catch (Exception ex)
		{
			Log.Warn($"{ModInfo.LogPrefix} Failed to clear tree-hole restore state: {ex}");
		}
	}

	private static string GetStatePath()
	{
		string godotPath = SaveManager.Instance.GetProfileScopedPath(
			Path.Combine("IntegratedStrategyEvents", StateFileName));
		return ProjectSettings.GlobalizePath(godotPath);
	}

	private sealed class PersistedState
	{
		public int Version { get; set; }

		public long StartTime { get; set; }

		public string Kind { get; set; } = string.Empty;

		public int CurrentActIndex { get; set; }

		public int CurrentActFloor { get; set; }

		public SerializableActMap? OriginalMap { get; set; }

		public List<MapCoord>? OriginalVisitedMapCoords { get; set; }

		public List<int>? OriginalMapPointHistoryCounts { get; set; }

		public int OriginalActFloor { get; set; }

		public string? StageLabel { get; set; }

		public string? DestinationActName { get; set; }

		public MapCoord TerminalCoord { get; set; }
	}
}
