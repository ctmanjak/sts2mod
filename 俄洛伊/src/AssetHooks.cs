using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Localization.Formatters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Audio;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Relics;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;
using SmartFormat.Core.Extensions;

namespace Illaoi;

internal static class AssetHooks
{
	private static readonly Dictionary<string, Texture2D> TextureCache = new();
	private static readonly Dictionary<string, AudioStream> AudioCache = new();
	private static AudioStreamPlayer? CharacterSelectSfxPlayer;
	private static Material? IllaoiFrameMaterial;
	[ThreadStatic]
	private static bool FormattingIllaoiCardEnergyIcons;
	private static bool IllaoiRunEnergyIconsActive;
	private static bool AllowIllaoiNewRunVoice;
	private static bool Installed;

	private static readonly FieldInfo NCardEnergyIconField = RequireField(typeof(NCard), "_energyIcon");
	private static readonly FieldInfo NCardUnplayableEnergyIconField = RequireField(typeof(NCard), "_unplayableEnergyIcon");
	private static readonly FieldInfo NRelicModelField = RequireField(typeof(NRelic), "_model");
	private static readonly FieldInfo CombatPowerModelField = RequireField(typeof(NPower), "_model");
	private static readonly FieldInfo CombatPowerIconField = RequireField(typeof(NPower), "_icon");
	private static readonly FieldInfo CombatPowerFlashField = RequireField(typeof(NPower), "_powerFlash");
	private static readonly FieldInfo CharacterSelectButtonIconField = RequireField(typeof(NCharacterSelectButton), "_icon");
	private static readonly FieldInfo CharacterSelectButtonIconAddField = RequireField(typeof(NCharacterSelectButton), "_iconAdd");
	private static readonly FieldInfo CharacterSelectScreenBgContainerField = RequireField(typeof(NCharacterSelectScreen), "_bgContainer");
	private static readonly FieldInfo NEnergyCounterPlayerField = RequireField(typeof(NEnergyCounter), "_player");
	private static readonly FieldInfo CardLibraryPoolFiltersField = RequireField(typeof(NCardLibrary), "_poolFilters");
	private static readonly FieldInfo CardLibraryCardPoolFiltersField = RequireField(typeof(NCardLibrary), "_cardPoolFilters");
	private static readonly FieldInfo CardLibraryMiscPoolFilterField = RequireField(typeof(NCardLibrary), "_miscPoolFilter");
	private static readonly FieldInfo CardLibraryLastHoveredControlField = RequireField(typeof(NCardLibrary), "_lastHoveredControl");
	private static readonly MethodInfo CardLibraryUpdateCardPoolFilterMethod = RequireMethod(typeof(NCardLibrary), "UpdateCardPoolFilter", BindingFlags.Instance | BindingFlags.NonPublic, typeof(NCardPoolFilter));

	private const string CharacterSelectArtNodeName = "IllaoiCharacterSelectIllustration";
	private const string CombatArtNodeName = "IllaoiCombatIllustration";
	private const string TentacleArtNodeName = "IllaoiTentacleIllustration";
	private const string EnergyCounterIconNodeName = "IllaoiEnergyCounterIcon";
	private const string CardLibraryPoolFilterNodeName = "IllaoiPool";
	private const string IllaoiAudioRoot = "res://Illaoi/audio/";
	private const string IllaoiEnergyIconTag = "[img]" + ModInfo.EnergySpriteFontIconPath + "[/img]";
	private const float CharacterSelectSfxVolumeScale = 0.65f;

