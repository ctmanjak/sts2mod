#!/bin/zsh
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
EVENT_DIR="$ROOT/src/Events"
DEFINITION_DIR="$EVENT_DIR/Definitions"
ENCOUNTER_DIR="$ROOT/src/Encounters"
UI_DIR="$ROOT/src/UI"
RELIC_DIR="$ROOT/src/Relics"
ASSET_DIR="$ROOT/assets"
CATALOG_FILE="$ROOT/src/IntegratedStrategyContentCatalog.cs"
CATALOG_EVENT_FILE="$ROOT/src/IntegratedStrategyContentCatalog.Events.cs"
CATALOG_RELIC_FILE="$ROOT/src/IntegratedStrategyContentCatalog.Relics.cs"
CATALOG_ENCOUNTER_FILE="$ROOT/src/IntegratedStrategyContentCatalog.Encounters.cs"
SUMMARY_FILES=(
  "$ROOT/event_descriptions.txt"
  "$ROOT/event_options.txt"
  "$ROOT/event_refresh_conditions.txt"
)
REQUIRED_STRUCTURE_FILES=(
  "$CATALOG_EVENT_FILE"
  "$CATALOG_RELIC_FILE"
  "$CATALOG_ENCOUNTER_FILE"
  "$EVENT_DIR/IntegratedStrategyEventModel.Navigation.cs"
  "$EVENT_DIR/IntegratedStrategyEventModel.Options.cs"
  "$EVENT_DIR/IntegratedStrategyEventModel.Vitals.cs"
  "$EVENT_DIR/IntegratedStrategyEventModel.Rewards.cs"
  "$EVENT_DIR/IntegratedStrategyEventModel.Deck.cs"
  "$EVENT_DIR/IntegratedStrategyEventLocalization.cs"
  "$EVENT_DIR/IntegratedStrategyEventEffects.Vitals.cs"
  "$EVENT_DIR/IntegratedStrategyEventEffects.Rewards.cs"
  "$EVENT_DIR/IntegratedStrategyEventEffects.CardGrant.cs"
  "$EVENT_DIR/IntegratedStrategyEventEffects.Deck.cs"
  "$EVENT_DIR/IntegratedStrategyEventRewards.Relics.cs"
  "$EVENT_DIR/IntegratedStrategyEventRewards.Potions.cs"
  "$EVENT_DIR/IntegratedStrategyEventRewards.CardRewards.cs"
  "$EVENT_DIR/IntegratedStrategyEventRewards.CardRolls.cs"
  "$EVENT_DIR/IntegratedStrategyEventRewards.CardPools.cs"
  "$EVENT_DIR/IntegratedStrategyEventSpawnRules.Acts.cs"
  "$EVENT_DIR/IntegratedStrategyEventSpawnRules.Gates.cs"
  "$EVENT_DIR/TimidThievesEvent.Loot.cs"
  "$ENCOUNTER_DIR/IntegratedStrategyEliteEncounter.cs"
  "$ENCOUNTER_DIR/IntegratedStrategyBossEncounter.cs"
  "$ENCOUNTER_DIR/IntegratedStrategyEncounterHook.cs"
  "$ENCOUNTER_DIR/IntegratedStrategyTwoSidedEliteEncounter.cs"
  "$ENCOUNTER_DIR/IntegratedStrategyEncounterSetup.cs"
  "$RELIC_DIR/RhodesDoorRelic.Pickup.cs"
  "$RELIC_DIR/RhodesDoorRelic.Combat.cs"
  "$UI_DIR/IntegratedStrategyEventPortraitFitter.cs"
  "$UI_DIR/IntegratedStrategyEventPortraitDriver.cs"
  "$UI_DIR/IntegratedStrategyEventLayoutApplier.cs"
  "$UI_DIR/IntegratedStrategyEventLayoutBaseline.cs"
  "$UI_DIR/IntegratedStrategyEventLayoutGeometry.cs"
  "$UI_DIR/IntegratedStrategyEventLayoutNodes.cs"
)

fail() {
  print -u2 "validate_event_structure: $*"
  exit 1
}

[[ -d "$DEFINITION_DIR" ]] || fail "missing $DEFINITION_DIR"
[[ -f "$CATALOG_FILE" ]] || fail "missing $CATALOG_FILE"

