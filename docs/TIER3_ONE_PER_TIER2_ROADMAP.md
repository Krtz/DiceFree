# DiceFree — Tier III coverage roadmap: at least one evolution per Tier II class

**2026-11-25 USER-APPROVED WORKFLOW / DESIGN PLANNING, NOT IMPLEMENTATION AUTHORIZATION**

## Objective and sequence

The user explicitly chose: **"try to make one tier three per class so all classes have a tier 3 option for now. Let's start with marksman, then move on to berserker and then so on."**

This means **at least one playable Tier III advancement choice for EVERY one of the eight normal Tier II World 1 classes**, before spending design time on numerous extra Tier III branches. **NOT** an arbitrary restriction of one Tier III *maximum* per Tier II. Existing extra evolutions remain available and future branches may be added later.

**Fixed immediate order:**
1. **Ranger → Marksman**, first new Tier III design round NOW. See [Marksman Tier III design](classes/MARKSMAN_TIER3_DESIGN.md).
2. **Greatblade → Berserker**, immediately after the Marksman design phase.
3. Other missing Tier II→Tier III options, to be selected collaboratively after these two; do not treat the unapproved candidate labels below as finalized.

## Coverage matrix

| Parent normal Tier II | Existing Tier III implemented / present intent | First-coverage design to complete | Status |
| --- | --- | --- | --- |
| **Psion** | **Psionic Knight** and **Mirage Mage**, reported by user on 2026-11-25 as fully implemented | Already covered; no need for a third branch in this pass | **USER REPORT: implemented**, not independently QA verified in this document |
| **Ranger** | No Tier III documented as complete | **Marksman** | **USER-SELECTED NEXT**; design interview begins |
| **Greatblade** | No Tier III documented as complete | **Berserker** | **USER-SELECTED AFTER MARKSMAN** |
| **Defender** | No Tier III documented as complete | Possible Guardian, Vanguard, or other concept | **UNDECIDED WHICH**, reserve previously approved future damage-storage/retaliation Iron Guard-style mechanic for potential Tier III |
| **Thief** | No Tier III documented as complete | Possible Assassin, Duelist, or Burglar | **UNDECIDED WHICH**, old idea bank remains tentative |
| **Mage** | No Tier III documented as complete | Possible Wizard, Sorcerer, or another magical specialization | **UNDECIDED WHICH** |
| **Priest** | No Tier III documented as complete | Possible Nature/Druid or Light/Hierophant/Prophet direction | **UNDECIDED WHICH** |
| **Acolyte** | No Tier III documented as complete | A new curse/DoT/necromantic/summon-focused evolution or another user choice | **UNDECIDED**, do not silently declare a 'Necromancer' class approved |

**Important:** Count eight normal Tier II classes: Greatblade, Defender, Ranger, Thief from Physically Blessed Novice; Mage, Priest, Acolyte, Psion from Magically Touched Novice. The user reported BOTH Psion evolutions fully implemented. Accordingly this coverage sprint has **seven not-yet-covered Tier II parent classes**, beginning with Marksman and Berserker. Do not automatically modify the existing Psion classes.

## Shared class-system boundaries

- The current general class advancement is **Tier II class level60 → new level1 Tier III manifestation**, preserving the parent timeline; other tiers and eligibility are per authoritative [Class Progression](CLASS_PROGRESSION.md).
- Default design interview style follows the successful Psion Tier III process: establish class identity, role, inherited/reworked/replaced skills, gear, main attributes, class resources, skills and exact rank gates, then a separate discovery/mentor trial and first-playtest numeric specification.
- **Psion-specific** five-skill rosters, 150 exact rank gates, hidden teachers, clone behavior, Mana formulas and dungeon trials do **NOT** automatically transfer to Marksman or any other Tier III. Reuse global Tier III cadence where applicable, but choose ability identities/rank caps and testing numbers for EACH class deliberately.
- The Ranger's existing AGI-only normal ranged basics, class-defined bow range, both bow families, Hunter's Mark, Hawk Dive and existing skills are respected as parent reference mechanics; nothing is retroactively changed in Tier II.
- **Documentation only this round.** The user said to start design, not to edit Unity, trigger builds or merge code. The current branch is `feat/verdant-wilds`. Preserve Codex working code.

## Marksman first design round

See [Marksman Tier III](classes/MARKSMAN_TIER3_DESIGN.md) for the Ranger baseline, preliminary identity and first **five open design questions**. All answers are pending. Update the canonical design documents after each new user decision.
