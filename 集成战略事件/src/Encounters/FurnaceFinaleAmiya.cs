using Godot;
using IntegratedStrategyEvents.Powers;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace IntegratedStrategyEvents.Encounters;

public sealed class FurnaceFinaleAmiya : MonsterModel
{
	public const string SummonLeftSlot = "amiya_summon_left";
	public const string SummonRightSlot = "amiya_summon_right";
	public const string BossSlot = "amiya";
	public const string ReviveMoveId = "REVIVE_MOVE";
	public const string RecreateMoveId = "RECREATE_MOVE";
	public const string EndMoveId = "END_MOVE";
	public const string SustainMoveId = "SUSTAIN_MOVE";
	public const string SilentHopeMoveId = "SILENT_HOPE_MOVE";
	public const string BlackCrownMoveId = "BLACK_CROWN_MOVE";

	private const int InitialHp = 480;
	private const int EndDamage = 10;
	private const int EndHits = 2;
	private const int EndStrengthGain = 1;
	private const int BlackCrownDamage = 25;
	private const int BlackCrownStrengthGain = 3;
	private const int SootDrawCount = 1;
	private const int SootDiscardCount = 2;
	private const float PhaseOneActionDelay = 0.85f;
	private const float EndHitDelay = 0.95f;
	private const float SustainCardDelay = 0.75f;
	private const float SilentHopeSummonDelay = 1.15f;
	private const float BlackCrownHitDelay = 1.2f;
	private const float ReviveEndDelay = 0.95f;
	private const string PhaseOneActionTrigger = "PhaseOneAction";
	private const string SilentHopeTrigger = "SilentHope";
	private const string BlackCrownTrigger = "BlackCrown";
	private const string ReviveBeginTrigger = "ReviveBegin";
	private const string ReviveEndTrigger = "ReviveEnd";
	private const string EndSfxPath = "event:/sfx/enemy/enemy_attacks/soul_fysh/soul_fysh_wave";
	private const string BlackCrownSfxPath =
		"event:/sfx/enemy/enemy_attacks/knowledge_demon/knowledge_demon_flame";

	private MoveState? _reviveState;
	private bool _hasRevived;
	private bool _secondPhase;

	public override int MinInitialHp => InitialHp;

	public override int MaxInitialHp => InitialHp;

	public override bool HasDeathSfx => false;

	public override bool HasHurtSfx => false;

	public override bool ShouldFadeAfterDeath => true;

	public override bool ShouldDisappearFromDoom => HasRevived;

	public override float DeathAnimLengthOverride => 1.7f;

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Magic;

	protected override string VisualsPath => "res://IntegratedStrategyEvents/scenes/creature_visuals/furnace_finale_amiya.tscn";

	public override Vector2 ExtraDeathVfxPadding => Vector2.One * 0.8f;

	public bool HasRevived => _hasRevived;

	private bool IsSecondPhase => _secondPhase;

	public override async Task AfterAddedToRoom()
	{
		await base.AfterAddedToRoom();
		await PowerCmd.Apply<UnfinishedFinalePower>(Creature, 1m, Creature, null);
	}

	public void BeginSecondPhaseVisuals()
	{
		AssertMutable();
		_secondPhase = true;
	}

	public void CompleteSecondPhaseRevive()
	{
		AssertMutable();
		_hasRevived = true;
		_secondPhase = true;
	}

