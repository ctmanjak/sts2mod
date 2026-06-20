using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Ancients;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Managers;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace Illaoi;

[ModInitializer(nameof(Initialize))]
public static class ModEntry
{
	private static Harmony? HarmonyInstance;
	private static bool HooksInstalled;
	private static bool PoolEntriesRegistered;

	public static void Initialize()
	{
		InjectSavedPropertyCaches();
		EnsureModelsRegisteredIfModelDbAlreadyInitialized();
		RegisterPoolEntries();
		Harmony harmony = InstallHooks();
		AssetHooks.Install(harmony);
		ResetModelDbCaches();
		ResetIllaoiCardPoolCache();
		LogIllaoiCardPoolState();
		Log.Info($"{ModInfo.LogPrefix} Loaded for Slay the Spire 2 {ModInfo.TargetGameVersion}.");
	}

	private static void InjectSavedPropertyCaches()
	{
		SavedPropertiesTypeCache.InjectTypeIntoCache(typeof(NagakabourosIdol));
		SavedPropertiesTypeCache.InjectTypeIntoCache(typeof(NagakabourosTouch));
		SavedPropertiesTypeCache.InjectTypeIntoCache(typeof(IllaoiTentacleMonster));
		SavedPropertiesTypeCache.InjectTypeIntoCache(typeof(IllaoiSoulMonster));
		SavedPropertiesTypeCache.InjectTypeIntoCache(typeof(IllaoiSoulLinkPower));
		SavedPropertiesTypeCache.InjectTypeIntoCache(typeof(IllaoiHuskPower));
		SavedPropertiesTypeCache.InjectTypeIntoCache(typeof(IllaoiTemporaryStrengthPower));
		SavedPropertiesTypeCache.InjectTypeIntoCache(typeof(IllaoiTemporaryDexterityPower));
		SavedPropertiesTypeCache.InjectTypeIntoCache(typeof(IllaoiFaithPower));
		SavedPropertiesTypeCache.InjectTypeIntoCache(typeof(IllaoiGrowTipPower));
		SavedPropertiesTypeCache.InjectTypeIntoCache(typeof(IllaoiTentacleTipPower));
		SavedPropertiesTypeCache.InjectTypeIntoCache(typeof(IllaoiCommandTipPower));
		SavedPropertiesTypeCache.InjectTypeIntoCache(typeof(IllaoiDrainPower));
		SavedPropertiesTypeCache.InjectTypeIntoCache(typeof(IllaoiAncientGodProphetPower));
		SavedPropertiesTypeCache.InjectTypeIntoCache(typeof(IllaoiSoulImpactPower));
		SavedPropertiesTypeCache.InjectTypeIntoCache(typeof(IllaoiNagakabourosDescendsPower));
		SavedPropertiesTypeCache.InjectTypeIntoCache(typeof(IllaoiTidecallerPower));
		SavedPropertiesTypeCache.InjectTypeIntoCache(typeof(IllaoiRelentlessFaithPower));
		SavedPropertiesTypeCache.InjectTypeIntoCache(typeof(IllaoiGrowthBlockPower));
		SavedPropertiesTypeCache.InjectTypeIntoCache(typeof(IllaoiRhythmOfMotionPower));
		SavedPropertiesTypeCache.InjectTypeIntoCache(typeof(IllaoiFervorOfMotionPower));
		SavedPropertiesTypeCache.InjectTypeIntoCache(typeof(IllaoiWatchfulIdolPower));
		SavedPropertiesTypeCache.InjectTypeIntoCache(typeof(IllaoiSeaAnswersPower));
		SavedPropertiesTypeCache.InjectTypeIntoCache(typeof(IllaoiNextTurnDrawPower));
		SavedPropertiesTypeCache.InjectTypeIntoCache(typeof(IllaoiDivineFormPower));
		SavedPropertiesTypeCache.InjectTypeIntoCache(typeof(IllaoiNextTurnFaithPower));
	}