	public static void Install(Harmony harmony)
	{
		if (Installed)
		{
			return;
		}

		Installed = true;
		PatchPostfix(harmony, RequireGetter(typeof(CardModel), nameof(CardModel.Description)), nameof(CardDescriptionPostfix));
		PatchPostfix(harmony, RequireGetter(typeof(CardModel), nameof(CardModel.Portrait)), nameof(CardPortraitPostfix));
		PatchPostfix(harmony, RequireGetter(typeof(CardModel), nameof(CardModel.EnergyIcon)), nameof(CardEnergyIconPostfix));
		PatchPostfix(harmony, RequireGetter(typeof(CardModel), nameof(CardModel.RunAssetPaths)), nameof(CardRunAssetPathsPostfix));
		PatchPostfix(harmony, RequireGetter(typeof(CardPoolModel), nameof(CardPoolModel.FrameMaterial)), nameof(CardPoolFrameMaterialPostfix));
		PatchPostfix(harmony, RequireMethod(typeof(NCard), "Reload", BindingFlags.Instance | BindingFlags.NonPublic), nameof(NCardReloadPostfix));
		PatchPrefix(harmony, RequireMethod(typeof(NCard), nameof(NCard.UpdateVisuals), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, typeof(PileType), typeof(CardPreviewMode)), nameof(NCardUpdateVisualsPrefix), Priority.First);
		PatchPostfix(harmony, RequireMethod(typeof(NCard), nameof(NCard.UpdateVisuals), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, typeof(PileType), typeof(CardPreviewMode)), nameof(NCardUpdateVisualsPostfix));
		PatchPrefix(harmony, RequireMethod(typeof(EnergyIconsFormatter), nameof(EnergyIconsFormatter.TryEvaluateFormat), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, typeof(IFormattingInfo)), nameof(EnergyIconsFormatterTryEvaluateFormatPrefix), Priority.First);
		PatchPostfix(harmony, RequireMethod(typeof(NHoverTipSet), "Init", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, typeof(Control), typeof(IEnumerable<IHoverTip>)), nameof(NHoverTipSetInitPostfix));
		PatchPrefix(harmony, RequireGetter(typeof(RelicModel), nameof(RelicModel.Icon)), nameof(RelicIconPrefix));
		PatchPrefix(harmony, RequireGetter(typeof(RelicModel), nameof(RelicModel.IconOutline)), nameof(RelicIconOutlinePrefix));
		PatchPrefix(harmony, RequireGetter(typeof(RelicModel), nameof(RelicModel.BigIcon)), nameof(RelicBigIconPrefix));
		PatchPrefix(harmony, RequireMethod(typeof(NRelic), "Reload", BindingFlags.Instance | BindingFlags.NonPublic), nameof(NRelicReloadPrefix));
		PatchPostfix(harmony, RequireGetter(typeof(PowerModel), nameof(PowerModel.Icon)), nameof(PowerIconPostfix));
		PatchPostfix(harmony, RequireGetter(typeof(PowerModel), nameof(PowerModel.BigIcon)), nameof(PowerBigIconPostfix));
		PatchPostfix(harmony, RequireMethod(typeof(NPower), "Reload", BindingFlags.Instance | BindingFlags.NonPublic), nameof(CombatPowerReloadPostfix));
		Patch(harmony, RequireMethod(typeof(MonsterModel), nameof(MonsterModel.CreateVisuals), BindingFlags.Instance | BindingFlags.Public), nameof(MonsterCreateVisualsPrefix), nameof(MonsterCreateVisualsPostfix));
		PatchPostfix(harmony, RequireMethod(typeof(NCreatureVisuals), nameof(NCreatureVisuals._Ready), BindingFlags.Instance | BindingFlags.Public), nameof(NCreatureVisualsReadyPostfix));
		PatchPostfix(harmony, RequireMethod(typeof(CharacterModel), nameof(CharacterModel.CreateVisuals), BindingFlags.Instance | BindingFlags.Public), nameof(CharacterCreateVisualsPostfix));
		PatchPrefix(harmony, RequireGetter(typeof(CharacterModel), "VisualsPath"), nameof(CharacterVisualsPathPrefix));
		PatchPrefix(harmony, RequireGetter(typeof(CharacterModel), "TrailPath"), nameof(CharacterTrailPathPrefix));
		PatchPrefix(harmony, RequireGetter(typeof(CharacterModel), nameof(CharacterModel.AssetPaths)), nameof(CharacterAssetPathsPrefix));
		PatchPrefix(harmony, RequireGetter(typeof(CharacterModel), nameof(CharacterModel.AssetPathsCharacterSelect)), nameof(CharacterSelectAssetPathsPrefix));
		PatchPrefix(harmony, RequireGetter(typeof(CharacterModel), nameof(CharacterModel.CharacterSelectBg)), nameof(CharacterSelectBgPrefix));
		PatchPrefix(harmony, RequireGetter(typeof(CharacterModel), nameof(CharacterModel.CharacterSelectIcon)), nameof(CharacterSelectIconPrefix));
		PatchPrefix(harmony, RequireGetter(typeof(CharacterModel), nameof(CharacterModel.CharacterSelectLockedIcon)), nameof(CharacterSelectLockedIconPrefix));
		PatchPrefix(harmony, RequireGetter(typeof(CharacterModel), nameof(CharacterModel.CharacterSelectTransitionPath)), nameof(CharacterSelectTransitionPathPrefix));
		PatchPrefix(harmony, RequireGetter(typeof(CharacterModel), "AttackSfx"), nameof(CharacterAttackSfxPrefix));
		PatchPrefix(harmony, RequireGetter(typeof(CharacterModel), "CastSfx"), nameof(CharacterCastSfxPrefix));
		PatchPrefix(harmony, RequireGetter(typeof(CharacterModel), "DeathSfx"), nameof(CharacterDeathSfxPrefix));
		PatchPrefix(harmony, RequireGetter(typeof(CharacterModel), nameof(CharacterModel.EnergyCounterPath)), nameof(CharacterEnergyCounterPathPrefix));
		PatchPrefix(harmony, RequireGetter(typeof(CharacterModel), nameof(CharacterModel.MerchantAnimPath)), nameof(CharacterMerchantAnimPathPrefix));
		PatchPrefix(harmony, RequireGetter(typeof(CharacterModel), nameof(CharacterModel.RestSiteAnimPath)), nameof(CharacterRestSiteAnimPathPrefix));
		PatchPrefix(harmony, RequireGetter(typeof(CharacterModel), "ArmPointingTexturePath"), nameof(CharacterArmPointingTexturePathPrefix));
		PatchPrefix(harmony, RequireGetter(typeof(CharacterModel), "ArmRockTexturePath"), nameof(CharacterArmRockTexturePathPrefix));
		PatchPrefix(harmony, RequireGetter(typeof(CharacterModel), "ArmPaperTexturePath"), nameof(CharacterArmPaperTexturePathPrefix));
		PatchPrefix(harmony, RequireGetter(typeof(CharacterModel), "ArmScissorsTexturePath"), nameof(CharacterArmScissorsTexturePathPrefix));
		PatchPrefix(harmony, RequireGetter(typeof(CharacterModel), nameof(CharacterModel.IconTexture)), nameof(CharacterIconTexturePrefix));
		PatchPrefix(harmony, RequireGetter(typeof(CharacterModel), nameof(CharacterModel.IconOutlineTexture)), nameof(CharacterIconOutlineTexturePrefix));
		PatchPrefix(harmony, RequireGetter(typeof(CharacterModel), nameof(CharacterModel.Icon)), nameof(CharacterIconPrefix));
		PatchPostfix(harmony, RequireMethod(typeof(NCharacterSelectButton), nameof(NCharacterSelectButton.Init), BindingFlags.Instance | BindingFlags.Public, typeof(CharacterModel), typeof(ICharacterSelectButtonDelegate)), nameof(NCharacterSelectButtonInitPostfix));
		PatchPostfix(harmony, RequireMethod(typeof(NCharacterSelectScreen), nameof(NCharacterSelectScreen.SelectCharacter), BindingFlags.Instance | BindingFlags.Public, typeof(NCharacterSelectButton), typeof(CharacterModel)), nameof(NCharacterSelectScreenSelectCharacterPostfix));
		PatchPostfix(harmony, RequireMethod(typeof(NMapMarker), nameof(NMapMarker.Initialize), BindingFlags.Instance | BindingFlags.Public, typeof(Player)), nameof(NMapMarkerInitializePostfix));
		PatchPostfix(harmony, RequireMethod(typeof(NEnergyCounter), nameof(NEnergyCounter._Ready), BindingFlags.Instance | BindingFlags.Public), nameof(NEnergyCounterReadyPostfix));
		PatchPostfix(harmony, RequireMethod(typeof(NCombatRoom), nameof(NCombatRoom.AddCreature), BindingFlags.Instance | BindingFlags.Public, typeof(Creature)), nameof(NCombatRoomAddCreaturePostfix));
		PatchPostfix(harmony, RequireMethod(typeof(NCardLibrary), nameof(NCardLibrary._Ready), BindingFlags.Instance | BindingFlags.Public), nameof(NCardLibraryReadyPostfix));
		PatchPrefix(harmony, RequireMethod(typeof(SfxCmd), nameof(SfxCmd.Play), BindingFlags.Static | BindingFlags.Public, typeof(string), typeof(float)), nameof(SfxCmdPlayPrefix), Priority.First);
		PatchPrefix(harmony, RequireMethod(typeof(SfxCmd), nameof(SfxCmd.Play), BindingFlags.Static | BindingFlags.Public, typeof(string), typeof(string), typeof(float), typeof(float)), nameof(SfxCmdPlayWithParameterPrefix), Priority.First);
		PatchPrefix(harmony, RequireMethod(typeof(SfxCmd), nameof(SfxCmd.PlayLoop), BindingFlags.Static | BindingFlags.Public, typeof(string), typeof(bool)), nameof(SfxCmdPlayLoopPrefix), Priority.First);
		PatchPrefix(harmony, RequireMethod(typeof(NAudioManager), nameof(NAudioManager.PlayOneShot), BindingFlags.Instance | BindingFlags.Public, typeof(string), typeof(float)), nameof(NAudioManagerPlayOneShotPrefix), Priority.First);
		PatchPrefix(harmony, RequireMethod(typeof(NAudioManager), nameof(NAudioManager.PlayOneShot), BindingFlags.Instance | BindingFlags.Public, typeof(string), typeof(Dictionary<string, float>), typeof(float)), nameof(NAudioManagerPlayOneShotWithParametersPrefix), Priority.First);
		PatchPrefix(harmony, RequireMethod(typeof(NAudioManager), nameof(NAudioManager.PlayLoop), BindingFlags.Instance | BindingFlags.Public, typeof(string), typeof(bool)), nameof(NAudioManagerPlayLoopPrefix), Priority.First);
	}

	private static void CardDescriptionPostfix(CardModel __instance, ref LocString __result)
	{
		if (__instance is RendSoul { IsUpgraded: true })
		{
			__result = new LocString("cards", "REND_SOUL.upgradedDescription");
		}
	}

