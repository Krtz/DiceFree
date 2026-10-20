# Class Progression

## Current Tier II additions (2026-10-16)

Both first branches now have a level-30 quest advancement: Physically Blessed Novice to Greatblade through the retired swordswoman's normal Slime Boss trial, and Magically Touched Novice to Mage through the brewer / retired mage's Slime Dungeon puzzle trial. Both require returning to the mentor and create a level-1 child manifestation while preserving the parent.

Mage is an unspecialized INT caster with six active spells and two passives. See [Mage specification](classes/MAGE.md) for the agreed rules, provisional balance and visual provenance. The flexible ability provider supports this eight-ability kit without assigning an ultimate.

The menu now groups saved manifestations by tier and displays equipped 2D portraits. Historical prototype sections below describe earlier milestones, not the complete current feature set.


## Core concept

Every new Echo lineage begins as a **Novice**.

See `docs/classes/NOVICE.md` for the detailed starting class.

Advancement branches create alternate class manifestations rather than deleting the parent manifestation.

Each class ID has at most **one persistent save-state** for the Echo.

The original class remains playable, while the first advancement into a new class creates that class's save-state at level 1.

## Advancement cadence

```
Novice        -> reach level 10  -> branch -> new Tier I manifestation starts at level 1
Tier I        -> reach level 30  -> advance -> new class starts at level 1
Tier II       -> reach level 60  -> advance -> new class starts at level 1
Tier III      -> reach level 120 -> advance -> new class starts at level 1
Tier IV       -> reach level 200 -> advance -> new class starts at level 1
Tier V
```

A character may continue leveling beyond an advancement threshold.

When they eventually advance:
- the new class begins at level 1;
- excess XP does not carry over;
- the new class uses its own base stats and growth.

Overleveling gives no automatic permanent bonus, but can make advancement quests easier and may satisfy hidden conditions.

A level-200 Novice is intentionally possible.

It is also intended to receive a hidden stronger Novice payoff (secret skill and potentially first passive) rather than remaining permanently identical to the level-10 kit.

## Branching manifestation model

Advancement is a **branch**, not an overwrite.

When a manifestation reaches an advancement threshold and chooses a class:
- the current manifestation is preserved as a playable parent slot at the fork point;
- if the target class does not yet have a save-state, that class save-state is created for the chosen advancement;
- the game immediately continues as that new advanced manifestation in the same session/world state;
- the new advanced manifestation begins at level 1 with its own class base stats/growth;
- **all manifestation-specific state** is snapshotted into the child at the fork, including gear, carried inventory/currency, quest/world progression and relevant class/skill state;
- the parent keeps its original state and the child receives a duplicate snapshot;
- the preserved parent resumes immediately before the blessing choice if loaded later;
- Echo-wide state remains shared;
- the parent manifestation can later choose another available branch, creating another class save-state if that class has not already been created;
- reaching the same resulting class through another route does not create or overwrite a duplicate save-state.

Example after the first split:
- Novice has one preserved save-state;
- Physically Blessed Novice can gain one save-state;
- Magically Touched Novice can gain one save-state.

The class/Ways UI can serve as the start-game selector for these class save-states.

### Current prototype implementation (#50, 2026-10-06)

The first manifestation-fork foundation is implemented on `feature/50-advancement`, stacked on the completed #49 Novice-skill branch.

Current prototype behavior:
- the player build boots into a dedicated `StartMenu` scene before Cornberg;
- a fresh save offers `Start as Novice` and then enters Cornberg as the initial level-1 Novice manifestation;
- an existing Echo lists every saved manifestation with class name and saved level, marks the last-active manifestation, and lets the player choose which existing class timeline to load;
- an unresolved/missing class definition remains visible as unavailable rather than silently deleting its save section;
- choosing an existing manifestation updates only Echo-level active-roster metadata transactionally, then Cornberg loads that saved class state;
- class switching is not exposed as an arbitrary in-world gameplay button; the start menu is the normal load-time selector;
- level-10+ Novice exposes two data-driven advancement edges: Physically Blessed Novice and Magically Touched Novice;
- advancement commits the preserved parent and new child in one Echo revision before changing the live actor;
- the child starts at level 1 / 0 XP and class skill ranks reset instead of numerically carrying Novice ranks;
- manifestation-owned quests/world state, anchor, carried gold, inventory and equipment snapshot into the child;
- copied child items receive new instance IDs and equipped references are remapped, so parent/child timelines do not claim the same item instance identity;
- parent and child then diverge independently and the preserved Novice can later create the other first branch;
- duplicate creation of an already-existing target class is rejected without overwriting it;
- an additive version-1 `echo:manifestations` roster section stores active class and branch history while the overall save/manifestation schema remains v5;
- #50 originally authored Tier-1 actor shells; both first branches are now real playable manifestations: Physically Blessed was completed in #51 and integrated into `poc/cornberg` at `77716b2`, while Magically Touched is implemented on review branch `feature/52-magically-touched` with its dedicated presentation, Mana economy, five-skill kit, persistence and confirm-cast targeting.

