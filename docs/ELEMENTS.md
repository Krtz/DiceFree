# Elements

## Direction

DiceFree should inherit the **full canonical DiceBound element roster** as its baseline rather than shrinking the sequel to a conventional fantasy-only set.

That means weird elements remain welcome alongside traditional ones.

The exact authoritative import list should be reconciled against DiceBound's live element registry when implementation begins, rather than duplicating a stale hard-coded list here.

Examples already established in DiceBound design include traditional and strange identities such as:
- Fire;
- Ice;
- Electric;
- Light / Holy;
- Radiation;
- Metal;
- Tech;
- Gun;
- Donut;
- Math;
- and other live DiceBound affinities/elements.

## Void — newly requested DiceFree identity (Acolyte 2026-10-19)

**User-confirmed class design:** Tier II Acolyte's **Drain Life** uses the **Void ELEMENT** on a Magical-damage channel. This must be a genuine named element with a stable registry identity and resistance lookup when implemented; it is not just dark-colored non-elemental damage.

**Acolyte ACOLYTE-021 (2026-10-20):** Mortal Coil explicitly has a **VOID-ELEMENT HEALING mode** for its caster, allied heroes and owned skeletons; the enemy-target mode deals Magical Void damage. Friendly Void healing uses **the recipient's Void elemental resistance** under existing `docs/STATS_AND_DAMAGE.md` elemental healing rules (positive resistance increases healing, negative decreases and very negative can invert it). This does **not** create a blanket Void-element-healing status proc or an exception for negative resistance, and is NOT yet implemented. Poison from Acolyte's passive is separately a **Poison semantic status tag**, **not** authorization to invent a Poison elemental affinity.\n\n**Registry provenance is OPEN:** Void was **not previously listed in this document's illustrative DiceBound examples**, and we have NOT verified whether the live DiceBound registry already contains a Void entry. Thus do **not** claim Void is already imported, nor overwrite another existing affinity. When implementing, first reconcile the source roster, then **add an explicit DiceFree Void element entry if needed**, with standard shared resistance processing (current general DiceFree defaults unless subsequently overridden by user). Void does NOT automatically introduce a new status effect, life-steal proc, penetration coefficient, or universal healing interaction: **Drain Life itself** is the authored life-draining channel. No standalone Void proc numeric is approved.


## Coffee — confirmed Acolyte Poison DoT element (2026-10-20)

**User explicitly corrected their own previous wording:** the Poison-tagged three-second nonstacking, nonspreading Acolyte wand-autoattack DoT uses the **MAGICAL damage channel with COFFEE ELEMENT**, **NOT Donut**. Tick once each second, three total if uninterrupted. The **Poison status tag** controls semantic debuff/dispels; the **Coffee element** controls matching damage resistances and other elemental interactions. It does **not** use the distinct contagious Radiation element, and does **not** introduce a new standalone "Poison" element.

The examples above included `Donut`, but that globally available element remains entirely separate and unchanged. `Coffee` has not been verified in DiceBound's source element registry; when implementation begins, reconcile the canonical source list and create a named Coffee element in DiceFree if needed, with stable ID/resistance/VFX hooks. **Do not pretend Coffee is already an implemented live element or invent a Coffee proc/penalty.** This is class DESIGN ONLY. See [Acolyte Tier II](classes/ACOLYTE_TIER2_DESIGN.md).

## Thief Tier II Physical Nature and Physical Tech status effects (2026-10-21; DESIGN ONLY)

**User-confirmed class-specific element/channel combinations:** **Envenom** is an instant Thief dagger stab applying a **NONSTACKING Poison-tagged DoT using PHYSICAL damage channel + NATURE element**; exact initial stabbing damage element, poison tick timing and duration remain open. **Smoke Bomb** is a ground-targeted, 4m-radius, five-second damaging cloud; its DoT uses **PHYSICAL damage channel + TECH element** while enemies inside suffer 20% miss chance on **basic attacks only**. The miss penalty ends instantly on exit. This does **NOT** redefine Tech/Nature elements globally, confer universal elemental proc effects, or imply smoke makes the Thief invisible. Element != damage channel != status tag: Poison is a status label, not a new element. Nature already explicitly exists below and Tech is part of the inherited illustrative element roster above; confirm stable IDs during implementation. [Thief Tier II source design](classes/THIEF_TIER2_DESIGN.md).


