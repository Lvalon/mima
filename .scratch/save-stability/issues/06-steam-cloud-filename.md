# 06: Steam Cloud filename

**What to build:** The shop file syncs through Steam Cloud like the game's own saves. On the dev machine, Steam's cloud list for LBoL contains only `*.sav` files and profile files, so the shop file is saved as `lvalonmimaShopSave.sav` in the same folder (its backup and corrupt copies follow the new name). If the `.sav` file is missing, the legacy `.txt` is read and migrated; the `.txt` is left in place, untouched, so downgrading or a failed first write loses nothing. Note the assumption: the cloud rule being `*.sav` at that root is inferred (research.md P1 and open questions), so the checklist must include confirming the new file actually appears in Steam's cloud list. See research.md section 5 item 7.

**Blocked by:** 05

**Status:** done (awaiting in-game checklist)

- [x] Saves go to `lvalonmimaShopSave.sav` (plus its `.bak`/`.corrupt-*`)
- [x] With only the legacy `.txt` present, it loads, migrates, and the next save writes `.sav`; the `.txt` is never modified or deleted
- [x] With both present, `.sav` wins and that is logged once
- [x] Compiles with the command in the README
- [x] In-game checklist appended, including checking `remotecache.vdf` (or a second machine) for the new file

The boxes above are met in code and by compiling. Whether Steam really syncs the new file still needs step 8 below.

**Load order.** Once any file of the `.sav` family (`.sav`, `.sav.tmp`, `.sav.bak`) exists, that family is read, exactly as in ticket 05, and a legacy `.txt` next to it is logged once (trace) and ignored. Two exceptions: when the `.txt` was modified after the `.sav` (an older build saved after this one, so the player downgraded and played), the `.txt` is loaded and copied to `.sav`, with a warning naming both times; and when no copy of the `.sav` family is readable, the legacy family is the last resort before an empty shop. Only when no `.sav` family file exists is the legacy family read, newest copy first: with `.txt` present, `.txt` → `.txt.bak` → `.txt.tmp` (the temp may be a torn write, so it comes last); with `.txt` missing, `.txt.tmp` → `.txt.bak` (an interrupted swap, as in ticket 05). The first readable copy is copied byte for byte to `.sav` and loaded. No legacy file is ever modified, renamed, deleted or moved to `.corrupt-*`, readable or not. If nothing legacy is readable, the shop starts empty, saves go to `.sav`, and the unreadable legacy files stay exactly as they are.

**Cloud rule.** On the dev machine `remotecache.vdf` (app 1140150) lists `history{0,1}.sav`, `save{0,1}.sav`, `game{0,1}.sav`, `sys.sav` and `Profiles/{Default,test}.json` under `AliothStudio/LBoL/<steamId>/`, and nothing else. That fits a `*.sav` rule at that root but does not prove it. `.sav.bak`, `.sav.tmp` and `.sav.corrupt-*` do not end in `.sav`, so they should stay local.

## In-game checklist

All files are in the save folder next to `save{0,1}.sav` (on macOS `~/Library/Application Support/AliothStudio/LBoL/<steamId>/`). Log lines start with `[ShopSave]` in `BepInEx/LogOutput.log`. Copy the whole save folder somewhere safe first and restore it at the end. For each step, record `ls -l lvalonmimaShopSave.*` and `shasum lvalonmimaShopSave.txt*` before launching, and compare afterwards: every legacy `.txt*` file must keep its size, mtime and hash in every step. To truncate a file, use the command in ticket 05's checklist.

1. **Only the legacy `.txt`.** Delete every `lvalonmimaShopSave.sav*` file. Keep `lvalonmimaShopSave.txt` (a 0.2.12 file is best), with no `.txt.bak`/`.txt.tmp`. Launch to the main menu.
   Expected: `Migrated lvalonmimaShopSave.txt from schema version 0 to 1` (for a 0.2.12 file), `Load (MainMenuPanel.Awake): copied legacy .../lvalonmimaShopSave.txt to .../lvalonmimaShopSave.sav; the legacy file is left in place.`, then `Load (MainMenuPanel.Awake): lvalonmimaShopSave.txt N bytes, ...`. No dialog, no errors. `cmp lvalonmimaShopSave.txt lvalonmimaShopSave.sav` reports no difference. Money, tiers, Challenger toggle, history labels and the quest backup are what the old build showed.
2. **Next save writes `.sav`.** Open the shop, buy a tier, refund it.
   Expected: `Save (...)` lines; `lvalonmimaShopSave.sav` changes, `lvalonmimaShopSave.sav.bak` appears, no `.sav.tmp` is left. The `.txt` hash and mtime are unchanged. Restart: the log now reads `Load (...): lvalonmimaShopSave.sav N bytes`, with no `copied legacy` line, and the shop is unchanged.