The start-menu presentation is deliberately functional/prototype-grade. Final title art, character cards, archive/delete UX and richer class preview presentation remain future UI work.

This one-save-per-class model is part of the Echo/timeline fantasy.

Open:
- archive/delete/recreate behavior for intentionally discarded class-local saves;
- UI presentation of historical parent/route relationships.

## Core attributes

Current attribute set:
- Vitality
- Strength
- Agility
- Intelligence
- Spirit

Every class defines:
- level-1 base attributes;
- per-level attribute growth;
- starting Physical Defense;
- starting Magical Defense;
- one or more primary attack attributes;
- movement speed;
- base HP;
- resource(s);
- other class-specific base values.

Later advancements generally start with higher base stats.

Players do not manually allocate stat points.

### Primary attributes

A class may use one or multiple primary attributes.

There is **no universal multi-primary formula**.

Each class defines its own attack scaling according to identity and balance.

Examples could include:
- equal STR + INT weighting;
- mostly STR with some INT;
- AGI + SPI;
- all five attributes for an exceptional secret class.

The exact coefficients are class data.

### Vitality

Vitality contributes to HP:

```
Max HP = class base HP + (Vitality × coefficient) + modifiers
```

The coefficient may be universal or class-dependent; unresolved.

### Secondary effects

Current direction:
- Vitality -> HP and HP regeneration;
- Strength -> modest Physical Defense;
- Agility -> very small Attack Speed and Movement Speed;
- Intelligence -> modest Magical Defense plus class-specific resource value;
- Spirit -> healing done and healing received with separate coefficients.

Class base stats, equipment, abilities and passives remain the main source of large combat differences.

See `docs/STATS_AND_DAMAGE.md` for the detailed model.

### Skill scaling

Skill scaling is ability-specific.

Abilities can use:
- weighted combinations of stats;
- highest of selected stats;
- highest stat overall;
- total stats;
- class-specific values.

The class's primary attribute(s) do not force every ability to use the same formula.

## Opening branches

First advancement:
- **Physically Blessed Novice**
- **Magically Touched Novice**

Working Tier-1 class sheets:
- `docs/classes/PHYSICALLY_BLESSED_NOVICE.md`
- `docs/classes/MAGICALLY_TOUCHED_NOVICE.md`

Current strong direction:
- both Tier-1 branches introduce **Mana** as a simple shared resource;
- specialized resources arrive on later classes where they reinforce identity;
- Physically Blessed remains a broad STR/AGI melee generalist;
- Magically Touched remains a broad INT/SPI magical generalist.

Current candidate level-30 branching:
- Physical: sword-and-board, rogue, ranger, two-handed melee DPS;
- Magical: shield/buff healer-support, healing-focused healer, arcane wizard, occult caster.

This four-plus-four split is still a working direction rather than locked final structure.

### World 1 normal Tier II scope and World 2 tier progression — LOCKED by user 2026-10-19

**USER-CONFIRMED FINAL NORMAL TIER II ROSTER FOR WORLD 1: EXACTLY EIGHT CLASSES, FOUR FROM EACH TIER I NOVICE.** This closes the immediate question of whether Acolyte/Thief are the last normal classes: **Psion is INCLUDED as the eighth class**. Other future ideas such as Druid are not additional normal World 1 Tier II classes unless user revises scope.

| Tier I parent | Standard World 1 Tier II class | Design status |
| --- | --- | --- |
| Physically Blessed Novice | **Greatblade** | Designed |
| Physically Blessed Novice | **Defender** | Designed |
| Physically Blessed Novice | **Ranger** | Designed |
| Physically Blessed Novice | **Thief** | **NOW ACTIVE design interview**, class kit not decided. [Thief Tier II](classes/THIEF_TIER2_DESIGN.md) |
| Magically Touched Novice | **Mage** | Designed |
| Magically Touched Novice | **Priest** | Designed |
| Magically Touched Novice | **Acolyte** | **TIER II CLASS DESIGN COMPLETE, Rounds1–8**, six-skill kit with exact 80-rank user-approved gates (curse, Radiation plague, skeletons, Void Drain Life, Mortal Coil dual-target spell, INT/SPR/poison passive). [Acolyte Tier II design](classes/ACOLYTE_TIER2_DESIGN.md) |
| Magically Touched Novice | **Psion** | Roster APPROVED; specific psychic kit/equipment/advancement NOT YET DESIGNED, after Thief |