for summary_file in "${SUMMARY_FILES[@]}"; do
  [[ -f "$summary_file" ]] || fail "missing $summary_file"
done

for structure_file in "${REQUIRED_STRUCTURE_FILES[@]}"; do
  [[ -f "$structure_file" ]] || fail "missing structural helper file: $structure_file"
done

event_id_from_class() {
  perl -pe 's/Event$//; s/([a-z0-9])([A-Z])/$1_$2/g; tr/a-z/A-Z/; s/^/EVENT./; s/$/_EVENT/;' <<< "$1"
}

for event_file in "$EVENT_DIR"/*Event.cs; do
  event_name="$(basename "$event_file" .cs)"
  [[ "$event_name" == IntegratedStrategyEvent ]] && continue
  [[ -f "$DEFINITION_DIR/$event_name.Definition.cs" ]] \
    || fail "missing definition file for $event_name"
  rg -q --fixed-strings "typeof($event_name)" "$CATALOG_EVENT_FILE" \
    || fail "$event_name is missing from IntegratedStrategyContentCatalog.Events.cs"

  event_id="$(event_id_from_class "$event_name")"
  for summary_file in "${SUMMARY_FILES[@]}"; do
    rg -q --fixed-strings "$event_id" "$summary_file" \
      || fail "$event_id is missing from $(basename "$summary_file")"
  done
done

if rg -n "LocManager|EventLoc|EventPageLoc|EventOptionLoc|CustomInitialPortraitPath|ShouldLeftAlignOptions|LeftAlignedContentWidthScale" "$EVENT_DIR"/*Event.cs >/dev/null; then
  fail "event logic files contain localization, portrait, or legacy layout declarations"
fi

if rg -n "HoverTipFactory|[.]WithRelic<|HoverTips[[:space:]]*=" "$EVENT_DIR"/*Event.cs >/dev/null; then
  fail "event logic files must use IntegratedStrategyEventModel option preview helpers instead of manual hover-tip wiring"
fi

if rg -n 'Choice[(][[:space:]]*StartFight,[[:space:]]*"FIGHT"' "$EVENT_DIR"/*Event.cs >/dev/null; then
  fail "event logic files must use FightChoice or ShowFightPage for fight follow-up options"
fi

if rg -n "EnterCombatWithoutExitingEvent|Array[.]Empty<Reward>|private Task StartFight" "$EVENT_DIR"/*Event.cs >/dev/null; then
  fail "event logic files must use EnterEventCombat, FightChoice<TEncounter>, or ShowFightPage<TEncounter> instead of direct combat-entry boilerplate"
fi

if rg -n "PotionCmd[.]TryToProcure|ProcureRandomPotion|ProcurePotion" "$EVENT_DIR" --glob '*.cs' >/dev/null; then
  fail "event potion rewards must use OfferPotionReward or OfferRandomPotionReward so potions appear in the reward-claim window"
fi

if rg -n "LocManager|MegaCrit.Sts2.Core.Localization" "$DEFINITION_DIR" >/dev/null; then
  fail "event definition files must use IntegratedStrategyEventLocalization instead of direct LocManager access"
fi

if rg -n "res://IntegratedStrategyEvents/images/events/|private const string PortraitPath" "$DEFINITION_DIR" >/dev/null; then
  fail "event definition files must use IntegratedStrategyEventDefinition.ForEventPortrait instead of hardcoded event portrait paths"
fi

for definition_file in "$DEFINITION_DIR"/*Event.Definition.cs; do
  rg -q "ForEventPortrait[(]" "$definition_file" \
    || fail "$(basename "$definition_file") must use IntegratedStrategyEventDefinition.ForEventPortrait"
done

if rg -n "GetOwnerOrThrow|private static bool CanLoseHp|private static Task LoseHp|private static bool HasUpgradableCards|private static bool HasTransformableDeckCards|CountTransformableDeckCards|RollPotion|Create.*CardReward|PullRandomRelic" "$EVENT_DIR"/*Event.cs >/dev/null; then
	fail "event logic files contain duplicated effect/reward helpers"
fi

if rg -n "private static readonly .*Rules|MinimumGold|MinimumHp|MinimumMaxHp|CardsToRemove" "$EVENT_DIR/IntegratedStrategyEventSpawnRules.cs" >/dev/null; then
	fail "IntegratedStrategyEventSpawnRules.cs must stay a facade; put act and gate rules in partial files"
fi

if rg -n "public static (readonly )?Type\\[\\] (EventTypes|EventRelicTypes|EncounterTypes)\\s*(=|=>)" "$CATALOG_FILE" >/dev/null; then
	fail "IntegratedStrategyContentCatalog.cs must stay a facade; put category type lists in catalog partial files"
fi

if rg -n ": CustomEncounterModel|base\\(RoomType\\.Elite|IsValidForAct\\(" "$ENCOUNTER_DIR" --glob '!IntegratedStrategyEliteEncounter.cs' --glob '!IntegratedStrategyBossEncounter.cs' --glob '!IntegratedStrategyTwoSidedEliteEncounter.cs' >/dev/null; then
	fail "concrete event encounters must inherit IntegratedStrategyEliteEncounter instead of repeating elite encounter boilerplate"
fi

if rg -n "ModelDb\\.Monster<" "$ENCOUNTER_DIR" --glob '!IntegratedStrategyEliteEncounter.cs' --glob '!IntegratedStrategyBossEncounter.cs' --glob '!IntegratedStrategyTwoSidedEliteEncounter.cs' >/dev/null; then
	fail "concrete event encounters must use IntegratedStrategyEliteEncounter monster helpers"
fi

if rg -n "SceneHelper\\.GetScenePath\\(\"encounters/kaiser_crab_boss\"\\)|FullyCenterPlayers|public const string (LeftSlot|RightSlot) = \"(crusher|rocket)\"" "$ENCOUNTER_DIR" --glob '!IntegratedStrategyTwoSidedEliteEncounter.cs' >/dev/null; then
	fail "Kaiser Crab-style two-sided encounter boilerplate must stay in IntegratedStrategyTwoSidedEliteEncounter"
fi

if rg -n ": CustomSingletonModel|base\\(HookType\\.Combat\\)|BeforeCombatStart\\(|TryGetCombatState<" "$ENCOUNTER_DIR" --glob '*Hook.cs' --glob '!IntegratedStrategyEncounterHook.cs' >/dev/null; then
	fail "concrete encounter hooks must inherit IntegratedStrategyEncounterHook instead of repeating combat-hook boilerplate"
fi

if rg -n "AddModelToPool<EventRelicPool" "$ROOT/src/ModEntry.cs" >/dev/null; then
	fail "ModEntry must register event relics from IntegratedStrategyContentCatalog.EventRelicTypes"
fi

if rg -n "AfterObtained|BeforeCombatStart|AfterCardPlayed|AfterCombatEnd|FindStartingDeckCard|TryGetPlayedDeckCard|_lastPlayedDeckCard" "$RELIC_DIR/RhodesDoorRelic.cs" >/dev/null; then
	fail "RhodesDoorRelic.cs must stay a facade; put pickup and combat behavior in RhodesDoorRelic partial files"
fi

if rg -n "TargetAspectRatio|FallbackViewportSize|TextureRect|GetViewportSize|FitSixteenByNineInside|GetGlobalScale|Fitted event portrait|IntegratedStrategyEventPortraitDriver : Node" "$UI_DIR/IntegratedStrategyEventPortraitPatch.cs" >/dev/null; then
	fail "IntegratedStrategyEventPortraitPatch.cs must stay a Harmony entry; put fitting and driver behavior in portrait helper files"
fi

if rg -n "Log\\.Info|GlobalPosition|ApplyContentWidth|GetHorizontalMargins|GetTargetContentWidth|Adjusted event text group|TryGetMatching|HasAppliedPosition|LastAppliedGlobalPosition" "$UI_DIR/IntegratedStrategyEventLayoutPatch.cs" >/dev/null; then
	fail "IntegratedStrategyEventLayoutPatch.cs must stay a Harmony entry; put layout application behavior in IntegratedStrategyEventLayoutApplier"
fi

while IFS= read -r portrait_file; do
  [[ -f "$ASSET_DIR/images/events/$portrait_file" ]] \
    || fail "missing portrait asset: $ASSET_DIR/images/events/$portrait_file"
done < <(rg --no-filename -o 'ForEventPortrait[(][[:space:]]*"[a-z0-9_]+[.]png"' "$DEFINITION_DIR" \
  | sed -E 's/.*"([^"]+)"/\1/')

print "Event structure validation passed."
