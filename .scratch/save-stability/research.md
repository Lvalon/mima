# Save system research: why Roguelite Shop / quest data resets and acts unstable

Scope: how the mod (`llbol.char.mima`, v0.2.12, working tree at `a8d1ce8` plus about 40 uncommitted files) saves and loads its data. This is research only; no source file was changed.

Sources used:
- this repo (paths are relative to the repo root, line numbers are from the current working tree);
- git history;
- the game assemblies, decompiled with `ilspycmd` 9.1 from `LBoL.app/Contents/Resources/Data/Managed`: `LBoL.Core.dll`, `LBoL.Presentation.dll`, `YamlDotNet.dll` 11.2.1, and `LBoL-Entity-Sideloader.dll`. Game code is cited as `Type.Method`, because decompiled line numbers are not stable;
- the developer machine's actual save folder and Steam cloud manifest. These were only read, never written.

Anything I worked out rather than read directly is marked **(inferred)**.

## Summary: most likely causes

- **Renaming a profile orphans all shop progress (confirmed).** Shop data is keyed by `profile:{CreationTimestamp}_{Name}` (`LiteProfileSaveData.cs:58-59`). The game lets players rename a profile (`ProfilePanel` confirm, then `GameMaster.SetProfileName`). After a rename the lookup misses, and `GetShopForCurrentProfile` quietly creates an empty `LiteShop` and saves it (`LiteProfileSaveData.cs:63-76`). Money, tiers, Challenger toggle and history labels all look reset. The old data is still in the file under the old key, so it can be recovered.
- **The shop file is not in Steam Cloud (confirmed on the dev machine).** LBoL's `remotecache.vdf` lists only `*.sav` and `Profiles/*.json`, never `lvalonmimaShopSave.txt`. A player on a second PC, a Steam Deck or a fresh install gets profiles, runs and histories back from the cloud, but not the shop.
- **The "end of run" quest reset actually runs on every UI load (confirmed).** `GameResultPanel_CustomLocalizationAsync_Patch` wipes the per-profile quest copy and writes the file (`GameRunHistory.cs:169-217`). But `CustomLocalizationAsync` is a panel load and locale hook: `UiManager.InternalLoadPanelAsync` calls it whenever `GameResultPanel` is loaded, and it is loaded with both the main-menu UI and the in-run UI (`GameMaster.LoadSharedUiTo`), plus on every language change (`L10nManager.ReloadAsync`). So the quest backup is erased on every Continue, Restart station, new run and return to menu.
- **Blue Points can go to the wrong profile (confirmed).** `_activeRunProfileKey` is cached during a run and cleared only on a Challenger-enabled run end (`LiteProfileSaveData.cs:92-120`). Sequence: play on profile A, Save & Quit, switch to profile B, finish a run. The Blue Points B earned go into A's shop.
- **Quest state is kept in two places with different rewind rules (confirmed).** One copy is in the run's `ExtraFlags`, inside the game's `game{i}.sav`. The other is in the mod's per-profile file. The game also shares the live `ExtraFlags` set with the in-memory "last save" (`GameRunController.Save` stores `ExtraFlags` by reference). So the in-run "Restart station" button rewinds the deck but not the quest flags, while quit-and-Continue rewinds both. Quest rewards resolved in the station-enter coroutine (quest 4, quest 10) can be lost, or applied in a mismatched way.
- **Every failure is silent, and writes are not atomic (confirmed design).** `Load` swallows all exceptions without logging (`ShopSaveLoader.cs:225-229`), `Restore` swallows too (`LiteProfileSaveData.cs:163-165`), and other paths log only at `LogDebug`, which is hidden by default. Writes go through the game's `File.WriteAllBytes` with no temp file and no backup. The file is rewritten about a dozen times per station. A truncated file shows a fully reset shop for that session, and the `_loadFailed` guard then silently drops every change made in that session.

The latest commit `a8d1ce8` (the Sideloader card index generator and gun name id) is **not** a cause. Saved data refers to cards by class-name Id (`DefaultConfig.cs:13-20`), never by `CardConfig.Index`, and no game save type stores card indexes (section 3.3).

---

## 1. Saved data inventory

### 1.1 Where things live

| Store | Path / format | Keyed by | Owner |
|---|---|---|---|
| Mod shop file | `<persistentDataPath>/<steamId>/lvalonmimaShopSave.txt` (`ShopSaveLoader.cs:20,71-74`; `SteamPlatformHandler.GetSaveDataFolder` returns `Path.Combine(Application.persistentDataPath, _steamId)`). It is YAML (YamlDotNet, `DisableAliases`), UTF-8 and uncompressed (`SaveDataHelper.EncodeYaml(text,false)`), then AES-256-CBC with a hard-coded key and IV (`ShopSaveLoader.cs:25-53,156-164`). | One file per Steam account. Inside: `Saves["profile:{CreationTimestamp}_{Name}"]`, one `LiteShop` per profile. | Mod |
| Run flags | Strings in `GameRunController.ExtraFlags`, serialized by the game into `game{i}.sav` (`GameRunController.Save`: `gameRunSaveData.ExtraFlags = ExtraFlags`) | Per save slot and per run | Game file, mod content |
| Stage flags | `Stage.ExtraFlags`, inside `game{i}.sav` | Per run | Game file, mod content |
| BepInEx config | `BepInEx/config/llbol.char.mima.cfg`, `mimaCardMult` (`BepinexPlugin.cs:58-64,85`) | Global | BepInEx |
| Game history (read only) | `history{i}.sav`, read by `GameMaster.GetGameRunHistory()` (`ShopModHandlers.StartupDraft.cs:41,78`) | Per slot | Game |

