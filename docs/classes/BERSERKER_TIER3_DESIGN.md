# DiceFree — Berserker Tier III (Greatblade evolution) — design interview

**2026-11-25 — Round 1 user-approved identity; Round 2 open.** This is a **DESIGN document** and NOT a declaration of Unity implementation or balance verification.

## Lineage and core identity (Round 1 USER-APPROVED)

- **Parent:** Greatblade (Physically Blessed Tier II). **Berserker is primarily a DIRECT UPGRADE / continuation of Greatblade**, not an unrelated dual-wield, bleeding, tank or spell-sword class. Existing Tier II Greatblade remains separately preserved under the shared Tier III advancement/manifestation framework; full unlock quest and starting stats not yet authored.
- **Primary weapons:** **two-handed greatswords**. Do not silently permit dual-wielding or replace the greatsword identity; future item-specific restrictions are separate.
- **Resource:** **REPLACE Momentum with Rage**, generated when the Berserker **DEALS AND RECEIVES DAMAGE**. The detailed gain coefficients, what counts as damage, caps, decay, cost/spend mechanisms, death/out-of-combat handling and prevention of self-farming remain **OPEN**. Do not accidentally retain Momentum as a second simultaneous resource.
- **Core risk/reward:** **LOWER CURRENT HP increases outgoing damage.** User chose HP-sensitive damage over 'high Rage reduces armor', HP-spending casts or primarily burst windows. Exact damage function, HP threshold, minimum HP, cap, healing behavior, and how Rage interacts with low-HP multiplier are **OPEN**. Do not silently add a defense penalty or HP costs.
- **Combat focus:** **SUSTAINED melee damage** during longer fights. Avoid interpreting the class as primarily burst, huge AoE or a tank; normal melee cleave remains available as inherited Greatblade identity.
- **RETAIN four Greatblade abilities/identities:** **Spin Dash, Overhead Breaker, Heavy Rhythm, Wide Arc**. Evolutions of rank scaling, exact names and mechanics for these are yet to be designed, but preserve their core recognizable use. The user did NOT select Battle Cry; it is **not part of the currently approved carried-over roster**. The current Tier II Greatblade also has Greatsword Mastery; the Round 1 multiple-choice did not ask about it, so **whether a Berserker evolution keeps Greatsword Mastery is UNDECIDED**, not expressly rejected.
- User requests **assistant design guidance** on complementary Greatblade Tier III branches, to keep Berserker's identity distinct, and notes this melee archetype is not their usual playstyle. Offer proposals as **noncanonical ideas for comparison only** rather than inventing approval.
- **150 total Tier III skill ranks** is the current suggested consistency goal (as with other Tier III classes), NOT separately reconfirmed by the user's answers in Round 1. No skill caps, unlock gates, attribute numbers or advancement quest approved.

### Existing Greatblade reference (Tier II, not automatically Berserker caps)

The current class-progression documentation lists Greatblade Tier II skill caps **Wide Arc20, Spin Dash16, Overhead Breaker18, Battle Cry8, Greatsword Mastery8, Heavy Rhythm8** = **78**. This is background only. Tier III purchases start from rank0 under established Tier III principles unless otherwise explicitly designed; do not just copy 78 parent skill ranks.

### Possible sibling Greatblade Tier III directions — DESIGNER IDEAS, NOT APPROVED

1. **Berserker:** direct, sustained **greatsword melee damage**, Rage from combat and stronger offense at low HP; keeps Wide Arc, Spin Dash, Overhead Breaker and Heavy Rhythm. This is the **only confirmed** sibling.
2. **Blademaster / Sword Saint:** **technical greatsword duelist**, precise timing, parries/counterstrikes and punish windows, consistency at high HP, no Rage or health-risk focus; possibly weapon mastery or combo-based precision. Distinct from Berserker by execution rather than self-endangerment.
3. **Juggernaut / Dreadnought:** **armored unstoppable greatsword bruiser**, stagger resistance, damage absorption and deliberate heavy swings; contrasted with Berserker's risk-heavy offense. Keep separate from Defender's established dedicated shielding/tanking role by emphasizing offense and anti-interruption, not taunt/shield. Could also be a future Tier IV concept.
4. **Warbringer / Warlord:** **party-supporting battle commander** with battle cries, ally melee buffs, team positioning and control. This would be a natural home for Greatblade's *unselected* Battle Cry identity and clearly different from Berserker's personal sustained DPS.

**Avoid canonizing these alternatives or their names without user approval.** Other possibilities (e.g. lifesteal blood knight, elemental sword warrior) can be explored in later discussions.

## Round 2 — suggested five decisions (NOT ANSWERED)

1. **Rage function:** Should Rage primarily *passively increase damage* while held, be *spent* to empower abilities, or support both? Define whether Rage builds from raw/pre-mitigation vs final actual damage dealt/taken; ensure no infinite gain from shields/DoTs without design.
2. **Low-HP damage scaling:** Smooth continuous bonus with missing HP, or meaningful breakpoints (e.g. 75/50/25% HP)? Decide a conservative first-playtest cap; make bonuses update immediately when healed.
3. **Existing ability upgrades:** How should Wide Arc, Spin Dash, Overhead Breaker, and Heavy Rhythm evolve? Should Heavy Rhythm become a **Rage management passive**? Preserve parent identity without blindly copying Momentum text.
4. **Additional abilities:** Should Berserker gain a **survival tool** (e.g. brief damage reduction/lifesteal at low health), a Rage spender finisher, a frenzy attack-speed buff, or a control/anti-interruption move? No fixed roster size until agreed.
5. **Sibling Greatblade evolution priorities:** Which of **Blademaster (technical/parries)**, **Juggernaut (durable bruiser)**, **Warbringer (team commander)**, or an original alternative should be reserved as a *possible later* evolution? This is a differentiation/vision discussion, NOT commitment to designing all branches now.

## Workflow

**After EVERY answered design-question round:** update relevant GitHub design docs, commit, **push** to active agreed development branch and report commit SHAs. Preserve concurrent Codex work; do not alter Unity source/assets during design-only rounds without explicit implementation request.

## Round 2 — 2026-11-25 (USER ANSWERS, priorities and older lineage reconciliation)

**Status:** Round 2 user directions recorded; this is still design only, not code, assets or playtest. Do not silently promote weapon/branch comparison ideas to canon.

### Round 2 answers: APPROVED / PREFERRED

1. **Rage resource name and gain:** User likes **Rage OR Anger**, delegates the choice. **Working class resource name: Rage** (chosen by designer to align with previous Round 1). Rage replaces Momentum and is generated from dealing/receiving damage. Whether held Rage provides passive damage, or only empowered spending, **remains open** because this question was not specifically answered; avoid inventing a passive high-Rage bonus.
2. **Low-HP output:** **SMOOTH/GRADUAL scaling with missing HP APPROVED**, rather than discrete 75/50/25% breakpoints. Exact damage curve, multiplier cap and defenses remain open, design should allow Berserker to deal viable DPS at high HP.
3. **Heavy Rhythm:** **APPROVED** to evolve from Momentum management into a **Rage-management passive** that improves Rage gain and retention. Quantitative gain and decay/rank effects remain open.
4. **Self-sustain should NOT use lifesteal tied to damage numbers.** User is concerned about exceedingly large late-game damage and HP values making lifesteal explode. **PREFERRED direction: spend/consume Rage for a discrete heal, shield or other defensively useful effect**, using HP%/max HP or other bounded carefully tuned formulas instead of a percentage of raw damage output. **Exact ability form (heal vs shield or mix), cost, cooldown, scaling and thresholds are not yet settled.**
5. **Sibling class preferences:** **Warbringer → Warlord** is a favored **future evolution chain**, focused on **Battle Cry development and placeable buff banners**. Not an approved finalized Tier III/Tier IV skill tree, tier assignment, or request to implement now. User is also open to **multiple distinct viable tank archetypes** beyond Defender, requiring genuinely different defensive/resource identities.
6. **Weapon discussion OPEN:** User proposes maybe Berserker should FAVOR **two-handed AXES** while also equipping qualifying **two-handed swords**; maybe Swordmaster/Blademaster should focus sword/katana; asks whether every Greatblade evolution must use 2H weapons and whether Assassin should be dual-wield. **Existing Round1 2H greatswords remains the confirmed preference until user explicitly settles a change.** Do not silently switch Berserker to axes or canonize Assassin dual-wield. Allow future specialization vs broad equip eligibility distinction as a designer recommendation only.

### Verified OLDER DiceFree history, not a newly invented branch

[Core Tier II v0.2](../TIER2_CORE_CLASSES_DESIGN_V02.md) explicitly includes:
```
Greatblade Tier II
  |-- Berserker, Tier III candidate
  |-- Swordmaster, Tier III candidate (precision / disciplined swordplay)
  |      |-- Samurai, possible Tier IV (advanced refined blade)
  |-- Witcher*, secret hybrid Tier III candidate
         requires eligible Greatblade and same-Echo
         Magically Touched Novice manifestation level 30+
```
The exact Samurai path **Swordmaster → Samurai** was written as a *recommendation* and was **not confirmed final**. `Witcher` is a **working name** that should be replaced/differentiated from existing outside fictional IP before publication. [Core Tier II v0.1](../TIER2_CORE_CLASSES_DESIGN_V01.md) previously floated **Defender → Guardian / Templar / Juggernaut**, so Juggernaut is **historically a DEFENDER candidate**, not an established Greatblade sibling; if moving its role to a Greatblade tank path, that requires a distinct new class decision. `Warbringer → Warlord` with battle cries and placeable banners is a NEWLY USER-FAVORED additional path as of Round 2, not present in this old evolution graph.

### Tank archetype comparison and possible differentiation (DESIGNER NOTES, NOT LOCKED)

FFXIV four tank identities: **Paladin** shield/party protection, **Warrior** high-HP/strong personal sustain with beast gauge, **Dark Knight** greatsword/dark magic/resource barriers, **Gunbreaker** gunblade/cartridge offense and planned mitigation. WoW six tank specs: **Protection Warrior** active shield blocks, **Protection Paladin** holy mitigation/protection utility, **Blood Death Knight** reactive post-hit healing and Death Strike, **Guardian Druid** bear-health/armor, **Brewmaster Monk** stagger/delayed damage management, **Vengeance Demon Hunter** mobile souls/magic damage and self-sustain. These are INSPIRATIONS, not mechanics to copy directly or claims of one game providing unique archetypes absent in the other.

Potential DiceFree tank identities:
- **Defender:** existing sword/shield **Resolve** and ally protection/control; dependable conventional tank.
- **Juggernaut:** armor/stagger/poise or delayed damage absorption; tank by preventing knockbacks, soaking damage and controlling position, not copying Defender's shield/buffs. Could branch from Defender as older docs suggested, or be reconsidered under Greatblade *only after approval*.
- **Dark/Mana barrier tank:** manage absorb shields and resources, closer to FFXIV Dark Knight than Defender, perhaps a future hybrid/evolution separate from Berserker; should not inherit a class identity by default.
- **Reactive damage-conversion tank:** bank staggered or mitigated damage and reduce/pay it off with skillful sequencing (Brewmaster-inspired). Existing user idea of a damage-storage/retaliation Iron Guard-style class should not be silently reassigned.

### Round 3 unresolved questions to ask

