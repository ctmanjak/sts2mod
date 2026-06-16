using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace IntegratedStrategyEvents.Encounters;

public sealed class ScavengerApostle : MonsterModel
{
	public enum OpeningMove
	{
		Attack,
		Pollution,
		Erosion
	}

	public const string AttackMoveId = "ATTACK_MOVE";
	public const string PollutionMoveId = "POLLUTION_MOVE";
	public const string ErosionMoveId = "EROSION_MOVE";

	private const int InitialHp = 120;
	private const int AttackDamage = 18;
	private const int PollutionDamage = 10;
	private const int PollutionStrength = 2;
	private const int DebuffAmount = 1;
	private const float AttackHitDelay = 0.85f;
	private const float PollutionHitDelay = 0.85f;
	private const float ErosionDelay = 0.75f;
	private const string AttackSfxPath = "event:/sfx/enemy/enemy_attacks/living_fog/living_fog_attack_blow";
	private const string PollutionSfxPath = "event:/sfx/enemy/enemy_attacks/living_fog/living_fog_attack_blow";

	private OpeningMove _initialMove = OpeningMove.Attack;

	public override int MinInitialHp => InitialHp;

	public override int MaxInitialHp => InitialHp;

	public override bool HasDeathSfx => false;

	public override bool HasHurtSfx => false;

	public override bool ShouldFadeAfterDeath => true;

	public override float DeathAnimLengthOverride => 1.2f;

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Magic;

	protected override string VisualsPath => "res://IntegratedStrategyEvents/scenes/creature_visuals/scavenger_apostle.tscn";

	public override Vector2 ExtraDeathVfxPadding => Vector2.One * 0.7f;

	public OpeningMove InitialMove
	{
		get => _initialMove;
		set
		{
			AssertMutable();
			_initialMove = value;
		}
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		MoveState attack = new(
			AttackMoveId,
			AttackMove,
			new SingleAttackIntent(AttackDamage));
		MoveState pollution = new(
			PollutionMoveId,
			PollutionMove,
			new SingleAttackIntent(PollutionDamage),
			new BuffIntent());
		MoveState erosion = new(
			ErosionMoveId,
			ErosionMove,
			new DebuffIntent());

		attack.FollowUpState = pollution;
		pollution.FollowUpState = erosion;
		erosion.FollowUpState = attack;

		MonsterState initialState = InitialMove switch
		{
			OpeningMove.Pollution => pollution,
			OpeningMove.Erosion => erosion,
			_ => attack
		};
		return new MonsterMoveStateMachine([attack, pollution, erosion], initialState);
	}

	private async Task AttackMove(IReadOnlyList<Creature> targets)
	{
		_ = targets;
		await DamageCmd.Attack(AttackDamage)
			.FromMonster(this)
			.WithAttackerAnim(CreatureAnimator.attackTrigger, 0f)
			.WithWaitBeforeHit(AttackHitDelay, AttackHitDelay)
			.WithHitFx("vfx/vfx_attack_slash", AttackSfxPath)
			.Execute(null);
	}

	private async Task PollutionMove(IReadOnlyList<Creature> targets)
	{
		_ = targets;
		await DamageCmd.Attack(PollutionDamage)
			.FromMonster(this)
			.WithAttackerAnim(CreatureAnimator.attackTrigger, 0f)
			.WithWaitBeforeHit(PollutionHitDelay, PollutionHitDelay)
			.WithHitFx("vfx/vfx_attack_slash", PollutionSfxPath)
			.Execute(null);

		await PowerCmd.Apply<StrengthPower>(Creature, PollutionStrength, Creature, null);
	}

	private async Task ErosionMove(IReadOnlyList<Creature> targets)
	{
		await MonsterAnimationHelper.TriggerAnimWithFixedWait(Creature, CreatureAnimator.attackTrigger, ErosionDelay);
		await PowerCmd.Apply<WeakPower>(targets, DebuffAmount, Creature, null);
		await PowerCmd.Apply<VulnerablePower>(targets, DebuffAmount, Creature, null);
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState idle = new("Idle", isLooping: true);
		AnimState start = new("Start")
		{
			NextState = idle
		};
		AnimState attack = new("Attack")
		{
			NextState = idle
		};
		AnimState die = new("Die");

		CreatureAnimator animator = new(start, controller);
		animator.AddAnyState(CreatureAnimator.idleTrigger, idle);
		animator.AddAnyState(CreatureAnimator.attackTrigger, attack);
		animator.AddAnyState(CreatureAnimator.deathTrigger, die);
		return animator;
	}
}
