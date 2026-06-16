namespace IntegratedStrategyEvents.TreeHoles;

internal static class TreeHoleFinaleMusicCoordinator
{
	public static void StopForRunReset()
	{
		IntegratedStrategyEndlessFinaleMusicController.Stop(restoreGameMusic: false);
	}

	public static void PlayForEnteredRoom(EndlessFinaleSession session)
	{
		if (session.Kind == SpecialFinaleKind.EndlessFinale)
		{
			IntegratedStrategyEndlessFinaleMusicController.Play();
		}
	}

	public static void PlayAfterFinaleEntry(SpecialFinaleKind finaleKind)
	{
		if (finaleKind == SpecialFinaleKind.EndlessFinale)
		{
			IntegratedStrategyEndlessFinaleMusicController.Play();
		}
	}

	public static void StopBeforeArchitectHandoff(EndlessFinaleSession session)
	{
		if (session.Kind == SpecialFinaleKind.EndlessFinale)
		{
			IntegratedStrategyEndlessFinaleMusicController.Stop(restoreGameMusic: false);
		}
	}
}
