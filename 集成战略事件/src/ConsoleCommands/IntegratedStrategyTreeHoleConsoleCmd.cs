using IntegratedStrategyEvents.TreeHoles;
using MegaCrit.Sts2.Core.DevConsole;
using MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Runs;

namespace IntegratedStrategyEvents.ConsoleCommands;

public sealed class IntegratedStrategyTreeHoleConsoleCmd : AbstractConsoleCmd
{
	private const string RandomArgument = "random";
	private const string DeepArgument = "deep";
	private const string CradleArgument = "cradle";
	private const string FragmentArgument = "fragment";
	private static readonly string[] CompletionEntries = [RandomArgument, DeepArgument, CradleArgument, FragmentArgument];
	private static readonly TreeHoleDestination[] RandomDestinations =
	[
		new(DeepArgument, "深埋迷境", "阶段0"),
		new(CradleArgument, "绀碧摇篮", "阶段？"),
		new(FragmentArgument, "诡谲断章", "阶段∅")
	];

	public override string CmdName => "treehole";

	public override string Args => "[random|deep|cradle|fragment]";

	public override string Description => "Jumps to a random IntegratedStrategyEvents tree-hole map.";

	public override bool IsNetworked => true;

	public override CmdResult Process(Player? issuingPlayer, string[] args)
	{
		if (!RunManager.Instance.IsInProgress)
		{
			return new CmdResult(success: false, "A run is currently not in progress!");
		}

		if (issuingPlayer == null)
		{
			return new CmdResult(success: false, "No issuing player found.");
		}

		if (IntegratedStrategyTreeHoleController.IsActive(issuingPlayer.RunState))
		{
			return new CmdResult(success: false, "A tree-hole map is already active.");
		}

		if (!TryPickDestination(issuingPlayer, args, out TreeHoleDestination destination, out string error))
		{
			return new CmdResult(success: false, error);
		}

		Task task = IntegratedStrategyTreeHoleController.EnterFromDebugCommand(
			issuingPlayer,
			destination.ActName,
			destination.StageLabel);
		return new CmdResult(
			task,
			success: true,
			$"Entering IntegratedStrategyEvents tree-hole: {destination.ActName}.");
	}

	public override CompletionResult GetArgumentCompletions(Player? player, string[] args)
	{
		if (args.Length <= 1)
		{
			return CompleteArgument(CompletionEntries, [], args.FirstOrDefault() ?? string.Empty);
		}

		return new CompletionResult
		{
			Type = CompletionType.Argument,
			ArgumentContext = CmdName
		};
	}

	private static bool TryPickDestination(
		Player issuingPlayer,
		string[] args,
		out TreeHoleDestination destination,
		out string error)
	{
		string requested = args.FirstOrDefault()?.ToLowerInvariant() ?? RandomArgument;
		if (requested == RandomArgument)
		{
			destination = RandomDestinations[issuingPlayer.PlayerRng.Rewards.NextInt(RandomDestinations.Length)];
			error = string.Empty;
			return true;
		}

		foreach (TreeHoleDestination candidate in RandomDestinations)
		{
			if (candidate.Argument == requested)
			{
				destination = candidate;
				error = string.Empty;
				return true;
			}
		}

		destination = default;
		error = $"Unknown tree-hole destination '{requested}'. Use: {string.Join(", ", CompletionEntries)}.";
		return false;
	}

	private readonly record struct TreeHoleDestination(string Argument, string ActName, string StageLabel);
}
