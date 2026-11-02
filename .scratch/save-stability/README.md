# Save stability

Players report that the Roguelite Shop and quest data resets and acts unstable. The causes are traced in [research.md](research.md); tickets cite its failure-mode ids (C1–C10 confirmed, P1–P9 plausible) and its section 5 refactor list.

Integration branch: `fix/save-stability` (from `master`).

## Rules for every ticket

- **Existing saves must port over.** A shop file written by 0.2.12 (unversioned, `.txt`, name-based profile keys) must load into the new code with no loss of money, tiers, Challenger toggle, history labels or quest backup. Never delete or overwrite the legacy file/keys until the migrated data has been written successfully. Each ticket that changes the file's shape or location says how its migration is checked.
- **Verify by compiling, not by building.** `dotnet build` runs `makezip.sh`, which installs into the shared game `Mods/` folder; don't do that from a worktree. Use:
  `dotnet msbuild lvalonmima.csproj -restore -t:Compile -p:Configuration=Release -v:minimal`
  It must finish with no errors and no new warnings.
- **No test project exists** and the game can't be run here. Each ticket instead ends with an **in-game checklist** (append it to the ticket file under `## In-game checklist`) that the user will run: steps, expected result.
- **Style:** match the surrounding code. No braces around single-statement `if`/loop bodies. Don't use `?.`/`??` on `UnityEngine.Object` values (it bypasses Unity's null check).
- Log through the plugin's BepInEx logger. Failures that lose or skip data are `LogError`/`LogWarning`; traces are `LogInfo` behind a single static toggle so they can be silenced later.
