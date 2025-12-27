#!/bin/sh
cd /Users/e/Library/Application\ Support/Steam/steamapps/common/LBoL/BepInEx/scripts
mkdir lvalonmima
rm -r lvalonmima/
cp -R -a /Users/e/Desktop/tachyon\ transmigration/projects/indev/lvalonmima/DIRRESOURCES/. lvalonmima/
cp -a /Users/e/Desktop/tachyon\ transmigration/projects/indev/lvalonmima/bin/Debug/netstandard2.1/lvalonmima.dll lvalonmima/
cp -a /Users/e/Desktop/tachyon\ transmigration/projects/indev/lvalonmima/CHANGELOG.md lvalonmima/
cp -a /Users/e/Desktop/tachyon\ transmigration/projects/indev/lvalonmima/CREDITS.md lvalonmima/
cp -a /Users/e/Desktop/tachyon\ transmigration/projects/indev/lvalonmima/icon.png lvalonmima/
cp -a /Users/e/Desktop/tachyon\ transmigration/projects/indev/lvalonmima/manifest.json lvalonmima/
cp -a /Users/e/Desktop/tachyon\ transmigration/projects/indev/lvalonmima/README.md lvalonmima/
rm -fr lvalonmima/Thumbs.db
zip -r -j lvalonmima.zip lvalonmima/*
