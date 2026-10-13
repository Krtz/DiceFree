# DiceFree Tier II — Four Core Classes (Design Draft v0.1)

**Date:** 2026-10-13
**Status:** PROPOSED FOR REVIEW — NOT LOCKED, no classes, quests, models, or NPC behavior implemented by this document.

## Existing rules to preserve

- Novice becomes **Physically Blessed Novice** or **Magically Touched Novice** at level 10. At **Tier I level 30**, a manifestation may advance into a new **Tier II class starting at level 1**. The parent remains available, the class has its own persistent save-state, and Echo-wide achievements can later be used for hybrid/secret class prerequisites.
- Class stat foundations: STR, AGI, INT, SPR, VIT; separate Physical/Magical Defense with diminishing returns. Healers should **contribute real damage**, play alone without misery, and be desirable in parties; shields, targets, cooldowns and control follow existing combat rules.
- **Four active ability slots (1–4) and one visible/upgradeable passive (5)** initially; 6–9 reserved for future class skills. Shared Attack/Interact/Return remain fixed in 10–12. Spell and passive ranks use in-bar '+' skill points; no separate talent screen invented for these classes.
- Physical classes should have distinct melee weapon silhouette, hit timing and defensive play; magical classes should differ beyond merely spell color. No arbitrary single-sex/race decision until an actual candidate rig and class body are approved.
- Items: real equipment types and compatibility with existing 11-slot paperdoll/back conventions; class-specific weapon constraints are data-driven. No dyes; transmog is separate and shared at Echo scope.
- Art selection: use locally downloaded pack assets first, confirm actual prefab/rig/material quality in Unity, adapt/retarget/recolor, keep raw licensed files out of public source. All suggestions below are **leads, not validated/implemented asset picks**.

## Provisional roster

| Tier II class | Parent | Intended combat role | Weapon | Resource/identity | Trainer |
|---|---|---|---|---|---|
| **Warrior / Greatblade** (name open) | Physically Blessed | Heavy 2H physical cleave/burst DPS, reasonable solo durability | Two-handed sword, occupies both hands | **Momentum** gained by actual melee contact | Retired swordswoman from the Cornberg adventurer couple |
| **Defender** (name open) | Physically Blessed | Shield tank/protector; solo capable | Sword + shield | **Resolve** from blocking, controlling and absorbing attacks | A distinguished Trimery gate guard/captain |
| **Mage** (name open; Wizard reserved for possible later mastery) | Magically Touched | Generalist elemental/arcane ranged DPS and control | Staff, wand/focus, or tome to be decided | **Mana + Spellweave**: alternate elements for efficiency/bonus | Retired mage, now brewer, partner of the swordswoman |
| **Priest** (name open) | Magically Touched | Primary healer **with a meaningful damage rotation** | Staff, mace/focus or tome to be decided | **Mana + Grace**, built partly by offensive spells | New Trimery village healer, replacing healing-well services |

This is the initial **four-class design pass**, not a requirement for exactly four physical and four magical branches. Ranger, Rogue, Druid, Acolyte and other classes can have their own schedules and graph/convergence rules.

## 1. Warrior / Greatblade — two-handed striker

**Class promise:** The biggest blade on screen; every attack feels impactful rather than fast. STR primary, VIT secondary, modest AGI for attack recovery. Two-handed **sword specifically**, not axe or shield. 2H consumes both equipment hand slots. Not a primary tank, although solo-capable.

**Core gameplay:** Melee hits and connecting skill strikes fill **Momentum** (provisional 0–100). Momentum is spent on a powerful finisher. Misses or losing contact hurt efficiency, but ordinary auto-attacks remain useful. Slow, heavy animation with deliberate cleave range and clear startup; no hidden animation/collider mismatch. Avoid universal stun-lock.

**Initial four active skills:**
1. **Wide Arc** — sweeping physical cleave in a frontal cone; crowd-damage baseline and good way to build Momentum.
2. **Driving Charge** — short committed rush/sword impact to reach prey or interrupt smaller enemies; cannot travel through walls or other units without an explicit ghosting effect.
3. **Overhead Breaker** — heavy single-target finisher consuming accumulated Momentum for stronger impact, potentially a short armor-break effect.
4. **Battle Cry** — short offensive self/team utility buff; helps the Warrior solo and brings group value without being a tank.

**Passive: Heavy Rhythm.** Consecutive successful sword attacks build Momentum and slightly accelerate the next committed swing; losing combat resets the streak. Later ranks increase reliability rather than pure enormous speed, preserving the heavy silhouette.

**Retired swordswoman NPC:** A capable veteran in civilian work clothes with one old piece of plate and a conspicuously oversized greatsword on a rack or back. She may fight only during future advancement story moments. Existing Cornberg NPC story remains; do not make her a second generic shopkeeper. Player-class silhouette is much more heavily armoured than her day-to-day look.

**Visual candidate packs:** `Female Warrior Princess` for female NPC rig/gestures; `FREE - Modular Character - Fantasy RPG Human Male` for usable two-handed attack, spin and run animations (retarget as needed); `Free Low Poly Swords` / `Fantasy Moon sword` for an oversized greatsword; `Low Poly Modular Armors - Free` for layered steel. **Do not assume the female animation pack already has complete 2H clips; verify/retarget.**

