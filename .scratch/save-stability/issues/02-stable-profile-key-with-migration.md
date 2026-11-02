# 02: Stable profile key with migration

**What to build:** Renaming a profile no longer resets its shop. Shops are keyed by the profile's creation timestamp only. When the new key is missing, the save looks for legacy keys made from the same timestamp plus a name and moves that data under the new key (if several legacy keys match, because the profile was already renamed and lost data, keep the one with the most money plus tiers and log the choice). That way players who already lost progress to a rename get it back. Every place that builds the key inline uses the one shared key function. A lookup made before the file has loaded must not create and persist an empty shop. See research.md C1 and section 5 item 1.

**Blocked by:** 01

**Status:** ready-for-agent

- [ ] One key function, timestamp-only; no other code builds profile keys by hand
- [ ] A 0.2.12 file with `profile:{ts}_{name}` keys loads with every shop intact under the new keys
- [ ] Several legacy keys for one timestamp: the richest is kept, the choice is logged, the rest stay in the file untouched (not deleted) so nothing is destroyed
- [ ] Renaming a profile keeps money, tiers, Challenger toggle and history labels
- [ ] No empty shop is created or saved for a profile before the file has loaded
- [ ] Compiles with the command in the README
- [ ] In-game checklist appended (include: buy a tier, rename, reopen shop; and load a copied 0.2.12 save)
