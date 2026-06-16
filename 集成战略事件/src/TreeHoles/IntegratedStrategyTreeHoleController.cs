using IntegratedStrategyEvents.Encounters;
using IntegratedStrategyEvents.Relics;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Runs.History;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.Core.Commands;

namespace IntegratedStrategyEvents.TreeHoles;

internal static class IntegratedStrategyTreeHoleController
{
	private const string DeepBuriedActName = "深埋迷境";
	private const string DeepBuriedStageLabel = "阶段0";
	private const string EndlessFinaleActName = "无终安息";
	private const string EndlessFinaleStageLabel = "阶段Ω ";
	private const string EternalDustFinaleActName = "永恒之尘";
	private const string EternalDustFinaleStageLabel = "阶段Δ ";
	private const string RadiantApexFinaleActName = "辉光天顶";
	private const string RadiantApexFinaleStageLabel = "阶段Γ ";
	private const string UnknownStageLabel = "阶段？";
	private static readonly TreeHoleSessionStore SessionStore = new();
	private static bool _installed;

	public static void Install()
	{
		if (_installed)
		{
			return;
		}

		RunManager.Instance.RunStarted += OnRunStarted;
		RunManager.Instance.RoomEntered += OnRoomEntered;
		_installed = true;
	}

	public static bool IsActive(IRunState? runState)
	{
		return runState is RunState state && SessionStore.IsActive(state);
	}

	public static bool IsActiveCurrentRun()
	{
		return IsActive(RunManager.Instance.DebugOnlyGetState());
	}

	public static bool TryRestoreCompletedCurrentRun()
	{
		RunState? state = RunManager.Instance.DebugOnlyGetState();
		if (state == null || !SessionStore.TryGetTreeHoleSession(state, out TreeHoleSession session))
		{
			return false;
		}

		if (!HasVisitedCoord(state.VisitedMapCoords, session.TerminalCoord))
		{
			return false;
		}

		RestoreOriginalMap(state, session);
		return true;
	}

	public static bool TryGetCurrentDestination(out string actName)
	{
		RunState? state = RunManager.Instance.DebugOnlyGetState();
		if (state != null && SessionStore.TryGetTreeHoleSession(state, out TreeHoleSession session))
		{
			actName = session.DestinationActName;
			return true;
		}

		if (state != null && SessionStore.TryGetFinaleSession(state, out EndlessFinaleSession finaleSession))
		{
			actName = finaleSession.DestinationActName;
			return true;
		}

		actName = string.Empty;
		return false;
	}

	public static bool TryGetCurrentDestination(out string stageLabel, out string actName)
	{
		RunState? state = RunManager.Instance.DebugOnlyGetState();
		if (state != null && SessionStore.TryGetTreeHoleSession(state, out TreeHoleSession session))
		{
			stageLabel = session.StageLabel;
			actName = session.DestinationActName;
			return true;
		}

		if (state != null && SessionStore.TryGetFinaleSession(state, out EndlessFinaleSession finaleSession))
		{
			stageLabel = finaleSession.StageLabel;
			actName = finaleSession.DestinationActName;
			return true;
		}

		stageLabel = string.Empty;
		actName = string.Empty;
		return false;
	}

	public static TreeHoleSaveSnapshot? GetSaveSnapshot(RunState? state)
	{
		return IntegratedStrategyTreeHoleSaveStateStore.CreateSnapshot(state, SessionStore);
	}

	public static void QueueRestoreFromSave(SerializableRun save, RunState state)
	{
		TreeHoleRestoreSnapshot? snapshot = IntegratedStrategyTreeHoleSaveStateStore.Load(save);
		if (snapshot == null)
		{
			return;
		}

		SessionStore.QueueRestore(state, snapshot);
		Log.Info($"{ModInfo.LogPrefix} Queued {snapshot.Kind} tree-hole restore from save.");
	}

