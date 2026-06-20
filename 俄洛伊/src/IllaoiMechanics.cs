using Godot;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.ValueProps;

namespace Illaoi;

internal static class IllaoiMechanics
{
	private const float TentacleMinSpacing = 92f;
	private const int TentacleOffsetRollAttempts = 24;

	private static bool _transferringSoulDamage;
	private static bool _attackingHuskTargetsAtTurnEnd;

	public static async Task Grow(Player player, int amount)
	{
		NagakabourosIdol? idol = GetNagakabourosRelic(player);
		if (idol == null || amount <= 0)
		{
			return;
		}

		ICombatState? combatState = player.Creature.CombatState;
		List<Vector2> occupiedTentacleOffsets = GetLivingTentacles(player)
			.Select(static creature => ((IllaoiTentacleMonster)creature.Monster!).VisualOffset)
			.ToList();
		bool addedTentacle = false;
		for (int i = 0; i < amount; i++)
		{
			if (combatState != null && player.Creature.IsAlive)
			{
				IllaoiTentacleMonster model = (IllaoiTentacleMonster)ModelDb.Monster<IllaoiTentacleMonster>().ToMutable();
				model.VisualOffset = RollTentacleOffset(idol.Tentacles + i, occupiedTentacleOffsets);
				occupiedTentacleOffsets.Add(model.VisualOffset);
				Creature tentacle = combatState.CreateCreature(model, player.Creature.Side, slot: null);
				await PlayerCmd.AddPet(tentacle, player);
				await CreatureCmd.SetMaxAndCurrentHp(tentacle, 1m);
				addedTentacle = true;
			}
		}

		if (addedTentacle)
		{
			IllaoiCombatVisuals.PositionTentacles(player);
			await TriggerAfterGrow(player, amount);
		}

		idol.Tentacles += amount;
		idol.Flash();
		Log.Info($"{ModInfo.LogPrefix} Grow {amount}; tentacles={idol.Tentacles}.");
	}

	public static int GetTentacleCount(Player player)
	{
		IReadOnlyList<Creature> tentacles = GetLivingTentacles(player);
		return tentacles.Count > 0 ? tentacles.Count : GetNagakabourosRelic(player)?.Tentacles ?? 0;
	}

	public static async Task CommandTentacles(PlayerChoiceContext choiceContext, Player player, Creature? target, CardModel? cardSource)
	{
		if (target == null || target.IsDead)
		{
			return;
		}

		bool targetHadHusk = HasHusk(target);
		decimal commandBonusDamage = ConsumeNextCommandBonusDamage(player);
		if (await AttackWithTentacles(choiceContext, player, target, cardSource, commandBonusDamage))
		{
			RecordCommand(player);
			await TriggerAfterCommand(choiceContext, player, targetHadHusk);
		}
	}

	public static async Task CommandTentaclesTimes(PlayerChoiceContext choiceContext, Player player, Creature? target, CardModel? cardSource, int times)
	{
		if (target == null || target.IsDead || times <= 0)
		{
			return;
		}

		for (int i = 0; i < times && target.IsAlive; i++)
		{
			await CommandTentacles(choiceContext, player, target, cardSource);
		}
	}

	public static Task CommandTentaclesAtRandomEnemy(PlayerChoiceContext choiceContext, Player player, CardModel? cardSource)
	{
		ICombatState? combatState = player.Creature.CombatState;
		if (combatState == null || player.Creature.IsDead)
		{
			return Task.CompletedTask;
		}

		List<Creature> enemies = combatState.Enemies
			.Where(static creature => creature.IsAlive)
			.ToList();
		if (enemies.Count == 0)
		{
			return Task.CompletedTask;
		}

		Creature target = enemies[combatState.RunState.Rng.CombatTargets.NextInt(enemies.Count)];
		return CommandTentacles(choiceContext, player, target, cardSource);
	}