**USER-APPROVED DESIGN AND WORLD ORDER:**
1. Finish all eight NORMAL World 1 Tier II class designs, specifically **Acolyte → Thief → Psion** for the three unfinished classes.
2. Then design the **Quillboar area/region**.
3. Then design the **major/big city** and its gameplay/services.
4. Some **Tier II advancement NPCs MAY be moved to that future city** for progression pacing; this is an open later decision. Do NOT quietly relocate already-agreed mentors/quests or invent finished city geography. Keep Cornberg swordswoman/Mage brewer, Trimery Priest healer, Trimery veteran Defender guard and wounded Ranger/Alpha Wolf as currently specified unless user explicitly revises them.
5. **DO NOT DESIGN ANY TIER III CLASSES NOW. Tier III belongs to World 2.** Older Greatblade, Ranger, Priest and Defender Tier III concept names/convergence snippets are historical brainstorming only; do not expand their trees, skills or quests until World 2 design is authorized.

**ACOLYTE ROUND 1 (2026-10-19):** user chose four core combat pillars: **one Curse, one distinct damage-over-time ability, one Soul Charge-spending Summon, one channeled Life Drain**. Acolyte uses **Mana + Soul Charges**, charges earned through offensive spells; charge caps/gain and summon type remain open. **Wand + grimoire** remain the equipment concept, but the grimoire is a **normal stat-bearing offhand for now**, NOT a compulsory special spell/inscription/book-charge system; authored item Intrinsics may modify class abilities later across all classes. Role is **sustained damage, curses/debuffs and life-drain self-sustain**, contrasting with Mage burst/CC. A future **graveyard mentor** asking for deceased people from **Cornberg and Trimery** to be brought for **proper burial** is a story hook; exact NPC, geography and advancement mechanics deliberately DEFERRED until location planning. **Acolyte Round 2 (2026-10-19):** Curse reduces Magical Defense and attack speed; a weak Radiation DoT spreads; controllable skeletons last a set time; Soul Charge cap 8 with 4 spent per skeleton; offensive spells, successful Drain Life ticks, and deaths of targets afflicted by the Radiation DoT generate Soul Charges. Drain Life deals Void-element damage, heals the caster based on damage dealt, channels for 5 seconds with one tick each second, and cancels on movement, stun, or explicit interrupt (not ordinary incoming damage). **ACOLYTE-011–015 newer decisions:** Curse uses **FLAT** reductions to enemy Magical Defense and attack speed (boss Tenacity reduces their effects; specific formula/open). Radiation infects nearby enemies only after **2 seconds of continuous proximity**, never re-infecting a previously infected member within the same chain; radius/independent re-casts open. **Up to 4 active controllable skeletons, each 15s lifetime**. Soul Charges **+1 per resolved Drain Life tick** and **+1 per enemy death while carrying own Radiation DoT**; applying Curse or Radiation grants **0**. Drain Life restores a **skill-rank-scaling percentage of ACTUAL enemy HP damage**, never from shield-absorbed damage. **ACOLYTE-016–020 ROUND4 CONFIRMED (supersedes those earlier OPEN items):** Curse rank1 **−5 flat MDEF, −0.10 attacks/sec for 10s**, boss reduction via shared Tenacity rules when defined; Radiation **3m proximity/2s uninterrupted contact**, resets if separated, infected hosts can pass it onward, no chain reinfection, manual recast refreshes duration; cap4 **15s controllable skeletons**, new summon **replaces oldest**, auto-retaliate responds to the hero's toggle; Drain Life **25% actual HP damage healing at rank1 +2 percentage points per rank**, target ~60–65% final cap not exactly selected. **User added TWO abilities to make SIX total:** **Mortal Coil** (1-second INT+SPR-scaling Void offensive spell, optionally heals allied heroes/owned skeletons instead), and a **passive** granting INT + smaller SPR and weak poison on wand basic attacks. Passive poison is currently a STATUS TAG with element/stacking/proc chance undecided. Mortal Coil ally healing, self-cast and charge-generation details, and exact rank limits are OPEN. **Earlier Occult Bolt idea NOT selected.** Details: [Acolyte class design](classes/ACOLYTE_TIER2_DESIGN.md).

**ACOLYTE-021–025 ROUND 5 (2026-10-20):** Mortal Coil's **ONE-SECOND Void spell** damages enemy targets with INT+SPR and **VOID-ELEMENT HEALS the caster, allied heroes or owned skeletons**, applying the healed target's Void elemental-resistance healing modifier. Channel cancels on movement/stun/explicit interruption, not on ordinary incoming damage. **+1 Soul Charge only for successful enemy DAMAGE; +0 on self/ally/skeleton healing.** Passive grants **+2 INT/+1 SPR per purchased rank** and **guaranteed 3s nonstacking Poison-tagged DoT on every successful wand basic attack**, refreshing duration rather than stacking; poison damage scales with INT and passive rank, its damage element/rate are OPEN. User delegated an **80-rank progression**; designer **PROPOSES, awaiting numeric sign-off:** Curse14, Radiation16, Skeleton12, DrainLife20, MortalCoil10, Passive8, staggered first gates and intervals, **verified a legal skill purchase at every class level1–80**, all 80 gates eligible by level77. Proposed Drain Life cap20 yields **63% healing from actual HP damage** under approved 25%+2 percentage points/rank formula. See [Acolyte Tier II](classes/ACOLYTE_TIER2_DESIGN.md). **DESIGN ONLY, not gameplay implementation.**

