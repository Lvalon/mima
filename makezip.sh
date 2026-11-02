#!/bin/sh
# Post-build export: installs the mod as a local mod in the game's Mods folder, where the game's mod
# loader finds it (and uploads it to the workshop from), and zips the same files next to it.
# $1 is the DLL to ship: the build passes its own output ($(TargetPath)), so Debug and Release both
# export what they just built. Run by hand without it, the Debug build is shipped.
MOD_NAME="lvalonmima"
PROJECT_DIR="$(cd "$(dirname "$0")" && pwd)"
GAME_DIR="/Users/e/Library/Application Support/Steam/steamapps/common/LBoL"
MODS_DIR="$GAME_DIR/Mods"
WORKSHOP_ROOT="/Users/e/Library/Application Support/Steam/steamapps/workshop/content/1140150"
DLL="${1:-$PROJECT_DIR/bin/Debug/netstandard2.1/$MOD_NAME.dll}"
MODINFO="$PROJECT_DIR/modinfo.json"

[ -f "$DLL" ] || { echo "makezip.sh: $DLL not found, not exporting"; exit 1; }
[ -f "$MODINFO" ] || { echo "makezip.sh: $MODINFO not found, not exporting"; exit 1; }
GUID=$(jq -r '.GUID // empty' "$MODINFO")
PUBLISHED_FILE_ID=$(jq -r '.PublishedFileId // empty' "$MODINFO")

# No PublishedFileId yet: look for this mod (same GUID) among the downloaded workshop items, so
# that uploading updates it instead of creating a new item. Several (duplicate uploads): the
# oldest (lowest id), with a warning.
if [ -z "$PUBLISHED_FILE_ID" ]; then
	FOUND=""
	FOUND_COUNT=0
	if [ -n "$GUID" ]; then
		for WORKSHOP_ITEM in "$WORKSHOP_ROOT"/*/modinfo.json; do
			[ -f "$WORKSHOP_ITEM" ] || continue
			if [ "$(jq -r '.GUID // empty' "$WORKSHOP_ITEM" 2>/dev/null)" = "$GUID" ]; then
				FOUND="$FOUND
$(basename "$(dirname "$WORKSHOP_ITEM")")"
				FOUND_COUNT=$((FOUND_COUNT + 1))
			fi
		done
	fi
	if [ "$FOUND_COUNT" -gt 0 ]; then
		PUBLISHED_FILE_ID=$(printf '%s\n' "$FOUND" | sed '/^$/d' | sort -n | head -n 1)
		if [ "$FOUND_COUNT" -gt 1 ]; then
			echo "WARNING: several workshop items have GUID $GUID: $(printf '%s' "$FOUND" | tr '\n' ' '); using the oldest, $PUBLISHED_FILE_ID"
		fi
		jq --argjson id "$PUBLISHED_FILE_ID" '.PublishedFileId = $id' "$MODINFO" > "$MODINFO.tmp" && mv "$MODINFO.tmp" "$MODINFO"
		echo "Set PublishedFileId to $PUBLISHED_FILE_ID from the workshop item with GUID $GUID"
	else
		echo "WARNING: modinfo.json has no PublishedFileId and no workshop item has GUID $GUID: uploading will create a NEW workshop item"
	fi
fi

WORKSHOP_MODINFO="$WORKSHOP_ROOT/$PUBLISHED_FILE_ID/modinfo.json"

if [ -n "$PUBLISHED_FILE_ID" ] && [ -f "$WORKSHOP_MODINFO" ]; then
	WORKSHOP_LAST_UPDATE=$(jq -r '.LastUpdateTime' "$WORKSHOP_MODINFO")
	if [ -n "$WORKSHOP_LAST_UPDATE" ] && [ "$WORKSHOP_LAST_UPDATE" != "null" ]; then
		jq --argjson t "$WORKSHOP_LAST_UPDATE" '.LastUpdateTime = $t' "$MODINFO" > "$MODINFO.tmp" && mv "$MODINFO.tmp" "$MODINFO"
		echo "Updated LastUpdateTime to $WORKSHOP_LAST_UPDATE from workshop copy"
	fi
else
	echo "No workshop copy found at $WORKSHOP_MODINFO, keeping existing LastUpdateTime"
fi

# Where to install: the Mods folder already holding this mod (same GUID) under whatever name it
# was installed as, else Mods/$MOD_NAME. Two folders with the same GUID would load the mod twice.
DEST="$MODS_DIR/$MOD_NAME"
if [ -n "$GUID" ] && [ ! -f "$DEST/modinfo.json" ]; then
	for INSTALLED in "$MODS_DIR"/*/modinfo.json; do
		[ -f "$INSTALLED" ] || continue
		if [ "$(jq -r '.GUID // empty' "$INSTALLED" 2>/dev/null)" = "$GUID" ]; then
			DEST="$(dirname "$INSTALLED")"
			break
		fi
	done
fi
for INSTALLED in "$MODS_DIR"/*/modinfo.json; do
	[ -f "$INSTALLED" ] && [ "$(dirname "$INSTALLED")" != "$DEST" ] || continue
	if [ -n "$GUID" ] && [ "$(jq -r '.GUID // empty' "$INSTALLED" 2>/dev/null)" = "$GUID" ]; then
		echo "WARNING: $(dirname "$INSTALLED") also has GUID $GUID: the mod will load twice until it is removed"
	fi
done

# Assemble the package away from the game, so a failed copy never leaves a half-installed mod.
STAGE=$(mktemp -d) || exit 1
trap 'rm -rf "$STAGE"' EXIT
PKG="$STAGE/$MOD_NAME"
mkdir "$PKG"

# The text and the dll: a package without them is broken, so stop rather than install it.
cp -R -a "$PROJECT_DIR/DirResources/." "$PKG/" || { echo "Cannot copy DirResources, not exporting"; exit 1; }
cp -a "$DLL" "$PKG/" || { echo "Cannot copy $DLL, not exporting"; exit 1; }
cp -a "$MODINFO" "$PKG/" || { echo "Cannot copy modinfo.json, not exporting"; exit 1; }
for FILE in CHANGELOG.md CREDITS.md icon.png manifest.json README.md; do
	[ -f "$PROJECT_DIR/$FILE" ] && cp -a "$PROJECT_DIR/$FILE" "$PKG/"
done
find "$PKG" \( -name Thumbs.db -o -name .DS_Store \) -exec rm -f {} +

mkdir -p "$MODS_DIR" || exit 1
rm -rf "$DEST"
mv "$PKG" "$DEST" || { echo "Cannot install to $DEST"; exit 1; }
echo "Installed $MOD_NAME to $DEST"

# The zip sits next to the installed folder, under the same name.
ZIP_TARGET="$DEST.zip"
# A fresh zip: zip only updates an existing one, which keeps entries of earlier builds.
rm -f "$ZIP_TARGET"
zip -q -r -j "$ZIP_TARGET" "$DEST"/*
ZIP_STATUS=$?
if [ $ZIP_STATUS -eq 0 ]; then
	echo "Wrote zip to $ZIP_TARGET"
fi

exit $ZIP_STATUS
