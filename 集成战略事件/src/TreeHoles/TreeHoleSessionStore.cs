using MegaCrit.Sts2.Core.Runs;

namespace IntegratedStrategyEvents.TreeHoles;

internal sealed class TreeHoleSessionStore
{
	private readonly Dictionary<RunState, TreeHoleSession> _treeHoleSessions = [];
	private readonly Dictionary<RunState, EndlessFinaleSession> _finaleSessions = [];
	private readonly Dictionary<RunState, TreeHoleRestoreSnapshot> _pendingRestoreSnapshots = [];
	private readonly HashSet<RunState> _pendingFinaleEntries = [];
	private readonly HashSet<RunState> _pendingArchitectCompletions = [];

	public bool IsActive(RunState state)
	{
		return _treeHoleSessions.ContainsKey(state) || _finaleSessions.ContainsKey(state);
	}

	public bool TryGetTreeHoleSession(RunState state, out TreeHoleSession session)
	{
		return _treeHoleSessions.TryGetValue(state, out session!);
	}

	public void SetTreeHoleSession(RunState state, TreeHoleSession session)
	{
		_treeHoleSessions[state] = session;
	}

	public bool RemoveTreeHoleSession(RunState state)
	{
		return _treeHoleSessions.Remove(state);
	}

	public bool TryGetFinaleSession(RunState state, out EndlessFinaleSession session)
	{
		return _finaleSessions.TryGetValue(state, out session!);
	}

	public void SetFinaleSession(RunState state, EndlessFinaleSession session)
	{
		_finaleSessions[state] = session;
	}

	public bool RemoveFinaleSession(RunState state)
	{
		return _finaleSessions.Remove(state);
	}

	public void QueueRestore(RunState state, TreeHoleRestoreSnapshot snapshot)
	{
		_pendingRestoreSnapshots[state] = snapshot;
	}

	public bool TryGetPendingRestore(RunState state, out TreeHoleRestoreSnapshot snapshot)
	{
		return _pendingRestoreSnapshots.TryGetValue(state, out snapshot!);
	}

	public bool RemovePendingRestore(RunState state)
	{
		return _pendingRestoreSnapshots.Remove(state);
	}

	public bool AddPendingFinaleEntry(RunState state)
	{
		return _pendingFinaleEntries.Add(state);
	}

	public bool RemovePendingFinaleEntry(RunState state)
	{
		return _pendingFinaleEntries.Remove(state);
	}

	public void AddPendingArchitectCompletion(RunState state)
	{
		_pendingArchitectCompletions.Add(state);
	}

	public bool HasPendingArchitectCompletion(RunState state)
	{
		return _pendingArchitectCompletions.Contains(state);
	}

	public bool RemovePendingArchitectCompletion(RunState state)
	{
		return _pendingArchitectCompletions.Remove(state);
	}

	public void ClearForRunStarted(RunState state)
	{
		_pendingRestoreSnapshots.TryGetValue(state, out TreeHoleRestoreSnapshot? pendingRestoreSnapshot);
		_treeHoleSessions.Clear();
		_finaleSessions.Clear();
		_pendingRestoreSnapshots.Clear();
		if (pendingRestoreSnapshot != null)
		{
			_pendingRestoreSnapshots[state] = pendingRestoreSnapshot;
		}

		_pendingFinaleEntries.Clear();
		_pendingArchitectCompletions.Clear();
	}
}
