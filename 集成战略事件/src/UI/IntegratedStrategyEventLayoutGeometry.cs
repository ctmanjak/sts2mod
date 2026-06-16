using Godot;
using IntegratedStrategyEvents.Events;
using MegaCrit.Sts2.Core.Nodes.Events;

namespace IntegratedStrategyEvents.UI;

internal static class IntegratedStrategyEventLayoutGeometry
{
	private const float LeftMarginRatio = 0.05f;
	private const float MinLeftMargin = 80f;
	private const float MaxLeftMargin = 140f;
	private const float RightMarginRatio = 0.02f;
	private const float MinRightMargin = 40f;
	private const float MaxRightMargin = 80f;
	private const float FallbackViewportWidth = 2560f;
	private const float FallbackContentWidthRatio = 0.42f;
	private const float MinContentWidth = 420f;
	private const float PositionTolerance = 1f;

	public static float GetViewportWidth(NEventLayout layout)
	{
		float viewportWidth = layout.GetViewportRect().Size.X;
		if (viewportWidth > 0f)
		{
			return viewportWidth;
		}

		return layout.Size.X > 0f ? layout.Size.X : FallbackViewportWidth;
	}

	public static (float left, float right) GetHorizontalMargins(float viewportWidth)
	{
		float left = Math.Clamp(viewportWidth * LeftMarginRatio, MinLeftMargin, MaxLeftMargin);
		float right = Math.Clamp(viewportWidth * RightMarginRatio, MinRightMargin, MaxRightMargin);
		return (left, right);
	}

	public static float GetRightAnchoredX(
		IntegratedStrategyEventLayoutBaseline baseline,
		float viewportWidth,
		float contentWidth)
	{
		float originalWidth = baseline.OriginalSize.X > contentWidth
			? baseline.OriginalSize.X
			: GetBaseContentWidth(viewportWidth);
		float widthDelta = Math.Max(0f, originalWidth - contentWidth);
		return baseline.OriginalGlobalPosition.X + widthDelta;
	}

	public static float ClampHorizontalPosition(
		float targetX,
		float viewportWidth,
		float contentWidth,
		float leftMargin,
		float rightMargin)
	{
		float maxX = viewportWidth - contentWidth - rightMargin;
		if (maxX < leftMargin)
		{
			return Math.Max(0f, maxX);
		}

		return Math.Clamp(targetX, leftMargin, maxX);
	}

	public static bool HasWidthAdjustment(IntegratedStrategyEventLayoutProfile layoutProfile)
	{
		return layoutProfile.LeftAligned
			|| Math.Abs(layoutProfile.ContentWidthScale - 1f) > 0.001f;
	}

	public static bool HasPositionAdjustment(IntegratedStrategyEventLayoutProfile layoutProfile)
	{
		return Math.Abs(layoutProfile.VerticalOffset) >= 0.1f
			|| Math.Abs(layoutProfile.HorizontalOffset) >= 0.1f;
	}

	public static float GetEffectiveVerticalOffset(
		int optionCount,
		IntegratedStrategyEventLayoutProfile layoutProfile)
	{
		if (layoutProfile.VerticalOffsetOptionCount.HasValue
			&& optionCount != layoutProfile.VerticalOffsetOptionCount.Value)
		{
			return 0f;
		}

		return layoutProfile.VerticalOffset;
	}

	public static int GetOptionButtonCount(NEventLayout layout)
	{
		int count = 0;
		foreach (Node _ in layout.OptionButtons)
		{
			count++;
		}

		return count;
	}

	public static bool PositionsNearlyEqual(Vector2 left, Vector2 right)
	{
		return Math.Abs(left.X - right.X) <= PositionTolerance
			&& Math.Abs(left.Y - right.Y) <= PositionTolerance;
	}

	public static float GetTargetContentWidth(
		float viewportWidth,
		IntegratedStrategyEventLayoutProfile layoutProfile)
	{
		float baseWidth = GetBaseContentWidth(viewportWidth);
		float targetWidth = baseWidth * layoutProfile.ContentWidthScale;
		if (baseWidth <= MinContentWidth)
		{
			return baseWidth;
		}

		return Math.Clamp(targetWidth, MinContentWidth, baseWidth);
	}

	private static float GetBaseContentWidth(float viewportWidth)
	{
		return viewportWidth * FallbackContentWidthRatio;
	}
}
