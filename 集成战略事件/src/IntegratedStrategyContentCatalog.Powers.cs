using IntegratedStrategyEvents.Powers;

namespace IntegratedStrategyEvents;

internal static partial class IntegratedStrategyContentCatalog
{
	public static Type[] PowerTypes =>
	[
		typeof(UnfinishedFinalePower),
		typeof(RedmarkEradicatorTacticsPower),
		typeof(SaintguardShieldPower),
		typeof(LugalszargusDecisivePower),
		typeof(HaranduhDecisivePower)
	];
}
