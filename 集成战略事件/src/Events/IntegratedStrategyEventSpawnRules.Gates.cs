using IntegratedStrategyEvents.TreeHoles;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace IntegratedStrategyEvents.Events;

internal static partial class IntegratedStrategyEventSpawnRules
{
	private const int EntrustAdventurerMinimumGold = 50;
	private const int AllComersWelcomeMinimumGold = 200;
	private const int NorthernWizardArenaMinimumGold = 120;
	private const int LiuerMinimumGold = 2;
	private const int TrappedPersonMinimumGold = 1;
	private const int TrappedPersonMaximumGold = 100;
	private const int FortuneFlowsMinimumGold = 150;
	private const int ForSurvivalMinimumStartingGold = 15;

	private const int WastefulRevelryMinimumHp = 19;
	private const int FatefulMeetingMinimumHp = 9;
	private const int PathOfSufferingMinimumHp = 25;
	private const int HundredMileEncampmentMinimumHp = 9;

	private const int TimidThievesMinimumMaxHp = 3;
	private const int FutureHunterCardsToRemove = 2;
	private const int SecondActIndex = 1;

	private static readonly IReadOnlyDictionary<Type, Func<IRunState, bool>> AllowRules =
		new Dictionary<Type, Func<IRunState, bool>>
		{
			[typeof(EntrustAdventurerEvent)] = static runState =>
				AllPlayersHaveGold(runState, EntrustAdventurerMinimumGold),
			[typeof(AllComersWelcomeEvent)] = static runState =>
				AllPlayersHaveGold(runState, AllComersWelcomeMinimumGold),
			[typeof(NorthernWizardArenaEvent)] = static runState =>
				AllPlayersHaveGold(runState, NorthernWizardArenaMinimumGold),
			[typeof(ForwardForestEvent)] = static _ => false,
			[typeof(GlimpseEvent)] = static _ => false,
			[typeof(ShiftingCityEvent)] = static _ => false,
			[typeof(StoryToBeToldEvent)] = static _ => false,
			[typeof(VoidPortentEvent)] = static runState =>
				IsSecondActOpeningBranchAvailable(runState),
			[typeof(ChangeEvent)] = static runState =>
				IsSecondActOpeningBranchAvailable(runState),
			[typeof(AnomalousReportEvent)] = static runState =>
				IsSecondActOpeningBranchAvailable(runState),
			[typeof(ReconstructionEvent)] = static runState =>
				runState is RunState state &&
				IntegratedStrategyTreeHoleController.IsAtEternalDustFirstEventPoint(state),
			[typeof(ExplorerSmallStepEvent)] = static runState =>
				runState is RunState state &&
				IntegratedStrategyTreeHoleController.IsAtEternalDustSecondEventPoint(state),
			[typeof(LiuerEvent)] = static runState =>
				AllPlayersHaveGold(runState, LiuerMinimumGold),
			[typeof(TrappedPersonEvent)] = static runState =>
				AllPlayersHaveGoldBetween(runState, TrappedPersonMinimumGold, TrappedPersonMaximumGold),
			[typeof(FortuneFlowsEvent)] = static runState =>
				AllPlayersHaveGold(runState, FortuneFlowsMinimumGold),
			[typeof(ForSurvivalEvent)] = static runState =>
				AllPlayersHaveGold(runState, ForSurvivalMinimumStartingGold),
			[typeof(TransmissionEvent)] = static runState =>
				runState.Players.Any(HasTransformableStrikeOrDefend),
			[typeof(WastefulRevelryEvent)] = static runState =>
				AllPlayersCanLoseHp(runState, WastefulRevelryMinimumHp - 1),
			[typeof(FatefulMeetingEvent)] = static runState =>
				AllPlayersCanLoseHp(runState, FatefulMeetingMinimumHp - 1),
			[typeof(PathOfSufferingEvent)] = static runState =>
				AllPlayersCanLoseHp(runState, PathOfSufferingMinimumHp - 1),
			[typeof(HundredMileEncampmentEvent)] = static runState =>
				AllPlayersCanLoseHp(runState, HundredMileEncampmentMinimumHp - 1),
			[typeof(TimidThievesEvent)] = static runState =>
				AllPlayersCanLoseMaxHp(runState, TimidThievesMinimumMaxHp - 1),
			[typeof(RoyalDisputeEvent)] = static runState =>
				runState.Players.All(HasOffColorCardPool),
			[typeof(FutureHunterEvent)] = static runState =>
				runState.Players.All(static player =>
					CountRemovableDeckCards(player, CardType.Attack) >= FutureHunterCardsToRemove
					|| CountRemovableDeckCards(player, CardType.Skill) >= FutureHunterCardsToRemove)
		};

	private static bool AllPlayersHaveGold(IRunState runState, int amount)
	{
		return runState.Players.All(player => player.Gold >= amount);
	}

	private static bool IsSecondActOpeningBranchAvailable(IRunState runState)
	{
		return runState is RunState state &&
			state.CurrentActIndex == SecondActIndex &&
			!IntegratedStrategyTreeHoleController.IsActive(state) &&
			!HasVisitedOrdinaryEventInCurrentAct(state);
	}

	private static bool HasVisitedOrdinaryEventInCurrentAct(RunState runState)
	{
		return runState.MapPointHistory.Count > runState.CurrentActIndex &&
			runState.MapPointHistory[runState.CurrentActIndex].Any(static entry =>
				entry.MapPointType == MapPointType.Unknown &&
				entry.Rooms.Any(static room => room.RoomType == RoomType.Event));
	}

	private static bool AllPlayersHaveGoldBetween(IRunState runState, int minimum, int maximum)
	{
		return runState.Players.All(player => player.Gold >= minimum && player.Gold <= maximum);
	}

	private static bool AllPlayersCanLoseHp(IRunState runState, int amount)
	{
		return runState.Players.All(player => IntegratedStrategyEventEffects.CanLoseHp(player, amount));
	}

	private static bool AllPlayersCanLoseMaxHp(IRunState runState, int amount)
	{
		return runState.Players.All(player => IntegratedStrategyEventEffects.CanLoseMaxHp(player, amount));
	}

	private static bool HasOffColorCardPool(Player player)
	{
		ModelId currentPoolId = player.Character.CardPool.Id;
		return player.UnlockState.CharacterCardPools.Any(pool =>
			!pool.IsColorless
			&& !pool.Id.Equals(currentPoolId)
			&& pool.AllCards.Any(static card => card.CanBeGeneratedByModifiers));
	}

	private static bool HasTransformableStrikeOrDefend(Player player)
	{
		return IntegratedStrategyEventEffects.CountTransformableBasicDeckCards(player, CardTag.Strike) > 0
			|| IntegratedStrategyEventEffects.CountTransformableBasicDeckCards(player, CardTag.Defend) > 0;
	}

	private static int CountRemovableDeckCards(Player player, CardType type)
	{
		return IntegratedStrategyEventEffects.CountRemovableDeckCards(player, type);
	}
}
