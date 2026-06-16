using System.Reflection;
using System.Runtime.CompilerServices;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;

namespace IntegratedStrategyEvents.Map;

internal static class IntegratedStrategySecretMapLegendController
{
	private const string LocTable = "map";
	private const string LocKeyPrefix = "LEGEND_SECRET";
	private const float LegendPaperTopExtension = 72f;
	private const float LegendPaperMinimumBottomExtension = 88f;
	private const float LegendPaperBottomPadding = 72f;
	private static readonly Vector2 SecretLegendIconSize = new(44f, 44f);
	private static readonly ConditionalWeakTable<Control, LegendPaperLayoutState> LegendPaperStates = new();
	private static readonly AccessTools.FieldRef<NMapLegendItem, TextureRect> LegendIconRef =
		AccessTools.FieldRefAccess<NMapLegendItem, TextureRect>("_icon");
	private static readonly AccessTools.FieldRef<NMapLegendItem, HoverTip> LegendHoverTipRef =
		AccessTools.FieldRefAccess<NMapLegendItem, HoverTip>("_hoverTip");
	private static readonly AccessTools.FieldRef<NMapLegendItem, MapPointType> LegendPointTypeRef =
		AccessTools.FieldRefAccess<NMapLegendItem, MapPointType>("_pointType");
	private static readonly MethodInfo? AnimHoverMethod = AccessTools.Method(typeof(NNormalMapPoint), "AnimHover");
	private static readonly MethodInfo? AnimUnhoverMethod = AccessTools.Method(typeof(NNormalMapPoint), "AnimUnhover");

	public static void EnsureSecretLegendItem(NMapScreen screen)
	{
		if (AccessTools.Field(typeof(NMapScreen), "_legendItems")?.GetValue(screen) is not Control legendItems)
		{
			return;
		}

		if (legendItems.GetNodeOrNull<NMapLegendItem>(IntegratedStrategySecretMapNodeController.SecretLegendItemName) is NMapLegendItem existing)
		{
			ConfigureSecretLegendItem(existing);
			ExpandLegendPaper(screen);
			return;
		}

		NMapLegendItem? template = legendItems.GetChildren()
			.OfType<NMapLegendItem>()
			.LastOrDefault();
		if (template == null)
		{
			return;
		}

		if (template.Duplicate((int)Node.DuplicateFlags.Scripts) is not NMapLegendItem secretItem)
		{
			return;
		}

		secretItem.Name = IntegratedStrategySecretMapNodeController.SecretLegendItemName;
		legendItems.AddChild(secretItem);
		ConfigureSecretLegendItem(secretItem);
		RebuildFocusNeighbors(legendItems);
		ExpandLegendPaper(screen);
	}

	public static void RefreshLegendPaper(NMapScreen screen)
	{
		ExpandLegendPaper(screen);
	}

	public static bool TrySetSecretLegendPointType(NMapLegendItem item, string name)
	{
		if (name != IntegratedStrategySecretMapNodeController.SecretLegendItemName)
		{
			return false;
		}

		LegendPointTypeRef(item) = IntegratedStrategySecretMapNodeController.SecretLegendHighlightPointType;
		return true;
	}

	public static bool TrySetSecretLegendLocalization(NMapLegendItem item, string name)
	{
		if (name != IntegratedStrategySecretMapNodeController.SecretLegendItemName)
		{
			return false;
		}

		item.GetNode<MegaLabel>("MegaLabel")
			.SetTextAutoSize(new LocString(LocTable, $"{LocKeyPrefix}.title").GetFormattedText());
		LegendHoverTipRef(item) = new HoverTip(
			new LocString(LocTable, $"{LocKeyPrefix}.hoverTip.title"),
			new LocString(LocTable, $"{LocKeyPrefix}.hoverTip.description"));
		return true;
	}

	public static void ConfigureSecretLegendItem(NMapLegendItem item)
	{
		Texture2D? icon = IntegratedStrategySecretMapNodeController.GetSecretIcon();
		if (icon != null)
		{
			TextureRect iconRect = LegendIconRef(item);
			iconRect.Texture = icon;
			ResizeAroundCenter(iconRect, SecretLegendIconSize);
		}
	}

	public static bool TryHandleSecretHighlight(NNormalMapPoint mapPointNode, MapPointType pointType)
	{
		bool isSecretNode = IntegratedStrategySecretMapNodeController.IsSecretMapPointNode(mapPointNode);
		bool isSecretHighlight = IntegratedStrategySecretMapNodeController.IsSecretLegendHighlightType(pointType);
		if (!isSecretNode && !isSecretHighlight)
		{
			return false;
		}

		MethodInfo? method = isSecretNode && isSecretHighlight
			? AnimHoverMethod
			: AnimUnhoverMethod;
		method?.Invoke(mapPointNode, null);
		IntegratedStrategySecretMapNodeController.ApplySecretNodeHighlightOpacity(
			mapPointNode,
			isSecretNode && isSecretHighlight);
		return true;
	}

	private static void RebuildFocusNeighbors(Control legendItems)
	{
		List<NMapLegendItem> items = legendItems.GetChildren().OfType<NMapLegendItem>().ToList();
		for (int i = 0; i < items.Count; i++)
		{
			NodePath path = items[i].GetPath();
			items[i].FocusNeighborTop = i > 0 ? items[i - 1].GetPath() : path;
			items[i].FocusNeighborBottom = i < items.Count - 1 ? items[i + 1].GetPath() : path;
			items[i].FocusNeighborRight = path;
		}
	}