**ACOLYTE-026–030 ROUND 6 — LATEST USER CONFIRMATION (2026-10-20):** The formerly proposed exact **80-rank class schedule is now APPROVED:** Withering Curse14 / Radiation Plague16 / Summon Skeleton12 / Drain Life20 / Mortal Coil10 / Occult Adept8, with audited level gates in [Acolyte design](classes/ACOLYTE_TIER2_DESIGN.md) and legal one-point purchases every class level1–80. Drain Life rank20 therefore heals **63%** actual HP damage (under approved +2pp/rank healing formula); passive rank8 gives **+16 INT / +8 SPR**. Mortal Coil enemy Magical Void damage uses both INT and SPR with **INT emphasis**; self/ally/skeleton Void-element healing uses both with **SPR emphasis**, both scale by rank. **USER CORRECTED PREVIOUS MISPRINT:** passive Poison DoT has **MAGICAL COFFEE-ELEMENT damage, NOT DONUT**, tick ONCE PER SECOND, three seconds, does NOT spread or stack, status tag Poison. Exactly **ONE skeleton per summon for 4 Soul Charges and ZERO Mana**, with Auto-Retaliate OFF attack only by explicit user order, ON reactive to being attacked. Designer-authored skill CDs/Mana/damage/healing coefficients in Acolyte spec are **FIRST-PASS PROPOSALS only**, not user-confirmed balance or implemented behavior. Coffee registry availability still to be checked; no global Donut changes.

**ACOLYTE-031–035 ROUND 7 (2026-10-20) — MOST RECENT CONFIRMED DETAILS:** Initial playtest cooldown/Mana costs are now USER-APPROVED: **Curse12s/12 Mana, Radiation10s/15 Mana, Summon Skeleton3s/0 Mana+4 Soul Charges, Drain Life14s/24 Mana, Mortal Coil7s/14 Mana, passive0**. Radiation duration is **3s at rank1 growing to 8s at rank16** (intermediate rank duration steps/tick frequency remain unapproved designer proposals). Summon Skeleton ranks improve **skeleton HP and BASIC ATTACK damage**, not quantity/15s lifetime/4-Soul cost; previous exact skeleton numeric HP/attack coefficients remain PROPOSED. Nonstacking, nonspreading three-second Poison-tag **MAGICAL COFFEE** wand DoT refreshes duration WITHOUT restarting its existing once-per-second tick clock. **Each successfully resolved Drain Life tick also damages an enemy SHIELD and awards +1 Soul Charge even when FULLY blocked**, while healing only from ACTUAL ENEMY HP damage, thus **ZERO healing from fully shielded damage**. User casually used "soul shard" for the tick reward; retain established **Soul Charges** name unless explicitly changed. All raw spell coefficients and shared Tenacity equation remain pending their own sign-off. [Acolyte design](classes/ACOLYTE_TIER2_DESIGN.md). This note is DESIGN ONLY.

**ACOLYTE FINAL ROUND 8 — USER-APPROVED 2026-10-20:** See [authoritative Acolyte final class spec](classes/ACOLYTE_TIER2_DESIGN.md), which supersedes historical open/proposed entries. Radiation duration increases at skill ranks **4,7,10,13,16** from **3s rank1** to **8s rank16**. Transmitted infection **INHERITS ONLY THE SOURCE'S REMAINING DoT TIME**, not a new full duration. Radiation DoT now ticks every **0.1 seconds (TEN PER SECOND)** with per-tick RAW Magical Radiation damage **1/10 of the previous approved one-second tick formula**, retaining expected DPS. At rank `r`: `[(2+0.10×casterINT)×(1+0.05×(r−1))]/10` RAW per 0.1s tick. Skeleton HP **`(55+2×ownerSPR_AT_SUMMON)×(1+0.05×(r−1))`** (SPR NOT VIT); Basic Physical raw attack `(3+0.18×ownerINT_AT_SUMMON)×(1+0.06×(r−1))`; owner stats **SNAPSHOT ON SUMMON**, 5% HP/6% attack rank scaling approved. Curse rank growth −1 more flat MDEF, −0.005 attacks/sec per rank (rank14 −18 MDEF/−0.165 attacks/s, 10s). **ALL** previously proposed raw damage/healing coefficient baseline values for Radiation, Drain Life, Mortal Coil and Magical Coffee poison now **USER APPROVED FOR INITIAL PLAYTEST**, with Radiation tick and SPR HP corrections above. Global Tenacity, assets, elemental registry, implementation and deferred graveyard quest remain external follow-up. **ACOLYTE INTERVIEW COMPLETE; THIEF IS NEXT.**

