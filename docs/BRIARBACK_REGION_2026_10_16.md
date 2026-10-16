# Provisional northern quilboar region

This implements Axel's selected next step: a connected World 1 region with a quest, normal groups, elite and useful equipment reward. **Briarback, Scout Runa, Harrowtusk, all quest/item names and all balance values are provisional implementation choices.** They are not newly approved world canon.

## Route and boundaries

North from Trimery's gate, along the existing approach, through the old watchpost and five woodland camps, with a branch to the elite stockade. The western upper trail indicates the future Big City direction but does not build the city. Trimery's WEST road remains destroyed by the Ogre Roadwarden; no new lore about who ordered him, encounter or repair is introduced.

Terrain joins the old northern edge with overlap. The old invisible northern navigation wall moved from z250.35 to the new outer ridge at z400. Camera/fog use explicit scene-owned bounds (-85,-70) to (353,406); dungeon return restores these bounds. Dungeons beyond the overworld grid use local radial/obstacle visibility without replacing overworld exploration arrays.

## Encounters

Twelve normal enemies, in four groups of Raider, Spear Thrower and Totemcaller, at levels 21/23/25/27. Harrowtusk is level 30. These are humanoid quilboars, distinct from Ironpine's animal quillrats.

All use existing CombatActor, ActorMotor, BasicAttack, AggroBehaviour, shared damage/status systems and respawn. Always-aggro policy prevents higher-level Echoes being ignored. Native leashes stay clear of Trimery. Melee club, ranged physical javelin and ranged Nature bolt establish three readable roles. Ranged projectiles are visual only; they do not apply duplicate hits. Normal respawn 65s; elite 180s.

Elite Bristlebreaker Slam: 4m amber warning ring, 1.1s windup, 160 raw physical damage, 9s cooldown. Walking out dodges it. Stun, death, disable and leash return cancel the windup and release only its owned action/motion blocks. Harrowtusk has 50% stun-duration resistance, not blanket stun immunity.

## Quest/reward

Scout Runa stands inside the northern approach and has an explicit roadside conversation point. The Northern Trail:
1. Reach the old watchpost.
2. Defeat 8 quilboars after acceptance/active stage.
3. Defeat Harrowtusk and return.

No hard level gate. Existing kill-credit rules are reused. Earlier kills, incomplete turn-in and duplicate reward attempts must not grant credit/rewards.

Reward: 150 gold, 1800 quest XP and one bound Bristlehide Mantle; Back slot, all classes, +3 Physical Defense, +2 Magical Defense, +3 Vitality. Original brown scalloped cape/bone studs display on live manifestations and equipped menu portraits. Item can neither sell nor use the wrong slot. Both player catalogs and menu catalog include it; the five-class gear test exercises the mantle.

## Art / scope

Original offline Blender creature: 2651 authored vertices, 5098 triangles, 22 bones, Idle/Locomotion/Attack/Slam/Death actions. Reuses first-party Novice rig/action builder, not a third-party creature. Original role equipment and URP palettes. Catalog of 240 downloaded packs searched first; no suitable quilboar creature found. No paid generation or canonical game-dev package claim. See SourceArt/Creatures/Briarback.

Woodland broadleaf placeholders can now receive the optional licensed textured tree visuals from the environment pass. Camp shelters and some effects/icons are still prototype art. No Big City, Roadwarden fight/repair, production multiplayer region authority, or final balance is claimed.

## Verification

Static topology/composition validation passed: 13 creatures, 16 complete navigation paths and animated mesh deformation. Focused runtime passed 164 checks: 30 actual walking checkpoints (607.8m), level-200 pursuit without level disengagement, 20.5 degrees of live walking leg motion, leash return, actual elite dodge/160-raw impact/stun interruption, preacceptance and stage credit, nine scripted defeat fixtures, exactly-once reward, live equipment, durable save and equipped menu portrait.
Functional quest/kill verification uses scripted defeat fixtures; pursuit, walking, dodge/impact/interrupt and animated deformation are separately exercised. This is not a complete manual combat playthrough.
The final camp/creature captures are an additional isolated visual fixture with the reward granted for framing; they are not presented as a screenshot of naturally earned gear. The giant city-tree backdrop was moved beyond this region after visual inspection; a final rendered check confirms the camp is clear. The five-class gear suite also checks mantle world size.
Final seven-scene Windows build and startup passed with zero build warnings/errors. JSON: docs/validation/Briarback-*.json and Environment-*.json.

Commands: dicefree.briarback.author / validate / test / status / resume. Author rebuilds only the region root and its own content, appends catalogs including inactive development player, preserves other world services and rebakes expanded navigation. Runtime tests use isolated saves; scripted defeat fixtures test credit/reward behavior separately from pursuit, walking, real slam impacts/dodges/interrupts and live skinned animation.
