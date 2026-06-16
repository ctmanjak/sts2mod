using IntegratedStrategyEvents.Encounters;
using IntegratedStrategyEvents.Relics;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;

namespace IntegratedStrategyEvents.Events;

public sealed partial class ChangeEvent : IntegratedStrategyEventModel
{
	public override bool IsShared => true;

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		return
		[
			Choice(AcceptInvitation, "ACCEPT_INVITATION"),
			Choice(ReachForWeapon, "REACH_FOR_WEAPON")
		];
	}

	private Task AcceptInvitation()
	{
		ShowFightPage("ACCEPT_INVITATION", EnterTrialCombat);
		return Task.CompletedTask;
	}

	private Task ReachForWeapon()
	{
		Finish("REACH_FOR_WEAPON");
		return Task.CompletedTask;
	}

	private Task EnterTrialCombat()
	{
		var owner = OwnerOrThrow;
		Reward[] rewards =
		[
			new RelicReward(ModelDb.Relic<DimensionalFluidRelic>().ToMutable(), owner)
		];
		return EnterEventCombat<ChangeRedmarkEncounter>(rewards);
	}
}