**CLASS IMPLEMENTATION SCOPE:** This is an explicit **DESIGN/DOCUMENTATION-ONLY** conversation. No Unity source, scene/world creation, builds, saves, NPC moves or implementation tickets without later instruction in the separate DiceFree implementation workflow. User approval of the eight-class roster is NOT evidence eight classes are already playable or coded. Final game names of Thief/Psion/Acolyte, their visuals, weapons and kits can still be tuned by their own upcoming interviews, while preserving the currently approved class identities unless later changed.

### Long-term class-web scale

A **very large class graph/web is an explicit long-term goal**.

Do not optimize the design around keeping the final class count small.

The tree also does not need to be development-symmetric:
- one class may initially have one implemented successor;
- another may already have three;
- additional descendants can be filled in over time;
- higher-tier concepts can take longer to design and implement.

The architecture must support arbitrary branch counts, convergence requirements and partial/asymmetric content without requiring placeholder classes or giant hard-coded trees.

See `docs/CLASS_REQUIREMENTS_AND_DISCOVERY.md` for multi-lineage requirements, visibility/discovery and class-web rules.

### No direct class creation

Unlocking/discovering a Way never creates it from the menu.

A missing class save is always created through an eligible earlier class's actual advancement quest/event.

This remains true when a much later-tier Echo-wide event unlocks an earlier-tier Way.

If a class save is permanently deleted, recreate it by advancing into it again from an appropriate earlier class; Echo-wide discovery and milestones remain intact.

### Universal rank-zero and ordinary basic-attack rules (user correction 2026-10-18)

**Approved design direction for ALL classes and tiers:**

- A newly created class manifestation begins with **every class ability/passive at rank 0**, even if that ability is eligible to learn at level 1. Do **not** automatically grant free rank-1 starter abilities.
- Every new manifestation starts at **class level 1 with exactly one unspent class skill point**; gaining levels grants more skill points under its class rules (the current common pattern is one per level).
- A player may spend that point on an eligible rank-0 skill to learn rank 1. Other skills remain rank 0 until purchased; class-specific level requirements may delay eligibility. Do not equate a visible locked rank-0 skill with an already learned skill.
- **All ordinary/basic/autoattacks deal normal, non-elemental damage**; do not implicitly make them Fire for Mage, Light for Priest, etc. A class's weapon, reach, animation, attack channel (Physical/Magical) and primary-stat scaling are independent design choices. Explicitly authored equipment/abilities may separately add elemental damage where appropriate.
- This **supersedes** prior prototype-level choices such as the Mage design's **free learned level-1 ability ranks**. Existing gameplay code and current class specs may still implement those legacy choices; treat that as **an implementation discrepancy to reconcile in the development chat**, not as a claim that every live class already conforms.
- This is a rules clarification only. No Unity code, player save or balance was modified by this documentation change.

### Tier II skill-point capacity, varied caps and legacy resets (user decisions 2026-10-19)

