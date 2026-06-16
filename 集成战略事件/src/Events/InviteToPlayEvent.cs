using IntegratedStrategyEvents.Encounters;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;

namespace IntegratedStrategyEvents.Events;

public sealed partial class InviteToPlayEvent : IntegratedStrategyEventModel
{
	private const int WalletGoldLoss = 60;
	private const int RareCardRewardOptionCount = 3;

	public override bool IsShared => true;

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		Player owner = OwnerOrThrow;
		return
		[
			Choice(Watch, "WATCH"),
			GoldChoice(owner, WalletGoldLoss, LoseWallet, "WALLET_LOST", "WALLET_LOST_LOCKED"),
			Choice(Leave, "LEAVE")
		];
	}

	private Task Watch()
	{
		ShowFightPage<InviteToPlayLostAndForgottenEncounter>("WATCH");
		return Task.CompletedTask;
	}

	private async Task LoseWallet()
	{
		await LoseGold(WalletGoldLoss);
		Finish("WALLET_LOST");
		await OfferRareCardReward(RareCardRewardOptionCount);
	}

	private Task Leave()
	{
		Finish("LEAVE");
		return Task.CompletedTask;
	}
}
