using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace IntegratedStrategyEvents.Encounters;

public sealed class RemainingCreativity : MonsterModel
{
	public const string SelfDestructMoveId = "SELF_DESTRUCT_MOVE";

	private const int InitialHp = 24;
	private const int SelfDestructDamage = 12;
	private const string SelfDestructTrigger = "ExplodeTrigger";
	private const string SelfDestructSfxPath = "event:/sfx/enemy/enemy_attacks/living_fog/living_fog_explode";
	private const string DeathSfxPath = "event:/sfx/enemy/enemy_attacks/living_fog/living_fog_minion_die";

	private bool _hasExploded;
	private bool _appliesMinionPower = true;

	public override int MinInitialHp => InitialHp;

	public override int MaxInitialHp => InitialHp;

	public override bool HasHurtSfx => false;

	public override bool ShouldFadeAfterDeath => false;

	public override string DeathSfx => DeathSfxPath;

	public override float DeathAnimLengthOverride => 1.2f;

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Magic;

	protected override string VisualsPath => "res://IntegratedStrategyEvents/scenes/creature_visuals/remaining_creativity.tscn";

	public override Vector2 ExtraDeathVfxPadding => Vector2.One * 0.5f;

	private bool HasExploded
	{
		get => _hasExploded;
		set
		{
			AssertMutable();
			_hasExploded = value;
		}
	}

	public bool AppliesMinionPower
	{
		get => _appliesMinionPower;
		set
		{
			AssertMutable();
			_appliesMinionPower = value;
		}
	}

	public override async Task AfterAddedToRoom()
	{
		await base.AfterAddedToRoom();
		if (AppliesMinionPower)
		{
			await PowerCmd.Apply<MinionPower>(Creature, 1m, Creature, null);
		}
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		MoveState selfDestruct = new(
			SelfDestructMoveId,
			SelfDestructMove,
			new DeathBlowIntent(() => SelfDestructDamage));

		selfDestruct.FollowUpState = selfDestruct;
		return new MonsterMoveStateMachine([selfDestruct], selfDestruct);
	}

	private async Task SelfDestructMove(IReadOnlyList<Creature> targets)
	{
		_ = targets;
		HasExploded = true;
		await DamageCmd.Attack(SelfDestructDamage)
			.FromMonster(this)
			.WithAttackerAnim(SelfDestructTrigger, 0.1f)
			.WithAttackerFx(null, SelfDestructSfxPath)
			.WithHitVfxNode((Creature _) => NGaseousImpactVfx.Create(CombatSide.Player, CombatState, new Color("#402f45")))
			.Execute(null);

		if (Creature.IsAlive)
		{
			await CreatureCmd.Kill(Creature);
		}
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState idle = new("Idle", isLooping: true);
		AnimState selfDestruct = new("Die");
		AnimState die = new("Die");

		CreatureAnimator animator = new(idle, controller);
		animator.AddAnyState(CreatureAnimator.idleTrigger, idle);
		animator.AddAnyState(SelfDestructTrigger, selfDestruct);
		animator.AddAnyState(CreatureAnimator.attackTrigger, selfDestruct);
		animator.AddAnyState(CreatureAnimator.deathTrigger, die, () => !HasExploded);
		return animator;
	}
}