	public static async Task CommandAllHuskTargets(PlayerChoiceContext choiceContext, Player player, CardModel? cardSource)
	{
		ICombatState? combatState = player.Creature.CombatState;
		if (combatState == null || player.Creature.IsDead)
		{
			return;
		}

		List<Creature> huskTargets = combatState.Enemies
			.Where(static creature => creature.IsAlive && creature.GetPower<IllaoiHuskPower>()?.Amount > 0)
			.ToList();
		bool commandedAny = false;
		decimal commandBonusDamage = huskTargets.Count > 0 ? ConsumeNextCommandBonusDamage(player) : 0m;
		foreach (Creature target in huskTargets)
		{
			if (target.IsAlive && await AttackWithTentacles(choiceContext, player, target, cardSource, commandBonusDamage))
			{
				commandedAny = true;
			}
		}

		if (commandedAny)
		{
			RecordCommand(player);
			await TriggerAfterCommand(choiceContext, player, targetHadHusk: true);
		}
	}

	public static Task ApplyHusk(Creature target, decimal amount, Creature? applier, CardModel? cardSource)
	{
		return target.IsAlive && amount > 0
			? PowerCmd.Apply<IllaoiHuskPower>(target, amount, applier, cardSource)
			: Task.CompletedTask;
	}

	public static Task ApplyTemporaryStrength(Creature target, decimal amount, Creature? applier, CardModel? cardSource)
	{
		return target.IsAlive && amount > 0
			? PowerCmd.Apply<IllaoiTemporaryStrengthPower>(target, amount, applier, cardSource)
			: Task.CompletedTask;
	}

	public static Task ApplyTemporaryDexterity(Creature target, decimal amount, Creature? applier, CardModel? cardSource)
	{
		return target.IsAlive && amount > 0
			? PowerCmd.Apply<IllaoiTemporaryDexterityPower>(target, amount, applier, cardSource)
			: Task.CompletedTask;
	}

	public static bool HasHusk(Creature? target)
	{
		return target?.GetPower<IllaoiHuskPower>()?.Amount > 0;
	}

	public static bool IsSoul(Creature? target)
	{
		return target?.GetPower<IllaoiSoulLinkPower>() != null;
	}

	public static bool HasCommandedThisTurn(Player player)
	{
		return GetNagakabourosRelic(player)?.CommandedThisTurn == true;
	}

	public static bool HasSoulForTarget(Creature? target)
	{
		if (target == null)
		{
			return false;
		}

		Creature body = ResolveBodyTarget(target);
		return body.CombatState?.Enemies.Any(creature =>
			creature.IsAlive && creature.GetPower<IllaoiSoulLinkPower>()?.Target == body) == true;
	}

	public static bool HasAnySoul(Player player)
	{
		return player.Creature.CombatState?.Enemies.Any(IsSoul) == true;
	}

	public static Task ApplyHuskToBodyIfTargetIsSoul(Creature target, decimal amount, Creature? applier, CardModel? cardSource)
	{
		Creature body = ResolveBodyTarget(target);
		return body != target
			? ApplyHusk(body, amount, applier, cardSource)
			: Task.CompletedTask;
	}

	public static Task ApplyVulnerableToBody(Creature target, decimal amount, Creature? applier, CardModel? cardSource)
	{
		Creature body = ResolveBodyTarget(target);
		return body.IsAlive && amount > 0
			? PowerCmd.Apply<VulnerablePower>(body, amount, applier, cardSource)
			: Task.CompletedTask;
	}

	public static Task ApplyVulnerableToBodyIfTargetIsSoul(Creature target, decimal amount, Creature? applier, CardModel? cardSource)
	{
		Creature body = ResolveBodyTarget(target);
		return body != target
			? ApplyVulnerableToBody(body, amount, applier, cardSource)
			: Task.CompletedTask;
	}

