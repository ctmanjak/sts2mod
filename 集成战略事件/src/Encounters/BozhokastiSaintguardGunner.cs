using Godot;
using IntegratedStrategyEvents.Powers;
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
using MegaCrit.Sts2.Core.ValueProps;

namespace IntegratedStrategyEvents.Encounters;

public sealed class BozhokastiSaintguardGunner : MonsterModel
{
	public const string BossSlot = "bozhokasti";
	public const string SummonLeftSlot = "bozhokasti_summon_left";
	public const string SummonRightSlot = "bozhokasti_summon_right";

	public const string ReviveMoveId = "REVIVE_MOVE";
	public const string AutoDefenseRiteMoveId = "AUTO_DEFENSE_RITE_MOVE";
	public const string ButtStrikeMoveId = "BUTT_STRIKE_MOVE";
	public const string HeavyButtStrikeMoveId = "HEAVY_BUTT_STRIKE_MOVE";
	public const string HoldShieldMoveId = "HOLD_SHIELD_MOVE";
	public const string PhaseTwoAutoDefenseRiteMoveId = "PHASE_TWO_AUTO_DEFENSE_RITE_MOVE";
	public const string LoadAmmoRiteMoveId = "LOAD_AMMO_RITE_MOVE";
	public const string SweepFireRiteMoveId = "SWEEP_FIRE_RITE_MOVE";
	public const string HolyCityCareRiteMoveId = "HOLY_CITY_CARE_RITE_MOVE";

	private const int InitialHp = 300;
	private const decimal InitialSaintguardShield = 3m;
	private const int ButtStrikeDamage = 15;
	private const int HeavyButtStrikeDamage = 10;
	private const int HeavyButtStrikeHits = 2;
	private const decimal HoldShieldStrengthGain = 2m;
	private const decimal HoldShieldBlockGain = 20m;
	private const decimal LoadAmmoStrengthGain = 1m;
	private const int SweepFireDamage = 3;
	private const int SweepFireHits = 8;
	private const decimal HolyCityCarePlating = 10m;

	private const float SummonDelay = 0.9f;
	private const float ButtStrikeHitDelay = 0.55f;
	private const float HeavyButtStrikeHitDelay = 0.6f;
	private const float HoldShieldDelay = 0.45f;
	private const float ReviveDelay = 1.1f;
	private const float LoadAmmoDelay = 0.85f;
	private const float SweepFireHitDelay = 0.03f;
	private const float SweepFireEndDelay = 0.25f;
	private const float HolyCityCareDelay = 0.9f;

	private const string PhaseOneSummonTrigger = "PhaseOneSummon";
	private const string PhaseOneAttackTrigger = "PhaseOneAttack";
	private const string ReviveEndTrigger = "ReviveEnd";
	private const string PhaseTwoSummonTrigger = "PhaseTwoSummon";
	private const string PhaseTwoLoadAmmoTrigger = "PhaseTwoLoadAmmo";
	private const string PhaseTwoSweepFireTrigger = "PhaseTwoSweepFire";
	private const string PhaseTwoSweepFireEndTrigger = "PhaseTwoSweepFireEnd";
	private const string PhaseTwoHolyCityCareTrigger = "PhaseTwoHolyCityCare";
	private const string RifleButtSfxPath =
		"event:/sfx/enemy/enemy_attacks/punch_construct/punch_construct_attack_single";
	private const string SweepFireSfxPath =
		"event:/sfx/enemy/enemy_attacks/turret_operator/turret_operator_attack";

	private MoveState? _reviveState;
	private bool _hasRevived;
	private bool _isReviving;
	private bool _secondPhase;

	public override int MinInitialHp => InitialHp;

	public override int MaxInitialHp => InitialHp;

	public override bool HasDeathSfx => false;

	public override bool HasHurtSfx => false;

	public override bool ShouldFadeAfterDeath => true;

	public override bool ShouldDisappearFromDoom => HasRevived;

	public override float DeathAnimLengthOverride => 1.5f;

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Armor;

