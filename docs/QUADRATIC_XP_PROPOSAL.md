# DiceFree quadratic XP curve — design proposal 2026-10-12
Status: **PROPOSAL / DISCUSSION ONLY**. The user likes a quadratic XP requirement because manifestations reset level at each advancement. **No code, asset or balance changes approved yet.**

## Current verified curve
`ExperienceCurve.ToNextLevel(level)` uses `firstThreshold=30` and `thresholdGrowth=10`:
`XP_to_next(L) = 30 + 10 × (L-1)`.
Maximum level is 200. XP accumulation is per manifestation; advancing creates a new level-1 class.

## Two quadratic candidates

**Exact user example:** `XP_to_next(L) = a × L²` (round to integer, min 1).
This is structurally simple but needs a carefully selected coefficient. If `a=30` to preserve 30 XP at level 1, level 10→11 takes **3,000 XP**, versus 120 today. If `a=1.2` to preserve today's level-10 requirement of 120 XP, level 1→2 requires only about **1 XP**. A pure coefficient cannot match both anchors.

**Suggested modified quadratic:** `XP_to_next(L) = round(30 + b × (L-1)²)` with starter `b=1.0`.
This keeps level 1→2 exactly 30 XP; reaches 111 XP at level 10 (vs 120 today); and grows much faster for high-level classes. Its 30 XP base is not tier-specific.

| Current level | Current linear | Pure aL² with a=1.2 | Suggested 30+(L-1)² |
|---:|---:|---:|---:|
| 1 | 30 | 1.2 | 30 |
| 5 | 70 | 30 | 46 |
| 10 | 120 | 120 | 111 |
| 20 | 220 | 480 | 391 |
| 30 | 320 | 1,080 | 871 |
| 60 | 620 | 4,320 | 3,511 |
| 120 | 1,220 | 17,280 | 14,191 |

Cumulative XP from level 1 to reach a cap (no quests/level-reset chain):
| Reach level | Current linear total | Suggested quadratic total |
|---:|---:|---:|
| 10 | 630 | 474 |
| 30 | 4,930 | 8,584 |
| 60 | 18,880 | 68,499 |
| 120 | 73,780 | 558,229 |
| 200 | 202,980 | 2,613,069 |

This quadratic naturally makes later advancements harder because their level *caps* are much higher: Novice L10, Tier I L30, Tier II L60, Tier III L120, Tier IV L200. No tier-dependent `b` is needed unless a fresh level-1 manifestation of a later tier is **also** supposed to have a steeper curve at the same level.

## Important balance ramifications

- Existing Cornberg quests were tested under the original XP curve (e.g. 3 slimes→level 2; starter Novice L10 by the expected play session). Switching formulas breaks exact quest-pacing test assertions; we must retune XP and tests **together**, not just replace a curve formula.
- Current Ironpine rewards are unusually high against either candidate at low levels: Quillrats L12-16 reward 121/129/138/146/154 XP; Grey wolves L14-16 reward 138/146/154; Dark wolves L17-19 reward 163/171/180; Alpha L20 rewards 752. More combat challenge now means more kill time, but quest+kill XP and level-gap rules need review before a quadratic goes live.
- New town Trimery and following quilboar Biome 3 should be tuned to the **fresh Tier I L1→30** journey. The world is not strictly level gated; the user prefers cautious lower-level players to pass along dangerous roads.
- When implementing, preserve migration/rehydration constraints: old stored `CurrentXp` may exceed new per-level thresholds at the same saved level. Decide whether to convert by proportional progress, grant owed levels, or migrate raw XP. Do **not** silently invalidate existing saves.
- Prefer integer XP thresholds and test representative milestones and save/load after curve migration.

## Open decisions for user
1. Formula preference: **pure `a×L²`** despite cheap early levels, or **shifted `30+b×(L-1)²`**? Starting suggestion `b=1`.
2. Early leveling target: should we preserve Novice ~30 minutes to level 10 (existing intended pacing)? The proposed curve's cumulative 474 vs 630 currently shortens the Novice start if XP rewards stay unchanged.
3. Should every class use the same `b` coefficient (higher caps automatically increase time), or should later tiers also have larger coefficients for their early levels?
4. Should higher-level enemies grant full XP to underleveled players, or should level-gap adjustments apply? Requires careful party XP design.

Until these decisions are settled: **do not change code, save format, quest XP, Ironpine rewards or the active `Starter XP curve.asset`**.

## 2026-10-12 — feedback: b=1 still too easy

The user thinks the starter quadratic `30+(L-1)²` still requires too little XP. Stronger shared-coefficient candidates while keeping fresh Novice progression manageable:

| Current level | Existing linear | b=1 | **b=2** | b=3 |
|---:|---:|---:|---:|---:|
| 1 | 30 | 30 | 30 | 30 |
| 10 | 120 | 111 | 192 | 273 |
| 20 | 220 | 391 | 752 | 1,113 |
| 30 | 320 | 871 | 1,712 | 2,553 |
| 60 | 620 | 3,511 | 6,992 | 10,473 |
| 120 | 1,220 | 14,191 | 28,352 | 42,513 |

| Fresh reach | Existing linear cumulative | b=1 cumulative | **b=2 cumulative** | b=3 cumulative |
|---:|---:|---:|---:|---:|
| L10 | 630 | 474 | **678** | 882 |
| L30 | 4,930 | 8,584 | **16,298** | 24,012 |
| L60 | 18,880 | 68,499 | **135,228** | 201,957 |
| L120 | 73,780 | 558,229 | **1,112,888** | 1,667,547 |

**Suggested next candidate: b=2.** This needs only 7.6% more cumulative XP for Novice L10 than today's tested onboarding, but ~3.3x more to Tier I L30, ~7.2x more to Tier II L60, and ~15x to Tier III L120. b=3 is a much harsher alternative, raising early Novice L10 requirement ~40%.

No code or existing XP awards, player saves, quests or thresholds changed. Await user approval and expected level 30/60 playtime targets before selecting curve and migrating saved XP.
