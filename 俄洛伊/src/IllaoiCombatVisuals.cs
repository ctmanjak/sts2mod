using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace Illaoi;

internal static class IllaoiCombatVisuals
{
	private const string SoulLinkNodeNamePrefix = "IllaoiSoulLinkLine";
	private const float SoulLinkWidth = 8f;
	private const float SoulLinkFlashWidth = 14f;
	private const float SoulLaneGapX = 44f;
	private const float SoulLaneGapY = 78f;
	private const float SoulColumnGapX = 70f;
	private static readonly Color SoulLinkColor = new(0.48f, 0.86f, 1f, 0.34f);
	private static readonly Color SoulLinkFlashColor = new(0.76f, 1f, 1f, 0.92f);
	private static readonly Color TentacleVisualModulate = new(1f, 1f, 1f, 0.62f);
	private static readonly float[] SoulLaneOrder = [0f, -1f, 1f, -2f, 2f, -3f, 3f];

	public static void PositionTentacle(Creature tentacle)
	{
		if (tentacle.Monster is IllaoiTentacleMonster && tentacle.PetOwner != null)
		{
			PositionTentacles(tentacle.PetOwner);
			return;
		}

		PositionTentacleNow(tentacle);
		Callable.From(() => PositionTentacleNow(tentacle)).CallDeferred();
	}

	public static void PositionTentacles(Player player)
	{
		PositionTentaclesNow(player);
		Callable.From(() => PositionTentaclesNow(player)).CallDeferred();
	}

	public static void CleanupTentacles(Player player)
	{
		NCombatRoom? room = NCombatRoom.Instance;
		if (room == null)
		{
			return;
		}

		foreach (Creature tentacle in player.Creature.Pets.Where(static creature => creature.Monster is IllaoiTentacleMonster).ToList())
		{
			NCreature? node = room.GetCreatureNode(tentacle);
			if (node == null)
			{
				continue;
			}

			node.ToggleIsInteractable(on: false);
			node.IntentContainer.Visible = false;
			node.Hide();
		}
	}

