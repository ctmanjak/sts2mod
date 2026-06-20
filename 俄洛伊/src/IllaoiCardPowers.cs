using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace Illaoi;

internal static class IllaoiHoverTips
{
	public static IHoverTip FromPowerWithoutIcon<TPower>() where TPower : PowerModel
	{
		PowerModel power = ModelDb.Power<TPower>();
		HoverTip tip = new(power.Title, power.Description.GetFormattedText())
		{
			Id = power.Id.ToString(),
			IsDebuff = power.Type == PowerType.Debuff,
			IsInstanced = power.InstanceType != PowerInstanceType.None,
			IsSmart = false
		};

		return tip;
	}
}

public abstract class IllaoiTemporaryStatPower<TStatPower> : IllaoiPowerBase, ITemporaryPower where TStatPower : PowerModel
{
	private bool _shouldIgnoreNextInstance;

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public abstract AbstractModel OriginModel { get; }

	public PowerModel InternallyAppliedPower => ModelDb.Power<TStatPower>();

	public void IgnoreNextInstance()
	{
		_shouldIgnoreNextInstance = true;
	}

	public override async Task BeforeApplied(Creature target, decimal amount, Creature? applier, CardModel? cardSource)
	{
		if (_shouldIgnoreNextInstance)
		{
			_shouldIgnoreNextInstance = false;
			return;
		}

		await PowerCmd.Apply<TStatPower>(target, amount, applier, cardSource, silent: true);
	}

	public override async Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
	{
		if (power != this || amount == Amount)
		{
			return;
		}

		if (_shouldIgnoreNextInstance)
		{
			_shouldIgnoreNextInstance = false;
			return;
		}

		await PowerCmd.Apply<TStatPower>(Owner, amount, applier, cardSource, silent: true);
	}

	public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
	{
		if (side != Owner.Side || Amount <= 0)
		{
			return;
		}

		Creature owner = Owner;
		decimal amount = Amount;
		Flash();
		await PowerCmd.Remove(this);
		await PowerCmd.Apply<TStatPower>(owner, -amount, owner, null);
	}
}

public sealed class IllaoiTemporaryStrengthPower : IllaoiTemporaryStatPower<StrengthPower>
{
	public override AbstractModel OriginModel => ModelDb.Card<SermonOfMotion>();

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<StrengthPower>()];
}

public sealed class IllaoiTemporaryDexterityPower : IllaoiTemporaryStatPower<DexterityPower>
{
	public override AbstractModel OriginModel => ModelDb.Card<TempleIdol>();

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<DexterityPower>()];
}

public sealed class IllaoiFaithPower : IllaoiPowerBase
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;
}

public sealed class IllaoiGrowTipPower : IllaoiPowerBase
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override bool IsVisibleInternal => false;
}

public sealed class IllaoiTentacleTipPower : IllaoiPowerBase
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override bool IsVisibleInternal => false;
}

public sealed class IllaoiCommandTipPower : IllaoiPowerBase
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override bool IsVisibleInternal => false;
}

public sealed class IllaoiDrainPower : IllaoiPowerBase
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public async Task TryGainBlock(Player player)
	{
		if (Amount <= 0 || Owner != player.Creature || Owner.IsDead)
		{
			return;
		}

		Flash();
		await CreatureCmd.GainBlock(player.Creature, Amount, ValueProp.Unpowered, null);
	}
}

public sealed class IllaoiAncientGodProphetPower : IllaoiPowerBase
{
	[SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
	public bool Illaoi_TriggeredThisTurn { get; set; }

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override Task AfterSideTurnStart(CombatSide side, ICombatState combatState)
	{
		if (side == Owner.Side)
		{
			Illaoi_TriggeredThisTurn = false;
		}

		return Task.CompletedTask;
	}

	public async Task AfterCommand(Player player)
	{
		if (Illaoi_TriggeredThisTurn || Amount <= 0 || Owner != player.Creature || Owner.IsDead)
		{
			return;
		}

		Illaoi_TriggeredThisTurn = true;
		Flash();
		await PowerCmd.Apply<IllaoiFaithPower>(Owner, Amount, Owner, null);
	}
}

public sealed class IllaoiSoulImpactPower : IllaoiPowerBase
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public async Task AfterSoulShattered(Creature body)
	{
		if (!body.IsAlive || Owner.IsDead)
		{
			return;
		}

		Flash();
		if (!body.IsStunned)
		{
			await CreatureCmd.Stun(body);
			return;
		}

		await PowerCmd.Apply<WeakPower>(body, 2m, Owner, null);
		await PowerCmd.Apply<VulnerablePower>(body, 2m, Owner, null);
	}
}

