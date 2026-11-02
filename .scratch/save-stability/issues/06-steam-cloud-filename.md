# 06: Steam Cloud filename

**What to build:** The shop file syncs through Steam Cloud like the game's own saves. On the dev machine, Steam's cloud list for LBoL contains only `*.sav` files and profile files, so the shop file is saved as `lvalonmimaShopSave.sav` in the same folder (its backup and corrupt copies follow the new name). If the `.sav` file is missing, the legacy `.txt` is read and migrated; the `.txt` is left in place, untouched, so downgrading or a failed first write loses nothing. Note the assumption: the cloud rule being `*.sav` at that root is inferred (research.md P1 and open questions), so the checklist must include confirming the new file actually appears in Steam's cloud list. See research.md section 5 item 7.

**Blocked by:** 05

**Status:** ready-for-agent

- [x] Saves go to `lvalonmimaShopSave.sav` (plus its `.bak`/`.corrupt-*`)
- [x] With only the legacy `.txt` present, it loads, migrates, and the next save writes `.sav`; the `.txt` is never modified or deleted
- [x] With both present, `.sav` wins and that is logged once
- [x] Compiles with the command in the README
- [x] In-game checklist appended, including checking `remotecache.vdf` (or a second machine) for the new file

The boxes above are met in code and by compiling. Whether Steam really syncs the new file still needs step 8 below.

**Load order.** Once any file of the `.sav` family (`.sav`, `.sav.tmp`, `.sav.bak`) exists, only that family is read, exactly as in ticket 05, and a legacy `.txt` next to it is logged once and ignored. Only when no `.sav` family file exists is the legacy family read, newest copy first: with `.txt` present, `.txt` → `.txt.bak` → `.txt.tmp` (the temp may be a torn write, so it comes last); with `.txt` missing, `.txt.tmp` → `.txt.bak` (an interrupted swap, as in ticket 05). The first readable copy is copied byte for byte to `.sav` and loaded. No legacy file is ever modified, renamed, deleted or moved to `.corrupt-*`, readable or not. If `.txt` exists and nothing legacy is readable, saving is blocked so that no fresh `.sav` hides the `.txt`; the next launch tries again. If only unreadable `.txt.tmp`/`.txt.bak` exist, the shop starts empty and saves go to `.sav`.

**Cloud rule.** On the dev machine `remotecache.vdf` (app 1140150) lists `history{0,1}.sav`, `save{0,1}.sav`, `game{0,1}.sav`, `sys.sav` and `Profiles/{Default,test}.json` under `AliothStudio/LBoL/<steamId>/`, and nothing else. That fits a `*.sav` rule at that root but does not prove it. `.sav.bak`, `.sav.tmp` and `.sav.corrupt-*` do not end in `.sav`, so they should stay local.

## In-game checklist

All files are in the save folder next to `save{0,1}.sav` (on macOS `~/Library/Application Support/AliothStudio/LBoL/<steamId>/`). Log lines start with `[ShopSave]` in `BepInEx/LogOutput.log`. Copy the whole save folder somewhere safe first and restore it at the end. For each step, record `ls -l lvalonmimaShopSave.*` and `shasum lvalonmimaShopSave.txt*` before launching, and compare afterwards: every legacy `.txt*` file must keep its size, mtime and hash in every step. To truncate a file, use the command in ticket 05's checklist.

1. **Only the legacy `.txt`.** Delete every `lvalonmimaShopSave.sav*` file. Keep `lvalonmimaShopSave.txt` (a 0.2.12 file is best), with no `.txt.bak`/`.txt.tmp`. Launch to the main menu.
   Expected: `Migrated lvalonmimaShopSave.txt from schema version 0 to 1` (for a 0.2.12 file), `Load (MainMenuPanel.Awake): copied legacy .../lvalonmimaShopSave.txt to .../lvalonmimaShopSave.sav; the legacy file is left in place.`, then `Load (MainMenuPanel.Awake): lvalonmimaShopSave.txt N bytes, ...`. No dialog, no errors. `cmp lvalonmimaShopSave.txt lvalonmimaShopSave.sav` reports no difference. Money, tiers, Challenger toggle, history labels and the quest backup are what the old build showed.
