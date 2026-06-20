using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.PotionPools;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace Illaoi;

internal static class IllaoiContent
{
	public static readonly Type[] ModelTypes =
	[
		typeof(IllaoiCharacter),
		typeof(IllaoiCardPool),
		typeof(IllaoiPotionPool),
		typeof(IllaoiRelicPool),
		typeof(IllaoiStrike),
		typeof(IllaoiDefend),
		typeof(SoulTrial),
		typeof(TempleIdol),
		typeof(BuhruFootwork),
		typeof(SermonOfMotion),
		typeof(LowSweep),
		typeof(GuardedAdvance),
		typeof(TidalSlash),
		typeof(RestlessCurrent),
		typeof(WatchfulIdol),
		typeof(SpiritMark),
		typeof(GraspingLesson),
		typeof(ConsecratedGround),
		typeof(HarshSermon),
		typeof(FollowMyVoice),
		typeof(BuhruMeditation),
		typeof(CrushingWave),
		typeof(TentacleSlam),
		typeof(PriestessGuard),
		typeof(IdolRecall),
		typeof(TrialDance),
		typeof(GuardBreakingWave),
		typeof(SurgingSermon),
		typeof(LineBreaker),
		typeof(DeepMeditation),
		typeof(RhythmOfMotion),
		typeof(FervorOfMotion),
		typeof(WoundedVessel),
		typeof(IdolWard),
		typeof(VesselCrack),
		typeof(SpiritLash),
		typeof(MotionOfNagakabouros),
		typeof(OversteppingFaith),
		typeof(SpiritualPreparation),
		typeof(ProphetOfNagakabouros),
		typeof(LeapOfFaith),
		typeof(Tidecaller),
		typeof(Drain),
		typeof(RelentlessFaith),
		typeof(KrakenPriestess),
		typeof(HarrowingSermon),
		typeof(NagakabourosRising),
		typeof(AncientGodProphet),
		typeof(SoulImpact),
		typeof(TrialByMotion),
		typeof(SerpentDance),
		typeof(Undertow),
		typeof(VoiceOfTheDeep),
		typeof(TheSeaAnswers),
		typeof(DivineForm),
		typeof(RagingTide),
		typeof(RendSoul),
		typeof(TrialOfTheAncientGod),
		typeof(NagakabourosDescends),
		typeof(NagakabourosIdol),
		typeof(NagakabourosTouch),
		typeof(IllaoiTentacleMonster),
		typeof(IllaoiSoulMonster),
		typeof(IllaoiSoulLinkPower),
		typeof(IllaoiHuskPower),
		typeof(IllaoiTemporaryStrengthPower),
		typeof(IllaoiTemporaryDexterityPower),
		typeof(IllaoiFaithPower),
		typeof(IllaoiGrowTipPower),
		typeof(IllaoiTentacleTipPower),
		typeof(IllaoiCommandTipPower),
		typeof(IllaoiDrainPower),
		typeof(IllaoiAncientGodProphetPower),
		typeof(IllaoiSoulImpactPower),
		typeof(IllaoiNagakabourosDescendsPower),
		typeof(IllaoiTidecallerPower),
		typeof(IllaoiRelentlessFaithPower),
		typeof(IllaoiGrowthBlockPower),
		typeof(IllaoiRhythmOfMotionPower),
		typeof(IllaoiFervorOfMotionPower),
		typeof(IllaoiWatchfulIdolPower),
		typeof(IllaoiSeaAnswersPower),
		typeof(IllaoiNextTurnDrawPower),
		typeof(IllaoiDivineFormPower),
		typeof(IllaoiNextTurnFaithPower)
	];
}

public sealed class IllaoiCharacter : IllaoiPlaceholderCharacterModel
{
	public override Color NameColor => new("1FAF9B");

	public override CharacterGender Gender => CharacterGender.Feminine;

	public override int StartingHp => 75;

	public override int StartingGold => 99;

	public override CardPoolModel CardPool => ModelDb.CardPool<IllaoiCardPool>();

	public override PotionPoolModel PotionPool => ModelDb.PotionPool<IllaoiPotionPool>();

	public override RelicPoolModel RelicPool => ModelDb.RelicPool<IllaoiRelicPool>();

	public override IEnumerable<CardModel> StartingDeck =>
	[
		ModelDb.Card<IllaoiStrike>(),
		ModelDb.Card<IllaoiStrike>(),
		ModelDb.Card<IllaoiStrike>(),
		ModelDb.Card<IllaoiStrike>(),
		ModelDb.Card<IllaoiDefend>(),
		ModelDb.Card<IllaoiDefend>(),
		ModelDb.Card<IllaoiDefend>(),
		ModelDb.Card<IllaoiDefend>(),
		ModelDb.Card<SoulTrial>(),
		ModelDb.Card<GraspingLesson>()
	];

	public override IReadOnlyList<RelicModel> StartingRelics => [ModelDb.Relic<NagakabourosIdol>()];

	public override float AttackAnimDelay => 0.15f;

	public override float CastAnimDelay => 0.25f;

	public override Color EnergyLabelOutlineColor => new("021F1BFF");

	public override Color DialogueColor => new("0A3D38");

	public override VfxColor SpeechBubbleColor => VfxColor.Cyan;

	public override Color MapDrawingColor => new("16A086");

	public override Color RemoteTargetingLineColor => new("47D9C3FF");

	public override Color RemoteTargetingLineOutline => new("06473DFF");

	public override string CharacterSelectSfx
	{
		get
		{
			string[] paths = ModInfo.CharacterSelectSfxPaths;
			return paths[Random.Shared.Next(paths.Length)];
		}
	}

	public override string? CustomVisualPath => ModInfo.CharacterVisualsScenePath;

	public override string? CustomIconTexturePath => ModInfo.CharacterTopPanelIconPath;

	public override string? CustomIconOutlineTexturePath => ModInfo.CharacterTopPanelIconPath;

	public override string? CustomEnergyCounterIconPath => ModInfo.EnergyIconPath;

	public override string? CustomRestSiteAnimPath => ModInfo.CharacterRestSiteScenePath;

	public override string? CustomMerchantAnimPath => ModInfo.CharacterMerchantScenePath;

	public override string? CustomCharacterSelectBg => ModInfo.CharacterSelectBgScenePath;

	public override string? CustomCharacterSelectIconPath => ModInfo.CharacterSelectButtonIconPath;

	public override string? CustomCharacterSelectLockedIconPath => ModInfo.CharacterSelectButtonIconPath;

	public override string? CustomMapMarkerPath => ModInfo.MapMarkerIconPath;

	public override IEnumerable<string> ExtraCustomAssetPaths =>
	[
		ModInfo.CharacterCombatImagePath,
		ModInfo.CharacterRestSiteImagePath,
		ModInfo.EnergySpriteFontIconPath
	];

	public override IEnumerable<string> ExtraCustomCharacterSelectAssetPaths =>
	[
		ModInfo.CharacterSelectImagePath
	];

	public override List<string> GetArchitectAttackVfx()
	{
		return
		[
			"vfx/vfx_attack_blunt",
			"vfx/vfx_heavy_blunt",
			"vfx/vfx_attack_slash",
			"vfx/vfx_rock_shatter"
		];
	}
}

public sealed class IllaoiCardPool : CardPoolModel
{
	public override string Title => "illaoi";

	public override string EnergyColorName => "ironclad";

	public override string CardFrameMaterialPath => "card_frame_green";

	public override Color DeckEntryCardColor => new("3FAE83");

