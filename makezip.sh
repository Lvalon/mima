#!/bin/sh
SCRIPT_DIR="/Users/e/Library/Application Support/Steam/steamapps/common/LBoL/BepInEx/plugins"
PROJECT_DIR="/Users/e/Desktop/tachyon transmigration/projects/indev/lvalonmima"
WORKSHOP_ROOT="/Users/e/Library/Application Support/Steam/steamapps/workshop/content/1140150"

MODINFO="$PROJECT_DIR/modinfo.json"
PUBLISHED_FILE_ID=$(jq -r '.PublishedFileId' "$MODINFO")
WORKSHOP_MODINFO="$WORKSHOP_ROOT/$PUBLISHED_FILE_ID/modinfo.json"

if [ -f "$WORKSHOP_MODINFO" ]; then
	WORKSHOP_LAST_UPDATE=$(jq -r '.LastUpdateTime' "$WORKSHOP_MODINFO")
	if [ -n "$WORKSHOP_LAST_UPDATE" ] && [ "$WORKSHOP_LAST_UPDATE" != "null" ]; then
		jq --argjson t "$WORKSHOP_LAST_UPDATE" '.LastUpdateTime = $t' "$MODINFO" > "$MODINFO.tmp" && mv "$MODINFO.tmp" "$MODINFO"
		echo "Updated LastUpdateTime to $WORKSHOP_LAST_UPDATE from workshop copy"
	fi
else
	echo "No workshop copy found at $WORKSHOP_MODINFO, keeping existing LastUpdateTime"
fi

cd "$SCRIPT_DIR" || { echo "Cannot cd to $SCRIPT_DIR"; exit 1; }

rm -rf "lvalonmima"
mkdir -p "lvalonmima"

cp -R -a "$PROJECT_DIR/DIRRESOURCES/." "lvalonmima/" || true
cp -a "$PROJECT_DIR/bin/Debug/netstandard2.1/lvalonmima.dll" "lvalonmima/" || true
cp -a "$PROJECT_DIR/CHANGELOG.md" "lvalonmima/" || true
cp -a "$PROJECT_DIR/CREDITS.md" "lvalonmima/" || true
cp -a "$PROJECT_DIR/icon.png" "lvalonmima/" || true
cp -a "$PROJECT_DIR/manifest.json" "lvalonmima/" || true
cp -a "$PROJECT_DIR/README.md" "lvalonmima/" || true
cp -a "$PROJECT_DIR/modinfo.json" "lvalonmima/" || true

rm -fr "lvalonmima/Thumbs.db" || true

rm -f "$PROJECT_DIR/lvalonmima.zip" || true

ZIP_TARGET="$SCRIPT_DIR/lvalonmima.zip"
zip -r -j "$ZIP_TARGET" "lvalonmima"/*
ZIP_STATUS=$?
if [ $ZIP_STATUS -eq 0 ]; then
	echo "Wrote zip to $ZIP_TARGET"
fi

exit $ZIP_STATUS

zip -r -j "$PROJECT_DIR/lvalonmima.zip" "lvalonmima"/*

exit $?