	public static bool TryRestoreSavedSessionForCurrentRun(ActMap map)
	{
		RunState? state = RunManager.Instance.DebugOnlyGetState();
		if (state == null || !SessionStore.TryGetPendingRestore(state, out TreeHoleRestoreSnapshot snapshot))
		{
			return false;
		}

		if (state.CurrentActIndex != snapshot.CurrentActIndex || !MapContainsCoord(map, snapshot.TerminalCoord))
		{
			Log.Warn($"{ModInfo.LogPrefix} Ignored stale tree-hole restore snapshot.");
			SessionStore.RemovePendingRestore(state);
			return false;
		}

		ActMap originalMap = new SavedActMap(snapshot.OriginalMap);
		List<IReadOnlyList<MapPointHistoryEntry>> originalHistory =
			CopyHistoryByCounts(state.MapPointHistory, snapshot.OriginalMapPointHistoryCounts);

		if (snapshot.Kind == TreeHoleSaveKind.TreeHole)
		{
			SessionStore.SetTreeHoleSession(state, new TreeHoleSession(
				originalMap,
				snapshot.OriginalVisitedMapCoords,
				originalHistory,
				snapshot.OriginalActFloor,
				snapshot.StageLabel,
				snapshot.DestinationActName,
				map,
				snapshot.TerminalCoord));
		}
		else
		{
			SpecialFinaleKind finaleKind = snapshot.Kind switch
			{
				TreeHoleSaveKind.EternalDustFinale => SpecialFinaleKind.EternalDust,
				TreeHoleSaveKind.RadiantApexFinale => SpecialFinaleKind.RadiantApex,
				_ => SpecialFinaleKind.EndlessFinale
			};
			SessionStore.SetFinaleSession(state, new EndlessFinaleSession(
				originalMap,
				snapshot.OriginalVisitedMapCoords,
				originalHistory,
				snapshot.OriginalActFloor,
				state.Act.ToSave(),
				snapshot.StageLabel,
				snapshot.DestinationActName,
				map,
				finaleKind));
		}

		state.ActFloor = snapshot.CurrentActFloor;
		SessionStore.RemovePendingRestore(state);
		RefreshLocationSynchronizers(state);
		Log.Info($"{ModInfo.LogPrefix} Restored {snapshot.Kind} tree-hole session from save.");
		return true;
	}

	public static bool IsCurrentTreeHoleMap(ActMap map)
	{
		RunState? state = RunManager.Instance.DebugOnlyGetState();
		return state != null &&
			SessionStore.TryGetTreeHoleSession(state, out TreeHoleSession session) &&
			ReferenceEquals(session.TreeHoleMap, map);
	}

	public static bool IsCurrentEndlessFinaleMap(ActMap map)
	{
		RunState? state = RunManager.Instance.DebugOnlyGetState();
		return state != null &&
			SessionStore.TryGetFinaleSession(state, out EndlessFinaleSession session) &&
			session.Kind == SpecialFinaleKind.EndlessFinale &&
			ReferenceEquals(session.FinaleMap, map);
	}

	public static bool IsCurrentEternalDustFinaleMap(ActMap map)
	{
		RunState? state = RunManager.Instance.DebugOnlyGetState();
		return state != null &&
			SessionStore.TryGetFinaleSession(state, out EndlessFinaleSession session) &&
			session.Kind == SpecialFinaleKind.EternalDust &&
			ReferenceEquals(session.FinaleMap, map);
	}

	public static bool IsCurrentRadiantApexFinaleMap(ActMap map)
	{
		RunState? state = RunManager.Instance.DebugOnlyGetState();
		return state != null &&
			SessionStore.TryGetFinaleSession(state, out EndlessFinaleSession session) &&
			session.Kind == SpecialFinaleKind.RadiantApex &&
			ReferenceEquals(session.FinaleMap, map);
	}

	public static bool IsAtEternalDustFirstEventPoint(RunState state)
	{
		return SessionStore.TryGetFinaleSession(state, out EndlessFinaleSession session) &&
			session.Kind == SpecialFinaleKind.EternalDust &&
			session.FinaleMap is IntegratedStrategyEternalDustFinaleActMap eternalDustMap &&
			state.CurrentMapCoord.HasValue &&
			state.CurrentMapCoord.Value.Equals(eternalDustMap.FirstEventMapPoint.coord);
	}

	public static bool IsAtEternalDustSecondEventPoint(RunState state)
	{
		return SessionStore.TryGetFinaleSession(state, out EndlessFinaleSession session) &&
			session.Kind == SpecialFinaleKind.EternalDust &&
			session.FinaleMap is IntegratedStrategyEternalDustFinaleActMap eternalDustMap &&
			state.CurrentMapCoord.HasValue &&
			state.CurrentMapCoord.Value.Equals(eternalDustMap.SecondEventMapPoint.coord);
	}

