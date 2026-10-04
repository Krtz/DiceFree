# Novice level 1–10 pacing audit

**Status:** deterministic progression math is verified from the current post-#39 `poc/cornberg` data. A normal human-playthrough duration is not verified; a real timed playtest remains necessary before changing balance.

## Current curve

`Assets/_DiceFree/Settings/Progression/Starter XP curve.asset` sets `firstThreshold = 30` and `thresholdGrowth = 10`. `ExperienceCurve.ToNextLevel(level)` therefore requires `30 + 10 × (level − 1)` XP at the current level. XP carries across level-ups.

| Transition | XP to level | Cumulative XP from level 1 |
|---|---:|---:|
| 1 → 2 | 30 | 30 |
| 2 → 3 | 40 | 70 |
| 3 → 4 | 50 | 120 |
| 4 → 5 | 60 | 180 |
| 5 → 6 | 70 | 250 |
| 6 → 7 | 80 | 330 |
| 7 → 8 | 90 | 420 |
| 8 → 9 | 100 | 520 |
| 9 → 10 | 110 | **630** |

## Deterministic current Cornberg progression

Values below assume a fresh level-1 Novice, all required quest kills, and no additional kills except where shown. Q1–Q3 requirements/rewards are taken from their authored quest and enemy definitions; Q4 values are from `docs/CORNBERG_Q4.md` and the corresponding definitions.

| Milestone | XP added at milestone | Cumulative XP | Result |
|---|---:|---:|---|
| Q1: five Crop Slimes (5 × 10) + 20 turn-in | 70 | 70 | Level 3, 0 XP |
| Q2: three Road Slimes (3 × 20) + 50 turn-in | 110 | 180 | Level 5, 0 XP |
| Q3: one Named Forest Slime (35) + 35 turn-in | 70 | 250 | Level 6, 0 XP |
| Runner Returns / MSQ5 availability | 0 | 250 | Level 6, 0 XP |
| Q4 elite route: one Elite Slime (80) + 200 turn-in | 280 | 530 | Level 9, 10 XP |
| Q4 elite route plus five post-accept Road Slimes (5 × 20) | 380 total for Q4 | 630 | Level 10, 0 XP |
| Q4 population route using 30 Road Slimes (30 × 20) + 200 turn-in | 800 total for Q4 | 1,050 | Level 13, 30 XP |

The Q4 elite route is 100 XP short of level 10 by itself. Five eligible Road Slime kills while Q4 is active provide exactly those 100 XP and can accompany the elite route. The authored 30-Road-Slime alternative yields three levels beyond the Q4 level-9–10 target. This result was already recorded in `docs/CORNBERG_Q4.md`; this audit consolidates it with the full Q1–Q5 baseline. Other eligible enemy mixes can grant more or less XP because Road, Named Forest and Elite Slimes have different authored rewards (20/35/80 XP).

Novice reaches level 10 at 630 total XP. MSQ5 currently grants zero XP and has no level-10 gate, so it preserves whichever level the player reached through Q4 and any optional combat. The first blessing opportunity is canonically tied to level 10 and the return-to-mountain call; this audit does not change that design.

## Time evidence

The XP values and resulting levels above are deterministic. They do not establish real minutes. Q1 and Q2 each use a single authored enemy with a 10-game-second respawn; completing five Q1 and three Q2 kills entails up to four and two respawn intervals respectively when fought serially against those single spawns. Game seconds, combat animation/decision time, travel, quest conversations, the Q4 search, death/recovery, and player exploration do not have a validated conversion to human-playtime minutes. Q4's elite has a 450-second authored respawn, but its route requires one defeat, so that timer is not a normal-route duration.

No human timed playthrough establishing the canonical roughly-30-minute target (or ruling out an hour) is recorded in the validation evidence reviewed here. The pacing target therefore remains **unverified by elapsed-time playtest**.

## Balance conclusion

No balance values were changed. The current data deterministically reaches level 6 at Q3, level 9 after Q4's elite route alone, and level 13 after the 30-Road-Slime route. The latter conflicts with the desired level-9–10 Q4 endpoint if treated as a typical path, but it is an explicit extended alternative and its over-level result is already documented as provisional tuning evidence. There is not yet a real human-playtime sample to choose whether to change the 30-kill count, XP rewards, or nothing. Keep all XP, quest requirements, and the optional level-200 path unchanged until playtesting distinguishes whether this extended route is being overused and whether the elite/search route reaches the first advancement opportunity within the desired time.

## Data sources

- `Assets/_DiceFree/Settings/Progression/Starter XP curve.asset`
- `Assets/_DiceFree/Settings/Progression/Cornberg crop Slimes.asset`
- `Assets/_DiceFree/Settings/Combat/Crop Slime combat stats.asset`
- `Assets/_DiceFree/Settings/Enemies/Cornberg road investigation.asset`
- `Assets/_DiceFree/Settings/Enemies/Road Slime combat stats.asset`
- `Assets/_DiceFree/Settings/Enemies/Cornberg named forest Slime.asset`
- `Assets/_DiceFree/Settings/Enemies/Named Forest Slime combat stats.asset`
- `Assets/_DiceFree/Settings/Enemies/Cornberg break Slime surge.asset`
- `Assets/_DiceFree/Settings/Enemies/Elite Forest Slime combat stats.asset`
- `docs/classes/NOVICE.md` (roughly 30-minute level-10 target)
- `docs/WORLD_1.md` (Q4 endpoint around level 9–10; normal advancement at level 10)
- `docs/CORNBERG_QUESTS.md`, `docs/CORNBERG_ROAD.md`, `docs/CORNBERG_FOREST.md`, `docs/CORNBERG_Q4.md`, and `docs/CORNBERG_MSQ5.md`
