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

public sealed class SarkazCasterLeader : MonsterModel
{
	public const string ShacklesMoveId = "SHACKLES_MOVE";
	public const string AttackMoveId = "ATTACK_MOVE";

	private const int InitialHp = 58;
	private const int AttackDamage = 12;
	private const int ChainsAmount = 1;
	private const float ShacklesAttackDelay = 0.75f;
	private const float AttackHitDelay = 0.75f;
	private const string AttackSfxPath =
		"event:/sfx/enemy/enemy_attacks/spectral_knight/spectral_knight_soul_flame";

	public override int MinInitialHp => InitialHp;

	public override int MaxInitialHp => InitialHp;

	public override bool HasDeathSfx => false;

	public override bool HasHurtSfx => false;

	public override bool ShouldFadeAfterDeath => true;

	public override float DeathAnimLengthOverride => 1.2f;

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Magic;

	protected override string VisualsPath => "res://IntegratedStrategyEvents/scenes/creature_visuals/sarkaz_caster_leader.tscn";

	public override Vector2 ExtraDeathVfxPadding => Vector2.One * 0.6f;

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		MoveState shackles = new(
			ShacklesMoveId,
			ShacklesMove,
			new DebuffIntent(strong: true));
		MoveState attack = new(
			AttackMoveId,
			AttackMove,
			new SingleAttackIntent(AttackDamage));

		shackles.FollowUpState = attack;
		attack.FollowUpState = attack;
		return new MonsterMoveStateMachine([shackles, attack], shackles);
	}

	private async Task ShacklesMove(IReadOnlyList<Creature> targets)
	{
		_ = targets;
		await MonsterAnimationHelper.TriggerAnimWithFixedWait(Creature, CreatureAnimator.attackTrigger, ShacklesAttackDelay);
		var combatState = Creature.CombatState;
		if (combatState != null)
		{
			IReadOnlyList<Creature> playerCreatures = combatState.Players
				.Select(static player => player.Creature)
				.Where(static creature => creature.IsAlive)
				.ToList();
			if (playerCreatures.Count > 0)
			{
				await PowerCmd.Apply<ChainsOfBindingPower>(playerCreatures, ChainsAmount, Creature, null);
			}
		}
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
