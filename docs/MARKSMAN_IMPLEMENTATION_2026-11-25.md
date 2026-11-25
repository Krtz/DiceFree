# Marksman implementation

Ranger's Tier III **Marksman** is implemented from the approved design rounds 1–9. It is a separate level-1 manifestation advanced from a level-60 Ranger; the Ranger and its saved progression remain available. The class has six named skills, 150 individually gated ranks, a native library model, compatible shortbows/longbows, a settlement-tour quest and a Tier III menu form.

## Advancement

Find **Aelwen, the Farwatch**, at the woodland edge south of Willowford in Steamroot Vale, World 2. A level-60 Ranger can accept **The Farwatch Pilgrimage**. Acceptance reveals one small archery target in each canonical town/city:

| World 1 | World 2 |
| --- | --- |
| Cornberg | Cinderwatch |
| Trimery | Willowford |
| Arborium | Anvilreach |

Ironpine and Briarback are wilderness regions, so they are not counted as towns. These six locations are authored from the current saved scenes; no new settlement was invented. Targets stand in accessible outdoor spaces, with connected navigation checked from each world's arrival area.

The quest journal lists each settlement with its own 0/1 or 1/1 count. Visit them in any order across both worlds. Only a successful **ordinary Ranger bow basic attack** credits a target: missing, passing through, AoE damage, another class and a repeat hit cannot grant extra progress. Targets are invulnerable quest props without defeat rewards. The target hides after that player's completed shot; the journal saves progress immediately. Targets stay hidden while the local manifestation is ineligible or the quest is not active. Credit belongs to the firing Ranger's journal rather than shared kill credit.

Return to Aelwen after all six hits. The ordinary advancement transaction creates Marksman at level 1, grants/equips a two-handed Ironpine Yew Longbow and preserves Ranger. The new manifestation uses the existing resurrection-anchor placement behavior; advancing does not promise to leave the new level-1 hero beside the teacher. The transaction now supports valid two-handed starter mains while rejecting an accompanying offhand atomically. Existing one-handed main/offhand starter loadouts remain supported.

## Combat kit

Let `r` be the purchased skill rank, `n=r−1`, and `A` current AGI including gear and Bow Mastery. All skill formulas below are raw damage before critical/defense/resistance. Normal item power and speed still affect basic attacks; skill formulas use current AGI rather than borrowing basic-only aura damage.

| Skill | Cap | Behavior | Cost / cooldown |
| --- | ---: | --- | --- |
| Aimed Shot | 30 | 1.5s stationary windup; guaranteed physical hit; `15.2+0.60n+(2.15+0.091n)A`; ignores `10%×r/30` target PDEF for this packet; eligible ordinary Bow Mastery crit | `18+12r` Mana / 7s |
| Arrow Rain | 25 | 1.5s interruptible stationary channel; then six noncritical physical waves across 2s in a 3m circle; each wave `3.2+0.176n+(0.40+0.022n)A`; released rain survives movement/stun | `20+15r` Mana / 22s |
| Bow Mastery | 20 | +3 AGI, +0.2m reach and +1.5 percentage points of basic/Aimed crit chance per rank; eligible crit multiplier 1.6× | Permanent passive |
| Strafe | 30 | +2r% attack speed and −2r% movement speed for `ceil(1+r/5)` seconds; activation cost only; normal stationary arrow windups remain | `10+9r` Mana / 18s |
| Ranged Damage Aura | 20 | +r% allied ranged BASIC raw damage, including self; radius follows current basic reach; strongest same-name source only; never boosts skills | Permanent passive |
| Power Shot | 25 | Noncritical physical projectile, 1.5m width; `9.2+0.40n+(1.25+0.058n)A` first raw, then ×0.8 for each successive distinct enemy; individual mitigation afterward; stops at first solid wall | `18+13r` Mana / 12s |

