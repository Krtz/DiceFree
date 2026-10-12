# Quadratic XP v1 — implementation and balance audit (2026-10-13)

**Implemented on:** `feature/visual-overhaul-vis00`. **Unity editor validation is pending** because the Windows Remote Desktop Commander connection stopped responding during implementation. No real player save has been read or modified by this work.

## Live progression model

Every manifestation now uses `XP_to_next(L) = 30 + 3 × (L−1)²`, where `L` is the current level. Same progression curve for Novice and all tiers, capped at level 200. Advancing to another manifestation still starts that class at level 1.

| Current level | New XP to next level | Previous linear XP |
|---:|---:|---:|
| 1 | 30 | 30 |
| 2 | 33 | 40 |
| 5 | 78 | 70 |
| 10 | 273 | 120 |
| 20 | 1,113 | 220 |
| 30 | 2,553 | 320 |
| 60 | 10,473 | 620 |
| 120 | 42,513 | 1,220 |

| Level reached from fresh level 1 | New total XP | Previous total XP |
|---:|---:|---:|
| 10 | 882 | 630 |
| 30 | 24,012 | 4,930 |
| 60 | 201,957 | 18,880 |
| 120 | 1,667,547 | 73,780 |
| 200 | 7,827,267 | 202,980 |

No changes to level-up stats, class XP reset, combat, loot, or level cap.

## Save preservation and migration

- Bumps Echo schema and manifestation sections **v5 → v6**. Supports all earlier schema v1–v5 records through existing migration paths.
- Every saved manifestation (including inactive Physically Blessed/Magically Touched/etc.) keeps its **exact level, identity, bank/gear, gold, quest and skill states**.
- Legacy save XP is the amount earned *within the current level*. Conversion maintains the nearest whole-XP equivalent of the **same fraction toward the next level**, using immutable historical threshold `30 + 10 × (L−1)`. It never awards levels and clamps below the new next-level threshold. Level 200 retains 0 XP.
- Example: old level 20, 110/220 XP (50%) becomes level 20, approximately 557/1,113 XP (50%).
- `LocalEchoStore.Load` validates the original Echo checksum before first migration and copies the original envelope unchanged to `primary-echo.json.pre-quadratic-original`. Normal rotating save backups are still retained. The extra copy is not overwritten.
- Upgrade is performed on an in-memory clone; invalid legacy XP fails migration instead of silently rewriting the original.
- Existing save path/revision, all unrelated Echo-wide sections, and inactive manifestation history remain intact. New saves use v6.

## Cornberg early-game rewards and milestones

The full leveling change is deliberately **harder**, but keeps the opening quest chain recognizable. Award changes are minimal:

| Event (fresh baseline as noted) | XP earned | Quadratic result |
|---|---:|---|
| First 3 Crop Slimes | 3 × 10 = 30 | Reach L2, 0/33 |
| Next 2 Crop Slimes + Q1 turn-in | 2 × 10 + 20 = 40 | Reach L3, 7/42 |
| Three Road Slimes + Q2 (starting at L3, 0 XP, isolated fixture) | 3 × 20 + 50 = 110 | Reach L5, 11/78 |
| Q1→Q2 continuously (starting Q2 at L3, 7 XP) | 110 | Reach L5, 18/78 |
| Named Forest Slime + Q3 (starting L5, 0 XP) | 35 + **43** | Reach L6, 0/105 |
| Q4 elite path (starting L6, 0 XP) | 80 + 200 | Reach L8, 37/177 |
| Q4 mass path (starting L6, 0 XP) | 30 × 20 + 200 | Reach L10, 158/273 |

**Only authored reward adjustment:** Q3 Named Forest Slime **turn-in XP 35 → 43**, keeping its intended level-six milestone. Enemy kill XP remains 35. Q1/Q2/Q4 enemy and quest rewards are unchanged.

The old automated Q1, Q2, Q4 regression fixture expectations are adjusted to their new exact level/XP remainders. Q3 continues to expect L6/0 with its recalibrated reward. No XP added to the Runner dialogue.

### Pacing concerns that need playtesting

- The **Novice level-10 total is 882 instead of 630 (+40%)**. The original goal was about 30 minutes to level 10. Do not declare this pacing goal met until measured in gameplay. Q4's 30-kill path still reaches level 10, but may take too long compared with the elite alternative.
- Near the end of Novice, per-level XP rises faster than ordinary slime kill XP. Future Cornberg quest reward tuning should be based on full-route playtest timings, not arbitrarily multiplying all enemy XP.
- The early Q2/Q3 rewards were tested under a previous linear curve; preserving milestone *levels* is not proof that quest difficulty or time-to-complete is right.
- No level-gap penalty exists yet. Killing more difficult enemies above a character's level gives full XP, subject to actual combat difficulty.

## Ironpine (unchanged XP rewards, now proportionally harder progression)

| Enemy | Level | XP per kill |
|---|---:|---:|
| Quillrats | 12–16 | 121–154 |
| Grey Wolves | 14–16 | 138–154 |
| Dark Wolves | 17–19 | 163–180 |
| Alpha Wolf | 20 | 752 |

At L15 the next threshold is **618 XP** (a 146-XP wolf is ~24% of a level). At L20, 180 XP is ~16% of 1,113. At L30, 180 XP is ~7% of 2,553. Higher-tier characters naturally outgrow forest XP farming.

**Balance conclusion:** don't automatically increase Ironpine XP until wolf kill-time is measured with the recently doubled HP and 50% damage buff. If Tier I L20–30 feels too slow, prioritize Trimery/Biome 3 quests and level-appropriate enemies over repeatedly killing low-level wolves.

## Code, migration, and validation

- `Assets/_DiceFree/Scripts/Progression/ExperienceCurve.cs`
- `Assets/_DiceFree/Settings/Progression/Starter XP curve.asset`
- `Assets/_DiceFree/Scripts/Persistence/SaveMigrations.cs`
- `Assets/_DiceFree/Scripts/Persistence/LocalEchoStore.cs`
- `Assets/_DiceFree/Scripts/Tools/Editor/QuadraticXpMigrationValidation.cs`
- Updated `CornbergQuestValidation`, `CornbergRoadValidation`, `CornbergSurgeValidation`, `NoviceSkillsValidation` expectations and Q3 quest definition/source.

Pure arithmetic checks confirmed all five cumulative level milestones and the seven Cornberg scenarios above. The new Unity Editor validator is designed to assert the actual curve asset, save migration across four manifestations, opaque save section integrity and a full authored XP reward report.

**Outstanding before considering this playtest-ready:** re-establish Remote Desktop Commander access; run Unity `dicefree.xp.quadratic.validate`, the Q1/Q2/Q3/Q4 and persistence Play Mode regression tests in a fresh isolated-save project, and visually inspect the first 30 minutes of leveling. Never run Play Mode regression against the user's active Echo profile. No change to `main` without approval.
