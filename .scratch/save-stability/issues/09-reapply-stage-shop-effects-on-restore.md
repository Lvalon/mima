# 09: Re-apply per-stage shop effects on restore

**What to build:** After a Continue, the shop's per-stage effects (the shop discount, seeing draw order, the blank-card colour limit) work immediately instead of only after the next station is entered. They aren't stored in the game save, so the restore hook re-applies them the same way it already re-adds "Gives You Wings", using the same values the station-enter path would. The gap is inferred (research.md P4), so the checklist must first confirm the effect is missing after a mid-battle or adventure Continue on the current code. See research.md section 5 item 9.

**Blocked by:** 07

**Status:** ready-for-agent

- [x] Restore re-applies the three per-stage effects, gated the same way station entry gates them (Challenger on, tier owned)
- [x] Entering the next station afterwards doesn't apply them twice
- [x] Compiles with the command in the README
- [x] In-game checklist appended (with each effect owned: save mid-battle or mid-adventure, quit, Continue, check the effect is active before the next station)

## Implementation notes

Checked against the decompiled 1.8.1 code: `GameRunController.Save`/`Restore` don't touch `ShopPriceMultiplier`, `CanViewDrawZoneActualOrder` or `RewardAndShopCardColorLimitFlag`, so a restored run starts at the defaults (1, 0, 0). For `BattleFinish`/`AfterBossReward` and `Adventure` timings, neither `Restore` nor `GameMaster`'s resume coroutines call `GameRunController.EnterStation` (`SystemBoard.EnterStation` only updates the UI), so `StationEntered` doesn't fire until the next node. An `EnterMapNode` save, which is also what a mid-battle Continue loads (the game doesn't save during battle), goes through `CoEnterMapNode` and does fire it.

The Restore postfix now calls `ShopModHandlers.ReapplyPerStageEffectsOnRestore(__result)`. It records the restored run as the cached run, then applies the effects through the same code as station entry, gated the same way (Challenger on, tier > 0). The next `StationEntered` sees the same run, undoes the recorded values and applies them again, so they don't stack. The undo now also resets the recorded values to neutral, so a station where the shop is missing or Challenger is off no longer leaves stale values that the next station would undo a second time.

## In-game checklist

With the BepInEx console or `LogOutput.log` open. Use a profile with Challenger on and these owned: `discount.shop` (tier N), `battle.seedraw` and `alter.blankcard`.

1. **Confirm the gap on the old build first** (0.2.12 or `fix/save-stability` before this ticket): win a battle so the reward screen shows, quit to the main menu, Continue. Go to a shop (or check the next card reward). Expected on the old build: shop prices are full price, and card rewards include off-colour cards until you enter the next node. Do the same in the middle of an event (Adventure). If the effect is already there on the old build, stop and report back.
2. **After-battle Continue, new build:** win a battle, quit on the reward screen, Continue. Expected: the log shows `[ShopSave] Restore re-applied per-stage effects: shop price x<1 - 0.05*N>, see draw order +<tier>, blank card +1`. Card rewards on that screen are limited to your colours (blank card).
3. **Adventure Continue:** quit in the middle of an event, Continue. Expected: the same log line. If the event offers a shop or cards, prices are discounted and cards are limited to your colours.
4. **No double application:** after step 2 or 3, go to the next node, then to a shop. Expected: the price is discounted once (for example tier 2 means 90% of normal, not 81%). In the next battle, the draw pile order view works as it did before quitting, and card rewards are still colour-limited. Then go one more node and check the shop price is unchanged.
5. **Battle Continue (EnterMapNode save):** quit during a battle, Continue. Expected: the battle restarts, the log line shows, the draw order view works, and the next shop price is discounted once.
6. **Gate:** turn Challenger off (or use a profile with none of the three items), start a run, win a battle, quit, Continue. Expected: no `re-applied per-stage effects` line, and shop prices are normal.
7. **New run after a restored one:** abandon the restored run and start a new one with Challenger on. Expected: the first shop is discounted once, not twice.
