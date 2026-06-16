using Godot;
using IntegratedStrategyEvents.Events;
using System.Runtime.CompilerServices;

namespace IntegratedStrategyEvents.UI;

internal sealed class IntegratedStrategyEventLayoutBaseline
{
	public string EventId { get; set; } = "";
	public bool LeftAligned { get; set; }
	public float ContentWidthScale { get; set; }
	public float HorizontalOffset { get; set; }
	public Vector2 OriginalGlobalPosition { get; set; }
	public Vector2 OriginalSize { get; set; }
	public Vector2 LastAppliedGlobalPosition { get; set; }
	public bool HasAppliedPosition { get; set; }
}

internal static class IntegratedStrategyEventLayoutBaselineStore
{
	private static readonly ConditionalWeakTable<Control, IntegratedStrategyEventLayoutBaseline> LayoutBaselines = new();

	public static IntegratedStrategyEventLayoutBaseline Get(
		Control control,
		IntegratedStrategyEventModel strategyEvent,
		IntegratedStrategyEventLayoutProfile layoutProfile)
	{
		IntegratedStrategyEventLayoutBaseline baseline = LayoutBaselines.GetOrCreateValue(control);
		if (!Matches(baseline, strategyEvent, layoutProfile))
		{
			Reset(baseline, control, strategyEvent, layoutProfile);
			return baseline;
		}

		if (!baseline.HasAppliedPosition
			|| !IntegratedStrategyEventLayoutGeometry.PositionsNearlyEqual(
				control.GlobalPosition,
				baseline.LastAppliedGlobalPosition))
		{
			baseline.OriginalGlobalPosition = control.GlobalPosition;
			baseline.OriginalSize = control.Size;
		}

		return baseline;
	}

	public static bool TryGetMatching(
		Control control,
		IntegratedStrategyEventModel strategyEvent,
		IntegratedStrategyEventLayoutProfile layoutProfile,
		out IntegratedStrategyEventLayoutBaseline baseline)
	{
		if (!LayoutBaselines.TryGetValue(control, out IntegratedStrategyEventLayoutBaseline? foundBaseline))
		{
			baseline = null!;
			return false;
		}

		baseline = foundBaseline;
		return Matches(baseline, strategyEvent, layoutProfile);
	}

	private static void Reset(
		IntegratedStrategyEventLayoutBaseline baseline,
		Control control,
		IntegratedStrategyEventModel strategyEvent,
		IntegratedStrategyEventLayoutProfile layoutProfile)
	{
		baseline.EventId = strategyEvent.Id.Entry;
		baseline.LeftAligned = layoutProfile.LeftAligned;
		baseline.ContentWidthScale = layoutProfile.ContentWidthScale;
		baseline.HorizontalOffset = layoutProfile.HorizontalOffset;
		baseline.OriginalGlobalPosition = control.GlobalPosition;
		baseline.OriginalSize = control.Size;
		baseline.LastAppliedGlobalPosition = control.GlobalPosition;
		baseline.HasAppliedPosition = false;
	}

	private static bool Matches(
		IntegratedStrategyEventLayoutBaseline baseline,
		IntegratedStrategyEventModel strategyEvent,
		IntegratedStrategyEventLayoutProfile layoutProfile)
	{
		return baseline.EventId == strategyEvent.Id.Entry
			&& baseline.LeftAligned == layoutProfile.LeftAligned
			&& Math.Abs(baseline.ContentWidthScale - layoutProfile.ContentWidthScale) <= 0.001f
			&& Math.Abs(baseline.HorizontalOffset - layoutProfile.HorizontalOffset) <= 0.001f;
	}
}