	private static void CardPortraitPostfix(CardModel __instance, ref Texture2D __result)
	{
		string? path = __instance switch
		{
			IllaoiStrike => ModInfo.IllaoiStrikePortraitPath,
			IllaoiDefend => ModInfo.IllaoiDefendPortraitPath,
			SoulTrial => ModInfo.SoulTrialPortraitPath,
			TempleIdol => ModInfo.TempleIdolPortraitPath,
			BuhruFootwork => ModInfo.BuhruFootworkPortraitPath,
			SermonOfMotion => ModInfo.SermonOfMotionPortraitPath,
			LowSweep => ModInfo.LowSweepPortraitPath,
			GuardedAdvance => ModInfo.GuardedAdvancePortraitPath,
			TidalSlash => ModInfo.TidalSlashPortraitPath,
			RestlessCurrent => ModInfo.RestlessCurrentPortraitPath,
			WatchfulIdol => ModInfo.WatchfulIdolPortraitPath,
			SpiritMark => ModInfo.SpiritMarkPortraitPath,
			GraspingLesson => ModInfo.GraspingLessonPortraitPath,
			ConsecratedGround => ModInfo.ConsecratedGroundPortraitPath,
			HarshSermon => ModInfo.HarshSermonPortraitPath,
			FollowMyVoice => ModInfo.FollowMyVoicePortraitPath,
			BuhruMeditation => ModInfo.BuhruMeditationPortraitPath,
			CrushingWave => ModInfo.CrushingWavePortraitPath,
			TentacleSlam => ModInfo.TentacleSlamPortraitPath,
			PriestessGuard => ModInfo.PriestessGuardPortraitPath,
			IdolRecall => ModInfo.IdolRecallPortraitPath,
			TrialDance => ModInfo.TrialDancePortraitPath,
			GuardBreakingWave => ModInfo.GuardBreakingWavePortraitPath,
			SurgingSermon => ModInfo.SurgingSermonPortraitPath,
			LineBreaker => ModInfo.LineBreakerPortraitPath,
			DeepMeditation => ModInfo.DeepMeditationPortraitPath,
			RhythmOfMotion => ModInfo.RhythmOfMotionPortraitPath,
			FervorOfMotion => ModInfo.FervorOfMotionPortraitPath,
			WoundedVessel => ModInfo.WoundedVesselPortraitPath,
			IdolWard => ModInfo.IdolWardPortraitPath,
			VesselCrack => ModInfo.VesselCrackPortraitPath,
			SpiritLash => ModInfo.SpiritLashPortraitPath,
			MotionOfNagakabouros => ModInfo.MotionOfNagakabourosPortraitPath,
			OversteppingFaith => ModInfo.OversteppingFaithPortraitPath,
			SpiritualPreparation => ModInfo.SpiritualPreparationPortraitPath,
			ProphetOfNagakabouros => ModInfo.ProphetOfNagakabourosPortraitPath,
			LeapOfFaith => ModInfo.LeapOfFaithPortraitPath,
			Tidecaller => ModInfo.TidecallerPortraitPath,
			Drain => ModInfo.DrainPortraitPath,
			RelentlessFaith => ModInfo.RelentlessFaithPortraitPath,
			KrakenPriestess => ModInfo.KrakenPriestessPortraitPath,
			HarrowingSermon => ModInfo.HarrowingSermonPortraitPath,
			NagakabourosRising => ModInfo.NagakabourosRisingPortraitPath,
			AncientGodProphet => ModInfo.AncientGodProphetPortraitPath,
			SoulImpact => ModInfo.SoulImpactPortraitPath,
			TrialByMotion => ModInfo.TrialByMotionPortraitPath,
			SerpentDance => ModInfo.SerpentDancePortraitPath,
			Undertow => ModInfo.UndertowPortraitPath,
			VoiceOfTheDeep => ModInfo.VoiceOfTheDeepPortraitPath,
			TheSeaAnswers => ModInfo.TheSeaAnswersPortraitPath,
			DivineForm => ModInfo.DivineFormPortraitPath,
			RagingTide => ModInfo.RagingTidePortraitPath,
			RendSoul => ModInfo.RendSoulPortraitPath,
			TrialOfTheAncientGod => ModInfo.TrialOfTheAncientGodPortraitPath,
			NagakabourosDescends => ModInfo.NagakabourosDescendsPortraitPath,
			_ => null
		};

		if (path != null && LoadPortableTexture(path) is { } texture)
		{
			__result = texture;
		}
	}

	private static void CardEnergyIconPostfix(CardModel __instance, ref Texture2D __result)
	{
		if (IsIllaoiCard(__instance) && LoadPortableTexture(ModInfo.EnergyIconPath) is { } texture)
		{
			__result = texture;
		}
	}

	private static void CardRunAssetPathsPostfix(CardModel __instance, ref IEnumerable<string> __result)
	{
		if (IsIllaoiCard(__instance))
		{
			__result = __result.Append(ModInfo.EnergySpriteFontIconPath).Distinct();
		}
	}

	private static void CardPoolFrameMaterialPostfix(CardPoolModel __instance, ref Material __result)
	{
		if (__instance.Id != ModelDb.GetId<IllaoiCardPool>())
		{
			return;
		}

		if (IllaoiFrameMaterial != null)
		{
			__result = IllaoiFrameMaterial;
			return;
		}

		Material source = __result;
		if (source is not ShaderMaterial shaderMaterial)
		{
			IllaoiFrameMaterial = source;
			return;
		}

		ShaderMaterial frameMaterial = (ShaderMaterial)shaderMaterial.Duplicate(true);
		frameMaterial.SetShaderParameter("h", 0.435f);
		frameMaterial.SetShaderParameter("s", 0.638f);
		frameMaterial.SetShaderParameter("v", 0.682f);
		frameMaterial.SetShaderParameter("ColorParameter", new Color("3FAE83"));
		IllaoiFrameMaterial = frameMaterial;
		__result = frameMaterial;
	}

	private static void NCardReloadPostfix(NCard __instance)
	{
		if (__instance.Model == null || !IsIllaoiCard(__instance.Model) || LoadPortableTexture(ModInfo.EnergyIconPath) is not { } texture)
		{
			return;
		}

		if (NCardEnergyIconField.GetValue(__instance) is TextureRect energyIcon)
		{
			energyIcon.Texture = texture;
		}

		if (NCardUnplayableEnergyIconField.GetValue(__instance) is TextureRect unplayableEnergyIcon)
		{
			unplayableEnergyIcon.Texture = texture;
		}
	}

	private static void NCardUpdateVisualsPrefix(NCard __instance)
	{
		FormattingIllaoiCardEnergyIcons = __instance.Model != null && IsIllaoiCard(__instance.Model);
	}

	private static void NCardUpdateVisualsPostfix()
	{
		FormattingIllaoiCardEnergyIcons = false;
	}

	private static bool EnergyIconsFormatterTryEvaluateFormatPrefix(IFormattingInfo formattingInfo, ref bool __result)
	{
		if (!ShouldUseIllaoiEnergyIcons())
		{
			return true;
		}

		try
		{
			int amount = GetEnergyIconAmount(formattingInfo.CurrentValue, formattingInfo.FormatterOptions);
			formattingInfo.Write(FormatIllaoiEnergyIcons(amount));
			__result = true;
			return false;
		}
		catch
		{
			return true;
		}
	}

	private static bool ShouldUseIllaoiEnergyIcons()
	{
		if (FormattingIllaoiCardEnergyIcons)
		{
			return true;
		}

		IllaoiRunEnergyIconsActive = IsCurrentRunIllaoi();
		return IllaoiRunEnergyIconsActive;
	}

	private static bool IsCurrentRunIllaoi()
	{
		try
		{
			RunManager? runManager = RunManager.Instance;
			if (runManager == null || !runManager.IsInProgress)
			{
				return false;
			}

			IRunState? runState = runManager.DebugOnlyGetState();
			if (runState == null)
			{
				return false;
			}

			Player? localPlayer = LocalContext.GetMe(runState.Players);
			return localPlayer?.Character is IllaoiCustomCharacterModel
				|| localPlayer?.Character.Id == ModelDb.GetId<IllaoiCharacter>();
		}
		catch
		{
			return false;
		}
	}

	private static int GetEnergyIconAmount(object? currentValue, string? formatterOptions)
	{
		if (currentValue is EnergyVar energyVar)
		{
			return Convert.ToInt32(energyVar.PreviewValue);
		}

		if (currentValue is CalculatedVar calculatedVar)
		{
			return Convert.ToInt32(calculatedVar.Calculate(null));
		}

		if (currentValue is decimal dec)
		{
			return (int)dec;
		}

		if (currentValue is int i)
		{
			return i;
		}

		if (currentValue is string str && int.TryParse(str, out int parsedString))
		{
			return parsedString;
		}

		if (int.TryParse(formatterOptions, out int parsedOptions))
		{
			return parsedOptions;
		}

		throw new InvalidOperationException($"Unsupported energy icon value: {currentValue} ({currentValue?.GetType()}).");
	}

	private static string FormatIllaoiEnergyIcons(int amount)
	{
		if (amount > 0 && amount < 4)
		{
			return string.Concat(Enumerable.Repeat(IllaoiEnergyIconTag, amount));
		}

		return $"{amount}{IllaoiEnergyIconTag}";
	}

