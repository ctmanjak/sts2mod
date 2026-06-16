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

public sealed class FinalChorale : MonsterModel
{
	public const string AttackMoveId = "ATTACK_MOVE";
	public const int InitialHp = 1000;

	private const int AttackDamage = 3;
	private const int StrengthGain = 1;
	private const float AttackHitDelay = 2f;
	private const string AttackSfxPath = "event:/sfx/enemy/enemy_attacks/soul_fysh/soul_fysh_wave";

	public override int MinInitialHp => InitialHp;

	public override int MaxInitialHp => InitialHp;

	public override bool HasDeathSfx => false;

	public override bool HasHurtSfx => false;

	public override bool ShouldFadeAfterDeath => true;

	public override float DeathAnimLengthOverride => 1.2f;

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Magic;

	protected override string VisualsPath => "res://IntegratedStrategyEvents/scenes/creature_visuals/final_chorale.tscn";

	public override Vector2 ExtraDeathVfxPadding => Vector2.One * 0.75f;

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		MoveState attack = new(
			AttackMoveId,
			AttackMove,
			new SingleAttackIntent(AttackDamage),
			new BuffIntent());

		attack.FollowUpState = attack;
		return new MonsterMoveStateMachine([attack], attack);
	}

	private async Task AttackMove(IReadOnlyList<Creature> targets)
	{
		_ = targets;
		await DamageCmd.Attack(AttackDamage)
			.FromMonster(this)
			.WithAttackerAnim("Attack", 0f)
			.WithWaitBeforeHit(AttackHitDelay, AttackHitDelay)
			.WithHitFx("vfx/vfx_attack_slash", AttackSfxPath)
			.Execute(null);
		await PowerCmd.Apply<StrengthPower>(Creature, StrengthGain, Creature, null);
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