	public override Color EnergyOutlineColor => new("021F1BFF");

	public override bool IsColorless => false;

	protected override CardModel[] GenerateAllCards()
	{
		return
		[
			ModelDb.Card<IllaoiStrike>(),
			ModelDb.Card<IllaoiDefend>(),
			ModelDb.Card<SoulTrial>(),
			ModelDb.Card<TempleIdol>(),
			ModelDb.Card<BuhruFootwork>(),
			ModelDb.Card<SermonOfMotion>(),
			ModelDb.Card<LowSweep>(),
			ModelDb.Card<GuardedAdvance>(),
			ModelDb.Card<TidalSlash>(),
			ModelDb.Card<RestlessCurrent>(),
			ModelDb.Card<WatchfulIdol>(),
			ModelDb.Card<SpiritMark>(),
			ModelDb.Card<GraspingLesson>(),
			ModelDb.Card<ConsecratedGround>(),
			ModelDb.Card<HarshSermon>(),
			ModelDb.Card<FollowMyVoice>(),
			ModelDb.Card<BuhruMeditation>(),
			ModelDb.Card<CrushingWave>(),
			ModelDb.Card<TentacleSlam>(),
			ModelDb.Card<PriestessGuard>(),
			ModelDb.Card<IdolRecall>(),
			ModelDb.Card<TrialDance>(),
			ModelDb.Card<GuardBreakingWave>(),
			ModelDb.Card<SurgingSermon>(),
			ModelDb.Card<LineBreaker>(),
			ModelDb.Card<DeepMeditation>(),
			ModelDb.Card<RhythmOfMotion>(),
			ModelDb.Card<FervorOfMotion>(),
			ModelDb.Card<WoundedVessel>(),
			ModelDb.Card<IdolWard>(),
			ModelDb.Card<VesselCrack>(),
			ModelDb.Card<SpiritLash>(),
			ModelDb.Card<MotionOfNagakabouros>(),
			ModelDb.Card<OversteppingFaith>(),
			ModelDb.Card<SpiritualPreparation>(),
			ModelDb.Card<ProphetOfNagakabouros>(),
			ModelDb.Card<LeapOfFaith>(),
			ModelDb.Card<Tidecaller>(),
			ModelDb.Card<Drain>(),
			ModelDb.Card<RelentlessFaith>(),
			ModelDb.Card<KrakenPriestess>(),
			ModelDb.Card<HarrowingSermon>(),
			ModelDb.Card<NagakabourosRising>(),
			ModelDb.Card<AncientGodProphet>(),
			ModelDb.Card<SoulImpact>(),
			ModelDb.Card<TrialByMotion>(),
			ModelDb.Card<SerpentDance>(),
			ModelDb.Card<Undertow>(),
			ModelDb.Card<VoiceOfTheDeep>(),
			ModelDb.Card<TheSeaAnswers>(),
			ModelDb.Card<DivineForm>(),
			ModelDb.Card<RagingTide>(),
			ModelDb.Card<RendSoul>(),
			ModelDb.Card<TrialOfTheAncientGod>(),
			ModelDb.Card<NagakabourosDescends>()
		];
	}
}

public sealed class IllaoiPotionPool : PotionPoolModel
{
	public override string EnergyColorName => "ironclad";

	public override Color LabOutlineColor => new("139A84");

	protected override IEnumerable<PotionModel> GenerateAllPotions()
	{
		return [];
	}
}

public sealed class IllaoiRelicPool : RelicPoolModel
{
	public override string EnergyColorName => "ironclad";

	public override Color LabOutlineColor => new("139A84");

	protected override IEnumerable<RelicModel> GenerateAllRelics()
	{
		return
		[
			ModelDb.Relic<NagakabourosIdol>()
		];
	}
}

public sealed class IllaoiStrike : CardModel
{
	protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];

	protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(6m, ValueProp.Move)];

	public override string PortraitPath => ModInfo.IllaoiStrikePortraitPath;

	public IllaoiStrike()
		: base(1, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
		await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(choiceContext);
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Damage.UpgradeValueBy(3m);
	}
}

public sealed class IllaoiDefend : CardModel
{
	public override bool GainsBlock => true;

	protected override HashSet<CardTag> CanonicalTags => [CardTag.Defend];

	protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(5m, ValueProp.Move)];

	public override string PortraitPath => ModInfo.IllaoiDefendPortraitPath;

	public IllaoiDefend()
		: base(1, CardType.Skill, CardRarity.Basic, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Block.UpgradeValueBy(3m);
	}
}

public sealed class SoulTrial : CardModel
{
	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new DynamicVar("Turns", ModInfo.SoulDurationTurns),
		new DynamicVar("HealthPercent", ModInfo.SoulHealthRatio * 100m),
		new DynamicVar("TransferPercent", ModInfo.SoulTransferRatio * 100m),
		new DynamicVar("HuskTurns", ModInfo.SoulHuskDurationTurns)
	];

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		HoverTipFactory.FromPower<IllaoiSoulLinkPower>(),
		HoverTipFactory.FromPower<IllaoiHuskPower>()
	];

	public override string PortraitPath => ModInfo.SoulTrialPortraitPath;

	public SoulTrial()
		: base(1, CardType.Skill, CardRarity.Basic, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
		await IllaoiMechanics.SummonSoul(choiceContext, Owner, cardPlay.Target, this);
	}

	protected override void OnUpgrade()
	{
		EnergyCost.UpgradeBy(-1);
	}
}

public sealed class TempleIdol : CardModel
{
	public override bool GainsBlock => true;

	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new BlockVar(4m, ValueProp.Move),
		new DynamicVar("Times", 2m),
		new DynamicVar("TemporaryDexterity", 1m)
	];

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<IllaoiTemporaryDexterityPower>()];

	public override string PortraitPath => ModInfo.TempleIdolPortraitPath;

	public TempleIdol()
		: base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await IllaoiMechanics.ApplyTemporaryDexterity(Owner.Creature, DynamicVars["TemporaryDexterity"].BaseValue, Owner.Creature, this);
		await IllaoiMechanics.GainBlockTimes(Owner.Creature, DynamicVars.Block.BaseValue, DynamicVars["Times"].IntValue, cardPlay);
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Block.UpgradeValueBy(1m);
	}
}

public sealed class BuhruFootwork : CardModel
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

	public override bool GainsBlock => true;

	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new BlockVar(2m, ValueProp.Move),
		new DynamicVar("Times", 2m),
		new CardsVar(2)
	];

	public override string PortraitPath => ModInfo.BuhruFootworkPortraitPath;

	public BuhruFootwork()
		: base(0, CardType.Skill, CardRarity.Common, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await IllaoiMechanics.GainBlockTimes(Owner.Creature, DynamicVars.Block.BaseValue, DynamicVars["Times"].IntValue, cardPlay);
		await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner, fromHandDraw: false);
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Block.UpgradeValueBy(1m);
	}
}

public sealed class SermonOfMotion : CardModel
{
	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new DynamicVar("TemporaryStrength", 1m),
		new CardsVar(1)
	];

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<IllaoiTemporaryStrengthPower>()];

	public override string PortraitPath => ModInfo.SermonOfMotionPortraitPath;

	public SermonOfMotion()
		: base(0, CardType.Skill, CardRarity.Common, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await IllaoiMechanics.ApplyTemporaryStrength(Owner.Creature, DynamicVars["TemporaryStrength"].BaseValue, Owner.Creature, this);
		await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner, fromHandDraw: false);
	}

	protected override void OnUpgrade()
	{
		DynamicVars["TemporaryStrength"].UpgradeValueBy(1m);
	}
}