	public static bool HasTemporaryStrength(Creature target)
	{
		return target.GetPower<IllaoiTemporaryStrengthPower>()?.Amount > 0;
	}

	public static bool HasTemporaryDexterity(Creature target)
	{
		return target.GetPower<IllaoiTemporaryDexterityPower>()?.Amount > 0;
	}

	public static bool HasStrength(Creature target)
	{
		return target.GetPower<StrengthPower>()?.Amount > 0;
	}

	public static bool HasGainedBlockThisTurn(Player player)
	{
		return GetNagakabourosRelic(player)?.GainedBlockThisTurn == true;
	}

	public static void AddNextCommandBonusDamage(Player player, decimal amount)
	{
		NagakabourosIdol? idol = GetNagakabourosRelic(player);
		if (idol != null && amount > 0)
		{
			idol.NextCommandBonusDamage += amount;
			idol.Flash();
		}
	}

	public static async Task<decimal> ConsumeHusk(Creature target)
	{
		IllaoiHuskPower? husk = target.GetPower<IllaoiHuskPower>();
		if (husk == null || husk.Amount <= 0)
		{
			return 0m;
		}

		decimal amount = husk.Amount;
		await PowerCmd.Remove(husk);
		return amount;
	}

	public static Task GainBlockPerTentacle(Player player, decimal blockPerTentacle, CardPlay cardPlay)
	{
		int count = GetTentacleCount(player);
		return count > 0
			? CreatureCmd.GainBlock(player.Creature, blockPerTentacle * count, ValueProp.Move, cardPlay)
			: Task.CompletedTask;
	}

	public static async Task GainBlockTimes(Creature target, decimal block, int times, CardPlay cardPlay)
	{
		for (int i = 0; i < times && target.IsAlive; i++)
		{
			await CreatureCmd.GainBlock(target, block, ValueProp.Move, cardPlay);
		}
	}

	public static async Task AttackTargetTimes(PlayerChoiceContext choiceContext, CardModel cardSource, Creature target, decimal damage, int times, string hitFx)
	{
		for (int i = 0; i < times && target.IsAlive; i++)
		{
			await DamageCmd.Attack(damage).FromCard(cardSource).Targeting(target)
				.WithHitFx(hitFx)
				.Execute(choiceContext);
		}
	}

	public static async Task<IReadOnlyList<CardModel>> DiscardFromHand(PlayerChoiceContext choiceContext, Player player, CardModel source, int amount)
	{
		if (amount <= 0 || player.Creature.IsDead || player.PlayerCombatState == null)
		{
			return [];
		}

		List<CardModel> selectedCards = (await CardSelectCmd.FromHandForDiscard(
			choiceContext,
			player,
			new CardSelectorPrefs(CardSelectorPrefs.DiscardSelectionPrompt, amount),
			filter: null,
			source)).ToList();
		if (selectedCards.Count > 0)
		{
			await CardCmd.Discard(choiceContext, selectedCards);
		}

		return selectedCards;
	}

	public static Task TriggerNagakabourosDescends(PlayerChoiceContext choiceContext, Player player, Creature target, CardModel? cardSource)
	{
		IllaoiNagakabourosDescendsPower? power = player.Creature.GetPower<IllaoiNagakabourosDescendsPower>();
		return power is { Amount: > 0 } && target.IsAlive
			? power.TryTrigger(choiceContext, player, target, cardSource)
			: Task.CompletedTask;
	}

	public static async Task AttackHuskTargetsAtTurnEnd(PlayerChoiceContext choiceContext, Player player)
	{
		ICombatState? combatState = player.Creature.CombatState;
		if (combatState == null || player.Creature.IsDead)
		{
			return;
		}

		_attackingHuskTargetsAtTurnEnd = true;
		try
		{
			List<Creature> huskTargets = combatState.Enemies
				.Where(static creature => creature.IsAlive && creature.GetPower<IllaoiHuskPower>()?.Amount > 0)
				.ToList();
			foreach (Creature target in huskTargets)
			{
				if (target.IsAlive && target.GetPower<IllaoiHuskPower>()?.Amount > 0)
				{
					await AttackWithTentacles(choiceContext, player, target, cardSource: null, bonusDamage: 0m);
				}
			}
		}
		finally
		{
			_attackingHuskTargetsAtTurnEnd = false;
		}
	}

