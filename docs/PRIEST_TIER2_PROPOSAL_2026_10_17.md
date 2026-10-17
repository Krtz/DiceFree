# Tier II Priest — design proposal, 2026-10-17

Status: discussion draft, NOT a shipped class or available advancement quest.
The Trimery healer service is a separate implementation in this change.

## Confirmed direction
- Tier II Priest, advancing from level-30 Magically Touched; staff user.
- Mana and Grace are separate resources. EVERY active skill costs Mana,
  including any Grace-empowered version.
- Grace improves both healing and damage.
- A reactive healer who remains viable alone and deals meaningful damage.
- Latest preferred core: Holy Light attack, Targeted Heal, Heal Nova, Dash.
- The Trimery healer is the Priest advancement mentor.
- The existing flexible skill/action system must remain flexible. Four core
  spells are a starting kit, not a slot-count or passive-count limit.

Sources: docs/TIER2_CORE_CLASSES_DESIGN_V02.md plus the user's earlier
Priest discussion (Oct 13–14) and current Trimery healer request (Oct 17).
No guessed numbers are represented as approved balance.

## Recommended starting kit (proposed)
| Skill | Role | Solo / group behavior |
| --- | --- | --- |
| Holy Light | Reliable ranged holy attack | Enough damage to quest alone; contributes to Grace |
| Targeted Heal | Responsive single-target heal | Explicit friendly target; self-cast supported |
| Holy Nova | Short-radius heal and holy damage | Heals living allies and damages enemies, each once per cast |
| Pilgrim's Step | Short directional dash | Reposition out of danger; respect walls and terrain |

All four cost Mana. At zero Grace both healing and offense still work.
No mandatory party, heal-over-time upkeep, pet, or damage specialization
is needed to make the class function. Tier III can specialize later.
Names, ranges, cooldowns, coefficients, unlock levels and mastery ranks
remain proposed, not final.

## Grace decision still needed
Recommendation: build Grace through effective offense and meaningful healing,
then deliberately empower a chosen heal, Nova or Holy Light. This lets the
player save a stronger heal for an emergency or spend Grace offensively alone.
The empowered cast still pays its normal Mana cost.

Safeguards for eventual implementation:
- No Grace for overhealing, invulnerable/dummy spam or missed attacks.
- Grant at most once per cast; extra AoE targets, chain jumps or ticks must
  not multiply generation.
- Empowerment spending must be atomic with a successful cast; canceled/invalid
  casts cannot consume resources or generate Grace.
- A Grace-empowered cast must not refund its own full Grace cost.
- Friendly targeting must distinguish living allies from hostile/dead targets.
- Decide explicit opt-in versus automatic spending with the user before coding.
- Cap, gain, spend, persistence and decay rules are NOT confirmed yet.

## Advancement quest proposal: Hands of Mercy
Giver/turn-in: the same Trimery healer, not a second mentor NPC.
Eligibility: level-30 Magically Touched, using existing advancement rules.

Proposed playable sequence:
1. Accept training from the healer. Pre-acceptance actions do not count.
2. Borrow a temporary quest healing action, so owning a particular
   Magically Touched skill/rank is not an accidental prerequisite.
3. Aid three distinct wounded lumberworkers near the safe north-road area.
   Track identities rather than letting one patient be healed repeatedly.
4. Protect one recovering worker through a short, solo-friendly encounter.
   Combine Holy Light practice/normal damage with reactive healing.
5. Return to the healer and explicitly turn in. Only then advance to
   Priest level 1, retaining the earlier manifestation per existing rules.

The borrowed action is removed on completion/cancellation; never overwrites
the player's action loadout. Save/load cannot duplicate rewards or bypass
required stages. Failed rescue can be retried without consuming a character
or requiring another player. Exact location, enemies and balance need approval.

Why this trial: it teaches what Priest does without repeating Greatblade's
Slime Boss kill or Mage's Slime Dungeon puzzle. It is not an escort across
the whole world and does not require another player to be injured.

## Resurrection
Do not conflate town healing, the existing resurrection stone and the future
Priest resurrection skill. Prior resurrection direction (expensive, long cast,
repeatable with stacking sickness, dead player eligible until self-respawn)
is retained for later design; it is not silently added to this initial kit.

## Implemented separately now
A placeholder ivory/blue priest blob beside Trimery's green provides free
HP treatment to living players via normal interaction. No Mana/Grace refill,
gold charge, quest completion, class unlock or resurrection is attached yet.
Final priest art and the treatment house are not part of this placeholder.
