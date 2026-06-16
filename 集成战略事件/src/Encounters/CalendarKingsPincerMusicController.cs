using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes;

namespace IntegratedStrategyEvents.Encounters;

internal sealed class CalendarKingsPincerMusicController : EncounterMusicController
{
	private const string PlayerName = "IntegratedStrategyCalendarKingsMusic";
	internal const string TrackPath = $"res://{ModInfo.ModId}/audio/music/calendar_kings.ogg";
	private const float VolumeScale = 0.82f;

	private static readonly CalendarKingsPincerMusicController Instance = new();

	private CalendarKingsPincerMusicController()
		: base(
			PlayerName,
			TrackPath,
			VolumeScale,
			"calendar kings",
			CalendarKingsPincerCreateBackgroundPatch.IsCalendarKingsPincerEncounter)
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
}

[HarmonyPatch(typeof(NGame), nameof(NGame.ReturnToMainMenu))]
internal static class CalendarKingsPincerMusicReturnToMainMenuPatch
{
	private static void Prefix()
	{
		CalendarKingsPincerMusicController.Stop(restoreGameMusic: false);
	}
}

[HarmonyPatch(typeof(NGame), nameof(NGame.ReturnToMainMenuAfterRun))]
internal static class CalendarKingsPincerMusicReturnToMainMenuAfterRunPatch
{
	private static void Prefix()
	{
		CalendarKingsPincerMusicController.Stop(restoreGameMusic: false);
	}
}