	public static void AnimateTentacleAttack(Creature tentacle, Creature target)
	{
		if (tentacle.Monster is not IllaoiTentacleMonster)
		{
			return;
		}

		NCombatRoom? room = NCombatRoom.Instance;
		NCreature? tentacleNode = room?.GetCreatureNode(tentacle);
		NCreature? targetNode = room?.GetCreatureNode(target);
		if (tentacleNode == null || targetNode == null)
		{
			return;
		}

		Vector2 start = tentacleNode.Position;
		Vector2 direction = targetNode.Position - start;
		if (direction.LengthSquared() < 1f)
		{
			tentacleNode.AnimShake();
			return;
		}

		Tween tween = tentacleNode.CreateTween();
		Vector2 lunge = start + direction.Normalized() * 55f;
		tween.TweenProperty(tentacleNode, "position", lunge, 0.08).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Sine);
		tween.TweenProperty(tentacleNode, "position", start, 0.14).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Sine);
	}

	public static void PositionSoulNearBody(Creature soul, Creature body)
	{
		PositionSoulsNearBody(body);
		Callable.From(() => PositionSoulsNearBody(body)).CallDeferred();
	}

	public static void PositionSoulsNearBody(Creature body)
	{
		PositionSoulsNearBodyNow(body);
	}

	public static void RemoveSoulLink(Creature soul)
	{
		foreach (Line2D line in FindSoulLinkLines(soul))
		{
			line.QueueFree();
		}

		if (soul.GetPower<IllaoiSoulLinkPower>()?.Target is { IsAlive: true } body)
		{
			Callable.From(() => PositionSoulsNearBodyNow(body)).CallDeferred();
		}
	}

	public static void RemoveSoulLinksForBody(Creature body)
	{
		foreach (Creature soul in GetLivingSoulsLinkedToBody(body))
		{
			RemoveSoulLink(soul);
		}
	}

	public static void FlashSoulLink(Creature soul)
	{
		FlashSoulLinkNow(soul);
		Callable.From(() => FlashSoulLinkNow(soul)).CallDeferred();
	}

	private static void PositionTentacleNow(Creature tentacle)
	{
		if (tentacle.Monster is not IllaoiTentacleMonster model || tentacle.PetOwner == null)
		{
			return;
		}

		NCombatRoom? room = NCombatRoom.Instance;
		NCreature? tentacleNode = room?.GetCreatureNode(tentacle);
		NCreature? ownerNode = room?.GetCreatureNode(tentacle.PetOwner.Creature);
		if (tentacleNode == null || ownerNode == null)
		{
			return;
		}

		Node? parent = tentacleNode.GetParent();
		if (parent == ownerNode.GetParent())
		{
			parent.MoveChild(tentacleNode, Math.Max(0, ownerNode.GetIndex()));
		}

		tentacleNode.Show();
		tentacleNode.Modulate = Colors.White;
		tentacleNode.Visuals.Show();
		tentacleNode.Visuals.Modulate = TentacleVisualModulate;
		tentacleNode.Position = ownerNode.Position + model.VisualOffset;
		tentacleNode.SetDefaultScaleTo(0.58f, 0f);
		tentacleNode.ToggleIsInteractable(on: false);
		tentacleNode.Visuals.Bounds.Visible = false;
		tentacleNode.IntentContainer.Visible = false;
	}

	private static void PositionTentaclesNow(Player player)
	{
		foreach (Creature tentacle in player.Creature.Pets.Where(static creature => creature.Monster is IllaoiTentacleMonster && creature.IsAlive))
		{
			PositionTentacleNow(tentacle);
		}
	}

	private static void PositionSoulsNearBodyNow(Creature body)
	{
		if (body.IsDead)
		{
			RemoveSoulLinksForBody(body);
			return;
		}

		NCombatRoom? room = NCombatRoom.Instance;
		NCreature? bodyNode = room?.GetCreatureNode(body);
		if (room == null || bodyNode == null)
		{
			return;
		}

		IReadOnlyList<Creature> souls = GetLivingSoulsLinkedToBody(body);
		for (int i = 0; i < souls.Count; i++)
		{
			NCreature? soulNode = room.GetCreatureNode(souls[i]);
			if (soulNode == null)
			{
				continue;
			}

			soulNode.Position = bodyNode.Position + GetSoulOffset(bodyNode, i);
			DrawSoulLink(souls[i], soulNode, bodyNode);
		}
	}

	private static IReadOnlyList<Creature> GetLivingSoulsLinkedToBody(Creature body)
	{
		return body.CombatState?.Enemies
			.Where(creature => creature.IsAlive
				&& creature.Monster is IllaoiSoulMonster
				&& creature.GetPower<IllaoiSoulLinkPower>()?.Target == body)
			.ToList() ?? [];
	}

	private static Vector2 GetSoulOffset(NCreature bodyNode, int index)
	{
		int laneIndex = index % SoulLaneOrder.Length;
		int column = index / SoulLaneOrder.Length;
		float lane = SoulLaneOrder[laneIndex];
		float baseX = Math.Max(155f, bodyNode.Visuals.Bounds.Size.X * 0.35f);
		return new Vector2(
			baseX + Math.Abs(lane) * SoulLaneGapX + column * SoulColumnGapX,
			-10f + lane * SoulLaneGapY);
	}

	private static void DrawSoulLink(Creature soul, NCreature soulNode, NCreature bodyNode)
	{
		Node? parent = soulNode.GetParent();
		if (parent == null || bodyNode.GetParent() != parent)
		{
			return;
		}

		string nodeName = GetSoulLinkNodeName(soul);
		foreach (Line2D existingLine in FindSoulLinkLines(soul))
		{
			existingLine.QueueFree();
		}

		Line2D line = new()
		{
			Name = nodeName,
			Width = SoulLinkWidth,
			DefaultColor = SoulLinkColor,
			Antialiased = true,
			ZIndex = -20,
			Points =
			[
				bodyNode.Position + new Vector2(0f, -120f),
				soulNode.Position + new Vector2(0f, -120f)
			]
		};
		parent.AddChild(line);
		parent.MoveChild(line, 0);
	}

	private static void FlashSoulLinkNow(Creature soul)
	{
		foreach (Line2D line in FindSoulLinkLines(soul))
		{
			line.DefaultColor = SoulLinkFlashColor;
			line.Width = SoulLinkFlashWidth;
			Tween tween = line.CreateTween();
			tween.TweenProperty(line, "default_color", SoulLinkColor, 0.18)
				.SetEase(Tween.EaseType.Out)
				.SetTrans(Tween.TransitionType.Sine);
			tween.Parallel().TweenProperty(line, "width", SoulLinkWidth, 0.18)
				.SetEase(Tween.EaseType.Out)
				.SetTrans(Tween.TransitionType.Sine);
		}
	}

	private static List<Line2D> FindSoulLinkLines(Creature soul)
	{
		List<Line2D> lines = [];
		NCombatRoom? room = NCombatRoom.Instance;
		if (room != null)
		{
			CollectSoulLinkLines(room, GetSoulLinkNodeName(soul), lines);
		}

		return lines;
	}

	private static void CollectSoulLinkLines(Node node, string nodeName, List<Line2D> lines)
	{
		foreach (Node child in node.GetChildren())
		{
			if (child.Name.ToString() == nodeName && child is Line2D line)
			{
				lines.Add(line);
			}

			CollectSoulLinkLines(child, nodeName, lines);
		}
	}

	private static string GetSoulLinkNodeName(Creature soul)
	{
		return $"{SoulLinkNodeNamePrefix}_{soul.CombatId?.ToString() ?? "none"}";
	}
}
