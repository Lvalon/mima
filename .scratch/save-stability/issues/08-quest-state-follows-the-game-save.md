# 08: Quest state follows the game save

**What to build:** "Restart station" rewinds quests the same way quit-and-Continue does. The game keeps the live run-flag set by reference inside its in-memory last save, so Restart station brings back the deck but keeps quest flags changed after the save (losing quest 4/10 rewards while genji or the Shadows return to the deck). After the game saves a run, give the in-memory snapshot its own copy of the flags so it matches what was written to disk. Accepting or abandoning a quest is written to the run's flags right away, so the deck changes it causes and the quest record rewind together. Runs saved by 0.2.12 need no migration (the file format doesn't change), but confirm a 0.2.12 mid-run save continues correctly. This changes a game object other mods may also hold, so log once when the copy is made and note any mod-compat concern in the checklist. See research.md C4, C7 and section 5 item 5.

**Blocked by:** 04, 07

**Status:** ready-for-agent

- [ ] The in-memory last save holds its own copy of run flags after every game save; disk output unchanged
- [ ] Accept and abandon write the quest's run flags immediately
- [ ] Restart station and quit-and-Continue give the same deck and quest state
- [ ] A mid-run save from 0.2.12 continues with quests intact
- [ ] Compiles with the command in the README
- [ ] In-game checklist appended (the quest 4 restart repro from research.md section 4; compare Restart station against quit-and-Continue)
