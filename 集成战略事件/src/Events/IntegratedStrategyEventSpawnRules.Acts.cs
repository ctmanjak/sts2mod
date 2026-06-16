using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;

namespace IntegratedStrategyEvents.Events;

internal static partial class IntegratedStrategyEventSpawnRules
{
	private static readonly IReadOnlyDictionary<Type, Func<ActModel[]>> ActRules =
		new Dictionary<Type, Func<ActModel[]>>
		{
			[typeof(BoundBloodEvent)] = static () => [ModelDb.Act<Overgrowth>(), ModelDb.Act<Underdocks>()],
			[typeof(SecretDoorEvent)] = static () => [ModelDb.Act<Overgrowth>(), ModelDb.Act<Underdocks>()],
			[typeof(SecretRoomEvent)] = static () => [ModelDb.Act<Overgrowth>(), ModelDb.Act<Underdocks>()],
			[typeof(DustDevouringSpreadEvent)] = static () => [ModelDb.Act<Overgrowth>(), ModelDb.Act<Underdocks>()],
			[typeof(SamiLanguageEvent)] = static () => [ModelDb.Act<Overgrowth>(), ModelDb.Act<Underdocks>()],
			[typeof(BlackFootprintsEvent)] = static () => [ModelDb.Act<Hive>()],
			[typeof(AfterStoryEndsEvent)] = static () => [ModelDb.Act<Hive>()],
			[typeof(DevoutPersonEvent)] = static () => [ModelDb.Act<Hive>()],
			[typeof(PopularAttractionEvent)] = static () => [ModelDb.Act<Hive>()],
			[typeof(SleepingStatueEvent)] = static () => [ModelDb.Act<Hive>()],
			[typeof(SuspicionChainEvent)] = static () => [ModelDb.Act<Hive>()],
			[typeof(TreasureChestDanceEvent)] = static () => [ModelDb.Act<Hive>()],
			[typeof(BusinessEmpireEvent)] = static () => [ModelDb.Act<Glory>()],
			[typeof(HundredMileEncampmentEvent)] = static () => [ModelDb.Act<Glory>()],
			[typeof(InviteToPlayEvent)] = static () => [ModelDb.Act<Glory>()],
			[typeof(NorthWindWitchEvent)] = static () => [ModelDb.Act<Glory>()],
			[typeof(FutureHunterEvent)] = static () => [ModelDb.Act<Glory>()]
		};
}