1. **Weapons:** Approve change from Berserker's Round1 greatsword-only specialization to **2H axes preferred, compatible 2H greatswords equipped**, and should Swordmaster favor 2H swords while Samurai ultimately favors katana?
2. **Rage function:** Primarily **spend** Rage on attacks and defense, or also a passive bonus while stocked? Define maximum Rage and its gain/decay direction.
3. **Low-HP power:** Select a mild bounded linear percent bonus vs stronger curve and maximum at low HP; ensure high-HP DPS remains viable.
4. **Self sustain:** Choose **Rage-consuming heal**, **Rage-consuming shield**, or a hybrid and plan bounded scaling based on Max HP, VIT, or resource spent rather than percentage of damage inflicted.
5. **Identity/tank lineage:** Tentatively reserve **Swordmaster→Samurai**, **Warbringer→Warlord**, old **secret hybrid**; decide whether **Juggernaut remains Defender** or becomes a separate Greatblade-derived tank and whether Berserker retains Greatsword Mastery in evolved form.

**Reminder:** after every completed answer round, update relevant docs, COMMIT and PUSH.

## Round 3 — 2026-11-25 (USER ANSWERS: axes, Rage spending, missing-HP bonus, elemental shield, branching)

**USER-APPROVED first-playtest decisions, subject to balancing:**

1. **Weapon specialty changed:** Berserker **favors two-handed AXES**, but **may also equip any eligible two-handed SWORDS**. This updates Round1's preferred greatswords. Future Swordmaster/sword and Samurai/katana concepts are separately **tentative**, not finalized equipment policy for those classes.
2. **Rage is a SPENDABLE resource**, not an additional passive damage multiplier simply for being held. Rage is gained by dealing and receiving damage (Round1), with Heavy Rhythm increasing generation and retention (Round2). How much gained per damage, resource cap, decay, skill costs and spending outputs remain open. Do not combine default stored-Rage damage multipliers with the already approved low-HP modifier.
3. **Gradual missing-HP damage:** Accept **up to +40% outgoing damage at 1% current HP** as a **first-playtest target**, smoothly scaling with HP missing, not discrete thresholds. Suggested mathematical interpolation candidate `bonus = 0.40 × clamp((1-currentHP/maxHP)/0.99,0,1)`, pending precise formula/rounding/interaction tuning. At full HP bonus =0; at 1% HP bonus=+40%; no unbounded amplification at 0 HP. Ensure useful DPS at full HP, and combat testing with healing/shields.
4. **Survival ability concept:** **Spend Rage to gain an ELEMENTAL SHIELD lasting X seconds** (element, cap/amount, cost, duration, cooldown and whether the shield follows global matching/nonmatching elemental rules still open). Rather than adding conventional lifesteal, use bounded shield scaling. User offered an alternative where any shield REMAINING ON EXPIRY turns into healing, then suggested perhaps **reserve that shield-to-heal effect for a Tier IV Berserker evolution**. This Tier IV reservation is **a suggestion rather than a confirmed trigger/rule**; Tier III shield itself is approved. Avoid asserting healing-on-expiry already belongs to Tier III or Tier IV until resolved.
5. **Sibling evolution identities accepted in principle:** **Berserker** (sustained Rage/missing-HP melee), future **Swordmaster→Samurai** (technique), future **Warbringer→Warlord** (Battle Cry and deployable buff banners), plus a secret *Witcher-style* same-Echo martial/magic convergence candidate (working name requires IP-distinct alternative). The latter three are **future lineage intentions, not approved ability numbers/implementation**. **Juggernaut may be Defender OR Greatblade** depending on future Defender evolution coverage. User does **not** require perfectly symmetrical numbers of branches but wants to avoid extremely lopsided offerings, e.g. one ancestor gets 1 while another gets 37; meaningful differences matter more than numeric equality.

### Verified previous Defender evolution candidates

The historic [Tier II Core Classes v0.1](../TIER2_CORE_CLASSES_DESIGN_V01.md) mentions **Guardian (party protection), Templar/Paladin (martial magic, possible convergence), Juggernaut (bruiser)**. [v0.2](../TIER2_CORE_CLASSES_DESIGN_V02.md) also uses Guardian, Templar, Juggernaut; later [Tier III Roadmap](../TIER3_ONE_PER_TIER2_ROADMAP.md) proposes **Guardian or Vanguard** and reserves **damage-storing/retaliation Iron Guard-inspired mechanics** for a later Defender path. None of these Defender Tier III class names is finalized by Round3. Potential distinct tank design identities: Guardian ally protection, Templar elemental/magical mitigation and support, Vanguard mobility/interception, Juggernaut damage delay/poise and punishment management. Avoid assigning the same combat mechanic to several classes automatically.

### Next design decisions (NOT YET APPROVED)

- Decide the **element** of Rage shield, cap/duration/cooldown and resource cost; confirm whether its remaining strength converts into HP **only at Tier IV** (possibly only on natural expiration).
- Decide Rage generation/maximum/decay, spent amounts and whether Heavy Rhythm modifies both gain and decay.
- Decide exact bounded +40% missing-HP multiplier and interactions with skills, shields and global damage rules.
- Decide what evolved Greatsword Mastery becomes now that Berserker favors two-handed axes; possibly replace with two-handed weapon mastery while keeping shared greatsword eligibility.
- Audit total Tier III skill rank cap and unlock/advancement quest later.

**Documentation-only interview:** No Unity implementation, asset, build or QA result is implied.

## Round 4 — 2026-11-25 (USER ANSWERS: coffee/fire shield, Tier IV expiry heal, 100 Rage, weapon mastery, two offensive concepts)

**Round 4 user directions, with APPROVED choices distinguished from OPEN variants:**

1. **Shield element:** user said **"i lean coffee... but fire could work as well"**. Preserve **COFFEE as the user's preferred whimsical element/theme**, **FIRE as acceptable fallback**. Neither is a finalized gameplay element assignment: check the canonical element system before authoring an unsupported "Coffee" damage element. Could use coffee-themed visuals/name with an existing Fire element if the user agrees; do not decide on their behalf.
2. **Tier IV shield upgrade:** USER APPROVED **RESERVING leftover elemental-shield strength converting to HP on expiry for the future Tier IV Berserker evolution**, not Tier III. Tier III currently grants a bounded Rage-cost elemental shield with no expiration heal. Exact Tier IV triggering on natural expiry vs destruction, healing cap and formula remain open, but it is now a planned meaningful Tier IV improvement.
3. **Rage resource:** USER APPROVED an **initial max of 100 Rage**, generated by dealing/receiving damage, **decaying outside combat**. Heavy Rhythm increases Rage gain and improves retention (previous approvals). Exact resource gain coefficients, normal vs out-of-combat decay pace, boss/DoT/shield rules and spending costs OPEN. Rage is for **spending**, not a passive held-Rage damage multiplier by default.
4. **Weapon mastery:** User likes the classic D&D feat name/nod **"Great Weapon Master"** (or approximate title) for Berserker's evolved weapon mastery supporting **two-handed axes as preferred and eligible two-handed swords**. Treat **Great Weapon Master as a favored WORKING skill name** and evolving mastery direction; exact benefits/rank cap/effect on 2H axes vs swords to design. The D&D tabletop feat is a naming inspiration, not permission to copy its mechanics verbatim.
5. **Offensive toolkit:** User likes **BOTH** an attack-speed **Frenzy** and a heavy Rage-consuming **finisher**, but explicitly asks how many skills Berserker has and which could belong to other Greatblade evolutions. **Do not claim both already finalized as distinct skill slots.** Current accounting: four retained from Greatblade **Wide Arc, Spin Dash, Overhead Breaker, Heavy Rhythm** (4); new user-approved **Elemental Shield** (1); favored evolving **Great Weapon Master** (1); proposed **Frenzy** (1); proposed **Rage Finisher** (1) => **6 carried/confirmed-preferred ability concepts +2 candidate additions = up to 8**, if all separate. A reasonable designer candidate is to **make Overhead Breaker the heavy Rage finisher and add Frenzy as one new skill**, yielding **seven total concepts** without a duplicate high-damage button. That consolidation is **NOT yet approved**. Another option is eight separate rankable skills, still within the flexible Tier III ability count and the 150 total suggested rank budget (150 not separately signed off for Berserker).
6. **Cross-class roles / tank coverage:** User wants a **viable Greatblade-derived tank option** and a **Defender-derived DPS option**, with **Vanguard** floated as the latter. These are **user-expressed class family goals**, not approved specific equipment/skills/lineage binding for either new evolution. Defender→Vanguard as an aggressive shield/DPS candidate; possible Greatblade→Juggernaut (or other tank name), with historical Defender→Juggernaut still a candidate until reassigned. More than Defender should be able to tank; user is specifically interested in unusual Season of Discovery identities (Warlock/Rogue/Shaman tanks, Mage healer) rather than only conventional plate-and-shield roles.
7. **Verified contemporary inspiration, not DiceFree approval:** FFXIV's 2026-announced **Bastion** (Evercold, scheduled Jan2027) is a main tank wielding **two greatshields called skyltborg** (official Square Enix July25 2026 announcement: https://press.na.square-enix.com/en-US/BASTION-REVEALED-AS-FIRST-NEW-JOB-FOR-FINAL-FANTASY-XIV-EVERCOLD ). WoW Season of Discovery expanded tank roles to **Warlock Metamorphosis, Rogue, and Shaman**, and Mage healing (https://www.wowhead.com/classic/guide/season-of-discovery/new-class-role-combinations ). Possible inspiration: metamorphic tank, agility/avoidance tank, magic-shield tank, delayed-damage tank. **Do not copy exact outside job names/art/mechanics uncritically or treat those inspirations as approved class branches.**

### Current ability accounting and sibling differentiation

| Berserker concept | Status as of Round 4 | Design function |
| --- | --- | --- |
| Wide Arc | Retained approved | Cleave |
| Spin Dash | Retained approved | Melee movement/AoE |
| Overhead Breaker | Retained approved | Heavy strike; could become Rage finisher (candidate) |
| Heavy Rhythm | Retained approved | Rage gain/retention; missing-HP scaling may be integrated (candidate, not yet approved) |
| Elemental Shield | Approved concept | Rage-spending timed shield; coffee flavor favored, Fire fallback, expiry heal **reserved Tier IV** |
| Great Weapon Master | Favored evolved mastery name/concept | Two-handed axe/sword mastery; numbers pending |
| Frenzy | User likes it; standalone ability not yet finalized | Attack speed for sustained DPS |
| Separate Rage Finisher | User likes it; standalone ability not yet finalized | Large Rage-spending strike, may instead upgrade Overhead Breaker |

**Future other-branch differentiation:** Warbringer/Warlord receives Battle Cry, support banners, buffs; Swordmaster/Samurai precise timing/parries/katana; secret martial-magic hybrid retains cross-lineage magic concept; viable Greatblade tank branch could use delayed damage/poise/mitigation; proposed Defender Vanguard could specialize aggressive tank-to-DPS play. Keep these distinct and avoid letting every branch take same best abilities.

### Suggested next five decisions (NOT YET APPROVED)