**Future branch examples, unapproved:** Swordmaster (precision/tempo) versus Berserker (aggression) or a separate Berserker route. User previously wanted orc Berserker; retain that possibility instead of silently assigning it to this lineage.

## 2. Mage — versatile spellcaster

**Class promise:** A real trained magician who knows several schools; **no permanent elemental specialization yet**. INT primary, SPR secondary, VIT lower. Uses a casting focus / staff, fights at range, controls groups, but can burst a solo target.

**Core gameplay:** Mana is the primary cost. **Spellweave** passive encourages alternating spell elements/schools rather than spamming one button; the effect must remain readable in tooltips and target feedback. Resistances and element interactions obey existing global mechanics, not a Mage-only damage table. Cast/channel indications use shared channel UI once those skills are implemented.

**Initial four active skills:**
1. **Ember Bolt** — reliable direct Fire spell and standard damage filler.
2. **Frost Snare** — controlled Ice burst/zone that slows or briefly roots rather than repeatedly hard-stunning; usable defensively in solo play.
3. **Arc Lightning** — Electric spell that chains or jumps on nearby enemies, stronger when targets are grouped.
4. **Arcane Ward** — a short-duration absorb shield / utility spell for solo safety, with a modest magical pulse or cast-efficiency benefit; not a Priest replacement.

**Passive: Elemental Weaving.** Landing two **different** schools in sequence grants a small efficiency or empowered-spell benefit; avoid mandatory rigid 1-2-3-4 combo for casual play. Mage is still useful versus enemies with particular elemental resistances.

**Retired brewer NPC:** Experienced adventurer now making drinks, surrounded by casks, herbs, copper piping and one incongruously dangerous-looking staff or spellbook. Civilian apron over faded travelling robes; magical competence is shown through subtle brewing/bubbling effects, not a permanently firing battlefield VFX. The existing retired adventurer couple relationship should remain intact.

**Player look:** Deliberate spellcaster silhouette with long coat or robe, oversized staff/focus, visible sleeves and a readable cast pose; neutral violet/brass fabric rather than all-fire branding. Mage may become Wizard, Arcanist or Elementalist later, but do not lock elemental school/race with this Tier II choice.

**Asset leads:** `Fantasy Staff Lite`, `FREE - Modular Character - Fantasy RPG Human Male` (SpellCast/CastingLoop/Buff), `Magic Effects FREE`, `Free Game VFX - Magic CircleURP`, `48 Particle Effect Pack`. Inspect animation poses and materials for camera readability.

**Future branches:** Elementalist (different elemental emphasis), Wizard/Arcanist (pure general mastery), perhaps Runemaster/Chronomancer later. Distinct Fire vs Ice wizard classes belong later if we decide that split is fun.

## 3. Priest — damage-capable village healer

**Class promise:** The clearest early **dedicated healer**, never a helpless group-only alt. SPR primary, INT secondary, VIT moderate. Magical range and healing, modest protective magic, no heavy-armor/shield tanking by default.

**Core gameplay:** Mana for baseline spell costs; generate **Grace** (or a comparable simple charge) by dealing damage and/or landing helpful support spells. Grace makes the next heal/protection cast stronger or cheaper. This gives a solo damage rotation and a party triage loop without forcing a healer to abandon the team to build resources. Heal the character by targeting the self portrait as already supported. No endless self-sustain exploit.

**Initial four active skills:**
1. **Judgment** — Holy/Light single-target damage, reliably generates Grace; main solo filler.
2. **Mend** — fast targeted heal usable on self or ally; may consume Grace for added healing, preserving overheal constraints.
3. **Sanctuary** — positioned brief circle that **heals allies and damages enemies** (gentler than pure DPS AOE); clear telegraph and cooldown.
4. **Aegis** — short protective shield / damage-reduction blessing on self or ally; existing shield priority rules apply.

**Passive: Shared Light.** Effective offensive spell hits and meaningful healing give controlled Grace generation; small group healing benefit at higher ranks without auto-healing everyone to full.

**Trimery NPC:** A respected active village healer inside a treatment house (rather than a static healing well), with cots, bandages, warm lanterns and people receiving care. Healing service applies through NPC interaction with clear cost/conditions; it does not replace the Dice resurrection anchor. NPC visibly performs short healing animations or a warm light spell on occasion.

**Player look:** Light cloth/robe with readable holy symbol/censer, oversized book/staff, soft ivory/gold/teal light. Different from Mage: clean, grounded silhouette and supportive field effects, not constant elemental eruptions.

**Asset leads:** `Fantasy Staff Lite` (remodel/crozier), `FREE - Modular Character - Fantasy RPG Human Male` spellcasting clips, `FX Magic Plane 20 free`, `Free Game VFX - Magic CircleURP`, `Magic Spells - 40 Fantasy Icons 512x512 HD`. A suitable priest/caregiver mesh still needs actual catalog inspection; avoid asserting we already have one.

