# 02: Stable profile key with migration

**What to build:** Renaming a profile no longer resets its shop. Shops are keyed by the profile's creation timestamp only. When the new key is missing, the save looks for legacy keys made from the same timestamp plus a name and moves that data under the new key (if several legacy keys match, because the profile was already renamed and lost data, keep the one with the most money plus tiers and log the choice). That way players who already lost progress to a rename get it back. Every place that builds the key inline uses the one shared key function. A lookup made before the file has loaded must not create and persist an empty shop. See research.md C1 and section 5 item 1.

**Blocked by:** 01

**Status:** done (awaiting in-game checklist)

- [x] One key function, timestamp-only; no other code builds profile keys by hand
- [x] A 0.2.12 file with `profile:{ts}_{name}` keys loads with every shop intact under the new keys
- [x] Several legacy keys for one timestamp: the richest is kept (most money plus Blue Points spent on bought tiers, then most tiers), the choice is logged, the rest stay in the file untouched (not deleted) so nothing is destroyed
- [x] Renaming a profile keeps money, tiers, Challenger toggle and history labels
- [x] No empty shop is created or saved for a profile before the file has loaded
- [x] Compiles with the command in the README
- [x] In-game checklist appended (include: buy a tier, rename, reopen shop; and load a copied 0.2.12 save)

## Implementation notes

- `LiteProfileSaveData.ProfileKey` is now `profile:{CreationTimestamp}` and is the only place a key is built. `LiteShopButton.GetMenuShop` uses the new non-creating `FindShop(profile)`.
- Migration runs in the schema step `ShopSaveLoader.MigrateV0ToV1` (`LiteProfileSaveData.MigrateLegacyProfileKeys`), on the data read from a version 0 file before it is merged into the active save, for every legacy key whose stable key is missing. Schema version 1 therefore means the keys are migrated, and a second load before the first save can't merge a legacy key back in. There is no lookup-time fallback: after the schema step no legacy key is left without its stable key.
- "Richest" = `MoneyOwned` plus the BluePoints spent on bought tiers (sum of `TierCosts` up to `CurrentTier`), then tier count, then key order. Summing raw money and tier count would always favour money and rank a profile that spent everything below an empty one.
- The chosen legacy entry is moved (removed from the dictionary and added under the new key), so it leaves the file in the same write that adds the new key. The other legacy entries are never touched.
- Until the file has loaded (`Restore` ran, or `Load` found no file), a lookup that misses returns no shop instead of creating one. That needed one line in `ShopSaveLoader.Load`'s no-file branch (`csd.MarkFileLoaded()`).
- Not covered here (write path, ticket 05): `ShopSaveLoader.Save` called before any load would still serialize whatever is in memory over an existing file. No current caller does that before `MainMenuPanel.Awake`, and with this ticket no empty shop is in memory to write.

## In-game checklist

Back up `lvalonmimaShopSave.txt` (in the game's save folder) before starting. Keep `BepInEx/LogOutput.log` open; all lines below are tagged `[ShopSave]`.

1. **Existing 0.2.12 save migrates.** Copy a 0.2.12 `lvalonmimaShopSave.txt` into the save folder, start the game, reach the main menu.
   Expected: the log shows `Migrated shop profile:{ts}_{name} (money=…, tiers=…, challenger=…) to key profile:{ts}` once per profile, then `Load (MainMenuPanel.Awake) … key=profile:{ts}, … money=…`. The main-menu shop button and the shop panel show the same money, tiers and Challenger toggle as before. No `Created new empty shop` warning.
2. **Migration is saved.** Do something that saves the shop (buy a tier, or start and abandon a run), quit, start again.
   Expected: no `Migrated shop` line this time (the file now holds `profile:{ts}`); the shop is unchanged.
3. **Rename keeps the shop.** Buy a tier and note money and tiers. Rename the profile in the profile menu. Reopen the shop.
   Expected: same money, tiers and Challenger toggle. The trace shows `SetProfileName(slot n): old key profile:{ts} (in save: True) -> new key profile:{ts} (in save: True)`. History labels for past runs still show. No `Created new empty shop` warning.
4. **An already-renamed profile gets its progress back.** Use a 0.2.12 save in which a profile was renamed after buying, so it has two keys `profile:{ts}_OldName` (the real progress) and `profile:{ts}_NewName` (nearly empty).
   Expected: a `Found 2 legacy shops for profile:{ts}; migrated the richest, profile:{ts}_OldName (…). Left in the save unchanged: profile:{ts}_NewName (…)` warning. The shop shows the OldName progress. After a save, decrypting the file (openssl command in research.md) shows `profile:{ts}` and the untouched `profile:{ts}_NewName`; `profile:{ts}_OldName` is gone because its data is now under `profile:{ts}`.
5. **New profile.** Create a new profile and open the shop.
   Expected: one `Created new empty shop for key profile:{ts}` Info trace (a warning only if the save already had keys for that timestamp or failed to load); the shop is at 0. Deleting a profile and recreating it under the same name also gives a fresh shop (new timestamp).
6. **No shop before load.** Start the game with trace on.
   Expected: no `Created new empty shop` line before the first `Load (MainMenuPanel.Awake)` line. If something asks for the shop early, the log shows at most one `Shop lookup for … before the shop file loaded; returning no shop` trace instead.
7. **Run end and Continue still work.** Finish a run with Challenger on, then continue a saved run from the main menu.
   Expected: BluePoints are added to the same profile's shop; no `Created new empty shop` warnings.