	public static bool IsAtRadiantApexCombatPoint(RunState state)
	{
		return SessionStore.TryGetFinaleSession(state, out EndlessFinaleSession session) &&
			session.Kind == SpecialFinaleKind.RadiantApex &&
			session.FinaleMap is IntegratedStrategyRadiantApexFinaleActMap radiantApexMap &&
			state.CurrentMapCoord.HasValue &&
			(state.CurrentMapCoord.Value.Equals(radiantApexMap.FirstCombatMapPoint.coord) ||
			 state.CurrentMapCoord.Value.Equals(radiantApexMap.SecondCombatMapPoint.coord));
	}

	public static Task EnterFromEvent(Player owner)
	{
		return EnterFromEvent(owner, DeepBuriedActName, DeepBuriedStageLabel);
	}

	public static Task EnterFromEvent(Player owner, string destinationActName)
	{
		return EnterFromEvent(owner, destinationActName, UnknownStageLabel);
	}

	public static Task EnterFromEvent(Player owner, string destinationActName, string stageLabel)
	{
		_ = TaskHelper.RunSafely(EnterFromEventDeferred(owner, destinationActName, stageLabel));
		return Task.CompletedTask;
	}

	public static Task EnterFromDebugCommand(Player owner, string destinationActName, string stageLabel)
	{
		return EnterFromEventDeferred(owner, destinationActName, stageLabel);
	}

	private static async Task EnterFromEventDeferred(Player owner, string destinationActName, string stageLabel)
	{
		RunManager runManager = RunManager.Instance;
		if (owner.RunState is not RunState state)
		{
			Log.Warn($"{ModInfo.LogPrefix} Tried to enter a tree-hole without a run state.");
			return;
		}

		if (IsActive(state))
		{
			Log.Warn($"{ModInfo.LogPrefix} Tried to enter a tree-hole while one is already active.");
			return;
		}

		await AwaitNextProcessFrame();

		if (!ReferenceEquals(runManager.DebugOnlyGetState(), state))
		{
			Log.Warn($"{ModInfo.LogPrefix} Tree-hole entry was cancelled because the active run changed.");
			return;
		}

		if (TestMode.IsOff && NGame.Instance != null)
		{
			await NGame.Instance.Transition.RoomFadeOut();
		}

		TreeHoleRunAccessor.ClearScreens(runManager);
		IntegratedStrategyTreeHoleActMap treeHoleMap =
			IntegratedStrategyTreeHoleActMap.Create(owner.PlayerRng.Rewards);
		TreeHoleSession session = new(
			state.Map,
			state.VisitedMapCoords.ToList(),
			state.MapPointHistory.Select(static history => history.ToList()).ToList(),
			state.ActFloor,
			stageLabel,
			destinationActName,
			treeHoleMap,
			treeHoleMap.TerminalCoord);

		SessionStore.SetTreeHoleSession(state, session);
		state.Map = treeHoleMap;
		state.ClearVisitedMapCoordsDebug();
		state.AddVisitedMapCoord(treeHoleMap.StartingMapPoint.coord);
		RefreshLocationSynchronizers(state);
		SetMapScreen(treeHoleMap, state, initMarker: false);

		Log.Info($"{ModInfo.LogPrefix} Entering {destinationActName} tree-hole.");
		await runManager.EnterRoom(new MapRoom());
		Log.Info($"{ModInfo.LogPrefix} Entered {destinationActName} tree-hole map room.");
		await TreeHoleRunAccessor.FadeIn(runManager, showTransition: true);
	}

