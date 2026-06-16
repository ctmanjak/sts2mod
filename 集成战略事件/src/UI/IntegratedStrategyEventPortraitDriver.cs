using Godot;
using MegaCrit.Sts2.Core.Nodes.Events;

namespace IntegratedStrategyEvents.UI;

internal sealed partial class IntegratedStrategyEventPortraitDriver : Node
{
	private const string NodeName = "IntegratedStrategyEventPortraitDriver";

	public NEventLayout? Layout { get; init; }

	internal static void Ensure(NEventLayout layout)
	{
		if (layout.GetNodeOrNull<IntegratedStrategyEventPortraitDriver>(NodeName) != null)
		{
			return;
		}

		layout.AddChild(new IntegratedStrategyEventPortraitDriver
		{
			Name = NodeName,
			Layout = layout
		});
	}

	public override void _Process(double delta)
	{
		if (Layout == null || !GodotObject.IsInstanceValid(Layout))
		{
			QueueFree();
			return;
		}

		IntegratedStrategyEventPortraitFitter.Apply(Layout, false);
	}
}