	private static void NHoverTipSetInitPostfix(NHoverTipSet __instance, Control owner, IEnumerable<IHoverTip> hoverTips)
	{
		try
		{
			if (!ShouldUseIllaoiHoverTipEnergyIcon(owner))
			{
				return;
			}

			Texture2D? replacement = LoadPortableTexture(ModInfo.EnergyIconPath);
			if (replacement == null)
			{
				return;
			}

			Node? textContainer = __instance.GetNodeOrNull("textHoverTipContainer");
			if (textContainer == null)
			{
				return;
			}

			int textTipIndex = 0;
			foreach (IHoverTip hoverTip in IHoverTip.RemoveDupes(hoverTips))
			{
				if (hoverTip is not HoverTip)
				{
					continue;
				}

				Node? child = textContainer.GetChildOrNull<Node>(textTipIndex);
				textTipIndex++;
				if (child == null || !IsEnergyHoverTip(hoverTip))
				{
					continue;
				}

				if (child.GetNodeOrNull<TextureRect>("%Icon") is { } iconNode)
				{
					iconNode.Texture = replacement;
				}
			}
		}
		catch
		{
			// Hover tips are auxiliary UI; do not let a texture replacement break screen creation.
		}
	}

	private static bool ShouldUseIllaoiHoverTipEnergyIcon(Control owner)
	{
		if (owner is NCardHolder holder && holder.CardModel != null && IsIllaoiCard(holder.CardModel))
		{
			return true;
		}

		return ShouldUseIllaoiEnergyIcons();
	}

	private static bool IsEnergyHoverTip(IHoverTip hoverTip)
	{
		return hoverTip.Id != null
			&& hoverTip.Id.Contains("static_hover_tips.ENERGY.title", StringComparison.OrdinalIgnoreCase);
	}

	private static bool RelicIconPrefix(RelicModel __instance, ref Texture2D __result)
	{
		if (!TryGetIllaoiRelicTexture(__instance, out Texture2D? texture))
		{
			return true;
		}

		__result = texture!;
		return false;
	}

	private static bool RelicIconOutlinePrefix(RelicModel __instance, ref Texture2D __result)
	{
		if (!TryGetIllaoiRelicTexture(__instance, out Texture2D? texture))
		{
			return true;
		}

		__result = texture!;
		return false;
	}

	private static bool RelicBigIconPrefix(RelicModel __instance, ref Texture2D __result)
	{
		if (!TryGetIllaoiRelicTexture(__instance, out Texture2D? texture))
		{
			return true;
		}

		__result = texture!;
		return false;
	}

	private static bool NRelicReloadPrefix(NRelic __instance)
	{
		if (!__instance.IsNodeReady()
			|| NRelicModelField.GetValue(__instance) is not RelicModel model
			|| !TryGetIllaoiRelicTexture(model, out Texture2D? texture))
		{
			return true;
		}

		model.UpdateTexture(__instance.Icon);
		__instance.Icon.Texture = texture;
		__instance.Outline.Visible = false;
		return false;
	}

	private static void PowerIconPostfix(PowerModel __instance, ref Texture2D __result)
	{
		if (TryGetIllaoiPowerTexture(__instance, out Texture2D texture))
		{
			__result = texture;
		}
	}

	private static void PowerBigIconPostfix(PowerModel __instance, ref Texture2D __result)
	{
		if (TryGetIllaoiPowerTexture(__instance, out Texture2D texture))
		{
			__result = texture;
		}
	}

	private static void CombatPowerReloadPostfix(NPower __instance)
	{
		if (!__instance.IsNodeReady()
			|| CombatPowerModelField.GetValue(__instance) is not PowerModel model
			|| !TryGetIllaoiPowerTexture(model, out Texture2D texture))
		{
			return;
		}

		((TextureRect)CombatPowerIconField.GetValue(__instance)!).Texture = texture;
		((CpuParticles2D)CombatPowerFlashField.GetValue(__instance)!).Texture = texture;
	}

	private static bool MonsterCreateVisualsPrefix(MonsterModel __instance, ref NCreatureVisuals __result)
	{
		if (__instance is not IllaoiSoulMonster soulMonster)
		{
			return true;
		}

		MonsterModel? sourceModel = soulMonster.ResolveVisualSourceModel();
		if (sourceModel == null)
		{
			Log.Warn($"{ModInfo.LogPrefix} Soul visual source missing for {soulMonster.Id}; using default monster visuals.");
			sourceModel = ModelDb.Monster<Chomper>();
		}

		__result = sourceModel.CreateVisuals();
		ApplySoulVisualStyle(__result);
		return false;
	}

	private static void MonsterCreateVisualsPostfix(MonsterModel __instance, NCreatureVisuals __result)
	{
		if (__instance is IllaoiTentacleMonster)
		{
			ApplyTentacleVisualStyle(__result);
		}
	}

	private static void NCreatureVisualsReadyPostfix(NCreatureVisuals __instance)
	{
		if (__instance.GetNodeOrNull<Node>(CombatArtNodeName) != null)
		{
			RefreshCombatIllustration(__instance);
		}
	}

	private static void CharacterCreateVisualsPostfix(CharacterModel __instance, NCreatureVisuals __result)
	{
		if (IsIllaoiCharacter(__instance))
		{
			ApplyCombatIllustration(__result);
		}
	}

	private static bool CharacterVisualsPathPrefix(CharacterModel __instance, ref string __result)
	{
		return TryUseCustomCharacterPath(__instance, ref __result, character => character.CustomVisualPath);
	}

	private static bool CharacterTrailPathPrefix(CharacterModel __instance, ref string __result)
	{
		return TryUseCustomCharacterPath(__instance, ref __result, character => character.CustomTrailPath);
	}

	private static bool CharacterAssetPathsPrefix(CharacterModel __instance, ref IEnumerable<string> __result)
	{
		if (__instance is not IllaoiCustomCharacterModel character)
		{
			return true;
		}

		__result = character.AllCustomAssetPaths;
		return false;
	}

	private static bool CharacterSelectAssetPathsPrefix(CharacterModel __instance, ref IEnumerable<string> __result)
	{
		if (__instance is not IllaoiCustomCharacterModel character)
		{
			return true;
		}

		__result = character.AllCustomCharacterSelectAssetPaths;
		return false;
	}

	private static bool CharacterSelectBgPrefix(CharacterModel __instance, ref string __result)
	{
		return TryUseCustomCharacterPath(__instance, ref __result, character => character.CustomCharacterSelectBg);
	}

	private static bool CharacterSelectIconPrefix(CharacterModel __instance, ref CompressedTexture2D __result)
	{
		return TryUseCustomCompressedTexture(__instance, ref __result, character => character.CustomCharacterSelectIconPath);
	}

	private static bool CharacterSelectLockedIconPrefix(CharacterModel __instance, ref CompressedTexture2D __result)
	{
		return TryUseCustomCompressedTexture(__instance, ref __result, character => character.CustomCharacterSelectLockedIconPath);
	}

	private static bool CharacterSelectTransitionPathPrefix(CharacterModel __instance, ref string __result)
	{
		return TryUseCustomCharacterPath(__instance, ref __result, character => character.CustomCharacterSelectTransitionPath);
	}

	private static bool CharacterAttackSfxPrefix(CharacterModel __instance, ref string __result)
	{
		return TryUseCustomCharacterPath(__instance, ref __result, character => character.CustomAttackSfx);
	}

	private static bool CharacterCastSfxPrefix(CharacterModel __instance, ref string __result)
	{
		return TryUseCustomCharacterPath(__instance, ref __result, character => character.CustomCastSfx);
	}

	private static bool CharacterDeathSfxPrefix(CharacterModel __instance, ref string __result)
	{
		return TryUseCustomCharacterPath(__instance, ref __result, character => character.CustomDeathSfx);
	}

	private static bool CharacterEnergyCounterPathPrefix(CharacterModel __instance, ref string __result)
	{
		return TryUseCustomCharacterPath(__instance, ref __result, character => character.CustomEnergyCounterPath);
	}

	private static bool CharacterMerchantAnimPathPrefix(CharacterModel __instance, ref string __result)
	{
		return TryUseCustomCharacterPath(__instance, ref __result, character => character.CustomMerchantAnimPath);
	}

	private static bool CharacterRestSiteAnimPathPrefix(CharacterModel __instance, ref string __result)
	{
		return TryUseCustomCharacterPath(__instance, ref __result, character => character.CustomRestSiteAnimPath);
	}

	private static bool CharacterArmPointingTexturePathPrefix(CharacterModel __instance, ref string __result)
	{
		return TryUseCustomCharacterPath(__instance, ref __result, character => character.CustomArmPointingTexturePath);
	}

