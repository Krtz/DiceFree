# DiceFree Tier II classes — approved decisions & revised design v0.2

> **Greatblade update (Oct 13):** The Greatblade kit, unlocked levels and evolving Mastery passive have been refined by the user. See [Greatblade design v0.3](GREATBLADE_PROTOTYPE_DESIGN_V03.md) and [Unity visual prototype](GREATBLADE_PROTOTYPE_IMPLEMENTATION.md). **Tier IV+ only** can equip slot-9 ultimates; earlier v0.2 wording about low-tier quest ultimates is obsolete.


**Date:** 2026-10-13
**Status:** User-approved class identities and system direction, with explicitly provisional skill details and future branches. **Design only** — no implementations, models, quest chains or art imported by this document.

> **LATEST USER-LOCKED WORLD 1 STANDARD TIER II ROSTER (2026-10-19):** Exactly **EIGHT normal Tier II classes** — **Physically Blessed:** Greatblade, Defender, Ranger, **Thief**; **Magically Touched:** Mage, Priest, **Acolyte**, **Psion**. Psion is **APPROVED as an ordinary eighth class**, NOT a secret class or a still-optional candidate. Complete remaining normal Tier II designs **Acolyte → Thief → Psion**, THEN design the **Quillboar area**, THEN the **major/big city**. Some Tier II advancement NPCs **MIGHT** relocate to that city; nothing moved or altered yet. **NO Tier III class design in World 1: defer all Tier III until World 2.** Historical Greatblade/Defender/Ranger/Priest Tier III names and secret convergence ideas below are *archived future concepts*, not current work or approved design tasks. **DESIGN ONLY**, do not alter Unity content in this chat. Authoritative roster and workflow: [Class progression](CLASS_PROGRESSION.md); active next interview: [Acolyte Tier II](classes/ACOLYTE_TIER2_DESIGN.md).

Supersedes `TIER2_CORE_CLASSES_DESIGN_V01.md` on all conflicting points. Preserve agreed base concepts from v0.1 unless revised below.

## Core progression (existing rules, unchanged)

- Novice L10 branches into **Physically Blessed Novice** or **Magically Touched Novice** (Tier I), each starting L1.
- Tier I reaches **L30** to qualify for Tier II advancement. New Tier II manifestation begins L1 and **does not overwrite the parent**. A later Tier II progression threshold is **L60** for a possible Tier III branch; Tier III's next threshold is L120, then L200 for Tier IV.
- Echo-wide milestones can unlock convergence classes without overwriting other class saves. Unlock requirements must support class-specific conditions, levels, and nested AND/OR.
- The previous suggestion that every class has *four active skills plus one passive* is explicitly **rejected as a universal design rule**. Vary skill/passive/active counts per class, even 2 active + 3 passive or up to 7 skills, according to class identity.

## Agreed four Tier II class identities

| Class | Parent | Weapons | Scaling/resources | NPC trainer |
|---|---|---|---|---|
| **Greatblade** (name **approved**) | Physically Blessed | **Oversized two-handed sword**, both hand slots | STR-first, Momentum (proposed) | Retired swordswoman in Cornberg |
| **Defender** (design agreed) | Physically Blessed | **One-handed sword + shield** | STR/VIT, Resolve (proposed) | Distinct Tier II veteran among Trimery gate guards |
| **Mage** (name/design agreed) | Magically Touched | **Wand + orb** (user-proposed, strong recommended distinction) | **INT ONLY for offensive/spell scaling**; Mana; no added SPR scaling | Retired mage brewer, swordswoman's partner, in Cornberg |
| **Priest** (design agreed) | Magically Touched | **Staff** (user-proposed) | SPR/INT as appropriate; **two real resources: Mana + Grace** | Active healer NPC in Trimery, replacing a healing well |

**Weapon/silhouette family proposals**, to reserve future differentiation:
- **Mage:** wand and orb. Wand projects elemental spells; orb is a catalyst or implement in off-hand, not an unrelated shield or book. Mage does NOT require an ordinary staff.
- **Acolyte** (future magical route/name not final): wand + **book/grimoire**; forbidden lore/death/spirit traditions distinct from Mage elemental magic. Book must have meaningful active/passive system, not just cosmetic.
- **Priest & Druid** (future Druid): **staff-focused** classes; Priest holy-support staff, Druid organic/nature staff (later Druids may also shapeshift). Retain weapon variety even if they use same category.
- Greatblade 2H sword; Defender sword & shield; future Ranger bow, Rogue daggers remain possible.