	protected override string VisualsPath => "res://IntegratedStrategyEvents/scenes/creature_visuals/bozhokasti_saintguard_gunner.tscn";

	public override Vector2 ExtraDeathVfxPadding => Vector2.One * 0.8f;

	public bool HasRevived => _hasRevived;

	private bool IsSecondPhase => _secondPhase;

	public override async Task AfterAddedToRoom()
	{
		await base.AfterAddedToRoom();
		await PowerCmd.Apply<SaintguardShieldPower>(Creature, InitialSaintguardShield, Creature, null, silent: true);
	}

	public override async Task AfterDeath(
		PlayerChoiceContext choiceContext,
		Creature creature,
		bool wasRemovalPrevented,
		float deathAnimLength)
	{
		_ = choiceContext;
		_ = deathAnimLength;
		if (wasRemovalPrevented || creature != Creature || HasRevived || _isReviving)
		{
			return;
		}

		_isReviving = true;
		await TriggerReviveWaitingState();
	}

	public override bool ShouldAllowHitting(Creature creature)
	{
		return creature != Creature || !_isReviving;
	}

	public override bool ShouldStopCombatFromEnding()
	{
		return Creature is { IsDead: true } && (_isReviving || !HasRevived);
	}

	public override bool ShouldCreatureBeRemovedFromCombatAfterDeath(Creature creature)
	{
		return creature != Creature || HasRevived;
	}

	private Task TriggerReviveWaitingState()
	{
		if (HasRevived || _reviveState == null)
		{
			return Task.CompletedTask;
		}

		SetMoveImmediate(_reviveState, forceTransition: true);
		return Task.CompletedTask;
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		MoveState revive = new(
			ReviveMoveId,
			ReviveMove,
			new HealIntent(),
			new BuffIntent())
		{
			MustPerformOnceBeforeTransitioning = true
		};
		MoveState autoDefenseRite = new(
			AutoDefenseRiteMoveId,
			AutoDefenseRiteMove,
			new SummonIntent());
		MoveState buttStrike = new(
			ButtStrikeMoveId,
			ButtStrikeMove,
			new SingleAttackIntent(ButtStrikeDamage));
		MoveState heavyButtStrike = new(
			HeavyButtStrikeMoveId,
			HeavyButtStrikeMove,
			new MultiAttackIntent(HeavyButtStrikeDamage, HeavyButtStrikeHits));
		MoveState holdShield = new(
			HoldShieldMoveId,
			HoldShieldMove,
			new DefendIntent(),
			new BuffIntent());
		MoveState phaseTwoAutoDefenseRite = new(
			PhaseTwoAutoDefenseRiteMoveId,
			PhaseTwoAutoDefenseRiteMove,
			new SummonIntent());
		MoveState loadAmmoRite = new(
			LoadAmmoRiteMoveId,
			LoadAmmoRiteMove,
			new BuffIntent());
		MoveState sweepFireRite = new(
			SweepFireRiteMoveId,
			SweepFireRiteMove,
			new MultiAttackIntent(SweepFireDamage, SweepFireHits));
		MoveState holyCityCareRite = new(
			HolyCityCareRiteMoveId,
			HolyCityCareRiteMove,
			new BuffIntent());

		_reviveState = revive;

		autoDefenseRite.FollowUpState = buttStrike;
		buttStrike.FollowUpState = heavyButtStrike;
		heavyButtStrike.FollowUpState = holdShield;
		holdShield.FollowUpState = autoDefenseRite;

		revive.FollowUpState = phaseTwoAutoDefenseRite;
		phaseTwoAutoDefenseRite.FollowUpState = loadAmmoRite;
		loadAmmoRite.FollowUpState = sweepFireRite;
		sweepFireRite.FollowUpState = holyCityCareRite;
		holyCityCareRite.FollowUpState = phaseTwoAutoDefenseRite;

		return new MonsterMoveStateMachine(
			[
				revive,
				autoDefenseRite,
				buttStrike,
				heavyButtStrike,
				holdShield,
				phaseTwoAutoDefenseRite,
				loadAmmoRite,
				sweepFireRite,
				holyCityCareRite
			],
			autoDefenseRite);
	}