- **CONFIRMED for Tier II Priest, Greatblade and Mage:** the class's **total available skill-rank investment capacity should use almost all earned skill points by approximately class level 80**, i.e. the normal Tier II level-60 advancement threshold plus ~33% extra levels. This is **not** a requirement to automatically advance at 60; characters may continue leveling.
- **CONFIRMED:** individual ability **maximum ranks are based on their effects, balance and performance**, not a universal cap. A high-damage spell can have many ranks while a special utility can have few; specifically **Mage's Math Barrier retains its existing five-rank maximum**.
- **CONFIRMED:** Greatblade and Mage retain the exact first-availability/unlock levels in their existing implemented class designs, but **all skills begin at rank 0** with **one spendable point at class level 1**, no free rank-1 abilities.
- **CONFIRMED:** existing Greatblade and Mage saves under the old four-free-starting-ranks economy require a **silent automatic one-time full class-skill reset and refund**, **not grandfathered free ranks**. Preserve the same class level, XP, quest/equipment/inventory and unrelated save progress, with recoverable backup, per-manifestation migration marker and idempotence. Refunded spendable budget is **min(saved class level, total class ranks)** (Greatblade max 78, Mage max 80), not class level plus four free ranks. Level-80+ saves are included; never discard XP or levels, and never manufacture unspendable overflow.
- **CONFIRMED (2026-10-19):** Greatblade and Mage use **skill-specific rank upgrade cadences**: certain abilities may gain one rank per class level while more powerful/technical skills require multiple class levels between ranks. Do not enforce one global upgrade interval. An example of Half-Life needing approximately **4 class levels between ranks** is not yet its final gate table.
- **CONFIRMED:** Mage max ranks **Ember Bolt 18, Frost Snare 12, Arc Lightning 12, Quicksand 10, Half-Life 8, Math Barrier 5, Elemental Weaving 8, Mana Attunement 7** (total **80**); Math Barrier's five-rank formulas stay intact. Greatblade max ranks **Wide Arc 20, Spin Dash 16, Overhead Breaker 18, Battle Cry 8, Greatsword Mastery 8, Heavy Rhythm 8** (total **78**). Mastery upgrades base STR/greatsword damage at every rank; combat features unlock at skill ranks **3 (Wide Arc haste), 5 (wider cleave), 7 (Overhead Breaker shield)**; rank 1 grants initial passive; ranks 2/4/6/8 add only baseline stat/damage improvement.
- **CONFIRMED:** increasing skill rank caps is allowed to use diminishing or capped secondary effects to avoid runaway crowd control, defense debuffs and Momentum, while primary rank improvements remain meaningful.
- **CONFIRMED:** existing Greatblade/Mage legacy saves are automatically migrated **on first load**, resetting/refunding class skills **without a notification or confirmation prompt**. Keep a recoverable backup, per-manifestation idempotence, correct level-derived skill-point pool and all unrelated character/quest/equipment progress.
- **CONFIRMED Greatblade alternating passive:** Mastery rank **1** grants STR/damage, **3** unlocks temporary Wide Arc haste, **5** widens Wide Arc, **7** grants an Overhead Breaker **METAL-element shield** for **5 seconds**, capacity **100 + 5% of Greatblade maximum HP**; shield is NOT a heal. **Every Mastery rank 1–8 additionally grants +1 STR and +2% greatsword damage**. Old ranks 2/3/4 trigger effects are deliberately moved to 3/5/7. This Metal shield uses the **latest global elemental rule (Defender Round 1)**: consumes **0.8 shield HP per matching Metal damage blocked** or **1.2 shield HP per other/untyped ordinary damage blocked**, equivalent to 1.25 matching / ~0.833 nonmatching blocked per 1 shield HP. Repeated Overhead Breaker **replaces/refreshes** the existing shield. Pure damage remains separately specified by the shield framework.
- **CONFIRMED timing:** Greatblade Wide Arc and Overhead Breaker, and Mage Ember Bolt can rank up **every class level**, starting from each skill's existing first-unlock level. Mage Half-Life rank levels **18/22/26/30/34/38/42/46**; Mage Math Barrier ranks at existing **6/12/18/24/30**.
- **CONFIRMED GLOBAL POINT CAP AND UI (2026-10-19):** `spendableBudget = min(classLevel, Σ(maxRank of class-owned skills))`; `unspent = max(0, spendableBudget − Σ(purchased ranks))`. Stop accruing **spendable** points once the class reaches its complete rank budget; normal XP, leveling and stat growth continue. **Do NOT add the Novice-style all-stat passive to every class.** When new ranks/classes are introduced in later updates, budget may increase according to existing level without losing earned progression. **Only show the skillpoint HUD badge / level-up reminder when unspent > 0 AND at least one rank is actually purchasable now**; hide it when only later rank gates remain, even if points are banked, and hide it once maxed. Banked point count should remain accessible through the class skill UI.
- **CONFIRMED designer delegation:** allow one point to be spent at **every character level until reaching the class's available total**, despite different skill-specific rank intervals. The audit's provisional per-skill unlock schedules were checked across **every level 1–78 Greatblade / 1–80 Mage** to satisfy `Σ(eligible ranks by level) >= min(level,total ranks)`; the final schedules, numeric high-rank balance and C# integration should be verified in playtesting. [Full proposed gate schedule and validation](TIER2_GREATBLADE_MAGE_SKILL_RANK_AUDIT_2026_10_19.md).
- **DEFENDER design-only progression (2026-10-19):** six Tier II skills (Shield Bash, Challenge, Bulwark, Shield Sweep, Hold the Line, working-name Iron Guard). The user delegated weighted rank caps/gates; designer **proposed 18/10/16/20/10/6 = 80 total** with a validated one-point-per-level path to level80. **User-approved 2026-10-19 initial playtest ranks and gates**, verified for every level 1–80; Iron Guard PDEF+MDEF +500 per rank, 20 Resolve, 2s lasting through CC, with immunity vs normal CC still open. Resolve decay 1 every 3s outside combat only. Ranger's new design interview is documented at [Ranger Tier II](classes/RANGER_TIER2_DESIGN.md), using the already-decided Wood Elf identity and possible physical ranged branch. Iron Guard grants +500 DEF per rank for 2 seconds while the player cannot act, reserved Tier III damaging counter evolution. See [Defender tier II design](classes/DEFENDER_TIER2_DESIGN.md). The same bounded rank-point budget and actionable-only reminder must apply when eventually implemented.
- **CONFIRMED user workflow (2026-10-19):** keep all Greatblade/Mage/Priest/Defender revisions in documentation until remote computer access is restored. The user will separately ask their implementation chat to reread current authoritative documents and implement. **No unified implementation handoff or game edits in this design chat.**
- **DESIGN-ONLY status:** this record **does not mean the playable Greatblade or Mage C# allocators/saves have been migrated**. The existing implementation still auto-learns four starter ranks and counts unspent points as `level−1−spent`. **Do not interpret the new policy as shipped.** Read the [Greatblade/Mage rank audit](TIER2_GREATBLADE_MAGE_SKILL_RANK_AUDIT_2026_10_19.md) before implementation.

