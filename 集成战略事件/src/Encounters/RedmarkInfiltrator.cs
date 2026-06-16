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

public sealed class RedmarkInfiltrator : MonsterModel
{
	public const string PhantomMoveId = "PHANTOM_MOVE";
	public const string BurstFireMoveId = "BURST_FIRE_MOVE";

	private const int InitialHp = 36;
	private const int SlipperyAmount = 2;
	private const int BurstFireDamage = 3;
	private const int BurstFireHits = 2;
	private const float PhantomDelay = 0.75f;
	private const float AttackFirstHitDelay = 0.7f;
	private const float AttackFollowUpHitDelay = 0.08f;
	private const string AttackSfxPath =
		"event:/sfx/enemy/enemy_attacks/turret_operator/turret_operator_attack";

	public override int MinInitialHp => InitialHp;

	public override int MaxInitialHp => InitialHp;

	public override bool HasDeathSfx => false;

	public override bool HasHurtSfx => false;

	public override bool ShouldFadeAfterDeath => true;

	public override float DeathAnimLengthOverride => 1.2f;

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Fur;

	protected override string VisualsPath => "res://IntegratedStrategyEvents/scenes/creature_visuals/redmark_infiltrator.tscn";

	public override Vector2 ExtraDeathVfxPadding => Vector2.One * 0.6f;

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		MoveState phantom = new(
			PhantomMoveId,
			PhantomMove,
			new BuffIntent());
		MoveState burstFire = new(
			BurstFireMoveId,
			BurstFireMove,
			new MultiAttackIntent(BurstFireDamage, BurstFireHits));

		phantom.FollowUpState = burstFire;
		burstFire.FollowUpState = phantom;
		return new MonsterMoveStateMachine([phantom, burstFire], phantom);
	}

	private async Task PhantomMove(IReadOnlyList<Creature> targets)
	{
		_ = targets;
		await MonsterAnimationHelper.TriggerAnimWithFixedWait(Creature, CreatureAnimator.attackTrigger, PhantomDelay);
		await PowerCmd.Apply<SlipperyPower>(Creature, SlipperyAmount, Creature, null);
	}

	private async Task BurstFireMove(IReadOnlyList<Creature> targets)
	{
		_ = targets;
		await DamageCmd.Attack(BurstFireDamage)
			.FromMonster(this)
			.WithAttackerAnim(CreatureAnimator.attackTrigger, 0f)
			.WithWaitBeforeHit(AttackFirstHitDelay, AttackFirstHitDelay)
			.WithHitFx("vfx/vfx_attack_slash", AttackSfxPath)
			.Execute(null);

		for (int i = 1; i < BurstFireHits; i++)
		{
			await Cmd.CustomScaledWait(AttackFollowUpHitDelay, AttackFollowUpHitDelay);
			await DamageCmd.Attack(BurstFireDamage)
				.FromMonster(this)
				.WithNoAttackerAnim()
				.WithHitFx("vfx/vfx_attack_slash", AttackSfxPath)
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
