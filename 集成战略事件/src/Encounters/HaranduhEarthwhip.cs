using IntegratedStrategyEvents.Powers;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace IntegratedStrategyEvents.Encounters;

public sealed class HaranduhEarthwhip : DecisiveDuelBoss
{
	public const string SlashMoveId = "SLASH_MOVE";
	public const string HeavySlashMoveId = "HAMSTRING_MOVE";
	public const string PrepareMoveId = "PREPARE_MOVE";
	public const string ConquerMoveId = "CONQUER_MOVE";
	public const string SharpenBladeMoveId = "SHARPEN_BLADE_MOVE";

	private const int InitialHpValue = 500;
	private const int SlashDamage = 8;
	private const int SlashHits = 2;
	private const int HeavySlashDamage = 10;
	private const int PlayerVulnerableAmount = 1;
	private const int NonPlayerVulnerableAmount = 2;
	private const int ConquerDamage = 30;
	private const int SharpenBladeStrengthAmount = 2;
	private const float SlashHitDelay = 0.8f;
	private const float HeavySlashHitDelay = 0.8f;
	private const float PrepareDelay = 0.8f;
	private const float ConquerHitDelay = 1.0f;
	private const float SharpenBladeDelay = 0.8f;
	private const string PrepareTrigger = "PrepareTrigger";
	private const string ConquerTrigger = "ConquerTrigger";

	protected override int InitialHp => InitialHpValue;

	protected override string VisualsPath =>
		"res://IntegratedStrategyEvents/scenes/creature_visuals/haranduh_earthwhip.tscn";

	public override async Task AfterAddedToRoom()
	{
		await base.AfterAddedToRoom();
		await PowerCmd.Apply<HaranduhDecisivePower>(Creature, 1m, Creature, null, silent: true);
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		MoveState slash = new(
			SlashMoveId,
			SlashMove,
			new MultiAttackIntent(SlashDamage, SlashHits));
		MoveState heavySlash = new(
			HeavySlashMoveId,
			HeavySlashMove,
			new SingleAttackIntent(HeavySlashDamage),
			new DebuffIntent());
		MoveState prepare = new(
			PrepareMoveId,
			PrepareMove,
			new BuffIntent());
		MoveState conquer = new(
			ConquerMoveId,
			ConquerMove,
			new SingleAttackIntent(ConquerDamage));
		MoveState sharpenBlade = new(
			SharpenBladeMoveId,
			SharpenBladeMove,
			new BuffIntent());

		slash.FollowUpState = heavySlash;
		heavySlash.FollowUpState = prepare;
		prepare.FollowUpState = conquer;
		conquer.FollowUpState = sharpenBlade;
		sharpenBlade.FollowUpState = slash;
		return new MonsterMoveStateMachine([slash, heavySlash, prepare, conquer, sharpenBlade], slash);
	}

	private async Task SlashMove(IReadOnlyList<Creature> targets)
	{
		_ = targets;
		await AttackAllOtherUnits(SlashDamage, CreatureAnimator.attackTrigger, SlashHitDelay, SlashHits);
	}

	private async Task HeavySlashMove(IReadOnlyList<Creature> targets)
	{
		_ = targets;
		await AttackAllOtherUnits(HeavySlashDamage, CreatureAnimator.attackTrigger, HeavySlashHitDelay);
		await ApplyToOtherLivingUnits<VulnerablePower>(PlayerVulnerableAmount, NonPlayerVulnerableAmount);
	}

	private async Task PrepareMove(IReadOnlyList<Creature> targets)
	{
		_ = targets;
		await PrepareWithLoopingAnimation(PrepareTrigger, PrepareDelay);
	}

	private async Task ConquerMove(IReadOnlyList<Creature> targets)
	{
		_ = targets;
		await AttackAllOtherUnitsWithCrashLandingVfx<LugalszargusCalendarKing>(ConquerDamage, ConquerTrigger, ConquerHitDelay);
	}

	private async Task SharpenBladeMove(IReadOnlyList<Creature> targets)
	{
		_ = targets;
		await TriggerAnimWithFixedWait(VictoryTrigger, SharpenBladeDelay);
		await PowerCmd.Apply<StrengthPower>(Creature, SharpenBladeStrengthAmount, Creature, null);
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState idle = new("Idle", isLooping: true);
		AnimState attack = new("Attack")
		{
			NextState = idle
		};
		AnimState prepareBegin = new("Skill_Begin");
		AnimState prepareLoop = new("Skill_Loop", isLooping: true);
		AnimState conquer = new("Skill_End")
		{
			NextState = idle
		};
		AnimState victory = new("Victory")
		{
			NextState = idle
		};
		AnimState die = new("Die");

		prepareBegin.NextState = prepareLoop;

		CreatureAnimator animator = new(idle, controller);
		animator.AddAnyState(CreatureAnimator.idleTrigger, idle);
		animator.AddAnyState(CreatureAnimator.attackTrigger, attack);
		animator.AddAnyState(PrepareTrigger, prepareBegin);
		animator.AddAnyState(ConquerTrigger, conquer);
		animator.AddAnyState(VictoryTrigger, victory);
		animator.AddAnyState(CreatureAnimator.deathTrigger, die);
		return animator;
	}
}