public sealed class LowSweep : CardModel
{
	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new DamageVar(3m, ValueProp.Move),
		new DynamicVar("Times", 3m)
	];

	public override string PortraitPath => ModInfo.LowSweepPortraitPath;

	public LowSweep()
		: base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
		await IllaoiMechanics.AttackTargetTimes(choiceContext, this, cardPlay.Target, DynamicVars.Damage.BaseValue, DynamicVars["Times"].IntValue, "vfx/vfx_attack_slash");
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Damage.UpgradeValueBy(1m);
	}
}

public sealed class GuardedAdvance : CardModel
{
	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new DamageVar(4m, ValueProp.Move),
		new DynamicVar("Times", 2m),
		new BlockVar(2m, ValueProp.Move)
	];

	public override string PortraitPath => ModInfo.GuardedAdvancePortraitPath;

	public GuardedAdvance()
		: base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
		await IllaoiMechanics.AttackTargetTimes(choiceContext, this, cardPlay.Target, DynamicVars.Damage.BaseValue, DynamicVars["Times"].IntValue, "vfx/vfx_attack_blunt");
		await IllaoiMechanics.GainBlockTimes(Owner.Creature, DynamicVars.Block.BaseValue, DynamicVars["Times"].IntValue, cardPlay);
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Damage.UpgradeValueBy(1m);
		DynamicVars.Block.UpgradeValueBy(1m);
	}
}

public sealed class TidalSlash : CardModel
{
	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new DamageVar(4m, ValueProp.Move),
		new DynamicVar("Times", 2m),
		new CardsVar(1)
	];

	public override string PortraitPath => ModInfo.TidalSlashPortraitPath;

	public TidalSlash()
		: base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
		await IllaoiMechanics.AttackTargetTimes(choiceContext, this, cardPlay.Target, DynamicVars.Damage.BaseValue, DynamicVars["Times"].IntValue, "vfx/vfx_attack_slash");
		await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner, fromHandDraw: false);
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Damage.UpgradeValueBy(2m);
	}
}

public sealed class RestlessCurrent : CardModel
{
	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new CardsVar(2),
		new DynamicVar("Discard", 1m),
		new DynamicVar("TemporaryStrength", 1m),
		new DynamicVar("TemporaryDexterity", 1m)
	];

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		HoverTipFactory.FromPower<IllaoiTemporaryStrengthPower>(),
		HoverTipFactory.FromPower<IllaoiTemporaryDexterityPower>()
	];

	public override string PortraitPath => ModInfo.RestlessCurrentPortraitPath;

	public RestlessCurrent()
		: base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner, fromHandDraw: false);
		IReadOnlyList<CardModel> discardedCards = await IllaoiMechanics.DiscardFromHand(choiceContext, Owner, this, DynamicVars["Discard"].IntValue);
		if (discardedCards.Any(static card => card.Type == CardType.Attack))
		{
			await IllaoiMechanics.ApplyTemporaryStrength(Owner.Creature, DynamicVars["TemporaryStrength"].BaseValue, Owner.Creature, this);
		}

		if (discardedCards.Any(static card => card.Type == CardType.Skill))
		{
			await IllaoiMechanics.ApplyTemporaryDexterity(Owner.Creature, DynamicVars["TemporaryDexterity"].BaseValue, Owner.Creature, this);
		}
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Cards.UpgradeValueBy(1m);
	}
}

public sealed class WatchfulIdol : CardModel
{
	protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(6m, ValueProp.Unpowered)];

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<IllaoiWatchfulIdolPower>()];

	public override string PortraitPath => ModInfo.WatchfulIdolPortraitPath;

	public WatchfulIdol()
		: base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		return PowerCmd.Apply<IllaoiWatchfulIdolPower>(Owner.Creature, DynamicVars.Block.BaseValue, Owner.Creature, this);
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Block.UpgradeValueBy(4m);
	}
}

public sealed class SpiritMark : CardModel
{
	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new DynamicVar("Husk", 1m),
		new DynamicVar("SoulTurns", 1m)
	];

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		HoverTipFactory.FromPower<IllaoiHuskPower>(),
		HoverTipFactory.FromPower<IllaoiSoulLinkPower>()
	];

	public override string PortraitPath => ModInfo.SpiritMarkPortraitPath;

	public SpiritMark()
		: base(0, CardType.Skill, CardRarity.Common, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ICombatState combatState = Owner.Creature.CombatState
			?? throw new InvalidOperationException("Spirit Mark played outside combat.");
		foreach (Creature enemy in combatState.Enemies.Where(static creature => creature.IsAlive).ToList())
		{
			if (IllaoiMechanics.IsSoul(enemy))
			{
				await IllaoiMechanics.ExtendSoulDuration(enemy, DynamicVars["SoulTurns"].BaseValue);
			}

			await IllaoiMechanics.ApplyHusk(enemy, DynamicVars["Husk"].BaseValue, Owner.Creature, this);
		}
	}

	protected override void OnUpgrade()
	{
		DynamicVars["Husk"].UpgradeValueBy(1m);
		DynamicVars["SoulTurns"].UpgradeValueBy(1m);
	}
}

public sealed class GraspingLesson : CardModel
{
	protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(7m, ValueProp.Move)];

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [IllaoiHoverTips.FromPowerWithoutIcon<IllaoiCommandTipPower>()];

	public override string PortraitPath => ModInfo.GraspingLessonPortraitPath;

	public GraspingLesson()
		: base(1, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
		await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(choiceContext);
		await IllaoiMechanics.CommandTentacles(choiceContext, Owner, cardPlay.Target, this);
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Damage.UpgradeValueBy(4m);
	}
}

public sealed class ConsecratedGround : CardModel
{
	public override bool GainsBlock => true;

	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new BlockVar(6m, ValueProp.Move),
		new DynamicVar("BonusBlock", 3m)
	];

	public override string PortraitPath => ModInfo.ConsecratedGroundPortraitPath;

	public ConsecratedGround()
		: base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		int tentacles = IllaoiMechanics.GetTentacleCount(Owner);
		decimal block = DynamicVars.Block.BaseValue + DynamicVars["BonusBlock"].BaseValue * tentacles;
		await CreatureCmd.GainBlock(Owner.Creature, block, ValueProp.Move, cardPlay);
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Block.UpgradeValueBy(3m);
	}
}

public sealed class HarshSermon : CardModel
{
	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new DamageVar(8m, ValueProp.Move),
		new DynamicVar("BonusDamage", 4m)
	];

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<IllaoiHuskPower>()];

	public override string PortraitPath => ModInfo.HarshSermonPortraitPath;

	public HarshSermon()
		: base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
		decimal consumedHusk = await IllaoiMechanics.ConsumeHusk(cardPlay.Target);
		decimal damage = DynamicVars.Damage.BaseValue + DynamicVars["BonusDamage"].BaseValue * consumedHusk;
		await DamageCmd.Attack(damage).FromCard(this).Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(choiceContext);
	}

	protected override void OnUpgrade()
	{
		DynamicVars["BonusDamage"].UpgradeValueBy(1m);
	}
}