2. **Next save writes `.sav`.** Open the shop, buy a tier, refund it.
   Expected: `Save (...)` lines; `lvalonmimaShopSave.sav` changes, `lvalonmimaShopSave.sav.bak` appears, no `.sav.tmp` is left. The `.txt` hash and mtime are unchanged. Restart: the log now reads `Load (...): lvalonmimaShopSave.sav N bytes`, with no `copied legacy` line, and the shop is unchanged.
3. **Both present.** With the files from step 2 still in place, restart. Then start a run and abandon it (this loads again).
   Expected: exactly one `Both lvalonmimaShopSave.sav (...) and legacy lvalonmimaShopSave.txt (...) exist; lvalonmimaShopSave.sav is used ...` line per session, even after the second load. The shop shows the `.sav` contents. To prove the `.sav` wins, buy something so the two files differ, restart, and check the purchase is still there.
4. **Downgrade.** Install the 0.2.12 build (or any build before this ticket) and launch.
   Expected: it reads the untouched `.txt` and shows the shop as it was before step 1. Quit, reinstall this build: it uses the `.sav` again (step 3's line).
5. **Legacy leftovers of ticket 05 builds.** Delete all `.sav*` files.
   a. Keep only `lvalonmimaShopSave.txt.bak` (rename a good `.txt` copy to it; no `.txt`, no `.txt.tmp`). Launch.
   Expected: `copied legacy .../lvalonmimaShopSave.txt.bak to .../lvalonmimaShopSave.sav`, no dialog, the shop has that file's contents.
   b. Delete the `.sav*` files again. Keep a good `.txt.tmp` and an older good `.txt.bak` (no `.txt`). Launch.
   Expected: the copy is made from `.txt.tmp`.
   c. Delete the `.sav*` files again. Truncate `.txt` to half, and keep a good `.txt.bak`. Launch.
   Expected: `[Error] Could not read .../lvalonmimaShopSave.txt ...`, then `copied legacy .../lvalonmimaShopSave.txt.bak to ...sav`, `[Warning] Load (...) recovered from legacy ....txt.bak. Unreadable legacy copies left in place: lvalonmimaShopSave.txt`, and one "could not be read" dialog naming `lvalonmimaShopSave.txt`. No `.corrupt-*` file is made; the truncated `.txt` keeps its size, mtime and hash.
6. **Unreadable legacy `.txt`, nothing else.** Delete the `.sav*` files and the `.txt.bak`/`.txt.tmp`. Truncate `.txt` to half. Launch.
   Expected: one `Could not read` error, then `[Error] Load failed (...): no readable copy of lvalonmimaShopSave.txt was found and lvalonmimaShopSave.sav does not exist yet. ... saving is blocked ...`, and one dialog saying the shop will not be saved this session, naming `lvalonmimaShopSave.txt`. The shop is empty. Buy something: one `[Warning] Save skipped (...)`, and no `lvalonmimaShopSave.sav` is created. The `.txt` is unchanged and not renamed. Restore a good `.txt` and launch again: it loads as in step 1.
7. **Unreadable `.sav`, good legacy `.txt`.** With a good `.txt` and a `.sav` from step 2, truncate both `.sav` and `.sav.bak`. Launch.
   Expected: ticket 05's step 5 behaviour on the `.sav` files (`no readable copy of lvalonmimaShopSave.sav was found ... saving is blocked`, one dialog), plus the once-per-session `Both ... exist` line. The `.txt` is not read and not changed. (The legacy file can be used by hand: delete the `.sav*` files and relaunch.)
8. **Steam Cloud picks up the file** (this confirms the inferred `*.sav` rule). After step 2, quit the game and wait until Steam shows the cloud sync as finished. Then search for the file in the manifest:
   `grep lvalonmimaShopSave "$HOME/Library/Application Support/Steam/userdata/<accountId>/1140150/remotecache.vdf"`
   Expected: `AliothStudio/LBoL/<steamId>/lvalonmimaShopSave.sav` is listed, and no `.sav.bak`, `.sav.tmp`, `.sav.corrupt-*` or `.txt`. If possible, also check on a second machine or Steam Deck with no shop file: after the cloud download, `lvalonmimaShopSave.sav` is there and the shop matches. If the file is **not** listed, the cloud rule is not `*.sav` at that root, and this ticket's assumption is wrong: report it, and do not rely on cloud sync.
