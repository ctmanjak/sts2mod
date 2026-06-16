using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace IntegratedStrategyEvents.Encounters;

internal static class MonsterAnimationHelper
{
	public static async Task TriggerAnimWithFixedWait(Creature creature, string triggerName, float waitTime)
	{
		await CreatureCmd.TriggerAnim(creature, triggerName, 0f);
		await Cmd.Wait(waitTime);
	}
}
