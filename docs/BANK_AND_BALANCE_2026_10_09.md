# DiceFree — Peter Banker and October 9 balance pass

Status: implemented in `T:\TEMP\DiceFree-Options`, branch `feature/54-world-dungeon-codex`; not committed, pushed, or merged. All existing uncommitted dungeon, villagers, equipment art, inventory icon, and prior playtest fixes preserved.

## User-requested changes

- Player scroll-wheel zoom sensitivity now **0.35** (previous **0.16**, about 2.19× stronger) without restoring camera autozoom behind trees; camera distance range 8–110 remains.
- Big Slime miniboss visual scale now **6.9** (previous **2.3**): precisely **3×** former rendered size. This is visual/physical actor scaling; boss HP remains at 800 until user requests another HP change.
- Normal final Slime Boss maximum HP now **3,000**, previously 2,100. The secret Slime Regent was subsequently increased on October 9 from **1,600 to 16,000 HP**.
- Big Slime miniboss and normal Slime Boss Divide fragments have **300 maximum HP each**. Secret Regent Divide fragments scale to **10% of the Regent's max HP**, currently **1,600 HP each**. Other slimes, puzzle slimes and boss adds retain their separate HP rules.
- The offhand Slime Shield uses a dynamic torso-relative position clamp following each humanoid left hand, positioned to the side/forward rather than behind the character; all three class forms have scene bindings.
- The banker and blacksmith 3D NPCs were moved to publicly reachable ground near their workplaces, outside obstructing storehouse chest and forge hearth; capsule colliders, stationary nav obstacles and clickable service approach points retained. The banker is named **Peter Banker**.

## Echo-wide shared bank

The Echo-wide bank uses `EchoSharedBank : IEchoWideDurableState`, persisted as the single optional `echo.shared-bank` section in `ManifestationPersistence`'s atomic Echo save. Item instance IDs and metadata survive; carried inventory/gold stay manifestation-specific. No separate stash or unrelated save file.

At Cornberg, interacting with Peter Banker opens `BankPanel` with:

- Item deposit and withdrawal; equipped items must be explicitly unequipped first, and withdrawals enforce class compatibility.
- Gold deposit, withdrawal and **Deposit All**, all only when physically interacting at the banker. Banked gold is Echo-wide and not carried gold.
- **60** starting item slots, **30** per purchased expansion, and provisionally **100 gold**, then **200 gold**, etc. for subsequent purchases (tuning undecided).
- Item sort by name, text search, semantic searches such as `weapons`, structured terms `slot:`, `type:`, `rarity:`, `class:`, and a visible Regex toggle; invalid/slow regex safely rejected.
- Item inventory icons when available. The HUD Bank button opens the same bank remotely for **item deposits only**; gold banking and item withdrawals remain Cornberg-only.
- Reentrant/double transaction protection and atomic item/wallet changes under `DeferDurableWrites` to avoid duplicate records or intermediate save snapshots. Invalid/missing item catalog references reject transfers without item loss.

## Commands and validation

Use in the connected Unity Editor, stopped Edit Mode:

```powershell
unity command dicefree.bank-balancing.install --project-path T:/TEMP/DiceFree-Options
unity command dicefree.bank-balancing.validate --project-path T:/TEMP/DiceFree-Options
unity command dicefree.bank-balancing.playtest --project-path T:/TEMP/DiceFree-Options
unity command dicefree.bank-balancing.status --project-path T:/TEMP/DiceFree-Options
```

Confirmed on 2026-10-09:
- `dicefree.bank-balancing.install`: **BANK_BALANCING_INSTALLED**, 1 hero and 3 shield bindings.
- `dicefree.bank-balancing.validate`: **BANK_BALANCING_DATA_OK** with 1 hero and 3 shield bindings; banker/blacksmith access, camera tuning and exact balance verified.
- `dicefree.bank-balancing.playtest`: **passed 136 checks**, with an isolated temporary save. Covers banked item/metadata preservation, deposit and withdrawal limits, double-click/reentrancy, banked/carry gold accounting, slot expansion/capacity, corrupted save rejection, class switching, actual scene reload, class restrictions and remote dungeon deposit.
- `dicefree.economy.validate`, `dicefree.village-art.validate`, `dicefree.item-icons.validate`, `dicefree.slime.validate`: all passed after installer. Recompile: completed with no reported errors.
- `dicefree.playtest-fixes.playtest`: **973 checks passed** including new boss fragment requirements.
- Full normal-route `dicefree.slime.playtest`: PASSED, phase 14, fresh isolated save `DiceFree-Slime-0efd692735ff4a79a5150b6f2da35bda`, actual 60-second staging through miniboss, 300-HP Divide fragments, normal 3000-HP boss, loot and return.
- Secret Regent-route `dicefree.slime.playtest --regent true`: PASSED, phase 14, fresh isolated save `DiceFree-Slime-cffc10922b9d483787089fc70f8b525a`, real staging through secret blue puzzle, Regent mechanics, loot and return. **Historical test from BEFORE the follow-up increase to 16,000 HP / 1,600-HP fragments; not evidence of the new balance passing.**
- `dicefree.economy.playtest`: PASSED, `CORNBERG_ECONOMY_PLAYTEST_OK`, 31 checks, fresh isolated save.
- `dicefree.village-art.playtest`: PASSED, 19,301 checks, fresh isolated save (`DiceFree-VillageArt-120f76448a5547668ac4595a266ffea4`) for gear presentation, NPC idle/roaming and no accidental movement.
- All Unity Play Mode runs were performed sequentially after the initial interrupted concurrent command; no failures in these final clear tests.

The bank Play Mode harness originally had a null setup reference to a transient `RunLoadoutLock` after scene reload; corrected and force-recompiled. A separate overlapping test run was invalidated because another command was invoked during Play Mode; subsequent test runs are serialized.

## Manual review

Verify visuals in camera-angle playtesting: shield tilt/clearance against torso and other gear, blacksmith and banker standing on natural ground, larger Big Slime hit/readability, and bank UI at smaller window sizes. The current bank/scene tests use local-authority simulated actors and isolated saves, not complete network transport or multiplayer verification.

Existing unrelated Unity Editor/ProjectSettings dirtiness predates this patch; do not indiscriminately revert it. No source commit/push/merge done.