**Thief Round5 element delivery refinement (2026-10-21; class DESIGN ONLY):** Envenom is an instant direct stab equal to **2× Thief normal basic RAW** using an ordinary **non-elemental PHYSICAL hit**, and separately inflicts a **three-second, one-tick-per-second, NONSTACKING Poison-status DoT using PHYSICAL NATURE**. Smoke Bomb DoT is **PHYSICAL TECH**, **FOUR damage ticks per second** with per-tick RAW reduced one quarter of otherwise equivalent per-second attack damage, **not Magical Tech**; overlapping clouds do not stack their DoT or basic attack miss penalty. `Poison` is a semantic status TAG, Nature is the damage element and Physical is the channel. These do not authorize new universal Nature/Tech proc mechanics. [Thief Round5 design](classes/THIEF_TIER2_DESIGN.md).

**Thief newest Round6 element formulas (2026-10-22, DESIGN ONLY):** Envenom delivers INITIAL non-elemental Physical stab RAW2× normal basic, followed by attached Poison-status **Physical Nature** 3s nonstacking DoT with one1s tick of RAW `(2+0.10×AGI)×(1+0.05×(r−1))`; refresh duration but DON'T restart tick clock. Smoke Bomb's **Physical Tech** per-second total RAW `(2+0.08×AGI)×(1+0.05×(r−1))` is divided by4 for EACH of four0.25s ticks per second in its 5s 4m cloud; overlapping effects nonstack. Fan of Knives is ONE ordinary non-elemental Physical damage instance per enemy within centered4m radius, moving usable. No new Nature/Tech element proc rules or generic Poison damage element. [Thief detailed spec](classes/THIEF_TIER2_DESIGN.md).

**Thief Round7 class-specific delivery clarification (2026-10-22, DESIGN ONLY):** **Smoke Bomb PHYSICAL TECH AoE DoT exists ONLY on targets physically inside its live4m smoke zone**, ticking 4/sec at approved RAW per tick `[(2+0.08×AGI)×(1+0.05×(SmokeRank−1))]/4`; damage AND 20% BASIC miss end immediately when target leaves. Overlapping clouds do not stack. **Envenom's attached PHYSICAL NATURE Poison-tagged3s,1tick/sec DoT persists on victim after moving away**, per tick `(2+0.10×AGI)×(1+0.05×(EnvenomRank−1))`, without restarting clock on reapplication. **New THIEF-036B correction:** Envenom direct Physical STAB that breaks Stealth gets the Stealth opener multiplier as does a breaking BASIC; applying the multiplier to its attached Poison ticks is still a pending question, so don't invent a new elemental proc or indiscriminately amplify every tick. [Thief Round7 authoritative design](classes/THIEF_TIER2_DESIGN.md).

## Nature

**Nature is explicitly a DiceFree element**, regardless of whether its exact DiceBound status changes before DiceFree implementation.

Nature covers themes such as:
- plants;
- thorns;
- roots;
- spores;
- earth/sand where appropriate;
- animal/primal magic;
- storms or natural forces when a specific other element is not a better fit.

Nature does not have to mean "healing."

The current Magically Touched direction can evolve Novice Magic Sand into a Nature-element spell.

A later **Nature Shaman** class/lineage is explicitly desired.

## Architecture

Elements are independent of the Physical/Magical damage channel.

The element system should be registry/data-driven:
- stable element ID;
- display name/icon/VFX hooks;
- resistance interaction;
- optional statuses/procs;
- damage/healing compatibility;
- class/item/enemy usage;
- no giant switch statement required to add another element.

DiceFree does not need to copy DiceBound's exact numeric proc percentages or turn-based effects. It inherits the **identity vocabulary**, then adapts mechanics to real-time combat.