public sealed class FollowMyVoice : CardModel
{
	protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("BonusCommands", 1m)];

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [IllaoiHoverTips.FromPowerWithoutIcon<IllaoiCommandTipPower>()];

	public override string PortraitPath => ModInfo.FollowMyVoicePortraitPath;

	public FollowMyVoice()
		: base(0, CardType.Skill, CardRarity.Common, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
		bool hadHusk = IllaoiMechanics.HasHusk(cardPlay.Target);
		await IllaoiMechanics.CommandTentacles(choiceContext, Owner, cardPlay.Target, this);
		if (hadHusk && cardPlay.Target.IsAlive)
		{
			await IllaoiMechanics.CommandTentaclesTimes(choiceContext, Owner, cardPlay.Target, this, DynamicVars["BonusCommands"].IntValue);
		}
	}

	protected override void OnUpgrade()
	{
	}
}

public sealed class BuhruMeditation : CardModel
{
	public override bool GainsBlock => true;

	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new BlockVar(8m, ValueProp.Move),
		new EnergyVar(1)
	];

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<EnergyNextTurnPower>()];

	public override string PortraitPath => ModInfo.BuhruMeditationPortraitPath;

	public BuhruMeditation()
		: base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
		await PowerCmd.Apply<EnergyNextTurnPower>(Owner.Creature, DynamicVars.Energy.BaseValue, Owner.Creature, this);
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Block.UpgradeValueBy(3m);
	}
}

public sealed class CrushingWave : CardModel
{
	protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(16m, ValueProp.Move)];

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [IllaoiHoverTips.FromPowerWithoutIcon<IllaoiCommandTipPower>()];

	public override string PortraitPath => ModInfo.CrushingWavePortraitPath;

	public CrushingWave()
		: base(2, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
		bool hadHusk = IllaoiMechanics.HasHusk(cardPlay.Target);
		await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_heavy_blunt")
			.Execute(choiceContext);
		await IllaoiMechanics.CommandTentaclesTimes(choiceContext, Owner, cardPlay.Target, this, hadHusk ? 2 : 1);
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Damage.UpgradeValueBy(5m);
	}
}

public sealed class TentacleSlam : CardModel
{
	protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(9m, ValueProp.Move)];

	public override string PortraitPath => ModInfo.TentacleSlamPortraitPath;

	public TentacleSlam()
		: base(1, CardType.Attack, CardRarity.Common, TargetType.AllEnemies)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ICombatState combatState = Owner.Creature.CombatState
			?? throw new InvalidOperationException("Tentacle Slam played outside combat.");
		await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).TargetingAllOpponents(combatState)
			.WithHitFx("vfx/vfx_heavy_blunt")
			.Execute(choiceContext);
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Damage.UpgradeValueBy(3m);
	}
}

public sealed class PriestessGuard : CardModel
{
	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new DamageVar(6m, ValueProp.Move),
		new DynamicVar("TemporaryStrength", 1m),
		new PowerVar<WeakPower>(1m)
	];

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		HoverTipFactory.FromPower<IllaoiTemporaryStrengthPower>(),
		HoverTipFactory.FromPower<WeakPower>()
	];

	public override string PortraitPath => ModInfo.PriestessGuardPortraitPath;

	public PriestessGuard()
		: base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
		await IllaoiMechanics.ApplyTemporaryStrength(Owner.Creature, DynamicVars["TemporaryStrength"].BaseValue, Owner.Creature, this);
		await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(choiceContext);
		if (cardPlay.Target.IsAlive)
		{
			await PowerCmd.Apply<WeakPower>(cardPlay.Target, DynamicVars.Weak.BaseValue, Owner.Creature, this);
		}
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Damage.UpgradeValueBy(2m);
	}
}

public sealed class IdolRecall : CardModel
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new DamageVar(4m, ValueProp.Move),
		new CardsVar(2),
		new DynamicVar("Discard", 2m)
	];

	public override string PortraitPath => ModInfo.IdolRecallPortraitPath;

	public IdolRecall()
		: base(0, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
		await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(choiceContext);
		await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner, fromHandDraw: false);
		await IllaoiMechanics.DiscardFromHand(choiceContext, Owner, this, DynamicVars["Discard"].IntValue);
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Damage.UpgradeValueBy(2m);
	}
}

public sealed class TrialDance : CardModel
{
	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new DamageVar(3m, ValueProp.Move),
		new DynamicVar("Times", 3m),
		new CardsVar(2)
	];

	public override string PortraitPath => ModInfo.TrialDancePortraitPath;

	public TrialDance()
		: base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
		bool hadHusk = IllaoiMechanics.HasHusk(cardPlay.Target);
		await IllaoiMechanics.AttackTargetTimes(choiceContext, this, cardPlay.Target, DynamicVars.Damage.BaseValue, DynamicVars["Times"].IntValue, "vfx/vfx_attack_slash");
		if (hadHusk)
		{
			await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner, fromHandDraw: false);
		}
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Damage.UpgradeValueBy(1m);
	}
}

public sealed class GuardBreakingWave : CardModel
{
	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new DamageVar(6m, ValueProp.Move),
		new DynamicVar("Times", 3m),
		new DynamicVar("Husk", 1m)
	];

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<IllaoiHuskPower>()];

	public override string PortraitPath => ModInfo.GuardBreakingWavePortraitPath;

	public GuardBreakingWave()
		: base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
		int times = DynamicVars["Times"].IntValue;
		for (int i = 0; i < times && cardPlay.Target.IsAlive; i++)
		{
			await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target)
				.WithHitFx("vfx/vfx_heavy_blunt")
				.Execute(choiceContext);
			if (cardPlay.Target.IsAlive)
			{
				await IllaoiMechanics.ApplyHusk(cardPlay.Target, DynamicVars["Husk"].BaseValue, Owner.Creature, this);
			}
		}
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Damage.UpgradeValueBy(2m);
	}
}

public sealed class SurgingSermon : CardModel
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => IsUpgraded ? [] : [CardKeyword.Exhaust];

	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new DynamicVar("TemporaryStrength", 2m),
		new DynamicVar("TemporaryDexterity", 2m)
	];

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		HoverTipFactory.FromPower<IllaoiTemporaryStrengthPower>(),
		HoverTipFactory.FromPower<IllaoiTemporaryDexterityPower>()
	];

	public override string PortraitPath => ModInfo.SurgingSermonPortraitPath;

	public SurgingSermon()
		: base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await IllaoiMechanics.ApplyTemporaryStrength(Owner.Creature, DynamicVars["TemporaryStrength"].BaseValue, Owner.Creature, this);
		await IllaoiMechanics.ApplyTemporaryDexterity(Owner.Creature, DynamicVars["TemporaryDexterity"].BaseValue, Owner.Creature, this);
	}

	protected override void OnUpgrade()
	{
		RemoveKeyword(CardKeyword.Exhaust);
	}
}

public sealed class LineBreaker : CardModel
{
	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new DamageVar(4m, ValueProp.Move),
		new DynamicVar("Times", 3m),
		new DynamicVar("Commands", 1m)
	];

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [IllaoiHoverTips.FromPowerWithoutIcon<IllaoiCommandTipPower>()];

	public override string PortraitPath => ModInfo.LineBreakerPortraitPath;

	public LineBreaker()
		: base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
		await IllaoiMechanics.AttackTargetTimes(choiceContext, this, cardPlay.Target, DynamicVars.Damage.BaseValue, DynamicVars["Times"].IntValue, "vfx/vfx_heavy_blunt");
		if (cardPlay.Target.IsAlive)
		{
			await IllaoiMechanics.CommandTentaclesTimes(choiceContext, Owner, cardPlay.Target, this, DynamicVars["Commands"].IntValue);
		}
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Damage.UpgradeValueBy(2m);
	}
}

