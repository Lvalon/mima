# 07: Restore cleanup

**What to build:** A run restore that throws (for example on a corrupt game save) no longer leaves the Quest Board stuck un-hydrated in every later run. The "restore in progress" flag and the deferred-hydration list are cleared whatever happens (a Harmony finalizer, not only the postfix), and deferred exhibits that belong to a run other than the current one are dropped instead of being synced into the profile's quest backup. The restore-snapshot branch that nothing can reach, and the pending-hydration flag that is set but never read, are removed. See research.md C8, C10 and section 5 item 6.

**Blocked by:** 01

**Status:** ready-for-agent

- [ ] Restore bookkeeping is cleared even when the game's restore throws, and the exception is logged
- [ ] Deferred exhibits from a dead run are discarded, not synced
- [ ] The unreachable restore-snapshot branch and its unused flag are gone, with no behaviour change on the paths that do run
- [ ] Compiles with the command in the README
- [ ] In-game checklist appended (corrupt `game{i}.sav`, Continue → error dialog, start a new run with the Quest Board: it hydrates)
