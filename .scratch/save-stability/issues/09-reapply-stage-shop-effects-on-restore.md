# 09: Re-apply per-stage shop effects on restore

**What to build:** After a Continue, the shop's per-stage effects (the shop discount, seeing draw order, the blank-card colour limit) work immediately instead of only after the next station is entered. They aren't stored in the game save, so the restore hook re-applies them the same way it already re-adds "Gives You Wings", using the same values the station-enter path would. The gap is inferred (research.md P4), so the checklist must first confirm the effect is missing after a mid-battle or adventure Continue on the current code. See research.md section 5 item 9.

**Blocked by:** 07

**Status:** ready-for-agent

- [ ] Restore re-applies the three per-stage effects, gated the same way station entry gates them (Challenger on, tier owned)
- [ ] Entering the next station afterwards doesn't apply them twice
- [ ] Compiles with the command in the README
- [ ] In-game checklist appended (with each effect owned: save mid-battle or mid-adventure, quit, Continue, check the effect is active before the next station)
