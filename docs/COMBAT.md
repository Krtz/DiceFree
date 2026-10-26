# Combat

## Direction

DiceFree is a real-time 3D isometric action RPG.

Primary design assumptions:
- mouse-driven click-to-move is the default/reference control style and primary balance target;
- WASD/direct movement is a fully supported alternative;
- compact, class-dependent kits;
- readable mechanics;
- class-specific movement and resources;
- strong class identity over universal action-game mechanics.

## Control schemes

Detailed input/selection/camera rules are documented in `docs/INPUT_CONTROLS.md`.

### Classic / mouse movement
Warcraft III / League-style click-to-move is the primary balance baseline.

Contextual right-click handles movement/attack/interact.

Attack-Move, Stop and Hold Position are dedicated commands.

### Direct / WASD movement
WASD feeds the same movement/combat systems and must not become the hidden requirement for encounter execution.

Classic Mouse and Direct/WASD use **different default keybind profiles**, because Direct mode reserves WASD for movement. Everything remains fully rebindable.

### Selection safety

Unit-selection behavior must be configurable enough to sit between ARPG and Warcraft III RTS conventions.

Accidental selection of allies/summons/other units must never make emergency movement unresponsive during combat.

A simultaneous Hybrid mode remains possible but not committed.

### Unit collision and ghosting

Heroes, enemies and normal controllable units occupy physical space and participate in pathing/collision.

They are not default ghost units that freely pass through each other.

Ghosting/phasing is a rare explicit mechanic for specific classes, passives, enemies or effects.

## Combat actions and ability composition

Basic attacks and active abilities should resolve through the same core combat-action/effect pipeline where practical.

A combat action can compose reusable pieces such as:
- targeting rule/shape;
- range;
- cast/wind-up time;
- resource cost;
- cooldown/cooldown groups;
- one or more damage/healing packets;
- Physical/Magical channel;
- element;
- applied status/effects;
- movement/knockback;
- summon/spawn behavior;
- threat;
- resource generation/consumption;
- other hooks.

Content should be data/composition-first, with a clean custom-code escape hatch for genuinely unusual class mechanics.

Do not require a bespoke combat engine for each class.

## Basic attacks

Every normal class has a Warcraft III / League-style repeating basic attack:
- command an attack on a target;
- move into attack range if necessary;
- repeatedly attack while the target remains valid.

Basic attacks may be melee or ranged depending on class.

Because basic attacks use the common action/effect pipeline, classes and gear may hook them to:
- generate Mana/Rage/Energy/Combo Points or other resources;
- apply on-hit effects;
- consume charges/stacks;
- trigger class mechanics;
- interact with lifesteal, elemental imbuements, accuracy/blind, threat and proc systems.

A later class may, for example, build Combo Points from ordinary auto-attacks and spend them on active skills.

Most caster classes will be ranged, but magical melee classes are fully valid.

A future class with no conventional basic attack is possible only as an intentional special-case design.

## Primary attributes

The detailed stat model is documented in `docs/STATS_AND_DAMAGE.md`.

Core attributes:
- Vitality
- Strength
- Agility
- Intelligence
- Spirit

Current secondary direction:
- VIT -> HP and HP regeneration;
- STR -> modest Physical Defense;
- AGI -> very small Attack Speed and Movement Speed gains;
- INT -> modest Magical Defense plus class-specific resource interactions where appropriate;
- SPI -> healing done and healing received, with separate scaling.

Class base stats, gear, abilities and passives remain more important than these secondary attribute bonuses.

Classes can use one or more primary attributes, and individual skills can use bespoke/adaptive scaling.

## Health

Vitality contributes to maximum HP.

Default baseline:

```
Max HP = class base HP + (Vitality × 15 HP) + other modifiers
HP regeneration = Vitality × 0.1 HP/sec + other modifiers
```

Classes can explicitly override these default Vitality coefficients. Items/effects can modify them further.

## Resources

Different classes can use different resources.

A class may have **multiple simultaneous resources**.

Examples:
- Mana only;
- Energy + Combo Points;
- Mana + Souls;
- Rage + Charges;
- several bespoke resources for exceptional classes.

Resources are independently defined components/data rather than one universal resource enum or mandatory Mana field.

### Load/reset defaults

Normal save/load is not a combat-state resume:
- manifestation loads at full HP;
- ordinary combat/consumable cooldowns reset;
- temporary buffs/debuffs reset;
- threat/aggro/targets/casts/projectiles reset;
- resources reset by default unless that resource explicitly opts into load persistence.

The resource/effect frameworks must still allow explicit persistent exceptions for rare authored mechanics.

### Resource lifecycle

Each resource defines its own lifecycle rules, including combinations of:
- persist normally;
- reset on combat end;
- reset on death;
- persist through death;
- decay over time;
- decay only out of combat;
- regenerate over time;
- generate only through actions;
- cap/floor behavior.

There is no universal "resources reset after combat" rule.

### Target-bound vs owner-bound resources

The framework supports both:
- **owner-bound** resources, such as generic Combo Points stored on the player;
- **target-bound** resources, such as Combo Points/stacks attached to a specific enemy relationship.

A class can choose either model.

### Multi-resource costs

One ability may require/consume several costs simultaneously.

Examples:
- 30 Mana + 3 Combo Points;
- 20% current HP + 50 Mana;
- one item/charge + Mana;
- all available stacks plus a flat resource cost.

Cost validation/payment should be composable rather than bespoke per class.

Intelligence should never be completely useless to a class just because that class does not use Mana, but Intelligence does **not** universally mean resource regeneration.

