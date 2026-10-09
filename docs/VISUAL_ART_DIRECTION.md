# DiceFree Visual Overhaul — Approved Direction (Interview)

Status: Round 1 approved by Axel on 2026-10-09. Round 2 onward pending. This is direction for planning, NOT authorization to replace content or alter gameplay yet.

## Central progression rule
**Visual escalation with progression:** early/low-level areas, classes, enemies, units, equipment, loot, buildings and experiences should be comparatively simple and grounded. As levels, regions and challenges rise, visuals progressively become grander, stranger, more elaborate and more epic. Avoid giving starter content the visual complexity of endgame content. Early-world beauty and strong landmarks are still welcome: simplicity is NOT low quality.

## Round 1 approved decisions
1. First impression: grand, epic fantasy RPG.
2. Environments: detailed, optimized for isometric viewing.
3. Six world faces: let individual region styles develop organically.
4. Cornberg: deliberate mix of styles.
5. Cornberg's main visual strength: strong exploration landmarks.
6. Environmental density: moderate detail with clear movement paths.
7. Player/enemy contrast: context-sensitive techniques.
8. Map changes: major visual redesigns allowed if gameplay continues to work.
9. Asset selection: balance visual quality, performance and editability.
10. Blender approach: create many original models, using licensed asset packs as inspiration or starting points when permitted.
11. Fantasy direction: wildly imaginative and unpredictable, with progression-aware restraint early on.
12. Graphics: scalable presets for different hardware.

## Cross-cutting visual rules
- Mixing realistic backgrounds with stylized fantasy characters, gear, NPCs, monsters or props is explicitly allowed; art-style contrast is consistent with the Diceworld lore.
- Do not demand a uniform realistic/stylized treatment, nor reject useful models merely because their style differs.
- Do not change authored combat paths, quest interactions, NavMesh accessibility, boss telegraphs, or progression merely to beautify an area without separate review.
- Asset license/redistribution, Unity URP compatibility, editable source geometry/rigging and performance need checking individually.
- Cornberg and Slime Dungeon are current first candidates; final visual scope and priorities are subject to later interview rounds.

## Interview continuation
Round 2: Cornberg architecture, landmarks, northern farmlands, eastern forest, transitions and navigability. Further rounds: Slime Dungeon, characters/gear, effects/lighting, technical/performance and implementation sequencing.

## Source
Direct user responses to visual-interview Round 1, October 9, 2026.

## Round 2 — Cornberg and surroundings (approved 2026-10-09)
13. Architecture: no unified style; each building may differ, intentionally.
14. Cornberg stays a small, cozy farming hamlet.
15. Central landmark: a distinctive well or fountain; prioritize upgrading the existing slightly magical healing well rather than inventing a competing landmark.
16. Surrounding landscape: no fixed preference.
17. Custom-build important structures in Blender, using licensed asset bases when appropriate.
18. Northern farms: realistic agricultural countryside.
19. Eastern Slime Forest: colorful, slightly magical atmosphere.
20. Slimes themselves are the only unusual features in early-game surroundings. Keep incidental magical scenery subtle; convey slight enchantment through color, lighting and the slimes rather than overt unrelated supernatural objects.
21. Roads and paths vary with location and traffic.
22. Prominent river, bridges and banks.
23. Terrain elevation varies by area.
24. Gradually combine NPC walking, professions, daily routines and ambient life.
25. Discovery mixes landmarks, roads, clues, NPC hints and hidden exploration, without obvious map markers everywhere.
26. Creative environmental storytelling encouraged; preserve established lore and quests.
27. Small visual changes tied to quest completion.

### Cornberg lore and gameplay constraints
- The existing village well is slightly magical, heals nearby living characters, and contributes to unusually large/healthy local crops (confirmed by Axel). Preserve existing healing/respawn functionality and enhance this established landmark, not replace its lore.
- Cornberg combat and healing-well rules are documented in docs/CORNBERG_COMBAT.md.
- Retain northern fields, crop slime quest, forest routes and their gameplay semantics when beautifying.
- Follow the approved Round 1 early-to-late visual escalation: beautiful starter scenery without endgame visual complexity.
- Round 3 will address Slime Dungeon environments, bosses, puzzle visuals and loot.

## Round 3 — Slime Dungeon visual direction (approved 2026-10-09)
28. Entry atmosphere: deceptively peaceful forest hiding secrets.
29. Compared with overworld: similar forest, but denser and more impressive; it should remain recognizable rather than a wholly unrelated biome.
30. Boundaries: deliberately mix dense trees, roots, vegetation, stone and other natural formations room by room.
31. Paths: natural tracks with rocks, plants and fallen branches; keep traversal/readability.
32. Mandatory Big Slime miniboss arena: simple natural woodland clearing, not an ornate boss stage.
33. Five-ring puzzle centerpiece: twisted tree with roots reaching toward the rings.
34. Puzzle rings: glowing circular markings integrated into grass and earth.
35. Completion: normal and five-blue-slime secret solutions have different visual intensities; secret should feel more dramatic without changing mechanical rules.
36. Normal top-hatted Slime Boss arena: majestic natural amphitheater formed by trees.
37. Main boss tone: comical but surprisingly dangerous; preserve characteristic top hat.
38. Secret Regent route/arena: dramatic royal slime domain.
39. Slime Regent: royal, majestic, visually distinct accessories.
40. Boss VFX: escalating/different approaches for miniboss, normal boss and Regent; telegraph readability remains authoritative.
41. Shared private reward scene: magical sanctuary outside normal space; preserve reusability and future per-dungeon decorative variants.
42. Major scenery/geometry redesign authorized IF dungeon mechanics, accessible routes, interactions, puzzle logic, staging, boss danger cues, secret route and reward/return lifecycle all remain correct.

### Implementation guardrails
- Respect canonical docs/SLIME_DUNGEON.md: mandatory Big Slime; five-ring tree puzzle; blue/green slime conditions and permanently blue southern trees; mutually exclusive normal boss or Regent; 15-second post-victory transition; no extra mechanics from cosmetic effects.
- Distinguish theatrical magic from gameplay signals: boss effects must not obscure Slime Slam targeting rings, Divide fragments, five capture rings or route gates.
- Early-game visual escalation still applies: miniboss clearing simple, main boss arena grander, hidden high-level Regent most elaborate.
- These are approved planning decisions, not an instruction to start redesigning the scenes during the interview.