	public Task TriggerReviveWaitingState()
	{
		if (HasRevived || _reviveState == null)
		{
			return Task.CompletedTask;
		}

		SetMoveImmediate(_reviveState, forceTransition: true);
		return Task.CompletedTask;
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		MoveState revive = new(
			ReviveMoveId,
			ReviveMove,
			new HealIntent(),
			new BuffIntent())
		{
			MustPerformOnceBeforeTransitioning = true
		};
		MoveState recreate = new(
			RecreateMoveId,
			RecreateMove,
			new SummonIntent());
		MoveState end = new(
			EndMoveId,
			EndMove,
			new MultiAttackIntent(EndDamage, EndHits),
			new BuffIntent());
		MoveState sustain = new(
			SustainMoveId,
			SustainMove,
			new StatusIntent(SootDrawCount + SootDiscardCount));
		MoveState silentHope = new(
			SilentHopeMoveId,
			SilentHopeMove,
			new SummonIntent());
		MoveState blackCrown = new(
			BlackCrownMoveId,
			BlackCrownMove,
			new SingleAttackIntent(BlackCrownDamage),
			new BuffIntent());

		ConditionalBranchState afterRecreate = new("AFTER_RECREATE_BRANCH");
		afterRecreate.AddState(silentHope, () => IsSecondPhase);
		afterRecreate.AddState(end, HasLivingPhaseOneSummon);
		afterRecreate.AddState(recreate, () => true);

		ConditionalBranchState afterEnd = new("AFTER_END_BRANCH");
		afterEnd.AddState(silentHope, () => IsSecondPhase);
		afterEnd.AddState(sustain, HasLivingPhaseOneSummon);
		afterEnd.AddState(recreate, () => true);

		ConditionalBranchState afterSustain = new("AFTER_SUSTAIN_BRANCH");
		afterSustain.AddState(silentHope, () => IsSecondPhase);
		afterSustain.AddState(end, HasLivingPhaseOneSummon);
		afterSustain.AddState(recreate, () => true);

		_reviveState = revive;

		revive.FollowUpState = silentHope;
		recreate.FollowUpState = afterRecreate;
		end.FollowUpState = afterEnd;
		sustain.FollowUpState = afterSustain;
		silentHope.FollowUpState = blackCrown;
		blackCrown.FollowUpState = silentHope;

		return new MonsterMoveStateMachine(
			[revive, recreate, end, sustain, silentHope, blackCrown, afterRecreate, afterEnd, afterSustain],
			recreate);
	}

	private async Task ReviveMove(IReadOnlyList<Creature> targets)
	{
		_ = targets;
		await KillLivingPhaseOneSummons();
		BeginSecondPhaseVisuals();
		await MonsterAnimationHelper.TriggerAnimWithFixedWait(Creature, ReviveEndTrigger, ReviveEndDelay);
		await CreatureCmd.Heal(Creature, Creature.MaxHp);
		Creature.GetPower<UnfinishedFinalePower>()?.DoRevive();
		CompleteSecondPhaseRevive();
	}

	private async Task RecreateMove(IReadOnlyList<Creature> targets)
	{
		_ = targets;
		await MonsterAnimationHelper.TriggerAnimWithFixedWait(Creature, PhaseOneActionTrigger, PhaseOneActionDelay);
		await SummonMinion<SarkazCasterLeader>(SummonLeftSlot);
		await SummonMinion<SarkazCursebearer>(SummonRightSlot);
	}

	private async Task EndMove(IReadOnlyList<Creature> targets)
	{
		_ = targets;
		await DamageCmd.Attack(EndDamage)
			.FromMonster(this)
			.WithHitCount(EndHits)
			.WithAttackerAnim(PhaseOneActionTrigger, 0f)
			.WithWaitBeforeHit(EndHitDelay, EndHitDelay)
			.WithHitFx("vfx/vfx_attack_slash", EndSfxPath)
			.OnlyPlayAnimOnce()
			.Execute(null);

		await ApplyStrengthToAllEnemies(EndStrengthGain);
	}

	private async Task SustainMove(IReadOnlyList<Creature> targets)
	{
		_ = targets;
		await MonsterAnimationHelper.TriggerAnimWithFixedWait(Creature, PhaseOneActionTrigger, SustainCardDelay);

		foreach (Creature target in CombatState.PlayerCreatures.Where(static creature => creature.IsAlive))
		{
			await AddSootCardsQuickly(target);
		}
	}

	private async Task AddSootCardsQuickly(Creature target)
	{
		Player? player = target.Player ?? target.PetOwner;
		if (player == null || player.Creature.IsDead || target.CombatState == null)
		{
			return;
		}

		List<CardPileAddResult> statusCards = new();
		statusCards.Add(await CardPileCmd.AddGeneratedCardToCombat(
			CombatState.CreateCard<Soot>(player),
			PileType.Draw,
			addedByPlayer: false,
			CardPilePosition.Bottom));
		for (int i = 0; i < SootDiscardCount; i++)
		{
			statusCards.Add(await CardPileCmd.AddGeneratedCardToCombat(
				CombatState.CreateCard<Soot>(player),
				PileType.Discard,
				addedByPlayer: false,
				CardPilePosition.Bottom));
		}

		if (LocalContext.IsMe(player))
		{
			CardCmd.PreviewCardPileAdd(statusCards, 1.2f, CardPreviewStyle.HorizontalLayout);
			await Cmd.Wait(1f);
		}
	}