Class/resource interactions with Intelligence are defined by class/system design.

An advancement may keep, modify, add or replace resources entirely.

## Damage, defense, resistances and crit

Detailed damage-instance, Physical/Magical Defense, elemental resistance, elemental healing, penetration and crit rules are maintained in `docs/STATS_AND_DAMAGE.md`.

Important combat rules:
- mixed damage uses separate damage instances;
- every instance has one Physical/Magical channel and one element/no element;
- Physical/Magical Defense and elemental resistance are separate mitigation layers;
- elemental resistance may be negative;
- elemental healing can use matching resistance as a positive healing modifier;
- crit is opt-in rather than universally assumed.

## Movement and mobility

There is no universal dodge/roll.

Mobility is class identity:
- some classes get dashes/teleports/rolls;
- others get none;
- base move speed can differ by class;
- strong mobility should have tradeoffs.

**Thief Tier II class-specific design decision (THIEF-016–020; 2026-10-21; NOT IMPLEMENTED):** In-combat Stealth prevents enemies from perceiving/targeting the hidden Thief even when hit by ordinary damage; **damage does NOT cancel Stealth**. On entry, enemies must DROP their stored threat against the Thief either entirely or drastically (exact 100% reset versus partial percent user choice pending), instead of only suspending target selection. This is NOT a global rule for all invisibility and NOT permission to reset boss encounters, other party members' threat or boss scripted mechanics. Thief's 4→8s stealth duration and ~1.1×→2× first-normal-basic opener increase gradually with rank. **Poison-tag Envenom damage-over-time is PHYSICAL NATURE**, while ground Smoke Bomb DoT is **PHYSICAL TECH**; normal Physical Defense and respective elemental resistances apply. Smoke Bomb 4m radius/5s and grants enemies within it **20% MISS CHANCE FOR ENEMY BASIC ATTACKS ONLY**, immediately removed on leaving, NO automatic ability/spell misses. The temporary crit buff grants guaranteed critical hits **on Thief BASIC ATTACKS ONLY**, lasting 1s first rank→4s max; this is skill-specific, not global crit eligibility. See [Thief Tier II design](classes/THIEF_TIER2_DESIGN.md).


**THIEF-021–025 latest CLASS-SPECIFIC UPDATE (2026-10-21; DESIGN ONLY):** Stealth ON CAST reduces the Thief's existing threat on each enemy's threat table by **95% (retains5%)**; no boss/party-threat reset, damage taken does NOT break invisibility. Envenom INSTANT SKILL stab RAW **2× normal autoattack RAW damage**, direct non-elemental Physical channel, applies **one nonstacking 3s Poison-status DoT dealing Physical Nature damage once per second**. The stab is not a normal/basic autoattack event, so no extra basic-only crit permission or Stealth-opening bonus by default. Smoke Bomb's Physical Tech DoT has **4 damage ticks per second (one every0.25s, 20 ticks/5s)**; each raw tick is a quarter of an equivalent once-per-second RAW value, actual per-second coefficient TBD. **Overlapping Smoke Bomb clouds are nonstacking** for damage AND 20% enemy basic miss penalty. Thief crit passive explicitly opts into **+1 percentage point basic attack crit chance per rank and 1.5× raw crit damage**, plus +1 AGI/rank; active buff temporarily grants 100% critical hits on basic attacks only for its 1–4s rank-scaled window. This is not global crit chance or physical spell crit eligibility. Exact 80-rank caps/gates are designer first-playtest proposals under user delegation, not implemented. See [Thief](classes/THIEF_TIER2_DESIGN.md).

**THIEF-026–030 LATEST CLASS-SPECIFIC DESIGN (2026-10-22) — overrides stale historical Thief rank/Stealth reveal entries below:** THIEF rank plan **designer-proposed** 16 Stealth/18 Envenom/10 Crit buff/18 Fan/10 Smoke/8 passive =80 with every class level1–80 legal. Envenom skill STAB direct RAW2× normal basic Physical, then **Physical Nature** Poison-tag nonstacking DoT, 3s 1 tick/s, RAW/tick `(2+0.10×AGI)×(1+0.05×(r−1))`. On reapplication RESET duration to3s **but KEEP tick clock**. Smoke Physical Tech 4 ticks/sec (0.25s each); approved total RAW/sec `(2+0.08×AGI)×(1+0.05×(r−1))`, RAW every tick one-quarter, overlapping clouds do not stack. Fan is one non-elemental Physical action HIT PER ENEMY within4m Thief-centered 360°, **usable during movement**, no basic crit permission. **Reveals:** Envenom, Fan, Smoke AND crit buff activation BREAK STEALTH and DO NOT grant basic opener bonus. NORMAL BASIC that reveals gets its stealth multiplier (rank scaling designer fit to 16), incoming hits do not reveal. Global action rules should not promote class skill hits into basic attacks or duplicate independent offhand events. Stealth still drops exactly95% enemy threat ON CAST. Initial cooldown/Mana values designer proposals only. No Unity changes. See [Thief design](classes/THIEF_TIER2_DESIGN.md).

