using Godot;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;

namespace Illaoi;

public abstract class IllaoiCustomCharacterModel : CharacterModel
{
	public virtual string? CustomVisualPath => null;

	public virtual string? CustomTrailPath => null;

	public virtual string? CustomIconTexturePath => null;

	public virtual string? CustomIconOutlineTexturePath => null;

	public virtual string? CustomIconPath => null;

	public virtual string? CustomEnergyCounterPath => null;

	public virtual string? CustomEnergyCounterIconPath => null;

	public virtual string? CustomRestSiteAnimPath => null;

	public virtual string? CustomMerchantAnimPath => null;

	public virtual string? CustomArmPointingTexturePath => null;

	public virtual string? CustomArmRockTexturePath => null;

	public virtual string? CustomArmPaperTexturePath => null;

	public virtual string? CustomArmScissorsTexturePath => null;

	public virtual string? CustomCharacterSelectBg => null;

	public virtual string? CustomCharacterSelectIconPath => null;

	public virtual string? CustomCharacterSelectLockedIconPath => null;

	public virtual string? CustomCharacterSelectTransitionPath => null;

	public virtual string? CustomMapMarkerPath => null;

	public virtual string? CustomAttackSfx => null;

	public virtual string? CustomCastSfx => null;

	public virtual string? CustomDeathSfx => null;

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
