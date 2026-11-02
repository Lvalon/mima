# 01: Make save failures visible

**What to build:** When the shop file fails to load or restore, or a save is skipped because an earlier load failed, the BepInEx log says so, with the reason, file size and exception. Every load and save writes one trace line saying why it ran, which profile key it used, the active run profile key, money, total tiers and Challenger state. When a lookup creates a brand-new empty shop for a profile, that is logged too, with the key. Profile changes (rename, select, create, delete) log the old and new key and whether the old key had a shop. This is the prefactor for the other tickets: saving takes a `reason` the way loading already takes a location, at every call site. See research.md C5 and section 4 steps 1–3.

**Blocked by:** None (can start immediately)

**Status:** ready-for-agent

- [x] Load and restore failures log an error with reason, file length and exception; nothing is swallowed silently
- [x] A save blocked by an earlier failed load logs a warning once per session
- [x] Every save call site passes a short reason string; every load and save writes one trace line (behind a single on/off toggle)
- [x] Creating an empty shop for a key that wasn't there is logged with the key
- [x] Profile rename/select/create/delete log old key, new key, and whether the old key exists in the save
- [x] No change to what is saved or loaded; the file format is untouched (checked by reading the diff; confirm with checklist step 7)
- [x] Compiles with the command in the README

## In-game checklist

All log lines start with `[ShopSave]` and go to `BepInEx/LogOutput.log`. Trace lines are `Info` and are switched off by setting `ShopSaveLoader.TraceEnabled = false`; errors and warnings are logged either way. Back up `lvalonmimaShopSave.txt` before step 4.

1. **Normal launch.** Start the game to the main menu.
   Expected: `Load (MainMenuPanel.Awake): file N bytes, K keys on disk. key=<no profile>, ...` (the profile is selected after the menu loads), then `SelectProfile(slot X): old key <none> (in save: False) -> new key profile:... (in save: True)`. No errors.
2. **Menu saves.** Open the Roguelite Shop, buy a tier, refund it, toggle Challenger inside the shop, close it, then shift-click the menu button to toggle Challenger from the menu.
   Expected: one `Save (<reason>): key=..., activeRunKey=<none>, money=..., tiers=..., challenger=..., keysInSave=K, file A -> B bytes (wrote B)` line for each, with reasons `OpenShopUI`, `ShopPanel.Purchase`, `ShopPanel.Refund`, `ShopPanel.ChallengerToggle`, `ToggleChallengerModeFromMenu`. Money, tiers and challenger match what the shop shows.
3. **Run saves.** Start a run with Challenger on, enter a couple of nodes, win a battle, Save & Quit, Continue, use Restart station, then finish or abandon the run.
   Expected: `Save` lines with reasons `GameResultPanel.CustomLocalizationAsync` (at run start, C2), `GameRunController.Save`, `StationEntered`, `PersistQuestProgress`, and at the end `EndGameStatistics` and `AppendGameRunHistory` (or `GameMaster.CoAbandonGameRun`). `Load (GameRunController.Restore)` lines on Continue and Restart station. During the run `activeRunKey` equals `key`.
4. **Corrupt file.** Quit, truncate `lvalonmimaShopSave.txt` to about half its size (`truncate -s 50% lvalonmimaShopSave.txt`, or a hex editor), launch.
   Expected: `[Error] [ShopSave] Load failed (MainMenuPanel.Awake), file N bytes; saving is blocked until a later load succeeds: <exception>` (padding or YAML error). Open the shop and buy something: the shop shows 0 money and tier 0 (the known C5 symptom), a `Created new empty shop` warning appears for the current key, and exactly one `[Warning] [ShopSave] Save skipped (OpenShopUI): an earlier load of lvalonmimaShopSave.txt failed ... (Logged once per session.)`; every later save logs only the Info line `Save skipped (<reason>): earlier load failed. ...`. The truncated file's size does not change. Restore the backup afterwards.
5. **Rename (C1).** Rename the current profile in the profile menu, then open the shop.
   Expected: `SetProfileName(slot X): old key profile:<ts>_<old> (in save: True) -> new key profile:<ts>_<new> (in save: False)`, then `[Warning] [ShopSave] Created new empty shop for key profile:<ts>_<new> (current profile); it was not in the save. Other keys: [profile:<ts>_<old>, ...]`. The shop showing empty here is the known bug that ticket 02 fixes. Rename the profile back to get the data back.
6. **Create / select / delete.** Create a profile in an empty slot, switch between profiles, delete the new one.
   Expected: `CreateAndSelectProfile(slot Y): old key <none> (in save: False) -> new key profile:... (in save: False)` (plus the nested `SelectProfile` line), one `SelectProfile` line per switch with old and new keys, and `DeleteProfile(slot Y): old key profile:... (in save: ...) -> new key ...`. The first shop lookup for the new profile logs `Created new empty shop for key ...`.
7. **No data change.** After all of the above, with the backup restored, money, tiers, Challenger toggle and history labels are as before; the decrypted file (see research.md section 4 step 6) has the same keys and fields as before this build.