The mod does **not** use PlayerPrefs, nor Sideloader's `CustomGameRunSaveData` (`*_gr_*.modd`). `LiteProfileSaveData` copies that API's shape (`filePrefix "gr"`, `GetID`, `RegisterSelf`, `DeleteFileOnGamerunEnd`), but `GetID` is never used for storage (`LiteProfileSaveData.cs:15-48`). I decrypted the dev machine's file with the in-code key and IV. It is plain YAML with two `profile:…` keys, which confirms the format.

### 1.2 Fields of `LiteShop` (one per profile; `LiteShop.cs:86-103`)

| Field | Player-facing meaning (En YAML key: text) | Written | Read |
|---|---|---|---|
| `MoneyOwned` | Blue Points banked. `Shop.LiteShopButton.Desc`: "Points gained at the end of the game under Challenger Mode are saved, they can be used in the Roguelite Shop…". Shown via `Shop.Money`. `difficulty.reverse` halves it and voids it on a loss; `difficulty.ascension` doubles it (`LiteProfileSaveData.cs:122-136`). | `LiteProfileSaveData.Save(amount)` via `GameMaster_EndGameStatistics_Patch` (`GameRunHistory.cs:145`); `ShopItem.PurchaseNextTier`/`Refund` (`LiteShop.cs:56-83`); refunds on reconcile (`LiteShop.cs:241-281`) | Shop panel (`LiteShopButton.ShopPanel.cs`), `CanPurchase` |
| `ChallengerModeEnabled` | `Shop.ChallengerModeToggle.*`, `Shop.LiteShopButton.Active/Inactive`. "Challenger Mode: The first Player Phase is skipped." It gates every shop effect. | Toggle button (`LiteShopButton.ChallengerToggle.cs:140-167`); shift-click on the menu button (`LiteShopButton.cs:269-281`) | Nearly every handler, e.g. `ShopModHandlers.ShopEconomy.cs:24-73`, `SystemBoard_OnEnterGameRun_Patch.cs:28-29`, `LiteProfileSaveData.cs:112` |
| `Items[id].CurrentTier` (plus `MaxTier`, `TierCosts`, `Initial`, `Delta`, `IsAlpha`, and the computed `IsMaxTier`/`Tier` written as noise) | One entry per shop upgrade. Keys `Shop.<id>` / `Shop.<id>.Desc` / `.Next`: `init.fp/sp/hp/gold/card/exhibit/solo`, `discount.sc/shop/upgrade/remove`, `feature.teasync/gapple/sponsor`, `battle.block/graze/heal/seedraw/rolldiscard/hacks`, `alter.freechoice/wings/blankcard`, `difficulty.reverse/ascension`, `refund` (`RogueliteShopEn.yaml:14-130`; ids in `LiteShop.cs:146-177`). Only `CurrentTier` survives a load; everything else is rebuilt from code (`LiteShop.cs:244-263`). | Purchase/refund (`LiteShopButton.ShopPanel.cs:446-512`) | `ShopModHandlers.*`; descriptions through `LocalisationKeys.GetShopItemDescription` (`LocalisationKeys.cs:179-210`) |
| `BPProgress {stage, level}` | The last station reached. It feeds the `BluePoint.hurry*` bonuses (`RogueliteShopEn.yaml:166-187`, e.g. "Lose within Level 4 of Act 1"), and it also stops quest stations from counting twice ("e.g. when restarting the stage", `ShopModHandlers.QuestEvents.cs:243-252`). | `ShopModHandlers.QuestEvents.cs:403-408`; reset at `GameRunHistory.cs:144` | `GameRunHistory.cs:87-91`; `QuestEvents.cs:246-252,385-401`; `exquesting.Rolling.cs:187` |
| `RunModifiersByTimestamp` | History panel: "Challenger Mode Active" label and modifier tooltip (`Shop.ChallengerModeHistory.*`, `Shop.Loadout.None`). Keyed by the game's `GameRunRecordSaveData.SaveTimestamp`. | `GameMaster_AppendGameRunHistory_Patch` (`GameRunHistory.cs:236-246`) | `HistoryPanelChallengerHistory.UpdateChallengerHistoryUi` (`GameRunHistory.cs:427-467`) |
| `CurrentRunCompletedQuests` → `RunCompletedQuestsByTimestamp` | History panel "Completed Quests" (`Shop.QuestHistory.*`) | `ShopModHandlers.RecordRewardedQuestCompletion` (`ShopModHandlers.cs:225-240`); moved over at history append (`GameRunHistory.cs:248-254`) | `GameRunHistory.cs:469-509` |
| `QuestProgress`, `QuestRequirements`, `QuestCompletedCards`, `QuestModifiers`, `QuestRolledSlots`, `QuestSoldSlots` | A per-profile backup copy of the Quest Board exhibit's state. `ExhibitsEn.yaml:21-26`: "Quest progress is saved at the end of each combat and when advancing a level"; `StatusEffectsEn.yaml:192-194` `sequest`: "Quests have Conditions or Challenges…". These are only read when the run flags are empty (`exquesting.Sync.cs:92-143`). | `ShopModHandlers.PersistQuestProgress(syncToLiteShop:true)` (`ShopModHandlers.cs:369-432`); wiped at `GameRunHistory.cs:205-213` and `ShopModHandlers.cs:584-593`; `Save()` filters them (`ShopSaveLoader.cs:110-151`) | `ShopModHandlers.Read*FromLiteShop` (`ShopModHandlers.cs:115-223`); `exquesting.Rolling.cs:215` |