public sealed class DeepMeditation : CardModel
{
	public override bool GainsBlock => true;

	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new BlockVar(4m, ValueProp.Move),
		new DynamicVar("Times", 2m),
		new DynamicVar("Faith", 1m)
	];

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<IllaoiNextTurnFaithPower>()];

	public override string PortraitPath => ModInfo.DeepMeditationPortraitPath;

	public DeepMeditation()
		: base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await IllaoiMechanics.GainBlockTimes(Owner.Creature, DynamicVars.Block.BaseValue, DynamicVars["Times"].IntValue, cardPlay);
		await PowerCmd.Apply<IllaoiNextTurnFaithPower>(Owner.Creature, DynamicVars["Faith"].BaseValue, Owner.Creature, this);
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Block.UpgradeValueBy(1m);
	}
}

public sealed class RhythmOfMotion : CardModel
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => IsUpgraded ? [CardKeyword.Innate] : [];

	protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("TemporaryDexterity", 1m)];

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<IllaoiRhythmOfMotionPower>()];

	public override string PortraitPath => ModInfo.RhythmOfMotionPortraitPath;

	public RhythmOfMotion()
		: base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		return PowerCmd.Apply<IllaoiRhythmOfMotionPower>(Owner.Creature, DynamicVars["TemporaryDexterity"].BaseValue, Owner.Creature, this);
	}

	protected override void OnUpgrade()
	{
		AddKeyword(CardKeyword.Innate);
	}
}

public sealed class FervorOfMotion : CardModel
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => IsUpgraded ? [CardKeyword.Innate] : [];

	protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("TemporaryStrength", 1m)];

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<IllaoiFervorOfMotionPower>()];

	public override string PortraitPath => ModInfo.FervorOfMotionPortraitPath;

	public FervorOfMotion()
		: base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		return PowerCmd.Apply<IllaoiFervorOfMotionPower>(Owner.Creature, DynamicVars["TemporaryStrength"].BaseValue, Owner.Creature, this);
	}

	protected override void OnUpgrade()
	{
		AddKeyword(CardKeyword.Innate);
	}
}

public sealed class WoundedVessel : CardModel
{
	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new DynamicVar("Turns", ModInfo.SoulDurationTurns),
		new PowerVar<WeakPower>(1m)
	];

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		HoverTipFactory.FromPower<IllaoiSoulLinkPower>(),
		HoverTipFactory.FromPower<IllaoiHuskPower>(),
		HoverTipFactory.FromPower<WeakPower>()
	];

	public override string PortraitPath => ModInfo.WoundedVesselPortraitPath;

	public WoundedVessel()
		: base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
		await IllaoiMechanics.SummonSoul(choiceContext, Owner, cardPlay.Target, this);
		if (cardPlay.Target.IsAlive)
		{
			Creature body = IllaoiMechanics.ResolveBodyTarget(cardPlay.Target);
			await PowerCmd.Apply<WeakPower>(body, DynamicVars.Weak.BaseValue, Owner.Creature, this);
		}
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Weak.UpgradeValueBy(1m);
	}
}

public sealed class IdolWard : CardModel
{
	public override bool GainsBlock => true;

	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new BlockVar(7m, ValueProp.Move),
		new DynamicVar("Faith", 1m)
	];

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<IllaoiFaithPower>()];

	public override string PortraitPath => ModInfo.IdolWardPortraitPath;

	public IdolWard()
		: base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
		await PowerCmd.Apply<IllaoiFaithPower>(Owner.Creature, DynamicVars["Faith"].BaseValue, Owner.Creature, this);
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Block.UpgradeValueBy(3m);
	}
}

public sealed class VesselCrack : CardModel
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new PowerVar<VulnerablePower>(2m)
	];

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		HoverTipFactory.FromPower<VulnerablePower>()
	];

	public override string PortraitPath => ModInfo.VesselCrackPortraitPath;

	public VesselCrack()
		: base(0, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
		await PowerCmd.Apply<VulnerablePower>(cardPlay.Target, DynamicVars.Vulnerable.BaseValue, Owner.Creature, this);
		await IllaoiMechanics.ApplyVulnerableToBodyIfTargetIsSoul(cardPlay.Target, DynamicVars.Vulnerable.BaseValue, Owner.Creature, this);
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Vulnerable.UpgradeValueBy(1m);
	}
}

public sealed class SpiritLash : CardModel
{
	protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(9m, ValueProp.Move)];

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		HoverTipFactory.FromPower<IllaoiHuskPower>(),
		IllaoiHoverTips.FromPowerWithoutIcon<IllaoiCommandTipPower>()
	];

	public override string PortraitPath => ModInfo.SpiritLashPortraitPath;

	public SpiritLash()
		: base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
		bool hadHusk = IllaoiMechanics.HasHusk(cardPlay.Target);
		await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(choiceContext);
		if (hadHusk && cardPlay.Target.IsAlive)
		{
			await IllaoiMechanics.CommandTentacles(choiceContext, Owner, cardPlay.Target, this);
		}
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Damage.UpgradeValueBy(3m);
	}
}

public sealed class MotionOfNagakabouros : CardModel
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new DynamicVar("Tentacles", 1m),
		new CardsVar(1)
	];

	public override string PortraitPath => ModInfo.MotionOfNagakabourosPortraitPath;

	public MotionOfNagakabouros()
		: base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await IllaoiMechanics.Grow(Owner, DynamicVars["Tentacles"].IntValue);
		await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner, fromHandDraw: false);
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Cards.UpgradeValueBy(1m);
	}
}

public sealed class OversteppingFaith : CardModel
{
	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new DamageVar(12m, ValueProp.Move),
		new DynamicVar("Tentacles", 1m)
	];

	public override string PortraitPath => ModInfo.OversteppingFaithPortraitPath;

	public OversteppingFaith()
		: base(2, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ICombatState combatState = Owner.Creature.CombatState
			?? throw new InvalidOperationException("Overstepping Faith played outside combat.");
		int enemyCount = combatState.Enemies.Count(static creature => creature.IsAlive);
		await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).TargetingAllOpponents(combatState)
			.WithHitFx("vfx/vfx_heavy_blunt")
			.Execute(choiceContext);
		await IllaoiMechanics.Grow(Owner, DynamicVars["Tentacles"].IntValue * enemyCount);
	}

	protected override void OnUpgrade()
	{
		EnergyCost.UpgradeBy(-1);
	}
}

public sealed class SpiritualPreparation : CardModel
{
	public override bool GainsBlock => true;

	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new BlockVar(5m, ValueProp.Move),
		new DynamicVar("Times", 2m),
		new EnergyVar(2),
		new CardsVar(2)
	];

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		HoverTipFactory.FromPower<EnergyNextTurnPower>(),
		HoverTipFactory.FromPower<IllaoiNextTurnDrawPower>()
	];

	public override string PortraitPath => ModInfo.SpiritualPreparationPortraitPath;

	public SpiritualPreparation()
		: base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await IllaoiMechanics.GainBlockTimes(Owner.Creature, DynamicVars.Block.BaseValue, DynamicVars["Times"].IntValue, cardPlay);
		if (IllaoiMechanics.HasAnySoul(Owner))
		{
			await PowerCmd.Apply<EnergyNextTurnPower>(Owner.Creature, DynamicVars.Energy.BaseValue, Owner.Creature, this);
			await PowerCmd.Apply<IllaoiNextTurnDrawPower>(Owner.Creature, DynamicVars.Cards.BaseValue, Owner.Creature, this);
		}
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Block.UpgradeValueBy(2m);
	}
}

