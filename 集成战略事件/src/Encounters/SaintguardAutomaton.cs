using System;
using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace IntegratedStrategyEvents.Encounters;

public sealed class SaintguardAutomaton : MonsterModel
{
	public const string AccelerateMoveId = "ACCELERATE_MOVE";
	public const string SecondAccelerateMoveId = "SECOND_ACCELERATE_MOVE";
	public const string SelfDestructMoveId = "SELF_DESTRUCT_MOVE";

	private const int InitialHp = 40;
	private const int SelfDestructDamage = 10;
	private const decimal ArtifactAmount = 1m;
	private const decimal VigorAmount = 5m;
	private const float IdleSpeedMultiplierPerAccelerate = 2f;
	private const float AccelerateDelay = 0.35f;
	private const float SelfDestructHitDelay = 0.25f;
	private const string SelfDestructTrigger = "SelfDestructTrigger";
	private const string SelfDestructSfxPath = "event:/sfx/enemy/enemy_attacks/living_fog/living_fog_explode";

	private bool _hasExploded;
	private int _accelerateUses;

	[NonSerialized]
	private MegaSprite? _spineController;

	public override int MinInitialHp => InitialHp;

	public override int MaxInitialHp => InitialHp;

	public override bool HasDeathSfx => false;

	public override bool HasHurtSfx => false;

	public override bool ShouldFadeAfterDeath => true;

	public override float DeathAnimLengthOverride => 1.2f;

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Armor;

	protected override string VisualsPath => "res://IntegratedStrategyEvents/scenes/creature_visuals/saintguard_automaton.tscn";

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

	private int AccelerateUses
	{
		get => _accelerateUses;
		set
		{
			AssertMutable();
			_accelerateUses = value;
		}
	}

	public override async Task AfterAddedToRoom()
	{
		await base.AfterAddedToRoom();
		await PowerCmd.Apply<ArtifactPower>(Creature, ArtifactAmount, Creature, null, silent: true);
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		MoveState firstAccelerate = new(
			AccelerateMoveId,
			AccelerateMove,
			new BuffIntent());
		MoveState secondAccelerate = new(
			SecondAccelerateMoveId,
			AccelerateMove,
			new BuffIntent());
		MoveState selfDestruct = new(
			SelfDestructMoveId,
			SelfDestructMove,
			new DeathBlowIntent(() => SelfDestructDamage));

		firstAccelerate.FollowUpState = secondAccelerate;
		secondAccelerate.FollowUpState = selfDestruct;
		selfDestruct.FollowUpState = firstAccelerate;
		return new MonsterMoveStateMachine([firstAccelerate, secondAccelerate, selfDestruct], firstAccelerate);
	}

	private async Task AccelerateMove(IReadOnlyList<Creature> targets)
	{
		_ = targets;
		await Cmd.Wait(AccelerateDelay);
		await PowerCmd.Apply<VigorPower>(Creature, VigorAmount, Creature, null);
		AccelerateUses++;
		SetAnimationTimeScale(GetIdleTimeScale());
	}

	private async Task SelfDestructMove(IReadOnlyList<Creature> targets)
	{
		_ = targets;
		SetAnimationTimeScale(1f);
		HasExploded = true;
		await DamageCmd.Attack(SelfDestructDamage)
			.FromMonster(this)
			.WithAttackerAnim(SelfDestructTrigger, 0.05f)
			.WithAttackerFx(null, SelfDestructSfxPath)
			.WithWaitBeforeHit(SelfDestructHitDelay, SelfDestructHitDelay)
			.WithHitVfxNode((Creature _) => NGaseousImpactVfx.Create(CombatSide.Player, CombatState, new Color("#402f45")))
			.Execute(null);

		if (Creature.IsAlive)
		{
			await CreatureCmd.Kill(Creature);
		}
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		_spineController = controller;
		AnimState idle = new("Idle", isLooping: true);
		AnimState die = new("Die");

		CreatureAnimator animator = new(idle, controller);
		animator.AddAnyState(CreatureAnimator.idleTrigger, idle);
		animator.AddAnyState(CreatureAnimator.attackTrigger, die);
		animator.AddAnyState(SelfDestructTrigger, die);
		animator.AddAnyState(CreatureAnimator.deathTrigger, die, () => !HasExploded);
		SetAnimationTimeScale(GetIdleTimeScale());
		return animator;
	}

	private float GetIdleTimeScale()
	{
		return MathF.Pow(IdleSpeedMultiplierPerAccelerate, AccelerateUses);
	}

	private void SetAnimationTimeScale(float timeScale)
	{
		_spineController?.GetAnimationState().SetTimeScale(timeScale);
	}
}