	private static bool CharacterArmRockTexturePathPrefix(CharacterModel __instance, ref string __result)
	{
		return TryUseCustomCharacterPath(__instance, ref __result, character => character.CustomArmRockTexturePath);
	}

	private static bool CharacterArmPaperTexturePathPrefix(CharacterModel __instance, ref string __result)
	{
		return TryUseCustomCharacterPath(__instance, ref __result, character => character.CustomArmPaperTexturePath);
	}

	private static bool CharacterArmScissorsTexturePathPrefix(CharacterModel __instance, ref string __result)
	{
		return TryUseCustomCharacterPath(__instance, ref __result, character => character.CustomArmScissorsTexturePath);
	}

	private static bool CharacterIconTexturePrefix(CharacterModel __instance, ref Texture2D __result)
	{
		if (__instance is not IllaoiCustomCharacterModel character
			|| string.IsNullOrWhiteSpace(character.CustomIconTexturePath)
			|| LoadPortableTexture(character.CustomIconTexturePath) is not { } texture)
		{
			return true;
		}

		__result = texture;
		return false;
	}

	private static bool CharacterIconOutlineTexturePrefix(CharacterModel __instance, ref Texture2D __result)
	{
		if (__instance is not IllaoiCustomCharacterModel character
			|| string.IsNullOrWhiteSpace(character.CustomIconOutlineTexturePath)
			|| LoadPortableTexture(character.CustomIconOutlineTexturePath) is not { } texture)
		{
			return true;
		}

		__result = texture;
		return false;
	}

	private static bool CharacterIconPrefix(CharacterModel __instance, ref Control __result)
	{
		if (__instance is not IllaoiCustomCharacterModel character
			|| string.IsNullOrWhiteSpace(character.CustomIconTexturePath)
			|| LoadPortableTexture(character.CustomIconTexturePath) is not { } texture)
		{
			return true;
		}

		__result = CreateTopPanelIcon(texture);
		return false;
	}

	private static void NCharacterSelectButtonInitPostfix(NCharacterSelectButton __instance, CharacterModel __0)
	{
		if (!IsIllaoiCharacter(__0) || LoadPortableTexture(ModInfo.CharacterSelectButtonIconPath) is not { } texture)
		{
			return;
		}

		if (CharacterSelectButtonIconField.GetValue(__instance) is TextureRect icon)
		{
			ApplyCharacterSelectTexture(icon, texture, TextureRect.StretchModeEnum.KeepAspectCentered);
		}

		if (CharacterSelectButtonIconAddField.GetValue(__instance) is TextureRect iconAdd)
		{
			ApplyCharacterSelectTexture(iconAdd, texture, TextureRect.StretchModeEnum.KeepAspectCentered);
		}
	}

	private static void NCharacterSelectScreenSelectCharacterPostfix(NCharacterSelectScreen __instance, CharacterModel __1)
	{
		ApplyCharacterSelectIllustration(__instance, __1);
	}

	private static bool SfxCmdPlayPrefix(string sfx, float volume)
	{
		if (!IsIllaoiLocalAudioPath(sfx))
		{
			return true;
		}

		if (ShouldSuppressIllaoiLocalAudio(sfx))
		{
			return false;
		}

		PlayIllaoiAudio(sfx, volume);
		return false;
	}

	private static bool SfxCmdPlayWithParameterPrefix(string sfx, string param, float val, float volume)
	{
		if (!IsIllaoiLocalAudioPath(sfx))
		{
			return true;
		}

		if (ShouldSuppressIllaoiLocalAudio(sfx))
		{
			return false;
		}

		PlayIllaoiAudio(sfx, volume);
		return false;
	}

	private static bool SfxCmdPlayLoopPrefix(string __0, bool __1)
	{
		if (!IsIllaoiLocalAudioPath(__0))
		{
			return true;
		}

		if (ShouldSuppressIllaoiLocalAudio(__0))
		{
			return false;
		}

		PlayIllaoiAudio(__0, 1f);
		return false;
	}

	private static bool NAudioManagerPlayOneShotPrefix(string __0, float __1)
	{
		if (!IsIllaoiLocalAudioPath(__0))
		{
			return true;
		}

		if (ShouldSuppressIllaoiLocalAudio(__0))
		{
			return false;
		}

		PlayIllaoiAudio(__0, __1);
		return false;
	}

	private static bool NAudioManagerPlayOneShotWithParametersPrefix(string __0, float __2)
	{
		if (!IsIllaoiLocalAudioPath(__0))
		{
			return true;
		}

		if (ShouldSuppressIllaoiLocalAudio(__0))
		{
			return false;
		}

		PlayIllaoiAudio(__0, __2);
		return false;
	}

	private static bool NAudioManagerPlayLoopPrefix(string __0, bool __1)
	{
		if (!IsIllaoiLocalAudioPath(__0))
		{
			return true;
		}

		if (ShouldSuppressIllaoiLocalAudio(__0))
		{
			return false;
		}

		PlayIllaoiAudio(__0, 1f);
		return false;
	}

	private static void NMapMarkerInitializePostfix(NMapMarker __instance, Player __0)
	{
		if (__0.Character is IllaoiCustomCharacterModel character
			&& !string.IsNullOrWhiteSpace(character.CustomMapMarkerPath)
			&& LoadPortableTexture(character.CustomMapMarkerPath) is { } texture)
		{
			__instance.Texture = texture;
		}
	}

	private static void NEnergyCounterReadyPostfix(NEnergyCounter __instance)
	{
		if (NEnergyCounterPlayerField.GetValue(__instance) is not Player player)
		{
			return;
		}

		IllaoiRunEnergyIconsActive = player.Character is IllaoiCustomCharacterModel
			|| player.Character.Id == ModelDb.GetId<IllaoiCharacter>();

		if (player.Character is not IllaoiCustomCharacterModel character
			|| string.IsNullOrWhiteSpace(character.CustomEnergyCounterIconPath)
			|| LoadPortableTexture(character.CustomEnergyCounterIconPath) is not { } texture)
		{
			return;
		}

		ApplyEnergyCounterIcon(__instance, texture);
	}

	private static void NCombatRoomAddCreaturePostfix(Creature __0)
	{
		if (__0.Monster is IllaoiTentacleMonster)
		{
			IllaoiCombatVisuals.PositionTentacle(__0);
		}
		else if (__0.Monster is IllaoiSoulMonster && __0.GetPower<IllaoiSoulLinkPower>()?.Target is { } body)
		{
			__0.GetPower<IllaoiSoulLinkPower>()?.EnsureDeathCleanupRegistered();
			IllaoiCombatVisuals.PositionSoulNearBody(__0, body);
		}
	}

	private static void NCardLibraryReadyPostfix(NCardLibrary __instance)
	{
		if (!ModelDb.Contains(typeof(IllaoiCharacter)) || !ModelDb.Contains(typeof(IllaoiCardPool)))
		{
			return;
		}

		if (CardLibraryPoolFiltersField.GetValue(__instance) is not Dictionary<NCardPoolFilter, Func<CardModel, bool>> poolFilters
			|| CardLibraryCardPoolFiltersField.GetValue(__instance) is not Dictionary<CharacterModel, NCardPoolFilter> cardPoolFilters
			|| CardLibraryMiscPoolFilterField.GetValue(__instance) is not NCardPoolFilter templateFilter)
		{
			return;
		}

		CharacterModel illaoi = ModelDb.Character<IllaoiCharacter>();
		if (cardPoolFilters.ContainsKey(illaoi))
		{
			return;
		}

		Node? parent = templateFilter.GetParent();
		if (parent == null)
		{
			return;
		}

		NCardPoolFilter filter;
		if (parent.GetNodeOrNull<NCardPoolFilter>(CardLibraryPoolFilterNodeName) is { } existingFilter)
		{
			filter = existingFilter;
		}
		else
		{
			filter = CreateCardLibraryPoolFilter(templateFilter);
			parent.AddChild(filter);
			parent.MoveChild(filter, Math.Max(0, templateFilter.GetIndex() - 1));
		}

		cardPoolFilters.Add(illaoi, filter);
		poolFilters[filter] = BelongsToIllaoiCardLibrary;
		filter.Toggled += selectedFilter => CardLibraryUpdateCardPoolFilterMethod.Invoke(__instance, [selectedFilter]);
		filter.FocusEntered += () => CardLibraryLastHoveredControlField.SetValue(__instance, filter);
		filter.Visible = true;
	}

