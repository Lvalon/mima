# 07: Restore cleanup

**What to build:** A run restore that throws (for example on a corrupt game save) no longer leaves the Quest Board stuck un-hydrated in every later run. The "restore in progress" flag and the deferred-hydration list are cleared whatever happens (a Harmony finalizer, not only the postfix), and deferred exhibits that belong to a run other than the current one are dropped instead of being synced into the profile's quest backup. The restore-snapshot branch that nothing can reach, and the pending-hydration flag that is set but never read, are removed. See research.md C8, C10 and section 5 item 6.

**Blocked by:** 01

**Status:** ready-for-agent

- [x] Restore bookkeeping is cleared even when the game's restore throws, and the exception is logged
- [x] Deferred exhibits from a dead run are discarded, not synced
- [x] The unreachable restore-snapshot branch and its unused flag are gone, with no behaviour change on the paths that do run
- [x] Compiles with the command in the README
- [x] In-game checklist appended (corrupt `game{i}.sav`, Continue → error dialog, start a new run with the Quest Board: it hydrates)

## In-game checklist

Run with Solo Questing (the Quest Board exhibit) available. Keep `BepInEx/LogOutput.log` open; trace lines start with `[ShopSave]`.

1. **Corrupt restore, then a new run (C8).** Start a run with the Quest Board, reach a station so `game{i}.sav` is written, quit to the menu. Close the game, corrupt `game{i}.sav` in the save folder (truncate it, or overwrite the middle with junk bytes; keep a copy). Launch, press Continue.
   - Expected: the game shows its "corrupted save" error dialog. The log has a `[ShopSave] GameRunController.Restore threw; cleared restore-in-progress and dropped N deferred Quest Board hydration(s): ...` warning with the exception. (If the corruption makes the game reject the file before it calls Restore, there is no warning; corrupt a different part of the file.)
2. Dismiss the broken run, start a new run that gets the Quest Board.
   - Expected: the board hydrates and works (quests roll at the first station, accepting one sticks). The log has **no** `Quest Board hydration deferred` line for this new run.
3. **Normal Continue still hydrates.** Put the good `game{i}.sav` copy back (or start a fresh run, accept a quest, reach the next station, quit to menu) and press Continue.
   - Expected: one `Quest Board hydration deferred until GameRunController.Restore finishes` line, then the usual `Load (GameRunController.Restore)` line. The board shows the same rolled and accepted quests and progress as before quitting. No `Dropped ... deferred Quest Board hydration(s) that belong to a run other than the one just restored` warning.
4. **Abandon from the menu.** With a saved run present, abandon it from the main menu (this also goes through Restore).
   - Expected: no `Restore threw` warning; the next new run's Quest Board hydrates normally.
5. **Restart station** (settings menu) in a run with the Quest Board.
   - Expected: board state after the restart matches what it was before this change, with no new warnings in the log.
