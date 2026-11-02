# 05: Safe writes, backup, schema version

**What to build:** A crash or force-quit during a save can no longer corrupt the shop file. Saves write to a temp file, flush it, then swap it in, keeping the previous good file as a `.bak` (with a delete-and-move fallback if replace isn't supported on Mono). If the main file fails to load, the backup is tried; the unreadable file is kept as `.corrupt-<timestamp>` rather than overwritten, the player sees an error dialog once (the way the game reports bad saves), and saving resumes from whatever was recovered. The file gains a top-level schema version: a file with no version is version 0 (0.2.12) and goes through an explicit migration step, so future field renames migrate instead of silently resetting. See research.md C5, C6, P6 and section 5 item 3.

**Blocked by:** 01

**Status:** done (awaiting in-game checklist)

- [x] Every write goes temp → flush → swap, leaving the previous file as `.bak`
- [x] A failed load tries `.bak`; the bad file is preserved as `.corrupt-<ts>`; a one-time dialog tells the player
- [x] After a recovered load, saving works again (the "load failed, skip every save" trap only applies when nothing could be recovered, and it's logged)
- [x] Schema version written on save; unversioned 0.2.12 files load through a version-0 migration with no data loss
- [x] Encryption and the YAML format are unchanged apart from the version field
- [x] Compiles with the command in the README
- [x] In-game checklist appended (load a 0.2.12 file; truncate the file and launch; delete both files and launch)

The boxes above are met in code and by compiling. Runtime behaviour, including the dialog, still needs the checklist below.

## In-game checklist

All files are in the save folder next to `save{0,1}.sav`. All log lines start with `[ShopSave]` in `BepInEx/LogOutput.log`. Before you start, copy the whole save folder somewhere safe; restore it after the last step. To truncate a file to half its size on macOS (no `truncate` there by default): `f=lvalonmimaShopSave.txt; head -c $(( $(stat -f%z "$f") / 2 )) "$f" > "$f.half" && mv "$f.half" "$f"`. To decrypt a file, use the openssl command in research.md section 4 step 6.

1. **Load a 0.2.12 file.** Put a file written by 0.2.12 in place as `lvalonmimaShopSave.txt`, with no `.bak` or `.tmp` next to it. Launch to the main menu.
   Expected: `Migrated lvalonmimaShopSave.txt from schema version 0 to 1`, then `Load (MainMenuPanel.Awake): lvalonmimaShopSave.txt N bytes, ...`. No errors and no dialog. Money, tiers, Challenger toggle, history labels and the quest backup match what 0.2.12 showed.
2. **First versioned save.** Open the shop (this saves), then close it.
   Expected: a `Save (OpenShopUI)` line, and a `lvalonmimaShopSave.txt.bak` that is byte-identical to the 0.2.12 file (`cmp`). No `.tmp` is left behind. The decrypted main file starts with `SchemaVersion: 1` and otherwise has the same keys and values as the decrypted 0.2.12 file. Restart the game: no `Migrated` line this time, and the shop is unchanged.
3. **Every save rotates the backup.** Buy a tier, then refund it.
   Expected: after each save, `.bak` is the previous main file (compare sizes/mtimes, or decrypt). No `File.Replace failed` warning; if there is one, the delete-and-move fallback is in use, and the saves should still succeed.
4. **Truncated main file, good backup.** Quit. Truncate `lvalonmimaShopSave.txt` to half. Launch.
   Expected: `[Error] Could not read .../lvalonmimaShopSave.txt (MainMenuPanel.Awake), N bytes: <exception>`, `[Warning] Kept unreadable ... as ...lvalonmimaShopSave.txt.corrupt-<yyyyMMdd-HHmmss>`, `[Warning] Load (MainMenuPanel.Awake) recovered from ....bak`. One error dialog: "The Roguelite Shop save could not be read." and the kept file name. The `.corrupt-...` file is the truncated one, and the main file now matches `.bak`. The shop shows the state from one save before the last.
   Then buy something and restart: the purchase is still there, there is no dialog and no error, and the `.corrupt-...` file is still present.
5. **Truncated main and backup.** Since ticket 06 the files are `lvalonmimaShopSave.sav` and `.sav.bak`; for this step have no legacy `lvalonmimaShopSave.txt*` in the folder (see ticket 06 step 7 for the case with one). Quit. Truncate both `lvalonmimaShopSave.sav` and `.sav.bak`. Launch.
   Expected: two `Could not read` errors, two `[Warning] Kept unreadable ... as ....corrupt-<yyyyMMdd-HHmmss>` lines (one for the main file, one for `.bak`), then `[Error] Load failed (MainMenuPanel.Awake): no readable copy of lvalonmimaShopSave.sav or legacy lvalonmimaShopSave.txt was found. The shop starts empty and saving resumes. Unreadable copies kept: ...`. One dialog: the shop started fresh, and the names the unreadable files were kept under. The shop is empty. Both `.corrupt-...` files are the truncated ones; no `.sav` or `.sav.bak` is left.
   Buy something: it saves normally (a `Save (...)` line, no `Save skipped`), and a new `lvalonmimaShopSave.sav` is written. Then start a run and abandon it, which returns to the main menu and loads again: the new file loads with no errors and there is no second dialog. Restore your copy afterwards.
6. **Main file missing, backup present** (an interrupted swap). Quit. Delete `lvalonmimaShopSave.txt` and any `.tmp`, but keep `.bak`. Launch.
   Expected: `[Warning] Load (...): ... is missing but a temp or backup copy exists ...`, then `recovered from ....bak`. No dialog. The main file is recreated from `.bak` and the shop shows its contents.
7. **Delete both files.** Quit. Delete `lvalonmimaShopSave.txt`, `.bak` and `.tmp`. Launch.
   Expected: `Load (MainMenuPanel.Awake): no file at ...`, no error, no dialog, and an empty shop. Open the shop: the main file is created (no `.bak` yet). Buy a tier: `.bak` appears.
8. **Chinese text.** Switch the game to 简体中文 and then 繁體中文, and repeat step 4 for each.
   Expected: the dialog is in that language, with no raw `Shop.SaveProblem...` key showing.
