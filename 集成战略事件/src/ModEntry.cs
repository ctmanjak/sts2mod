using System.Reflection;
using IntegratedStrategyEvents.Encounters;
using IntegratedStrategyEvents.Events;
using IntegratedStrategyEvents.Relics;
using IntegratedStrategyEvents.TreeHoles;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace IntegratedStrategyEvents;

[ModInitializer(nameof(Initialize))]
public static class ModEntry
{
	private static Harmony? HarmonyInstance;

	public static void Initialize()
	{
		InjectSavedPropertyCaches();
		RegisterRelics();
		EnsureModelsRegisteredIfModelDbAlreadyInitialized();
		HarmonyInstance ??= new Harmony(ModInfo.HarmonyId);
		IntegratedStrategyModelIdSerializationWarningHooks.Install(HarmonyInstance);
		HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
		IntegratedStrategyEncounterLocalization.Install();
		IntegratedStrategyEventRuntimeCompatibility.Install();
		IntegratedStrategyPotionTracker.Install();
		IntegratedStrategyTreeHoleController.Install();
		Log.Info($"{ModInfo.LogPrefix} Loaded for Slay the Spire 2 {ModInfo.TargetGameVersion}.");
	}

	private static void InjectSavedPropertyCaches()
	{
		SavedPropertiesTypeCache.InjectTypeIntoCache(typeof(ProphecyProjectionRelic));
	}

	private static void RegisterRelics()
	{
		foreach (Type relicType in IntegratedStrategyContentCatalog.EventRelicTypes)
		{
			ModHelper.AddModelToPool(typeof(EventRelicPool), relicType);
		}
	}

	private static void EnsureModelsRegisteredIfModelDbAlreadyInitialized()
	{
		if (!ModelDb.Contains(typeof(Ironclad)))
		{
			return;
		}

		foreach (Type type in IntegratedStrategyContent.ModelTypes)
		{
			if (ModelDb.Contains(type))
			{
				continue;
			}

			ModelDb.Inject(type);
			ModelId id = ModelDb.GetId(type);
			ModelDb.GetById<AbstractModel>(id).InitId(id);
		}
	}
}
