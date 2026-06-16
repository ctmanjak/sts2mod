using IntegratedStrategyEvents.Powers;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace IntegratedStrategyEvents.Encounters;

public sealed class LugalszargusCalendarKing : DecisiveDuelBoss
{
	public const string HamstringMoveId = "HAMSTRING_MOVE";
	public const string PrepareMoveId = "PREPARE_MOVE";
	public const string EdictMoveId = "EDICT_MOVE";
	public const string ForgeSwordMoveId = "FORGE_SWORD_MOVE";
	public const string FlashStepMoveId = "FLASH_STEP_MOVE";

	private const int InitialHpValue = 500;
	private const int HamstringDamage = 10;
	private const int PlayerWeakAmount = 1;
	private const int NonPlayerWeakAmount = 2;
	private const int EdictDamage = 30;
	private const int ForgeSwordPainfulStabsAmount = 1;
	private const int FlashStepDamage = 6;
	private const int FlashStepHits = 3;
	private const float HamstringHitDelay = 0.8f;
	private const float PrepareDelay = 0.8f;
	private const float EdictHitDelay = 1.0f;
	private const float ForgeSwordDelay = 0.8f;
	private const float FlashStepHitDelay = 0.9f;
	private const string PrepareTrigger = "PrepareTrigger";
	private const string EdictTrigger = "EdictTrigger";
	private const string FlashStepTrigger = "FlashStepTrigger";

	protected override int InitialHp => InitialHpValue;

	protected override string VisualsPath =>
		"res://IntegratedStrategyEvents/scenes/creature_visuals/lugalszargus_calendar_king.tscn";

	public override async Task AfterAddedToRoom()
	{
		await base.AfterAddedToRoom();
		await PowerCmd.Apply<LugalszargusDecisivePower>(Creature, 1m, Creature, null, silent: true);
	}

	public override async Task BeforeDeath(Creature creature)
	{
		if (ReferenceEquals(creature, Creature))
		{
			await PowerCmd.Remove<PainfulStabsPower>(Creature);
		}
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		MoveState hamstring = new(
			HamstringMoveId,
			HamstringMove,
			new SingleAttackIntent(HamstringDamage),
			new DebuffIntent());
		MoveState prepare = new(
			PrepareMoveId,
			PrepareMove,
			new BuffIntent());
		MoveState edict = new(
			EdictMoveId,
			EdictMove,
			new SingleAttackIntent(EdictDamage));
		MoveState forgeSword = new(
			ForgeSwordMoveId,
			ForgeSwordMove,
			new BuffIntent());
		MoveState flashStep = new(
			FlashStepMoveId,
			FlashStepMove,
			new MultiAttackIntent(FlashStepDamage, FlashStepHits));

		hamstring.FollowUpState = prepare;
		prepare.FollowUpState = edict;
		edict.FollowUpState = forgeSword;
		forgeSword.FollowUpState = flashStep;
		flashStep.FollowUpState = hamstring;
		return new MonsterMoveStateMachine([hamstring, prepare, edict, forgeSword, flashStep], hamstring);
	}

	private async Task HamstringMove(IReadOnlyList<Creature> targets)
	{
		_ = targets;
		await AttackAllOtherUnits(HamstringDamage, CreatureAnimator.attackTrigger, HamstringHitDelay);
		await ApplyToOtherLivingUnits<WeakPower>(PlayerWeakAmount, NonPlayerWeakAmount);
	}

	private async Task PrepareMove(IReadOnlyList<Creature> targets)
	{
		_ = targets;
		await PrepareWithLoopingAnimation(PrepareTrigger, PrepareDelay);
	}

	private async Task EdictMove(IReadOnlyList<Creature> targets)
	{
		_ = targets;
		await AttackAllOtherUnitsWithCrashLandingVfx<HaranduhEarthwhip>(EdictDamage, EdictTrigger, EdictHitDelay);
	}

	private async Task ForgeSwordMove(IReadOnlyList<Creature> targets)
	{
		_ = targets;
		await TriggerAnimWithFixedWait(VictoryTrigger, ForgeSwordDelay);
		await PowerCmd.Apply<PainfulStabsPower>(Creature, ForgeSwordPainfulStabsAmount, Creature, null);
	}

	private async Task FlashStepMove(IReadOnlyList<Creature> targets)
	{
		_ = targets;
		await AttackAllOtherUnits(FlashStepDamage, FlashStepTrigger, FlashStepHitDelay, FlashStepHits);
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState idle = new("Idle", isLooping: true);
		AnimState slash = new("Attack")
		{
			NextState = idle
		};
		AnimState prepareBegin = new("Skill_2_Begin");
		AnimState prepareLoop = new("Skill_2_Loop", isLooping: true);
		AnimState edict = new("Skill_2_End")
		{
			NextState = idle
		};
		AnimState flashStepBegin = new("Skill_1_Begin");
		AnimState flashStepEnd = new("Skill_1_End")
		{
			NextState = idle
		};
		AnimState victory = new("Victory")
		{
			NextState = idle
		};
		AnimState die = new("Die");

		prepareBegin.NextState = prepareLoop;
		flashStepBegin.NextState = flashStepEnd;

		CreatureAnimator animator = new(idle, controller);
		animator.AddAnyState(CreatureAnimator.idleTrigger, idle);
		animator.AddAnyState(CreatureAnimator.attackTrigger, slash);
		animator.AddAnyState(PrepareTrigger, prepareBegin);
		animator.AddAnyState(EdictTrigger, edict);
		animator.AddAnyState(FlashStepTrigger, flashStepBegin);
		animator.AddAnyState(VictoryTrigger, victory);
		animator.AddAnyState(CreatureAnimator.deathTrigger, die);
		return animator;
	}
}