	private async Task SilentHopeMove(IReadOnlyList<Creature> targets)
	{
		_ = targets;
		await MonsterAnimationHelper.TriggerAnimWithFixedWait(Creature, SilentHopeTrigger, SilentHopeSummonDelay);
		await SummonMinion<RemainingCreativity>(SummonLeftSlot);
		await SummonMinion<RemainingCreativity>(SummonRightSlot);
	}

	private async Task BlackCrownMove(IReadOnlyList<Creature> targets)
	{
		_ = targets;
		await DamageCmd.Attack(BlackCrownDamage)
			.FromMonster(this)
			.WithAttackerAnim(BlackCrownTrigger, 0f)
			.WithWaitBeforeHit(BlackCrownHitDelay, BlackCrownHitDelay)
			.WithHitFx("vfx/vfx_attack_slash", BlackCrownSfxPath)
			.Execute(null);
			await PowerCmd.Apply<StrengthPower>(Creature, BlackCrownStrengthGain, Creature, null);
	}

	private async Task SummonMinion<TMonster>(string slotName)
		where TMonster : MonsterModel
	{
		if (CombatState == null || Creature.IsDead || IsSummonSlotOccupied(slotName))
		{
			return;
		}

		Creature minion = await CreatureCmd.Add<TMonster>(CombatState, slotName);
		if (!minion.HasPower<MinionPower>())
		{
			await PowerCmd.Apply<MinionPower>(minion, 1m, Creature, null, silent: true);
		}
	}

	private bool IsSummonSlotOccupied(string slotName)
	{
		return CombatState?.Enemies.Any(creature => creature.IsAlive && creature.SlotName == slotName) == true;
	}

	private async Task KillLivingPhaseOneSummons()
	{
		IReadOnlyList<Creature> summons = CombatState.Enemies
			.Where(static creature =>
				creature.IsAlive
				&& (creature.Monster is SarkazCasterLeader || creature.Monster is SarkazCursebearer))
			.ToList();

		if (summons.Count > 0)
		{
			await CreatureCmd.Kill(summons, force: true);
		}
	}

	private async Task ApplyStrengthToAllEnemies(int amount)
	{
		IReadOnlyList<Creature> enemies = CombatState.Enemies
			.Where(static creature => creature.IsAlive)
			.ToList();
		await PowerCmd.Apply<StrengthPower>(enemies, amount, Creature, null);
	}

	private bool HasLivingPhaseOneSummon()
	{
		return CombatState?.Enemies.Any(static creature =>
			creature.IsAlive
			&& (creature.Monster is SarkazCasterLeader || creature.Monster is SarkazCursebearer)) == true;
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState phaseOneIdle = new("A_Idle", isLooping: true);
		AnimState phaseTwoIdle = new("B_Idle", isLooping: true);
		AnimState phaseOneAction = new("A_Attack")
		{
			NextState = phaseOneIdle
		};
		AnimState silentHope = new("B_Skill_2")
		{
			NextState = phaseTwoIdle
		};
		AnimState blackCrown = new("B_Skill_1")
		{
			NextState = phaseTwoIdle
		};
		AnimState reviveLoop = new("Revive_Loop", isLooping: true);
		AnimState reviveBegin = new("Revive_Begin")
		{
			NextState = reviveLoop
		};
		AnimState reviveEnd = new("Revive_End")
		{
			NextState = phaseTwoIdle
		};
		AnimState die = new("B_Die");

		CreatureAnimator animator = new(phaseOneIdle, controller);
		animator.AddAnyState(CreatureAnimator.idleTrigger, phaseOneIdle, () => !IsSecondPhase);
		animator.AddAnyState(CreatureAnimator.idleTrigger, phaseTwoIdle, () => IsSecondPhase);
		animator.AddAnyState(PhaseOneActionTrigger, phaseOneAction);
		animator.AddAnyState(SilentHopeTrigger, silentHope);
		animator.AddAnyState(BlackCrownTrigger, blackCrown);
		animator.AddAnyState(ReviveBeginTrigger, reviveBegin);
		animator.AddAnyState(ReviveEndTrigger, reviveEnd);
		animator.AddAnyState(CreatureAnimator.deathTrigger, reviveBegin, () => !HasRevived);
		animator.AddAnyState(CreatureAnimator.deathTrigger, die, () => HasRevived);
		return animator;
	}
}
