using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;

namespace IntegratedStrategyEvents.Encounters;

public sealed class PopularAttractionGremlinMercsEncounter : IntegratedStrategyEliteEncounter
{
	public const string StarterSneakySlot = "starter_sneaky";
	public const string MercSlot = "merc";
	public const string SpawnedSneakySlot = "sneaky";
	public const string FatSlot = "fat";

	public override string? CustomScenePath =>
		"res://IntegratedStrategyEvents/scenes/encounters/popular_attraction_gremlins.tscn";

	public override IEnumerable<MonsterModel> AllPossibleMonsters =>
	[
		Monster<SneakyGremlin>(),
		Monster<GremlinMerc>(),
		Monster<FatGremlin>()
	];

	public override IReadOnlyList<string> Slots => [StarterSneakySlot, MercSlot, SpawnedSneakySlot, FatSlot];

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		return
		[
			(MutableMonster<SneakyGremlin>(), StarterSneakySlot),
			(MutableMonster<GremlinMerc>(), MercSlot)
		];
	}
}