### 1.3 Run flags (in the game's `game{i}.sav`)

| Flag | Meaning | Writer / reader |
|---|---|---|
| `exquesting.quest:<cardId>=<n>` | Progress on an accepted quest | `ShopModHandlers.cs:298-306` / `:55-64` |
| `exquesting.requirement:<cardId>=<enc>` | The quest's rolled condition | `:308-316` / `:71-80` |
| `exquesting.completed:<cardId>` | A completed quest whose reward is still pending | `:318-324` / `:104-113` |
| `exquesting.modifier:<cardId>=<n>` | Stacks for modifier quests (26-30) | `:326-339` / `:142-151` |
| `exquesting.rolled:<slot>=<cardId>`, `exquesting.sold:<slot>` | Board slots (added in `bda8858` "roll quest consistency fix") | `:341-366` / `:82-102` |
| `shop.remove.applied:<n>` | `discount.remove` ("Snowballing Advantage") already applied | `ShopModHandlers.ShopEconomy.cs:97-116` |
| Stage flag `lvalonmima.freechoice` | `alter.freechoice` ("I Knew It") locked in for the run. The 0.2.11 fix moved it to the stage because "those are restored before Restore() calls FinishStation" | `ShopModHandlers.ShopEconomy.cs:19-20,75-87` |

### 1.4 State that is not saved but behaves like saved data

- `LiteProfileSaveData._activeRunProfileKey` (`LiteProfileSaveData.cs:54,92-107`)
- `ShopSaveLoader._loadFailed`, `_pendingRestoreQuestHydration`, `IsGameRunRestoreInProgress` (`ShopSaveLoader.cs:21-23`)
- `exquesting.DeferredRestoreHydration` (static, `exquesting.Sync.cs:28`)
- `ShopModHandlers.LastAppliedShopDiscountFactor/SeeOrder/BlankCard` (static, `ShopModHandlers.QuestEvents.cs:38-42`)
- the static `_currentShop`, `_shopPanel` in `LiteShopButton`

The other YAML files: `JadeBoxesEn.yaml` holds only `JadeBoxCreative`, which has no saved state of its own. The uncommitted `StatusEffectsEn.yaml` change (`seSeija`) is not related to saving. The ZhHans and ZhHant `RogueliteShop` files have the same keys as En (only the order differs), and their `exquesting` text says the same thing about saving as En.

---

## 2. Current design and past measures

### 2.1 Lifecycle

1. **Registration.** `BepinexPlugin.Awake` runs `new LiteProfileSaveData().RegisterSelf(...)`, which makes an empty instance active in the `MiniTracker` singleton (`BepinexPlugin.cs:92`; `LiteProfileSaveData.cs:21-43`; `MiniTracker.cs:41-46`).
2. **Load** (`ShopSaveLoader.Load`, `ShopSaveLoader.cs:182-230`): read bytes, decrypt, decode, deserialize with `IgnoreUnmatchedProperties`, run `LiteShop.ReconcileWithDefaults` on each shop, then `Restore()`. That merge **replaces** every in-memory profile shop with the disk copy (`LiteProfileSaveData.cs:146-166`). Load is triggered by:
   - `MainMenuPanel.Awake` (`ShopSaveLoader.cs:233-241`). The main menu is destroyed when a run starts and rebuilt when it ends (`GameMaster.CoSetupGameRun` → `UnloadMainMenuUi`; `CoLeaveGameRun` → `LoadMainMenuUiAsync`), so this fires at startup and after every run;
   - `GameRunController.Restore` postfix (`ShopSaveLoader.cs:261-285`), which runs on Continue, on Restart station (`GameMaster.CoReenterStation`, from `SettingPanel`) and on Abandon from the menu (`GameMaster.RequestAbandonGameRun`).