	public static async Task<Creature?> SummonSoul(PlayerChoiceContext choiceContext, Player player, Creature target, CardModel cardSource)
	{
		Creature body = ResolveBodyTarget(target);
		ICombatState? combatState = player.Creature.CombatState;
		if (combatState == null || body.IsDead)
		{
			return null;
		}

		IllaoiSoulMonster soulModel = (IllaoiSoulMonster)ModelDb.Monster<IllaoiSoulMonster>().ToMutable();
		if (body.Monster != null)
		{
			soulModel.SetVisualSource(body.Monster);
		}
		Creature soul = await CreatureCmd.Add(soulModel, combatState, CombatSide.Enemy, slotName: null);
		int soulMaxHp = GetSoulHp(body.MaxHp);
		int soulCurrentHp = Math.Clamp(GetSoulHp(body.CurrentHp), 1, soulMaxHp);
		await CreatureCmd.SetMaxHp(soul, soulMaxHp);
		await CreatureCmd.SetCurrentHp(soul, soulCurrentHp);

		await PowerCmd.Apply<IllaoiHuskPower>(soul, ModInfo.SoulHuskDurationTurns, player.Creature, cardSource);
		IllaoiSoulLinkPower? link = await PowerCmd.Apply<IllaoiSoulLinkPower>(soul, ModInfo.SoulDurationTurns, player.Creature, cardSource);
		if (link != null)
		{
			link.SetBodyTarget(body);
		}

		IllaoiCombatVisuals.PositionSoulNearBody(soul, body);
		await TriggerNagakabourosDescends(choiceContext, player, soul, cardSource);
		Log.Info($"{ModInfo.LogPrefix} Summoned soul for {body.LogName} hp={soul.CurrentHp}/{soul.MaxHp} soulTurns={ModInfo.SoulDurationTurns} soulHuskTurns={ModInfo.SoulHuskDurationTurns}.");
		return soul;
	}

	public static Task ExtendSoulDuration(Creature? soul, decimal turns)
	{
		if (soul == null || soul.IsDead || turns <= 0)
		{
			return Task.CompletedTask;
		}

		IllaoiSoulLinkPower? link = soul.GetPower<IllaoiSoulLinkPower>();
		return link != null
			? PowerCmd.Apply<IllaoiSoulLinkPower>(soul, turns, link.Applier, null)
			: Task.CompletedTask;
	}

	public static async Task ApplyHuskFromShatteredSoul(PlayerChoiceContext choiceContext, Creature body, Creature? applier)
	{
		IllaoiHuskPower? husk = await PowerCmd.Apply<IllaoiHuskPower>(body, ModInfo.ShatteredSoulHuskDurationTurns, applier, null);
		if (husk != null && _attackingHuskTargetsAtTurnEnd)
		{
			husk.SkipNextPlayerTurnEndTick = true;
		}

		await TriggerSoulShatteredPowers(body, applier);
	}

	public static async Task TransferSoulDamage(PlayerChoiceContext choiceContext, IllaoiSoulLinkPower link, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (_transferringSoulDamage || result.Receiver != link.Owner || result.UnblockedDamage <= 0)
		{
			return;
		}

		Creature? body = link.Target;
		if (body == null || body.IsDead)
		{
			return;
		}

		int transferDamage = Math.Max(1, (int)Math.Ceiling(result.UnblockedDamage * ModInfo.SoulTransferRatio));
		IllaoiCombatVisuals.FlashSoulLink(link.Owner);
		_transferringSoulDamage = true;
		try
		{
			await CreatureCmd.Damage(
				choiceContext,
				body,
				transferDamage,
				ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.SkipHurtAnim,
				dealer,
				cardSource);
		}
		finally
		{
			_transferringSoulDamage = false;
		}
	}

