using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;

namespace Illaoi;

public abstract class IllaoiCustomCharacterModel : CustomCharacterModel
{
	public override string? CustomVisualPath => null;

	public override string? CustomTrailPath => null;

	public override string? CustomIconTexturePath => null;

	public override string? CustomIconOutlineTexturePath => null;

	public override string? CustomIconPath => null;

	public override string? CustomEnergyCounterPath => null;

	public virtual string? CustomEnergyCounterIconPath => null;

	public override string? CustomRestSiteAnimPath => null;

	public override string? CustomMerchantAnimPath => null;

	public override string? CustomArmPointingTexturePath => null;

	public override string? CustomArmRockTexturePath => null;

	public override string? CustomArmPaperTexturePath => null;

	public override string? CustomArmScissorsTexturePath => null;

	public override string? CustomCharacterSelectBg => null;

	public override string? CustomCharacterSelectIconPath => null;

	public override string? CustomCharacterSelectLockedIconPath => null;

	public override string? CustomCharacterSelectTransitionPath => null;

	public override string? CustomMapMarkerPath => null;

	public override string? CustomAttackSfx => null;

	public override string? CustomCastSfx => null;

	public override string? CustomDeathSfx => null;

	public virtual IEnumerable<string> ExtraCustomAssetPaths => [];

	public virtual IEnumerable<string> ExtraCustomCharacterSelectAssetPaths => [];

	protected override CharacterModel? UnlocksAfterRunAs => null;

	protected override string CharacterSelectIconPath => CustomCharacterSelectIconPath ?? base.CharacterSelectIconPath;

	protected override string CharacterSelectLockedIconPath => CustomCharacterSelectLockedIconPath ?? base.CharacterSelectLockedIconPath;

	protected override string MapMarkerPath => CustomMapMarkerPath ?? base.MapMarkerPath;

	public virtual IEnumerable<string> AllCustomAssetPaths => NonEmpty(
		CustomVisualPath,
		CustomIconTexturePath,
		CustomIconOutlineTexturePath,
		CustomIconPath,
		CustomEnergyCounterPath,
		CustomEnergyCounterIconPath,
		CustomRestSiteAnimPath,
		CustomMerchantAnimPath,
		CustomMapMarkerPath,
		CustomTrailPath,
		CustomArmPointingTexturePath,
		CustomArmRockTexturePath,
		CustomArmPaperTexturePath,
		CustomArmScissorsTexturePath)
		.Concat(ExtraCustomAssetPaths)
		.Where(path => !string.IsNullOrWhiteSpace(path))
		.Distinct();

	public virtual IEnumerable<string> AllCustomCharacterSelectAssetPaths => NonEmpty(
		CustomCharacterSelectBg,
		CustomCharacterSelectIconPath,
		CustomCharacterSelectLockedIconPath,
		CustomCharacterSelectTransitionPath,
		CustomIconTexturePath)
		.Concat(ExtraCustomCharacterSelectAssetPaths)
		.Where(path => !string.IsNullOrWhiteSpace(path))
		.Distinct();

	private static IEnumerable<string> NonEmpty(params string?[] paths)
	{
		foreach (string? path in paths)
		{
			if (!string.IsNullOrWhiteSpace(path))
			{
				yield return path;
			}
		}
	}
}

public abstract class IllaoiPlaceholderCharacterModel : IllaoiCustomCharacterModel
{
	public virtual string PlaceholderId => "ironclad";

	private string PlaceholderKey => PlaceholderId.ToLowerInvariant();

	public override int StartingGold => 99;

	public override float AttackAnimDelay => 0.15f;

	public override float CastAnimDelay => 0.25f;

	public override string? CustomVisualPath => SceneHelper.GetScenePath("creature_visuals/" + PlaceholderKey);

	public override string? CustomTrailPath => SceneHelper.GetScenePath("vfx/card_trail_" + PlaceholderKey);

	public override string? CustomIconTexturePath => ImageHelper.GetImagePath("ui/top_panel/character_icon_" + PlaceholderKey + ".png");

	public override string? CustomIconOutlineTexturePath => ImageHelper.GetImagePath("ui/top_panel/character_icon_" + PlaceholderKey + "_outline.png");

	public override string? CustomIconPath => SceneHelper.GetScenePath("ui/character_icons/" + PlaceholderKey + "_icon");

	public override string? CustomEnergyCounterPath => SceneHelper.GetScenePath("combat/energy_counters/" + PlaceholderKey + "_energy_counter");

	public override string? CustomRestSiteAnimPath => SceneHelper.GetScenePath("rest_site/characters/" + PlaceholderKey + "_rest_site");

	public override string? CustomMerchantAnimPath => SceneHelper.GetScenePath("merchant/characters/" + PlaceholderKey + "_merchant");

	public override string? CustomArmPointingTexturePath => ImageHelper.GetImagePath("ui/hands/multiplayer_hand_" + PlaceholderKey + "_point.png");

	public override string? CustomArmRockTexturePath => ImageHelper.GetImagePath("ui/hands/multiplayer_hand_" + PlaceholderKey + "_rock.png");

	public override string? CustomArmPaperTexturePath => ImageHelper.GetImagePath("ui/hands/multiplayer_hand_" + PlaceholderKey + "_paper.png");

	public override string? CustomArmScissorsTexturePath => ImageHelper.GetImagePath("ui/hands/multiplayer_hand_" + PlaceholderKey + "_scissors.png");

	public override string? CustomCharacterSelectBg => SceneHelper.GetScenePath("screens/char_select/char_select_bg_" + PlaceholderKey);

	public override string? CustomCharacterSelectIconPath => ImageHelper.GetImagePath("packed/character_select/char_select_" + PlaceholderKey + ".png");

	public override string? CustomCharacterSelectLockedIconPath => ImageHelper.GetImagePath("packed/character_select/char_select_" + PlaceholderKey + "_locked.png");

	public override string? CustomCharacterSelectTransitionPath => "res://materials/transitions/" + PlaceholderKey + "_transition_mat.tres";

	public override string? CustomMapMarkerPath => ImageHelper.GetImagePath("packed/map/icons/map_marker_" + PlaceholderKey + ".png");

	public override string? CustomAttackSfx => $"event:/sfx/characters/{PlaceholderKey}/{PlaceholderKey}_attack";

	public override string? CustomCastSfx => $"event:/sfx/characters/{PlaceholderKey}/{PlaceholderKey}_cast";

	public override string? CustomDeathSfx => $"event:/sfx/characters/{PlaceholderKey}/{PlaceholderKey}_die";

	public override string CharacterSelectSfx => $"event:/sfx/characters/{PlaceholderKey}/{PlaceholderKey}_select";

	public override string CharacterTransitionSfx => "event:/sfx/ui/wipe_" + PlaceholderKey;
}
