# 03: Blue Points go to the right profile

**What to build:** Blue Points earned in a run always go to the profile that played the run. Today the run's profile key is remembered the first time it's needed and cleared only when a Challenger-enabled run ends, so quitting on profile A and finishing a run on profile B credits A. Store the owning profile key with the run itself (as a run flag), so it survives quit-and-Continue, and clear any remembered key when a run starts, when a run is left, and when the profile changes. Runs saved by 0.2.12 have no such flag; for those, fall back to the current profile. See research.md C3 and section 5 item 4.

**Blocked by:** 02

**Status:** ready-for-agent

- [ ] The run records its owning profile key (new key format from 02) and banking uses it
- [ ] The remembered key is cleared on run start, run leave and profile change, including when Challenger is off
- [ ] A run in progress from 0.2.12 (no owner flag) banks to the current profile
- [ ] Compiles with the command in the README
- [ ] In-game checklist appended (profile A run → Save & Quit → profile B finishes a run → check both balances)
