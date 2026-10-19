# Greatblade — Tier II prototype v0.3 (user-approved kit, cadence and 3D asset pass)

> **Later user skill-progression decisions (2026-10-19):** original Greatblade skill **first availability levels** are confirmed, but this historical design's suggestion of **four free level-1 skills is superseded** by the global **rank-0, one-point-at-level-1** rule. Per-ability upgrade intervals are now approved in principle: some skills allow every-level investment, others must wait multiple class levels. Weighted maximum ranks should allow near-complete spend-down by level 80. **Greatsword Mastery is specifically under review for fewer ranks OR major effect milestones separated by small flat-stat bonus ranks.** Exact Greatblade caps and rank schedules remain to be approved. Existing saves will be **automatically and silently reset/refunded on first legacy class load** in a safe, once-per-manifestation migration (no notification). See [Greatblade/Mage skill-rank audit](TIER2_GREATBLADE_MAGE_SKILL_RANK_AUDIT_2026_10_19.md). Do not confuse as-built prototype behavior with finalized redesigned progression.


**2026-10-13 — gameplay design locked where noted, balance and rank dates provisional.**
Supersedes Greatblade entries in `TIER2_CORE_CLASSES_DESIGN_V01.md` and `V02.md`.

## Confirmed identity and gameplay

- **Greatblade**, Tier II from level-30 Physically Blessed Novice. Heavy two-handed **greatsword** melee damage dealer, not Defender.
- Retired **swordswoman** in Cornberg uses the **exact same underlying Greatblade base model** as the player class; equipped cosmetics/gear can distinguish her later. Do not create a different NPC base body merely to make her seem special.
- **Simple Momentum**. Successfully hitting with weapons / appropriate skills builds Momentum; powerful **Overhead Breaker** consumes it. No 25/50/75% threshold damage buffs, no compulsory hit-chain penalty. Heavy Rhythm is a **separate** passive improving gain/decay and possibly maximum Momentum.
- **Wide Arc**, **Spin Dash** and **Heavy Rhythm** available **at level 1**. The evolving **Greatsword Mastery** passive also begins level 1. **Overhead Breaker first available level 6**. **Battle Cry** remains a later active.
- **Spin Dash replaces Driving Charge**, and targets a **ground location**, rapidly spinning/moving there while applying AoE damage along the traversed path (not an enemy-targeted charge). Follow actual terrain, collision/NavMesh and no-unit-overlap; cannot spin through walls, solid trees, buildings, or inaccessible terrain.
- Separate previous passive ideas `Greatsword Mastery`, `Executioner's Poise` and similar scaling-stat fillers are folded into ONE **evolving Greatsword Mastery** passive. Earlier bonuses scale up with every rank; new mechanics appear at specific ranks.
- One-class-one-fixed-four-active-one-passive constraint rejected. Slots 1–8 are mixed actives/passives. **Slot 9 exclusively for Tier IV+ quest-unlocked ultimate**, never for Greatblade Tier II; slots 10–12 shared Attack / Interact / Return.
- Skills can unlock at different levels and have different rank-up breakpoints. Do not permit generic "spend any point every level" ranking.

## Skillbar layout and proposed progression

| Slot | Skill | Type | Unlock | Example additional rank breakpoints |
|---|---|---|---:|---|
| 1 | Wide Arc | Active cone cleave | 1 | 6, 12, 18, 24, 30 (subject to balance) |
| 2 | Spin Dash | Active targeted-point spinning AoE traversal | 1 | 9, 18, 27 (proposal) |
| 3 | Overhead Breaker | Momentum-consuming single-target finisher | **6** | 12, 18, 24, 30 (proposal) |
| 4 | Battle Cry | Short offensive buff | Later, example 18 | 24, 30 (proposal) |
| 5 | Greatsword Mastery | **Evolving** passive | 1 | 6, 12, 18, 24, 30 (proposal) |
| 6 | Heavy Rhythm | Momentum generation / decay passive | 1 | 5, 10, 15, 20, 25, 30 (proposal) |
| 7–8 | Reserved | Other class skills/passives | — | — |
| 9 | **Ultimate reserved (Tier IV+)** | Unavailable to this Tier II class | never at this tier | Quest-gated for later classes |
| 10–12 | Attack, Interact, Return | Shared | existing | unchanged |

**Starter learning requirement:** The four level-1 abilities/passives are *available immediately*, even though the current skill-point system grants only one point per level. If the design wants them to be usable from the beginning, initialize them as learned rank 1 (free starting ranks) rather than requiring four points at level 1. Subsequent ranks use level-gated allocation as usual; define exact starter point economy before implementing Greatblade-specific save progression.

## Greatsword Mastery: one evolving passive (confirmed ranks 1–4)

