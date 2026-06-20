#!/bin/zsh
set -euo pipefail

SCRIPT_PATH="${0:A}"
SCRIPT_DIR="${SCRIPT_PATH:h}"
ROOT="${SCRIPT_DIR:h}"
FILE_STEM="Illaoi"
MANIFEST_SRC="$ROOT/assets/$FILE_STEM.json"
GAME_APP="${STS2_GAME_APP:-/Users/iniad/Library/Application Support/Steam/steamapps/common/Slay the Spire 2/SlayTheSpire2.app}"
GAME_BIN="$GAME_APP/Contents/MacOS/Slay the Spire 2"
MOD_DIR="$GAME_APP/Contents/MacOS/mods/$FILE_STEM"
BASELIB_SRC="$GAME_APP/Contents/MacOS/BaseLib"
BASELIB_MOD_DIR="$GAME_APP/Contents/MacOS/mods/BaseLib"
BASELIB_RELEASE_ZIP="${BASELIB_RELEASE_ZIP:-/Users/iniad/Downloads/BaseLib.3.3.0.zip}"
BASELIB_REQUIRED_VERSION="${BASELIB_REQUIRED_VERSION:-v3.3.0}"
BUILD_OUT="$ROOT/src/bin/Release/net9.0"
PROJECT_PATH="$ROOT/src/$FILE_STEM.csproj"
IMPORT_PROJECT="$ROOT/.build/import_project"

if (( ${+commands[dotnet]} )); then
  DOTNET_BIN="${commands[dotnet]}"
elif [[ -x "/opt/homebrew/Cellar/dotnet/9.0.8/libexec/dotnet" ]]; then
  DOTNET_BIN="/opt/homebrew/Cellar/dotnet/9.0.8/libexec/dotnet"
else
  print -u2 "Could not find a usable dotnet executable. Install .NET 9 SDK or add dotnet to PATH."
  exit 1
fi

DOTNET_ROOT="$(cd "$(dirname "$DOTNET_BIN")" && pwd)"

clean_macos_metadata() {
  local target="$1"
  [[ -d "$target" ]] || return 0

  find "$target" -name "__MACOSX" -type d -prune -exec rm -rf {} +
  find "$target" -name ".DS_Store" -type f -delete
  find "$target" -name "._*" -type f -delete
}

baselib_has_required_version() {
  local manifest="$1"
  [[ -f "$manifest" ]] || return 1
  grep -q "\"version\"[[:space:]]*:[[:space:]]*\"$BASELIB_REQUIRED_VERSION\"" "$manifest"
}

install_baselib_from_dir() {
  local source_dir="$1"

  rm -rf "$BASELIB_MOD_DIR"
  mkdir -p "$BASELIB_MOD_DIR"
  cp "$source_dir"/BaseLib.json "$BASELIB_MOD_DIR/BaseLib.json"
  cp "$source_dir"/BaseLib.pck "$BASELIB_MOD_DIR/BaseLib.pck"
  cp "$source_dir"/BaseLib.dll "$BASELIB_MOD_DIR/BaseLib.dll"
  clean_macos_metadata "$BASELIB_MOD_DIR"
}

ensure_baselib() {
  if baselib_has_required_version "$BASELIB_MOD_DIR/BaseLib.json"; then
    return 0
  fi

  if [[ -d "$BASELIB_SRC" ]] && baselib_has_required_version "$BASELIB_SRC/BaseLib.json"; then
    install_baselib_from_dir "$BASELIB_SRC"
    return 0
  fi

  if [[ -f "$BASELIB_RELEASE_ZIP" ]]; then
    local tmp_dir
    tmp_dir="$(mktemp -d)"
    unzip -q "$BASELIB_RELEASE_ZIP" -d "$tmp_dir"

    local source_dir="$tmp_dir/BaseLib.${BASELIB_REQUIRED_VERSION#v}"
    if [[ ! -d "$source_dir" ]]; then
      local manifest_path
      manifest_path="$(find "$tmp_dir" -type f -name BaseLib.json -print -quit)"
      if [[ -n "$manifest_path" ]]; then
        source_dir="$(dirname "$manifest_path")"
      fi
    fi

    if [[ ! -d "$source_dir" ]] || [[ ! -f "$source_dir/BaseLib.json" ]]; then
      print -u2 "Could not locate BaseLib files in $BASELIB_RELEASE_ZIP."
      rm -rf "$tmp_dir"
      exit 1
    fi

    if ! baselib_has_required_version "$source_dir/BaseLib.json"; then
      print -u2 "$BASELIB_RELEASE_ZIP is not BaseLib $BASELIB_REQUIRED_VERSION."
      rm -rf "$tmp_dir"
      exit 1
    fi

    install_baselib_from_dir "$source_dir"
    rm -rf "$tmp_dir"
    return 0
  fi

  print -u2 "BaseLib $BASELIB_REQUIRED_VERSION is required. Put BaseLib.${BASELIB_REQUIRED_VERSION#v}.zip at $BASELIB_RELEASE_ZIP or set BASELIB_RELEASE_ZIP."
  exit 1
}

