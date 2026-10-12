# DiceFree XP ladder audit — 2026-10-12

> **Historical audit, before quadratic XP implementation.** The live feature branch now uses `30 + 3 × (Level−1)²` with schema-v6 save migration; see [the new implementation and balance audit](QUADRATIC_XP_IMPLEMENTATION.md). Figures below document the *previous* linear curve.


**Status:** Read-only balance audit. No XP curve or XP rewards changed in this combat/UI patch.

## Current source of truth

`Assets/_DiceFree/Scripts/Progression/ExperienceCurve.cs` and
`Assets/_DiceFree/Settings/Progression/Starter XP curve.asset`:

- `firstThreshold = 30`
- `thresholdGrowth = 10`
- `levelLimit = 200`

Per-level XP requirement: **30 + 10 × (currentLevel - 1)**. Total XP accumulated from fresh level 1 to arrive at level N:
**(N - 1) × (5N + 20)** (no bonuses/advancement reset). The cap is level 200.

| Current level | XP to next level | Cumulative XP to reach current level |
|---:|---:|---:|
| 1 | 30 | 0 |
| 2 | 40 | 30 |
| 5 | 70 | 180 |
| 9 | 110 | 520 |
| 10 | 120 | 630 |
| 15 | 170 | 1,330 |
| 20 | 220 | 2,280 |
| 29 | 310 | 4,620 |
| 30 | 320 | 4,930 |
| 60 | 620 | 18,880 |
| 120 | 1,220 | 73,780 |
| 200 | cap | 202,980 |

**Advancement does not continue the same XP progress:** Novice L10 -> new Tier I L1; Tier I L30 -> new Tier II L1; later planned thresholds Tier II L60, Tier III L120, Tier IV L200. Previous manifestations are retained, but new manifestation starts at level 1.

`ExperienceProgression.Grant` pools kill/quest XP and can level up several times on a single grant. `KillCreditReceiver` forwards the credited victim's authored `experienceReward`; the XP threshold code has **no recipient/defeated-level penalty**.

## Ironpine wildlife XP amounts (read from authored ActorDefinition assets)

- **Quillrats L12–16:** 121, 129, 138, 146, 154 XP.
- **Grey Wolves L14–16:** 138, 146, 154 XP.
- **Dark Wolves L17–19:** 163, 171, 180 XP.
- **Alpha Wolf L20:** 752 XP (4x level-20 ordinary wolf XP per prior design; no item loot).

Contrast: a level-10 Tier I character needs 120 XP to reach level 11; a single quillrat or grey wolf already gives more than that. Reaching level 30 from a fresh level 1 requires just **4,930 XP**, roughly 30–40 ordinary Ironpine kills at current award amounts if every kill could be farmed; this excludes quest XP and assumes a powerful enough player to beat this dangerous wildlife. At level 1 the same large kill grant may jump multiple levels. This creates potential progression-speed distortion.

The wildlife's new **2x wolf HP and +50% damage** do NOT change kill XP, respawn timers, Codex, pack sizes or item drops. We should separately playtest kill time and revise XP rewards/curve when the Tier I -> Tier II / Village 2 path is designed.

## Open balance decisions

1. Is the desired Tier I level 1–30 journey about a few hours, or notably longer?
2. Should significantly higher-level enemy kills retain full XP (rewarding brave low-level kills) or have level-gap XP adjustment?
3. Should Village 2 be reached around Tier I L15–20, or can it be entered earlier by cautious low-level characters as planned for Ironpine?

No XP code was adjusted by this audit.
