using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;

namespace IntegratedStrategyEvents.Encounters;

internal sealed class BozhokastiSaintguardGunnerMusicController : EncounterMusicController
{
	private const string PlayerName = "IntegratedStrategyBozhokastiMusic";
	internal const string TrackPath = $"res://{ModInfo.ModId}/audio/music/bozhokasti_boss.ogg";
	private const float VolumeScale = 0.38f;

	private static readonly BozhokastiSaintguardGunnerMusicController Instance = new();

	private BozhokastiSaintguardGunnerMusicController()
		: base(
			PlayerName,
			TrackPath,
			VolumeScale,
			"Bozhokasti",
			IsBozhokastiSaintguardGunnerEncounter)
	{
	}

	public static void Play()
	{
		Instance.PlayMusic();
	}

	public static void Stop(bool restoreGameMusic)
	{
		Instance.StopMusic(restoreGameMusic);
	}

	internal static bool IsBozhokastiSaintguardGunnerEncounter(EncounterModel encounter)
	{
		return encounter is BozhokastiSaintguardGunnerBossEncounter ||
			encounter.CanonicalInstance is BozhokastiSaintguardGunnerBossEncounter ||
			encounter is BozhokastiSaintguardGunnerTestEncounter ||
			encounter.CanonicalInstance is BozhokastiSaintguardGunnerTestEncounter;
	}
}

[HarmonyPatch(typeof(NGame), nameof(NGame.ReturnToMainMenu))]
internal static class BozhokastiSaintguardGunnerMusicReturnToMainMenuPatch
{
	private static void Prefix()
	{
		BozhokastiSaintguardGunnerMusicController.Stop(restoreGameMusic: false);
	}
}

[HarmonyPatch(typeof(NGame), nameof(NGame.ReturnToMainMenuAfterRun))]
internal static class BozhokastiSaintguardGunnerMusicReturnToMainMenuAfterRunPatch
{
	private static void Prefix()
	{
		BozhokastiSaintguardGunnerMusicController.Stop(restoreGameMusic: false);
	}
}