	public static async Task RemoveExistingSoul(PlayerChoiceContext choiceContext, Player player)
	{
		ICombatState? combatState = player.Creature.CombatState;
		if (combatState == null)
		{
			return;
		}

		foreach (Creature soul in combatState.Enemies.Where(IsSoulOwnedBy(player)).ToList())
		{
			if (soul.IsAlive)
			{
				soul.GetPower<IllaoiSoulLinkPower>()?.SuppressShatterReward();
				IllaoiCombatVisuals.RemoveSoulLink(soul);
				await CreatureCmd.Kill(soul, force: true);
			}
		}
	}

	private static int GetSoulHp(decimal bodyHp)
	{
		return Math.Max(1, (int)Math.Ceiling(bodyHp * ModInfo.SoulHealthRatio));
	}

	private static async Task<bool> AttackWithTentacles(PlayerChoiceContext choiceContext, Player player, Creature target, CardModel? cardSource, decimal bonusDamage)
	{
		IReadOnlyList<Creature> tentacles = GetLivingTentacles(player);
		int tentacleCount = tentacles.Count > 0 ? tentacles.Count : GetTentacleCount(player);
		if (tentacleCount <= 0 || target.IsDead || player.Creature.IsDead)
		{
			return false;
		}

		NagakabourosIdol? idol = GetNagakabourosRelic(player);
		idol?.Flash();
		bool attacked = false;
		for (int i = 0; i < tentacleCount; i++)
		{
			if (target.IsDead)
			{
				break;
			}

			Creature visualDealer = i < tentacles.Count ? tentacles[i] : player.Creature;
			bool targetHadHusk = target.GetPower<IllaoiHuskPower>()?.Amount > 0;
			IllaoiCombatVisuals.AnimateTentacleAttack(visualDealer, target);
			await CreatureCmd.Damage(
				choiceContext,
				target,
				GetTentacleDamage(player) + bonusDamage,
				ValueProp.Unpowered | ValueProp.SkipHurtAnim,
				player.Creature,
				cardSource);
			attacked = true;
			await TriggerDrain(player, targetHadHusk);
		}

		return attacked;
	}

	private static decimal GetTentacleDamage(Player player)
	{
		decimal faith = player.Creature.GetPower<IllaoiFaithPower>()?.Amount ?? 0m;
		return Math.Max(0m, ModInfo.TentacleDamage + faith);
	}

	private static Task TriggerDrain(Player player, bool targetHadHusk)
	{
		if (!targetHadHusk || player.Creature.IsDead)
		{
			return Task.CompletedTask;
		}

		IllaoiDrainPower? drain = player.Creature.GetPower<IllaoiDrainPower>();
		return drain is { Amount: > 0 }
			? drain.TryGainBlock(player)
			: Task.CompletedTask;
	}

	private static async Task TriggerSoulShatteredPowers(Creature body, Creature? applier)
	{
		IllaoiSoulImpactPower? impact = applier?.GetPower<IllaoiSoulImpactPower>();
		if (body.IsAlive && impact is { Amount: > 0 })
		{
			await impact.AfterSoulShattered(body);
		}

		IllaoiSeaAnswersPower? seaAnswers = applier?.GetPower<IllaoiSeaAnswersPower>();
		if (seaAnswers is { Amount: > 0 })
		{
			await seaAnswers.AfterSoulShattered();
		}
	}

	private static Task TriggerAfterGrow(Player player, int amount)
	{
		IllaoiGrowthBlockPower? power = player.Creature.GetPower<IllaoiGrowthBlockPower>();
		return power != null && amount > 0
			? power.AfterGrow(amount)
			: Task.CompletedTask;
	}

