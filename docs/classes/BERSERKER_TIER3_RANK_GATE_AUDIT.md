# Berserker Tier III — proposed full 150-rank gate audit (Round 14, 2026-11-26)

**STATUS: DESIGNER-PROPOSED / NOT YET USER APPROVED.** The level-1 first-unlock requirements, rank caps, and total 150 skill points are USER APPROVED. This file supplies one mathematically verified candidate progression that must still be signed off by the user before implementation. No Unity assets or gameplay changed.

## User-approved fixed constraints

- Exactly **five Berserker rankable skills** totaling **150 ranks** and **1 skill point per Berserker class level, levels 1–150** (not one free rank for each starting skill).
- First available: **Wide Arc at level 1, Anger Management at level 1, Spin Dash at level 3, Rage Breaker at level 6, Coffee Shield at level 8**.
- Final caps: **Wide Arc35, Spin Dash30, Rage Breaker30, Anger Management25, Coffee Shield30**.
- Follow the global rank-0 convention, bounded allocation budget `min(classLevel,150)`, skill-specific rank gates, and upgrade notification only when at least one legal rank purchase is available.
- Aim for different per-ability upgrade intervals, but assure that every class level from 1 to 150 can spend its earned point in some eligible skill.

## Proposed exact rank eligibility levels (each cell lists class levels for skill ranks 1 through its cap)

| Skill | Cap | First | Last | Class-level gates for ranks 1..cap |
| --- | ---: | ---: | ---: | --- |
| Wide Arc | 35 | 1 | 128 | 1, 2, 3, 4, 8, 12, 16, 20, 24, 28, 32, 36, 40, 44, 48, 52, 56, 60, 64, 68, 72, 76, 80, 84, 88, 92, 96, 100, 104, 108, 112, 116, 120, 124, 128 |
| Spin Dash | 30 | 3 | 148 | 3, 8, 13, 18, 23, 28, 33, 38, 43, 48, 53, 58, 63, 68, 73, 78, 83, 88, 93, 98, 103, 108, 113, 118, 123, 128, 133, 138, 143, 148 |
| Rage Breaker | 30 | 6 | 122 | 6, 10, 14, 18, 22, 26, 30, 34, 38, 42, 46, 50, 54, 58, 62, 66, 70, 74, 78, 82, 86, 90, 94, 98, 102, 106, 110, 114, 118, 122 |
| Anger Management | 25 | 1 | 145 | 1, 7, 13, 19, 25, 31, 37, 43, 49, 55, 61, 67, 73, 79, 85, 91, 97, 103, 109, 115, 121, 127, 133, 139, 145 |
| Coffee Shield | 30 | 8 | 124 | 8, 12, 16, 20, 24, 28, 32, 36, 40, 44, 48, 52, 56, 60, 64, 68, 72, 76, 80, 84, 88, 92, 96, 100, 104, 108, 112, 116, 120, 124 |

### Compact implementation candidate

Use `r` as the target purchased skill rank (one-based). These are per-skill eligibility gates, **NOT mandatory purchases at that level**, so players can spend points in any currently eligible and not-yet-maxed skill.

- **Wide Arc**: ranks 1–4 unlock at class levels 1,2,3,4; ranks 5–35 at `4+4×(r−4)` (rank35 level128). This deliberately frontloads the early cleave so levels 2–5 remain spendable under staggered other unlocks, then transitions to four-level cadence.
- **Spin Dash**: ranks 1–30 at `3+5×(r−1)`, ending level148.
- **Rage Breaker**: ranks 1–30 at `6+4×(r−1)`, ending level122.
- **Anger Management**: ranks 1–25 at `1+6×(r−1)`, ending level145.
- **Coffee Shield**: ranks 1–30 at `8+4×(r−1)`, ending level124.

## Exhaustive numeric audit: all class levels 1..150

For each level `L`, `availableRanks(L)=sum_over_skills(count(gate <= L))`; `earnedBudget(L)=min(L,150)`. Validation checked **all 150 levels** and confirmed `availableRanks(L) >= earnedBudget(L)` in every case. This proves a player can keep spending one rank every level using some legal allocation (and may instead bank points). All skill caps reachable by level150, with **all 150 gates released already by level148**; the last class levels spend their remaining earned points on already-eligible ranks.

- **Total rank gates:** 150 / 150, matching approved caps.
- **Failed levels:** 0.
- **Smallest eligibility surplus:** 0, at level150.
- **Largest eligibility surplus:** 16 ranks.
- **Staggered first unlock levels:** 1, 1, 3, 6, 8 (matches all approved first unlocks).

| Berserker level | Available individual rank gates | Surplus versus earned class points |
| ---: | ---: | ---: |
| 1 | 2 | 1 |
| 2 | 3 | 1 |
| 3 | 5 | 2 |
| 4 | 6 | 2 |
| 5 | 6 | 1 |
| 6 | 7 | 1 |
| 8 | 11 | 3 |
| 20 | 24 | 4 |
| 60 | 68 | 8 |
| 100 | 113 | 13 |
| 128 | 143 | 15 |
| 145 | 149 | 4 |
| 148 | 150 | 2 |
| 150 | 150 | 0 |

**Important scope:** This mathematical audit tests gate counts, per-skill rank sequencing and spendability. It does **not** test gameplay pacing quality, optimal build allocations, Rage income, class DPS, Unity compilation or runtime. Schedule is a designer first-playtest candidate for user approval.

## Crit clarification from user (same conversation)

**Berserker has NO BUILT-IN CRITICAL-HIT CHANCE from its class baseline or passive kit.** Any actual crit chance must come from **gear, equipment affixes, external buffs, consumables or another externally granted source**; do not invent flat innate crit chance or extra Anger Management crit. **Rage Breaker CAN crit only if such externally supplied crit chance is present** and its global critical-roll succeeds, while preserving guaranteed ordinary accuracy on valid targets. **Wide Arc and Spin Dash NEVER crit**, including with gear/external crit chance. Standard basic attacks should use whatever externally acquired crit chance the global rules allow, not an invented Berserker-only bonus.

## Outstanding questions

1. Approve or adjust this proposed level-gate schedule, especially Wide Arc's ranks 1–4 rapid start and later four-level intervals.
2. Decide any later unlock milestones for special effects, if desired, separate from raw damage coefficients.
3. After gate approval, draft advancement quest/teacher and audit interactions with class level 1 manifested stats and point refunds.
