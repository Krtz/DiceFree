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