3. **Both present.** With the files from step 2 still in place, restart. Then start a run and abandon it (this loads again).
   Expected: exactly one `Both lvalonmimaShopSave.sav (...) and legacy lvalonmimaShopSave.txt (...) exist; lvalonmimaShopSave.sav is newer and used ...` trace line per session, even after the second load. The shop shows the `.sav` contents. To prove the `.sav` wins, buy something so the two files differ, restart, and check the purchase is still there.
4. **Downgrade.** Install the 0.2.12 build (or any build before this ticket) and launch.
   Expected: it reads the untouched `.txt` and shows the shop as it was before step 1. Buy or refund something so the old build writes the `.txt`. Quit, reinstall this build, launch.
   Expected: `[Warning] Load (MainMenuPanel.Awake): legacy lvalonmimaShopSave.txt (modified <time>) is newer than lvalonmimaShopSave.sav (modified <time>), so an older build of the mod saved after this one. ...`, then `copied legacy .../lvalonmimaShopSave.txt to .../lvalonmimaShopSave.sav`. The shop shows what the old build left, `cmp lvalonmimaShopSave.txt lvalonmimaShopSave.sav` reports no difference, and the previous `.sav` is now `.sav.bak`. Restart once more: no warning, step 3's line instead. (If the old build saved nothing, the `.sav` stays newer and is used, with step 3's line.)
5. **Legacy leftovers of ticket 05 builds.** Delete all `.sav*` files.
   a. Keep only `lvalonmimaShopSave.txt.bak` (rename a good `.txt` copy to it; no `.txt`, no `.txt.tmp`). Launch.
   Expected: `copied legacy .../lvalonmimaShopSave.txt.bak to .../lvalonmimaShopSave.sav`, no dialog, the shop has that file's contents.
   b. Delete the `.sav*` files again. Keep a good `.txt.tmp` and an older good `.txt.bak` (no `.txt`). Launch.
   Expected: the copy is made from `.txt.tmp`.
   c. Delete the `.sav*` files again. Truncate `.txt` to half, and keep a good `.txt.bak`. Launch.
   Expected: `[Error] Could not read .../lvalonmimaShopSave.txt ...`, then `copied legacy .../lvalonmimaShopSave.txt.bak to ...sav`, `[Warning] Load (...) recovered from legacy ....txt.bak. Unreadable legacy copies left in place: lvalonmimaShopSave.txt`, and one "could not be read" dialog naming `lvalonmimaShopSave.txt`. No `.corrupt-*` file is made; the truncated `.txt` keeps its size, mtime and hash.
6. **Unreadable legacy `.txt`, nothing else.** Delete the `.sav*` files and the `.txt.bak`/`.txt.tmp`. Truncate `.txt` to half. Launch.
   Expected: one `Could not read` error, then `[Error] Load failed (...): only unreadable copies of legacy lvalonmimaShopSave.txt were found and lvalonmimaShopSave.sav does not exist yet; saves go to lvalonmimaShopSave.sav and the legacy files are left in place. The shop starts empty and saving resumes. ...`, and one dialog saying the shop started fresh and the unreadable file was kept as `lvalonmimaShopSave.txt`. The shop is empty. Buy something: it saves normally (no `Save skipped`) and `lvalonmimaShopSave.sav` is created. The `.txt` is unchanged and not renamed. Restart: the `.sav` loads, no dialog. Then copy a good `.txt` back in place with plain `cp` (not `cp -p`, so it is newer than the `.sav`) and launch: it is loaded and copied to `.sav` as in step 4's second half.
7. **Unreadable `.sav`, good legacy `.txt`.** With a good `.txt` and a `.sav` from step 2, truncate both `.sav` and `.sav.bak`. Launch.
   Expected: two `Could not read` errors and two `Kept unreadable ... as ....corrupt-<ts>` warnings (the `.sav` and `.sav.bak`), then `[Error] Load (...): no readable copy of lvalonmimaShopSave.sav was found, so the shop was restored from legacy .../lvalonmimaShopSave.txt, which may be missing recent progress. ...`. One dialog saying the shop was restored from the older `lvalonmimaShopSave.txt` and naming the `.corrupt-...` files. The shop shows the `.txt` contents, `cmp lvalonmimaShopSave.txt lvalonmimaShopSave.sav` reports no difference, and saving works. The `.txt` keeps its size, mtime and hash.
8. **Steam Cloud picks up the file** (this confirms the inferred `*.sav` rule). After step 2, quit the game and wait until Steam shows the cloud sync as finished. Then search for the file in the manifest:
   `grep lvalonmimaShopSave "$HOME/Library/Application Support/Steam/userdata/<accountId>/1140150/remotecache.vdf"`
   Expected: `AliothStudio/LBoL/<steamId>/lvalonmimaShopSave.sav` is listed, and no `.sav.bak`, `.sav.tmp`, `.sav.corrupt-*` or `.txt`. If possible, also check on a second machine or Steam Deck with no shop file: after the cloud download, `lvalonmimaShopSave.sav` is there and the shop matches. If the file is **not** listed, the cloud rule is not `*.sav` at that root, and this ticket's assumption is wrong: report it, and do not rely on cloud sync.
