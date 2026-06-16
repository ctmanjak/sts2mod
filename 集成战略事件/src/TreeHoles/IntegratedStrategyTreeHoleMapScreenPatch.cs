using System.Collections;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Runs;

namespace IntegratedStrategyEvents.TreeHoles;

[HarmonyPatch(typeof(NMapScreen), nameof(NMapScreen.SetMap))]
internal static class IntegratedStrategyTreeHoleMapScreenPatch
{
	private static void Postfix(NMapScreen __instance, ActMap map)
	{
		IntegratedStrategyTreeHoleController.TryRestoreSavedSessionForCurrentRun(map);

		if (IntegratedStrategyTreeHoleController.TryRestoreCompletedCurrentRun())
		{
			return;
		}

		if (map is IntegratedStrategyTreeHoleActMap ||
			IntegratedStrategyTreeHoleController.IsCurrentTreeHoleMap(map))
		{
			HideSpecialPoint(__instance, "_startingPointNode");
			HideSpecialPoint(__instance, "_bossPointNode");
			HideSpecialPaths(__instance, map.StartingMapPoint.coord, map.BossMapPoint.coord);
			return;
		}

		if (map is IntegratedStrategyEndlessFinaleActMap ||
			IntegratedStrategyTreeHoleController.IsCurrentEndlessFinaleMap(map))
		{
			HideSpecialPoint(__instance, "_startingPointNode");
			HideSpecialPaths(__instance, map.StartingMapPoint.coord, map.BossMapPoint.coord);
			StyleEndlessFinaleBossNode(__instance);
			return;
		}

		if (map is IntegratedStrategyEternalDustFinaleActMap ||
			IntegratedStrategyTreeHoleController.IsCurrentEternalDustFinaleMap(map))
		{
			HideSpecialPoint(__instance, "_startingPointNode");
			HideSpecialPaths(__instance, map.StartingMapPoint.coord);
			return;
		}

		if (map is IntegratedStrategyRadiantApexFinaleActMap ||
			IntegratedStrategyTreeHoleController.IsCurrentRadiantApexFinaleMap(map))
		{
			HideSpecialPoint(__instance, "_startingPointNode");
			HideSpecialPaths(__instance, map.StartingMapPoint.coord);
		}
	}

	private static void StyleEndlessFinaleBossNode(NMapScreen screen)
	{
		if (AccessTools.Field(typeof(NMapScreen), "_bossPointNode")?.GetValue(screen) is not NBossMapPoint bossPoint)
		{
			return;
		}

		bossPoint.Position = new Vector2(760f, -520f);
		bossPoint.Scale = Vector2.One * 2.5f;
		bossPoint.ZIndex = 0;
	}

	private static void HideSpecialPoint(NMapScreen screen, string fieldName)
	{
		if (AccessTools.Field(typeof(NMapScreen), fieldName)?.GetValue(screen) is CanvasItem item)
		{
			item.Hide();
			item.ProcessMode = Node.ProcessModeEnum.Disabled;
		}
	}

	private static void HideSpecialPaths(NMapScreen screen, params MapCoord[] hiddenCoords)
	{
		object? pathsValue = AccessTools.Field(typeof(NMapScreen), "_paths")?.GetValue(screen);
		if (pathsValue is not IDictionary paths)
		{
			return;
		}

		foreach (DictionaryEntry entry in paths)
		{
			if (entry.Key is not ValueTuple<MapCoord, MapCoord> key ||
				(!IsSpecialCoord(key.Item1, hiddenCoords) &&
				 !IsSpecialCoord(key.Item2, hiddenCoords)))
			{
				continue;
			}

			if (entry.Value is not IEnumerable<TextureRect> pathTextures)
			{
				continue;
			}

			foreach (TextureRect pathTexture in pathTextures)
			{
				pathTexture.Hide();
			}
		}
	}

	private static bool IsSpecialCoord(MapCoord coord, IReadOnlyList<MapCoord> hiddenCoords)
	{
		return hiddenCoords.Any(hiddenCoord => coord.Equals(hiddenCoord));
	}
}

[HarmonyPatch(typeof(NMapScreen), nameof(NMapScreen.Open))]
internal static class IntegratedStrategyTreeHoleMapOpenPatch
{
	private static void Postfix()
	{
		if (RunManager.Instance.DebugOnlyGetState()?.Map is { } map)
		{
			IntegratedStrategyTreeHoleController.TryRestoreSavedSessionForCurrentRun(map);
		}

		IntegratedStrategyTreeHoleController.TryRestoreCompletedCurrentRun();
	}
}

[HarmonyPatch(typeof(NMapScreen), "InitMapPrompt")]
internal static class IntegratedStrategyTreeHoleMapPromptPatch
{
	private static void Postfix(NMapScreen __instance)
	{
		if (!IntegratedStrategyTreeHoleController.TryGetCurrentDestination(out string stageLabel, out string actName))
		{
			return;
		}

		ApplyPromptText(__instance, $"{stageLabel}{actName}");
	}

	private static void ApplyPromptText(Node node, string text)
	{
		if (IsPromptNode(node))
		{
			switch (node)
			{
				case MegaRichTextLabel richTextLabel:
					richTextLabel.SetTextAutoSize(text);
					break;
				case MegaLabel megaLabel:
					megaLabel.SetTextAutoSize(text);
					break;
				case Label label:
					label.Text = text;
					break;
			}
		}

		foreach (Node child in node.GetChildren())
		{
			ApplyPromptText(child, text);
		}
	}

	private static bool IsPromptNode(Node node)
	{
		string name = node.Name.ToString();
		return name.Contains("Prompt", StringComparison.OrdinalIgnoreCase) &&
			(node is Label || node is MegaLabel || node is MegaRichTextLabel);
	}
}

[HarmonyPatch(typeof(NActBanner), nameof(NActBanner.Create))]
internal static class IntegratedStrategyTreeHoleActBannerPatch
{
	private static void Postfix(NActBanner __result)
	{
		IntegratedStrategyTreeHoleActBannerText.Apply(__result);
	}
}

[HarmonyPatch(typeof(NActBanner), nameof(NActBanner._Ready))]
internal static class IntegratedStrategyTreeHoleActBannerReadyPatch
{
	private static void Postfix(NActBanner __instance)
	{
		IntegratedStrategyTreeHoleActBannerText.Apply(__instance);
	}
}

internal static class IntegratedStrategyTreeHoleActBannerText
{
	public static void Apply(NActBanner banner)
	{
		if (!IntegratedStrategyTreeHoleController.TryGetCurrentDestination(out string stageLabel, out string actName))
		{
			return;
		}

		SetMegaLabelField(banner, "_actNumber", stageLabel);
		SetMegaLabelField(banner, "_actName", actName);
	}

	private static void SetMegaLabelField(NActBanner banner, string fieldName, string text)
	{
		if (AccessTools.Field(typeof(NActBanner), fieldName)?.GetValue(banner) is MegaLabel label)
		{
			label.SetTextAutoSize(text);
		}
	}
}
