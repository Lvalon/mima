# 01: Make save failures visible

**What to build:** When the shop file fails to load or restore, or a save is skipped because an earlier load failed, the BepInEx log says so, with the reason, file size and exception. Every load and save writes one trace line saying why it ran, which profile key it used, the active run profile key, money, total tiers and Challenger state. When a lookup creates a brand-new empty shop for a profile, that is logged too, with the key. Profile changes (rename, select, create, delete) log the old and new key and whether the old key had a shop. This is the prefactor for the other tickets: saving takes a `reason` the way loading already takes a location, at every call site. See research.md C5 and section 4 steps 1–3.

**Blocked by:** None (can start immediately)

**Status:** ready-for-agent

- [ ] Load and restore failures log an error with reason, file length and exception; nothing is swallowed silently
- [ ] A save blocked by an earlier failed load logs a warning once per session
- [ ] Every save call site passes a short reason string; every load and save writes one trace line (behind a single on/off toggle)
- [ ] Creating an empty shop for a key that wasn't there is logged with the key
- [ ] Profile rename/select/create/delete log old key, new key, and whether the old key exists in the save
- [ ] No change to what is saved or loaded; the file format is untouched
- [ ] Compiles with the command in the README
