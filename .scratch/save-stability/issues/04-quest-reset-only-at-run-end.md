# 04: Quest reset only at run end; bank Blue Points without localization

**What to build:** The per-profile quest backup is cleared exactly once, when a run actually ends, not every time the result panel loads (which happens on run start, Continue, Restart station, return to menu and language change). The clear covers all quest backup fields, including rolled and sold slots and the current run's completed quests once they've been moved to history. The result panel's hook only injects its Blue Point text. Separately, Blue Points and the station-progress reset are banked even if the Roguelite Shop localization table is empty; an empty table is logged instead of silently stopping. See research.md C2, C9 and section 5 items 2 and 8.

**Blocked by:** 01

**Status:** done (awaiting in-game checklist)

- [x] The result-panel hook no longer changes or saves quest data
- [x] The quest backup reset runs once per ended run, in the run-history hook, and clears every quest backup field
- [x] Blue Points are banked before any localization check; an empty table logs a warning
- [x] Compiles with the command in the README
- [x] In-game checklist appended (accept a quest, quit, Continue: progress still there; finish a run: backup cleared; change language mid-run: nothing reset)

Note: a second clear, `ShopModHandlers.ResetQuestStateForNewRun` on entering the first station of a run (Challenger on), predates this branch and is still there; this ticket left it alone.

## In-game checklist

Keep `ShopSaveLoader.TraceEnabled = true` (ticket 01) so every save logs `[ShopSave] Save (<reason>): ...` to `BepInEx/LogOutput.log`. Use a profile with Challenger mode on and the Quest Board. Back up `lvalonmimaShopSave.txt` first.

1. **Continue keeps quest progress.** Start a run, accept a quest, make progress on it (win a battle), then advance to the next station so it persists. Save & Quit to the menu, then Continue.
   Expected: the quest and its progress are still on the Quest Board. The log has no `Save (GameResultPanel.CustomLocalizationAsync)` line at run start, on return to the menu or on Continue.
2. **Restart station keeps quest progress.** Mid-run with an accepted quest, use Restart station.
   Expected: the quest is still there; no `Save (GameResultPanel.CustomLocalizationAsync)` line.
3. **Language change mid-run resets nothing.** Mid-run with quest progress, switch the game language and back.
   Expected: quest progress unchanged, no `Save (GameResultPanel.CustomLocalizationAsync)` line. At the end of the run the Blue Point bonus rows on the result screen show names in the current language.
4. **Run end clears the backup once.** Finish a run (win or lose) with at least one completed quest and at least one rolled or sold Quest Board slot.
   Expected: the log shows `Save (EndGameStatistics)` and exactly one `Save (AppendGameRunHistory)` for that run. The History panel lists the run's completed quests and Challenger modifiers. Start a new run: the Quest Board starts fresh (no old progress, rolled or sold slots), and when the new run ends its history does not repeat the previous run's completed quests.
5. **Run end with Challenger off.** Turn Challenger off and finish a run.
   Expected: one `Save (AppendGameRunHistory)` line, no Blue Points banked (as before), no errors or warnings.
6. **Blue Points without the localization table.** Temporarily rename `RogueliteShopEn.yaml` in the installed mod so the table is empty, then finish a run with Challenger on.
   Expected: a warning `[ShopSave] Roguelite Shop En localization table has no BluePoint.* keys (...); banking N Blue Points without run bonuses`, and shop money goes up by the run's base Blue Points (no hurry bonuses). Put the file back afterwards.
7. **Old saves port over.** With a 0.2.12 shop file holding a quest backup from a run in progress, launch and Continue.
   Expected: quest progress, money, tiers, Challenger toggle and history labels are unchanged.
