# 05: Safe writes, backup, schema version

**What to build:** A crash or force-quit during a save can no longer corrupt the shop file. Saves write to a temp file, flush it, then swap it in, keeping the previous good file as a `.bak` (with a delete-and-move fallback if replace isn't supported on Mono). If the main file fails to load, the backup is tried; the unreadable file is kept as `.corrupt-<timestamp>` rather than overwritten, the player sees an error dialog once (the way the game reports bad saves), and saving resumes from whatever was recovered. The file gains a top-level schema version: a file with no version is version 0 (0.2.12) and goes through an explicit migration step, so future field renames migrate instead of silently resetting. See research.md C5, C6, P6 and section 5 item 3.

**Blocked by:** 01

**Status:** ready-for-agent

- [ ] Every write goes temp → flush → swap, leaving the previous file as `.bak`
- [ ] A failed load tries `.bak`; the bad file is preserved as `.corrupt-<ts>`; a one-time dialog tells the player
- [ ] After a recovered load, saving works again (the "load failed, skip every save" trap only applies when nothing could be recovered, and it's logged)
- [ ] Schema version written on save; unversioned 0.2.12 files load through a version-0 migration with no data loss
- [ ] Encryption and the YAML format are unchanged apart from the version field
- [ ] Compiles with the command in the README
- [ ] In-game checklist appended (load a 0.2.12 file; truncate the file and launch; delete both files and launch)