public sealed class ProphetOfNagakabouros : CardModel
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new DynamicVar("Tentacles", 1m),
		new DynamicVar("Faith", 2m)
	];

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		IllaoiHoverTips.FromPowerWithoutIcon<IllaoiGrowTipPower>(),
		HoverTipFactory.FromPower<IllaoiFaithPower>()
	];

	public override string PortraitPath => ModInfo.ProphetOfNagakabourosPortraitPath;

	public ProphetOfNagakabouros()
		: base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await IllaoiMechanics.Grow(Owner, DynamicVars["Tentacles"].IntValue);
		await PowerCmd.Apply<IllaoiFaithPower>(Owner.Creature, DynamicVars["Faith"].BaseValue, Owner.Creature, this);
	}

	protected override void OnUpgrade()
	{
		EnergyCost.UpgradeBy(-1);
	}
}

public sealed class LeapOfFaith : CardModel
{
	protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(9m, ValueProp.Move)];

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		IllaoiHoverTips.FromPowerWithoutIcon<IllaoiCommandTipPower>(),
		HoverTipFactory.FromPower<IllaoiFaithPower>()
	];

	public override string PortraitPath => ModInfo.LeapOfFaithPortraitPath;

	public LeapOfFaith()
		: base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
		bool hasFaith = Owner.Creature.GetPower<IllaoiFaithPower>()?.Amount > 0;
		await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_heavy_blunt")
			.Execute(choiceContext);
		if (hasFaith && cardPlay.Target.IsAlive)
		{
			await IllaoiMechanics.CommandTentacles(choiceContext, Owner, cardPlay.Target, this);
		}
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Damage.UpgradeValueBy(3m);
	}
}

public sealed class Tidecaller : CardModel
{
	protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1)];

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		HoverTipFactory.FromPower<IllaoiTidecallerPower>(),
		IllaoiHoverTips.FromPowerWithoutIcon<IllaoiCommandTipPower>()
	];

	public override string PortraitPath => ModInfo.TidecallerPortraitPath;

	public Tidecaller()
		: base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		return PowerCmd.Apply<IllaoiTidecallerPower>(Owner.Creature, DynamicVars.Cards.BaseValue, Owner.Creature, this);
	}

	protected override void OnUpgrade()
	{
		EnergyCost.UpgradeBy(-1);
	}
}

public sealed class Drain : CardModel
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => IsUpgraded ? [CardKeyword.Innate] : [];

	protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(2m, ValueProp.Unpowered)];

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<IllaoiDrainPower>()];

	public override string PortraitPath => ModInfo.DrainPortraitPath;

	public Drain()
		: base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		return PowerCmd.Apply<IllaoiDrainPower>(Owner.Creature, DynamicVars.Block.BaseValue, Owner.Creature, this);
	}

	protected override void OnUpgrade()
	{
		AddKeyword(CardKeyword.Innate);
	}
}

public sealed class RelentlessFaith : CardModel
{
	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		HoverTipFactory.FromPower<IllaoiRelentlessFaithPower>(),
		IllaoiHoverTips.FromPowerWithoutIcon<IllaoiCommandTipPower>()
	];

	public override string PortraitPath => ModInfo.RelentlessFaithPortraitPath;

	public RelentlessFaith()
		: base(2, CardType.Power, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		return PowerCmd.Apply<IllaoiRelentlessFaithPower>(Owner.Creature, 1m, Owner.Creature, this);
	}

	protected override void OnUpgrade()
	{
		EnergyCost.UpgradeBy(-1);
	}
}

public sealed class KrakenPriestess : CardModel
{
	protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Faith", 1m)];

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		HoverTipFactory.FromPower<IllaoiGrowthBlockPower>(),
		HoverTipFactory.FromPower<IllaoiFaithPower>()
	];

	public override string PortraitPath => ModInfo.KrakenPriestessPortraitPath;

	public KrakenPriestess()
		: base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		return PowerCmd.Apply<IllaoiGrowthBlockPower>(Owner.Creature, DynamicVars["Faith"].BaseValue, Owner.Creature, this);
	}

	protected override void OnUpgrade()
	{
		EnergyCost.UpgradeBy(-1);
	}
}

public sealed class HarrowingSermon : CardModel
{
	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new DynamicVar("Turns", ModInfo.SoulDurationTurns),
		new PowerVar<VulnerablePower>(1m)
	];

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		HoverTipFactory.FromPower<IllaoiSoulLinkPower>(),
		HoverTipFactory.FromPower<VulnerablePower>()
	];

	public override string PortraitPath => ModInfo.HarrowingSermonPortraitPath;

	public HarrowingSermon()
		: base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
		Creature? soul = await IllaoiMechanics.SummonSoul(choiceContext, Owner, cardPlay.Target, this);
		if (soul is { IsAlive: true })
		{
			await PowerCmd.Apply<VulnerablePower>(soul, DynamicVars.Vulnerable.BaseValue, Owner.Creature, this);
		}
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Vulnerable.UpgradeValueBy(1m);
	}
}

public sealed class NagakabourosRising : CardModel
{
	protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(15m, ValueProp.Move)];

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		HoverTipFactory.FromPower<IllaoiHuskPower>(),
		IllaoiHoverTips.FromPowerWithoutIcon<IllaoiCommandTipPower>()
	];

	public override string PortraitPath => ModInfo.NagakabourosRisingPortraitPath;

	public NagakabourosRising()
		: base(2, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ICombatState combatState = Owner.Creature.CombatState
			?? throw new InvalidOperationException("Nagakabouros Rising played outside combat.");
		await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).TargetingAllOpponents(combatState)
			.WithHitFx("vfx/vfx_heavy_blunt")
			.Execute(choiceContext);
		await IllaoiMechanics.CommandAllHuskTargets(choiceContext, Owner, this);
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Damage.UpgradeValueBy(5m);
	}
}

public sealed class AncientGodProphet : CardModel
{
	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new DynamicVar("Tentacles", 2m),
		new DynamicVar("Faith", 1m)
	];

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		HoverTipFactory.FromPower<IllaoiAncientGodProphetPower>(),
		IllaoiHoverTips.FromPowerWithoutIcon<IllaoiGrowTipPower>(),
		IllaoiHoverTips.FromPowerWithoutIcon<IllaoiCommandTipPower>(),
		HoverTipFactory.FromPower<IllaoiFaithPower>()
	];

	public override string PortraitPath => ModInfo.AncientGodProphetPortraitPath;

	public AncientGodProphet()
		: base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		return OnPlayAsync();
	}

	private async Task OnPlayAsync()
	{
		await IllaoiMechanics.Grow(Owner, DynamicVars["Tentacles"].IntValue);
		await PowerCmd.Apply<IllaoiAncientGodProphetPower>(Owner.Creature, DynamicVars["Faith"].BaseValue, Owner.Creature, this);
	}

	protected override void OnUpgrade()
	{
		EnergyCost.UpgradeBy(-1);
	}
}

public sealed class SoulImpact : CardModel
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => IsUpgraded ? [CardKeyword.Retain] : [];

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<IllaoiSoulImpactPower>()];

	public override string PortraitPath => ModInfo.SoulImpactPortraitPath;

	public SoulImpact()
		: base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		return PowerCmd.Apply<IllaoiSoulImpactPower>(Owner.Creature, 1m, Owner.Creature, this);
	}

	protected override void OnUpgrade()
	{
		AddKeyword(CardKeyword.Retain);
	}
}