	public static bool HandleEnterNextAct(RunManager runManager, ref Task result)
	{
		RunState? state = runManager.DebugOnlyGetState();
		if (state == null)
		{
			return true;
		}

		if (SessionStore.TryGetFinaleSession(state, out EndlessFinaleSession finaleSession))
		{
			if (IsEndlessFinaleBossComplete(state, finaleSession))
			{
				RestoreOriginalMapForArchitect(state, finaleSession);
			}

			return true;
		}

		if (ShouldCompleteArchitectAfterEndlessFinale(state))
		{
			if (TryCompleteArchitectAfterEndlessFinale(runManager, state, out Task? completionTask))
			{
				result = completionTask;
				return false;
			}

			return true;
		}

		SpecialFinaleKind? finaleKind = GetSpecialFinaleEntryKind(state);
		if (finaleKind == null)
		{
			return true;
		}

		if (!SessionStore.AddPendingFinaleEntry(state))
		{
			result = Task.CompletedTask;
			return false;
		}

		result = EnterSpecialFinale(runManager, state, finaleKind.Value);
		return false;
	}

	public static void SuppressArchitectActChangeOptions(EventModel eventModel)
	{
		if (!ShouldSuppressArchitectActChangeOptions(eventModel))
		{
			return;
		}

		if (eventModel.CurrentOptions is not List<EventOption> options || options.Count <= 1)
		{
			return;
		}

		int originalCount = options.Count;
		options.RemoveAll(static option => !IsBaseArchitectOption(option));
		int removedCount = originalCount - options.Count;
		if (removedCount > 0)
		{
			Log.Info($"{ModInfo.LogPrefix} Suppressed {removedCount} non-vanilla Architect option(s) after endless finale.");
		}
	}

	public static IEnumerable<EventOption> FilterArchitectActChangeOptionsForDisplay(
		EventModel eventModel,
		IEnumerable<EventOption> options)
	{
		if (!ShouldSuppressArchitectActChangeOptions(eventModel))
		{
			return options;
		}

		List<EventOption> optionList = options.ToList();
		List<EventOption> filteredOptions = optionList.Where(IsBaseArchitectOption).ToList();
		int removedCount = optionList.Count - filteredOptions.Count;
		if (removedCount > 0)
		{
			Log.Info($"{ModInfo.LogPrefix} Hid {removedCount} non-vanilla Architect option button(s) after endless finale.");
		}

		return filteredOptions;
	}

	public static bool ShouldChooseArchitectOption(EventModel eventModel, EventOption option)
	{
		if (!ShouldSuppressArchitectActChangeOptions(eventModel) || IsBaseArchitectOption(option))
		{
			return true;
		}

		Log.Warn(
			$"{ModInfo.LogPrefix} Blocked non-vanilla Architect option '{option.TextKey}' " +
			"after endless finale.");
		SuppressArchitectActChangeOptions(eventModel);
		return false;
	}

	public static bool HandleCreateRoom(RoomType roomType, AbstractModel? model, ref AbstractRoom result)
	{
		if (!ShouldForceEndlessFinaleBoss(roomType, out _))
		{
			return true;
		}

		result = CreateEndlessFinaleBossRoom(model);
		return false;
	}

	public static void EnsureCreatedRoomIsEndlessFinaleBoss(
		RoomType roomType,
		AbstractModel? model,
		ref AbstractRoom result)
	{
		if (!ShouldForceEndlessFinaleBoss(roomType, out SpecialFinaleKind finaleKind) ||
			IsExpectedFinaleBossRoom(result, finaleKind))
		{
			return;
		}

		AbstractModel? replacedModel = result is CombatRoom combatRoom
			? combatRoom.Encounter
			: model;
		result = CreateEndlessFinaleBossRoom(replacedModel);
	}

	public static BossNodeRenderSwap? BeginEndlessFinaleBossNodeRender(MapPoint point)
	{
		RunState? state = RunManager.Instance.DebugOnlyGetState();
		if (state == null ||
			!SessionStore.TryGetFinaleSession(state, out EndlessFinaleSession session) ||
			!point.coord.Equals(session.FinaleMap.BossMapPoint.coord))
		{
			return null;
		}

		SerializableActModel originalActSave = state.Act.ToSave();
		state.Act.SetBossEncounter(GetFinaleBossEncounter(session.Kind));
		state.Act.SetSecondBossEncounter(null);
		return new BossNodeRenderSwap(state, originalActSave);
	}

	public static void EndEndlessFinaleBossNodeRender(BossNodeRenderSwap? swap)
	{
		if (swap == null)
		{
			return;
		}

		TreeHoleRunAccessor.RestoreActRooms(swap.State, swap.OriginalActSave);
	}

