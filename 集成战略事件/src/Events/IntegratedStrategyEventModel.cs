using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;

namespace IntegratedStrategyEvents.Events;

public abstract partial class IntegratedStrategyEventModel : CustomEventModel
{
	protected const string InitialPage = "INITIAL";

	protected abstract IntegratedStrategyEventDefinition Definition { get; }

	public override string? CustomInitialPortraitPath => Definition.PortraitPath;

	public virtual IntegratedStrategyEventLayoutProfile LayoutProfile =>
		Definition.Layout ?? IntegratedStrategyEventLayoutProfile.Standard;

	public bool AlignHoverTipsRight => Definition.AlignHoverTipsRight;

	public override List<(string, string)>? Localization =>
		IntegratedStrategyRichText.ApplyFontSizes(Definition.CreateLocalization());

	public override ActModel[] Acts => IntegratedStrategyEventSpawnRules.GetActs(GetType());

	public override bool IsAllowed(IRunState runState)
	{
		return IntegratedStrategyEventSpawnRules.IsAllowed(GetType(), runState);
	}

	protected Player OwnerOrThrow => Owner
		?? throw new InvalidOperationException($"{GetType().Name} has no owner.");

	protected override Task BeforeEventStarted(bool isPreFinished)
	{
		IntegratedStrategyEventRuntimeCompatibility.MergeCurrentEventLocalization();
		return base.BeforeEventStarted(isPreFinished);
	}
}
