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
