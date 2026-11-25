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