## Greatblade — approved core, updated future graph

**Confirmed:** Greatblade is the Tier II name, a heavy two-handed swordswoman-inspired martial damage class with meaningful Momentum and big cleaves/finisher; mentor is retired swordswoman.

**Initial ability proposal, details still to playtest:**
- **Wide Arc:** close frontal cleave.
- **Driving Charge:** closing strike (cannot ignore solid collision).
- **Overhead Breaker:** consume Momentum in a deliberate boss/single-target sword finisher.
- **Battle Cry:** offensive/team utility.
- **Heavy Rhythm** passive: successful sword attacks build Momentum and a short rhythm benefit; not a permanent hyperspeed class.

**New user-requested evolution possibilities (keep as a graph, not a compulsory single line):**
```
Physically Blessed Novice (Tier I)
     |
 Greatblade (Tier II)
     |-- Berserker (potential Tier III): relentless rage / high risk + damage
     |-- Swordmaster (potential Tier III): timing / disciplined sword skill
     |       |-- Samurai (potential Tier IV): an advanced refined blade identity
     |-- Witcher* (secret/convergence Tier III candidate):
              requires Greatblade advancement eligibility AND
              Echo-wide milestone: Magically Touched Novice >= level 30
```
* `Witcher` is a **working class name**. Check final publishing/trademark/visual-narrative differentiation before shipping: don't copy the CD Projekt / Andrzej Sapkowski Witcher characters, iconography, signs or signature aesthetic. Could later name it Hexblade, Witch Hunter, Spellhunter or another original title. No decision yet.

**Important:** The cross-lineage unlock checks the **same Echo**, not active manifestation or party member. The exact milestone user requested is a **level-30 Magically Touched Novice**; merely starting that class, or reaching L30 as a different mage branch, should not count unless user revises. Advancement still occurs from an eligible Greatblade manifestation, typically at its own L60 requirement; don't make a special class earlier automatically. Berserker orc identity is an existing *intent*, not confirmed new appearance or restriction. Samurai's ordering **Swordmaster → Samurai** is a sensible *recommendation*, not a user-approved final Tier IV structure.

## Mage — INT scaling, elemental kit, wand/orb

**Confirmed corrections to v0.1:**
- Mage has **INT as its only spell-scaling primary**. Do not scale Mage spells/passive potency off SPR. Shared world-level non-primary attributes (such as VIT/HP) still function normally.
- **No Arcane damage, no Arcane Ward** in this class's base kit.
- Defensive spell is **Math-elemental shield**, tying into the DiceBound/DiceFree element system rather than inventing generic Arcane.
- **Wand and orb** is the preferred equipment concept, pending actual item/equipment restrictions and player appearance test.

**Proposed first kit:**
1. **Ember Bolt** — Fire direct spell, basic repeatable ranged damage.
2. **Frost Snare** — Ice slow/control spell; don't spam permanent freeze/stun.
3. **Arc Lightning** — Electric damage with chaining/positioning rewards.
4. **Math Barrier** (working name) — Math-element defensive ward on self/ally. It could mitigate incoming damage and manipulate the next hit by a small deterministic amount, or grant a limited chance to confuse attacker, but **choose exact Math mechanics after comparison to existing Math element**; do not implement an Arcane shield recolored blue.
5. **Elemental Weaving** passive — benefit from intelligently alternating schools; Math counts as a school in its identity, yet defensive rotation must remain useful when solo or versus resistant foes.

All abilities use Mana until a distinct second resource is approved. Different schools enable later elemental specialist branches without forcing specialization at Tier II. The brewer NPC remains a retired **Mage** in practical brewing garb with advanced wand/orb gear within reach. No room-consuming staff required.

## Priest — two resource bars by design

**Confirmed:** Priest consumes **Mana on every active skill** (even those empowered by Grace) and has a separate **Grace meter**. Grace provides *additional* power to both **healing and damaging spells**, not just healing. Solo Priest must be a real fighter with damage rotation, group Priest must heal effectively.

**Proposed resource behavior, not numerically locked:**
- `Mana` is the spell cost and regenerates through regular class Mana rules.
- `Grace` is earned through **effective damaging attacks and meaningful support/healing**, with anti-overheal/anti-dummy-spam safeguards.
- A spell with sufficient Grace becomes **Empowered**, optionally spending some/all Grace when activated; the **Mana cost always still applies**. Empowering can change damage, heal amount, shields or secondary effect. Show Mana + Grace bars in HUD and exact tooltip costs/bonuses.
- No all-or-nothing reliance on Grace: at empty Grace, heals and damage casts still work for Mana cost.

