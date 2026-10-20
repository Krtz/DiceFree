# DiceFree — Thief Tier II — design interview

**Created:** 2026-10-20  
**Status:** USER APPROVED NORMAL WORLD 1 TIER II CLASS; THIEF DESIGN ROUND 1 AWAITING ANSWERS. No ability, class mechanic, advancement quest, silhouette or number of skills finalized. **DESIGN ONLY; NO UNITY C#/SCENE/ASSET/BUILD/SAVE CHANGES.**  
**Branch:** `integration/world1-foundation`  
**Links:** [Class Progression](../CLASS_PROGRESSION.md), [Physically Blessed Novice](PHYSICALLY_BLESSED_NOVICE.md), [Tier II Core Classes v0.2](../TIER2_CORE_CLASSES_DESIGN_V02.md), [Stats and Damage](../STATS_AND_DAMAGE.md), [Combat](../COMBAT.md), [Class Requirements](../CLASS_REQUIREMENTS_AND_DISCOVERY.md), [Ranger Tier II](RANGER_TIER2_DESIGN.md), [Completed Acolyte Tier II](ACOLYTE_TIER2_DESIGN.md).

## Confirmed World 1 design scope

Exactly eight ordinary Tier II classes in World 1:
- Physically Blessed Novice → Greatblade, Defender, Ranger, **Thief**.
- Magically Touched Novice → Mage, Priest, Acolyte, Psion.

**USER APPROVED CLASS AND SEQUENCE (2026-10-20):** Acolyte Tier II design is now completed as a reference baseline. **Thief is the NEXT ACTIVE design interview**, then Psion. After all eight normal Tier II designs, design the Quillboar area and major city. Some Tier II advancement mentors **MAY** move to the city, but their location and quests cannot be silently changed in this chat. **No Tier III class designing until World 2.** No secret class design during the current normal Tier II workflow.

## Grounded foundations vs unapproved ideas

**Already confirmed:**
- Thief is one of the **four standard Tier II Physically Blessed Novice branches**, eligible for advancement when the parent reaches **Tier I class level30**; a **new child manifestation at Tier II class level1**, not overwriting the parent. Keep the shared Echo class/manifestation state and ability purchasing rules.
- Generic physically blessed player gameplay is a STR/AGI melee generalist with Mana, but **Thief class resource, preferred attribute and attack pattern are NOT approved** merely from lineage.
- Shared combat framework supports explicit per-class attack styles, hand-slot equipment, DoT/status tags, positional or dodge mechanics, conditional critical hits, and independent owner effects; **none are automatically unlocked on Thief**.
- The original `docs/TIER2_CORE_CLASSES_DESIGN_V02.md` listed **Rogue daggers as a possible weapon/silhouette proposal**, NOT as a completed Thief item-class restriction.
- For recent Tier II classes, weighted class skill caps/gates target **roughly 80 purchasable ranks through Tier II level80**, with skills beginning rank0 and 1 point earned at level1 and per further level, but Thief's exact skill roster, rank caps and availability gates are NOT YET AUTHORED.

**DESIGNER SUGGESTIONS, NOT USER DECISIONS:**
- **AGI-leaning mobile melee physical damage dealer** with dual short blades, opportunistic positioning, quick chained basic strikes, selective dodges, and debuff/pickpocket-style tricks could differentiate Thief from Greatblade heavy Momentum attacks, Defender sword+shield tanking and Ranger physical ranged archery. But no permanent invisibility, positional damage bonus, dual wield rule, poison, crit multiplier, combo resource or special steal system should be assumed without explicit agreement.
- Potential visual class direction: dexterous nimble rogue in soft gear/hooded burglar silhouette; whether to use one male/female class body, race/species, oversized daggers and clothing are open.
- Could be **Thief** (scrappy dirty-tricks identity) rather than **Rogue** (stealthy professional) depending on user's fantasy. User currently approved name *Thief* as the branch label, while final shipping name can be changed explicitly if later desired.
- One possible non-tank melee party function: exploit boss openings and provide limited target debuffs without universal stun locks; should remain viable in solo play and on CC-immune bosses.
- Class advancement NPC/story **NOT SELECTED**. Don't require a theft or crime quest in Cornberg or Trimery without user approval, and don't invent big-city NPC coordinates before city region is designed.

## Round 1 — EXACTLY FIVE questions (AWAITING USER)

**THIEF-001 — Combat fantasy:** Should Thief lean toward a **fast dagger assassin** (bursts/critical backstabs), a **nimble duelist** (mobility/dodges/consistent attacks), or a **dirty-tricks burglar** (bleeds, tricks, theft/weakening)? A deliberate mixture is possible, but what should be its signature?

**THIEF-002 — Weapons and autoattacks:** Do you picture **two one-handed daggers** occupying the weapon and offhand slots, **a single dagger plus free offhand**, or another setup? If dual daggers, should **both daggers attack independently/alternately**, or should the offhand merely add item stats while the Thief's basic animation swings both?

**THIEF-003 — Stealth and positioning:** Should ordinary Tier II Thief have **true stealth/invisibility**, a **short vanish/reposition trick**, or **no stealth** at Tier II and instead reward backstabs/side attacks? Consider solo boss fights where enemies can't always be approached from behind.

**THIEF-004 — Class resource and movement:** Should Thief remain **Mana-only**, use a second **Combo Point/Guile/Trickery** resource generated by basic attacks and skills, or use something like **Stamina/Energy** for frequent quick attacks? We can keep movement responsive (WASD + click-to-move) and avoid adding a resource bar without meaningful gameplay.

**THIEF-005 — Identity and advancement direction:** What body/race/look do you imagine for Thief? And for the **Tier II advancement mentor/quest**, should we leave the exact NPC and location for the future big city, or brainstorm a Thief trial in current Cornberg/Trimery now? These are design questions only; no map/NPC moved.

## Decision ledger

| Date | ID | Decision | State |
| --- | --- | --- | --- |
| 2026-10-19 | WORLD1-ROSTER | Standard Tier II Thief under Physically Blessed, one of four physical/eight normal Tier II | **USER CONFIRMED** |
| 2026-10-20 | WORLD1-ORDER | Acolyte completed → Thief now → Psion next → Quillboar → major city; Tier III World2 | **USER CONFIRMED** |
| 2026-10-20 | THIEF-FOUNDATIONS | AGI/daggers/stealth resource details not chosen; older Rogue dagger silhouette remains suggestion | **DESIGNER ONLY** |
| 2026-10-20 | THIEF-001–005 | Five Round1 questions | **AWAITING USER** |
