namespace IntegratedStrategyEvents;

internal static partial class IntegratedStrategyContentCatalog
{
	public static Type[] ModelTypes =>
	[
		.. EventTypes,
		.. EventRelicTypes,
		.. EncounterTypes,
		.. PowerTypes
	];
}