	private static NCardPoolFilter CreateCardLibraryPoolFilter(NCardPoolFilter templateFilter)
	{
		Texture2D? iconTexture = LoadPortableTexture(ModInfo.CharacterTopPanelIconPath)
			?? LoadPortableTexture(ModInfo.CharacterSelectButtonIconPath);
		ShaderMaterial? material = DuplicateCardLibraryFilterMaterial(templateFilter);
		NCardPoolFilter filter = new()
		{
			Name = CardLibraryPoolFilterNodeName,
			CustomMinimumSize = new Vector2(64f, 64f),
			Size = new Vector2(64f, 64f)
		};
		TextureRect image = new()
		{
			Name = "Image",
			Texture = iconTexture,
			ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
			StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
			Size = new Vector2(56f, 56f),
			Position = new Vector2(4f, 4f),
			Scale = new Vector2(0.9f, 0.9f),
			PivotOffset = new Vector2(28f, 28f),
			Material = material
		};
		TextureRect shadow = new()
		{
			Name = "Shadow",
			Texture = iconTexture,
			ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
			StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
			Size = new Vector2(56f, 56f),
			Position = new Vector2(4f, 3f),
			PivotOffset = new Vector2(28f, 28f),
			ShowBehindParent = true,
			Modulate = new Color(0f, 0f, 0f, 0.25f)
		};
		NSelectionReticle reticle = PreloadManager.Cache.GetScene("res://scenes/ui/selection_reticle.tscn").Instantiate<NSelectionReticle>(PackedScene.GenEditState.Disabled);
		reticle.Name = "SelectionReticle";
		reticle.UniqueNameInOwner = true;
		filter.AddChild(image);
		filter.AddChild(reticle);
		reticle.Owner = filter;
		image.AddChild(shadow);
		return filter;
	}

	private static ShaderMaterial? DuplicateCardLibraryFilterMaterial(NCardPoolFilter templateFilter)
	{
		return templateFilter.GetNodeOrNull<Control>("Image")?.GetMaterial() is ShaderMaterial shaderMaterial
			? (ShaderMaterial)shaderMaterial.Duplicate(true)
			: null;
	}

	private static bool BelongsToIllaoiCardLibrary(CardModel card)
	{
		CardPoolModel cardPool = ModelDb.CardPool<IllaoiCardPool>();
		return cardPool.AllCardIds.Contains(card.Id)
			|| card.ShouldShowInCardLibrary && card.VisualCardPool == cardPool;
	}

	private static void ApplyEnergyCounterIcon(NEnergyCounter counter, Texture2D texture)
	{
		Control? layers = counter.GetNodeOrNull<Control>("%Layers");
		Control? rotationLayers = counter.GetNodeOrNull<Control>("%RotationLayers");

		if (layers != null)
		{
			HideTextureRects(layers);
			RemoveNodeByName(layers, EnergyCounterIconNodeName);
			TextureRect icon = new()
			{
				Name = EnergyCounterIconNodeName,
				Texture = texture,
				MouseFilter = Control.MouseFilterEnum.Ignore,
				ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
				StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
				Material = null
			};
			layers.AddChild(icon);
			icon.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
		}

		if (rotationLayers != null)
		{
			HideTextureRects(rotationLayers);
		}
	}

	private static Control CreateTopPanelIcon(Texture2D texture)
	{
		TextureRect icon = new()
		{
			Name = "IllaoiTopPanelIcon",
			Texture = texture,
			MouseFilter = Control.MouseFilterEnum.Ignore,
			ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
			StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
			Material = null
		};
		icon.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
		return icon;
	}

	private static void ApplyCharacterSelectIllustration(NCharacterSelectScreen screen, CharacterModel character)
	{
		if (CharacterSelectScreenBgContainerField.GetValue(screen) is not Control bgContainer)
		{
			return;
		}

		RemoveCharacterSelectIllustration(bgContainer);
		if (!IsIllaoiCharacter(character) || LoadPortableTexture(ModInfo.CharacterSelectImagePath) is not { } texture)
		{
			return;
		}

		TextureRect art = new()
		{
			Name = CharacterSelectArtNodeName,
			Texture = texture,
			MouseFilter = Control.MouseFilterEnum.Ignore,
			ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
			StretchMode = TextureRect.StretchModeEnum.Scale,
			Modulate = Colors.White
		};
		bgContainer.AddChild(art);
		FillViewportShortSide(art, texture, screen);
	}

	private static void FillViewportShortSide(TextureRect art, Texture2D texture, Control screen)
	{
		Vector2 viewportSize = screen.GetViewportRect().Size;
		if (viewportSize.X <= 0f || viewportSize.Y <= 0f || texture.GetHeight() <= 0)
		{
			viewportSize = new Vector2(texture.GetWidth(), texture.GetHeight());
		}

		float targetHeight = Mathf.Min(viewportSize.X, viewportSize.Y);
		float scale = targetHeight / texture.GetHeight();
		Vector2 artSize = new(texture.GetWidth() * scale, texture.GetHeight() * scale);
		art.Size = artSize;
		art.GlobalPosition = new Vector2((viewportSize.X - artSize.X) * 0.5f, (viewportSize.Y - artSize.Y) * 0.5f);
	}

	private static void RemoveCharacterSelectIllustration(Node root)
	{
		foreach (Node child in root.GetChildren())
		{
			if (child.Name.ToString() == CharacterSelectArtNodeName)
			{
				child.QueueFree();
				continue;
			}

			RemoveCharacterSelectIllustration(child);
		}
	}

	private static void ApplyCharacterSelectTexture(TextureRect textureRect, Texture2D texture, TextureRect.StretchModeEnum stretchMode)
	{
		if (!GodotObject.IsInstanceValid(textureRect) || !GodotObject.IsInstanceValid(texture))
		{
			return;
		}

		try
		{
			textureRect.Texture = texture;
			textureRect.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
			textureRect.StretchMode = stretchMode;
		}
		catch (ObjectDisposedException)
		{
			Log.Warn($"{ModInfo.LogPrefix} Character select texture was disposed before it could be applied.");
		}
	}

	private static void ApplyCombatIllustration(NCreatureVisuals visuals)
	{
		if (LoadPortableTexture(ModInfo.CharacterCombatImagePath) is not { } texture)
		{
			return;
		}

		if (visuals.GetNodeOrNull<Sprite2D>("%Visuals") is { } body)
		{
			body.Texture = texture;
			body.Centered = true;
			body.Visible = true;
			body.Position = new Vector2(0f, -210f);
			body.Scale = new Vector2(0.78f, 0.78f);
			return;
		}

		RefreshCombatIllustration(visuals);
	}

	private static void RefreshCombatIllustration(NCreatureVisuals visuals)
	{
		RemoveNodeByName(visuals, CombatArtNodeName);
		if (LoadPortableTexture(ModInfo.CharacterCombatImagePath) is not { } texture)
		{
			return;
		}

		HideExistingCombatVisuals(visuals);
		Sprite2D art = new()
		{
			Name = CombatArtNodeName,
			Texture = texture,
			Centered = true,
			Position = new Vector2(0f, -210f),
			Scale = new Vector2(0.78f, 0.78f),
			ZIndex = 20
		};
		visuals.AddChild(art);
	}

	private static void ApplySoulVisualStyle(NCreatureVisuals visuals)
	{
		visuals.Modulate = new Color(0.48f, 0.86f, 1f, 0.48f);
	}

	private static void ApplyTentacleVisualStyle(NCreatureVisuals visuals)
	{
		RemoveNodeByName(visuals, TentacleArtNodeName);
		if (LoadPortableTexture(ModInfo.TentacleImagePath) is not { } texture)
		{
			return;
		}

		HideExistingCombatVisuals(visuals);
		Sprite2D art = new()
		{
			Name = TentacleArtNodeName,
			Texture = texture,
			Centered = true,
			Position = new Vector2(0f, -155f),
			Scale = new Vector2(0.42f, 0.42f),
			ZIndex = 0
		};
		visuals.AddChild(art);
	}

