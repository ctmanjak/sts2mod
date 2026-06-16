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

public sealed class SarkazDescendantHatredCollector : MonsterModel
{
	public enum OpeningMove
	{
		Attack,
		Rally,
		Sweep
	}

	public const string AttackMoveId = "ATTACK_MOVE";
	public const string RallyMoveId = "RALLY_MOVE";
	public const string SweepMoveId = "SWEEP_MOVE";

	private const int InitialHp = 68;
	private const int AttackDamage = 14;
	private const int RallyStrength = 2;
	private const int SweepDamage = 6;
	private const int SweepHits = 2;
	private const float AttackHitDelay = 0.8f;
	private const float RallyDelay = 0.75f;
	private const float SweepFirstHitDelay = 0.8f;
	private const float SweepFollowUpHitDelay = 0.08f;
	private const string AttackSfxPath =
		"event:/sfx/enemy/enemy_attacks/the_kin_minion/the_kin_minion_quick_slash";
	private const string SweepSfxPath =
		"event:/sfx/enemy/enemy_attacks/the_kin_minion/the_kin_minion_boomerang_slashh";

	private OpeningMove _initialMove = OpeningMove.Attack;

	public override int MinInitialHp => InitialHp;

	public override int MaxInitialHp => InitialHp;

	public override bool HasDeathSfx => false;

	public override bool HasHurtSfx => false;

	public override bool ShouldFadeAfterDeath => true;

	public override float DeathAnimLengthOverride => 1.2f;

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Magic;

	protected override string VisualsPath =>
		"res://IntegratedStrategyEvents/scenes/creature_visuals/sarkaz_descendant_hatred_collector.tscn";

	public override Vector2 ExtraDeathVfxPadding => Vector2.One * 0.6f;

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
		MoveState rally = new(
			RallyMoveId,
			RallyMove,
			new BuffIntent());
		MoveState sweep = new(
			SweepMoveId,
			SweepMove,
			new MultiAttackIntent(SweepDamage, SweepHits));

		attack.FollowUpState = rally;
		rally.FollowUpState = sweep;
		sweep.FollowUpState = attack;

		MonsterState initialState = InitialMove switch
		{
			OpeningMove.Rally => rally,
			OpeningMove.Sweep => sweep,
			_ => attack
		};
		return new MonsterMoveStateMachine([attack, rally, sweep], initialState);
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

	private async Task RallyMove(IReadOnlyList<Creature> targets)
	{
		_ = targets;
		await MonsterAnimationHelper.TriggerAnimWithFixedWait(Creature, CreatureAnimator.attackTrigger, RallyDelay);
		await PowerCmd.Apply<StrengthPower>(Creature, RallyStrength, Creature, null);
	}

	private async Task SweepMove(IReadOnlyList<Creature> targets)
	{
		_ = targets;
		await DamageCmd.Attack(SweepDamage)
			.FromMonster(this)
			.WithAttackerAnim(CreatureAnimator.attackTrigger, 0f)
			.WithWaitBeforeHit(SweepFirstHitDelay, SweepFirstHitDelay)
			.WithHitFx("vfx/vfx_attack_slash", SweepSfxPath)
			.Execute(null);

		for (int i = 1; i < SweepHits; i++)
		{
			await Cmd.CustomScaledWait(SweepFollowUpHitDelay, SweepFollowUpHitDelay);
			await DamageCmd.Attack(SweepDamage)
				.FromMonster(this)
				.WithNoAttackerAnim()
				.WithHitFx("vfx/vfx_attack_slash", SweepSfxPath)
				.Execute(null);
		}
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState idle = new("Idle", isLooping: true);
		AnimState attack = new("Attack")
		{
			NextState = idle
		};
		AnimState die = new("Die");

		CreatureAnimator animator = new(idle, controller);
		animator.AddAnyState(CreatureAnimator.idleTrigger, idle);
		animator.AddAnyState(CreatureAnimator.attackTrigger, attack);
		animator.AddAnyState(CreatureAnimator.deathTrigger, die);
		return animator;
	}
}