public sealed class IllaoiNagakabourosDescendsPower : IllaoiPowerBase
{
	[SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
	public bool Illaoi_TriggeredThisTurn { get; set; }

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override Task AfterSideTurnStart(CombatSide side, ICombatState combatState)
	{
		if (side == Owner.Side)
		{
			Illaoi_TriggeredThisTurn = false;
		}

		return Task.CompletedTask;
	}

	public async Task TryTrigger(PlayerChoiceContext choiceContext, Player player, Creature target, CardModel? cardSource)
	{
		if (Illaoi_TriggeredThisTurn
			|| Amount <= 0
			|| Owner != player.Creature
			|| Owner.IsDead
			|| target.IsDead)
		{
			return;
		}

		Illaoi_TriggeredThisTurn = true;
		Flash();
		await IllaoiMechanics.CommandTentacles(choiceContext, player, target, cardSource);
	}
}

public sealed class IllaoiTidecallerPower : IllaoiPowerBase
{
	[SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
	public bool Illaoi_TriggeredThisTurn { get; set; }

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override Task AfterSideTurnStart(CombatSide side, ICombatState combatState)
	{
		if (side == Owner.Side)
		{
			Illaoi_TriggeredThisTurn = false;
		}

		return Task.CompletedTask;
	}

	public async Task AfterCommand(PlayerChoiceContext choiceContext, Player player, bool targetHadHusk)
	{
		if (Illaoi_TriggeredThisTurn || Amount <= 0 || Owner != player.Creature || Owner.IsDead)
		{
			return;
		}

		Illaoi_TriggeredThisTurn = true;
		Flash();
		await CardPileCmd.Draw(choiceContext, Amount, player, fromHandDraw: false);
	}
}

public sealed class IllaoiRelentlessFaithPower : IllaoiPowerBase
{
	[SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
	public int Illaoi_TriggersUsedThisTurn { get; set; }

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override Task AfterSideTurnStart(CombatSide side, ICombatState combatState)
	{
		if (side == Owner.Side)
		{
			Illaoi_TriggersUsedThisTurn = 0;
		}

		return Task.CompletedTask;
	}

	public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
	{
		Player? player = Owner.Player;
		Creature? target = cardPlay.Target;
		if ((decimal)Illaoi_TriggersUsedThisTurn >= Amount
			|| Amount <= 0
			|| Owner.IsDead
			|| player == null
			|| target == null
			|| target.IsDead
			|| cardPlay.Card.Owner != player
			|| cardPlay.Card.Type != CardType.Attack)
		{
			return;
		}

		Illaoi_TriggersUsedThisTurn++;
		Flash();
		await IllaoiMechanics.CommandTentacles(context, player, target, cardPlay.Card);
	}
}

public sealed class IllaoiGrowthBlockPower : IllaoiPowerBase
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public async Task AfterGrow(int growAmount)
	{
		if (Owner.IsDead || Amount <= 0 || growAmount <= 0)
		{
			return;
		}

		Flash();
		await PowerCmd.Apply<IllaoiFaithPower>(Owner, Amount * growAmount, Owner, null);
	}
}

public sealed class IllaoiRhythmOfMotionPower : IllaoiPowerBase
{
	[SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
	public bool Illaoi_TriggeredThisTurn { get; set; }

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override Task AfterSideTurnStart(CombatSide side, ICombatState combatState)
	{
		if (side == Owner.Side)
		{
			Illaoi_TriggeredThisTurn = false;
		}

		return Task.CompletedTask;
	}

	public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
	{
		if (Illaoi_TriggeredThisTurn
			|| Amount <= 0
			|| Owner.IsDead
			|| cardPlay.Card.Owner?.Creature != Owner
			|| cardPlay.Card.Type != CardType.Skill)
		{
			return;
		}

		Illaoi_TriggeredThisTurn = true;
		Flash();
		await IllaoiMechanics.ApplyTemporaryDexterity(Owner, Amount, Owner, cardPlay.Card);
	}
}

public sealed class IllaoiFervorOfMotionPower : IllaoiPowerBase
{
	[SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
	public bool Illaoi_TriggeredThisTurn { get; set; }

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override Task AfterSideTurnStart(CombatSide side, ICombatState combatState)
	{
		if (side == Owner.Side)
		{
			Illaoi_TriggeredThisTurn = false;
		}

		return Task.CompletedTask;
	}

	public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
	{
		if (Illaoi_TriggeredThisTurn
			|| Amount <= 0
			|| Owner.IsDead
			|| cardPlay.Card.Owner?.Creature != Owner
			|| cardPlay.Card.Type != CardType.Attack)
		{
			return;
		}

		Illaoi_TriggeredThisTurn = true;
		Flash();
		await IllaoiMechanics.ApplyTemporaryStrength(Owner, Amount, Owner, cardPlay.Card);
	}
}

public sealed class IllaoiWatchfulIdolPower : IllaoiPowerBase
{
	[SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
	public bool Illaoi_TriggeredThisTurn { get; set; }

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override Task AfterSideTurnStart(CombatSide side, ICombatState combatState)
	{
		if (side == Owner.Side)
		{
			Illaoi_TriggeredThisTurn = false;
		}

		return Task.CompletedTask;
	}

	public override async Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
	{
		if (Illaoi_TriggeredThisTurn
			|| Amount <= 0
			|| Owner.IsDead
			|| amount <= 0
			|| power.Owner != Owner
			|| power.Id != ModelDb.GetId<IllaoiTemporaryDexterityPower>())
		{
			return;
		}

		Illaoi_TriggeredThisTurn = true;
		Flash();
		await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Unpowered, null);
	}
}

public sealed class IllaoiSeaAnswersPower : IllaoiPowerBase
{
	[SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
	public int Illaoi_CardsPlayedThisTurn { get; set; }

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public async Task AfterSoulShattered()
	{
		Player? player = Owner.Player;
		if (Amount <= 0
			|| Owner.IsDead
			|| player == null)
		{
			return;
		}

		Flash();
		await PlayerCmd.GainEnergy(Amount, player);
	}
}

public sealed class IllaoiNextTurnDrawPower : IllaoiPowerBase
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, ICombatState combatState)
	{
		Player? player = Owner.Player;
		if (side != Owner.Side || Owner.IsDead || Amount <= 0 || player == null)
		{
			return;
		}

		Flash();
		await CardPileCmd.Draw(choiceContext, Amount, player, fromHandDraw: false);
		await PowerCmd.Remove(this);
	}
}

public sealed class IllaoiDivineFormPower : IllaoiPowerBase
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override async Task AfterSideTurnStart(CombatSide side, ICombatState combatState)
	{
		Player? player = Owner.Player;
		if (side != Owner.Side || Owner.IsDead || Amount <= 0 || player == null)
		{
			return;
		}

		Flash();
		await PowerCmd.Apply<IllaoiFaithPower>(Owner, Amount, Owner, null);
		await IllaoiMechanics.Grow(player, (int)Amount);
	}
}

public sealed class IllaoiNextTurnFaithPower : IllaoiPowerBase
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, ICombatState combatState)
	{
		if (side != Owner.Side || Owner.IsDead || Amount <= 0)
		{
			return;
		}

		Flash();
		await PowerCmd.Apply<IllaoiFaithPower>(Owner, Amount, Owner, null);
		await PowerCmd.Remove(this);
	}
}