1. Should Overhead Breaker become Berserker's **Rage-spending heavy finisher**, freeing a slot for standalone Frenzy, or keep a separate new finisher?
2. Should the missing-HP +40% damage bonus be intrinsic to Berserker, built into **Heavy Rhythm**, or an independent passive with purchased ranks?
3. For Elemental Shield, is **Coffee** an actual new damage element or just a visual/theme with a **Fire**-element shield? Decide element compatibility, expiry duration, Rage cost and shield scaling afterward.
4. Approve exact skill name **Great Weapon Master**, and how it boosts two-handed axes vs eligible two-handed swords.
5. For Greatblade tank/Defender DPS families, should **Juggernaut** be provisionally reserved under Greatblade and **Vanguard** under Defender, or should both lineage decisions remain open while defining tank/DPS role kits?

**Status:** Round4 documented, committed/pushed through GitHub workflow. **NO Unity implementation implied.**

## Round 5 — 2026-11-25 (USER APPROVED: Rage Breaker, Anger Management, Coffee shield, sibling roles)

**This is the newest USER-directed Berserker design round; supersedes historical questions/proposals above where they conflict. This update is documentation-only, NOT a Unity ability implementation.**

1. **RAGE BREAKER:** Rename/evolve parent **Overhead Breaker → Rage Breaker**. It is the **heavy Rage-consuming finisher**; **do not introduce a second redundant Rage finisher skill**. Rage Breaker replaces Overhead Breaker in the Tier III kit, with rank-scaled coefficients, Rage cost, cast time/cooldown, and whether spent Rage increases its damage to be designed. Parent Greatblade's skill remains unchanged.
2. **ONE COMBINED PASSIVE, working name ANGER MANAGEMENT:** Rename/evolve **Heavy Rhythm → Anger Management** (user says "something like Anger Management"; name is favored rather than irrevocably fixed). Fold all effects previously proposed for separate **Great Weapon Master** into this same passive; **REMOVE a separate weapon-mastery rankable passive from Berserker's roster**. The combined passive covers **Rage gain and retention**, **smooth bonus damage as HP falls (initial target up to +40% at 1% HP)** and **general two-handed eligible weapon mastery**. **No exclusive bonus for two-handed axes**; Berserker *favors* axes stylistically, but mechanical weapon bonuses apply equally to eligible two-handed axes and two-handed swords. Exact rank-dependent modifiers, max rank and whether missing-HP effect begins at rank0 or requires purchase are still open.
3. **COFFEE IS A CANONICAL ELEMENT, not just fire-themed art.** User explicitly says "Coffee is an element. We have all the elements from DiceBound" and recalls a skill using it. **Verified current source:** `Assets/_DiceFree/Settings/Combat/Elements/Magical Coffee.asset` has `stableId: element.magical-coffee`, `displayName: Magical Coffee`; `Assets/_DiceFree/Scripts/Combat/AcolyteAfflictions.cs` and `AcolyteSkillCaster.cs` use it for Acolyte's poison effect. Berserker's **Rage-cost Coffee-element shield** must use the real shared element/resistance and existing shield compatibility rules, **NOT substitute Fire**. Match/nonmatch shield effectiveness follows global rules unless separately changed. The specific final shield ability display name (e.g. Coffee Shield), Rage cost, capacity, duration and cooldown are open. **Unused capacity → healing upon natural expiry remains RESERVED FOR TIER IV**, already approved Round4; TierIII shield does NOT heal on expiry.
4. **SIBLING LINEAGES, provisional future plans:** User agrees to provisionally reserve **Greatblade→Juggernaut** as a **TANK** choice, and **Defender→Vanguard** as a **DPS** choice, while Guardian and Templar remain prospective Defender tank/support evolutions. These are *future preferred branches*, not fully specced classes/committed implementation; preserve previous lore/design and distinct tank/DPS combat identities. The earlier Swordmaster→Samurai, Warbringer→Warlord (Battle Cry and planted party banners), and secret martial-magic lineage remain candidate future Greatblade branches.

### Current Berserker ability count (Round 5)

| Ability | Status | Role |
| --- | --- | --- |
| **Wide Arc** | Approved carryover | Sweeping melee cleave |
| **Spin Dash** | Approved carryover | Offensive mobility/AoE |
| **Rage Breaker** | **Approved** replacement for Overhead Breaker | Heavy Rage-consuming finisher |
| **Anger Management** | Approved merged passive concept, name favored | Heavy Rhythm's Rage management + missing-HP bonus + 2H weapon mastery |
| **Coffee-element shield** | Approved shield element and skill concept | Rage-consuming temporary elemental shield; no TierIII expiry heal |
| **Frenzy** | User previously liked its purpose, numbers and final independent slot not yet specifically signed off | Attack-speed buff for sustained melee offense |

**Accounting:** **Five approved core ability identities + one well-liked provisional Frenzy = six candidate rankable skills**. There is **NO separate Great Weapon Master**, **NO separate Rage finisher**, and **Battle Cry stays available to a future Warbringer**. TierIII 150-point skill budget is an established other-class precedent, not yet explicitly approved for Berserker. Keep class-specific skill counts flexible under the existing combat-bar design.

**Suggested next Round 6 decisions, OPEN:** (1) formally lock Anger Management as the passive name and Frenzy as the sixth skill, (2) specify how +40% missing-HP damage is apportioned across Anger Management ranks, (3) Coffee shield Rage cost/HP-based absorb/duration/cooldown, (4) Rage gain/decay and Rage Breaker cost/damage scaling, (5) initial rank caps and class stats, plus eventual teacher quest.


## Round 6 — 2026-11-25: USER APPROVED final five-skill roster

**LATEST DECISIONS, superseding older tentative sixth-skill Frenzy and a separate weapon-mastery passive. Design only; no Unity code has been changed.**

1. **Frenzy is REMOVED entirely.** User specifically says "frenzy is gone, it's not gonna happen." Instead **Rage Breaker grants a temporary ATTACK SPEED BUFF AFTER USING it**. Rage Breaker remains the single Rage-consuming heavy finisher evolving Greatblade's Overhead Breaker. No separately purchased or activated Frenzy and no duplicate finisher. Haste percentage, duration, rank scaling, trigger on ability execution versus successfully resolved hit, refresh/stacking and Rage Breaker cooldown remain OPEN.
2. **Anger Management** is the APPROVED combined passive name replacing Heavy Rhythm. Every purchased rank improves **Rage gain**, **two-handed axe/sword damage** (no axe-exclusive bonus), and the **gradual damage bonus for missing HP**. Selected ranks improve **Rage retention/reduce decay**. The initial max-rank missing-HP target remains **up to +40% outgoing damage at 1% current HP**; exact per-rank progression and rank0 baseline are OPEN. No separate Great Weapon Master skill.
3. **Coffee Shield is SELF-ONLY**, not castable on allies. User agrees to **25 Rage activation cost and 5-second duration** as first-playtest defaults. Shield capacity scales with skill rank and max HP; exact coefficient, cooldown, Coffee-element mitigation/other interactions TBD under shared combat rules. The Coffee element already exists in Unity with ID element.magical-coffee and is used by Acolyte's poison. **Unspent capacity converting into healing on expiration is reserved for a future Tier IV upgrade, NOT Tier III**.
4. **100 max Rage**, from dealt and received damage, **normalized relative to appropriate maximum HP** to avoid scaling explosions. The user agrees to this generation approach; numeric generation factors, what counts as damage vs shield, anti-exploit/caps and decay speed are still OPEN. Rage decays outside combat, and Anger Management improves retention. Holding Rage does not inherently increase outgoing damage; Rage is spent.
5. **Rage Breaker Rage-cost model is NOT decided.** The last answer removed Frenzy and attached its attack-speed effect to Rage Breaker; it did not answer whether Rage Breaker costs a fixed amount versus all Rage. A fixed cost, for example 30 Rage, remains a designer PROPOSAL.

### Berserker authoritative rankable-skill roster: FIVE abilities

| Skill | Role |
| --- | --- |
| Wide Arc | Evolved melee sweeping attack |
| Spin Dash | Evolved offensive movement and AoE |
| Rage Breaker | Rage-consuming heavy finisher with post-use attack-speed buff |
| Anger Management | Single passive: Rage gain/retention, missing-HP damage, general 2H axe/sword mastery |
| Coffee Shield (working ability name) | Self-only Coffee-element shield, 25 Rage / 5s first-playtest target |

**NOT present:** Frenzy, separate Great Weapon Master, another Rage finisher, Battle Cry. Five distinct skills, not six. The shared Tier III 150-rank precedent is still a **proposal** for Berserker, not specifically reapproved; rank caps, unlock levels, advancement teacher/quest and numerical damage/haste formulas remain open.

### Round 7 questions, NOT YET ANSWERED

1. Should Rage Breaker consume a fixed 30 Rage or all current Rage?
2. What attack-speed bonus and duration should Rage Breaker give, and does a blocked/missed attack qualify?
3. How should Rage gain from dealt/taken damage normalized by Max HP and out-of-combat decay be numerically defined?
4. How much does Coffee Shield absorb per rank and maximum HP, and what cooldown should it have?
5. Confirm 150 total Berserker rank purchases across five skills and first-unlock levels; proceed to eligibility audit, stats and quest design.


## Round 7 — 2026-11-26 (USER-APPROVED first-playtest values and 150-rank split)

This is the newest authority for the stated values; historical Round 6 questions asking about these specific choices are superseded. **Design/documentation only, not an implemented or tested Unity class.**

### Decisions approved this round

1. **Rage Breaker Rage cost:** **FIXED 30 Rage per activation** for first playtest, rather than spending all available Rage. Retains Rage for a possible Coffee Shield.
2. **Rage Breaker haste:** after using Rage Breaker, grant a **4-second attack-speed buff at +2% attack speed per purchased Rage Breaker rank**, up to **+60% at rank 30**. Repeat applications **REFRESH rather than stack** the haste. Whether blocked/missed attacks still count for the post-use buff, and exact hit/cast trigger integration, remain OPEN; do not silently settle them. Rage Breaker still replaces Overhead Breaker, and **Frenzy is removed**.
3. **Normalized Rage generation:** user APPROVED the direction of generating Rage proportionally to **damage dealt relative to the ENEMY's max HP** and **damage received relative to BERSERKER's own max HP**, with **per-hit caps** to protect boss and scaling interactions. Rage remains capped at **100**, decays outside combat, and Anger Management increases gain/reduces loss. Exact numeric gain multipliers, cap values, AoE/DoT aggregation and decay-per-second have NOT been chosen yet. "Sure" approves the model, not invented coefficients.
4. **Coffee Shield:** **self-only**, true **Coffee** elemental shield, costing **25 Rage**, lasting **5 seconds**, with **15-second first-playtest cooldown**. User OVERRIDES the suggested 10%→25% absorption: **5% of BERSERKER MAX HP at Coffee Shield rank 1**, scaling to **35% of MAX HP at rank 30**. A natural smooth linear designer implementation candidate is `ShieldCapacity(r) = MaxHP × [0.05 + 0.30×(r−1)/29]` for integer ranks **1..30**; user approved endpoints, not the precise curve/rounding. Keep all shared elemental shield and matching/nonmatching interactions. **Remaining absorb capacity converts to healing ONLY in a future Tier IV upgrade**, not Tier III.
5. **Exactly 150 Berserker skill-rank points, one per Berserker class level 1..150,** APPROVED. Exact skill caps, following user-requested transfer of **10 from Anger Management to Coffee Shield** and **5 from Rage Breaker to Spin Dash**:

| Berserker Tier III ability | APPROVED maximum rank | Role |
| --- | ---: | --- |
| **Wide Arc** | **35** | Sweeping sustained melee cleave |
| **Spin Dash** | **30** | Offensive mobility and AoE |
| **Rage Breaker** | **30** | 30-Rage heavy finisher; post-use +2% attack-speed/rank for 4s |
| **Anger Management** | **25** | Rage gain/retention, rank-scaled missing-HP damage and 2H axe/sword mastery, no axe-exclusive effect |
| **Coffee Shield** | **30** | 25-Rage 5s Coffee-element self-shield, 15s cooldown, 5%→35% MaxHP absorption |
| **TOTAL** | **150** | One skill point per Berserker level 1..150 |

**No separate Frenzy; no separate Great Weapon Master; no extra Rage finisher.** At max Anger Management 25, the previously approved low-HP damage bonus target still reaches **up to +40% at 1% HP**. General weapon damage/rank and Rage gain/rank still need values. Mana coefficients are unrelated to Berserker's Rage; do not invent per-class Mana exceptions.

### Open for Round 8

- Exact Rage generation percentages/weights **per dealt vs taken max-HP-normalized damage**, per-hit/AoE caps, decay-per-second, and Heavy Rhythm→Anger Management passive coefficients.
- Coffee Shield precise interpolation/rounding from 5%→35%, interaction with elemental shield shared rules and whether cooldown begins on cast or expiry (15s first-test baseline).
- Rage Breaker post-use haste trigger when a hit misses/is blocked, and Rage Breaker damage/rank coefficients; all offensive skill FLAT and STAT coefficients should scale with purchased rank under the established global design principle.
- First unlock levels and **150-rank full eligibility gate schedule** that allows a legal purchase each Berserker level 1..150, with final rank caps **35/30/30/25/30**.
- Berserker starting attributes, per-level stat gains, and Tier III advancement teacher/quest.

**Commit/push after each answered question round; never treat document approval as gameplay implementation.**


## Round 8 — 2026-11-26 (user decisions: linear Coffee Shield, confirmed-hits haste, OOC Rage decay, staggered unlocks; boss Rage unresolved)

**Most recent Round 8 user answers.** This is an **approved design discussion**, not a Unity implementation or a tested balance result. Where Round 8 introduces a concern, preserve it as an open decision rather than silently codifying a harmful formula.

### Approved decisions

1. **Coffee Shield rank scaling is LINEAR** between the previously approved rank1 **5% of MaxHP** and rank30 **35% of MaxHP**. Proposed exact first-test expression (now endorsed linear interpolation): for purchased shield rank r in 1..30, **absorption = current MaxHP × (0.05 + 0.30 × (r−1)/29)**. Decide integer HP rounding and snapshot/recompute semantics in implementation. Other confirmed parameters stay **self-only**, **Coffee element**, **25 Rage cost**, **5s duration**, **15s cooldown**; shield-to-heal after expiry is reserved for Tier IV only.
2. **Rage Breaker post-use attack-speed haste requires at least one SUCCESSFUL HIT on a valid enemy**. A fully **shield-absorbed hit counts as successful** (even with zero enemy HP damage); no target/no resolved hit/miss does NOT grant haste. Existing approved buff **+2% attack speed per Rage Breaker purchased rank for 4s**, refreshes rather than stacks; Rage Breaker costs fixed **30 Rage**. Determine ability's actual hit/miss/blocked resolution using global hit and shield systems; do not accidentally require HP loss.
3. **Out-of-combat Rage decays at BASE 5 Rage per second**, reduced by purchased **Anger Management** ranks, per earlier approved passive design. The amount of reduction per rank, whether decay pauses in combat, thresholds, and combat state transition mechanics are not yet specified; ensure nonnegative effective decay and no automatic Rage-to-HP conversion.
4. **Berserker's five rankable skills have STAGGERED FIRST UNLOCK LEVELS**, not all automatically available at Berserker level1. **Exact first unlock levels and per-rank gate schedule are OPEN** and must be authored/audited for the approved 150 points across Wide Arc35, Spin Dash30, Rage Breaker30, Anger Management25, Coffee Shield30. Preserve the baseline ability to buy one rank per Berserker class level1..150 and avoid dead-end levels.

### Rage gain: proposal accepted WITH explicit boss-fight objection (NOT FINAL)

The user selected the presented first-test proposal of **5 Rage per 10% of an enemy's maximum HP dealt** and **10 Rage per 10% of Berserker's own maximum HP received**, but immediately added a critical caveat: **"for number 3, that wont work against bosses? maybe 1% = 1 rage?"**. Record the originally selected OUTGOING candidate as **0.5 Rage per 1% target MaxHP damage**, and the user's **counterproposal** as **1 Rage per 1% target MaxHP damage**. The incoming first-test proposal is **1 Rage per 1% Berserker MaxHP damage actually received**. The outgoing rate/denominator is **REOPENED** because a boss with a very large HP pool would generate negligible Rage even at 1 Rage per 1% boss maxHP.

**Illustration of the unresolved scaling problem:** dealing 1,000 damage to a 1,000,000 HP boss removes 0.1% of its MaxHP: candidate 0.5/1% yields **0.05 Rage**, and the suggested 1/1% yields only **0.1 Rage**. At 30 Rage per Rage Breaker and 25 Rage per Coffee Shield, repeated ordinary attacks would starve the class in longer boss fights. This is a *design concern*, not a selected numeric fix.

**Possible solutions for next round, NOT user-approved:**
- A fixed **per-successful-basic-hit Rage floor** (possibly different on abilities), supplemented by normalized HP damage; cap per event and per ability use, clarify DoT/AoE/multi-hit cases.
- Normalize outgoing damage to **Berserker's OWN MaxHP** or another level/damage-scale reference instead of enemy MaxHP. Tune multipliers and per-hit caps, avoid uncontrolled farming from many weak targets.
- Award Rage based on **attack events or expected damage per level** so bosses and normal enemies produce similar combat resource cadence, not strictly each enemy's health bar percentage.
- Consider **incoming Rage from final HP damage** versus shield absorbed damage carefully; shield cannot accidentally generate rage both as taking and absorbing without a rule. Pick a boss-safe system, then run simulated solo and group combat cases before codifying values.

### Keep previously approved core kit unchanged

- **Wide Arc35**; **Spin Dash30**; **Rage Breaker30** (fixed30 Rage, 4s rank-scaled post-hit haste); **Anger Management25** (Rage gain/retention, two-handed axes/swords mastery, smooth missing-HP bonus reaching first-test +40% at 1% HP); **Coffee Shield30** (self only, linear5%→35% MaxHP).
- No Frenzy, separate Great Weapon Master, duplicate finisher or Battle Cry in Berserker.
- Round 9 should settle boss-safe Rage generation and incoming damage/shield interactions, staggered first-unlock levels, and Anger Management retention/rank gain numbers before full gate audit/advancement quest.

**STATUS:** Design docs updated and committed/pushed for this round; no gameplay files or save data touched.


## Round 9 — 2026-11-26 (USER-APPROVED attack-event Rage, Spin Dash gain, Wide Arc cost, shielded hits, low-decay endpoint, staggered unlock levels)

This is the LATEST Berserker resource/first-unlock design. It overrides the older suggestions in Round 8 wherever they differ. **Design documentation only; not implemented or verified in Unity.**

### Round 9 approved decisions

1. **BASIC ATTACK RAGE GENERATION:** Successful eligible normal/basic attacks award the fixed, rank-scaled amount **`1 + ceil(0.1 × AngerManagementRank)` Rage**. This is the user's exact expression, "attacks generate 1 + (anger management rank x 0.1 rounded up)"; the fixed amount does NOT divide outgoing damage by enemy maximum HP, addressing the boss-scaling starvation concern. At passive rank **0: 1 Rage**; ranks **1–10: 2 Rage**; ranks **11–20: 3 Rage**; ranks **21–25: 4 Rage** per successful basic attack. Make the rounding rule explicit; apply only to actual eligible **successful BASIC/AUTO ATTACK events**, not to every skill damage packet, damage-over-time tick, or every target of an AoE. If a basic is fully shield-absorbed, the general hit/absorb policy still needs explicit agreement before implementation; do not infer a free on-miss gain. **No additional outgoing percent-target-HP Rage bonus was approved**; any optional contribution remains an open *proposal*.
2. **SPIN DASH RAGE GENERATION:** **+2 Rage per enemy hit**, with an absolute **20 Rage maximum from ONE Spin Dash use**. Count enemies hit as unique eligible enemies per activation unless the user decides that repeat contacts should award additional Rage; proposed deduplication is a protective implementation detail, not a user-approved new mechanic. No Rage cost for Spin Dash was specified. Prevent the same enemy from farming infinite Rage from repeated collider events; no Rage on pure movement without hit.
3. **WIDE ARC COST:** **10 Rage to activate Wide Arc.** This overrides any implication from earlier Greatblade Momentum-based resource handling that Wide Arc is free or grants Rage. Wide Arc stays the sweeping melee ability. Whether rank changes cost or Wide Arc on-hit grants extra Rage is not approved; current fixed-cost reading is **10 Rage per accepted cast**.
4. **COFFEE SHIELD ABSORBED DAMAGE GENERATES RAGE AT THE SAME RATE AS HEALTH DAMAGE RECEIVED.** The user explicitly rejects reduced-rate gain. Use the existing Round8 **10 Rage per 10% of BERSERKER maximum HP received**, equivalent to **1 Rage per 1% of Berserker MaxHP**, for both **actual incoming HP damage AND damage absorbed by the user's Coffee Shield** (with no double-counting of the same damage packet). For a packet partially shielded and partially applied to HP, count the **sum of absorbed shield damage and applied HP damage ONCE** against Berserker MaxHP. Exact damage event eligibility, prevented vs shield absorbed, caps/rounding and whether other shields count are still open. This is an **incoming** resource rule and is independent of the new fixed basic-attack generation. Rage remains max100.
5. **OUT-OF-COMBAT RAGE DECAY:** Starts at **5 Rage per second at Anger Management rank0** and reaches **0.1 Rage/sec (1 Rage per 10 seconds) at Anger Management rank25**. The user's chosen endpoints supersede earlier proposals to end at 1 Rage/sec or zero decay. Exact intermediate step progression vs linear coefficient, accumulator precision and combat-state transitions remain open. Do not round 0.1/sec to 0 in integer-only resource ticks; preserve fractional accumulation or periodic drain.
6. **STAGGERED FIRST UNLOCK LEVELS APPROVED:** **Wide Arc at class level1, Anger Management at level1, Spin Dash at level3, Rage Breaker at level6, Coffee Shield at level8.** This is the set user accepted via "sure". Exact rank 2+ purchasing gates remain to be authored/audited; they must support the already approved **150 points total** and five caps **Wide Arc35 / Spin Dash30 / Rage Breaker30 / Anger Management25 / Coffee Shield30** with one skill point available per class level1..150. A level1 character needs at least one legal purchase among Wide Arc and Anger Management.

### Five-skill Rage economy summary (latest approved)