3. **Save** (`ShopSaveLoader.Save`, `ShopSaveLoader.cs:95-180`) serializes the whole multi-profile object and writes it with the game's private `GameMaster.WriteSaveData`, which is just `File.WriteAllBytes` wrapped in a `Debug.LogError` catch. It is triggered by:
   - every `GameRunController.Save` (`ShopSaveLoader.cs:243-259`): enter map node, battle finish, adventure;
   - `StationEntered` (`ShopModHandlers.QuestEvents.cs:407-408`);
   - every `PersistQuestProgress(saveToDisk:true)`: station enter, battle end, the station-enter reward coroutine (`exquesting.Sync.cs:374`, `ShopModHandlers.BattleReactors.cs:602`, `ShopModHandlers.cs:571`, `QuestEvents.cs:344`);
   - each quest completion (`ShopModHandlers.cs:238-239`);
   - run end (`GameRunHistory.cs:145-146,255-256`, `ShopSaveLoader.cs:300-319`);
   - every `GameResultPanel` load (`GameRunHistory.cs:215-216`);
   - menu actions: open shop, purchase, refund, toggle (`LiteShopButton.cs:258,280`; `LiteShopButton.ShopPanel.cs:462,501`; `LiteShopButton.ChallengerToggle.cs:160`).
4. **Profile resolution.** `GameMaster.Instance.CurrentProfile` is read at call time (`LiteProfileSaveData.cs:61`). At startup it is null until `SelectProfile`, which `GameMaster.CoStartupEnterMainMenu` calls after `LoadMainMenuUiAsync`. That is harmless because Load is not profile-specific.

### 2.2 Defensive measures already in place

- **Write guard after a failed load.** `_loadFailed` blocks `Save` while the file exists (`ShopSaveLoader.cs:98-99`). It is reset only by a later successful load (`:223`). It was there from the first shop commit `dc64a34`.
- **Schema reconciliation.** `ReconcileWithDefaults` rebuilds a fresh `LiteShop` from code. It keeps tiers (clamped to the new max), refunds tiers above the max and items that were removed, and null-coalesces every collection (`LiteShop.cs:185-282`, also since `dc64a34`). Together with `IgnoreUnmatchedProperties` this is the only migration logic. The file has **no schema version**.
- **Run-flag primary, lite fallback.** Quest state is mirrored into run flags so it rewinds with the game save. The per-profile copy is used only when the run flags are empty (`exquesting.Sync.cs:88-143`). Rolled and sold flags were added in `bda8858`; modifier flags and the "persist pending progress from the previous station" step came in `33fb82c` ("quest18 & persist fix again"; `QuestEvents.cs:382-401`).
- **Deferred hydration during Restore.** `OnAdded` is postponed while `IsGameRunRestoreInProgress` is set and replayed in the Restore postfix (`exquesting.Sync.cs:336-364`; `ShopSaveLoader.cs:265-285`).
- **Restart guards.** `BPProgress` is compared before counting station quests (`QuestEvents.cs:243-252`). "Gives You Wings" is re-added after a restart; that was the 0.2.10 fix, gated by Challenger mode since 0.2.12 (`ShopSaveLoader.cs:278-279,287-297`). "I Knew It" was moved to `Stage.ExtraFlags` in 0.2.11 (`ShopModHandlers.ShopEconomy.cs:19-20`).
- **What `Save` keeps.** It drops empty keys, and drops requirements for quests that were not accepted (`ShopSaveLoader.cs:128-149`).
- **Logging.** `a086a40` (0.2.0) removed about 178 `[EXQUESTING SAVE]` `LogInfo` trace lines. `05b2f96` (0.2.12) added `LogDebug(e)` in several catches, but `Load` and `Restore` still log nothing.

### 2.3 What the uncommitted changes touch

Running `git diff -U0` and ignoring `using` lines leaves only these changes:
- the `seSeija` rewrite (`Source/StatusEffects/seSeija.cs` plus its YAML);
- `makezip.sh`, which now installs into `<LBoL>/Mods/<name>` instead of `BepInEx/plugins/lvalonmima` and warns when two folders have the same GUID;
- the `csproj` passing `$(TargetPath)`;
- the `modinfo.json` timestamp.

None of them changes save logic. The new install path matters only for a possible double load (section 3.2, P5).

### 2.4 Comparison with how LBoL saves

The game uses one file per slot (`save{i}.sav`, `game{i}.sav`, `history{i}.sav`), LZSS-compressed YAML, also non-atomic `File.WriteAllBytes` (`GameMaster.WriteSaveData`). It keeps a `_backup` copy only when it deletes a file (`GameMaster.TryDeleteSaveData`). It checks the run save's version and shows a dialog when it discards one (`GameMaster.SelectProfile` → `GameRunSaveDataLoadResult.VersionMismatch/Failed`), and history loading falls back to an empty list with `Debug.LogError` (`GameMaster.AppendGameRunHistory`). So the mod copies the game's non-atomic writes, but writes far more often, keeps one shared file for all profiles, and never tells the player when something fails.

---

## 3. Failure modes

### 3.1 Confirmed (path traced end to end)

**C1. A profile rename resets the shop.** `ProfilePanel` input confirm → `GameMaster.SetProfileName(index, text)` changes `ProfileSaveData.Name`. The key `profile:{CreationTimestamp}_{Name}` (`LiteProfileSaveData.cs:58-59`) then misses, and `GetShopForCurrentProfile` inserts a `new LiteShop()` (`:69-73`). The next save persists that empty shop. Everything else follows from the same miss: the menu label falls back to the plain text (`LiteShopButton.cs:174-189`), the history labels disappear (`GameRunHistory.cs:427-433`), and Challenger mode is off. The old entry remains in the file, so it can be recovered. The same thing happens, as intended, when a profile is deleted and recreated.