public sealed class TrialByMotion : CardModel
{
	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new PowerVar<StrengthPower>(1m),
		new PowerVar<DexterityPower>(1m),
		new DynamicVar("Tentacles", 1m)
	];

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		HoverTipFactory.FromPower<StrengthPower>(),
		HoverTipFactory.FromPower<DexterityPower>(),
		IllaoiHoverTips.FromPowerWithoutIcon<IllaoiGrowTipPower>()
	];

	public override string PortraitPath => ModInfo.TrialByMotionPortraitPath;

	public TrialByMotion()
		: base(1, CardType.Power, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await PowerCmd.Apply<StrengthPower>(Owner.Creature, DynamicVars.Strength.BaseValue, Owner.Creature, this);
		await PowerCmd.Apply<DexterityPower>(Owner.Creature, DynamicVars.Dexterity.BaseValue, Owner.Creature, this);
		await IllaoiMechanics.Grow(Owner, DynamicVars["Tentacles"].IntValue);
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Strength.UpgradeValueBy(1m);
		DynamicVars.Dexterity.UpgradeValueBy(1m);
	}
}

public sealed class SerpentDance : CardModel
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new EnergyVar(1),
		new CardsVar(2),
		new DynamicVar("Discard", 2m)
	];

	public override string PortraitPath => ModInfo.SerpentDancePortraitPath;

	public SerpentDance()
		: base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);
		await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner, fromHandDraw: false);
		await IllaoiMechanics.DiscardFromHand(choiceContext, Owner, this, DynamicVars["Discard"].IntValue);
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Energy.UpgradeValueBy(1m);
	}
}

public sealed class Undertow : CardModel
{
	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new DamageVar(3m, ValueProp.Move),
		new DynamicVar("Times", 4m)
	];

	public override string PortraitPath => ModInfo.UndertowPortraitPath;

	public Undertow()
		: base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ICombatState combatState = Owner.Creature.CombatState
			?? throw new InvalidOperationException("Undertow played outside combat.");
		for (int i = 0; i < DynamicVars["Times"].IntValue; i++)
		{
			await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).TargetingAllOpponents(combatState)
				.WithHitFx("vfx/vfx_heavy_blunt")
				.Execute(choiceContext);
		}
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Damage.UpgradeValueBy(1m);
	}
}

public sealed class VoiceOfTheDeep : CardModel
{
	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new DynamicVar("Commands", 2m),
		new DynamicVar("Faith", 1m)
	];

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		IllaoiHoverTips.FromPowerWithoutIcon<IllaoiCommandTipPower>(),
		HoverTipFactory.FromPower<IllaoiHuskPower>(),
		HoverTipFactory.FromPower<IllaoiFaithPower>()
	];

	public override string PortraitPath => ModInfo.VoiceOfTheDeepPortraitPath;

	public VoiceOfTheDeep()
		: base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
		bool hadHusk = IllaoiMechanics.HasHusk(cardPlay.Target);
		await IllaoiMechanics.CommandTentaclesTimes(choiceContext, Owner, cardPlay.Target, this, DynamicVars["Commands"].IntValue);
		if (hadHusk)
		{
			await PowerCmd.Apply<IllaoiFaithPower>(Owner.Creature, DynamicVars["Faith"].BaseValue, Owner.Creature, this);
		}
	}

	protected override void OnUpgrade()
	{
		DynamicVars["Faith"].UpgradeValueBy(1m);
	}
}

public sealed class TheSeaAnswers : CardModel
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => IsUpgraded ? [CardKeyword.Innate] : [];

	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new EnergyVar(1),
		new DynamicVar("Faith", 1m)
	];

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		HoverTipFactory.FromPower<IllaoiSeaAnswersPower>(),
		HoverTipFactory.FromPower<IllaoiFaithPower>()
	];

	public override string PortraitPath => ModInfo.TheSeaAnswersPortraitPath;

	public TheSeaAnswers()
		: base(1, CardType.Power, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await PowerCmd.Apply<IllaoiFaithPower>(Owner.Creature, DynamicVars["Faith"].BaseValue, Owner.Creature, this);
		await PowerCmd.Apply<IllaoiSeaAnswersPower>(Owner.Creature, DynamicVars.Energy.BaseValue, Owner.Creature, this);
	}

	protected override void OnUpgrade()
	{
		AddKeyword(CardKeyword.Innate);
	}
}

public sealed class DivineForm : CardModel
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => IsUpgraded ? [CardKeyword.Retain] : [];

	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new DynamicVar("Faith", 1m),
		new DynamicVar("Tentacles", 1m)
	];

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		HoverTipFactory.FromPower<IllaoiDivineFormPower>(),
		HoverTipFactory.FromPower<IllaoiFaithPower>(),
		IllaoiHoverTips.FromPowerWithoutIcon<IllaoiGrowTipPower>()
	];

	public override string PortraitPath => ModInfo.DivineFormPortraitPath;

	public DivineForm()
		: base(3, CardType.Power, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		return PowerCmd.Apply<IllaoiDivineFormPower>(Owner.Creature, 1m, Owner.Creature, this);
	}

	protected override void OnUpgrade()
	{
		AddKeyword(CardKeyword.Retain);
	}
}

public sealed class RagingTide : CardModel
{
	protected override bool HasEnergyCostX => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Commands", 2m)];

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [IllaoiHoverTips.FromPowerWithoutIcon<IllaoiCommandTipPower>()];

	public override string PortraitPath => ModInfo.RagingTidePortraitPath;

	public RagingTide()
		: base(0, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
		int x = Math.Max(0, (int)ResolveEnergyXValue());
		await IllaoiMechanics.CommandTentaclesTimes(choiceContext, Owner, cardPlay.Target, this, x + DynamicVars["Commands"].IntValue);
	}

	protected override void OnUpgrade()
	{
		DynamicVars["Commands"].UpgradeValueBy(1m);
	}
}

public sealed class RendSoul : CardModel
{
	protected override bool HasEnergyCostX => true;

	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new DynamicVar("Turns", ModInfo.SoulDurationTurns),
		new DynamicVar("BonusSouls", 0m)
	];

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<IllaoiSoulLinkPower>()];

	public override string PortraitPath => ModInfo.RendSoulPortraitPath;

	public RendSoul()
		: base(0, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
		int x = Math.Max(0, (int)ResolveEnergyXValue());
		int soulCount = x + DynamicVars["BonusSouls"].IntValue;
		for (int i = 0; i < soulCount && cardPlay.Target.IsAlive; i++)
		{
			await IllaoiMechanics.SummonSoul(choiceContext, Owner, cardPlay.Target, this);
		}
	}

	protected override void OnUpgrade()
	{
		DynamicVars["BonusSouls"].UpgradeValueBy(1m);
	}
}

public sealed class TrialOfTheAncientGod : CardModel
{
	protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(12m, ValueProp.Move)];

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		HoverTipFactory.FromPower<IllaoiSoulLinkPower>(),
		IllaoiHoverTips.FromPowerWithoutIcon<IllaoiCommandTipPower>()
	];

	public override string PortraitPath => ModInfo.TrialOfTheAncientGodPortraitPath;

	public TrialOfTheAncientGod()
		: base(1, CardType.Attack, CardRarity.Ancient, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
		bool targetWasSoul = IllaoiMechanics.IsSoul(cardPlay.Target);
		Creature body = IllaoiMechanics.ResolveBodyTarget(cardPlay.Target);
		await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_heavy_blunt")
			.Execute(choiceContext);

		if (cardPlay.Target.IsAlive)
		{
			await IllaoiMechanics.CommandTentacles(choiceContext, Owner, cardPlay.Target, this);
		}

		if (targetWasSoul && body.IsAlive)
		{
			await IllaoiMechanics.CommandTentacles(choiceContext, Owner, body, this);
		}
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Damage.UpgradeValueBy(6m);
	}
}

