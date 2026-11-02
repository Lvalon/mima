# 06: Steam Cloud filename

**What to build:** The shop file syncs through Steam Cloud like the game's own saves. On the dev machine, Steam's cloud list for LBoL contains only `*.sav` files and profile files, so the shop file is saved as `lvalonmimaShopSave.sav` in the same folder (its backup and corrupt copies follow the new name). If the `.sav` file is missing, the legacy `.txt` is read and migrated; the `.txt` is left in place, untouched, so downgrading or a failed first write loses nothing. Note the assumption: the cloud rule being `*.sav` at that root is inferred (research.md P1 and open questions), so the checklist must include confirming the new file actually appears in Steam's cloud list. See research.md section 5 item 7.

**Blocked by:** 05

**Status:** ready-for-agent

- [ ] Saves go to `lvalonmimaShopSave.sav` (plus its `.bak`/`.corrupt-*`)
- [ ] With only the legacy `.txt` present, it loads, migrates, and the next save writes `.sav`; the `.txt` is never modified or deleted
- [ ] With both present, `.sav` wins and that is logged once
- [ ] Compiles with the command in the README
- [ ] In-game checklist appended, including checking `remotecache.vdf` (or a second machine) for the new file