**THIEF ROUND7 NEWEST USER RULES — 2026-10-22 (DESIGN ONLY; supersedes earlier 'crit buff breaks Stealth' and 'Envenom never receives opener' text below):** **Activating the Thief's timed guaranteed BASIC critical buff DOES NOT BREAK/REVEAL STEALTH**. It can be cast while hidden and only NORMAL BASIC attacks are guaranteed to crit for its1–4s window. **ENVENOM SKILL STAB THAT BREAKS STEALTH DOES GET THE SKILL-RANK-BASED STEALTH OPENING DAMAGE MULTIPLIER (1.1×→2×)** just like a breaking normal BASIC hit; do **NOT** mistake Envenom for a normal BASIC attack or grant it buff/passive basic-only crit rolls as a side effect. The exact bonus scope for subsequent Poison DoT ticks remains pending user answer; safest designer recommendation: direct Envenom stab packet only. Fan of Knives and Smoke Bomb continue to BREAK STEALTH without the opener multiplier; incoming damage does NOT reveal. On breaking/expiring Stealth, enemies reacquire normally from **the5% stored threat remaining after activation's95% threat reduction PLUS NEW threat**, and NEVER restore deleted95%; boss encounter never resets. **USER APPROVED first-playtest active Mana/cooldowns** Stealth24s/20, Envenom8s/12, crit buff18s/16, Fan9s/18, Smoke18s/20; six skill caps16/18/10/18/10/8 and exact rank gates validated level1–80. **Fan** raw non-elemental Physical per enemy `(8+0.45×AGI)×(1+0.06×(FanRank−1))`, one packet per4m-target while moving. **Smoke Bomb** PHYSICAL TECH DoT and basic miss ONLY WHILE ENEMY PHYSICALLY INSIDE cloud, STOP IMMEDIATELY on leaving, while **Envenom's attached PHYSICAL NATURE Poison DoT PERSISTS** when victim moves away (until regular expiry/dispels). Current authoritative [Thief spec](classes/THIEF_TIER2_DESIGN.md). These exceptions do not alter general crit, stealth, skill attack or damage rules for other classes.

**THIEF FINAL ROUND 8 SUPERSEDING RULE (2026-10-22; DESIGN COMPLETE, NOT IMPLEMENTED):** Envenom revealing from Stealth gains the opener multiplier on **DIRECT non-elemental Physical SKILL STAB ONLY**, NOT its attached PHYSICAL NATURE Poison DoT tick packets: `directRaw = 2×currentNormalBasicRaw×(1.1+0.9×(StealthRank−1)/15)` for purchased Stealth rank1–16; at rank16, direct RAW4×currentBasicRaw. Envenom skill is still not a BASIC attack, so doesn't get basic-only guaranteed crit. **Activating crit self-buff PRESERVES Stealth** and its 1–4s timer continues counting down in real time while hidden. **First revealing NORMAL BASIC or Envenom attempted attack consumes the Stealth opening bonus even on MISS or COMPLETE SHIELD ABSORPTION**, without waiting for HP damage. Normal movement, taking incoming damage, potions and inventory/UI navigation DO NOT reveal; actual looting, NPC speech or world-object interaction BREAK Stealth and do not grant opener. Fan and Smoke reveal without opener. Threat remains95% DROPPED on Stealth activation, with enemy reacquisition using5% retained plus subsequently generated threat on reveal. The full first-playtest Thief gameplay design is now COMPLETE, not coded or tested; newer rules override earlier contradictory Thief entries below. [Completed Thief design](classes/THIEF_TIER2_DESIGN.md). PSION now active, no Psion combat mechanics designed yet.

## Threat

DiceFree has a proper threat system.

Every relevant combat action can define its own threat value/coefficient, including:
- basic attacks;
- damaging skills;
- healing;
- buffs/support;
- special tank abilities;
- crowd control where appropriate.

Healing generates threat.

Bosses and special enemies may deliberately override ordinary threat.

### Taunts and threat builders

Tank kits can include:
- temporary forced-target taunts;
- very high threat-generating abilities;
- abilities that both force target and establish/catch up threat.

Not every tank tool needs to work the same way.

### Threat UI

Default threat presentation should be simple and readable, using color/state indicators rather than a permanent wall of numbers.

Players may enable more detailed threat information as an option.

The underlying system should always track exact values for debugging and encounter logic.

## Healers

Healers are full combat classes, not passive health-bar babysitters.

Healers should:
- have real damage rotations;
- contribute meaningful DPS while keeping the party alive;
- be able to play solo;
- farm and grind effectively;
- use healing as part of their class loop rather than their only activity.

Different healer classes may connect damage and healing in very different ways.

## Healing/support targeting

Support abilities can target:
- characters in the 3D world;
- eligible allies through party frames/UI.

## Ability charges

Abilities may have multiple charges.

The framework supports different recharge policies, including:
- **independent recharge** — each spent charge tracks/recharges independently;
- **sequential recharge** — one charge recharges, then the next begins;
- **all-at-once recharge** — one cooldown restores all charges together.

Charges may interact with normal/shared cooldown groups according to the ability definition.

## Casting, channeling and interruption

Every ability defines its own cast/movement/interrupt behavior.

Supported dimensions include:
- instant vs cast-time;
- normal cast vs channel;
- can/cannot move while casting;
- movement interrupts or does not interrupt;
- taking damage interrupts or does not interrupt;
- explicit interrupt/silence behavior;
- channel tick cadence;
- cast/channel completion behavior;
- optional spell pushback/delay.

**Default direction:** no universal spell pushback. Damage does not automatically delay every cast.

Spell pushback is supported for abilities/classes that explicitly use it.

