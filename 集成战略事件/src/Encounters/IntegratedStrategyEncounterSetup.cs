using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace IntegratedStrategyEvents.Encounters;

internal static class IntegratedStrategyEncounterSetup
{
	private const float TwoSidedRightEnemyLeftOffset = 45f;

	public static bool TryGetCombatState<TEncounter>(out CombatState combatState)
		where TEncounter : class
	{
		CombatState? current = CombatManager.Instance.DebugOnlyGetState();
		if (current?.Encounter?.CanonicalInstance is not TEncounter)
		{
			combatState = null!;
			return false;
		}

		combatState = current;
		return true;
	}

	public static Creature? FindEnemyBySlot(CombatState combatState, string slotName)
	{
		return combatState.Enemies.FirstOrDefault(enemy => enemy.SlotName == slotName);
	}

	public static bool TryFindEnemyBySlot(CombatState combatState, string slotName, out Creature enemy)
	{
		Creature? found = FindEnemyBySlot(combatState, slotName);
		if (found == null)
		{
			enemy = null!;
			return false;
		}

		enemy = found;
		return true;
	}

	public static bool TryFindEnemyPairBySlots(
		CombatState combatState,
		string leftSlotName,
		string rightSlotName,
		out Creature leftEnemy,
		out Creature rightEnemy)
	{
		Creature? left = FindEnemyBySlot(combatState, leftSlotName);
		Creature? right = FindEnemyBySlot(combatState, rightSlotName);
		if (left == null || right == null)
		{
			leftEnemy = null!;
			rightEnemy = null!;
			return false;
		}

		leftEnemy = left;
		rightEnemy = right;
		return true;
	}

	public static Creature[] FindEnemies<TMonster>(CombatState combatState, int count)
		where TMonster : MonsterModel
	{
		return combatState.Enemies
			.Where(static enemy => enemy.Monster?.CanonicalInstance is TMonster)
			.Take(count)
			.ToArray();
	}

	public static bool TryFindEnemies<TMonster>(CombatState combatState, int count, out Creature[] enemies)
		where TMonster : MonsterModel
	{
		enemies = FindEnemies<TMonster>(combatState, count);
		return enemies.Length >= count;
	}

	public static async Task ApplyTwoSidedBackAttackPowers(CombatState combatState, Creature leftEnemy, Creature rightEnemy)
	{
		AlignTwoSidedEnemyPositions(leftEnemy, rightEnemy);
		await PowerCmd.Apply<BackAttackLeftPower>(leftEnemy, 1m, leftEnemy, null);
		FaceCreatureBodyTowardCenter(leftEnemy);
		await PowerCmd.Apply<SurroundedPower>(combatState.GetOpponentsOf(rightEnemy), 1m, rightEnemy, null);
		await PowerCmd.Apply<BackAttackRightPower>(rightEnemy, 1m, rightEnemy, null);
	}

	public static void FaceCreatureBodyRight(Creature creature)
	{
		SetCreatureBodyScaleX(creature, positive: true);
	}

	public static void ForceMoveStart<TMonster>(Creature? creature, string moveId)
		where TMonster : MonsterModel
	{
		if (creature?.Monster?.CanonicalInstance is not TMonster)
		{
			return;
		}

		if (creature.Monster.MoveStateMachine?.States.TryGetValue(moveId, out MonsterState? state) != true)
		{
			return;
		}

		if (state is MoveState move)
		{
			creature.Monster.SetMoveImmediate(move, forceTransition: true);
		}
	}

	private static void FaceCreatureBodyTowardCenter(Creature creature)
	{
		SetCreatureBodyScaleX(creature, positive: false);
	}

	private static void AlignTwoSidedEnemyPositions(Creature leftEnemy, Creature rightEnemy)
	{
		NCombatRoom? room = NCombatRoom.Instance;
		NCreature? leftNode = room?.GetCreatureNode(leftEnemy);
		NCreature? rightNode = room?.GetCreatureNode(rightEnemy);
		if (leftNode == null || rightNode == null)
		{
			return;
		}

		Vector2 rightPosition = rightNode.Position;
		rightNode.Position = new Vector2(rightPosition.X - TwoSidedRightEnemyLeftOffset, leftNode.Position.Y);
	}

	private static void SetCreatureBodyScaleX(Creature creature, bool positive)
	{
		NCreature? creatureNode = NCombatRoom.Instance?.GetCreatureNode(creature);
		if (creatureNode == null)
		{
			return;
		}

		Node2D body = creatureNode.Visuals.GetCurrentBody();
		Vector2 scale = body.Scale;
		float absX = Math.Abs(scale.X);
		body.Scale = new Vector2(positive ? absX : -absX, scale.Y);
	}
}