	private static void OnRunStarted(RunState _)
	{
		TreeHoleFinaleMusicCoordinator.StopForRunReset();
		SessionStore.ClearForRunStarted(_);
	}

	private static void OnRoomEntered()
	{
		RunState? state = RunManager.Instance.DebugOnlyGetState();
		if (state != null &&
			SessionStore.TryGetFinaleSession(state, out EndlessFinaleSession finaleSession))
		{
			TreeHoleFinaleMusicCoordinator.PlayForEnteredRoom(finaleSession);
		}

		if (state?.CurrentRoom is not MapRoom || !SessionStore.TryGetTreeHoleSession(state, out TreeHoleSession session))
		{
			return;
		}

		if (!HasVisitedCoord(state.VisitedMapCoords, session.TerminalCoord))
		{
			return;
		}

		RestoreOriginalMap(state, session);
	}

	private static void RestoreOriginalMap(RunState state, TreeHoleSession session)
	{
		SessionStore.RemoveTreeHoleSession(state);
		state.Map = session.OriginalMap;
		state.ClearVisitedMapCoordsDebug();
		foreach (MapCoord coord in session.OriginalVisitedMapCoords)
		{
			state.AddVisitedMapCoord(coord);
		}

		RestoreMapPointHistory(state, session.OriginalMapPointHistory);
		state.ActFloor = session.OriginalActFloor;
		RefreshLocationSynchronizers(state);
		SetMapScreen(session.OriginalMap, state, initMarker: state.CurrentMapCoord.HasValue);
		Log.Info($"{ModInfo.LogPrefix} Returned from {session.DestinationActName} tree-hole.");
	}

	private static SpecialFinaleKind? GetSpecialFinaleEntryKind(RunState state)
	{
		if (IsActive(state) ||
			state.CurrentActIndex < state.Acts.Count - 1 ||
			state.CurrentRoom is not CombatRoom { RoomType: RoomType.Boss, IsVictoryRoom: false })
		{
			return null;
		}

		if (HasTimeAndLight(state))
		{
			return SpecialFinaleKind.RadiantApex;
		}

		if (HasDimensionalFluid(state))
		{
			return SpecialFinaleKind.EternalDust;
		}

		return HasEndlessKey(state) ? SpecialFinaleKind.EndlessFinale : null;
	}

	private static bool HasEndlessKey(RunState state)
	{
		return state.Players.Any(static player =>
			player.Relics.Any(static relic => !relic.IsMelted && relic is EndlessKeyRelic));
	}

	private static bool HasDimensionalFluid(RunState state)
	{
		return state.Players.Any(static player =>
			player.Relics.Any(static relic => !relic.IsMelted && relic is DimensionalFluidRelic));
	}

	private static bool HasTimeAndLight(RunState state)
	{
		return state.Players.Any(static player =>
			player.Relics.Any(static relic => !relic.IsMelted && relic is TimeAndLightRelic));
	}

	private static async Task EnterSpecialFinale(
		RunManager runManager,
		RunState state,
		SpecialFinaleKind finaleKind)
	{
		try
		{
			await AwaitNextProcessFrame();
			if (!ReferenceEquals(runManager.DebugOnlyGetState(), state) ||
				GetSpecialFinaleEntryKind(state) != finaleKind)
			{
				Log.Warn($"{ModInfo.LogPrefix} Special finale entry was cancelled because the run state changed.");
				return;
			}

			if (TestMode.IsOff && NGame.Instance != null)
			{
				await NGame.Instance.Transition.RoomFadeOut();
			}

			TreeHoleRunAccessor.ClearScreens(runManager);
			ActMap finaleMap = CreateFinaleMap(finaleKind, state);
			EndlessFinaleSession session = new(
				state.Map,
				state.VisitedMapCoords.ToList(),
				state.MapPointHistory.Select(static history => history.ToList()).ToList(),
				state.ActFloor,
				state.Act.ToSave(),
				GetFinaleStageLabel(finaleKind),
				GetFinaleActName(finaleKind),
				finaleMap,
				finaleKind);

			SessionStore.SetFinaleSession(state, session);
			state.Map = finaleMap;
			state.ClearVisitedMapCoordsDebug();
			state.AddVisitedMapCoord(finaleMap.StartingMapPoint.coord);
			if (finaleKind == SpecialFinaleKind.EndlessFinale)
			{
				await HealPlayersToFull(state);
			}
			RefreshLocationSynchronizers(state);
			SetMapScreen(finaleMap, state, initMarker: false);

			Log.Info($"{ModInfo.LogPrefix} Entering {session.DestinationActName} finale act.");
			await runManager.EnterRoom(new MapRoom());
			TreeHoleFinaleMusicCoordinator.PlayAfterFinaleEntry(finaleKind);

			await TreeHoleRunAccessor.FadeIn(runManager, showTransition: true);
		}
		finally
		{
			SessionStore.RemovePendingFinaleEntry(state);
		}
	}

