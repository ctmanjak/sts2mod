using Godot;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.Events;

namespace IntegratedStrategyEvents.UI;

internal static class IntegratedStrategyEventPortraitFitter
{
	private const float TargetAspectRatio = 16f / 9f;
	private static readonly Vector2 FallbackViewportSize = new(1920f, 1080f);

	internal static void ApplyWithDriver(NEventLayout layout)
	{
		Apply(layout, true);
		IntegratedStrategyEventPortraitDriver.Ensure(layout);
	}

	internal static void Apply(NEventLayout layout, bool logResult)
	{
		if (!GodotObject.IsInstanceValid(layout))
		{
			return;
		}

		TextureRect? portrait = layout.GetNodeOrNull<TextureRect>("%Portrait");
		if (portrait == null || !GodotObject.IsInstanceValid(portrait))
		{
			return;
		}

		Vector2 viewportSize = GetViewportSize(layout);
		Rect2 targetRect = FitSixteenByNineInside(viewportSize);
		Vector2 inheritedScale = GetGlobalScale(portrait);

		portrait.TopLevel = false;
		portrait.MouseFilter = Control.MouseFilterEnum.Ignore;
		portrait.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
		portrait.StretchMode = TextureRect.StretchModeEnum.Scale;
		portrait.CustomMinimumSize = Vector2.Zero;
		portrait.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.TopLeft);
		portrait.Size = targetRect.Size;
		portrait.PivotOffset = Vector2.Zero;
		portrait.Set("global_transform", new Transform2D(0f, targetRect.Position));
		if (logResult)
		{
			Vector2 actualScale = GetGlobalScale(portrait);
			Log.Info(
				$"{ModInfo.LogPrefix}[UI] Fitted event portrait: viewport={viewportSize.X:0.0}x{viewportSize.Y:0.0}, "
				+ $"target={targetRect.Position.X:0.0},{targetRect.Position.Y:0.0} {targetRect.Size.X:0.0}x{targetRect.Size.Y:0.0}, "
				+ $"inheritedScale={inheritedScale.X:0.###},{inheritedScale.Y:0.###}, "
				+ $"actualScale={actualScale.X:0.###},{actualScale.Y:0.###}.");
		}
	}

	private static Vector2 GetViewportSize(Control layout)
	{
		Vector2 viewportSize = layout.GetViewportRect().Size;
		if (viewportSize.X > 0f && viewportSize.Y > 0f)
		{
			return viewportSize;
		}

		return layout.Size.X > 0f && layout.Size.Y > 0f
			? layout.Size
			: FallbackViewportSize;
	}

	private static Rect2 FitSixteenByNineInside(Vector2 viewportSize)
	{
		float viewportAspect = viewportSize.X / viewportSize.Y;
		Vector2 targetSize = viewportAspect > TargetAspectRatio
			? new Vector2(viewportSize.Y * TargetAspectRatio, viewportSize.Y)
			: new Vector2(viewportSize.X, viewportSize.X / TargetAspectRatio);
		Vector2 targetPosition = (viewportSize - targetSize) * 0.5f;
		return new Rect2(targetPosition, targetSize);
	}

	private static Vector2 GetGlobalScale(Control control)
	{
		Transform2D transform = control.GetGlobalTransform();
		float scaleX = transform.X.Length();
		float scaleY = transform.Y.Length();
		return new Vector2(
			scaleX > 0f ? scaleX : 1f,
			scaleY > 0f ? scaleY : 1f);
	}
}