**Ranger-specific design-only channel example (2026-10-19, NOT IMPLEMENTED):** Ranger Tier II **Arrow Rain** first performs a **1.5-second channel**, animating the Ranger firing a **large arrow then a smaller arrow into the sky**. Only after that channel completes does an independent **four-wave, two-second ground rain** begin; enemies entering later can be struck at subsequent waves. This is **not** a 3.5s player channel: the player channels for 1.5s, while rain resolves for 2s. **RANGER-026 CONFIRMED:** MOVEMENT, STUN and explicit INTERRUPT cancel the initial 1.5s channel before its completion, preventing the rain entirely. Ordinary incoming damage does NOT cancel on its own. **After channel completion, all four subsequent waves continue through the full 2s even if Ranger moves or is stunned/interrupted afterward.** The per-ability casting framework above supports this independent post-channel damage phase. Ranger's 3m radius and rank1 0.4× basic arrow raw damage per wave are confirmed; rank growth is a designer proposal. See [Ranger Tier II design](classes/RANGER_TIER2_DESIGN.md).

## Cooldowns and shared cooldown groups

DiceFree has **no universal global cooldown by default**.

Abilities normally use their own:
- cooldown;
- resource cost;
- cast/wind-up/recovery rules.

The framework must still support:
- per-class or per-ability shared cooldown groups;
- explicit GCD-like groups for classes that need them;
- item/consumable cooldown groups;
- cooldown modifiers;
- **shared cooldown state across different players** when an encounter/class mechanic explicitly requires it.

Cross-player shared cooldowns are exceptional authored mechanics, not the default for ordinary abilities.

## Fixed kits

Classes use fixed ability kits rather than freely swapping from a large skill library.

Advancement may add, evolve or replace parts of that fixed kit.

Gear, Intrinsics and special effects can modify how the fixed kit behaves.

Normal tooltips should show readable final values. Holding a modifier key such as Shift should expose detailed formulas/scaling where useful, including the currently-selected attribute for adaptive-scaling abilities.

## Trigger / proc framework

Classes, gear, effects, encounters and world systems may react to semantic combat/gameplay triggers.

The framework should support a broad trigger vocabulary, for example:
- OnBasicAttack;
- OnAttackStarted;
- OnHit;
- OnCrit;
- OnDamageDealt;
- OnDamageTaken;
- OnHealGiven;
- OnHealReceived;
- OnKill;
- OnDeath;
- OnResourceGenerated;
- OnResourceSpent;
- OnEffectApplied;
- OnEffectRemoved;
- OnCastStarted;
- OnCastCompleted;
- OnInterrupt;
- OnBlock/Absorb;
- OnThornsDamageGiven;
- OnThornsDamageReceived;
- authored/custom trigger conditions.

The trigger system should be extensible enough for intentionally strange future mechanics rather than limited to a frozen small enum.

### Proc origin and recursion safety

Generated actions/events carry semantic origin/context such as:
- basic attack;
- active ability;
- DoT/HoT tick;
- proc;
- reflection/thorns;
- aura;
- summon;
- environmental/world effect;
- other authored origins.

Default proc policies prevent accidental infinite recursion.

Examples:
- reflection should not recursively reflect itself forever;
- a proc-generated hit should not automatically retrigger the same proc chain unless allowed.

**Important:** recursion prevention is a default safety policy, not a hard ban.

A future class/item may deliberately allow **controlled recursion**. Such mechanics must opt in explicitly and define limits/conditions such as depth, count, cooldown, diminishing value or eligible trigger origins.

## Periodic effects: DoTs / HoTs

Periodic effects can choose their scaling model:
- **snapshot** — relevant source stats/modifiers are captured when applied;
- **dynamic** — values are recalculated from live state each tick.

Finite-duration DoTs/HoTs commonly default to snapshot unless content specifies otherwise, but both models are first-class.

**Acolyte Tier II design-only skill-specific example (2026-10-19):** Acolyte summons **player-controllable, timed skeletons**, costing **4 Soul Charges** (bar max **8**) per summon; **ACOLYTE-013 CONFIRMED: at most FOUR concurrently active skeletons for this Acolyte, each lasting 15 seconds** (Acolyte-specific limit only). Soul Charges generate **+1 per resolved/successful Drain Life tick** and **+1 per enemy death WHILE afflicted with the owning Acolyte's Radiation DoT**, subject to max8; **0** from merely applying Curse or Radiation. Radiation DoT propagates after **2 continuous seconds** of spatial proximity from an infected to a not-yet-infected target, but **must not bounce back to an earlier recipient within the same infection chain**. **ACOLYTE-016–020 update:** Radiation range **3m**, contact window **2 continuous seconds resetting on separation**; chained spread to new hosts, no reinfection of previously infected recipient within lineage, and direct recast refreshes duration. Skeleton **fifth summon replaces OLDEST**; their idle retaliation follows the hero's **Auto-Retaliate** toggle while user-issued orders take priority. Drain Life's **heal starts 25% of actual HP damage, +2 percentage points per rank** (possible future cap ~60–65%; exact cap open). Additional Acolyte **Mortal Coil** (one-second channeled enemy Magical Void damage based on INT+SPR OR an allied/summoned-unit heal) and the passive's weak **basic-attack poison STATUS** are class-specific authored mechanics, not global rules; Mortal Coil healing element and passive poison proc details are unselected. **ACOLYTE-021–025 newer user-approved specifics:** Mortal Coil **Void-element healing of self/allies/skeletons** (use normal elemental HEALING rules with target Void resistance), and hostile Mortal Coil success **+1 Soul Charge**; friendly healing grants none. Its **1s channel cancels on movement/stun/explicit interrupt only, not normal damage**. Poison passive **+2 INT/+1 SPR per purchased rank**, and successful basic wand attacks **guarantee one refreshable 3-second NONSTACKING Poison-tag DoT** whose damage scales from INT and passive rank; its Poison status tag **is not automatically a new Poison element**. **ACOLYTE-026–030 latest override (2026-10-20): skill caps/gates are NOW USER-APPROVED** (Curse14/Radiation16/Skeleton12/Drain20/Coil10/Passive8 =80, all class levels 1–80 accessible). **Mortal Coil DAMAGE (INT-weighted) and Void-element friendly HEAL (SPR-weighted) each scale both INT+SPR**, plus skill rank. **Poison-tagged wand-auto DoT deals MAGICAL COFFEE-element damage, NOT Donut** (explicit user correction), once/second for 3s, nonstacking and NONSPREADING; Coffee is the resistance element, Poison is the status tag. **Summon Skeleton spawns ONE skeleton per cast and costs 4 Soul Charges ONLY, ZERO Mana.** Hero Auto-Retaliate OFF skeletons attack only on explicit orders; ON they can fight back when attacked. Numeric CD/Mana/damage coefficient proposals in Acolyte doc not user-approved; NO Unity implementation. See Acolyte spec. Details on Soul gain through zero-damage or shielded ticks remain open. **Drain Life channels 5s, 1 tick/s, Void-element Magical damage + SKILL-RANK-SCALING self-heal as a percentage of actual HP damage done; shield-absorbed damage grants NO HEALING**, and cancels on player movement, stun or explicit interrupt, not ordinary incoming damage. This is proposed class content, NOT implemented or a global pet/summon resource cost rule. Details: [Acolyte Tier II design](classes/ACOLYTE_TIER2_DESIGN.md).