select_godot_import_bin() {
  local default_editor="$ROOT/../.tools/godot-4.5.1/Godot_mono.app/Contents/MacOS/Godot"

  if [[ -n "${GODOT_IMPORT_BIN:-}" ]]; then
    :
  elif [[ -n "${GODOT_EDITOR:-}" ]]; then
    GODOT_IMPORT_BIN="$GODOT_EDITOR"
  elif [[ -x "$default_editor" ]]; then
    GODOT_IMPORT_BIN="$default_editor"
  elif [[ -x "/opt/homebrew/bin/godot" ]]; then
    GODOT_IMPORT_BIN="/opt/homebrew/bin/godot"
  elif (( ${+commands[godot]} )); then
    GODOT_IMPORT_BIN="${commands[godot]}"
  elif [[ -x "/Applications/Godot.app/Contents/MacOS/Godot" ]]; then
    GODOT_IMPORT_BIN="/Applications/Godot.app/Contents/MacOS/Godot"
  else
    print -u2 "Could not find a Godot editor executable for asset import."
    exit 1
  fi

  if [[ ! -x "$GODOT_IMPORT_BIN" ]]; then
    print -u2 "Godot editor executable is not runnable: $GODOT_IMPORT_BIN"
    exit 1
  fi
}

first_line() {
  sed -n '1p'
}

godot_major_minor() {
  local version="$1"
  print -r -- "$version" | sed -E -n 's/^[^0-9]*([0-9]+)\.([0-9]+).*/\1.\2/p' | first_line
}

require_matching_godot_versions() {
  local game_version import_version game_major_minor import_major_minor

  game_version="$("$GAME_BIN" --version 2>&1 || true)"
  import_version="$("$GODOT_IMPORT_BIN" --version 2>&1 || true)"
  game_version="$(print -r -- "$game_version" | first_line)"
  import_version="$(print -r -- "$import_version" | first_line)"
  game_major_minor="$(godot_major_minor "$game_version")"
  import_major_minor="$(godot_major_minor "$import_version")"

  print "Game Godot version: ${game_version:-<unknown>}"
  print "Import Godot version: ${import_version:-<unknown>}"

  if [[ -z "$game_major_minor" ]] || [[ -z "$import_major_minor" ]]; then
    print -u2 "Could not parse Godot major.minor versions. Refusing to import assets."
    exit 1
  fi

  if [[ "$game_major_minor" != "$import_major_minor" ]]; then
    print -u2 "Godot version mismatch: game uses $game_major_minor but asset importer uses $import_major_minor."
    print -u2 "Set GODOT_IMPORT_BIN or GODOT_EDITOR to a Godot editor matching the game's major.minor version."
    exit 1
  fi
}

select_godot_import_bin
require_matching_godot_versions

rm -rf "$ROOT/src/bin" "$ROOT/src/obj" "$ROOT/dist" "$ROOT/.build"

ensure_baselib

DOTNET_ROOT="$DOTNET_ROOT" "$DOTNET_BIN" build "$PROJECT_PATH" -c Release

mkdir -p "$ROOT/dist"
rm -rf "$MOD_DIR"

mkdir -p "$IMPORT_PROJECT/$FILE_STEM"
cp "$ROOT/tools/project.godot" "$IMPORT_PROJECT/project.godot"
rsync -a --exclude "$FILE_STEM.json" "$ROOT/assets/" "$IMPORT_PROJECT/$FILE_STEM/"
clean_macos_metadata "$IMPORT_PROJECT"

"$GODOT_IMPORT_BIN" --headless --path "$IMPORT_PROJECT" --import

cp "$MANIFEST_SRC" "$ROOT/dist/$FILE_STEM.json"

"$GAME_BIN" --headless \
  --path "$ROOT/tools" \
  -s res://pack_mod.gd -- \
  "$MANIFEST_SRC" \
  "$ROOT/dist/$FILE_STEM.pck" \
  "$IMPORT_PROJECT"

for dll in "$BUILD_OUT"/*.dll; do
  base_name="$(basename "$dll")"
  case "$base_name" in
    sts2.dll|GodotSharp.dll|BaseLib.dll|0Harmony.dll|SmartFormat.dll)
      continue
      ;;
  esac

	cp "$dll" "$ROOT/dist/$base_name"
done
clean_macos_metadata "$ROOT/dist"

mkdir -p "$MOD_DIR"
cp "$ROOT/dist/$FILE_STEM.json" "$MOD_DIR/$FILE_STEM.json"
cp "$ROOT/dist/$FILE_STEM.pck" "$MOD_DIR/$FILE_STEM.pck"

for dll in "$ROOT/dist"/*.dll; do
	cp "$dll" "$MOD_DIR/$(basename "$dll")"
done
clean_macos_metadata "$MOD_DIR"

echo "Deployed to $MOD_DIR"