public sealed class NagakabourosDescends : CardModel
{
	protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Tentacles", 2m)];

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		IllaoiHoverTips.FromPowerWithoutIcon<IllaoiGrowTipPower>(),
		HoverTipFactory.FromPower<IllaoiSoulLinkPower>(),
		IllaoiHoverTips.FromPowerWithoutIcon<IllaoiCommandTipPower>(),
		HoverTipFactory.FromPower<IllaoiNagakabourosDescendsPower>()
	];

	public override string PortraitPath => ModInfo.NagakabourosDescendsPortraitPath;

	public NagakabourosDescends()
		: base(2, CardType.Power, CardRarity.Ancient, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await IllaoiMechanics.Grow(Owner, DynamicVars["Tentacles"].IntValue);
		await PowerCmd.Apply<IllaoiNagakabourosDescendsPower>(Owner.Creature, 1m, Owner.Creature, this);
	}

	protected override void OnUpgrade()
	{
		EnergyCost.UpgradeBy(-1);
	}
}

public class NagakabourosIdol : IllaoiRelicBase
{
	private int _tentacles;

	public override RelicRarity Rarity => RelicRarity.Starter;

	public override string PackedIconPath => ModInfo.IdolIconPath;

	protected override string PackedIconOutlinePath => PackedIconPath;

	protected override string BigIconPath => PackedIconPath;

	public override bool ShowCounter => true;

	public override bool SpawnsPets => true;

	public override int DisplayAmount => !IsCanonical ? _tentacles : 0;

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		IllaoiHoverTips.FromPowerWithoutIcon<IllaoiGrowTipPower>(),
		IllaoiHoverTips.FromPowerWithoutIcon<IllaoiTentacleTipPower>()
	];

	[SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
	public int Illaoi_Tentacles
	{
		get => _tentacles;
		set
		{
			_tentacles = Math.Max(0, value);
			InvokeDisplayAmountChanged();
		}
	}

	public int Tentacles
	{
		get => Illaoi_Tentacles;
		set => Illaoi_Tentacles = value;
	}

	public bool CommandedThisTurn { get; set; }

	public bool GainedBlockThisTurn { get; set; }

	public decimal NextCommandBonusDamage { get; set; }

	public override async Task BeforeCombatStart()
	{
		ResetCombatState();
		await IllaoiMechanics.Grow(Owner, 1);
	}

	public override Task AfterSideTurnStart(CombatSide side, ICombatState combatState)
	{
		if (side == CombatSide.Player)
		{
			CommandedThisTurn = false;
			GainedBlockThisTurn = false;
			NextCommandBonusDamage = 0m;
		}

		return Task.CompletedTask;
	}

	public override Task AfterBlockGained(Creature creature, decimal amount, ValueProp props, CardModel? cardSource)
	{
		if (creature == Owner.Creature && amount > 0m)
		{
			GainedBlockThisTurn = true;
		}

		return Task.CompletedTask;
	}

	public override Task BeforeTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
	{
		return side == CombatSide.Player
			? IllaoiMechanics.AttackHuskTargetsAtTurnEnd(choiceContext, Owner)
			: Task.CompletedTask;
	}

	public override Task AfterCombatEnd(CombatRoom room)
	{
		IllaoiCombatVisuals.CleanupTentacles(Owner);
		ResetCombatState();
		return Task.CompletedTask;
	}

	protected void ResetCombatState()
	{
		Tentacles = 0;
		CommandedThisTurn = false;
		GainedBlockThisTurn = false;
		NextCommandBonusDamage = 0m;
	}
}

public sealed class NagakabourosTouch : NagakabourosIdol
{
	public override RelicRarity Rarity => RelicRarity.Starter;

	public override string PackedIconPath => ModInfo.TouchIconPath;

	public override async Task BeforeCombatStart()
	{
		ResetCombatState();
		await IllaoiMechanics.Grow(Owner, 3);
	}
}

public sealed class IllaoiTentacleMonster : MonsterModel
{
	public override LocString Title => MonsterModel.L10NMonsterLookup("ILLAOI_TENTACLE_MONSTER.name");

	public override int MinInitialHp => 1;

	public override int MaxInitialHp => 1;

	public override bool CanChangeScale => true;

	public override bool HasDeathSfx => false;

	public override bool HasHurtSfx => false;

	public override bool IsHealthBarVisible => false;

	[SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
	public float Illaoi_VisualOffsetX { get; set; }

	[SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
	public float Illaoi_VisualOffsetY { get; set; }

	public float VisualOffsetX
	{
		get => Illaoi_VisualOffsetX;
		set => Illaoi_VisualOffsetX = value;
	}

	public float VisualOffsetY
	{
		get => Illaoi_VisualOffsetY;
		set => Illaoi_VisualOffsetY = value;
	}

	public Vector2 VisualOffset
	{
		get => new(VisualOffsetX, VisualOffsetY);
		set
		{
			VisualOffsetX = value.X;
			VisualOffsetY = value.Y;
		}
	}

	protected override string VisualsPath => SceneHelper.GetScenePath("creature_visuals/fallback");

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		MoveState state = new("NOTHING", _ => Task.CompletedTask, new HiddenIntent());
		state.FollowUpState = state;
		return new MonsterMoveStateMachine([state], state);
	}
}

public sealed class IllaoiSoulMonster : MonsterModel
{
	private string _visualSourceCategory = string.Empty;
	private string _visualSourceEntry = string.Empty;

	[SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
	public string Illaoi_VisualSourceCategory
	{
		get => _visualSourceCategory;
		set => _visualSourceCategory = value ?? string.Empty;
	}

	[SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
	public string Illaoi_VisualSourceEntry
	{
		get => _visualSourceEntry;
		set => _visualSourceEntry = value ?? string.Empty;
	}

	public void SetVisualSource(MonsterModel sourceModel)
	{
		ModelId id = sourceModel.Id;
		Illaoi_VisualSourceCategory = id.Category;
		Illaoi_VisualSourceEntry = id.Entry;
	}

	public MonsterModel? ResolveVisualSourceModel()
	{
		if (string.IsNullOrEmpty(Illaoi_VisualSourceCategory) || string.IsNullOrEmpty(Illaoi_VisualSourceEntry))
		{
			return null;
		}

		return ModelDb.GetByIdOrNull<MonsterModel>(new ModelId(Illaoi_VisualSourceCategory, Illaoi_VisualSourceEntry));
	}

	public override LocString Title => MonsterModel.L10NMonsterLookup("ILLAOI_SOUL_MONSTER.name");

	public override int MinInitialHp => 30;

	public override int MaxInitialHp => 30;

	public override bool HasDeathSfx => false;

	public override bool HasHurtSfx => false;

	protected override string VisualsPath => SceneHelper.GetScenePath("creature_visuals/fallback");

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		MoveState state = new("NOTHING", _ => Task.CompletedTask, new HiddenIntent());
		state.FollowUpState = state;
		return new MonsterMoveStateMachine([state], state);
	}
}