	private static void HideExistingCombatVisuals(Node root)
	{
		foreach (Node child in root.GetChildren())
		{
			if (child.Name.ToString() == CombatArtNodeName || child.Name.ToString() == TentacleArtNodeName)
			{
				continue;
			}

			if (child is Node2D node2D)
			{
				node2D.Hide();
			}

			HideExistingCombatVisuals(child);
		}
	}

	private static void RemoveNodeByName(Node root, string nodeName)
	{
		foreach (Node child in root.GetChildren())
		{
			if (child.Name.ToString() == nodeName)
			{
				child.QueueFree();
			}
		}
	}

	private static void HideTextureRects(Node root)
	{
		foreach (Node child in root.GetChildren())
		{
			if (child.Name.ToString() == EnergyCounterIconNodeName)
			{
				child.QueueFree();
				continue;
			}

			if (child is TextureRect textureRect)
			{
				textureRect.Visible = false;
			}

			HideTextureRects(child);
		}
	}

	private static bool TryUseCustomCharacterPath(CharacterModel character, ref string result, Func<IllaoiCustomCharacterModel, string?> selector)
	{
		if (character is not IllaoiCustomCharacterModel customCharacter)
		{
			return true;
		}

		string? path = selector(customCharacter);
		if (string.IsNullOrWhiteSpace(path))
		{
			return true;
		}

		result = path;
		return false;
	}

	private static bool TryUseCustomCompressedTexture(CharacterModel character, ref CompressedTexture2D result, Func<IllaoiCustomCharacterModel, string?> selector)
	{
		if (character is not IllaoiCustomCharacterModel customCharacter)
		{
			return true;
		}

		string? path = selector(customCharacter);
		if (string.IsNullOrWhiteSpace(path))
		{
			return true;
		}

		CompressedTexture2D? texture = ResourceLoader.Load<CompressedTexture2D>(path, null, ResourceLoader.CacheMode.Ignore);
		if (texture == null || !GodotObject.IsInstanceValid(texture))
		{
			return true;
		}

		result = texture;
		return false;
	}

	private static bool IsIllaoiCard(CardModel self)
	{
		return self is IllaoiStrike
			or IllaoiDefend
			or SoulTrial
			or TempleIdol
			or BuhruFootwork
			or SermonOfMotion
			or LowSweep
			or GuardedAdvance
			or TidalSlash
			or RestlessCurrent
			or WatchfulIdol
			or SpiritMark
			or GraspingLesson
			or ConsecratedGround
			or HarshSermon
			or FollowMyVoice
			or BuhruMeditation
			or CrushingWave
			or TentacleSlam
			or PriestessGuard
			or IdolRecall
			or TrialDance
			or GuardBreakingWave
			or SurgingSermon
			or LineBreaker
			or DeepMeditation
			or RhythmOfMotion
			or FervorOfMotion
			or WoundedVessel
			or IdolWard
			or VesselCrack
			or SpiritLash
			or MotionOfNagakabouros
			or OversteppingFaith
			or SpiritualPreparation
			or ProphetOfNagakabouros
			or LeapOfFaith
			or Tidecaller
			or Drain
			or RelentlessFaith
			or KrakenPriestess
			or HarrowingSermon
			or NagakabourosRising
			or AncientGodProphet
			or SoulImpact
			or TrialByMotion
			or SerpentDance
			or Undertow
			or VoiceOfTheDeep
			or TheSeaAnswers
			or DivineForm
			or RagingTide
			or RendSoul
			or TrialOfTheAncientGod
			or NagakabourosDescends;
	}

	private static bool IsIllaoiCharacter(CharacterModel self)
	{
		return self.Id == ModelDb.GetId<IllaoiCharacter>();
	}

	private static bool IsIllaoiRelic(RelicModel self)
	{
		return self.Id == ModelDb.GetId<NagakabourosIdol>()
			|| self.Id == ModelDb.GetId<NagakabourosTouch>();
	}

	private static bool TryGetIllaoiRelicTexture(RelicModel self, out Texture2D? texture)
	{
		texture = null;
		if (!IsIllaoiRelic(self))
		{
			return false;
		}

		string iconPath = self.Id == ModelDb.GetId<NagakabourosTouch>()
			? ModInfo.TouchIconPath
			: ModInfo.IdolIconPath;
		texture = LoadPortableTexture(iconPath);
		return texture != null;
	}

	private static bool IsIllaoiPower(PowerModel self)
	{
		return self.Id == ModelDb.GetId<IllaoiSoulLinkPower>()
			|| self.Id == ModelDb.GetId<IllaoiHuskPower>()
			|| self.Id == ModelDb.GetId<IllaoiTemporaryStrengthPower>()
			|| self.Id == ModelDb.GetId<IllaoiTemporaryDexterityPower>()
				|| self.Id == ModelDb.GetId<IllaoiFaithPower>()
				|| self.Id == ModelDb.GetId<IllaoiGrowTipPower>()
				|| self.Id == ModelDb.GetId<IllaoiTentacleTipPower>()
				|| self.Id == ModelDb.GetId<IllaoiCommandTipPower>()
				|| self.Id == ModelDb.GetId<IllaoiDrainPower>()
			|| self.Id == ModelDb.GetId<IllaoiAncientGodProphetPower>()
			|| self.Id == ModelDb.GetId<IllaoiSoulImpactPower>()
			|| self.Id == ModelDb.GetId<IllaoiNagakabourosDescendsPower>()
			|| self.Id == ModelDb.GetId<IllaoiTidecallerPower>()
			|| self.Id == ModelDb.GetId<IllaoiRelentlessFaithPower>()
			|| self.Id == ModelDb.GetId<IllaoiGrowthBlockPower>()
			|| self.Id == ModelDb.GetId<IllaoiRhythmOfMotionPower>()
			|| self.Id == ModelDb.GetId<IllaoiFervorOfMotionPower>()
			|| self.Id == ModelDb.GetId<IllaoiWatchfulIdolPower>()
			|| self.Id == ModelDb.GetId<IllaoiSeaAnswersPower>()
			|| self.Id == ModelDb.GetId<IllaoiNextTurnDrawPower>()
			|| self.Id == ModelDb.GetId<IllaoiDivineFormPower>()
			|| self.Id == ModelDb.GetId<IllaoiNextTurnFaithPower>();
	}

	private static bool TryGetIllaoiPowerTexture(PowerModel self, out Texture2D texture)
	{
		texture = null!;
		string? iconPath = GetIllaoiPowerIconPath(self);
		if (iconPath == null)
		{
			return false;
		}

		Texture2D? loadedTexture = LoadPortableTexture(iconPath);
		if (loadedTexture == null)
		{
			return false;
		}

		texture = loadedTexture;
		return true;
	}

