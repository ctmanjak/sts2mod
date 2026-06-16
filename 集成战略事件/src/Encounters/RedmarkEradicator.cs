using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using IntegratedStrategyEvents.Powers;

namespace IntegratedStrategyEvents.Encounters;

public sealed class RedmarkEradicator : MonsterModel
{
	public const string SnipeMoveId = "SNIPE_MOVE";
	public const string KnifeSlashMoveId = "KNIFE_SLASH_MOVE";

	private const int InitialHp = 25;
	private const int SnipeDamage = 12;
	private const int KnifeSlashDamage = 6;
	private const float AttackHitDelay = 0.8f;
	private const string BranchStateId = "TARGET_BRANCH";
	private const string KnifeSlashTrigger = "KnifeSlashTrigger";
	private const string SnipeSfxPath =
		"event:/sfx/enemy/enemy_attacks/crossbow_ruby_raider/crossbow_ruby_raider_attack";
	private const string KnifeSlashSfxPath =
		"event:/sfx/enemy/enemy_attacks/the_kin_minion/the_kin_minion_quick_slash";

	public override int MinInitialHp => InitialHp;

	public override int MaxInitialHp => InitialHp;

	public override bool HasDeathSfx => false;

	public override bool HasHurtSfx => false;

	public override bool ShouldFadeAfterDeath => true;

	public override float DeathAnimLengthOverride => 1.2f;

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Fur;

	protected override string VisualsPath => "res://IntegratedStrategyEvents/scenes/creature_visuals/redmark_eradicator.tscn";

	public override Vector2 ExtraDeathVfxPadding => Vector2.One * 0.6f;

	public override async Task AfterAddedToRoom()
	{
		await base.AfterAddedToRoom();
		await PowerCmd.Apply<RedmarkEradicatorTacticsPower>(Creature, 1m, Creature, null, silent: true);
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		MoveState snipe = new(
			SnipeMoveId,
			SnipeMove,
			new SingleAttackIntent(SnipeDamage));
		MoveState knifeSlash = new(
			KnifeSlashMoveId,
			KnifeSlashMove,
			new SingleAttackIntent(KnifeSlashDamage));
		ConditionalBranchState branch = new(BranchStateId);

		snipe.FollowUpState = branch;
		knifeSlash.FollowUpState = branch;
		branch.AddState(snipe, HasLivingInfiltrator);
		branch.AddState(knifeSlash, () => true);
		return new MonsterMoveStateMachine([snipe, knifeSlash, branch], branch);
	}

	private bool HasLivingInfiltrator()
	{
		return Creature.CombatState?.Enemies.Any(enemy =>
			enemy.IsAlive &&
			enemy.Monster?.CanonicalInstance is RedmarkInfiltrator) == true;
	}

	private async Task SnipeMove(IReadOnlyList<Creature> targets)
	{
		_ = targets;
		await DamageCmd.Attack(SnipeDamage)
			.FromMonster(this)
			.WithAttackerAnim(CreatureAnimator.attackTrigger, 0f)
			.WithWaitBeforeHit(AttackHitDelay, AttackHitDelay)
			.WithHitFx("vfx/vfx_attack_slash", SnipeSfxPath)
			.Execute(null);
	}

	private async Task KnifeSlashMove(IReadOnlyList<Creature> targets)
	{
		_ = targets;
		await DamageCmd.Attack(KnifeSlashDamage)
			.FromMonster(this)
			.WithAttackerAnim(KnifeSlashTrigger, 0f)
			.WithWaitBeforeHit(AttackHitDelay, AttackHitDelay)
			.WithHitFx("vfx/vfx_attack_slash", KnifeSlashSfxPath)
			.Execute(null);
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState idle = new("C_Idle", isLooping: true);
		AnimState snipe = new("C_Attack")
		{
			NextState = idle
		};
		AnimState knifeSlash = new("C_Combat")
		{
			NextState = idle
		};
		AnimState die = new("C_Die");

		CreatureAnimator animator = new(idle, controller);
		animator.AddAnyState(CreatureAnimator.idleTrigger, idle);
		animator.AddAnyState(CreatureAnimator.attackTrigger, snipe);
		animator.AddAnyState(KnifeSlashTrigger, knifeSlash);
		animator.AddAnyState(CreatureAnimator.deathTrigger, die);
		return animator;
	}
}
