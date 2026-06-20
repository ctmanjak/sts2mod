namespace Illaoi;

internal static class ModInfo
{
	public const string Id = "Illaoi";
	public const string LogPrefix = "[Illaoi]";
	public const string TargetGameVersion = "0.107.1";

	public const int TentacleDamage = 3;
	public const decimal SoulHealthRatio = 0.5m;
	public const decimal SoulTransferRatio = 1m;
	public const int SoulDurationTurns = 3;
	public const int SoulHuskDurationTurns = SoulDurationTurns;
	public const int ShatteredSoulHuskDurationTurns = 2;

	private const string CardImageRoot = "res://Illaoi/images/cards/";
	private const string PowerImageRoot = "res://Illaoi/images/powers/";
	private const string AudioRoot = "res://Illaoi/audio/";

	public const string IllaoiStrikePortraitPath = CardImageRoot + "strike.png";
	public const string IllaoiDefendPortraitPath = CardImageRoot + "defend.png";
	public const string SoulTrialPortraitPath = CardImageRoot + "soul_trial.png";
	public const string TempleIdolPortraitPath = CardImageRoot + "temple_idol.png";
	public const string BuhruFootworkPortraitPath = CardImageRoot + "buhru_footwork.png";
	public const string SermonOfMotionPortraitPath = CardImageRoot + "sermon_of_motion.png";
	public const string LowSweepPortraitPath = CardImageRoot + "low_sweep.png";
	public const string GuardedAdvancePortraitPath = CardImageRoot + "guarded_advance.png";
	public const string TidalSlashPortraitPath = CardImageRoot + "tidal_slash.png";
	public const string RestlessCurrentPortraitPath = CardImageRoot + "restless_current.png";
	public const string WatchfulIdolPortraitPath = CardImageRoot + "watchful_idol.png";
	public const string SpiritMarkPortraitPath = CardImageRoot + "spirit_mark.png";
	public const string GraspingLessonPortraitPath = CardImageRoot + "admonish.png";
	public const string ConsecratedGroundPortraitPath = CardImageRoot + "consecrated_ground.png";
	public const string HarshSermonPortraitPath = CardImageRoot + "harsh_sermon.png";
	public const string FollowMyVoicePortraitPath = CardImageRoot + "follow_my_voice.png";
	public const string BuhruMeditationPortraitPath = CardImageRoot + "buhru_meditation.png";
	public const string CrushingWavePortraitPath = CardImageRoot + "crushing_wave.png";
	public const string TentacleSlamPortraitPath = CardImageRoot + "tentacle_slam.png";
	public const string PriestessGuardPortraitPath = CardImageRoot + "priestess_guard.png";
	public const string IdolRecallPortraitPath = CardImageRoot + "idol_recall.png";
	public const string TrialDancePortraitPath = CardImageRoot + "trial_dance.png";
	public const string GuardBreakingWavePortraitPath = CardImageRoot + "guard_breaking_wave.png";
	public const string SurgingSermonPortraitPath = CardImageRoot + "surging_sermon.png";
	public const string LineBreakerPortraitPath = CardImageRoot + "line_breaker.png";
	public const string DeepMeditationPortraitPath = CardImageRoot + "deep_meditation.png";
	public const string RhythmOfMotionPortraitPath = CardImageRoot + "rhythm_of_motion.png";
	public const string FervorOfMotionPortraitPath = CardImageRoot + "fervor_of_motion.png";
	public const string WoundedVesselPortraitPath = CardImageRoot + "wounded_vessel.png";
	public const string IdolWardPortraitPath = CardImageRoot + "idol_ward.png";
	public const string VesselCrackPortraitPath = CardImageRoot + "vessel_crack.png";
	public const string SpiritLashPortraitPath = CardImageRoot + "spirit_lash.png";
	public const string MotionOfNagakabourosPortraitPath = CardImageRoot + "motion_of_nagakabouros.png";
	public const string OversteppingFaithPortraitPath = CardImageRoot + "overstepping_faith.png";
	public const string SpiritualPreparationPortraitPath = CardImageRoot + "spiritual_preparation.png";
	public const string ProphetOfNagakabourosPortraitPath = CardImageRoot + "prophet_of_nagakabouros.png";
	public const string LeapOfFaithPortraitPath = CardImageRoot + "shield_of_faith.png";
	public const string TidecallerPortraitPath = CardImageRoot + "tidecaller.png";
	public const string DrainPortraitPath = CardImageRoot + "drain.png";
	public const string RelentlessFaithPortraitPath = CardImageRoot + "relentless_faith.png";
	public const string KrakenPriestessPortraitPath = CardImageRoot + "kraken_priestess.png";
	public const string HarrowingSermonPortraitPath = CardImageRoot + "harrowing_sermon.png";
	public const string NagakabourosRisingPortraitPath = CardImageRoot + "nagakabouros_rising.png";
	public const string AncientGodProphetPortraitPath = CardImageRoot + "ancient_god_prophet.png";
	public const string SoulImpactPortraitPath = CardImageRoot + "soul_impact.png";
	public const string TrialByMotionPortraitPath = CardImageRoot + "trial_by_motion.png";
	public const string SerpentDancePortraitPath = CardImageRoot + "serpent_dance.png";
	public const string UndertowPortraitPath = CardImageRoot + "undertow.png";
	public const string VoiceOfTheDeepPortraitPath = CardImageRoot + "voice_of_the_deep.png";
	public const string TheSeaAnswersPortraitPath = CardImageRoot + "the_sea_answers.png";
	public const string DivineFormPortraitPath = CardImageRoot + "divine_form.png";
	public const string RagingTidePortraitPath = CardImageRoot + "raging_tide.png";
	public const string RendSoulPortraitPath = CardImageRoot + "rend_soul.png";
	public const string TrialOfTheAncientGodPortraitPath = CardImageRoot + "trial_of_the_ancient_god.png";
	public const string NagakabourosDescendsPortraitPath = CardImageRoot + "nagakabouros_descends.png";
	public const string SoulPowerIconPath = PowerImageRoot + "soul.png";
	public const string HuskPowerIconPath = PowerImageRoot + "husk.png";
	public const string TemporaryStrengthPowerIconPath = PowerImageRoot + "temporary_strength.png";
	public const string TemporaryDexterityPowerIconPath = PowerImageRoot + "temporary_dexterity.png";
	public const string FaithPowerIconPath = PowerImageRoot + "faith.png";
	public const string DrainPowerIconPath = PowerImageRoot + "drain.png";
	public const string AncientGodProphetPowerIconPath = PowerImageRoot + "ancient_god_prophet.png";
	public const string SoulImpactPowerIconPath = PowerImageRoot + "soul_impact.png";
	public const string NagakabourosDescendsPowerIconPath = PowerImageRoot + "nagakabouros_descends.png";
	public const string TidecallerPowerIconPath = PowerImageRoot + "tidecaller.png";
	public const string RelentlessFaithPowerIconPath = PowerImageRoot + "relentless_faith.png";
	public const string GrowthBlockPowerIconPath = PowerImageRoot + "kraken_priestess.png";
	public const string RhythmOfMotionPowerIconPath = PowerImageRoot + "rhythm_of_motion.png";
	public const string FervorOfMotionPowerIconPath = PowerImageRoot + "fervor_of_motion.png";
	public const string WatchfulIdolPowerIconPath = PowerImageRoot + "watchful_idol.png";
	public const string SeaAnswersPowerIconPath = PowerImageRoot + "sea_answers.png";
	public const string NextTurnDrawPowerIconPath = PowerImageRoot + "next_turn_draw.png";
	public const string DivineFormPowerIconPath = PowerImageRoot + "kraken_priestess.png";
	public const string NextTurnFaithPowerIconPath = PowerImageRoot + "faith.png";
	public const string IdolIconPath = "res://Illaoi/images/relics/idol.png";
	public const string TouchIconPath = "res://Illaoi/images/relics/touch.png";
	public const string CharacterSelectImagePath = "res://Illaoi/images/characters/character_select.png";
	public const string CharacterSelectButtonIconPath = "res://Illaoi/images/characters/button_icon.png";
	public const string CharacterCombatImagePath = "res://Illaoi/images/characters/combat.png";
	public const string CharacterRestSiteImagePath = "res://Illaoi/images/characters/rest_site.png";
	public const string CharacterMerchantScenePath = "res://Illaoi/scenes/merchant/illaoi_merchant.tscn";
	public const string CharacterRestSiteScenePath = "res://Illaoi/scenes/rest_site/illaoi_rest_site.tscn";
	public static readonly string[] CharacterSelectSfxPaths =
	[
		AudioRoot + "character_select/select_01.mp3",
		AudioRoot + "character_select/select_02.mp3",
		AudioRoot + "character_select/select_03.mp3",
		AudioRoot + "character_select/select_04.mp3",
		AudioRoot + "character_select/select_05.mp3",
		AudioRoot + "character_select/select_06.mp3"
	];
	public const string CharacterTransitionSfxPath = AudioRoot + "character_transition/sermon_of_bones.mp3";
	public const string CharacterVisualsScenePath = "res://Illaoi/scenes/creature_visuals/illaoi.tscn";
	public const string CharacterSelectBgScenePath = "res://Illaoi/scenes/screens/char_select/char_select_bg_illaoi.tscn";
	public const string CharacterTopPanelIconPath = "res://Illaoi/images/ui/top_panel_icon.png";
	public const string MapMarkerIconPath = "res://Illaoi/images/ui/map_marker.png";
	public const string EnergyIconPath = "res://Illaoi/images/ui/energy.png";
	public const string EnergySpriteFontIconPath = "res://Illaoi/images/packed/sprite_fonts/energy.png";
	public const string TentacleImagePath = "res://Illaoi/images/vfx/tentacle.png";
	public const string CharacterImagePath = CharacterSelectImagePath;
}