Basic bow damage is authored as **8 + 1×AGI** before item modifiers and the normal game damage spread. Basic/Aimed reach starts at 11m and caps at 15m; Power Shot reaches 1m farther, 12–16m. Shortbows and longbows remain compatible and share class reach. There is no Hawk Dive, Hunter's Mark, pet or additional resource. The exact six approved rank-gate sequences are stored unchanged in `MarksmanProgression`; all 150 points can be spent by level 150, starting from rank zero with one point at class level 1.

Starting attributes: VIT19 / STR10 / AGI34 / INT9 / SPI11. Growth: +2 / +0.9 / +6 / +1 / +1.2 per level. Base HP20, 15 HP/VIT, movement5, and neutral base defenses follow the approved starting values. At level 150 with Bow Mastery20 and no attribute gear, AGI is 988.

All six hotbar entries include their spell name, rank/cap, scaling or buff values, current raw total, Mana cost, cooldown and next eligible class level where applicable. Ranged aura damage is applied only at actual basic packet resolution. HUD attack previews show it, while the existing basic-damage snapshots used by Ranger skills and Psion images remain unbuffed so those effects cannot inherit the aura twice.

The aura has a green-and-gold arrowhead sigil. Sigils sample the visible travel-surface height above the physical ground and draw after transparent road overlays, so raised dirt roads and paving do not cover them. Depth testing still hides the sigil underneath opaque actors and objects. Existing travel-speed containment keeps its early-exit behavior; visual height queries separately select the highest eligible surface.

## Shared Mana and Ranger correction

All Mana profiles now share the engine-enforced formulas:

`MaximumMana = ClassBaseMana + 5×INT`

`ManaRegenPerSecond = 4 + 0.1×SPI`

Eleven current Mana profile assets were audited/migrated. Per-class base maximums remain intact; Marksman uses base180, giving 225 maximum at level 1 and 970 at level 150 before external maximum modifiers. Flat maximum/regen modifiers and existing regeneration auras still pass through the resource controller; its actual regeneration tick now uses that same composed value. Non-Mana resources retain their authored formulas. Loaded current Mana clamps through the normal save pipeline; this change does not refill every existing save.

Ranger's authored basic AGI weight is corrected from 1.05 to 1.0, retaining its base damage5. The Ranger authoring helper carries the same correction. Current skill costs and approved high-rank numbers remain first-playtest values. In particular, all four max-rank Marksman actives cost 1,396 Mana together, exceeding its unmodified 970 pool; no claim is made that one full uninterrupted sequence is affordable without recovery, equipment or later tuning.

## Art

The library's existing rigged **Ironpine Wood Elf** is reused as a distinct **Farwatch Marksman** variant: ivory overcoat, deep green quilted cloth/lining, aged brass and pale hair. Its native valid rig, three skinned body/bow meshes and tested Ranger motion controller are retained. Both working bow presentations remain supported, with longbow favored by default. This is a library adaptation, not a newly modeled or newly rigged character. The source FBX and materials already exist in tracked native art; new prefabs/material variants introduce no private source-pack dependency.

## Verification

The saved-scene/static review passed **198 checks**: all approved rank gates, class/basic values, all 11 Mana profiles, non-Mana preservation, six target approaches, teacher approach, class providers and Tier III model/menu registration. The full Windows-player review passed **426 checks**, including real bow misses/hits in both worlds, the real advancement transaction and two-handed bow, all 150 purchases through the ability provider, both bow types, guaranteed Aimed/PDEF bypass, movement/stun cancellation, six actual rain packets, piercing falloff/wall collision, Strafe values/expiry, strongest-only aura and basic-only damage. **20 reload/cleanup checks** cover actual disk reload, exact ranks, parent/checklist preservation, two-handed/offhand starter rejection, one-handed starter compatibility and death cleanup. Further final-build road/aura checks are recorded in the evidence folder.

These are automated reviews on a disposable copied profile. Additional classes and travel prerequisites were seeded solely for review setup; quest target credit and advancement used normal bow/projectile/quest transactions. Synthetic actors isolate combat packets and aura recipients. No claim is made of manual balance acceptance or live network co-op testing. World-camera screenshots omit screen-space overlay UI in batch mode.

[Evidence and Windows delivery receipt](validation/marksman-2026-11-25/README.md).
