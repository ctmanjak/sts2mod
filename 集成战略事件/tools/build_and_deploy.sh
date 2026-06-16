#!/bin/zsh
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
FILE_STEM="IntegratedStrategyEvents"
MANIFEST_SRC="$ROOT/assets/$FILE_STEM.json"
GAME_APP="${STS2_GAME_APP:-/Users/iniad/Library/Application Support/Steam/steamapps/common/Slay the Spire 2/SlayTheSpire2.app}"
GAME_BIN="$GAME_APP/Contents/MacOS/Slay the Spire 2"
MOD_DIR="$GAME_APP/Contents/MacOS/mods/$FILE_STEM"
BASELIB_SRC="$GAME_APP/Contents/MacOS/BaseLib"
BASELIB_MOD_DIR="$GAME_APP/Contents/MacOS/mods/BaseLib"
BASELIB_RELEASE_ZIP="${BASELIB_RELEASE_ZIP:-/Users/iniad/Downloads/BaseLib.3.2.1.zip}"
BASELIB_REQUIRED_VERSION="${BASELIB_REQUIRED_VERSION:-v3.2.1}"
BUILD_OUT="$ROOT/src/bin/Release/net9.0"
PROJECT_PATH="$ROOT/src/$FILE_STEM.csproj"
IMPORT_PROJECT="$ROOT/.build/import_project"
DEFAULT_GODOT_EDITOR="$ROOT/../.tools/godot-4.5.1/Godot_mono.app/Contents/MacOS/Godot"
if [[ -z "${GODOT_EDITOR:-}" && -x "$DEFAULT_GODOT_EDITOR" ]]; then
  GODOT_EDITOR="$DEFAULT_GODOT_EDITOR"
else
  GODOT_EDITOR="${GODOT_EDITOR:-/opt/homebrew/bin/godot}"
fi

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

major_minor_version() {
  sed -E 's/^([0-9]+[.][0-9]+).*/\1/' <<< "$1"
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

    local manifest_path
    manifest_path="$(find "$tmp_dir" -type f -name BaseLib.json -print -quit)"
    if [[ -z "$manifest_path" ]]; then
      print -u2 "Could not locate BaseLib files in $BASELIB_RELEASE_ZIP."
      rm -rf "$tmp_dir"
      exit 1
    fi

    local source_dir
    source_dir="$(dirname "$manifest_path")"
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

if [[ ! -x "$GODOT_EDITOR" ]]; then
  print -u2 "Could not find a usable Godot editor executable at $GODOT_EDITOR."
  exit 1
fi

GAME_GODOT_VERSION="$("$GAME_BIN" --version 2>/dev/null | head -n 1)"
IMPORT_GODOT_VERSION="$("$GODOT_EDITOR" --version 2>/dev/null | head -n 1)"
if [[ -n "$GAME_GODOT_VERSION" && -n "$IMPORT_GODOT_VERSION" \
  && "$(major_minor_version "$GAME_GODOT_VERSION")" != "$(major_minor_version "$IMPORT_GODOT_VERSION")" ]]; then
  print -u2 "Warning: asset import Godot version ($IMPORT_GODOT_VERSION) differs from game runtime ($GAME_GODOT_VERSION)."
  print -u2 "Set GODOT_EDITOR to a matching 4.5.x editor if texture compatibility regresses."
fi

rm -rf "$ROOT/src/bin" "$ROOT/src/obj" "$ROOT/dist" "$ROOT/.build"

ensure_baselib

DOTNET_ROOT="$DOTNET_ROOT" "$DOTNET_BIN" build "$PROJECT_PATH" -c Release

mkdir -p "$ROOT/dist"
rm -rf "$MOD_DIR"

mkdir -p "$IMPORT_PROJECT/$FILE_STEM"
cp "$ROOT/tools/project.godot" "$IMPORT_PROJECT/project.godot"
rsync -a --exclude "$FILE_STEM.json" "$ROOT/assets/" "$IMPORT_PROJECT/$FILE_STEM/"
clean_macos_metadata "$IMPORT_PROJECT"

"$GODOT_EDITOR" --headless \
  --path "$IMPORT_PROJECT" \
  --import

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
    sts2.dll|GodotSharp.dll|BaseLib.dll|0Harmony.dll)
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

if [[ "${KEEP_BUILD_ARTIFACTS:-0}" != "1" ]]; then
  rm -rf "$ROOT/.build"
fi

echo "Deployed to $MOD_DIR"
