# Crimson Crucible

Crimson Mire now has a repeatable six-boss dungeon at the red-crystal arch east of its central trail (World 1 X80/Z149). Recommended level: 55. This is a short gauntlet rather than a wave survival dungeon; there is no new hard level gate or main-story prerequisite.

| Cell | Boss | Main warning |
| --- | --- | --- |
| 1 | Verdigris Globule | Alternating targeted gel bursts and a circle around the boss |
| 2 | Moonfang Alpha | Locked frontal cone, followed by a pounce |
| 3 | Bristlebreaker | Broad frontal slam |
| 4 | Bloomheart Thornwarden | Targeted root circles; hits slow by 35% for 2.2 seconds |
| 5 | Stormscale Hierophant | Three narrow lightning fans with gaps between them |
| 6 | Crimson Sovereign | Targeted eruptions plus alternating boss-centered bursts; faster casts below half health |

Each guardian has its own 36m-square walled cell. Five portcullises physically block the connecting corridors and carve the runtime navigation mesh. Defeating the current guardian opens only its gate. The next guardian appears when a living participant enters the newly available cell. Future guardians cannot be attacked early. These bosses have no overworld respawn controller or health regeneration.

Boss warnings use the existing dragon encounter telegraph geometry and common encounter HUD. Specials have interruptible windups; normal stun cancels the pending impact. The wolf stops short of its locked target rather than landing inside a character. Model, materials, animation and decorations reuse the project's existing slime, wolf, Chieftain, Thornlash, lizardman, red slime and VisualBatch01 library content. Boss family identifiers remain intact for existing family-based kill credit.

## Entry and rewards

The dungeon uses `StoryDungeonRun`: the same shared waiting-room, initiator-start, loadout-lock, death-return, treasury and exit foundation used by the story dungeons. Up to four participants can join. Each extra living participant adds 65% of the boss's base maximum HP at activation. Leaving, wiping, or returning to the menu clears the encounter; a fresh attempt resets all six cells.

The last boss grants the clear bonus of 12,000 XP and 650 gold through the shared completion flow. Individual boss defeats also report their ordinary XP. No intermediate guardian awards treasury gear. After the final victory, each eligible participant receives a separate normal treasury offer. The unchanged pool rules use a 5% rare branch; otherwise there is one ordinary item, with a 25% chance of offering two distinct ordinary items. A player claims one item or chooses the 4,000 XP / 180 gold alternative. Another player's choice cannot consume their offer.

## Gear coverage

An audit of the existing authored equipment found no level-40-or-higher legs or backs, and only one level-50-or-higher head and shoulder item each. The new pool therefore fills those four slots instead of adding another chest set.

| Role | Set | Emphasis |
| --- | --- | --- |
| STR / tank | Bristleforged Warhelm, Pauldrons, Cuisses, Warcloak | STR, vitality, physical defense, regeneration |
| AGI / physical attacker | Moonfang Cowl, Mantle, Legwraps, Hunting Cape | AGI, attack speed, flat basic-attack damage |
| INT / caster | Stormscale Hood, Epaulets, Leggings, Spellcloak | INT, SPI, magical defense |
| SPI / healer | Bloomkeeper Veil, Shawl, Legwraps, Prayer Cape | SPI, INT, magical defense, regeneration |

All 16 ordinary pieces are level 55 and unrestricted by class ID, so the role most appropriate to a class or advancement can equip them. The rare universal back, **Crucible Victor's Shroud**, grants +12 STR/AGI/INT/SPI, +20 vitality, 18 physical and magical defense, 4% attack speed, 3% movement speed and 3 regeneration. Other backs have 2% movement speed.

Each item has a rendered slot icon. These PNGs were rendered from the locally installed Polytope modular armor meshes, tinted per role and decorated with a small trial gem. The raw armor FBX remains outside this change; it is an authoring input, not a new runtime asset dependency. Gear is ordinary stat equipment, with no new set-bonus system or claim of new worn character meshes.

## Validation and delivery

Evidence: [review folder](validation/crimson-crucible-2026-11-25/README.md).

- Windows development build succeeded with 15 scenes, zero errors and one existing collision-prebake warning.
- The full review passed 137 checks: complete solo and two-member runs, every gate traversed by normal movement, actual special damage, dodges, root slow, interrupts, inactive future bosses, all 17 icons/items equippable, 6,000 seeded pool rolls, private treasury choices, duplicate-claim prevention, abandonment and menu cleanup.
- The delivered build passed 17 additional checks: actual owned-item disk reload, real stun cancellation, wipe stopping all encounters without opening gates or granting gear, death-return revival, fresh-run reset, and scene/session cleanup on menu exit.
- Main project import/compile exited zero with no C# errors. A receipt compares 141 task runtime assets and source files between main and the QA build project. Cornberg scene record order was normalized after copying; its diff only adds the entrance.

Runtime review was automated against a disposable copied profile. Boss completion used real health packets with accelerated lethal damage; special attacks resolved real combat packets. Party coverage used two local party actors through the shared dungeon flow. These checks do not establish online multiplayer, manual difficulty balance, or every class's full combat clear. Screenshots explicitly render the world camera in a batch player and omit screen-space overlay UI.

The full 137-check build was `CrimsonCrucible-20261125-090151`. The delivered `CrimsonCrucible-20261125-091545` uses the same gameplay code, brighter inventory icons and the decorated entrance; its separate 17-check lifecycle report is included. The final development player retains the project's local runtime automation configuration. Saves used for review are not committed.

Unrelated PackageManager settings and old recovery scenes remain outside this change. No merge into main is part of this delivery.