	private static async Task TriggerAfterCommand(PlayerChoiceContext choiceContext, Player player, bool targetHadHusk)
	{
		IllaoiTidecallerPower? tidecaller = player.Creature.GetPower<IllaoiTidecallerPower>();
		if (tidecaller != null)
		{
			await tidecaller.AfterCommand(choiceContext, player, targetHadHusk);
		}

		IllaoiAncientGodProphetPower? prophet = player.Creature.GetPower<IllaoiAncientGodProphetPower>();
		if (prophet != null)
		{
			await prophet.AfterCommand(player);
		}
	}

	private static void RecordCommand(Player player)
	{
		NagakabourosIdol? idol = GetNagakabourosRelic(player);
		if (idol != null)
		{
			idol.CommandedThisTurn = true;
		}
	}

	private static decimal ConsumeNextCommandBonusDamage(Player player)
	{
		NagakabourosIdol? idol = GetNagakabourosRelic(player);
		if (idol == null || idol.NextCommandBonusDamage <= 0)
		{
			return 0m;
		}

		decimal bonusDamage = idol.NextCommandBonusDamage;
		idol.NextCommandBonusDamage = 0m;
		return bonusDamage;
	}

	private static NagakabourosIdol? GetNagakabourosRelic(Player player)
	{
		return player.GetRelic<NagakabourosTouch>() ?? player.GetRelic<NagakabourosIdol>();
	}

	private static IReadOnlyList<Creature> GetLivingTentacles(Player player)
	{
		return player.Creature.Pets
			.Where(creature => creature.Monster is IllaoiTentacleMonster && creature.IsAlive)
			.ToList();
	}

	private static Vector2 RollTentacleOffset(int index, IReadOnlyList<Vector2> occupiedOffsets)
	{
		Rng rng = Rng.Chaotic;
		Vector2 bestOffset = Vector2.Zero;
		float bestDistanceSquared = -1f;

		for (int attempt = 0; attempt < TentacleOffsetRollAttempts; attempt++)
		{
			Vector2 offset = RollTentacleOffsetCandidate(index, rng);
			float distanceSquared = GetNearestDistanceSquared(offset, occupiedOffsets);
			if (distanceSquared >= TentacleMinSpacing * TentacleMinSpacing)
			{
				return offset;
			}

			if (distanceSquared > bestDistanceSquared)
			{
				bestDistanceSquared = distanceSquared;
				bestOffset = offset;
			}
		}

		return bestOffset;
	}

	private static Vector2 RollTentacleOffsetCandidate(int index, Rng rng)
	{
		float x = rng.NextFloat(120f, 330f);
		float y = rng.NextFloat(-115f, 105f);
		if (index % 3 == 2)
		{
			x = rng.NextFloat(85f, 245f);
			y = rng.NextFloat(-135f, 50f);
		}

		return new Vector2(x, y);
	}

	private static float GetNearestDistanceSquared(Vector2 offset, IReadOnlyList<Vector2> occupiedOffsets)
	{
		if (occupiedOffsets.Count == 0)
		{
			return float.PositiveInfinity;
		}

		float nearest = float.PositiveInfinity;
		foreach (Vector2 occupiedOffset in occupiedOffsets)
		{
			nearest = Math.Min(nearest, offset.DistanceSquaredTo(occupiedOffset));
		}

		return nearest;
	}

	public static Creature ResolveBodyTarget(Creature target)
	{
		IllaoiSoulLinkPower? link = target.GetPower<IllaoiSoulLinkPower>();
		return link?.Target ?? target;
	}

	private static Func<Creature, bool> IsSoulOwnedBy(Player player)
	{
		return creature =>
		{
			IllaoiSoulLinkPower? link = creature.GetPower<IllaoiSoulLinkPower>();
			return link != null && link.Applier == player.Creature;
		};
	}
}