	private static string? GetIllaoiPowerIconPath(PowerModel self)
	{
		if (self.Id == ModelDb.GetId<IllaoiGrowTipPower>()
			|| self.Id == ModelDb.GetId<IllaoiTentacleTipPower>()
			|| self.Id == ModelDb.GetId<IllaoiCommandTipPower>())
		{
			return null;
		}

		if (self.Id == ModelDb.GetId<IllaoiSoulLinkPower>())
		{
			return ModInfo.SoulPowerIconPath;
		}

		if (self.Id == ModelDb.GetId<IllaoiHuskPower>())
		{
			return ModInfo.HuskPowerIconPath;
		}

		if (self.Id == ModelDb.GetId<IllaoiTemporaryStrengthPower>())
		{
			return ModInfo.TemporaryStrengthPowerIconPath;
		}

		if (self.Id == ModelDb.GetId<IllaoiTemporaryDexterityPower>())
		{
			return ModInfo.TemporaryDexterityPowerIconPath;
		}

		if (self.Id == ModelDb.GetId<IllaoiFaithPower>())
		{
			return ModInfo.FaithPowerIconPath;
		}

		if (self.Id == ModelDb.GetId<IllaoiDrainPower>())
		{
			return ModInfo.DrainPowerIconPath;
		}

		if (self.Id == ModelDb.GetId<IllaoiAncientGodProphetPower>())
		{
			return ModInfo.AncientGodProphetPowerIconPath;
		}

		if (self.Id == ModelDb.GetId<IllaoiSoulImpactPower>())
		{
			return ModInfo.SoulImpactPowerIconPath;
		}

		if (self.Id == ModelDb.GetId<IllaoiNagakabourosDescendsPower>())
		{
			return ModInfo.NagakabourosDescendsPowerIconPath;
		}

		if (self.Id == ModelDb.GetId<IllaoiTidecallerPower>())
		{
			return ModInfo.TidecallerPowerIconPath;
		}

		if (self.Id == ModelDb.GetId<IllaoiRelentlessFaithPower>())
		{
			return ModInfo.RelentlessFaithPowerIconPath;
		}

		if (self.Id == ModelDb.GetId<IllaoiGrowthBlockPower>())
		{
			return ModInfo.GrowthBlockPowerIconPath;
		}

		if (self.Id == ModelDb.GetId<IllaoiRhythmOfMotionPower>())
		{
			return ModInfo.RhythmOfMotionPowerIconPath;
		}

		if (self.Id == ModelDb.GetId<IllaoiFervorOfMotionPower>())
		{
			return ModInfo.FervorOfMotionPowerIconPath;
		}

		if (self.Id == ModelDb.GetId<IllaoiWatchfulIdolPower>())
		{
			return ModInfo.WatchfulIdolPowerIconPath;
		}

		if (self.Id == ModelDb.GetId<IllaoiSeaAnswersPower>())
		{
			return ModInfo.SeaAnswersPowerIconPath;
		}

		if (self.Id == ModelDb.GetId<IllaoiNextTurnDrawPower>())
		{
			return ModInfo.NextTurnDrawPowerIconPath;
		}

		if (self.Id == ModelDb.GetId<IllaoiDivineFormPower>())
		{
			return ModInfo.DivineFormPowerIconPath;
		}

		if (self.Id == ModelDb.GetId<IllaoiNextTurnFaithPower>())
		{
			return ModInfo.NextTurnFaithPowerIconPath;
		}

		return IsIllaoiPower(self) ? ModInfo.IdolIconPath : null;
	}

	private static Texture2D? LoadPortableTexture(string path)
	{
		if (TextureCache.TryGetValue(path, out Texture2D? cachedTexture))
		{
			if (GodotObject.IsInstanceValid(cachedTexture))
			{
				return cachedTexture;
			}

			TextureCache.Remove(path);
		}

		Texture2D? loadedTexture = ResourceLoader.Load<Texture2D>(path, null, ResourceLoader.CacheMode.Ignore);
		if (loadedTexture != null && GodotObject.IsInstanceValid(loadedTexture))
		{
			TextureCache[path] = loadedTexture;
			return loadedTexture;
		}

		byte[] bytes = Godot.FileAccess.GetFileAsBytes(path);
		if (bytes.Length == 0)
		{
			return null;
		}

		Image image = new();
		Error err = path.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
			? image.LoadPngFromBuffer(bytes)
			: image.LoadJpgFromBuffer(bytes);
		if (err != Error.Ok)
		{
			return null;
		}

		PortableCompressedTexture2D texture = new();
		texture.CreateFromImage(image, PortableCompressedTexture2D.CompressionMode.Lossless);
		TextureCache[path] = texture;
		return texture;
	}

	private static bool IsIllaoiLocalAudioPath(string path)
	{
		return path.StartsWith(IllaoiAudioRoot, StringComparison.OrdinalIgnoreCase);
	}

	private static bool ShouldSuppressIllaoiLocalAudio(string path)
	{
		return IsIllaoiNewRunVoicePath(path) && !AllowIllaoiNewRunVoice;
	}

	private static bool IsIllaoiNewRunVoicePath(string path)
	{
		return string.Equals(path, ModInfo.CharacterTransitionSfxPath, StringComparison.OrdinalIgnoreCase);
	}

	internal static void PlayIllaoiNewRunVoice()
	{
		AllowIllaoiNewRunVoice = true;
		try
		{
			PlayIllaoiAudio(ModInfo.CharacterTransitionSfxPath, 1f);
		}
		finally
		{
			AllowIllaoiNewRunVoice = false;
		}
	}

	private static void PlayIllaoiAudio(string path, float volume)
	{
		AudioStream? stream = LoadIllaoiAudio(path);
		if (stream == null)
		{
			Log.Warn($"{ModInfo.LogPrefix} Could not load audio stream: {path}");
			return;
		}

		EnsureCharacterSelectSfxPlayer();
		if (!GodotObject.IsInstanceValid(CharacterSelectSfxPlayer))
		{
			Log.Warn($"{ModInfo.LogPrefix} Could not attach audio player for: {path}");
			return;
		}

		CharacterSelectSfxPlayer!.Stop();
		CharacterSelectSfxPlayer.Stream = stream;
		CharacterSelectSfxPlayer.VolumeLinear = Math.Clamp(volume * GetSfxVolumeMultiplier() * GetIllaoiAudioVolumeScale(path), 0f, 1f);
		CharacterSelectSfxPlayer.Play();
	}

	private static float GetSfxVolumeMultiplier()
	{
		return SaveManager.Instance?.SettingsSave?.VolumeSfx ?? 1f;
	}

	private static float GetIllaoiAudioVolumeScale(string path)
	{
		return path.StartsWith(IllaoiAudioRoot + "character_select/", StringComparison.OrdinalIgnoreCase)
			? CharacterSelectSfxVolumeScale
			: 1f;
	}

	private static AudioStream? LoadIllaoiAudio(string path)
	{
		if (AudioCache.TryGetValue(path, out AudioStream? cachedStream))
		{
			return cachedStream;
		}

		AudioStream? stream = GD.Load<AudioStream>(path) ?? ResourceLoader.Load<AudioStream>(path);
		if (stream != null)
		{
			AudioCache[path] = stream;
		}

		return stream;
	}

	private static void EnsureCharacterSelectSfxPlayer()
	{
		if (GodotObject.IsInstanceValid(CharacterSelectSfxPlayer))
		{
			return;
		}

		CharacterSelectSfxPlayer = new AudioStreamPlayer
		{
			Name = "IllaoiCharacterSelectSfx"
		};
		NGame.Instance?.AddChild(CharacterSelectSfxPlayer);
	}

	private static void PatchPrefix(Harmony harmony, MethodInfo original, string prefixName, int? priority = null)
	{
		Patch(harmony, original, prefixName, postfixName: null, priority);
	}

	private static void PatchPostfix(Harmony harmony, MethodInfo original, string postfixName)
	{
		Patch(harmony, original, prefixName: null, postfixName);
	}

	private static void Patch(Harmony harmony, MethodInfo original, string? prefixName, string? postfixName, int? prefixPriority = null)
	{
		HarmonyMethod? prefix = prefixName != null ? new HarmonyMethod(RequirePatchMethod(prefixName)) : null;
		HarmonyMethod? postfix = postfixName != null ? new HarmonyMethod(RequirePatchMethod(postfixName)) : null;
		if (prefix != null && prefixPriority.HasValue)
		{
			prefix.priority = prefixPriority.Value;
		}
		harmony.Patch(original, prefix, postfix);
	}

	private static MethodInfo RequireMethod(Type type, string name, BindingFlags flags, params Type[] parameterTypes)
	{
		return type.GetMethod(name, flags, binder: null, parameterTypes, modifiers: null)
			?? throw new InvalidOperationException($"Could not find method {type.FullName}.{name}.");
	}

	private static MethodInfo RequireGetter(Type type, string propertyName)
	{
		return type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.GetMethod
			?? throw new InvalidOperationException($"Could not find property getter {type.FullName}.{propertyName}.");
	}

	private static FieldInfo RequireField(Type type, string name)
	{
		return type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)
			?? throw new InvalidOperationException($"Could not find field {type.FullName}.{name}.");
	}

	private static MethodInfo RequirePatchMethod(string methodName)
	{
		return typeof(AssetHooks).GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic)
			?? throw new InvalidOperationException($"Could not find Harmony patch method {methodName}.");
	}
}