### Tier-1 skill-rank pattern

Current Tier-1 direction:
- 5 skills;
- 6 ranks each;
- 1 class skill point at level 1;
- +1 class skill point per level;
- exactly 30 total points by level 30.

This clean fit is specific to Tier 1 and is **not** a universal rule for later tiers.

## Class identity

Each class defines:
- allowed equipment families;
- weapon categories;
- melee/ranged basic attack;
- attack range/speed;
- primary attribute(s);
- primary-stat coefficients;
- resource;
- fixed ability kit;
- passive mechanics;
- advancement quest;
- visual identity;
- **body/species/form identity where applicable**.

A Way is allowed to transform the Echo substantially. Classes are not constrained to one human body or even one universal humanoid rig.

Settled Tier-2 visual identities:
- **Ranger -> Wood Elf** — **CONFIRMED documentation-only Tier II physical ranged DPS** from level30 Physically Blessed Novice after accepting wounded Ranger's quest near Trimery Priest, killing existing Alpha Wolf after acceptance, and returning. Two-handed bows: shortbow attacks faster with lower per-shot dmg; longbow slower with higher per-shot dmg (comparable longbow ~25% more damage and ~20% slower as ITEM design guideline); **no equippable quiver**, both share Ranger's class-based range. Move during autoattack windup cancels unfired arrow. **Mana only, no pet**; Hawk Dive is one-time hawk attack.
  - **USER-APPROVED skill roster and level-gate schedule (80 ranks):** **Aimed Shot20, Exploding Arrow18, Arrow Rain16, Hawk Dive10, Evasive Step8, Bow Mastery8**, rank0 starters with class level1 one spendable skill point and a purchase possible every level through 80; exact eligible class levels in [Ranger Tier II class spec](classes/RANGER_TIER2_DESIGN.md). These are **not yet implemented**.
  - **Confirmed combat:** Hawk Dive 15s CD and Hunter's Mark 8s, 10% damage bonus to that Ranger's attacks/skills, one mark target/Ranger. **Aimed Shot's extra marked-target RAW damage** = `0.5 × current AGI × (1 + 0.05 × (AimedShotRank−1))`, in addition to the mark %. **Exploding Arrow** replaced historical Multishot; **targeted arrow CANNOT MISS**, directly strikes enemy then **Physical Gun 2.5m-radius splash CENTERED ON HIT TARGET** (primary direct + splash, secondary splash); earlier assistant proposal about missing and landing elsewhere **SUPERSEDED/INVALID**. **Arrow Rain** channels **1.5s** with big upward then small upward arrow; player MOVEMENT, STUN or explicit INTERRUPT cancels channel without rain, but normal damage alone does not. Once successfully completed, **4 damage waves over 2s keep falling regardless of later Ranger CC/movement**, in **3m radius**, **0.4× current basic shot raw per wave at rank1**, increasing with rank. **Evasive Step:** 3–4m mouse-directed leap, 3s +10% move speed and +1pp basic-attack-only evasion/rank; cannot cast rooted/stunned. **Bow Mastery:** +2 AGI/rank, +0.1m normal+ Aimed Shot range/rank, +1pp crit/rank at 1.4× on basic+ Aimed ONLY; no other Ranger skills inherit crit.
  - **RANGER-030 DESIGNER balance PROPOSALS, NOT APPROVED NOR IMPLEMENTED:** Aimed Shot cooldown6s/Mana10, Exploding Arrow12s/20, Arrow Rain20s/30, Hawk Dive **confirmed**15s/Mana15 proposed, Evasive Step15s/12, Bow Mastery passive; damage coefficient growth and high-rank mark growth proposed in the [Ranger spec](classes/RANGER_TIER2_DESIGN.md). Future Tier III Beastmaster/Marksman with class-range differences, or one-handed crossbows/dual-pistol Gunslinger, remain **speculative only**, not established class branches.
- **Wizard -> High Elf**.

Tentative future directions:
- Berserker may be an Orc;
- one Tier-4 tank may be a Centaur.

Those tentative examples are not locked class canon yet.

Architecture/art must support different proportions, rig families and potentially non-biped forms without making those classes hacks or exceptions bolted onto a human-only player architecture.

### Authored body presentation

