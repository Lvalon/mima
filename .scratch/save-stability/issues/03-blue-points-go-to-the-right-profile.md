# 03: Blue Points go to the right profile

**What to build:** Blue Points earned in a run always go to the profile that played the run. Today the run's profile key is remembered the first time it's needed and cleared only when a Challenger-enabled run ends, so quitting on profile A and finishing a run on profile B credits A. Store the owning profile key with the run itself (as a run flag), so it survives quit-and-Continue, and clear any remembered key when a run starts, when a run is left, and when the profile changes. Runs saved by 0.2.12 have no such flag; for those, fall back to the current profile. See research.md C3 and section 5 item 4.

**Blocked by:** 02

**Status:** ready-for-agent

- [x] The run records its owning profile key (new key format from 02) and banking uses it
- [x] The remembered key is cleared on run start, run leave and profile change, including when Challenger is off
- [x] A run in progress from 0.2.12 (no owner flag) banks to the current profile
- [x] Compiles with the command in the README
- [x] In-game checklist appended (profile A run → Save & Quit → profile B finishes a run → check both balances)

## Implementation notes

- The owner is the run flag `lvalonmima.owner:profile:{CreationTimestamp}` in `GameRunController.ExtraFlags`, so it is saved in `game{i}.sav` and comes back on Continue, Restart station and abandon-from-menu.
- `GameRunController.Create` postfix (new file `RunOwnerPatches.cs`) writes the flag from `GameRunStartupParameters.UserProfile`, the profile the run is started on.
- `LiteProfileSaveData.ResolveRunOwnerKey(run)` reads the flag. A run without one (saved by 0.2.12) gets the current profile's key, and that key is written to the run at once, so it is saved with the next game save. With no run, the current profile is used. The remembered `_activeRunProfileKey` is now only a copy of the last resolved owner, kept for the trace (`activeRunKey=` in `DescribeState`). Banking never reads it, so a stale value can no longer send Blue Points to the wrong profile.
- The remembered key is cleared on `GameRunController.Create` (run start), `GameRunController.Restore` (Continue; re-resolved from the flag right away), `GameMaster.LeaveGameRun` (Save & Quit and the normal end of a run), `SelectProfile`, `CreateAndSelectProfile` and `DeleteProfile`. It is also cleared when `EndGameStatistics` exits early because the owner's shop has Challenger off.
- `EndGameStatistics` now checks Challenger on the owner's shop (`GetShopForRun(gameRun)`) and banks with `Save(amount, gameRun)`. The multipliers come from the run passed in, not from `CurrentGameRun`, which is null during abandon-from-menu. `Save(int, bool)` keeps its signature for the existing callers in `ShopSaveLoader.cs` and resolves through `CurrentGameRun`.
- Other in-run shop effects still read `GetShopForCurrentProfile()`. In the game the owner and the current profile are the same while a run is loaded (each profile slot has its own run save), so this only matters for banking, where the stale key was the bug.

## In-game checklist

Keep `BepInEx/LogOutput.log` open; the lines below are tagged `[ShopSave]` (trace on, `ShopSaveLoader.TraceEnabled`). Use two profiles, A and B, both with Challenger on, and note each one's shop money before starting.

1. **Wrong-profile repro (C3).** On A, start a run and play past the first node so it saves. Save & Quit from the settings menu. Switch to B in the profile menu. Start a run on B and finish it (win or lose).
   Expected: the log shows `Active run profile key <none> -> profile:{A ts}` when A's run starts, `Cleared active run profile key profile:{A ts} (run leave)` on Save & Quit, then `Active run profile key <none> -> profile:{B ts}` for B's run, and at the end `Banked N Blue Points (base M) to profile:{B ts}; current profile profile:{B ts}`. B's money went up by N; A's money is unchanged.
2. **Owner survives quit-and-Continue.** Switch back to A, press Continue, finish A's run.
   Expected: `Active run profile key … -> profile:{A ts}` after the restore, and `Banked … to profile:{A ts}` at the end. A's money goes up; B's is unchanged.
3. **Challenger off still clears the key.** Turn Challenger off on A, start and finish a run on A, then turn it back on, switch to B and finish a run there.
   Expected: no `Banked` line for A's run; a `Cleared active run profile key profile:{A ts} (run end, nothing banked)` or `(run leave)` line; B's run banks to `profile:{B ts}`.
4. **Abandon from the main menu.** On A, start a run, Save & Quit, then abandon it from the main menu.
   Expected: the abandon result screen shows Blue Points, the log shows `Banked … to profile:{A ts}` (if any were earned), and A's money matches.
5. **Run saved by 0.2.12 (no owner flag).** With 0.2.12 installed, start a run on A and Save & Quit. Install this build, start the game, select A and Continue.
   Expected: `Run has no owner profile flag (saved by 0.2.12 or earlier); assigned it to the current profile profile:{A ts}`. Finish the run: `Banked … to profile:{A ts}`, and A's money goes up. Quit and Continue again on a later save: the "no owner profile flag" line does not appear again.
6. **Restart station.** In a run on A, use Restart station from the settings menu, then finish the run.
   Expected: no "no owner profile flag" line, and Blue Points bank to `profile:{A ts}`.
7. **Profile switch clears the key.** In the main menu after a run, switch profile, create a new profile, or delete one.
   Expected: if a key was remembered, a `Cleared active run profile key … (SelectProfile)` / `(CreateAndSelectProfile)` / `(DeleteProfile)` line; no Blue Points move between profiles.