	private static async Task HealPlayersToFull(RunState state)
	{
		foreach (Player player in state.Players)
		{
			await CreatureCmd.SetCurrentHp(player.Creature, player.Creature.MaxHp);
		}
	}

	private static bool IsEndlessFinaleBossComplete(RunState state, EndlessFinaleSession session)
	{
		return state.CurrentRoom is CombatRoom combatRoom &&
			!combatRoom.IsVictoryRoom &&
			IsAtEndlessFinaleBossPoint(state, session) &&
			IsExpectedFinaleEncounter(combatRoom.Encounter, session.Kind);
	}

	private static bool IsAtEndlessFinaleBossPoint(RunState state, EndlessFinaleSession session)
	{
		return state.CurrentMapCoord.HasValue &&
			state.CurrentMapCoord.Value.Equals(session.FinaleMap.BossMapPoint.coord);
	}

	private static bool ShouldForceEndlessFinaleBoss(RoomType roomType, out SpecialFinaleKind finaleKind)
	{
		RunState? state = RunManager.Instance.DebugOnlyGetState();
		if (state != null &&
			roomType == RoomType.Boss &&
			SessionStore.TryGetFinaleSession(state, out EndlessFinaleSession session) &&
			IsAtEndlessFinaleBossPoint(state, session))
		{
			finaleKind = session.Kind;
			return true;
		}

		finaleKind = default;
		return false;
	}

	private static CombatRoom CreateEndlessFinaleBossRoom(AbstractModel? replacedModel)
	{
		RunState state = RunManager.Instance.DebugOnlyGetState() ??
			throw new InvalidOperationException("Cannot create endless finale boss room without an active run state.");
		if (!SessionStore.TryGetFinaleSession(state, out EndlessFinaleSession session))
		{
			throw new InvalidOperationException("Cannot create special finale boss room without an active finale session.");
		}

		EncounterModel encounter = GetFinaleBossEncounter(session.Kind).ToMutable();
		CombatRoom room = new(encounter, state);
		if (replacedModel is EncounterModel incomingEncounter)
		{
			Log.Info(
				$"{ModInfo.LogPrefix} Forced {session.DestinationActName} finale boss encounter to " +
				$"{encounter.Id} instead of {incomingEncounter.Id}.");
		}

		return room;
	}

	private static bool IsExpectedFinaleBossRoom(AbstractRoom result, SpecialFinaleKind finaleKind)
	{
		return result is CombatRoom combatRoom &&
			IsExpectedFinaleEncounter(combatRoom.Encounter, finaleKind);
	}

	private static bool IsExpectedFinaleEncounter(EncounterModel encounter, SpecialFinaleKind finaleKind)
	{
		return finaleKind switch
		{
			SpecialFinaleKind.EndlessFinale =>
				encounter is FurnaceFinaleAmiyaEncounter ||
				encounter.CanonicalInstance is FurnaceFinaleAmiyaEncounter,
			SpecialFinaleKind.EternalDust =>
				encounter is CalendarKingsPincerBossEncounter ||
				encounter.CanonicalInstance is CalendarKingsPincerBossEncounter,
			SpecialFinaleKind.RadiantApex =>
				encounter is BozhokastiSaintguardGunnerBossEncounter ||
				encounter.CanonicalInstance is BozhokastiSaintguardGunnerBossEncounter,
			_ => false
		};
	}