	private async Task ReviveMove(IReadOnlyList<Creature> targets)
	{
		_ = targets;
		await MonsterAnimationHelper.TriggerAnimWithFixedWait(Creature, ReviveEndTrigger, ReviveDelay);
		_secondPhase = true;
		await CreatureCmd.Heal(Creature, Creature.MaxHp);
		_hasRevived = true;
		_isReviving = false;
	}

	private async Task AutoDefenseRiteMove(IReadOnlyList<Creature> targets)
	{
		_ = targets;
		await MonsterAnimationHelper.TriggerAnimWithFixedWait(Creature, PhaseOneSummonTrigger, SummonDelay);
		await SummonSaintguardAutomata();
	}

	private async Task ButtStrikeMove(IReadOnlyList<Creature> targets)
	{
		_ = targets;
		await DamageCmd.Attack(ButtStrikeDamage)
			.FromMonster(this)
			.WithAttackerAnim(PhaseOneAttackTrigger, 0f)
			.WithWaitBeforeHit(ButtStrikeHitDelay, ButtStrikeHitDelay)
			.WithHitFx("vfx/vfx_attack_blunt", RifleButtSfxPath)
			.Execute(null);
	}

	private async Task HeavyButtStrikeMove(IReadOnlyList<Creature> targets)
	{
		_ = targets;
		await DamageCmd.Attack(HeavyButtStrikeDamage)
			.FromMonster(this)
			.WithHitCount(HeavyButtStrikeHits)
			.WithAttackerAnim(PhaseOneAttackTrigger, 0f)
			.WithWaitBeforeHit(HeavyButtStrikeHitDelay, HeavyButtStrikeHitDelay)
			.WithHitFx("vfx/vfx_attack_blunt", RifleButtSfxPath)
			.OnlyPlayAnimOnce()
			.Execute(null);
	}

	private async Task HoldShieldMove(IReadOnlyList<Creature> targets)
	{
		_ = targets;
		await Cmd.Wait(HoldShieldDelay);
		await PowerCmd.Apply<StrengthPower>(Creature, HoldShieldStrengthGain, Creature, null);
		await CreatureCmd.GainBlock(Creature, HoldShieldBlockGain, ValueProp.Unpowered, null);
	}

	private async Task PhaseTwoAutoDefenseRiteMove(IReadOnlyList<Creature> targets)
	{
		_ = targets;
		await MonsterAnimationHelper.TriggerAnimWithFixedWait(Creature, PhaseTwoSummonTrigger, SummonDelay);
		await SummonSaintguardAutomata();
	}

	private async Task LoadAmmoRiteMove(IReadOnlyList<Creature> targets)
	{
		_ = targets;
		await MonsterAnimationHelper.TriggerAnimWithFixedWait(Creature, PhaseTwoLoadAmmoTrigger, LoadAmmoDelay);
		await PowerCmd.Apply<StrengthPower>(Creature, LoadAmmoStrengthGain, Creature, null);
	}

	private async Task SweepFireRiteMove(IReadOnlyList<Creature> targets)
	{
		_ = targets;
		await CreatureCmd.TriggerAnim(Creature, PhaseTwoSweepFireTrigger, 0f);
		await DamageCmd.Attack(SweepFireDamage)
			.FromMonster(this)
			.WithHitCount(SweepFireHits)
			.WithNoAttackerAnim()
			.WithWaitBeforeHit(SweepFireHitDelay, SweepFireHitDelay)
			.WithHitFx("vfx/vfx_attack_blunt", SweepFireSfxPath)
			.Execute(null);
		await MonsterAnimationHelper.TriggerAnimWithFixedWait(Creature, PhaseTwoSweepFireEndTrigger, SweepFireEndDelay);
	}

