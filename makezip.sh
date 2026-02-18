#!/bin/sh
SCRIPT_DIR="/Users/e/Library/Application Support/Steam/steamapps/common/LBoL/BepInEx/scripts"
PROJECT_DIR="/Users/e/Desktop/tachyon transmigration/projects/indev/lvalonmima"

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