	private static ActMap CreateFinaleMap(SpecialFinaleKind finaleKind, RunState state)
	{
		return finaleKind switch
		{
			SpecialFinaleKind.EndlessFinale => new IntegratedStrategyEndlessFinaleActMap(),
			SpecialFinaleKind.EternalDust => new IntegratedStrategyEternalDustFinaleActMap(),
			SpecialFinaleKind.RadiantApex => CreateRadiantApexFinaleMap(state),
			_ => throw new ArgumentOutOfRangeException(nameof(finaleKind), finaleKind, null)
		};
	}

	private static IntegratedStrategyRadiantApexFinaleActMap CreateRadiantApexFinaleMap(RunState state)
	{
		Rng rng = new(CreateRadiantApexCombatNodeSeed(state), "integrated_strategy_radiant_apex_combat_nodes");
		MapPointType firstCombatPointType = RollRadiantApexCombatPointType(rng);
		MapPointType secondCombatPointType = RollRadiantApexCombatPointType(rng);
		Log.Info(
			$"{ModInfo.LogPrefix} Radiant Apex combat nodes generated as " +
			$"{firstCombatPointType} and {secondCombatPointType}.");
		return new IntegratedStrategyRadiantApexFinaleActMap(firstCombatPointType, secondCombatPointType);
	}

	private static MapPointType RollRadiantApexCombatPointType(Rng rng)
	{
		return rng.NextBool() ? MapPointType.Elite : MapPointType.Monster;
	}

	private static uint CreateRadiantApexCombatNodeSeed(RunState state)
	{
		return unchecked(state.Rng.Seed ^
			(uint)state.CurrentActIndex * 0x9e3779b9u ^
			(uint)state.ActFloor * 0x85ebca6bu ^
			0xA9E5_2026u);
	}

	private static string GetFinaleStageLabel(SpecialFinaleKind finaleKind)
	{
		return finaleKind switch
		{
			SpecialFinaleKind.EndlessFinale => EndlessFinaleStageLabel,
			SpecialFinaleKind.EternalDust => EternalDustFinaleStageLabel,
			SpecialFinaleKind.RadiantApex => RadiantApexFinaleStageLabel,
			_ => throw new ArgumentOutOfRangeException(nameof(finaleKind), finaleKind, null)
		};
	}

	private static string GetFinaleActName(SpecialFinaleKind finaleKind)
	{
		return finaleKind switch
		{
			SpecialFinaleKind.EndlessFinale => EndlessFinaleActName,
			SpecialFinaleKind.EternalDust => EternalDustFinaleActName,
			SpecialFinaleKind.RadiantApex => RadiantApexFinaleActName,
			_ => throw new ArgumentOutOfRangeException(nameof(finaleKind), finaleKind, null)
		};
	}

	private static EncounterModel GetFinaleBossEncounter(SpecialFinaleKind finaleKind)
	{
		return finaleKind switch
		{
			SpecialFinaleKind.EndlessFinale => ModelDb.Encounter<FurnaceFinaleAmiyaEncounter>(),
			SpecialFinaleKind.EternalDust => ModelDb.Encounter<CalendarKingsPincerBossEncounter>(),
			SpecialFinaleKind.RadiantApex => ModelDb.Encounter<BozhokastiSaintguardGunnerBossEncounter>(),
			_ => throw new ArgumentOutOfRangeException(nameof(finaleKind), finaleKind, null)
		};
	}

	private static void RestoreOriginalMapForArchitect(RunState state, EndlessFinaleSession session)
	{
		TreeHoleFinaleMusicCoordinator.StopBeforeArchitectHandoff(session);

		SessionStore.RemoveFinaleSession(state);
		SessionStore.RemovePendingFinaleEntry(state);
		state.Map = session.OriginalMap;
		state.ClearVisitedMapCoordsDebug();
		foreach (MapCoord coord in session.OriginalVisitedMapCoords)
		{
			state.AddVisitedMapCoord(coord);
		}

		RestoreMapPointHistory(state, session.OriginalMapPointHistory);
		state.ActFloor = session.OriginalActFloor;
		TreeHoleRunAccessor.RestoreActRooms(state, session.OriginalActSave);
		RefreshLocationSynchronizers(state);
		SessionStore.AddPendingArchitectCompletion(state);
		Log.Info($"{ModInfo.LogPrefix} Returned from {session.DestinationActName} finale before entering The Architect.");
	}