	private async Task HolyCityCareRiteMove(IReadOnlyList<Creature> targets)
	{
		_ = targets;
		await MonsterAnimationHelper.TriggerAnimWithFixedWait(Creature, PhaseTwoHolyCityCareTrigger, HolyCityCareDelay);
		await PowerCmd.Apply<PlatingPower>(Creature, HolyCityCarePlating, Creature, null);
	}

	private async Task SummonSaintguardAutomata()
	{
		await SummonSaintguardAutomaton(SummonLeftSlot);
		await SummonSaintguardAutomaton(SummonRightSlot);
	}

	private async Task SummonSaintguardAutomaton(string slotName)
	{
		if (CombatState == null || Creature.IsDead || IsSummonSlotOccupied(slotName))
		{
			return;
		}

		Creature minion = await CreatureCmd.Add<SaintguardAutomaton>(CombatState, slotName);
		if (!minion.HasPower<MinionPower>())
		{
			await PowerCmd.Apply<MinionPower>(minion, 1m, Creature, null, silent: true);
		}
	}

	private bool IsSummonSlotOccupied(string slotName)
	{
		return CombatState?.Enemies.Any(creature => creature.IsAlive && creature.SlotName == slotName) == true;
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState phaseOneIdle = new("C1_Idle", isLooping: true);
		AnimState phaseTwoIdle = new("C2_Idle", isLooping: true);
		AnimState phaseOneSummon = new("C1_Skill_1")
		{
			NextState = phaseOneIdle
		};
		AnimState phaseOneAttack = new("C1_Attack")
		{
			NextState = phaseOneIdle
		};
		AnimState phaseOneDieLoop = new("C1_Die_Loop", isLooping: true);
		AnimState phaseOneDie = new("C1_Die")
		{
			NextState = phaseOneDieLoop
		};
		AnimState reviveEnd = new("C1_Die_End")
		{
			NextState = phaseTwoIdle
		};
		AnimState phaseTwoSummon = new("C2_Skill_1")
		{
			NextState = phaseTwoIdle
		};
		AnimState loadAmmoIdle = new("C2_Skill_2_Idle", isLooping: true);
		AnimState loadAmmoBegin = new("C2_Skill_2_Begin")
		{
			NextState = loadAmmoIdle
		};
		AnimState sweepFireEnd = new("C2_Skill_2_End")
		{
			NextState = phaseTwoIdle
		};
		AnimState sweepFireLoop = new("C2_Skill_2_Loop", isLooping: true);
		AnimState holyCityCareEnd = new("C2_Skill_3_End")
		{
			NextState = phaseTwoIdle
		};
		AnimState holyCityCareBegin = new("C2_Skill_3_Begin")
		{
			NextState = holyCityCareEnd
		};
		AnimState phaseTwoDie = new("C2_Die");

		CreatureAnimator animator = new(phaseOneIdle, controller);
		animator.AddAnyState(CreatureAnimator.idleTrigger, phaseOneIdle, () => !IsSecondPhase);
		animator.AddAnyState(CreatureAnimator.idleTrigger, phaseTwoIdle, () => IsSecondPhase);
		animator.AddAnyState(PhaseOneSummonTrigger, phaseOneSummon);
		animator.AddAnyState(PhaseOneAttackTrigger, phaseOneAttack);
		animator.AddAnyState(ReviveEndTrigger, reviveEnd);
		animator.AddAnyState(PhaseTwoSummonTrigger, phaseTwoSummon);
		animator.AddAnyState(PhaseTwoLoadAmmoTrigger, loadAmmoBegin);
		animator.AddAnyState(PhaseTwoSweepFireTrigger, sweepFireLoop);
		animator.AddAnyState(PhaseTwoSweepFireEndTrigger, sweepFireEnd);
		animator.AddAnyState(PhaseTwoHolyCityCareTrigger, holyCityCareBegin);
		animator.AddAnyState(CreatureAnimator.deathTrigger, phaseOneDie, () => !HasRevived);
		animator.AddAnyState(CreatureAnimator.deathTrigger, phaseTwoDie, () => HasRevived);
		return animator;
	}
}
