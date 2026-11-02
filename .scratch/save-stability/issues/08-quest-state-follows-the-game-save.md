# 08: Quest state follows the game save

**What to build:** "Restart station" rewinds quests the same way quit-and-Continue does. The game keeps the live run-flag set by reference inside its in-memory last save, so Restart station brings back the deck but keeps quest flags changed after the save (losing quest 4/10 rewards while genji or the Shadows return to the deck). After the game saves a run, give the in-memory snapshot its own copy of the flags so it matches what was written to disk. Accepting or abandoning a quest is written to the run's flags right away, so the deck changes it causes and the quest record rewind together. Runs saved by 0.2.12 need no migration (the file format doesn't change), but confirm a 0.2.12 mid-run save continues correctly. This changes a game object other mods may also hold, so log once when the copy is made and note any mod-compat concern in the checklist. See research.md C4, C7 and section 5 item 5.

**Blocked by:** 04, 07

**Status:** ready-for-agent

- [x] The in-memory last save holds its own copy of run flags after every game save; disk output unchanged
- [x] Accept and abandon write the quest's run flags immediately
- [ ] Restart station and quit-and-Continue give the same deck and quest state
- [ ] A mid-run save from 0.2.12 continues with quests intact
- [x] Compiles with the command in the README
- [x] In-game checklist appended (the quest 4 restart repro from research.md section 4; compare Restart station against quit-and-Continue)

## In-game checklist

Confirmed in the decompiled game code (LBoL 1.8.1): `GameRunController.Save` sets `ExtraFlags` by reference; `GameMaster.SaveGameRun` writes the file, then keeps the object as `GameMaster.GameRunSaveData`; `RequestReenterStation` → `CoReenterStation(GameRunSaveData)` → `GameRunController.Restore` copies `data.ExtraFlags`. The new postfix (`GameMaster_SaveGameRun_Patch`) runs after the write, so disk output is unchanged.

1. **Log line.** Start or Continue a run and enter any node. `LogOutput.log` has one `[ShopSave] GameMaster.SaveGameRun: the kept game-run snapshot now holds its own copy of ExtraFlags (N flags)...` line per session, not one per save.
2. **Quest 4 restart repro (research.md section 4, C4).** Solo Questing, Challenger on. Accept quest 4 (genji is added), finish it, enter the next node. The reward runs: genji is removed, power and HP are granted.
   - Then **Restart station** from settings. Expected: genji is back in the deck and the reward runs again on re-entry (genji removed again, power and HP granted again). Before this fix genji came back and the reward was lost.
   - Repeat from a fresh attempt, this time using **quit to menu and Continue** instead. Expected: the same deck, HP, power and Quest Board state as the Restart station result.
3. **Quest 10.** As step 2 with quest 10 (Shadows): after Restart station the Shadows come back and the HP and gold reward runs again, the same as quit-and-Continue.
4. **Accept, then rewind.** In a station, accept quest 4 (genji added), then Restart station without leaving the station. Expected: no genji, quest 4 not accepted. Do the same with quit-and-Continue: same deck and the same Quest Board (same rolled quests and requirement text).
5. **Abandon, then rewind.** With quest 4 accepted at the last save, abandon it (genji removed, slot sold out), then Restart station. Expected: genji back and quest 4 still accepted with its progress. Quit-and-Continue gives the same.
6. **Accept, then save.** Accept quest 13 (curse added), then go to the next node, then quit and Continue. Expected: curse in the deck and quest 13 accepted with the same requirement text as before the quit.
7. **Restart twice.** Use Restart station twice in a row in the same node. Expected: both restarts give the same deck and quest state (the snapshot is not altered by the first restarted run).
8. **0.2.12 mid-run save.** With 0.2.12 installed, start a run, accept a quest, advance one node, quit. Install this build and Continue. Expected: the quest board shows the same accepted quests, progress and rolled slots; the deck is unchanged. The file format does not change, so no migration runs.
9. **Rolled board survives restart.** Note the rolled quests and requirement text on entering a node, then Restart station. Expected: the same board. Rolls written after the game save are no longer carried into Restart station by the shared set; they are rolled again from the restored `CardRng`, the same way Continue does. If the board differs between the two, report it (it would mean a roll is not seeded by the run RNG).
10. **Battle-finish restart.** Win a battle that advances a quest, then Restart station from the reward screen. Expected: quest progress matches what a quit-and-Continue at the same point shows.

**Mod-compat notes.** After every game save, `GameMaster.Instance.GameRunSaveData.ExtraFlags` is a separate set from `CurrentGameRun.ExtraFlags`. A mod that adds flags to the live run after a save and expects Restart station to keep them (relying on the shared reference) will now see them rewound, the same as on Continue. A mod that writes into the kept snapshot's flags expecting the live run to change will no longer affect the live run. The postfix runs at `Priority.Last`, so other mods' `SaveGameRun` postfixes still see the original shared set. With RngFix and LBOLMP installed, repeat step 2 once and check for errors in the log.