**C2. The quest backup is wiped on every UI load and locale change.** `UiManager.InternalLoadPanelAsync` always awaits `panel.CustomLocalizationAsync()`. `GameMaster.LoadSharedUiTo` loads `GameResultPanel` for both `MainMenuUiList` and `GameRunUiList`, and `L10nManager.ReloadAsync` calls it on every panel when the language changes. The postfix (`GameRunHistory.cs:174-217`) therefore resets `QuestProgress/Requirements/CompletedCards/Modifiers` and writes the file:
- at run start;
- after every Continue or Restart station (the UI is rebuilt after `Restore`);
- on every return to the menu;
- when the language changes in the middle of a run.

During a Continue the exhibit has already hydrated in the `Restore` postfix, so the effect is that the lite copy on disk is empty until the next station-enter persist. Any later restore whose run flags are empty then has nothing to fall back to. The reset does reach players at the end of a run, but only by accident, through the menu's UI load. It also misses `QuestRolledSlots/QuestSoldSlots` and `CurrentRunCompletedQuests`.

**C3. Blue Points are credited to the wrong profile.** `GetActiveRunProfileKey` caches the key the first time it is called during a run (`LiteProfileSaveData.cs:92-102`). Every `GameRunController.Save` postfix calls it. It is cleared only at `:118-119`, and that line is unreachable when the cached shop is null or Challenger is off (`:111-113`). Only `GameRunHistory.cs:145` passes `ending:true`. Sequence: run on profile A, Save & Quit, switch to profile B in `ProfilePanel`, finish B's run. Then `EndGameStatistics` gates on B's shop (`GameRunHistory.cs:31-32`), but `Save(__result.BluePoint)` credits A's shop with A's multipliers. The result is that B never gains money, which looks like a reset or like instability.

**C4. Restart station rewinds the deck but not the quest flags.** The steps are all in the game code:
1. `GameRunController.Save` assigns `gameRunSaveData.ExtraFlags = ExtraFlags`, a reference and not a copy.
2. `GameMaster.SaveGameRun` keeps that object (`GameRunSaveData = data`).
3. `GameMaster.RequestReenterStation` → `CoReenterStation(instance.GameRunSaveData)` → `GameRunController.Restore`, which copies `data.ExtraFlags.ToHashSet()`.

So the in-memory snapshot used by Restart station carries the **live** flags, while the deck, HP and money come from the snapshot. Quit and Continue reads the file, which was serialized at save time. The mod changes flags after the save in `CoResolveCompletedQuestEffectsOnStationEnter`, which runs a frame later (`ShopModHandlers.cs:450-572`): it removes `cardgenji` and grants power and HP for quest 4, removes Shadows and grants HP and gold for quest 10, then calls `ClearQuestCompleted` and persists the flags. After Restart station, the deck has genji or the Shadows back, the completion flag is gone, and the reward never runs again. After quit and Continue the reward runs again, consistently. Same game, different outcome. This matches the 0.2.10 and 0.2.11 changelog entries about things breaking "after a restart / sl".

**C5. Load and restore failures are silent and look like a reset.**
- `ShopSaveLoader.Load` catches everything and only sets `_loadFailed` (`ShopSaveLoader.cs:225-229`). There is no log line.
- `LiteProfileSaveData.Restore` has an empty catch (`:163-165`).
- On a failed load, the in-memory state stays as the empty registered instance, so the shop shows 0 money and tier 0 for the whole session. Because `Save` is then a no-op (`:98-99`), every purchase, toggle and Blue Point banked in that session is silently thrown away.

Load fails on a truncated or garbled file: `CryptoStream` raises a padding error, YAML parsing fails, or `ReconcileWithDefaults` hits an NRE at `LiteShop.cs:246-249` when an `Items` value is null. The game's own `Debug.LogError` from `WriteSaveData` goes to `Player.log`, not to the BepInEx log.

**C6. Non-atomic writes, many times per station.** The write path is `GameMaster.WriteSaveData` → `File.WriteAllBytes`, which truncates the file and then writes it. There is no temp file, no rename and no backup. On one map-node entry the file is written at least by: `GameRunController.Save` postfix, `QuestEvents.StationEntered`, `exquesting.OnStationEntered`, and possibly the reward coroutine. It is written again at every battle end, every quest completion and every UI load. A crash, force-quit or Alt-F4 during any of these leaves a truncated file, which leads to C5.

**C7. Accepting or abandoning a quest is never persisted by itself.** `ExquestingUiLauncher.cs:294` calls `PersistQuestProgress(..., syncToLiteShop:false, saveToDisk:false, writeToRunFlags:false)`, which writes nothing. Side effects that happen on accept, such as `cardgenji` or Shadows added to the deck, gold for quest 11, or a curse for quest 13 (`:264-281`), are game state. The quest itself becomes durable only at the next station-enter persist. This matches the exhibit text ("saved at the end of each combat and when advancing a level"). But together with C4 and with the shared-reference flags it creates mismatches between deck and quest state. **(inferred)** Most such mismatches happen when the player quits or uses Restart in the station where they accepted the quest.