| Skill | First unlock class level | Cap | Resource action |
| --- | ---: | ---: | --- |
| **Wide Arc** | **1** | 35 | **Spends 10 Rage** on use |
| **Spin Dash** | **3** | 30 | **Generates 2 Rage per enemy hit, capped at 20 per activation** |
| **Rage Breaker** | **6** | 30 | **Spends fixed 30 Rage**; after successfully hitting an enemy, including a shield-absorbed hit, grants +2% attack speed per purchased rank for 4s, refresh/nonstack |
| **Anger Management** | **1** | 25 | Passive gives basic attacks **1+ceil(0.1r)** Rage; scales missing-HP damage and general 2H axe/sword damage; reduces OOC Rage decay from 5 to 0.1/sec by rank25 |
| **Coffee Shield** | **8** | 30 | **Spends 25 Rage**, self-only Coffee-element shield 5s, 15s cooldown, capacity linearly 5%→35% MaxHP across ranks1..30; shield-absorbed incoming damage builds Rage at SAME rate as HP damage |

**Global class traits unchanged:** max Rage100; Berserker favors 2H axes but can equip eligible 2H swords, no axe-exclusive mastery damage; Tier IV, not Tier III, may convert unused Coffee Shield capacity to healing on natural expiration. Missing-HP damage scales smoothly up to +40% at 1% HP at max Anger Management rank25. No Frenzy, separate Great Weapon Master, duplicate Rage finisher or Battle Cry.

### Open points for Round 10, NOT yet approved

- Is **every successful basic attack**, including a fully shield-absorbed basic hit, eligible for the 1+ceil(0.1r) Rage? Are environmental props/dummies/immortal summons valid combat targets?
- How does Rage gain from incoming damage apply to enemy shields other than Coffee Shield, if any; establish anti-exploit protection, fractional Rage behavior and per-hit/per-second caps.
- Exact Anger Management rank progression for out-of-combat **5→0.1 Rage/sec**; earlier design suggested selected-rank retention milestones while user's endpoint could use a smooth rank-based curve.
- Rank-scaled full damage and flat/STR coefficients for Wide Arc, Spin Dash and Rage Breaker; Rage Breaker success trigger and Coffee Shield cap rules already broadly defined, skill rank gates and starting attributes/advancement quest still open.
- Check that **level1 Wide Arc cost 10 Rage** and **Anger Management availability** provide reasonable initial Rage accumulation, and that 30-Rage Rage Breaker does not starve on bosses.

**Workflow:** User requires relevant design docs be committed and pushed after every answered question round. No Unity source/gameplay implementation is claimed by this update.


## Round 10 — 2026-11-26 (USER APPROVED: shielded basics, linear Rage retention, uncapped incoming hits, unique Spin Dash targets; stat design NEXT)

**LATEST USER-APPROVED Berserker decisions. Documentation only; no Unity changes or playtest claimed.**

1. **Successful BASIC ATTACKS generate the full previously approved Rage even if all damage is absorbed by the enemy's shield**: `1+ceil(0.1×AngerManagementRank)`, rank0=1, rank1–10=2, rank11–20=3, rank21–25=4 Rage. No Rage from a miss or invalid target; shield absorption counts as successful contact. This mirrors the previously confirmed Rage Breaker haste trigger on shielded successful hits. Do not inadvertently award additional Rage for the same hit from both damage and a separate shield mechanic.
2. **Anger Management's out-of-combat Rage decay progression is LINEAR by purchased rank** from **5 Rage/sec at rank0** to **0.1 Rage/sec at rank25**: `OutOfCombatRageDecayPerSecond(r)=5−(4.9/25)×clamp(r,0,25)`. For example r1=4.804, r10=3.04, r20=1.08, r25=0.1. Preserve fractional accumulation/time deltas rather than rounding 0.1 Rage/sec down to zero. This replaces the previous unresolved milestone-based alternative.
3. **NO additional per-incoming-hit Rage cap** beyond the **existing total 100 Rage resource ceiling**. Incoming HP damage and Coffee Shield absorption grant Rage at the SAME normal rate, **1 Rage per 1% of Berserker's own maximum HP worth of damage taken/absorbed**, with no double-counting a packet's shielded and HP portions. Thus a huge incoming hit can fill Rage to 100, rather than being artificially capped at 20 per hit. Combat event eligibility, overkill, and whether a lethal hit pays Rage before death still require implementation-specific decisions.
4. **Spin Dash generates 2 Rage per UNIQUE enemy hit ONCE PER activation**, to a maximum of **20 Rage per Spin Dash use**. Repeated contact with a previously counted enemy in the same dash does NOT grant more Rage. Use target identity deduplication on the same activation; don't grant extra Rage from repeated hitboxes/colliders.
5. **Next priority chosen by user: Berserker starting stats and per-level growth.** No attribute numbers were approved in the user's Round10 answers. Ground the new proposals in the actual Greatblade source baseline before asking approval.

### Existing Greatblade baseline verified from Unity repo

`Assets/_DiceFree/Settings/Skills/Greatblade/Greatblade class.asset` currently records starting **VIT14 / STR18 / AGI9 / INT5 / SPR5**, and per-level growth **VIT+2 / STR+3 / AGI+1 / INT+0.5 / SPR+0.5** (=7 total). Its current HP parameters are `baseHp=10`, `hpPerVitality=15`, `regenerationPerVitality=0.1` with no class-specific overrides. The future Berserker uses its OWN TierIII level1 stats and level growth (do not add accumulated parent Greatblade stats on top) under the existing class manifestation progression approach.

### Round11 designer-only STAT PROPOSAL, NOT APPROVED

| Attribute | Greatblade current Lv1 | Berserker PROPOSED Lv1 | Berserker PROPOSED gain/level |
|---|---:|---:|---:|
| Vitality | 14 | **22** | **+2.8** |
| Strength | 18 | **32** | **+6.0** |
| Agility | 9 | **13** | **+1.3** |
| Intelligence | 5 | **7** | **+0.7** |
| Spirit | 5 | **7** | **+0.7** |
| Growth total | **7.0** | | **11.5** |

At Berserker level150, without gear/passive stat bonuses, **STR=32+149×6=926** and **VIT=22+149×2.8=439.2** before gameplay rounding; if inheriting Greatblade's current HP equation `10+15×VIT`, proposed HP ~**6598** at 150. **This is merely an illustrative proposal and NOT an approval of HP coefficients, stat values, rounding or class resource specifics.** Greatblade's actual class growth is 7.0 and TierIII Marksman has independently approved stronger progression, so Berserker may scale more than parent while staying melee STR-focused. User should choose/refine starting stats and gains in Round11 before locking them.

### Still-open design after these choices

- Validate stat proposal, starting health/regen, crit/attack speed and attack coefficient link to STR; decide shield capacity snapshots and interplay with low-HP +40% damage passive.
- Offensive damage numeric coefficients scaling with purchased rank for Wide Arc, Spin Dash, Rage Breaker; Anger Management rank benefits to damage and Rage generation.
- Full 150-point rank eligibility gates preserving first unlocks WideArc1/AngerManagement1/SpinDash3/RageBreaker6/CoffeeShield8, ability to spend point at every class level.
- Advancement teacher/story quest and visual/asset requirements.
- Retain previously approved Wide Arc10 Rage, SpinDash+2 per unique enemy to20, RageBreaker30 Rage/+2% AS per rank for4s on successful hit, CoffeeShield25 Rage/5s/15s linear5%→35%MaxHP, and maximum Rage100.

**Workflow:** after every question round, commit and push relevant design docs; no Unity code altered during this Round10 docs update.


## Round 11 — 2026-11-26 (USER-APPROVED Berserker starting attributes, growth and GLOBAL 15 HP/VIT)

**This is the AUTHORITATIVE Round 11 correction. The Round 10 suggested VIT22/STR32/AGI13/INT7/SPR7 and proposed gains 2.8/6/1.3/0.7/0.7 are superseded.**

### USER-APPROVED Berserker Tier III base/growth (VIT, STR, AGI, INT, SPR)

User says: "**move 3 agi into vit, and move 2 points from int into str**" in starting stats, and "**move 0.2 int into vit and 0.1 agi into spi**" in per-level growth.

| Attribute | Greatblade Tier II source, Lv1 | Berserker Tier III **APPROVED Lv1** | Greatblade current per level | Berserker Tier III **APPROVED growth/level** |
| --- | ---: | ---: | ---: | ---: |
| VIT | 14 | **25** | +2.0 | **+3.0** |
| STR | 18 | **34** | +3.0 | **+6.0** |
| AGI | 9 | **10** | +1.0 | **+1.2** |
| INT | 5 | **5** | +0.5 | **+0.5** |
| SPR | 5 | **7** | +0.5 | **+0.8** |
| **Sum** | **51** | **81** | **+7.0** | **+11.5** |

Approved no-gear class-level 150 illustrative unrounded attributes: **VIT = 25 + 149×3 = 472**, **STR = 34 + 149×6 = 928**, **AGI = 10 +149×1.2 = 188.8**, **INT=5+149×0.5=79.5**, **SPR=7+149×0.8=126.2**. Determine engine rounding/display rules from shared implementation, do not invent one. Tier III Berserker is a separate class manifestation with its own class Lv1 stats and growth, NOT Greatblade Lv60's accumulated stats stacked atop these.

### Global player-class vitality policy: USER APPROVED

**1 VIT = +15 MAX HP globally for ALL player classes.** No unique Berserker HP-per-VIT multiplier, and no class-specific 20 HP/VIT or other coefficient for later classes. Base HP, items, temporary buffs and special cases (e.g. summoned clones with intentionally altered HP computations) should be evaluated separately, but must not be used as excuses to diverge player-class VIT scaling. Existing Unity `CombatTuning.hpPerVitality` defaults to **15**, and `ActorDefinition.hpPerVitality` also defaults to **15**, but code supports class overrides through `overrideVitalityHp` and additive modifiers. **Implementation follow-up:** audit all player class assets/active overrides and runtime modifiers for actual uniform VIT-to-HP contribution before claiming global runtime compliance. Approval is a *design rule*, not a confirmation that every runtime class currently follows it.

At Lv1 Berserker, attribute-contributed max HP = 25×15 = **375**, PLUS the shared/base HP component, equipment and buffs. At Lv150, VIT-contributed max HP = 472×15 = **7,080**, PLUS base HP/gear/buffs. Do not approve a Berserker-specific base HP or regen multiplier by implication.

### Existing Berserker identity preserved

The established 150 rank skill allocation **Wide Arc35 / Spin Dash30 / Rage Breaker30 / Anger Management25 / Coffee Shield30**, 100 Rage cap, fixed basic-hit Rage `1+ceil(0.1×AngerManagementRank)`, unique Spin Dash target Rage gain, Wide Arc10 Rage cost, Rage Breaker30 Rage cost and post-hit haste, Coffee Shield25 Rage/5s/15s and linear rank-based MaxHP absorption remain as documented in Rounds 1–10. Class stats do not implicitly change those coefficients or add axe-only damage bonuses.

### Next design work, NOT YET APPROVED

- Decide basic attack raw damage structure for Berserker (generally Physical, two-handed weapon eligibility, and class primary STR coefficient consistent with current global basic-attack standard), and rank-scaling flat+STR coefficients for Wide Arc, Spin Dash and Rage Breaker; confirm no unexpected double-application of Anger Management weapon bonus.
- Decide Anger Management exact per-rank STR/weapon or all-damage contribution, missing-HP +40% bonus distribution, and whether passive effects begin only when rank1 is purchased.
- Create/validate all **150 skill-rank level eligibility gates** while preserving approved first-unlocks Wide Arc1, Anger Management1, Spin Dash3, Rage Breaker6, Coffee Shield8, and spendability each class level 1..150.
- Design advancement quest/mentor and origin/transformation model; identify if HP and Rage regeneration need further class-specific decisions.
- **Validate global 15 HP per VIT** across playable class assets when Unity implementation is later authorized.

