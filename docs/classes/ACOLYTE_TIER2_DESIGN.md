# DiceFree — Acolyte Tier II — design interview

**Created:** 2026-10-19  
**Status:** WORLD 1 STANDARD TIER II CLASS, ROSTER APPROVED; CLASS KIT/MENTOR/QUEST NOT YET DESIGNED. Awaiting Round 1 answers. **DESIGN/DOCUMENTATION ONLY, NO UNITY IMPLEMENTATION.**  
**Branch:** `integration/world1-foundation`  
**References:** [Class Progression](../CLASS_PROGRESSION.md), [Magically Touched Novice](MAGICALLY_TOUCHED_NOVICE.md), [Tier II Core Classes v0.2](../TIER2_CORE_CLASSES_DESIGN_V02.md), [Stats and Damage](../STATS_AND_DAMAGE.md), [Class Requirements and Discovery](../CLASS_REQUIREMENTS_AND_DISCOVERY.md).

## User-approved World 1 roster and design order

There are EXACTLY EIGHT normal World 1 Tier II classes, four under each Tier I parent:

- **Physically Blessed Novice →** Greatblade, Defender, Ranger, Thief.
- **Magically Touched Novice →** Mage, Priest, **Acolyte**, Psion.

**Next design interview Acolyte → then Thief → then Psion**, then Quillboar area, then big city. **Tier III is for World 2; DO NOT design any Tier III class skills/advancements now.** The user may revisit some mentor locations after the city is designed, but has NOT yet approved moving existing mentors or quests.

## Earlier foundations to preserve

1. **APPROVED AS NORMAL CLASS IN ROSTER (2026-10-19):** Acolyte belongs to **Magically Touched Novice at Tier I level 30** and starts as a new **Tier II level-1 manifestation**, preserving the Tier I parent state, in line with all standard advancement rules. Acolyte's *name as final shipping title*, specific skill kit, body, quest and numeric combat rules are still open.
2. **EARLIER DOCUMENTED EQUIPMENT CONCEPT** from `docs/TIER2_CORE_CLASSES_DESIGN_V02.md`: **wand (mainhand) plus book/grimoire (offhand)** for Acolyte, distinct from Mage's wand+orb and Priest's staff. **The book MUST be mechanically meaningful**, not just a cosmetic offhand; exact mechanic not approved yet.
3. **EARLIER DOCUMENTED FANTASY:** forbidden lore, occult magic, death or spirits. Be different from Mage (elemental/general INT damage), Priest (healing/Grace), and the newly approved but yet-undesigned Psion (mental-force identity). Avoid making Acolyte a second Mage, Priest or Psion.
4. **TIER I ROOTS:** `Magically Touched Novice` uses Mana, magical ranged autoattacks and existing targeting UX. **Acolyte's resource remains OPEN**—it may use Mana alone, or Mana with a clearly purposeful secondary resource. No invented class-specific resource bar without user selection.
5. **GLOBAL ABILITY/RANK ECONOMY:** all new Tier II skills start **rank 0**, the Tier II character starts level1 with **1 spendable skill point** and earns one per level until the total configured rank capacity. Recent Greatblade/Mage/Priest/Defender/Ranger designs target **~78–80 total class-skill ranks by class level80**, and skill-specific rank-level eligibility should permit a point purchase each class level. Exact Acolyte skills/max ranks/gates will be designed AFTER its combat kit is selected, not preselected.
6. **LOCATION/QUEST OPEN:** some new Tier II advancement NPCs could eventually live in the future big city, which the user wants designed AFTER the Quillboar area; do not define a city map/NPC coordinate or change other mentor locations here.

## Initial unapproved class concepts (discussion seeds ONLY)

- **Occult cursecaster:** significant damage over time, debilitation and deliberate curse maintenance on enemies. Bosses still susceptible to damage and weaker debuffs even if immune to hard CC.
- **Spirit reader / ritualist:** temporary spirit attack manifestations (e.g. ghostly hands or a spirit arrow), not automatically a permanent summoned minion or Necromancer pet system.
- **Grimoire choices:** authored glyphs/pages, a book charge that augments certain cast categories, or a spell that reads/consumes a grimoire entry. The grimoire needs **actual gameplay behavior**, but should not introduce swappable spell loadouts if they conflict with DiceFree's fixed class kits.
- **Magic damage element:** no generic Arcane element is approved in DiceFree; if curses/death magic use an existing DiceBound/DiceFree element (e.g. Radiation, Math, Light, etc.), obtain user agreement on actual effect before implementation. Alternatively use **non-elemental Magical** damage with explicit curse status; do not invent a new element automatically.
- **Early spell candidates (not approved):** *Occult Bolt* (basic active), *Withering Curse* (single-target DoT), *Spirit Leech* (damage/self-sustain), *Grimoire Seal* (offhand-related effect), *Ritual Circle* (zone control), *Forbidden Knowledge* (passive). No six-skill roster or exact counts yet approved. These are merely prompts for discussion and can be replaced.
- **Character look:** a wandering scholar, masked cult reader or haunted archivist silhouette rather than standard wizard robes. Species/sex/armor and animations need user design; the character can look eccentric without forcing villain alignment.

## Round 1 — five questions (AWAITING USER)

**ACOLYTE-001 — Core gameplay fantasy:** Should Acolyte emphasize **curses and damage-over-time**, **spirits and temporary supernatural attacks**, or a deliberate mix of both? How dark/forbidden do you want it to feel compared with Mage and Priest?

**ACOLYTE-002 — Wand and grimoire:** We previously envisioned a **wand mainhand + grimoire offhand** with a meaningful book mechanic. Should a grimoire provide **different authored effects based on the equipped book**, build/consume **pages or charges during combat**, or allow **specific curse spells to be modified based on inscriptions**? We should preserve the fixed class skill kit and avoid arbitrary freely swapped skill libraries.

**ACOLYTE-003 — Resource:** Would you like Acolyte to run on **Mana only**, or **Mana plus a small occult secondary mechanic** (e.g. accumulating curse stacks or Soul/Ink charges through successful spells)? My initial lean is **Mana with a grimoire-based stack/charge mechanic**, without a second big HUD resource bar unless it earns its place.

**ACOLYTE-004 — Role and solo play:** Should the class primarily be a **damage dealer that sustains itself with life-drain/hexes**, an **offensive debuffer/support** that strengthens allies' damage, or a hybrid? We can make the mechanics meaningful even against bosses immune to hard crowd control.

**ACOLYTE-005 — Advancement mentor/quest and location:** Since the **big city is designed later**, should we **leave the Acolyte mentor/advancement location OPEN until the city exists**, or design a **mysterious wandering book scholar near Trimery/Cornberg now** with a simple occult quest? No existing NPC location needs to change yet.

## Decision ledger

| Date | ID | Topic | State |
| --- | --- | --- | --- |
| 2026-10-19 | WORLD1-ROSTER | Acolyte one of eight **normal** Tier II classes under Magically Touched Novice; next design before Thief and Psion | **USER APPROVED** |
| Earlier | ACOLYTE-FOUNDATIONS | Wand + grimoire; occult/death/spirit fantasy; book meaningful, not cosmetic | **PRIOR DOCUMENTED DESIGN DIRECTION**, exact mechanics not confirmed |
| 2026-10-19 | WORLD1-ORDER | Eight normal Tier II → Quillboar area → big city; Tier III only in World 2 | **USER APPROVED** |
| 2026-10-19 | ACOLYTE-001–005 | Five initial Acolyte questions | **AWAITING USER** |