	private static void ExpandLegendPaper(NMapScreen screen)
	{
		Control? mapLegend = AccessTools.Field(typeof(NMapScreen), "_mapLegend")?.GetValue(screen) as Control ??
			screen.GetNodeOrNull<Control>("MapLegend");
		if (mapLegend == null)
		{
			return;
		}

		LegendPaperLayoutState state = LegendPaperStates.GetValue(mapLegend, static legend => new LegendPaperLayoutState(legend));
		ApplyLegendPaperTopExtension(mapLegend, state);

		if (mapLegend is TextureRect textureRect)
		{
			textureRect.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
			textureRect.StretchMode = TextureRect.StretchModeEnum.Scale;
		}

		float targetHeight = Math.Max(
			state.OriginalSize.Y + LegendPaperTopExtension + LegendPaperMinimumBottomExtension,
			GetLegendContentBottom(mapLegend) + LegendPaperBottomPadding);
		mapLegend.Size = new Vector2(state.OriginalSize.X, targetHeight);
		mapLegend.CustomMinimumSize = new Vector2(
			state.OriginalCustomMinimumSize.X,
			Math.Max(state.OriginalCustomMinimumSize.Y, targetHeight));
	}

	private static void ApplyLegendPaperTopExtension(Control mapLegend, LegendPaperLayoutState state)
	{
		mapLegend.Position = new Vector2(
			state.OriginalPosition.X,
			state.OriginalPosition.Y - LegendPaperTopExtension);

		foreach (Control child in mapLegend.GetChildren().OfType<Control>())
		{
			if (!state.OriginalChildPositions.TryGetValue(child, out Vector2 originalChildPosition))
			{
				continue;
			}

			child.Position = new Vector2(
				originalChildPosition.X,
				originalChildPosition.Y + LegendPaperTopExtension);
		}
	}

	private static float GetLegendContentBottom(Control mapLegend)
	{
		float bottom = 0f;
		foreach (Control child in EnumerateDescendantControls(mapLegend))
		{
			Rect2 rect = child.GetGlobalRect();
			Vector2 bottomRight = mapLegend.GetGlobalTransform().AffineInverse() * (rect.Position + rect.Size);
			bottom = Math.Max(bottom, bottomRight.Y);
		}

		return bottom;
	}

	private static IEnumerable<Control> EnumerateDescendantControls(Control root)
	{
		foreach (Node child in root.GetChildren())
		{
			if (child is not Control control)
			{
				continue;
			}

			yield return control;
			foreach (Control descendant in EnumerateDescendantControls(control))
			{
				yield return descendant;
			}
		}
	}

	private static void ResizeAroundCenter(TextureRect rect, Vector2 targetSize)
	{
		Vector2 center = rect.Position + rect.Size * 0.5f;
		rect.Size = targetSize;
		rect.Position = center - targetSize * 0.5f;
		rect.PivotOffset = targetSize * 0.5f;
		rect.Scale = Vector2.One;
	}

	private sealed class LegendPaperLayoutState
	{
		public LegendPaperLayoutState(Control mapLegend)
		{
			OriginalPosition = mapLegend.Position;
			OriginalSize = mapLegend.Size;
			OriginalCustomMinimumSize = mapLegend.CustomMinimumSize;
			OriginalChildPositions = mapLegend.GetChildren()
				.OfType<Control>()
				.ToDictionary(static child => child, static child => child.Position);
		}

		public Vector2 OriginalPosition { get; }
		public Vector2 OriginalSize { get; }
		public Vector2 OriginalCustomMinimumSize { get; }
		public Dictionary<Control, Vector2> OriginalChildPositions { get; }
	}
}

[HarmonyPatch(typeof(NMapScreen), nameof(NMapScreen._Ready))]
internal static class IntegratedStrategySecretMapLegendReadyPatch
{
	private static void Postfix(NMapScreen __instance)
	{
		IntegratedStrategySecretMapLegendController.EnsureSecretLegendItem(__instance);
	}
}

[HarmonyPatch(typeof(NMapScreen), nameof(NMapScreen.Open))]
internal static class IntegratedStrategySecretMapLegendOpenPatch
{
	private static void Postfix(NMapScreen __instance)
	{
		IntegratedStrategySecretMapLegendController.RefreshLegendPaper(__instance);
	}
}

[HarmonyPatch(typeof(NMapLegendItem), "SetMapPointType")]
internal static class IntegratedStrategySecretMapLegendTypePatch
{
	private static bool Prefix(NMapLegendItem __instance, string name)
	{
		return !IntegratedStrategySecretMapLegendController.TrySetSecretLegendPointType(__instance, name);
	}
}

[HarmonyPatch(typeof(NMapLegendItem), "SetLocalizedFields")]
internal static class IntegratedStrategySecretMapLegendLocalizationPatch
{
	private static bool Prefix(NMapLegendItem __instance, string name)
	{
		return !IntegratedStrategySecretMapLegendController.TrySetSecretLegendLocalization(__instance, name);
	}
}

[HarmonyPatch(typeof(NNormalMapPoint), "OnHighlightPointType")]
internal static class IntegratedStrategySecretMapNodeHighlightPatch
{
	private static bool Prefix(NNormalMapPoint __instance, MapPointType pointType)
	{
		return !IntegratedStrategySecretMapLegendController.TryHandleSecretHighlight(__instance, pointType);
	}
}