| Rank | Scales previous effects AND adds new effect |
|---:|---|
| **1** | Small increased greatsword damage and small bonus to STR. Both are real combat modifiers (not tooltip-only). |
| **2** | Increase prior damage/STR bonuses, **brief attack-speed increase after using Wide Arc**. Rank-up affects both magnitude and duration modestly. |
| **3** | Increase earlier bonuses again, and **widen Wide Arc's hit cone**. Respect honest telegraph/multi-target hit area. |
| **4** | Increase everything again; using **Overhead Breaker** also awards **small shield OR small heal**. This is an *open choice*: prototype shield first because Greatblade should remain different from a dedicated healer; don't treat this as locked. |
| **5+** | Improve existing values; additional distinct milestone(s) may be added later. Avoid designing filler abilities or compulsory effect inflation now. |

Suggested **illustrative only, not finalized** starting values: rank 1 +2% greatsword damage and +1 STR, increasing smoothly with rank. Rank-2 cleave haste proc ~3s. Rank-3 cleave area increase ~15%. Rank-4 finisher shield ~5% max HP with short expiry. All require combat timing/playtesting and appropriate modifier cleanup on class switch.

## Heavy Rhythm: separate Momentum passive

- Rank 1 increases Momentum gained on effective hits and/or reduces out-of-combat decay.
- Later ranks further increase generation, slow decay, and **may raise max Momentum**.
- Heavy Rhythm is not an attack-speed or damage stat passive, and must **not duplicate** Greatsword Master's cleave/STR/damage bonuses.
- Momentum is a normal resource with explicit cap, current value, generation and depletion; do not implement threshold-bonus stacks or hit-based combo resets.

## Four active concepts

- **Wide Arc:** heavy frontal cleave, multi-target; proc small attack-speed buff once **Greatsword Mastery rank 2** exists; width increases with Mastery rank 3. Hits can generate Momentum in a sensible capped way if multiple units are struck.
- **Spin Dash:** click a **ground location**; character spins while traversing a collision-safe path, damaging enemies near the traveled route. Avoid multiple hits on the same enemy per frame unless a deliberate tick interval/cap is part of design; avoid getting through enemies without an explicit collision/ghosting permission. Targeting should use existing ground cursor UI, not a teleporter.
- **Overhead Breaker:** unlock rank 1 at level 6; single-target giant impact, consumes Momentum. At Greatsword Mastery rank 4, triggers modest shield or heal on successful use (shield currently recommended, not fixed).
- **Battle Cry:** later unlock (e.g. level 18), offensive buff, not a Tier II ultimate.

## Prototype assets

Use the local Asset Store catalog before making substitutes:
- **Female Warrior Princess**: `Assets/AllStarCharacterLibrary/Models/Characters/A03.FBX` (candidate **shared** base for player/retired swordswoman). Contains female idle/walk/basic-attack source animations; needs Unity import/rig validation and retarget compatibility check.
- **FREE — Modular Character — Fantasy RPG Human Male**: `Assets/Blink/Art/Animations/Combat/MeeleeAttack_TwoHanded.fbx`, `SpinAttack_TwoHanded.fbx`, `SpinLoop_TwoHanded.fbx`, `SpinToWin_TwoHanded.fbx` for movement/cleave/spin prototype. Can retarget to a female humanoid if both import as Unity Humanoid; do not claim until tested.
- **Free Low Poly Swords**: `Assets/PurePoly/Free_Swords/Prefabs/PP_Sword_0177.prefab`, `PP_Sword_0363.prefab`, `PP_Sword_1039.prefab` for oversized sword. Inspect geometry in actual camera before locking sword.
- Originals remain **LOCAL and gitignored**. Tracked code/fallback prototype model must work without these packs.

**Prototype deliverables:** Unity visual prefab with a readable Greatblade silhouette and oversized two-handed sword; reuse the same base for future swordswoman mentor; visual idle/walk/attack/Spin Dash test; progression prototype with level gates and upgrade gates; then combat implementation for Wide Arc, Spin Dash and Overhead Breaker. These are separate stages: an art-only scene/prototype is not a complete playable Tier II class.

## Explicitly NOT decided

- Exact numerical damage, cooldown, spin speed/distance/width, Momentum cap/gain/decay.
- Rank 4 Greatsword Mastery **shield versus healing**, and effect values.
- Specific Battle Cry unlock level and rank cadence.
- Whether Spin Dash can temporarily move through **enemy hitboxes** as an explicit effect; default is no ghosting.
- NPC mentor quest details and tier unlock quest.
- Greatblade's future Berserker, Swordmaster→Samurai, or secret Witcher-style class precise advancement mechanics (their existence as possible branches is recorded in Tier II v0.2).

**Branch:** `feature/visual-overhaul-vis00`; do not merge into `main` without approval; preserve current user character saves and unrelated licensed shader/material changes.