**No gameplay implementation or runtime testing is claimed by this design-only approval.**


## Round 12 — 2026-11-26 (USER-APPROVED damage rules; designer first-test coefficient proposal awaiting approval)

**LATEST USER ANSWERS approved these four mechanics:** (1) Berserker basic attacks use the shared **physical weapon/base damage + 1 × STR** rule, not a mixed AGI+STR class-specific coefficient; (2) **Rage Breaker is a guaranteed-hit skill**, subject to a valid target and normal range/line-of-sight/geometry, not an unconditional teleporting hit; (3) **EVERY purchased damaging skill rank increases BOTH its FLAT damage contribution and its STR coefficient**, never flat-only or stat-only; (4) **Anger Management's gradual missing-HP damage bonus applies to basic attacks AND every damaging Berserker skill**, not just basic or just skills. Keep previous smooth maximum +40% damage at 1% HP when Anger Management rank25 is reached. Avoid double application through the global damage pipeline. (5) The user selects **design exact offensive ability coefficients next**; the numerical values below are designer PROPOSALS and NOT yet user-approved.

### Verified current Greatblade source baseline

`Assets/_DiceFree/Scripts/Combat/GreatbladeCombatTuning.cs` currently uses per-rank STR coefficient formulas **Wide Arc `1.8+0.2×(r−1)`**, **Spin Dash `1.2+0.2×(r−1)`**, **Overhead Breaker `2.5+0.3×(r−1)`**; its three attack assets are physical, STR-weighted, with current `baseDamage=0`, and accuracy checks on. Greatblade source already adds separate skill-rank FLAT damage. That is the grounded parent architecture, not a rule requiring Tier III to inherit Tier II's extreme per-rank slopes unchanged. The general AttackDefinition pipeline has additive `baseDamage+coefficient×scaledAttribute`. Berserker should follow the same engine structure without adding weapon base twice.

### Designer first-playtest COEFFICIENT PROPOSALS ONLY (need user acceptance)

Let `r` be the rank of the individual skill, 1..approved cap, and let `S` be caster STR after applicable attribute bonuses. Suggested **raw Physical damage per eligible target**, before defense/resistance, rarity, buffs, low-HP bonus, and attack variance:

| Skill | Proposed FLAT(rank) | Proposed STR coefficient(rank) | Proposed raw damage at rank r | Maximum |
| --- | --- | --- | --- | ---: |
| Wide Arc | `12+4×(r−1)` | `1.8+0.10×(r−1)` | `12+4(r−1)+[1.8+0.10(r−1)]×S` | 35 |
| Spin Dash | `8+3×(r−1)` | `1.2+0.08×(r−1)` | `8+3(r−1)+[1.2+0.08(r−1)]×S` | 30 |
| Rage Breaker | `25+8×(r−1)` | `2.5+0.16×(r−1)` | `25+8(r−1)+[2.5+0.16(r−1)]×S` | 30 |

At **max rank**, before other effects: Wide Arc `148+5.2S`; Spin Dash `95+3.52S`; Rage Breaker `257+7.14S`. At **Berserker class level150 pre-gear STR928**, this predicts approximately **4973.6 Wide Arc / 3361.56 Spin Dash per unique target / 6882.92 Rage Breaker raw damage**, at each skill's cap and before buffs/mitigation; these are **illustrative designer values** and not tested game DPS. Do not interpret illustrative STR as existing game save stats or actual implemented class damage.

**Implementation guidance, still subject to damage-system audit:** Resolve Basic Auto damage using the game's GLOBAL selected-main-stat 1×STR policy; physical autos remain distinct from skill damage. Ensure all skill ranks update both the flat and stat-derived terms. Apply Anger Management's weapon effect and smooth missing-HP damage to basic/skill RAW damage precisely once, including all affected targets of Wide Arc/Spin Dash; do not accidentally apply a damage bonus again to a packet after mitigation. Rage Breaker **guaranteed-hit** bypasses accuracy RNG on valid in-range enemies; it **still** must resolve target validity, path/range constraints and hit/absorb. Rage Breaker's **4-second haste (+2% attack speed per Rage Breaker rank)** activates upon successful attack resolution, including an enemy shield-absorbed hit, which is compatible with guaranteed-hit unless the target vanishes/is invalid. Keep cost **30 Rage** and earlier approved behavior.

**Open questions for Round 13:** user approval/adjustment of these three FLAT and STR scaling proposals, attack/skill coefficient interaction with two-handed axe/sword equipped weapon base, accuracy/crit eligibility for Wide Arc & Spin Dash, and 150-point rank eligibility gates. No changes to five-skill roster, 100 Rage cap, or Coffee Shield 5%→35% rules. No Unity code/asset/test was modified by Round12 documentation.


## Round 13 — 2026-11-26 (USER APPROVED three offensive formulas, equipment interaction and crit eligibility)

**LATEST USER APPROVED, superseding the Round12 "designer proposals only" label for exactly these three formulas and rules.** Treat as locked **initial playtest targets**, not evidence that the Unity Berserker has been implemented or numerically playtested.

### Approved physical raw damage per hit before defense / buffs

Let `r` be the purchased rank of the INDIVIDUAL offensive skill, and `S` be the caster's resolved STR. For all skills, both FLAT damage and STR coefficient grow with rank.

| Berserker skill | Approved rank range | **APPROVED** raw Physical damage per valid enemy hit | At skill rank cap | Can crit? |
| --- | ---: | --- | --- | --- |
| **Wide Arc** | 1..35 | `12 + 4×(r−1) + [1.8 + 0.10×(r−1)]×S` | `148 + 5.2×S` | **NO, never crit** |
| **Spin Dash** | 1..30 | `8 + 3×(r−1) + [1.2 + 0.08×(r−1)]×S` | `95 + 3.52×S` | **NO, never crit** |
| **Rage Breaker** | 1..30 | `25 + 8×(r−1) + [2.5 + 0.16×(r−1)]×S` | `257 + 7.14×S` | **YES, normal global crit chance/multiplier** |

**Rage Breaker remains GUARANTEED HIT** against valid targets, but guaranteed accuracy is **not a guaranteed critical hit**. The crit roll on Rage Breaker is ordinary game-wide critical hit determination and can succeed or fail, independent of the guaranteed-hit property. Keep valid target, range, collision/geometry and shield absorption rules. Upon a successful hit, including an enemy shield-absorbed hit, grant the preapproved **+2% attack speed per purchased Rage Breaker rank for 4 seconds**; repeated buff applications refresh rather than stack. Cost remains 30 Rage.

**Wide Arc and Spin Dash cannot critically hit even if the attacker has significant critical strike chance from AGI, gear, items or auras.** This is a class-skill-specific prohibition, not a nerf to Berserker basic attacks or a global crit rule. Both still use their ordinary hit/accuracy and damage/defense interactions unless explicitly overridden later. Wide Arc costs 10 Rage, Spin Dash generates 2 Rage per unique enemy hit per activation up to20.

**Weapon interaction confirmed:** keep the standalone skill raw formulas above based on FLAT + rank-scaled STR, as the Greatblade parent skills do. Apply eligible two-handed axe/sword equipment/item bonuses through the EXISTING shared equipment/combat system, not by adding a second weapon base term inside each skill formula. The global BASIC attack still uses the ordinary physical **weapon/base damage + 1×STR** rule. Anger Management's general two-handed weapon damage passive and smooth missing-HP damage bonus (all basics and damaging skills, up to +40% at rank25 and 1% HP) must each be applied in the correct pipeline **once**, without double-dipping.

**Balance note:** first-test coefficients are approved, but damage/DPS and high-HP encounters still require runtime simulation and balance validation before final release.

### Next design: 150-rank eligibility audit, NOT YET USER APPROVED

Preserve exact approved skill caps **Wide Arc35, Spin Dash30, Rage Breaker30, Anger Management25, Coffee Shield30** = 150 ranks with one point per Berserker class level 1..150. Preserve FIRST-unlock levels **Wide Arc1 / Anger Management1 / Spin Dash3 / Rage Breaker6 / Coffee Shield8**. Choose rank-2+ level gates that collectively allow **at least L eligible purchases by every class level L from 1..150**, with all 150 ranks eligible by level150 and each skill cap reachable, while preserving the user's previous preference for a staggered unlock progression. Verify numerically with a small eligibility validator before requesting approval.

**Status:** Round13 user decisions documented; no Unity source/asset/build/test change in this design-doc commit.


## Round 14 — 2026-11-26: CRIT SOURCE CORRECTION + math-audited 150-rank gate proposal

**USER CLARIFIED (APPROVED): Berserker has NO built-in crit chance.** The class, its baseline and Anger Management must not grant intrinsic critical chance. Rage Breaker can crit using the shared critical-hit rules **only when crit chance is supplied from gear or another external source**; its guaranteed HIT remains separate from its possible critical-roll. Wide Arc and Spin Dash **never crit**, even with gear or external buffs; Berserker's basic attacks follow global basic-attack crit rules without a Berserker-specific chance bonus. Do not assume this statement disables normal externally sourced crit chance.

**Next task user requested:** proceed to design the 150-rank level gates. Designed a proposed skill-specific schedule, then **exhaustively audited all 150 levels** for spendability. **THIS GATE SCHEDULE IS A DESIGNER PROPOSAL, NOT YET USER-APPROVED.** The user's earlier approved FIRST unlocks and rank caps are preserved.

| Ability | Approved cap | Approved first unlock | Proposed rank-gate cadence | Proposed last rank gate |
| --- | ---: | ---: | --- | ---: |
| Wide Arc | 35 | 1 | First four ranks at class 1/2/3/4, then rank5 at8 and every4 levels | 128 |
| Spin Dash | 30 | 3 | Every5 class levels | 148 |
| Rage Breaker | 30 | 6 | Every4 class levels | 122 |
| Anger Management | 25 | 1 | Every6 class levels | 145 |
| Coffee Shield | 30 | 8 | Every4 class levels | 124 |

**Audit result:** 150/150 rank gates, **0 failing class levels** from 1..150, minimum `availableRanks(L)−L` is **0** at level150, maximum surplus16. All ranks have unlocked by class level148, so the level149–150 points remain spendable. One-point-per-level purchase stays possible, and no starter ranks are free.

**Complete exact gate lists, rank formulas and sampled audit counts:** [BERSERKER_TIER3_RANK_GATE_AUDIT.md](BERSERKER_TIER3_RANK_GATE_AUDIT.md). Ask user to approve/adjust before implementation. No Unity game files changed; documentation only.


## Round 14 follow-up (2026-11-26): FULL RANK-GATE SCHEDULE USER APPROVED