Each class has **one authored body/sex presentation** rather than player-selectable male/female variants for that Way.

A class can be visibly male, visibly female or androgynous.

The visual transformation should generally become more pronounced as the class lineage specializes:
- Novice begins deliberately neutral/androgynous;
- early branches can make modest directional changes;
- later Ways can become strongly species-, body- and silhouette-specific.

The form is part of the Way's authored identity, not a separate character-creator axis.

A descendant Way may fully redefine the body/species/form again. Visual inheritance is not a promise that every descendant remains the same species as its parent; advancement can transform the Echo again when that is part of the new Way's identity.

Body form can also carry authored gameplay footprint/collision data. Different class forms may use different hitbox/agent dimensions rather than treating size as presentation-only.

Attack/basic-attack/ability reach remains separately authored and is not automatically derived from body size.

Required world progression must remain accessible to every intended playable form. Large forms can use authored navigation/clearance forgiveness at bottlenecks rather than being locked out because of their footprint.

### Equipment permission inheritance

A descendant Way inherits its parent's allowed equipment/form-family permissions by default.

A descendant may:
- add new equipment families;
- explicitly remove/restrict inherited families when its body/class identity changes;
- override specific compatibility where an inherited appearance no longer fits.

This mirrors visual-appearance inheritance: lineage compatibility is the default, explicit divergence is the exception.

### Gear reconciliation during advancement

If the new Way/form is incompatible with equipment currently worn by the parent snapshot, advancement reconciles it safely:
1. keep compatible equipment equipped where normal slot/permission rules allow;
2. unequip incompatible/conflicting items into the child manifestation's carried inventory when there is room;
3. if carried inventory cannot safely retain them, send those items to the Echo-wide shared bank as a fallback;
4. if neither destination can safely retain every displaced item, the advancement transaction must fail before mutation rather than destroy/drop/overwrite gear.

This reconciliation is part of the atomic advancement/fork transaction.

The parent manifestation remains unchanged with its own original equipment/state.

### Optional transformation presentation

An advancement may optionally define a visual transformation presentation.

The class/manifestation fork is authoritative; transformation VFX/animation are presentation layered on top and may be simple, bespoke, or absent.

Weapon categories are created according to actual class needs rather than predefining every possible weapon.

## Fixed kits and advancement

Classes use fixed kits.

Advancement may:
- add abilities;
- evolve abilities;
- replace abilities;
- change passives;
- replace resources;
- change stats;
- alter equipment permissions;
- change visual form.

## Convergence and Echo-wide class evidence

Class progression is not limited to parent -> child lineage requirements.

The Echo permanently records meaningful class/tier milestones account-wide.

A class may require:
- one lineage milestone;
- several lineage milestones together;
- class evidence combined with quests/items/achievements/discoveries;
- nested AND/OR requirement expressions.

The same resulting class can be reached through different qualifying parent manifestations, but the target class normally uses the same advancement quest/NPC/location regardless of which parent supplies the timeline fork.

A single prerequisite combination can also unlock multiple different convergence classes.

Deleting/cleaning up an old manifestation does not erase class milestones the Echo already earned.

See `docs/CLASS_REQUIREMENTS_AND_DISCOVERY.md`.

## Secret classes

Secret classes may branch from many points and use unusual conditions:
- achievements;
- items;
- exploration;
- encounters;
- account discoveries;
- extreme overleveling;
- strange class-specific feats.

Secret classes use the same advancement thresholds after entering their lineage.

## Healer viability

Healer/support lineages must be fully viable for solo progression.

They should have:
- a real damage rotation;
- sufficient solo damage to quest/farm/grind;
- healing/support tools that become especially valuable in groups;
- class-specific ways to connect offense and sustain where appropriate.

## Open questions

- exact archive UI and permanent-delete confirmation UX;
- exact secondary effect of each attribute;
- universal vs class-specific Vitality coefficient;
- exact second-tier classes;
- weapon categories as classes require them;
- number of character slots.

## Additional settled class-progression rules

### Attribute growth

Primary-attribute growth is determined by **class and gear**.

Players do not receive a general pool of manual VIT/STR/AGI/INT/SPI points to distribute on level-up.

This preserves strong authored class identity while still allowing build variation through gear, skills, effects and class choice.

### Respec philosophy

DiceFree does **not** need a broad always-available respec system as a core progression loop.

The early Novice/onboarding stage may provide free respec support so players can learn the game and experiment before committing to later Ways.

Later classes should not assume routine respec availability unless a particular class/content design explicitly needs it.

### Advancement events

Advancement quests/events are decided **case by case**.

The first Tier 0 -> Tier 1 advancement is intentionally simple: the Novice is told to **go to the mountain** rather than completing a conventional bespoke class trial.

Later advancements can use authored class-specific quests, trials, rituals, discoveries or other requirements as appropriate.
