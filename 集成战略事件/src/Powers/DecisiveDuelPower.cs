using BaseLib.Abstracts;
using IntegratedStrategyEvents.Encounters;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace IntegratedStrategyEvents.Powers;

public abstract class DecisiveDuelPower<TPartnerMonster> : PowerModel, ICustomPower
	where TPartnerMonster : MonsterModel
{
	private sealed class Data
	{
		public bool HasTriggered;
	}

	private const int StrengthGain = 10;
	private const float VictoryAnimLength = 0.85f;
	private const string JuggernautPowerPackedIconPath =
		"res://images/atlases/power_atlas.sprites/juggernaut_power.tres";
	private const string JuggernautPowerBigIconPath = "res://images/powers/juggernaut_power.png";

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

	public string? CustomPackedIconPath => JuggernautPowerPackedIconPath;

	public string? CustomBigIconPath => JuggernautPowerBigIconPath;

	protected override object InitInternalData()
	{
		return new Data();
	}

	public override async Task AfterDeath(
		PlayerChoiceContext choiceContext,
		Creature creature,
		bool wasRemovalPrevented,
		float deathAnimLength)
	{
		_ = choiceContext;
		_ = deathAnimLength;

		Data data = GetInternalData<Data>();
		if (data.HasTriggered ||
			wasRemovalPrevented ||
			!Owner.IsAlive ||
			ReferenceEquals(creature, Owner) ||
			creature.Monster?.CanonicalInstance is not TPartnerMonster)
		{
			return;
		}

		data.HasTriggered = true;
		Flash();
		await CreatureCmd.TriggerAnim(Owner, DecisiveDuelBoss.VictoryTrigger, VictoryAnimLength);
		await PowerCmd.Apply<StrengthPower>(Owner, StrengthGain, Owner, null);
	}
}