**Proposed first kit:**
1. **Judgment:** Mana-cost Holy/Light damage; can be Grace-empowered for stronger hit or secondary burst.
2. **Mend:** Mana-cost targeted/self heal; Grace empowers its throughput or small bonus protection.
3. **Sanctuary:** Mana-cost small area heal on allies **and damage to enemies**; Grace empowers both without trivializing encounter positioning.
4. **Aegis:** Mana-cost short protective shield; Grace can strengthen shield and/or briefly reduce incoming damage.
5. **Shared Light** passive: rewards fighting/helping allies, drives legitimate Grace gain rather than noncombat mana waste.

**Mentor/service:** a proper Tier II staff-wielding village Priest **in Trimery**, taking the role of a living healer instead of static healing well. Actual NPC heal service can be configured independently from the later advancement quest. Visual distinction from generic Mage is crucial.

## Defender — original v0.1 concept (updated design interview available)

> **Newer Defender user interview (2026-10-19):** [Defender Tier II design document](classes/DEFENDER_TIER2_DESIGN.md). The Defender name and Trimery gate-defense quest direction are confirmed; **Resolve gains from incoming damage BEFORE mitigation and expressly Resolve-generating skills**. **Round 2 confirms the core mechanics of all five original skills**: Shield Bash charges a short distance to its target and interrupts/stuns; Challenge is real AoE threat/taunt; Bulwark spends Resolve but continues pre-mitigation Resolve generation; Shield Sweep generates Resolve and grants nearby allies bonus **Physical AND Magical DEF per enemy hit**; Hold the Line primarily improves personal defense/Resolve with a weak **+1 elemental resistance per rank** ally aura, whose radius exactly matches Shield Sweep's ally-buff radius. **Round 3 also confirms** that Shield Bash follows the existing **Do Nothing / Move Into Range** targeting option; Shield Sweep grants **flat +1 PDEF AND +1 MDEF × skill rank × up to five enemies hit**, including Defender; the shared allied radius is **5 meters**, and Hold the Line grants **+1 percentage point to every elemental resistance per passive rank**, using strongest identical aura. Resolve is **0–100** with very slow decay and pre-mitigation hit-based generation. A separate high-defense **2-second damage-storage counterattack (+500 PDEF and MDEF per skill rank, releasing accumulated pre-mitigation damage)** is documented as a **possible future Tier III skill**, NOT approved as Tier II or guaranteed Tier III. **Round 4 adds a SIXTH Tier II skill** (working name **Iron Guard**): +500 Defense per skill rank for two seconds while the Defender is **unable to act**; **damage storage/release belongs to future Tier III**. Resolve decays **1 point per 3 seconds** (combat-state rule open), Shield Sweep's rank×up-to-5-target DEF buff lasts **4s**; same-caster recasts **replace**, different Defenders' buffs **stack**, and Shield Bash range is confirmed at **5m**. **Round 5 confirms**: Iron Guard adds **+500 FLAT Physical DEF and +500 FLAT Magical DEF per rank**, lasts 2s, costs **20 Resolve**, initial **20–25s cooldown target**; enemy CC does **not end the DEF buff**, but actual CC immunity is still open (recommend NO CC immunity). Resolve decays by **1 point per 3s OUTSIDE combat only**, never in combat. **All six skill caps 18/10/16/20/10/6 = 80 total and exact rank gates are now APPROVED** as initial playtest design, with a legal upgrade every level 1–80. The user has requested Ranger next; [new Ranger design interview](classes/RANGER_TIER2_DESIGN.md). Other balance coefficients/cooldowns remain under design. The latest global elemental shield efficiency from this discussion is **0.8 shield HP per matching damage blocked, 1.2 per nonmatching**, recorded in [Stats and Damage](STATS_AND_DAMAGE.md). Stronger-specialized Guardian/Vanguard names are potential Tier III paths. **Design only, no Unity implementation here.**

- **Sword and shield**, VIT + STR, active tank/support play through **Resolve**.
- Initial proposed abilities: **Shield Bash** (impact/interrupt), **Challenge** (real threat), **Bulwark** (defensive stance), **Shield Sweep** (close AoE and frontline safety).
- **Hold the Line** passive; solo damage must remain workable and shield performance needs clear feedback.
- Upgrade **one actual Trimery guard** visually and narratively to Tier II Defender; leave the other five ordinary.
- Later Guardian/Templar/Juggernaut are possible successors, not committed.

## New skillbar / quest ultimate architecture — design requirement

