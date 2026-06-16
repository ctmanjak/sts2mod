using Godot;
using IntegratedStrategyEvents.Events;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.Events;

namespace IntegratedStrategyEvents.UI;

internal static class IntegratedStrategyEventLayoutApplier
{
	public static void ResetBeforeOptionsAdded(
		NEventLayout layout,
		IntegratedStrategyEventModel strategyEvent)
	{
		IntegratedStrategyEventLayoutProfile profile = strategyEvent.LayoutProfile;
		if (!IntegratedStrategyEventLayoutGeometry.HasPositionAdjustment(profile))
		{
			return;
		}

		VBoxContainer? optionsContainer = IntegratedStrategyEventLayoutNodes.GetOptionsContainer(layout);
		if (optionsContainer == null || !GodotObject.IsInstanceValid(optionsContainer))
		{
			return;
		}

		Control containerToMove = IntegratedStrategyEventLayoutNodes.GetEventTextContainer(layout, optionsContainer);
		if (!IntegratedStrategyEventLayoutBaselineStore.TryGetMatching(
				containerToMove,
				strategyEvent,
				profile,
				out IntegratedStrategyEventLayoutBaseline baseline)
			|| !baseline.HasAppliedPosition)
		{
			return;
		}

		containerToMove.GlobalPosition = baseline.OriginalGlobalPosition;
	}

	public static void ApplyAfterOptionsAdded(
		NEventLayout layout,
		IntegratedStrategyEventModel strategyEvent)
	{
		IntegratedStrategyEventLayoutProfile layoutProfile = strategyEvent.LayoutProfile;
		if (!IntegratedStrategyEventLayoutGeometry.HasWidthAdjustment(layoutProfile)
			&& !IntegratedStrategyEventLayoutGeometry.HasPositionAdjustment(layoutProfile))
		{
			return;
		}

		if (!GodotObject.IsInstanceValid(layout))
		{
			return;
		}

		VBoxContainer? optionsContainer = IntegratedStrategyEventLayoutNodes.GetOptionsContainer(layout);
		if (optionsContainer == null || !GodotObject.IsInstanceValid(optionsContainer))
		{
			return;
		}

		float viewportWidth = IntegratedStrategyEventLayoutGeometry.GetViewportWidth(layout);
		(float left, float right) = IntegratedStrategyEventLayoutGeometry.GetHorizontalMargins(viewportWidth);
		Control containerToMove = IntegratedStrategyEventLayoutNodes.GetEventTextContainer(layout, optionsContainer);
		float contentWidth = IntegratedStrategyEventLayoutGeometry.GetTargetContentWidth(viewportWidth, layoutProfile);
		IntegratedStrategyEventLayoutBaseline baseline =
			IntegratedStrategyEventLayoutBaselineStore.Get(containerToMove, strategyEvent, layoutProfile);
		int optionCount = IntegratedStrategyEventLayoutGeometry.GetOptionButtonCount(layout);
		float verticalOffset = IntegratedStrategyEventLayoutGeometry.GetEffectiveVerticalOffset(optionCount, layoutProfile);
		float targetX = layoutProfile.LeftAligned
			? left
			: IntegratedStrategyEventLayoutGeometry.GetRightAnchoredX(baseline, viewportWidth, contentWidth);
		targetX += layoutProfile.HorizontalOffset;
		targetX = IntegratedStrategyEventLayoutGeometry.ClampHorizontalPosition(
			targetX,
			viewportWidth,
			contentWidth,
			left,
			right);
		float targetY = baseline.OriginalGlobalPosition.Y + verticalOffset;
		if (IntegratedStrategyEventLayoutGeometry.HasWidthAdjustment(layoutProfile))
		{
			IntegratedStrategyEventLayoutNodes.ApplyContentWidth(containerToMove, contentWidth);
			IntegratedStrategyEventLayoutNodes.ApplyContentWidth(optionsContainer, contentWidth);
			IntegratedStrategyEventLayoutNodes.ApplyOptionButtonWidths(layout, contentWidth);
		}

		Vector2 targetPosition = new(targetX, targetY);
		containerToMove.GlobalPosition = targetPosition;
		baseline.LastAppliedGlobalPosition = targetPosition;
		baseline.HasAppliedPosition = true;
		Log.Info(
			$"{ModInfo.LogPrefix}[UI] Adjusted event text group at x={targetX:0.0}, y={targetY:0.0}, "
			+ $"baseY={baseline.OriginalGlobalPosition.Y:0.0}, "
			+ $"leftAligned={layoutProfile.LeftAligned}, width={contentWidth:0.0}, "
			+ $"baseWidth={baseline.OriginalSize.X:0.0}, "
			+ $"rightMargin={right:0.0}, "
			+ $"scale={layoutProfile.ContentWidthScale:0.###}, "
			+ $"horizontalOffset={layoutProfile.HorizontalOffset:0.0}, "
			+ $"verticalOffset={verticalOffset:0.0}/{layoutProfile.VerticalOffset:0.0}, "
			+ $"optionCount={optionCount}.");
	}
}
