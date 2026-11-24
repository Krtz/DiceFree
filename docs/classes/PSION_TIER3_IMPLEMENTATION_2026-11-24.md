# Psion Tier III implementation — 24 November 2026

**Presentation follow-up:** the original screenshot still had arm deformation. The [arm repair](PSION_ARM_REPAIR_2026-11-24.md) supplies the corrected mesh/avatar binding, fitted arm animation and grip placement, with new close-up and live-gameplay evidence.

Psionic Knight and Mirage Mage are playable in Cornberg and the development room. Implementation follows the approved Round 22 decisions in [the design history](PSION_TIER3_EVOLUTIONS_DESIGN.md), fetched from `feat/verdant-wilds` at design commit `7112c94d5ee408165eb4e6b8f90599d41ae5d14a`.

## Advancement

A level-60 Psion can accept both hidden teachers' trials concurrently. Seren, the Whispered Blade, is in Ironpine; Vaelis, the Glass Seer, is in Steamroot Vale. Discovery within 2m works for every class and level, persists across the Echo, and adds only discovered names to the Codex. These teachers do not advertise themselves through map pins or undiscovered Codex placeholders.

The Knight requires a new solo completion of all ten existing Moonlit Clearing waves. The Mage requires a new solo completion of existing Fourfold Hollow, with its ordinary entrance requirements. A quest must be active when that run starts, and the same manifestation must finish it. Historical Codex clears and grouped clears do not qualify. Return to the corresponding teacher to advance; dungeon victory alone does not advance the player.

Each child starts at its own level 1. The original Psion, including its other trial, remains independently playable. Each route awards and equips its own main hand and Ionic Stone through the existing advancement transaction.

## Combat and progression

Both classes have their approved attributes, growth, HP, Mana profiles, basic-attack weights and reaches. Exact per-rank unlock tables and the 150-point limit are implemented, with five skills per class:

| Psionic Knight | Maximum rank | Mirage Mage | Maximum rank |
|---|---:|---|---:|
| Psionic Blades Mastery | 30 | Psychic Bolt | 34 |
| Mobile Mental Image | 25 | Mirroring Mental Image | 25 |
| Void Aegis | 29 | Lightning Storm | 30 |
| Empowered Attacks | 34 | Metal Shield | 29 |
| Speed Presence | 32 | Speed Presence | 32 |

Knight empowerment resolves ordinary Physical and separate non-critical Magical Void packets. Funded landed hits, including shielded hits, spend one charge and its Mana; misses and unfunded hits preserve charges. Images receive finite independent charge snapshots and never spend the owner's resources. The ending haste uses the approved strongest-only replacement/refresh behavior and is lost when the owner is dead.

Knight images move, follow and acquire enemies. Mage images remain stationary, mirror committed spells after 0.4s, snapshot original raw amounts, omit copied control effects, retarget when the original enemy dies and cancel pending echoes when the source image is gone. Copied shields have independent shared pools; secondary images are bounded to one generation and retain full explosion damage with half HP/basic damage/lifetime. Existing images survive owner death.

Speed Presence uses a common strongest-only 12m aura. Images receive it without becoming emitters. Supported positive timed speed buffs, damage reduction and basic-attack augments are copied with bounded remaining timers; the Knight applies its rank-dependent temporary-buff potency. Its personal shield and image-ending haste also have explicit bounded copying rules.

All five slots have names, rank requirements, scaling information and relevant raw damage or shield totals. Mana and cooldown begin only on cast commitment; invalid or interrupted pre-commit casts spend neither. Moving or being stunned interrupts remaining Storm waves.

Existing shared AGI/INT equipment compatibility includes authored class gear and dungeon rewards, including Packhunter's Fang and Starfall Wand. Ionic Stones support both new classes.

## Presentation and assets

Warrior Mage supplies the Knight; Card Magic Girl supplies the Mirage Mage. Both use native humanoid animation, equipment bindings, spectral reflections and equipped HUD/menu portraits. Starter items include Whispersteel Mindblade, Prismbound Grimoire and two Ionic Stones, with authored icons and hand props.

The Knight's supplied mesh had been recentered independently of its skeleton. Its native FBX derivative normalizes transforms, aligns the skeleton to the visible mesh and repairs legacy root weights. This removes the animation stretching. Portrait framing also handles imported unit conversion without applying it twice.

Canonical source packages remain under `Assets/_DiceFree/Art/Vendor`, with verified vendor locks and receipts. Native derivatives and their hashes are recorded in `tools/assets/psion-tier3/native-derivatives.json`; reproduction notes are in that folder's README. The archives did not provide verified author/license metadata, so provenance remains honestly recorded as `LicenseRef-UserSupplied`, under the user's existing prototype-use authorization.

## Verification

The standalone development build contains all 14 existing enabled scenes, including prior World 2 work. Build result: succeeded, zero errors, one existing collision-prebake warning. Results and unedited screenshots are in [the validation evidence folder](../validation/psion-tier3-2026-11-24/).

- **653 integration checks:** copied old Psion save, eligibility, both concurrent quests, negative historical/grouped credit, normal existing dungeon staging/start/victory flows, teacher turn-ins, separate child/parent manifestations, every legal point purchase through level 150, core abilities and save writes.
- **58 focused combat checks:** funded/missed/shielded/unfunded empowerment, independent image charges, timed-buff potency, personal shield copying, haste stacking, owner-death survival, invalid/interrupted casting, interrupted Storm waves, dead-target fallback, snapshotted spell damage after a stat change, no copied stun, independent shields, cancellation and bounded secondary images.
- **21 fresh-process checks:** menu availability, equipped previews, both level-150 saves and all 150 ranks each, both teacher discoveries, class switching and the separately preserved parent level.

These **732 checks** use isolated copied saves, controlled targets and damage fixtures to complete dungeon encounters. They verify integration and mechanics; they are not a normal-speed balance or multiplayer playtest. Approved numbers remain initial playtest values.

The actual `DiceFree-Options` working project also completed a Unity 6000.6.3f1 import/compile with exit code 0 and no C# compiler errors. Existing project configuration remained byte-identical across that import. The import summary is included with the validation evidence.

Scene integration preserves every pre-existing scene object and the original document order: Cornberg adds 54 serialized objects and changes 11; DevelopmentRoom adds 45 and changes 10; WorldTwoArrival adds eight and changes one. Whitespace and default-value serialization churn were excluded. Existing working-tree changes and configuration were preserved.

Runtime harnesses are retained in `tools/validation/PsionTier3*.cs` for the existing Unity automation `eval_file` workflow. Run the integration scenario on a fresh isolated copied Psion save, then combat checks; restart with that saved profile for the reload checks. The tests intentionally modify their isolated fixture profile.

No commit, push or merge was performed for this implementation.