The user explicitly approved the previously proposed, exhaustively validated 150-rank Berserker eligibility schedule. **It is now the authorized initial-playtest design, not a pending proposal.** Wide Arc ranks 1-4 first become eligible at class levels 1/2/3/4 then every 4 levels through cap35 at level128. Spin Dash cap30 begins at level3, then every 5 levels to level148. Rage Breaker cap30 begins at level6, then every 4 levels to level122. Anger Management cap25 begins at level1, then every 6 levels to level145. Coffee Shield cap30 begins at level8, then every 4 levels to level124. Exactly 150 total purchasable ranks with one class point each level 1-150; all levels pass numerical eligibility audit and no free starting ranks are granted. Any older "awaiting user approval" references to this exact schedule are superseded. [Approved exact 150-rank gate audit](BERSERKER_TIER3_RANK_GATE_AUDIT.md). Next step: design Berserker's advancement quest and mentor, which are NOT yet approved. Design-doc approval only; no Unity code changed.


## Advancement quest design round — 2026-11-26: Old Berserker mentor + partial Crimson Crucible boss hunt

**Latest USER DIRECTION:** user prefers option (1) "The Old Berserker" mentor with some of option (2) boss-combat trial. They suggest using the existing NEW RED SLIME AREA DUNGEON as the quest activity, ideally **NOT requiring all six bosses**, e.g. "kill the bristleboar". **This is an approved design DIRECTION, not explicit final approval of precise quest steps, mentor location, reward handling or quest title.**

### Verified dungeon facts; use actual authored names