**Acolyte ACOLYTE-031–035 latest class-specific confirmation (2026-10-20; DESIGN ONLY):** Acolyte class FIRST-PLAYTEST cooldowns/Mana are approved: Curse12s/12, Radiation10s/15, Skeleton3s/**0 Mana+4 Soul**, Drain Life14s/24, Mortal Coil7s/14. Acolyte's contagious Radiation duration lasts **3s at skill rank1 and scales to 8s at rank16** (exact intermediate duration and tick frequency open). Acolyte skeleton skill ranks boost **summoned HP and BASIC attack damage only**, rather than increasing quantity, lifetime, or Soul cost. Its MAGICAL COFFEE Poison-tag DoT refreshes duration to3s without resetting its running 1s tick clock. **CRITICAL shielded Drain Life rule:** a successfully resolved Void damage tick damages eligible enemy SHIELD in the ordinary framework and **awards +1 Soul Charge EVEN when fully shield-absorbed**; its healing share is calculated ONLY from actual enemy HP loss, so **NO HP healing from shield-absorbed damage**. This is a **class-specific charge event**, not a global lifesteal-from-shield rule. Numeric raw damage/HP coefficients and Tenacity formula remain open. See [Acolyte spec](classes/ACOLYTE_TIER2_DESIGN.md).

**Acolyte ACOLYTE-036–040 FINAL class-specific rules (2026-10-20), superseding earlier Acolyte 1-second Radiation tick / full-infection-duration / VIT skeleton suggestions:** Radiation Magical DoT damages **TEN times per second (every 0.1s)**, RAW each tick **one tenth** of original `(2+0.10×casterINT)×(1+0.05×(r−1))` per-second tick proposal; no automatic Soul gains on these faster periodic ticks. Radiation duration `3+floor((rank−1)/3)`s for valid ranks1–16, spreading after2s continuous proximity within3m, and new recipients inherit **source's REMAINING DURATION** at propagation (not fresh time), with visited-host chain tracking. Summoned skeleton **HP `(55+2×ownerSPR_atSummon)×(1+0.05×(r−1))`, NOT VIT**; basic Physical RAW `(3+0.18×ownerINT_atSummon)×(1+0.06×(r−1))`, stats **SNAPSHOTTED when summoned**. Curse rank growth +1 flat MDEF reduction/+0.005 attacks/sec reduction per rank after1; baseline damage/healing coefficients user approved for initial playtest, not shipped. Class design complete; see [Acolyte](classes/ACOLYTE_TIER2_DESIGN.md). These rules are **Acolyte-specific only**, and do not alter global periodic ticks or generic summons.

## Summons, pets and controllable units

Owned combat units use a shared summon/companion framework.

Important rules:
- owned units do not consume player party slots;
- there is no universal gameplay summon cap;
- each ability/class defines its own quantity/lifetime limits;
- autonomous, directly controllable and hybrid units are supported;
- ownership and control are separate concepts and control can be transferred;
- summons share the owner's **level** rather than leveling independently;
- stats can be authored, snapshot-inherited, dynamically inherited or mixed;
- owned units can have their own/shared/no resources;
- own threat by default, with authored threat-transfer mechanics;
- persistent companions can have stable IDs and equipment;
- ally targeting is tag-filterable;
- no special summon collision exception is assumed until playtesting demonstrates a need.

See `docs/SUMMONS_AND_COMPANIONS.md`.

## Summon source attribution

Summon actions retain both:
- the **immediate source** (the summon);
- the **owner/controller** (the player/Echo manifestation or other owning actor).

Different systems can choose the attribution they need.

Examples:
- summon owns its own threat entry;
- quest/XP credit may resolve to the owning player;
- a proc may explicitly care about summon-origin damage;
- combat logs can show both source and owner.

## Auras

Auras are reusable **effect emitters**.

An aura defines:
- emitter/source;
- radius/shape;
- valid target relationship/filter;
- emitted effect identity;
- update/application/removal behavior.

Leaving the aura's valid area, emitter death/despawn, or aura removal stops that emitter's contribution.

Normal effect stacking policy then resolves overlap:
- strongest wins;
- unique per source;
- capped stacks;
- etc.

The same framework supports beneficial player auras, enemy debuff auras, item auras and future world effects.

## Effect stacking and dispels

Effects define their own stacking/refresh policy in data.

Supported policies include:
- unique per source;
- refresh duration;
- replace with stronger;
- add stacks up to a cap;
- independent instances;
- strongest copy wins;
- other explicit authored policies.

**Default direction:** many ordinary effects are **unique per source**, allowing different actors to maintain their own copy without one source creating uncontrolled duplicates.

### Dispel strength

The primary gameplay-facing dispel axis is:
- **Weak Dispel**;
- **Strong Dispel**;
- **Undispellable**.

A Strong Dispel can remove effects that a Weak Dispel can remove plus effects explicitly requiring Strong Dispel.

Undispellable effects cannot be removed by ordinary dispels.

Effects may additionally carry semantic tags such as:
- Poison;
- Curse;
- Disease;
- Bleed;
- Magic;
- CC;
- elemental/status families.

These tags support situational tools such as antidotes or specialized class abilities, but normal healer kits should not be forced into a large collection of narrow one-tag-only cleanse buttons unless that restriction is part of the ability's identity.

## Shields, sustain and combat-effect framework

The detailed generic framework for shields, lifesteal, reflection, immunities and CC resistance is documented in `docs/STATS_AND_DAMAGE.md`.

Key rules:
- shields can filter/scale differently by damage channel or element;
- default overhealing disappears, but classes/items may convert it;
- lifesteal/damage-to-healing is framework-level;
- reflection/thorns cannot accidentally recurse forever;
- immunity/untargetable flags are first-class statuses;
- CC resistance and repeated-CC diminishing returns are supported from the framework level.

HP regeneration remains active in combat. Resource regeneration is class/resource-specific.

## Consumables

Consumables have cooldowns.

Different consumables can use different cooldown durations/groups.

Classes, passives, gear and profession effects may modify consumable behaviour.

## Resurrection

Repeatable healer resurrection is allowed but costly:
- long cast time;
- high resource cost;
- stacking **Resurrection Sickness**.

### Resurrection Sickness — confirmed 2026-10-19

- Each stack lasts **exactly 5 minutes (300 seconds)**, with its **OWN INDEPENDENT expiration timer**. Acquiring a fresh stack does **not refresh or replace the remaining durations** of older stacks.
- Ending an encounter does **not** remove Resurrection Sickness. A stack ends only when its own 5-minute timer expires (unless a future explicitly authored cleansing mechanic says otherwise).
- **Each active stack reduces all five primary attributes by 10%**: Vitality, Strength, Agility, Intelligence and Spirit. Derived quantities change through their normal stat formulas.
- The **maximum stack count, attribute floor/minimum and save/load persistence** remain undecided; do not invent them in a class implementation.
- How much HP/Mana the revived target receives is **skill-specific**: Priest's Tier II Revive currently restores **15% maximum HP and 15% maximum Mana**, distinct from the sickness's 10%-per-stack attribute penalty. Other class revives may restore different resource amounts.

## Enemy AI and encounter architecture

Enemies use authored/fixed levels by default and do not universally scale to the player.

Enemy definitions can compose:
- base archetype;
- variant overrides;
- direct monster stats and/or primary attributes;
- combat actions;
- threat/targeting policy;
- AI decision rules;
- leash/home behavior;
- semantic tags;
- reusable modifiers.

AI may be as simple as "attack closest enemy" or use complex AND/OR conditional priorities for advanced enemies/bosses.

Encounter-wide state such as phases, hazards, doors, adds and timers belongs to reusable encounter definitions rather than being forced into one boss actor.

See `docs/ENEMIES_AND_ENCOUNTERS.md`.

## Enemy level display

Enemy levels are normally visible.

Current working idea:
- enemies far enough above the character may display **???** instead of an exact level.

A threshold around 20+ levels above the character is a candidate, not yet final.

## Combat state

Threat-list membership / active aggro is the primary definition of being "in combat".

See `docs/STATS_AND_DAMAGE.md` for shield ordering, Pure Damage, lifesteal, reflection, dispels, summons, death prevention and CC framework rules.

## Open questions

- offensive targeting details;
- Hybrid mode;
- controller support;
- final secondary effect of each primary attribute;
- universal vs class-specific Vitality-to-HP coefficient;
- exact defense diminishing-returns formula;
- resistance caps/penetration;
- detailed crit rules;
- exact Resurrection Sickness math;
- exact ??? level threshold;
- in-combat gear swapping.


### Defense penetration basis and order

Percentage Defense penetration resolves before flat Defense penetration, but percentage penetration is calculated from the target's **original positive Defense**, not from the already-reduced remainder.

Flat penetration may push effective Defense below zero.

If original Defense is negative, percentage Defense reduction and percentage Defense penetration do not apply; negative base Defense is not an intended normal content state.


### Penetration below zero

Penetration is not clamped at zero Defense.

If penetration exceeds the target's current Defense, effective Defense becomes negative and the normal negative-Defense vulnerability curve applies.

Example:
```
5 Defense - 10 flat penetration = -5 effective Defense
```


### Defense reduction vs penetration

Defense reduction resolves before penetration.

Percentage reduction and percentage penetration both reference the target's original positive Defense value for the resolution.

Default conceptual order:
1. establish original Defense;
2. subtract percentage reduction(s), additive from original positive Defense;
3. subtract flat reduction;
4. subtract percentage penetration from original positive Defense;
5. subtract flat penetration;
6. apply mitigation/vulnerability from final effective Defense.


### Full Defense modifier ordering

Percentage Defense reduction resolves before flat Defense reduction.

Default Physical/Magical Defense pipeline:

1. establish original Defense;
2. percentage Defense reduction(s), additive from original positive Defense;
3. flat Defense reduction;
4. percentage Defense penetration, calculated from original positive Defense;
5. flat Defense penetration;
6. final mitigation/vulnerability curve.

Flat reduction and flat penetration can create negative effective Defense.

Reduction is target-state modification; penetration is attacker-specific.


### Underlying Defense for percentage shred/penetration

Percentage Defense reduction and percentage Defense penetration reference the target's underlying Defense before temporary positive Defense buffs.

Positive Defense buffs resolve percentage first, then flat.

Percentage reduction/penetration may exceed 100% and can push final effective Defense below zero.


### Defense percentage stacking

Positive percentage Defense buffs stack additively.

Percentage Defense reductions stack additively.

Percentage Defense penetrations stack additively.

All three use the relevant underlying Defense reference already defined by the Defense pipeline; reduction and penetration may exceed 100%.


### Defense reference stability and precision

Temporary negative Defense effects do not alter the underlying Defense reference; they affect only current/effective Defense.

Defense math keeps fractional precision through intermediate calculations. Rounding is presentation-only unless a specific mechanic explicitly requires discrete values.

The exact components that make up underlying Defense remain an open design question.


### Explicit critical-hit rules

DiceFree has no universal baseline crit chance or crit multiplier.

An attack, heal, DoT tick or other resolved effect can crit only when an explicit gameplay source grants permission. The granting source defines that crit rule's chance and multiplier/behavior.

Therefore:
- baseline crit chance is 0%;
- there is no default 150%, 200% or other global crit multiplier;
- direct damage does not automatically crit;
- DoTs do not automatically crit;
- healing does not automatically crit;
- any of those may crit if a skill, passive, item or other effect explicitly says they can.

Ordinary simultaneous crit rules follow the highest-multiplier, first-success policy below. Modifier composition and rare stacking exceptions remain open; rules never collapse into a global chance/multiplier.


### Multiple explicit crit rules

When several explicit crit rules apply to the same resolved action, ordinary crit resolution uses priority by multiplier:

1. sort applicable crit rules from highest multiplier to lowest;
2. roll the highest-multiplier rule first;
3. on success, apply that rule and stop;
4. on failure, continue to the next rule;
5. if all fail, the action is non-critical.

This is intentionally not a merged global crit chance.

Rare authored rules may explicitly stack with or multiply another crit result, but such behavior must be stated by the source and is not part of default crit resolution.

### Explicit crit-rule modification

A source may explicitly alter another crit rule instead of adding its own independent roll.

Examples include modifying chance, modifying multiplier, or granting crit permission to a normally ineligible effect.

Such modifiers must declare their intended target/eligibility. There is no automatic global Crit Chance or Crit Damage stat.


### Crit modifier resolution order

Explicit crit-rule modifiers apply before crit-rule priority is determined.

After modifiers, ordinary crit rules are sorted by their final multiplier and rolled highest-first.

Ordinary crit chance is clamped to 0-100%. Values above 100% have no special default behavior; any overflow mechanic must be explicitly authored.

### Crit position in the damage pipeline

Default crit resolution modifies the raw damage packet before mitigation layers.

Default order:
1. construct raw damage packet;
2. apply successful crit multiplier/behavior;
3. apply Physical/Magical Defense;
4. apply elemental resistance;
5. apply resulting HP damage.

Rare explicit mechanics may override where their crit behavior is applied.


### Equal-multiplier crit tie-break

Ordinary crit rules sort by final modified multiplier descending.

For equal final multipliers:
1. explicit authored crit priority, if present;
2. otherwise stable deterministic source-ID order.

This keeps provenance deterministic without inventing a gameplay advantage from equal multipliers.

### Action-wide crit resolution

Default crit resolution happens once per action.

A multi-packet action shares one crit result across all eligible raw packets, which are then mitigated independently by their own Physical/Magical Defense and elemental resistance layers.

Per-packet crit rolls require an explicit mechanic.

Default critical-hit signaling is one critical-hit event per action. Explicit per-packet crit mechanics may opt into per-packet critical events.


### Crit modifier clarity and provenance

Crit modifiers are an available but intentionally uncommon tool.

Any modifier to crit chance or multiplier must explicitly define its operation, such as:
- additive percentage points;
- multiplicative change to existing chance;
- additive multiplier amount;
- multiplicative change to existing multiplier.

Do not infer one meaning from ambiguous "+X% crit" wording.

Critical resolution preserves both the originating action identity and the winning crit-rule/source identity. These are separate provenance fields for future action-trigger and source-trigger logic.


### Hit-before-crit ordering

For actions with accuracy/miss mechanics, resolve whether the action connects before evaluating crit rules.

A miss consumes no crit roll by default.

### Critical result vs HP damage

Critical resolution and HP damage application are separate stages.

A crit may occur even when later prevention reduces actual HP damage to zero. Trigger systems should be able to distinguish:
- an action that critically resolved;
- actual critical HP damage dealt.

### Child-effect crit inheritance

Triggered/secondary effects do not inherit the parent action's crit result by default.

Each child effect requires its own explicit crit permission/rules unless an authored mechanic explicitly says it inherits or copies the parent crit.


### Elemental penetration in healing resolution

When an elemental penetration effect explicitly applies to elemental healing, it works opposite to its damage behavior:

- for damage, penetration lowers the target's effective matching resistance;
- for healing, penetration raises the effective matching resistance used by that heal.

Example: +10 percentage points of Fire penetration applied to a Fire heal treats the target as having +10 percentage points more Fire resistance for that heal.

Effects must explicitly state whether their penetration applies to damage, healing, both, or another context.

Actual resistance debuffs alter the target's resistance itself, so they affect matching elemental damage and matching elemental healing.

Negative resistance remains uncapped. Elemental healing reaches zero at -100% effective matching resistance and becomes damage below -100%.


### Resistance overcap

Elemental resistance retains an uncapped raw value. The normal positive resistance cap applies only when deriving the effective value used for resolution.

Debuffs and penetration operate on uncapped raw resistance before the positive cap is applied.

Example:
- 110% raw Fire resistance;
- -20 percentage-point Fire resistance debuff;
- 90% raw remains;
- with a 75% cap, effective Fire resistance is still 75%.

Ordinary resistance bonuses/debuffs stack additively by default. Multiplicative/special stacking must be explicitly authored.

Negative resistance remains uncapped.


### Resistance percentage wording and >100% inversion

Elemental resistance and elemental resistance penetration are always expressed as percentages in authored/player-facing text.

Penetration values are percentage-point subtraction from matching resistance and stack additively by default.

Explicit resistance-cap increases may raise the normal 75% cap, including beyond 100% if an unusual build/mechanic manages it.

Matching elemental damage uses the resistance factor:

```
1 - effective resistance
```

Therefore:
- 100% effective resistance negates matching elemental damage;
- above 100%, the result becomes healing;
- 105% effective resistance heals for 5% of the matching damage that would have resolved at 0% resistance.

This behavior is intentionally allowed even though normal encounter design should not depend on players reaching >100% resistance.


### Resistance-inversion result handling

If elemental resistance above 100% turns incoming matching elemental damage into HP restoration:

- Healing Received does not modify the amount;
- the target still counts as having been hit by the originating elemental action;
- the target did not take damage;
- the result restores HP through a distinct resistance-inversion category;
- it does not automatically satisfy generic ordinary-heal triggers;
- damage shields do not consume capacity because the result reaching the shield stage is not positive damage.

This is separate from authored healing and from elemental-healing inversion.


### Mixed packet damage and resistance healing

Packets from one action resolve independently.

One action may simultaneously:
- deal HP damage through one packet;
- restore HP through >100% elemental-resistance inversion on another packet.

These are preserved as separate action outcomes rather than collapsed into a net damage number.

If any packet deals actual HP damage, the action counts as having caused damage even if another packet restored HP.

Resistance-inversion restoration is capped by missing HP. Excess is discarded unless an explicit mechanic says otherwise.


### One element per packet and cap modifiers

Each damage packet has at most one element. Multi-element actions use multiple packets.

Resistance-cap modifiers are independent from raw resistance:
- global cap modifiers can affect all elements;
- element-specific cap modifiers affect one element;
- cap bonuses/reductions stack additively by default.

Example:
- base cap 75%;
- +5% all-element cap;
- +10% Fire cap;
- Fire cap = 90%, other elemental caps = 80%.

A cap debuff lowers the effective ceiling without changing raw resistance. For example, 110% raw Fire resistance with a 55% Fire cap resolves as 55% effective Fire resistance.


### Resistance-cap floor and no inversion resurrection

Elemental Resistance Cap bottoms out at 0% by default.

Reducing the cap below 0% does not create vulnerability; raw/effective resistance must actually be reduced below 0% for vulnerability.

Elemental healing uses the same effective Resistance Cap as elemental damage after its context-specific resistance/penetration calculation.

Resistance-inversion HP restoration from >100% elemental resistance does not resurrect dead targets unless an explicit resurrection mechanic says otherwise.


### Default hit and action-wide miss resolution

Actions hit by default unless an explicit source introduces miss chance. There is no universal hidden baseline miss chance.

When miss chance exists, it resolves once per action by default. A miss causes all normal packets from that action to miss and crit resolution does not run.

Explicit per-projectile/per-packet hit checks require an authored exception.

Miss chance is expressed as an explicit percentage. Ordinary miss-chance modifiers stack additively in percentage points and the final chance is clamped to 0-100%.


## 2026-10-26 — Default overlap of independent damaging AoEs (user-confirmed)

**GLOBAL DEFAULT:** If two or more independently created damaging AoEs overlap spatially and temporally, **each AoE resolves its own full damage against eligible units on its own hit/tick schedule**, including when the effects originate from different friendly casters or multiple Psions. Do not automatically reduce damage, merge pools, cancel one AoE or replace an older instance just because the damaging regions overlap. Ordinary per-packet mitigation, shields, element/resistance, hit volumes and target eligibility still apply. Explicitly authored exceptions for nonstacking statuses, same-source effects or specialized mechanics remain possible and **do not redefine the default for independent damage packets**.

**Concrete Psion confirmation:** multiple independent Lightning Storm ground circles can overlap; each storm delivers its own full, separately resolved Lightning waves at 0.0/0.3/0.6s after the 0.1s cast, if the owning Psion sustains its 0.6s channel. Every wave separately checks occupants of that storm's 4m circle, under universal AoE hit-volume rules. See `docs/classes/PSION_TIER2_DESIGN.md`. Design rule only, not an implementation claim.
