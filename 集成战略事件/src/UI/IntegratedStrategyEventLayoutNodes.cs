using Godot;
using MegaCrit.Sts2.Core.Nodes.Events;

namespace IntegratedStrategyEvents.UI;

internal static class IntegratedStrategyEventLayoutNodes
{
	public static void ApplyOptionButtonWidths(NEventLayout layout, float width)
	{
		foreach (Node button in layout.OptionButtons)
		{
			if (button is Control buttonControl)
			{
				ApplyContentWidth(buttonControl, width);
			}
		}
	}

	public static void ApplyContentWidth(Control control, float width)
	{
		Vector2 customMinimumSize = control.CustomMinimumSize;
		control.CustomMinimumSize = new Vector2(width, customMinimumSize.Y);
		control.SizeFlagsHorizontal = Control.SizeFlags.ShrinkBegin;
		control.Size = new Vector2(width, control.Size.Y);
		foreach (Node child in control.GetChildren())
		{
			if (child is Control childControl)
			{
				ApplyContentWidth(childControl, width);
			}
		}
	}

	public static VBoxContainer? GetOptionsContainer(NEventLayout layout)
	{
		VBoxContainer? optionsContainer = layout.GetNodeOrNull<VBoxContainer>("%OptionsContainer");
		if (optionsContainer != null)
		{
			return optionsContainer;
		}

		return layout.OptionButtons.FirstOrDefault()?.GetParent() as VBoxContainer;
	}

	public static Control GetEventTextContainer(NEventLayout layout, VBoxContainer optionsContainer)
	{
		Node? parent = optionsContainer.GetParent();

		return parent is Control control && control != layout
			? control
			: optionsContainer;
	}
}