	private static void EnsureModelsRegisteredIfModelDbAlreadyInitialized()
	{
		if (!ModelDb.Contains(typeof(Ironclad)))
		{
			return;
		}

		foreach (Type type in IllaoiContent.ModelTypes)
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

	private static void RegisterPoolEntries()
	{
		if (PoolEntriesRegistered)
		{
			return;
		}

		ModHelper.AddModelToPool<EventRelicPool, NagakabourosTouch>();
		PoolEntriesRegistered = true;
	}

	private static Harmony InstallHooks()
	{
		Harmony harmony = HarmonyInstance ??= new Harmony("Natsuki.Illaoi");
		if (HooksInstalled)
		{
			return harmony;
		}

		HooksInstalled = true;
		PatchAncientDialogues<Neow>(harmony, nameof(NeowDefineDialoguesPostfix));
		PatchAncientDialogues<Darv>(harmony, nameof(DarvDefineDialoguesPostfix));
		PatchAncientDialogues<Nonupeipe>(harmony, nameof(NonupeipeDefineDialoguesPostfix));
		PatchAncientDialogues<Tanx>(harmony, nameof(TanxDefineDialoguesPostfix));
		PatchAncientDialogues<Orobas>(harmony, nameof(OrobasDefineDialoguesPostfix));
		PatchAncientDialogues<Pael>(harmony, nameof(PaelDefineDialoguesPostfix));
		PatchAncientDialogues<Tezcatara>(harmony, nameof(TezcataraDefineDialoguesPostfix));
		PatchAncientDialogues<Vakuu>(harmony, nameof(VakuuDefineDialoguesPostfix));
		PatchAncientDialogues<TheArchitect>(harmony, nameof(TheArchitectDefineDialoguesPostfix));
		harmony.Patch(
			RequireMethod(
				typeof(AncientDialogueSet),
				nameof(AncientDialogueSet.GetValidDialogues),
				BindingFlags.Instance | BindingFlags.Public,
				typeof(ModelId),
				typeof(int),
				typeof(int),
				typeof(bool)),
			prefix: new HarmonyMethod(RequirePatchMethod(nameof(IllaoiAncientDialogueSetGetValidDialoguesPrefix))));
		harmony.Patch(
			RequireMethod(typeof(ProgressSaveManager), "CheckFifteenElitesDefeatedEpoch", BindingFlags.Instance | BindingFlags.NonPublic, typeof(Player)),
			prefix: new HarmonyMethod(RequirePatchMethod(nameof(ProgressCharacterEpochCheckPrefix))));
		harmony.Patch(
			RequireMethod(typeof(ProgressSaveManager), "CheckFifteenBossesDefeatedEpoch", BindingFlags.Instance | BindingFlags.NonPublic, typeof(Player)),
			prefix: new HarmonyMethod(RequirePatchMethod(nameof(ProgressCharacterEpochCheckPrefix))));
		harmony.Patch(
			RequireMethod(
				typeof(ProgressSaveManager),
				"ObtainCharUnlockEpoch",
				BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
				typeof(Player),
				typeof(int)),
			prefix: new HarmonyMethod(RequirePatchMethod(nameof(ProgressCharacterEpochCheckPrefix))));
		harmony.Patch(
			RequireMethod(
				typeof(TouchOfOrobas),
				"GetUpgradedStarterRelic",
				BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
				typeof(RelicModel)),
				postfix: new HarmonyMethod(RequirePatchMethod(nameof(TouchOfOrobasGetUpgradedStarterRelicPostfix))));
		harmony.Patch(
			RequireMethod(typeof(ArchaicTooth), "get_TranscendenceUpgrades", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic),
			postfix: new HarmonyMethod(RequirePatchMethod(nameof(ArchaicToothTranscendenceUpgradesPostfix))));
		harmony.Patch(
			RequireMethod(
				typeof(RunState),
				nameof(RunState.CreateForNewRun),
				BindingFlags.Static | BindingFlags.Public,
				typeof(IReadOnlyList<Player>),
				typeof(IReadOnlyList<ActModel>),
				typeof(IReadOnlyList<ModifierModel>),
				typeof(GameMode),
				typeof(int),
				typeof(string)),
			postfix: new HarmonyMethod(RequirePatchMethod(nameof(RunStateCreateForNewRunPostfix))));
		return harmony;
	}

	private static void PatchAncientDialogues<TAncient>(Harmony harmony, string patchMethodName)
	{
		MethodInfo? defineDialoguesMethod = AccessTools.Method(typeof(TAncient), "DefineDialogues");
		if (defineDialoguesMethod != null)
		{
			harmony.Patch(
				defineDialoguesMethod,
				postfix: new HarmonyMethod(RequirePatchMethod(patchMethodName)));
			return;
		}

		Log.Warn($"{ModInfo.LogPrefix} Could not find {typeof(TAncient).Name}.DefineDialogues; Illaoi dialogue will not be registered.");
	}

	private static void NeowDefineDialoguesPostfix(AncientDialogueSet __result)
	{
		if (!TryGetIllaoiCharacterEntry(out string characterEntry))
		{
			return;
		}

		__result.CharacterDialogues[characterEntry] =
		[
			new AncientDialogue(["event:/sfx/npcs/neow/neow_welcome", "", "event:/sfx/npcs/neow/neow_sleepy", ""])
			{
				VisitIndex = 0
			},
			new AncientDialogue(["event:/sfx/npcs/neow/neow_welcome", "", "", ""])
			{
				VisitIndex = 1
			},
			new AncientDialogue(["event:/sfx/npcs/neow/neow_sleepy", "", "event:/sfx/npcs/neow/neow_welcome"])
			{
				VisitIndex = 4
			}
		];
	}

	private static void DarvDefineDialoguesPostfix(AncientDialogueSet __result)
	{
		if (!TryGetIllaoiCharacterEntry(out string characterEntry))
		{
			return;
		}

		__result.CharacterDialogues[characterEntry] =
		[
			new AncientDialogue(["event:/sfx/npcs/darv/darv_introduction", "", "event:/sfx/npcs/darv/darv_endeared"])
			{
				VisitIndex = 0
			},
			new AncientDialogue(["event:/sfx/npcs/darv/darv_excited", "", "", "event:/sfx/npcs/darv/darv_outta_the_way"])
			{
				VisitIndex = 1
			},
			new AncientDialogue(["event:/sfx/npcs/darv/darv_endeared", "", "event:/sfx/npcs/darv/darv_excited"])
			{
				VisitIndex = 4
			}
		];
	}

	private static void NonupeipeDefineDialoguesPostfix(AncientDialogueSet __result)
	{
		if (!TryGetIllaoiCharacterEntry(out string characterEntry))
		{
			return;
		}

		__result.CharacterDialogues[characterEntry] =
		[
			new AncientDialogue(["event:/sfx/npcs/nonupeipe/nonupeipe_eeked", "", "event:/sfx/npcs/nonupeipe/nonupeipe_giggle", ""])
			{
				VisitIndex = 0
			},
			new AncientDialogue(["event:/sfx/npcs/nonupeipe/nonupeipe_giggle", "", "", "event:/sfx/npcs/nonupeipe/nonupeipe_eeked"])
			{
				VisitIndex = 1
			},
			new AncientDialogue(["", "", "event:/sfx/npcs/nonupeipe/nonupeipe_giggle"])
			{
				VisitIndex = 4
			}
		];
	}

	private static void TanxDefineDialoguesPostfix(AncientDialogueSet __result)
	{
		if (!TryGetIllaoiCharacterEntry(out string characterEntry))
		{
			return;
		}

		__result.CharacterDialogues[characterEntry] =
		[
			new AncientDialogue(["event:/sfx/npcs/tanx/tanx_curiosity", "", "event:/sfx/npcs/tanx/tanx_laugh", "", "event:/sfx/npcs/tanx/tanx_roar"])
			{
				VisitIndex = 0
			},
			new AncientDialogue(["event:/sfx/npcs/tanx/tanx_roar", "", "event:/sfx/npcs/tanx/tanx_laugh"])
			{
				VisitIndex = 1
			},
			new AncientDialogue(["event:/sfx/npcs/tanx/tanx_laugh", "", "", "event:/sfx/npcs/tanx/tanx_roar"])
			{
				VisitIndex = 4
			}
		];
	}

	private static void OrobasDefineDialoguesPostfix(AncientDialogueSet __result)
	{
		if (!TryGetIllaoiCharacterEntry(out string characterEntry))
		{
			return;
		}

		__result.CharacterDialogues[characterEntry] =
		[
			new AncientDialogue(["", "", "", "", ""])
			{
				VisitIndex = 0
			},
			new AncientDialogue(["", "", "", ""])
			{
				VisitIndex = 1
			},
			new AncientDialogue(["", "", ""])
			{
				VisitIndex = 4
			}
		];
	}

	private static void PaelDefineDialoguesPostfix(AncientDialogueSet __result)
	{
		if (!TryGetIllaoiCharacterEntry(out string characterEntry))
		{
			return;
		}

		__result.CharacterDialogues[characterEntry] =
		[
			new AncientDialogue(["", "", ""])
			{
				VisitIndex = 0
			},
			new AncientDialogue(["", "", ""])
			{
				VisitIndex = 1
			},
			new AncientDialogue(["", "", ""])
			{
				VisitIndex = 4
			}
		];
	}

	private static void TezcataraDefineDialoguesPostfix(AncientDialogueSet __result)
	{
		if (!TryGetIllaoiCharacterEntry(out string characterEntry))
		{
			return;
		}

		__result.CharacterDialogues[characterEntry] =
		[
			new AncientDialogue(["", "", ""])
			{
				VisitIndex = 0
			},
			new AncientDialogue(["", "", ""])
			{
				VisitIndex = 1
			},
			new AncientDialogue(["", "", ""])
			{
				VisitIndex = 4
			}
		];
	}

	private static void VakuuDefineDialoguesPostfix(AncientDialogueSet __result)
	{
		if (!TryGetIllaoiCharacterEntry(out string characterEntry))
		{
			return;
		}

		__result.CharacterDialogues[characterEntry] =
		[
			new AncientDialogue(["", "", ""])
			{
				VisitIndex = 0
			},
			new AncientDialogue(["", "", ""])
			{
				VisitIndex = 1
			},
			new AncientDialogue(["", "", ""])
			{
				VisitIndex = 4
			}
		];
	}

	private static void TheArchitectDefineDialoguesPostfix(AncientDialogueSet __result)
	{
		if (!TryGetIllaoiCharacterEntry(out string characterEntry))
		{
			return;
		}

		__result.CharacterDialogues[characterEntry] =
		[
			new AncientDialogue(["", "", "", ""])
			{
				VisitIndex = 0,
				EndAttackers = ArchitectAttackers.Both
			},
			new AncientDialogue(["", "", "", ""])
			{
				VisitIndex = 1,
				EndAttackers = ArchitectAttackers.Both
			},
			new AncientDialogue(["", "", "", ""])
			{
				VisitIndex = 2,
				EndAttackers = ArchitectAttackers.Both
			},
			new AncientDialogue(["", "", "", ""])
			{
				VisitIndex = 3,
				EndAttackers = ArchitectAttackers.Both
			}
		];
	}

	private static bool IllaoiAncientDialogueSetGetValidDialoguesPrefix(
		AncientDialogueSet __instance,
		ModelId characterId,
		int charVisits,
		ref IEnumerable<AncientDialogue> __result)
	{
		if (!TryGetIllaoiCharacterEntry(out string characterEntry)
			|| characterId.Entry != characterEntry
			|| !__instance.CharacterDialogues.TryGetValue(characterEntry, out IReadOnlyList<AncientDialogue>? characterDialogues))
		{
			return true;
		}

		List<AncientDialogue> exactDialogues = characterDialogues
			.Where(dialogue => dialogue.VisitIndex == charVisits)
			.ToList();
		if (exactDialogues.Count > 0)
		{
			__result = exactDialogues;
			return false;
		}

		List<AncientDialogue> repeatingDialogues = characterDialogues
			.Where(dialogue => dialogue.IsRepeating
				&& (!dialogue.VisitIndex.HasValue || charVisits >= dialogue.VisitIndex.Value))
			.ToList();
		if (repeatingDialogues.Count > 0)
		{
			__result = repeatingDialogues;
			return false;
		}

		return true;
	}

	private static bool TryGetIllaoiCharacterEntry(out string characterEntry)
	{
		characterEntry = string.Empty;
		if (!ModelDb.Contains(typeof(IllaoiCharacter)))
		{
			return false;
		}

		characterEntry = ModelDb.GetId<IllaoiCharacter>().Entry;
		return true;
	}

	private static void ArchaicToothTranscendenceUpgradesPostfix(Dictionary<ModelId, CardModel> __result)
	{
		if (!ModelDb.Contains(typeof(GraspingLesson)) || !ModelDb.Contains(typeof(TrialOfTheAncientGod)))
		{
			return;
		}

		__result[ModelDb.Card<GraspingLesson>().Id] = ModelDb.Card<TrialOfTheAncientGod>();
	}

	private static void RunStateCreateForNewRunPostfix(IReadOnlyList<Player> __0)
	{
		if (__0.Any(player => player.Character is IllaoiCharacter
			|| player.Character.Id == ModelDb.GetId<IllaoiCharacter>()))
		{
			AssetHooks.PlayIllaoiNewRunVoice();
		}
	}

	private static bool ProgressCharacterEpochCheckPrefix(Player __0)
	{
		if (__0.Character is IllaoiCharacter)
		{
			Log.Info($"{ModInfo.LogPrefix} Skipping vanilla character-specific progress epoch check for Illaoi.");
			return false;
		}

		return true;
	}

	private static void TouchOfOrobasGetUpgradedStarterRelicPostfix(RelicModel starterRelic, ref RelicModel __result)
	{
		if (!ModelDb.Contains(typeof(NagakabourosIdol))
			|| !ModelDb.Contains(typeof(NagakabourosTouch))
			|| starterRelic.Id != ModelDb.GetId<NagakabourosIdol>())
		{
			return;
		}

		__result = ModelDb.Relic<NagakabourosTouch>();
	}

	private static void ResetModelDbCaches()
	{
		string[] cacheFields =
		[
			"_allCards",
			"_allCardPools",
			"_allCharacterCardPools",
			"_allPotions",
			"_allPotionPools",
			"_allCharacterPotionPools",
			"_allRelics",
			"_allCharacterRelicPools"
		];

		foreach (string fieldName in cacheFields)
		{
			FieldInfo? field = typeof(ModelDb).GetField(fieldName, BindingFlags.Static | BindingFlags.NonPublic);
			field?.SetValue(null, null);
		}
	}

	private static void ResetIllaoiCardPoolCache()
	{
		if (!ModelDb.Contains(typeof(IllaoiCardPool)))
		{
			return;
		}

		CardPoolModel cardPool = ModelDb.CardPool<IllaoiCardPool>();
		foreach (string fieldName in new[] { "_allCards", "_allCardIds" })
		{
			FieldInfo? field = typeof(CardPoolModel).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
			field?.SetValue(cardPool, null);
		}
	}

	private static void LogIllaoiCardPoolState()
	{
		if (!ModelDb.Contains(typeof(IllaoiCardPool)) || !ModelDb.Contains(typeof(TheSeaAnswers)))
		{
			return;
		}

		CardModel[] cards = ModelDb.CardPool<IllaoiCardPool>().AllCards.ToArray();
		bool includesTheSeaAnswers = cards.Any(card => card.Id == ModelDb.GetId<TheSeaAnswers>());
		Log.Info($"{ModInfo.LogPrefix} Card pool cards={cards.Length}; includesTheSeaAnswers={includesTheSeaAnswers}.");
	}

	private static MethodInfo RequireGetter(Type type, string propertyName, BindingFlags flags)
	{
		return type.GetProperty(propertyName, flags)?.GetMethod
			?? throw new InvalidOperationException($"Could not find getter {type.FullName}.{propertyName}.");
	}

	private static MethodInfo RequireMethod(Type type, string methodName, BindingFlags flags, params Type[] parameterTypes)
	{
		return type.GetMethod(methodName, flags, null, parameterTypes, null)
			?? throw new InvalidOperationException($"Could not find method {type.FullName}.{methodName}.");
	}

	private static MethodInfo RequirePatchMethod(string methodName)
	{
		return typeof(ModEntry).GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic)
			?? throw new InvalidOperationException($"Could not find Harmony patch method {methodName}.");
	}
}
