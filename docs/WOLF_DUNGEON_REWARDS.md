# Wolf dungeon rewards — 2026-11-06

The user requested normal dungeon drop rules and clarified six entries: ring, chest armour, healer weapon, mage weapon, thief weapon and acolyte weapon. The previously repeated thief entry is one weapon, not an additional armour drop.

## Normal shared reward flow

After all ten waves, the existing five-second victory summary leads into the reusable treasure room. **Claim rewards** opens it immediately. The shared `DungeonRewardPool` is unchanged: 5% sole rare override; otherwise 75% one normal item / 25% two distinct normal items. The owner chooses one offered item or the XP/gold alternative; all other offers disappear. There are no new per-wolf item drops or automatic completion payouts.

`IDungeonRewardRun` removes the reward room's hard dependency on Slime Dungeon. Slime retains its pool and payouts. The room resolves a choice before publishing inventory/XP callbacks, rejects invalid, foreign and repeat choices, checks gold overflow and finishes each source run independently even if another owner's choice remains. Menu/desktop exit discards an unclaimed choice without a payout and clears ownership. Ordinary wolf kill XP remains unchanged.

The alternative mirrors the existing normal Slime dungeon baseline: **50 XP and 25 gold**, instead of an item. These and the following item values are first-playtest tuning, not a claim of completed full-run class balance.

## Item catalog

All six items are item level 20. Armour and the ring allow every class. Weapons also allow their corresponding current Tier I parent so the drops are usable before those later class advancements. Class design rules remain unchanged: Priest staff, Mage wand, Thief one-handed dagger, Acolyte wand. This pass does not implement new class kits or mentors.

| Item | Drop | Stats | Eligibility |
| --- | --- | --- | --- |
| Moonfang Signet | Rare ring, 5% override | +2 all attributes, +2 physical/magical defense, +1 health regeneration | All classes |
| Nightpelt Vest | Normal chest armour | +6 VIT, +2 STR, +6 physical defense, +2 magical defense | All classes |
| Moonwell Staff | Normal two-handed healer weapon | +8 SPR, +2 VIT, +0.4 health regeneration | Magically Touched Novice, Priest |
| Starfall Wand | Normal one-handed mage weapon | +7 INT, +2 VIT, +3 basic attack damage | Magically Touched Novice, Mage |
| Packhunter's Fang | Normal one-handed thief dagger | +7 AGI, +2 STR, +6% attack speed, +3 basic attack damage | Physically Blessed Novice, Thief |
| Duskwake Wand | Normal one-handed acolyte weapon | +5 INT, +4 SPR, +2 basic attack damage | Magically Touched Novice, Acolyte |

Definitions live under `Resources/DiceFree/DungeonRewards/Wolf`. Inventory registration includes these resources before restoring saved ownership, so an awarded item still resolves after a fresh scene load or restart. Existing inventory definitions and potion registration remain intact. Weapon stance follows the existing one-/two-handed equipment rule; this pass provides inventory icons and stats and reuses the existing class weapon presentation.

## Presentation and provenance

Each item has a distinct original 512px icon rendered from reproducible Unity geometry. The locally downloaded Asset Store catalog was checked first; no new vendor source or paid-provider output is admitted. The existing first-party city materials are reused in the shared treasure room: fitted stone, a blue runner, bronze trim, lit columns and storage props. The original room's ground collider, navigation and scene identities are preserved. Decorative geometry is on the existing nonblocking art layer.

Reward previews show icons and names; hovering displays stats and class eligibility. The corresponding physical chests carry matching numbers. Claims remain owner-only world interactions. `WolfRewardsAuthoring.cs` authors the six definitions/pool/icons; `DungeonTreasuryAuthoring.cs` preserves a recovery of the original reward scene before furnishing it. The first icon render exposed an asynchronous shader warm-up issue; synchronous material creation and corrected front lighting produce the final icons.

## Validation

`WolfRewardsReview.cs` uses an isolated profile, tests 20,000 pool rolls and actual equipment eligibility, and completes three ten-wave fixtures for item claim, XP/gold claim and an unclaimed menu exit. It checks private ownership, invalid/reentrant choices, saved item resolution, re-entry, the original Slime Dungeon's reward flow and independent source-run completion. Lifecycle fixtures use explicit defeat packets and a direct Slime puzzle completion fixture; these do not demonstrate a naturally balanced full clear.

The first re-entry fixture was corrected to wait for the reusable loading overlay to close. The independent-run fixture refreshes the live pool after scene transitions because Unity may unload the old native asset instance. Earlier failed fixture reports are retained under `Logs`.

`WindowsWolfRewardsSmoke.cs` repeats the critical paths in a fresh standalone profile, including a natural wolf bite. Existing Windows wolf/world smoke scripts now claim their completion reward before expecting a return to the entrance.

Editor acceptance passed **21,694 checks**, including 20,000 rolls (992 rare overrides; 4,816 two-item normal offers), all class eligibility cases and all reward/persistence/cleanup paths, with zero Unity errors. Architecture, managed references, source ownership and dependency policy passed for 11 first-party assemblies.

Final Windows build: `Builds/BriarbackMilestone-20261106-172739/DiceFree.exe`, eight scenes, 474,771,397 bytes, **zero build errors and warnings**. A fresh-profile standalone pass completed three ten-wave runs, observed a natural wolf bite and passed item/bonus exclusivity, owner-only claims, inventory and XP callback reentrancy, gold-overflow rejection without partial payout, saved item resolution, unclaimed menu cleanup, re-entry and the original Slime reward flow. Zero Unity errors were observed. Normal Quit exited PID 24604 with code 0. An earlier passing build is retained; the final build also corrects the old 35 m label cutoff so matching chest numbers remain visible at the gameplay camera.

Final captures and consolidated evidence are under `VisualBaselines/WolfRewards`, with standalone images prefixed `windows-` and results in `validation.json`. All 23 original reward-scene object IDs, its ground collider/NavMesh, all 54 pre-existing modified files and seven protected settings are preserved. Cornberg is unchanged. Unity's old scene recovery remains at `Assets/_Recovery/0 (1).unity` with a verified copy in `Logs`; it is not added to the focused commit.

Work remains on `feature/wolf-dungeon-rewards`; no push or merge is included.