	private static bool ShouldCompleteArchitectAfterEndlessFinale(RunState state)
	{
		return SessionStore.HasPendingArchitectCompletion(state) &&
			state.CurrentRoom?.IsVictoryRoom == true;
	}

	private static bool TryCompleteArchitectAfterEndlessFinale(
		RunManager runManager,
		RunState state,
		out Task completionTask)
	{
		completionTask = Task.CompletedTask;
		if (!TreeHoleRunAccessor.TryWinRun(runManager, out Task task))
		{
			Log.Warn($"{ModInfo.LogPrefix} Could not complete endless finale Architect handoff via RunManager.WinRun.");
			return false;
		}

		SessionStore.RemovePendingArchitectCompletion(state);
		completionTask = task;
		Log.Info($"{ModInfo.LogPrefix} Completing run from endless finale Architect handoff.");
		return true;
	}

	private static bool IsBaseArchitectOption(EventOption option)
	{
		return option.TextKey == "PROCEED" ||
			option.TextKey.StartsWith("THE_ARCHITECT.dialogue.", StringComparison.Ordinal);
	}

	private static bool ShouldSuppressArchitectActChangeOptions(EventModel eventModel)
	{
		return eventModel is TheArchitect &&
			eventModel.Owner?.RunState is RunState state &&
			SessionStore.HasPendingArchitectCompletion(state);
	}

	private static void RestoreMapPointHistory(
		RunState state,
		IReadOnlyList<IReadOnlyList<MapPointHistoryEntry>> originalHistory)
	{
		if (!TreeHoleRunAccessor.TryGetMapPointHistory(state, out List<List<MapPointHistoryEntry>> mapPointHistory))
		{
			Log.Warn($"{ModInfo.LogPrefix} Could not restore tree-hole map history.");
			return;
		}

		mapPointHistory.Clear();
		foreach (IReadOnlyList<MapPointHistoryEntry> actHistory in originalHistory)
		{
			mapPointHistory.Add(actHistory.ToList());
		}
	}

	private static List<IReadOnlyList<MapPointHistoryEntry>> CopyHistoryByCounts(
		IReadOnlyList<IReadOnlyList<MapPointHistoryEntry>> source,
		IReadOnlyList<int> counts)
	{
		List<IReadOnlyList<MapPointHistoryEntry>> result = [];
		for (int actIndex = 0; actIndex < counts.Count; actIndex++)
		{
			IReadOnlyList<MapPointHistoryEntry> sourceHistory =
				actIndex < source.Count ? source[actIndex] : [];
			int takeCount = Math.Min(Math.Max(counts[actIndex], 0), sourceHistory.Count);
			result.Add(sourceHistory.Take(takeCount).ToList());
		}

		return result;
	}

	private static bool HasVisitedCoord(IEnumerable<MapCoord> visitedCoords, MapCoord coord)
	{
		return visitedCoords.Any(visited => visited.Equals(coord));
	}

	private static bool MapContainsCoord(ActMap map, MapCoord coord)
	{
		return map.HasPoint(coord) ||
			map.StartingMapPoint.coord.Equals(coord) ||
			map.BossMapPoint.coord.Equals(coord) ||
			map.SecondBossMapPoint?.coord.Equals(coord) == true;
	}

	private static void RefreshLocationSynchronizers(RunState state)
	{
		RunManager.Instance.MapSelectionSynchronizer.OnLocationChanged(state.MapLocation);
		RunManager.Instance.RunLocationTargetedBuffer.OnLocationChanged(state.RunLocation);
	}

	private static void SetMapScreen(ActMap map, RunState state, bool initMarker)
	{
		NMapScreen? mapScreen = NMapScreen.Instance;
		if (mapScreen == null)
		{
			return;
		}

		mapScreen.SetMap(map, state.Rng.Seed, clearDrawings: true);
		if (initMarker && state.CurrentMapCoord is { } currentCoord && map.HasPoint(currentCoord))
		{
			mapScreen.InitMarker(currentCoord);
		}

		mapScreen.SetTravelEnabled(true);
		mapScreen.RefreshAllMapPointVotes();
	}

	private static async Task AwaitNextProcessFrame()
	{
		if (NGame.Instance != null)
		{
			await NGame.Instance.AwaitProcessFrame();
			return;
		}

		await Task.Yield();
	}

}