**Future branches:** High Priest (healing), Inquisitor (offensive discipline), Oracle/Hierophant (support). Not decided.

## 4. Defender — sword-and-shield frontliner

**Class promise:** Safest frontline presence, not simply Warrior with a shield. STR/VIT primary, AGI minor. Sword + shield required; authentic shield-hand equip/animations. Solo damage is respectable but below 2H Warrior in equivalent encounters.

**Core gameplay:** **Resolve** generated by successfully intercepting damage, maintaining threat and landing sword/shield attacks. Tanks should contribute to encounters without needing three other players to feel functional. Facing and ally-protection are readable; shield collisions are visual/action gameplay, not a permanent free hitbox blocking roads.

**Initial four active skills:**
1. **Shield Bash** — melee hit with short interrupt or stun against vulnerable targets, modest damage.
2. **Challenge** — force/raise enemy attention using the game's actual threat/targeting system; meaningful against packs, not a cosmetic red flash.
3. **Bulwark** — brief active guarding stance with notable damage reduction and limited mobility or offensive cost, useful for telegraphed hits.
4. **Shield Sweep** — close-range cone that hits nearby enemies, builds Resolve and can briefly protect close allies; avoids a tank with no AOE/solo capability.

**Passive: Hold the Line.** Improves Resolve stability and grants modest extra resilience when maintaining a frontline position; increases party protection without indefinite invulnerability.

**Trimery NPC:** Upgrade **one existing gate guard** into a distinct veteran guard captain: substantial shield, reinforced leather/plate, visible rank/sash and unique idle/guard stance. Other five guards remain ordinary guards. Initially an NPC, future advancement trainer.

**Player look:** Broader shield profile than any other class, short sword, heavy shoulders, shield impact/recovery animations and strong, planted movement poses; distinct from Warrior even zoomed out.

**Asset leads:** `Modular Fantasy knight character`, `Low Poly Modular Armors - Free`, `Viking Shields pack`, `Free Low Poly Swords`, `FREE - Modular Character - Fantasy RPG Human Male` BlockingLoop/MeeleeAttack_OneHanded. Confirm real one-handed + shield rig sockets, not just static offhand ornament.

**Future branches:** Guardian (protection), Templar/Paladin (martial magic if a magic convergence condition is met), Juggernaut (bruiser) — names/requirements unconfirmed.

## Advancement story hooks — for later implementation

- Warrior and Mage mentors are the existing retired swordswoman and mage-brewer in **Cornberg**. Keep their couple relationship; their jobs/retirement make the class revelation more interesting. Unlock quests should reveal their capabilities through deliberate play, not a menu-only class button.
- Priest trainer is the **Trimery healer**, who replaces the health well as an in-world healing service and eventually offers a Tier II healer advancement quest.
- Defender trainer is **one distinctive Trimery guard**, possibly responsible for civilian safety against nearby Ironpine wolves. No generic guard gains tier powers automatically.
- Common unlock requirement: eligible parent at Tier I **level 30+**, a meaningful authored mentor quest, then instantiate that Tier II class's new level-1 manifestation without deleting the Tier I parent. Class availability must follow existing directed advancement graph.
- Future Warrior+Mage milestones can qualify an **Echo-wide convergence/secret** class such as Psion/Battlemind, accessible through a preserved eligible lower-tier manifestation. **Concept only**, do not add it to the four standard branches now.

## Visual/asset production checklist

1. First validate actual model rig, weapons, one-hand/two-hand casting and shield grips for a representative player avatar of each class.
2. Create a distinct 360° NPC/player silhouette for each, suitable for the isometric WC3-style camera.
3. Provide idle, walk, attack, hit, death, skill-cast and class-special animations; explicit forward orientation and projectile attachment points.
4. Fit real capsule/NavMesh footprint, avoidance and no-overlap behavior. VFX must be readable and optional graphics under accessibility settings.
5. Derive portrait/inspection render and ability icons from selected local art; keep code-defined fallbacks and third-party raw assets private.
6. Test abilities/skills with the existing inline green '+', rank tooltips, target selection, self-casting and overhead HP/channel displays.

## Approval questions (not yet locked)

- Name of two-handed sword class: **Warrior** (broad familiar) vs **Greatblade** (precise/unique); shield class **Defender** (clear), caster **Mage** (generalist) and healer **Priest** (accessible) are provisional.
- Should Warrior's second major action be a **charge** or a **counterattack**? Charge gives a distinct gap-closer, counter creates a deliberate heavy duelist.
- Mage: should elemental variety be central at Tier II, or should it have two generic arcane attacks with Fire/Ice/Lightning spread among later ranks?
- Priest: heal service free in Trimery or require a modest service fee? Does it heal HP only or also remove statuses?
- Defender: does Challenge produce hard taunt for a few seconds or only a large threat increase, particularly in 1–4-player encounters?
- Avatars' racial/gender choices should be selected after inspecting actual source packs and before importing artwork.

**Implementation status:** No Tier II manifests, skills, models, animations or mentor advancement quests created yet. Do not merge designs into main as confirmed canon without user review.
