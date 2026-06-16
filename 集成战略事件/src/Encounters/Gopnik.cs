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

public sealed class Gopnik : MonsterModel
{
	public const string HammerMoveId = "HAMMER_MOVE";

	private const int InitialHp = 250;
	private const int HammerDamage = 25;
	private const decimal ArtifactAmount = 1m;
	private const float HammerHitDelay = 0.95f;
	private const string HammerSfxPath =
		"event:/sfx/enemy/enemy_attacks/punch_construct/punch_construct_attack_single";

	public override int MinInitialHp => InitialHp;

	public override int MaxInitialHp => InitialHp;

	public override bool HasDeathSfx => false;

	public override bool HasHurtSfx => false;

	public override bool ShouldFadeAfterDeath => true;

	public override float DeathAnimLengthOverride => 1.2f;

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Fur;

	protected override string VisualsPath => "res://IntegratedStrategyEvents/scenes/creature_visuals/gopnik.tscn";

	public override Vector2 ExtraDeathVfxPadding => Vector2.One * 0.8f;

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		MoveState hammer = new(
			HammerMoveId,
			HammerMove,
			new SingleAttackIntent(HammerDamage),
			new BuffIntent());

		hammer.FollowUpState = hammer;
		return new MonsterMoveStateMachine([hammer], hammer);
	}

	private async Task HammerMove(IReadOnlyList<Creature> targets)
	{
		_ = targets;
		await DamageCmd.Attack(HammerDamage)
			.FromMonster(this)
			.WithAttackerAnim(CreatureAnimator.attackTrigger, 0f)
			.WithWaitBeforeHit(HammerHitDelay, HammerHitDelay)
			.WithHitFx("vfx/vfx_attack_blunt", HammerSfxPath)
			.Execute(null);

		await PowerCmd.Apply<ArtifactPower>(Creature, ArtifactAmount, Creature, null);
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