**C8. A restore that throws leaves `IsGameRunRestoreInProgress` stuck on true.** The Prefix sets it (`ShopSaveLoader.cs:265-268`) and only the Postfix clears it (`:281-284`). Harmony does not run postfixes when the original method throws, and `GameRunController.Restore` throws on corrupted saves (the game catches that in `CoRestoreGameRun` and `RequestAbandonGameRun`). From then on every `exquesting.OnAdded` is deferred (`exquesting.Sync.cs:338-339`), so the Quest Board in new runs never hydrates. It also leaves a static set (`:28`) holding exhibits from dead runs. The next successful `Restore` then syncs those stale exhibits into the current profile's lite copy (`:346-363`, `:282`).

**C9. Banking Blue Points depends on the localization table.** `GameRunHistory.cs:36-48` builds the bonus ids from `LocalisationKeys.GetTable(Locale.En)` and `return`s when the table is empty. That return comes before `shop.BPProgress = []` and before `Save(__result.BluePoint)` (`:144-146`). If `RogueliteShopEn.yaml` is not found (the 0.2.12 refactor moved this table from C# to YAML discovered through `DirectorySource`; see `LocalisationKeys.cs:97-122`, `Localization.cs:33-40`), **no** Blue Points are banked at all, and nothing is logged.

**C10. The restore-snapshot branch is dead code.** `useRestoreSnapshot` requires `QuestSyncReason.CreateExhibitWidget` (`exquesting.Sync.cs:74-75`), and nothing passes that reason (grep; the comment at `:12-16` says so too). `_pendingRestoreQuestHydration` is set on every Restore load (`ShopSaveLoader.cs:184`) and never consumed. So the "prefer run flags, else lite" restore logic in `:94-115` never runs.

### 3.2 Plausible risks (mechanism is clear; trigger not proven)

**P1. Steam Cloud (strong local evidence).** The dev machine's `userdata/<account>/1140150/remotecache.vdf` lists only `history{0,1}.sav`, `save{0,1}.sav`, `game{0,1}.sav`, `sys.sav` and `Profiles/*.json`. The mod file, and Sideloader's `.modd` files, are not synced. Any second device or reinstall restores profiles but gives an empty shop. Cloud restores can also bring back an older `game{i}.sav` than the mod file expects. **(inferred)** The cloud pattern is probably `*.sav` at that root.

**P2. Load replaces in-memory objects.** Every load builds new `LiteShop` instances (`LiteProfileSaveData.cs:160-161`). Mutations that were not yet saved are lost, and any reference held across a load (the static `LiteShopButton._currentShop`, set at `LiteShopButton.ShopPanel.cs:78`) points at a detached object. The shop panel refreshes `_currentShop` on show, so I found no live bug here. It is still a trap for future code, and it is why a single failed or skipped save (C5) turns into lost data at the next load.

**P3. Rolled-quest requirements are dropped from the lite copy.** `Save` keeps requirements only for accepted quests (`ShopSaveLoader.cs:132-136`). When the lite fallback is used, rolled cards come back but their conditions are regenerated, so quest conditions change after a reload.

**P4. Per-stage shop effects are not in the game save.** `ShopPriceMultiplier`, `CanViewDrawZoneActualOrder` and `RewardAndShopCardColorLimitFlag` are not serialized by `GameRunController.Save`. The mod re-applies them on `StationEntered`, using static `LastApplied*` values reset per controller (`QuestEvents.cs:347-363,443-497`). Restores whose timing does not re-enter the station (`BattleFinish`/`AfterBossReward`, `Adventure`) run without these effects until the next station **(inferred)**.

**P5. The mod could load twice.** With the new `makezip.sh` install target (`Mods/`) next to an old `BepInEx/plugins/lvalonmima` copy, or with a Workshop subscription plus a manual install, two instances would each keep their own in-memory state and write the same file. The last writer wins. The script itself warns about this. **(inferred)** I don't know if LBoL 1.8.1's Mods loader and BepInEx would both load it.

**P6. No schema version.** Renaming any serialized property (e.g. `MoneyOwned`) would silently reset it through `IgnoreUnmatchedProperties`. `ReconcileWithDefaults` has to list every field by hand (`LiteShop.cs:187-236`); a field left out resets on every load. All current fields are listed.

**P7. Private setters.** `MoneyOwned` and `Items` have private setters. YamlDotNet 11.2.1's deserializer keeps properties where `PropertyInfo.CanWrite` is true, which includes private setters, and writes them with `PropertyInfo.SetValue` (`ReadableAndWritablePropertiesTypeInspector`, `ReadablePropertiesTypeInspector.ReflectionPropertyDescriptor.Write`). So this works today **(inferred for Mono runtime behaviour)**, but it depends on reflection details.

**P8. Stale quest state across runs.** `ResetQuestStateForNewRun` runs only when Challenger is on and only at the first entry station (`QuestEvents.cs:369-380`). It does not clear modifier, rolled or sold flags (`ShopModHandlers.cs:595-602`) or the lite `QuestRolledSlots/QuestSoldSlots`. That is mostly harmless today, because new runs start with empty `ExtraFlags` and the lite fallback picks the run source (`exquesting.Sync.cs:185-190`).

**P9. Other mods.** The dev machine also runs RngFix, LBOLMP and others that hook Save and Restore. Extra `GameRunController.Save`/`Restore` calls multiply C2, C6 and C8 **(inferred)**.

### 3.3 Checked and ruled out

- **`a8d1ce8` (card index generator).** Only `config.Index` changed. For quests, the old value was `(millions+1)*1e7 - 500000 + 1e7 + quest` and the new one is `floor(Sideloader.GetUniqueIndex/1e7)*1e7 + 1e7 - 500000 + quest`, the same block arithmetic (`CardTemplate.cs` `QuestIndex`; `LBoLEntitySideloader.ExtraFunc.CardIndexGenerator.GetOrComputeOffset`). Non-quest indexes changed, but the mod's saved data and the game's save types (`LBoL.Core.SaveData/*`) store card **Ids**, which are class names (`DefaultConfig.cs:13-20`; `toolbox.cs:119-125`). The gun-name change touches only gun configs.
- **Unity `?.`/`??` on `UnityEngine.Object`.** None of these are on the save path. `GameMaster.Instance?.` (30 uses) targets a singleton that is never destroyed. The others are UI-only: `TMP_Settings.defaultFontAsset ??` (`LiteShopButton.ShopPanel.cs:41,276`) and `GetComponent<…>() ?? AddComponent` (`ExquestingPanelController.cs:117`, which is fine in player builds).
- **Thread races.** None. All of this runs on Unity's main thread, and the `UniTask.ContinueWith` callback runs on the player loop.

---

## 4. Recommended debugging

Do steps 1 and 2 first: they turn most of the silent failures above into a log line.

1. **Make failures visible.** In `ShopSaveLoader.Load`'s catch (`:225`) log `LogError` with the reason string, the file length and the exception. Do the same in `LiteProfileSaveData.Restore`'s catch, and raise the existing `LogDebug(e)` in save paths to `LogWarning`. When `_loadFailed` blocks a save (`:98`), log a warning once per session.
2. **Add a one-line trace for every load and save**, with a `reason` argument added to `Save(string reason)` the same way `Load(location)` has one. Log: reason; `CurrentProfile` key; whether the key existed before this call (log inside `GetShopForCurrentProfile` whenever it creates a new shop); `_activeRunProfileKey`; `MoneyOwned`; the sum of tiers; `ChallengerModeEnabled`; file size before and after. Expected result: C1 shows up as "created new shop for key …", C3 as active key ≠ current key, C2 as a reset with reason `GameResultPanel.CustomLocalizationAsync` during run start.
3. **Profile hooks.** Postfix `GameMaster.SetProfileName`, `SelectProfile`, `CreateAndSelectProfile` and `DeleteProfile` to log old key, new key and whether the old key is in `Saves`.
4. **Restore hooks.** Add a Harmony **Finalizer** on `GameRunController.Restore` that logs when an exception went through and the state of `IsGameRunRestoreInProgress`. In `exquesting.OnAdded`, log when an exhibit is deferred.
5. **Log quest-flag diffs.** In `PersistQuestProgress`, log the set of `exquesting.*` flags before and after, and the caller (pass a reason). Also log in the reward coroutine (`ShopModHandlers.cs:450`).
6. **What to ask players for:** `BepInEx/LogOutput.log`, Unity `Player.log`, and a copy of `lvalonmimaShopSave.txt`. The file can be decrypted for inspection with: `openssl enc -d -aes-256-cbc -K 4c76616c6f6e6d696d6153686f704b6579323032345365637265744461746121 -iv 21617461447465726365533432303279 -in lvalonmimaShopSave.txt` (the key and IV from `ShopSaveLoader.cs:25-37`). A key list with an old name next to a new one confirms C1.
7. **Repro scripts:**
   - C1: buy a tier, rename the profile in the profile menu, open the shop. It shows 0 or empty.
   - C3: profile A with Challenger on, start a run, quit to menu; switch to profile B with Challenger on, finish a run, check both balances.
   - C2: put a breakpoint or log in the GameResultPanel postfix and press Continue. It fires before the first station.
   - C4: accept quest 4, finish it, enter the next station (genji is removed), use Restart station from settings. Genji is back and the reward is gone. Compare with quit and Continue.
   - C5/C6: truncate the file to half its size, launch, and check the log, then buy something, restart and check the shop.
   - C8: corrupt `game{i}.sav`, press Continue (the game shows an error dialog), start a new run with Solo Questing. The board never hydrates.
   - P1: copy the save folder without the `.txt` file to emulate a cloud restore.

## 5. Recommended refactoring, ranked by likely impact

1. **Use a stable profile key and migrate (fixes C1).** Key by `CreationTimestamp` only, for example `profile:{CreationTimestamp}`. On lookup, if the key is missing, look for a legacy key starting with `profile:{CreationTimestamp}_` and move it. If there are several, take the one with the highest `MoneyOwned` and the most tiers, and log it. Never create a shop inside a getter that is called before load has finished. This is a small change with a large effect, and it also recovers players who already lost data.
2. **Move the quest-state reset to a real run-end hook (fixes C2).** Keep only the BluePoint string-table injection in the `CustomLocalizationAsync` postfix (`GameRunHistory.cs:196-203`). Move the reset and save into the existing `GameMaster_AppendGameRunHistory_Patch` (`GameRunHistory.cs:222-262`, which runs exactly once per ended run in `SaveProfileWithEndingGameRun`). Clear `QuestRolledSlots/QuestSoldSlots` there too.
3. **One save module with atomic, backed-up writes (fixes C5 and C6).** A single `ModSave` static with:
   - `Load(reason)` called once at startup and again only when the profile changes or the file changed on disk. There should be no reload on `MainMenuPanel.Awake` or `Restore`: the mod file is not per-run, so reloading it there gains nothing and causes P2.
   - `MarkDirty(reason)` plus a debounced `Flush(reason)` at well-defined points (game save postfix, run end, menu actions).
   - Writes go to `file.tmp`, are flushed, then swapped in with `File.Replace(tmp, file, file.bak)`, falling back to delete and move on Mono if `Replace` is unsupported **(inferred)**.
   - On load failure: try `.bak`, then keep the bad file as `file.corrupt-<ts>`, and show a dialog once, the way the game does with `ShowErrorDialogWithKey`.
   - A top-level `SchemaVersion` with explicit migrations, replacing reliance on `IgnoreUnmatchedProperties`.
4. **Fix the active-run profile key (fixes C3).** Clear `_activeRunProfileKey` on `GameMaster.StartGame`, `CoLeaveGameRun`, `SelectProfile` and `RestoreGameRun`, and clear it before the early return at `LiteProfileSaveData.cs:112-113`. Better still, store the owning profile key in a run flag (`lvalonmima.profile:<key>`) so it survives a Continue and is unambiguous.
5. **Make quest state follow the game save (fixes C4, C7, C10, P3).**
   - Postfix `GameMaster.SaveGameRun` with `data.ExtraFlags = new HashSet<string>(data.ExtraFlags)`, so the in-memory snapshot matches what was serialized. That gives Restart station the same rewind as Continue, and disk output is unchanged because serialization has already happened. Expect side effects for other mods that rely on the shared reference **(inferred)**.
   - Treat run flags as the only source of truth for in-run quest state. Drop or narrow the per-profile quest copy, or key it by run (seed plus start time) so it can never feed a different run.
   - On accept or abandon, write the run flags (`writeToRunFlags:true`).
   - Delete the dead `CreateExhibitWidget`/`_pendingRestoreQuestHydration` branch, or wire it up.
6. **Put Restore bookkeeping in a Finalizer (fixes C8).** Clear `IsGameRunRestoreInProgress` and `DeferredRestoreHydration` whatever happens, and skip deferred exhibits whose `GameRun` is not the current one.
7. **Cloud-friendly filename (P1).** Write `lvalonmimaShopSave.sav`, keep reading the legacy `.txt` when the new file is missing, and verify on a second machine that Steam now syncs it. **(inferred)** This assumes the Auto-Cloud rule is `*.sav` at that root.
8. **Bank Blue Points independently of localization (fixes C9).** Move `Save(__result.BluePoint)` and the `BPProgress` reset before the `ids.Count == 0` return in `GameRunHistory.cs:47`, and log when the table is empty.
9. **Re-apply per-stage effects on restore (P4).** In the `Restore` postfix, re-apply `discount.shop`, `battle.seedraw` and `alter.blankcard` the way `alter.wings` already is (`ShopSaveLoader.cs:278-297`), instead of waiting for `StationEntered`.

## 6. Open questions

- Is the LBoL Steam Auto-Cloud rule really `*.sav` at the save root? Check on a second machine, or by seeing whether a test `.sav` file appears in `remotecache.vdf`.
- Do the reporting players rename profiles, use more than one device or a Steam Deck, or use Restart station? Each points to C1, P1 or C4 respectively.
- Can the mod be loaded twice when both a Workshop or `Mods/` copy and an old `BepInEx/plugins/lvalonmima` copy exist (P5)? How does LBoL 1.8.1's Mods loader interact with BepInEx GUID deduplication?
- Is `MainMenuPanel.Awake` guaranteed to run before the first `GameResultPanel` load on every platform? It does in the `LoadMainMenuUiAsync` order, assuming the prefab is active at instantiate. If not, a save before load would overwrite the file with empty data **(inferred, not observed)**.
- The dev machine has a second save root, `<steamId>/alpha/`, with its own `lvalonmimaShopSave.txt` and different profiles. Its origin (a beta branch? a manual copy?) is unknown, and the game code I read never writes there.
- Does `EndGameStatistics` ever run twice for one run, for example through `GameRunFailCheck`? If so, Blue Points would be credited twice. I did not trace this.