Exact source `docs/CRIMSON_CRUCIBLE_2026-11-25.md` documents the already-built **Crimson Crucible** in **Crimson Mire**, an optional/repeatable level-55 six-boss gauntlet entered through a **red-crystal arch at World1 X80/Z149**, east of the central trail. Ordered cells:
1. **Verdigris Globule** (slime)
2. **Moonfang Alpha** (wolf)
3. **Bristlebreaker** (boar-themed guardian, broad frontal slam; this is likely the user's "bristleboar" referent)
4. **Bloomheart Thornwarden** (plant)
5. **Stormscale Hierophant** (lizardman)
6. **Crimson Sovereign** (red slime)

**Critical world constraint:** Five sequential portcullises require each current boss to die before the next becomes reachable. The user CANNOT currently go directly to Bristlebreaker without first defeating Verdigris Globule and Moonfang Alpha. The dungeon's normal final treasury/reward happens ONLY after all six bosses. Existing abandonment/exit/wipe resets the run. Thus a Berserker quest rewarding a boss-3 defeat requires a **quest-specific persistent kill-event credit + safe interim exit/return-to-mentor UX**, NOT pretending the first two bosses can be skipped or that the normal final treasury is awarded early.

### Quest concept proposed for review, not finalized

- An **Old Berserker** (mentor, suggested aging orc warrior; location/name/look/unlock dialogue NOT YET DECIDED) teaches that Rage should be directed, not merely unleashed.
- A qualified Greatblade seeking TierIII Berserker accepts a **Bristlebreaker hunt** in existing Crimson Crucible, not a separate instanced arena. Suggested quest title **"Trial of Fury"** is provisional.
- Clear the required first two guardians and **defeat Bristlebreaker in Cell 3**. Track an explicit successful **guardian-kill event**, preferably for the quest owner's participation under existing party/kill-credit conventions, and persist quest progress immediately.
- **No need to defeat guardian cells 4, 5, or 6** for the class quest. Player may still choose to finish the full dungeon to earn its ordinary final treasury, but quest progression must not require that.
- Returning to mentor after qualifying kill unlocks a **new Lv1 Berserker TierIII manifestation**, preserving the Greatblade manifestation, its progression, equipment/inventory and other class state under existing advancement rules. Do not transform the current level60+ Greatblade in place or grant free rank1 to each skill.
- **OPEN technical issue for implementation:** Once Bristlebreaker dies, how to let the player leave **without** losing quest progress or claiming inappropriate full clear rewards. Reusing the existing dungeon Abandon/exit might be acceptable ONLY if kill-credit state persists outside the transient run; otherwise add a quest-specific safe exit. Do not alter existing normal dungeon six-boss reward conditions, gate sequence or final completion.
- All mentor specifics, narrative beats, encounter difficulty, class quest item reward, exact quest flags, whether group kills count for participants, and any requirement beyond eligible Greatblade level60 remain open for user approval.

**User decision to request next:** confirm Bristlebreaker specifically as objective, old orc mentor in/near Crimson Mire versus other world location, whether first two mandatory guardians are acceptable, and whether to offer a safe exit at Cell3 or use persistent objective completion plus normal abandon. **No Unity implementation authorized by this design-only discussion.**


## Advancement quest follow-up — 2026-11-26: USER APPROVED Bristlebreaker hunt, normal abandon persistence, mentor location delegated to Codex

These newest user decisions **supersede the prior open quest questions**; this is a design-document handoff, not a Unity implementation.

### Confirmed quest design

1. **Old Berserker mentor:** quest-giving retired Berserker, with a suggested old orc identity. The general setting is an outdoor camp near the Crimson Mire, BUT user explicitly says **"we leave that up to codex... maybe the bristleboar area north of triimery is a good area?"**. Therefore **Codex owns the exact NPC camp placement** within the existing world and its navigation/asset constraints; **Quillboar/bristleboar region north of Trimery is a favored CANDIDATE**, not a fixed coordinate, final approved location or required new area. In existing world docs the biome north of Trimery is formally called the **Quillboar region**, levels25–35. Keep the user's informal "bristleboar area" referent while using correct authored region name. No invented camp coordinates or NPC identity are approved.
2. **Quest objective:** An eligible Greatblade TierII manifestation (ordinary TierIII advancement at level60+, subject to global lineage/quest checks) accepts the old Berserker's challenge, enters the NORMAL **Crimson Crucible**, defeats **Verdigris Globule in cell1** and **Moonfang Alpha in cell2** as required by existing sequential gates, then kills **Bristlebreaker in cell3**. **Bristlebreaker is the objective**, NOT a separate one-boss encounter or renamed optional target. **Do not require the full six-boss clear**, cells4–6 are optional for this quest; do not alter ordinary dungeon encounters/reward requirements.
3. **Normal abandon/leave, with PERSISTENT QUEST CREDIT:** User specifically prefers **the EXISTING dungeon abandon/leave interaction** after Bristlebreaker, with the qualifying boss kill saved independently of ephemeral dungeon run state. No special exit portal/button/early treasury needed. Persist quest progress for the eligible quest owner before the dungeon resets on leave/abandon/wipe/menu exit. On re-entry the dungeon itself resets normally, while completed quest kill credit stays intact. After exiting, return to the Old Berserker to turn in the quest and unlock a **separate Lv1 Berserker TierIII class manifestation**, retaining Greatblade class save/progression and respecting zero free skill ranks and prior approved 150-rank gates.
4. **Quest attribution and coexistence:** Only an eligible accepted/active quest should be completed by a valid Bristlebreaker kill. Prefer existing party participation/kill-credit conventions; no obligation that the player lands the last hit, but party credit specifics still need Codex to adapt/validate existing engine contracts. Ordinary dungeon final treasure, XP/gold full-clear reward, gates and cells4–6 remain intact; do not award all-six treasury on cell3 kill.
5. **Suggested quest identity:** "Trial of Fury" remains a working title, not confirmed final NPC name/dialogue or cutscene. The story combines an experienced retired Berserker teaching controlled rage with an existing-world hunt rather than a bespoke arena. Mentor visuals/placement, exact dialogue, quest flag/save migration, marker guidance, and demonstration/balance testing are Codex implementation decisions as consistent with existing class systems.

### Existing content references

- `docs/CRIMSON_CRUCIBLE_2026-11-25.md`: World1 Crimson Mire arch (X80/Z149), level55 recommended, 6 bosses, cell3 **Bristlebreaker** with broad frontal slam, 5 portcullises, repeatable `StoryDungeonRun` and normal abandon reset.
- `docs/WORLD_1.md` / `docs/QUILLBOAR_REGION_DESIGN.md`: **Quillboar region north of Trimery**, levels25–35, contains hostile tribal camps and hills, potential mentor camp staging candidate. This is a location option, NOT a final fixed quest source or required dungeon relocation.

### Codex handoff acceptance criteria, not yet implemented

- Teacher NPC can offer the quest only to correctly eligible player Greatblade lineage; quest appears and persists using normal quest framework.
- Bristlebreaker kill in ordered **cell3** grants durable quest credit after cells1–2, independent of the transient instance; exiting via **normal Abandon/Leave** retains credit while resetting dungeon as usual.
- Report to mentor upgrades/unlocks Berserker TierIII Lv1 manifestation, does NOT destroy Greatblade, and does not add phantom starter ranks. Repeat interaction cannot award duplicate unlocks.
- Bristlebreaker quest completion does NOT prematurely award any six-boss final treasure or force cells4–6; ordinary six-cell full-clear unaffected.
- Multiplayer party participation, save/load, abandon and menu return should be covered by relevant end-to-end tests, including quest restoration after a fresh session.
- Actual teacher placement left to Codex; evaluate **north-of-Trimery Quillboar area** vs **Crimson Mire vicinity** in existing level layout, choose a grounded reachable camp rather than inventing premature coordinates; communicate placement in the implementation handover.

**Latest approved direction is complete enough for implementation planning; Unity source/assets were NOT edited in this documentation round.**


## Combat tuning round — 2026-11-26: Anger Management damage and all four active cooldowns, DESIGNER PROPOSALS

**USER REQUESTED:** explicitly design how Anger Management's damage bonuses work and provide cooldowns for Berserker skills. The **specific new damage percentages, interpolation/stacking approach, and proposed cooldowns below are NOT YET USER APPROVED**, unless already previously approved (Coffee Shield cooldown15s). No Unity implementation is authorized in this documentation-only design round.

### Prior approved anchors retained

- Anger Management is one passive with **25 ranks**, improving basic attack Rage `1+ceil(0.1×r)`, two-handed axe/sword weapon damage and missing-HP damage; smooth missing-HP bonus reaches **up to +40% at 1% HP when rank25** and applies to basic attacks and all damaging Berserker abilities. Berserker has no innate critical chance.
- Out-of-combat Rage decay `5−4.9×r/25` Rage/sec (approved), cap100 Rage. Rank0 Rage per successful basic hit1, rank1–10 2, rank11–20 3, rank21–25 4; fully shield-absorbed successful basics count.
- Skill costs: Wide Arc10 Rage, Spin Dash0 cost and +2 Rage per unique enemy hit max20 per cast, Rage Breaker30 Rage, Coffee Shield25 Rage. Rage Breaker +2% attack speed per rank for4s on successful hit; refresh/no stack. Coffee Shield5s duration, **15s cooldown user already approved**.
- Existing live Greatblade tuning for reference only: `arcCooldown=5`, `spinCooldown=10`, `breakerCooldown=7` seconds (verified `Assets/_DiceFree/Settings/Skills/Greatblade/Greatblade combat tuning.asset`), not evidence Berserker cooldowns have been implemented.

### NEW PROPOSAL A: Anger Management two damage channels (NOT APPROVED)

Let `r` be purchased Anger Management rank in 0..25 and `h=clamp(CurrentHP/MaxHP,0,1)`. If `r=0`, both of these passively granted damage bonuses are zero.

1. **Weapon mastery:** `WeaponDamageBonus = 0.005×r` (**+0.5% Physical damage per purchased rank, max +12.5% at r25**), conditional on wielding an eligible **two-handed AXE OR two-handed SWORD**, equally. Designer suggestion: apply to Berserker's physical basic and the three damaging offensive skills, not shields, heals, elemental shield absorb or other classes. This is a general 2H physical damage modifier, **not extra weapon-base damage** and not an axe-exclusive perk. Exact 0.5% coefficient and affected skills await user approval.
2. **Bloodied Fury** (working label for the missing-HP effect, not a separate skill): `MissingHpBonus = 0.40×(r/25)×clamp((1−h)/0.99,0,1)`. At full HP bonus0, at **1% HP or below** reaches `40%×r/25`, so max rank r25 gives the already approved 40% ceiling. No sudden low-HP breakpoint. At passive r25 / HP50% the new formula yields `0.40×(0.5/0.99)=20.202...%`; at HP25% gives30.303...%; r1 max1.6%, r10 max16%, r25 max40%. The **40% endpoint is APPROVED**, the exact rank/hp interpolation is a designer candidate.
3. **Stacking proposal:** compute approved raw Physical skill/basic damage with current rank coefficient and STR, then multiply exactly once by `(1+WeaponDamageBonus)×(1+MissingHpBonus)` if both conditions apply, then use shared enemy defense/mitigation. At r25, HP1%, matching eligible 2H weapon, total combined modifier `1.125×1.40=1.575`, i.e. **+57.5% raw Physical damage combined** (not +40% total). Weapon condition does not change the already-approved missing-HP bonus scope: missing-HP scaling still applies to normal basics and ALL damaging Berserker skills. Alternative ADDITIVE stacking `1+WeaponDamageBonus+MissingHpBonus` would cap at +52.5% combined; this distinction needs user approval. Avoid applying existing equipment weapon damage twice, and don't accidentally multiply or stack the rank25 effect on shield/DoT packets.
4. **Potential snapshot behavior:** current HP at the moment the damage packet is resolved, not at skill cast initiation; no retroactive changes to already resolved attacks. Exact implementation policy requires user agreement/engine compatibility.

### NEW PROPOSAL B: All active cooldowns (NOT APPROVED except Coffee Shield15s)

| Active skill | Proposed cooldown | Previous Greatblade reference | Approved resource action | Other notes |
| --- | ---: | ---: | --- | --- |
| Wide Arc | **5 seconds** | 5s | Spend 10 Rage | Keep as repeatable cleave; no crit |
| Spin Dash | **10 seconds** | 10s | Gain 2 Rage per unique enemy, cap20 | Movement and AoE; no crit |
| Rage Breaker | **8 seconds** | Greatblade Overhead Breaker7s | Spend fixed30 Rage | Guaranteed hit on valid target, can crit only via external crit chance, grants 4s +2% attack speed/rank on successful hit |
| Coffee Shield | **15 seconds APPROVED EARLIER** | Not present | Spend25 Rage | Self-only Coffee element, lasts5s, rank1 5%→rank30 35% maxHP absorb |

- **Anger Management is PASSIVE**, has no activation cooldown.
- Designer suggestion: **start cooldown on accepted cast**, subject to ability target/geometry validation and skill cast rejection rules; not after damage lands or after the 5-second Coffee Shield expires. Cancel/interrupt/Rage refund semantics should follow global cast rules; do not silently introduce a free resource refund.
- A 4s Rage Breaker attack-speed buff on an 8s proposed cooldown has at most 50% uptime from isolated uses; actual Rage economy can lower uptime.
- **User decisions needed:** confirm damage rank curve and +0.5% 2H bonus/rank, additive vs multiplicative stacking, proposed cooldown values and cooldown start/invalid/interrupt behavior, and whether to treat working "Bloodied Fury" solely as a descriptive passive sub-effect or rename it.

**No Unity code modified, built, or tested in this design discussion.**


## Combat tuning approval follow-up — 2026-11-26: Anger Management damage APPROVED, longer cooldowns pending

**LATEST USER APPROVAL:**
1. **Anger Management weapon bonus** at rank `r` in 0..25: `W = 0.005*r` = **+0.5% eligible two-handed axe/sword Physical damage per rank**, max +12.5% at rank25. Applies to ordinary Berserker basic physical attacks and its three damaging physical active abilities while wielding a valid two-handed axe OR sword, equally. No innate crit chance, no axe-only advantage.
2. **Anger Management missing-HP damage bonus:** `B = 0.40*(r/25)*clamp((1-h)/0.99,0,1)` where `h = clamp(CurrentHP/MaxHP,0,1)`. Smooth linear rank scaling and missing-HP scaling; max **+40% at 1% HP and rank25**; zero bonus at full health or zero passive rank. Applies to basics and all damaging Berserker skills. "Bloodied Fury" is a WORKING effect description within Anger Management, NOT a separate sixth skill or independently rankable ability.
3. **MULTIPLICATIVE stacking approved:** `FinalAngerManagementMultiplier = (1+W)*(1+B)` when both applicable. With rank25, HP1%, eligible 2H weapon this yields `1.125*1.40 = 1.575` **(+57.5% combined damage)**, not additive +52.5%. Apply each component only once in ordinary outgoing raw-damage pipeline without re-adding weapon base, repeated hitboxes, or post-mitigation double-dipping. The exact damage-resolution HP sampling point remains an implementation detail to confirm with existing combat behavior.

**USER REJECTED THE SHORTER NEW COOLDOWN SUGGESTIONS AS FINAL:** response was "Prefer longer cooldowns, discuss specifics". No new cooldown numbers were approved by that reply; do not mark new candidate values as accepted. The previously approved Coffee Shield 15-second cooldown remains the existing baseline **until explicitly changed**.

**APPROVED COOLDOWN START TIMING:** cooldown starts **AFTER the relevant skill cast/attack finishes** rather than upon cast acceptance or initiation. For instantaneous/quick activation, trigger after actual activation completion; for attacks, after the ability finishes resolving, not merely on click. For Coffee Shield this means after *activation animation/cast finishes*, **NOT after its full 5-second shield duration expires**. A rejected cast should not start cooldown; interruption/resource-refund policy should follow shared game rules and be examined separately.

### NEXT ROUND DESIGNER-ONLY LONGER COOLDOWN PROPOSAL (NOT USER APPROVED)

| Active skill | Previous Round proposal | New suggested longer cooldown | Rage |
| --- | ---: | ---: | --- |
| Wide Arc | 5 seconds | **7 seconds** | Cost10 |
| Spin Dash | 10 seconds | **14 seconds** | Gain2/unique enemy hit, max20/cast |
| Rage Breaker | 8 seconds | **12 seconds** | Cost30; on successful hit grants rank-scaled 4s attack-speed buff |
| Coffee Shield | **15 seconds (ALREADY APPROVED baseline)** | **20 seconds (PROPOSAL to replace old baseline)** | Cost25; 5s shield duration |

Anger Management passive has no activation cooldown. Suggested 7/14/12/20 are deliberately longer than 5/10/8/15; **none of these new values should be implemented without user signoff**. Overall pacing and actual Rage economy / cooldown utilization still require playtesting.

**NEXT QUESTION TO USER:** approve/adjust each of WideArc7, SpinDash14, RageBreaker12, CoffeeShield20. The established 15s Coffee Shield stays authoritative until explicit replacement. Documentation only; no Unity gameplay changes.


## Final Berserker combat-cooldown approval — 2026-11-26 (LATEST USER DECISION)

**USER APPROVED the longer cooldown proposal with TWO exact edits:** "coffee shield 25 sec and spin dash 15 sec. otherwise it looks good." Therefore lock the following as **AUTHORITATIVE final first-playtest cooldown values**, superseding all previous suggested durations and the earlier Coffee Shield 15-second baseline:

| Berserker active skill | USER-APPROVED cooldown | Previously approved Rage action | Other notes |
| --- | ---: | --- | --- |
| **Wide Arc** | **7 seconds** | Spend 10 Rage | No crit, physical cleave |
| **Spin Dash** | **15 seconds** | +2 Rage per unique enemy hit, capped at 20 per cast | No crit; outgoing movement/AoE |
| **Rage Breaker** | **12 seconds** | Spend fixed 30 Rage | Guaranteed hit on valid target; can crit ONLY with external crit chance; on successful hit +2% attack speed/rank for 4s, refresh not stack |
| **Coffee Shield** | **25 seconds** | Spend 25 Rage | Self-only Coffee-element shield lasting 5 seconds; rank1=5% maxHP to rank30=35% maxHP |

**Cooldown timing USER APPROVED in preceding round:** start the cooldown only **after the relevant cast or attack FINISHES/resolves**, never when the button is first pressed. Coffee Shield's cooldown begins after its actual activation/cast finishes, **not after its five-second shield lifespan expires**. Anger Management is a permanent 25-rank passive with **no activation cooldown**.

**Anger Management numeric damage is ALREADY APPROVED**: +0.5% eligible 2H axe/sword Physical damage per purchased rank; low HP damage `0.40*(rank/25)*clamp((1-CurrentHP/MaxHP)/0.99,0,1)`; multiply damage effects `(1+0.005rank)*(1+lowHpBonus)`, up to **+57.5% combined at rank25 and 1% HP** for valid 2H weapons. No built-in crit chance. No need to reopen this topic merely because prior sections contain old proposals.

**USER SAYS CODEX IS HAPPY WITH EVERYTHING, AND REQUESTS MOVING TO NEXT DESIGN TOPIC.** Record this as the user's report, not verified implementation, deployment, tests, or external Codex signoff. The class's foundational design and advancement quest can now be considered approved for the initial implementation handoff; no Unity source or playtest changes were made here. Implementation details such as interruption semantics, damage snapshot timing and visuals may be resolved through existing shared systems or brought back as focused questions only if necessary.

## Character asset selection — 2026-11-26 (USER APPROVED)

The user selected recommendation **2, Orc Warrior**, for **BOTH the playable Berserker class and its retired Berserker mentor**. This supersedes the earlier recommended Viking Shieldmaiden player model and removes any need to select different base characters for these roles.

- **Exact library source:** `T:/TEMP/ART_ASSETS/orc-warrior (2).zip`, containing `source/Orc Character.zip` and its `Orc Character/Model/Character.fbx`. Do not substitute the separately named `orc-warrior.zip` or `orc-warrior (1).zip`; those were inspected and are unrigged alternatives.
- **Publisher/model:** [Orc Warrior by Callan McEntee](https://sketchfab.com/3d-models/orc-warrior-21eaa407bf164cdea21135a1d1b2b8f3).
- **Verified source contents:** archive CRC passed; Blender inspection found 9,310 triangles, five mesh objects, four meshes with vertex groups, a 71-bone character armature and a one-bone auxiliary armature. The publisher describes an idle animation. Multiple imported action datablocks belong to the same `Take 001`; they are not evidence of a complete combat animation library.
- **Presentation direction:** retain the hand-painted orc identity for both roles. Distinguish the mentor with an aged, weathered appearance and the player with a readable combat appearance. Exact ageing, palette, equipment and animation adaptations are implementation/art choices; no exact new colours or armour designs were approved by this selection.
- **Weapon and animation work:** preserve the approved preference for two-handed axes and compatibility with eligible two-handed swords. Replace/adapt the source spear presentation as needed; verify two-handed grip, swing/spin animation retargeting and gameplay-camera readability during implementation.

This records the selected source and roles. It does not claim that the asset has been imported, combat-retargeted, visually approved in DiceFree or wired to an implemented Berserker class.
