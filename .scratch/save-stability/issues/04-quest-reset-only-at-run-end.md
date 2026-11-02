# 04: Quest reset only at run end; bank Blue Points without localization

**What to build:** The per-profile quest backup is cleared exactly once, when a run actually ends, not every time the result panel loads (which happens on run start, Continue, Restart station, return to menu and language change). The clear covers all quest backup fields, including rolled and sold slots and the current run's completed quests once they've been moved to history. The result panel's hook only injects its Blue Point text. Separately, Blue Points and the station-progress reset are banked even if the Roguelite Shop localization table is empty; an empty table is logged instead of silently stopping. See research.md C2, C9 and section 5 items 2 and 8.

**Blocked by:** 01

**Status:** ready-for-agent

- [ ] The result-panel hook no longer changes or saves quest data
- [ ] The quest backup reset runs once per ended run, in the run-history hook, and clears every quest backup field
- [ ] Blue Points are banked before any localization check; an empty table logs a warning
- [ ] Compiles with the command in the README
- [ ] In-game checklist appended (accept a quest, quit, Continue: progress still there; finish a run: backup cleared; change language mid-run: nothing reset)