The existing bar has **12 numbered slots** in two rows of six, and fixed universal commands:
- **Slots 1–8:** flexible class ability slots. These may show **active abilities, passive skills, auras or no icon**, in any combination class designers choose. Passive and aura icons are noncastable but hoverable and upgradeable via the inline '+' where appropriate.
- **Slot 9:** **reserved ULTIMATE slot** for a class's late-game quest-unlocked ultimate. Show a distinctive border or locked icon before its quest is completed; do not consume this slot for a routine fifth/sixth skill. Some classes may never get their ultimate until much later in their lineage.
- **Slots 10–12:** unchanged universal **Attack/Attack-Move**, **Interact**, **Return to Revive Stone**.
- A class with 2 active + 3 passive uses five slots plus the locked/unlocked slot 9. A class with seven ordinary abilities/passives fills seven of 1–8, leaving one ordinary slot for growth.
- **Class identity determines ability-count**. No general enforcement of four active/one passive. Some heavily passive-driven or pet-driven classes may have different proportions.
- Ultimate gating by an **authored quest** and possibly a class/echo milestone, not only automatically receiving it at a level. On completion, skill appears in the reserved slot and remains available via relevant manifestation persistence; rank-ups may be quest-driven, not automatically normal skill points. Define per-class rules later.
- Preserve skill points, save state, remappable hotkeys, tooltip rank tracking, self/ally targeting, cooldowns and existing channel bars for actual channeled ultimates.
- **Implementation status:** This is a design decision for future code. The current 3 starter manifestations still implement a fixed four-active/one-passive mapping; simply editing these docs does not add a dynamic skillbar or ultimate system.

## Art / model sourcing (reviewed in current catalog, not downloaded)

Existing local packs remain valid candidates: `Female Warrior Princess`; `FREE - Modular Character - Fantasy RPG Human Male` (has actual two-handed attacks and SpellCast/CastingLoop); `Modular Fantasy knight character`; `Low Poly Modular Armors - Free`; `Viking Shields pack`; `Free Low Poly Swords`; `Fantasy Staff Lite`; `Magic Effects FREE`; `Free Game VFX - Magic CircleURP`. Ward/grace VFX should respect actual Math/Light gameplay elements and accessibility settings.

**New source: [BlendKit / BlenderKit](https://www.blendkit.com/?query=category_subtree:model+order:-created+availability:free+rig:true+sexualizedContent:)**
- Site's official `/docs/licenses/` lists **CC0** and **Royalty Free**, both allow commercial games. RF forbids resale/redistribution as raw model/asset pack and requires game art not be simply extractable as assets from shipped binaries. `/docs/licenses/licensing-faq/`, `/terms-and-conditions-2026/` support this.
- **Prefer CC0 for public GitHub source**; use RF for compiled game assets only when distribution conditions can be met, with originals in ignored/local folders and an in-repo fallback. Never assume **free** = CC0.
- `rig:true` means a model advertises a rig, **not guaranteed animation clips**. Verify humanoid skeleton, bone deformation, animation presence, hand sockets, pose, clip loops, number of meshes/materials, triangle count, LOD, textures and required Blender/FBX export steps. Unity Humanoid Avatar retargeting needs bone mapping validation.
- Prioritize a **female 2H greatsword capable model** for retired swordswoman/Greatblade; one **wand+orb capable Mage**; one **staff Priest**; and an authentic **shield rig** for Defender. Test at in-game isometric camera distance and ensure legible silhouette.
- Keep source URL, creator, license type, acquisition timestamp, local asset path and modifications in future provenance manifest. No actual user-selected BlendKit assets downloaded or checked into Git at this stage.

## Open follow-up design details

1. Mage **wand and orb**: is the orb an equippable offhand with separate stats and customizations? Recommended **yes**.
2. Name and final mechanical formula for **Math Barrier**. Avoid declaring the ultimate confusion proc before checking current Math element.
3. **Ultimate slot 9** quest timing and rank-up system for different classes; exact icons/lock UI not implemented.
4. Priest Grace: desired max stack/cap, generation rate and whether empowered casts auto-spend or opt-in; no speculative balance numbers yet.
5. Greatblade Tier III: keep Berserker (raw fury), Swordmaster (technique), Witcher (secret dual-lineage); Samurai possibly later Swordmaster progression.
6. No Tier II player avatar appearance or training quest is finalized until actual packs are inspected and user reviews in-game previews.

**No code, scene, skill, stats or class unlock changes have been applied by v0.2.** `main` remains untouched.
