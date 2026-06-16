namespace IntegratedStrategyEvents.Relics;

public sealed partial class RhodesDoorRelic : IntegratedStrategyEventRelic
{
	public RhodesDoorRelic()
		: base("rhodes_door.png")
	{
	}

	public override bool HasUponPickupEffect => true;
}
