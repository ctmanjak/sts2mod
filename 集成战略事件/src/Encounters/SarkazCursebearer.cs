using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.ValueProps;

namespace IntegratedStrategyEvents.Encounters;

public sealed class SarkazCursebearer : MonsterModel
{
	public const string SleepMoveId = "SLEEP_MOVE";
	public const string AttackMoveId = "ATTACK_MOVE";

	private const int InitialHp = 68;
	private const int AttackDamage = 6;
	private const int AttackHits = 3;
	private const float WakeDelay = 0.95f;
	private const float AttackFirstHitDelay = 0.9f;
	private const float AttackFollowUpHitDelay = 0.08f;
	private const string WakeTrigger = "WakeTrigger";
	private const string AttackSfxPath =
		"event:/sfx/enemy/enemy_attacks/spectral_knight/spectral_knight_soul_slash";

	private bool _isAwake;

	public override int MinInitialHp => InitialHp;

	public override int MaxInitialHp => InitialHp;

	public override bool HasDeathSfx => false;

	public override bool HasHurtSfx => false;

	public override bool ShouldFadeAfterDeath => true;

	public override float DeathAnimLengthOverride => 1.2f;

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Magic;

	protected override string VisualsPath => "res://IntegratedStrategyEvents/scenes/creature_visuals/sarkaz_cursebearer.tscn";

	public override Vector2 ExtraDeathVfxPadding => Vector2.One * 0.6f;

	private bool IsAwake
	{
		get => _isAwake;
		set
		{
			AssertMutable();
			_isAwake = value;
		}
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		MoveState sleep = new(
			SleepMoveId,
			SleepMove,
			new SleepIntent());
		MoveState attack = new(
			AttackMoveId,
			AttackMove,
			new MultiAttackIntent(AttackDamage, AttackHits));

		sleep.FollowUpState = sleep;
		attack.FollowUpState = attack;
		return new MonsterMoveStateMachine([sleep, attack], sleep);
	}

	private async Task SleepMove(IReadOnlyList<Creature> targets)
	{
		_ = targets;
		await Cmd.CustomScaledWait(0.1f, 0.2f);
	}

	private async Task AttackMove(IReadOnlyList<Creature> targets)
	{
		_ = targets;
		await DamageCmd.Attack(AttackDamage)
			.FromMonster(this)
			.WithAttackerAnim(CreatureAnimator.attackTrigger, 0f)
			.WithWaitBeforeHit(AttackFirstHitDelay, AttackFirstHitDelay)
			.WithHitFx("vfx/vfx_attack_slash", AttackSfxPath)
			.Execute(null);

		for (int i = 1; i < AttackHits; i++)
		{
			await Cmd.CustomScaledWait(AttackFollowUpHitDelay, AttackFollowUpHitDelay);
			await DamageCmd.Attack(AttackDamage)
				.FromMonster(this)
				.WithNoAttackerAnim()
				.WithHitFx("vfx/vfx_attack_slash", AttackSfxPath)
				.Execute(null);
		}
	}

	public override async Task AfterDamageReceived(
		PlayerChoiceContext choiceContext,
		Creature target,
		DamageResult result,
		ValueProp props,
		Creature? dealer,
		CardModel? cardSource)
	{
		await base.AfterDamageReceived(choiceContext, target, result, props, dealer, cardSource);
		if (IsAwake || target != Creature || result.UnblockedDamage <= 0 || result.WasTargetKilled || !Creature.IsAlive)
		{
			return;
		}

		IsAwake = true;
		if (MoveStateMachine?.States.TryGetValue(AttackMoveId, out MonsterState? nextState) == true && nextState is MoveState attack)
		{
			SetMoveImmediate(attack, forceTransition: true);
		}

		await MonsterAnimationHelper.TriggerAnimWithFixedWait(Creature, WakeTrigger, WakeDelay);
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState sleepingIdle = new("Idle", isLooping: true);
		AnimState awakenedIdle = new("Skill_Idle", isLooping: true);
		AnimState wake = new("Skill_Begin")
		{
			NextState = awakenedIdle
		};
		AnimState attack = new("Skill_Attack")
		{
			NextState = awakenedIdle
		};
		AnimState die = new("Die");

		CreatureAnimator animator = new(sleepingIdle, controller);
		animator.AddAnyState(CreatureAnimator.idleTrigger, sleepingIdle, () => !IsAwake);
		animator.AddAnyState(CreatureAnimator.idleTrigger, awakenedIdle, () => IsAwake);
		animator.AddAnyState(WakeTrigger, wake);
		animator.AddAnyState(CreatureAnimator.attackTrigger, attack);
		animator.AddAnyState(CreatureAnimator.deathTrigger, die);
		return animator;
	}
}
