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

## Round 4 — Characters, monsters, equipment & Blender (approved 2026-10-09)
43. Character proportions and visual styles vary by class and race; do not enforce one shared body proportion.
44. Level-1 Novice looks like an ordinary villager wearing simple clothing.
45. Every advancement should establish a noticeable new visual identity, with escalating spectacle in later tiers.
46. Every class eventually gets its **own distinct model**; reuse of underlying rigs/technical components is acceptable only when the resulting models remain distinct.
47. Playable races may be unusual creative reinterpretations when suitable, while preserving identity and clarity.
48. Equipment styles can mix, with a clear increase in complexity/spectacle as progression advances.
49. Actual item identity matters more than rarity alone; do not make every rare item follow an identical glow/color rule.
50. Some legendary/endgame equipment can transform visually under authored conditions, including animated forms or effects.
51. Appearance customization/transmog has higher priority than strict visual mirroring of every currently equipped item. Preserve functional gear/stats and existing appearance systems.
52. Most background NPCs may use generic models; important NPCs deserve individual models.
53. Important NPC art (custom model, distinctive clothing/equipment, animation, visual history) scales with narrative significance.
54. Low-level monster art direction varies by monster family.
55. Higher-level monster evolution varies by family: size, features, accessories, mutations, effects and animations can be used selectively.
56. Blender workflow is flexible: original meshes, kitbashing and modifying permitted licensed assets are all valid.
57. First character art pass should improve **all three currently playable character presentations** (Novice, Physically Blessed Novice and Magically Touched Novice), not Novice alone.

### Future class-representative NPCs — approved vision
- Later, aim for **one recognizable NPC representative per playable class**, visually embodying that class.
- These NPCs are the expected principal quest-givers/mentors for advancement into the corresponding classes.
- Exceptions are explicitly permitted; secret classes may use entirely different discovery or advancement routes and should not be forced into the standard NPC pattern.
- This is a future narrative, class-content, quest and character-art goal; do not retroactively add all representatives to Cornberg or mechanically change existing advancement without separate planning.
- Maintain prior progression rule: humble first-tier appearance, distinctive presentation at every advancement, increasingly epic later tiers.

## Round 5 — Lighting, weather, animation, VFX and graphics (approved 2026-10-09)
58. Cinematic lighting that can change dramatically by location.
59. A slower, atmospheric day/night cycle.
60. Weather authored specifically for each region rather than assuming identical global weather.
61. Lighting transitions vary with location and story importance.
62. Fog treatment is biome-specific.
63. Environmental animation favors subtle foliage sway and water movement.
64. High-quality, unique animations for each playable class.
65. Combat animation should be fluid, flashy and cinematic while preserving control responsiveness.
66. Spell effects combine tier progression, elemental identity and class-specific visual language, always subject to combat readability.
67. **Default boss-danger communication: extremely clear FFXIV-style ground telegraphs.** Specific bosses or harder content may intentionally change, shorten or otherwise adapt telegraphs when separately authored; these are deliberate mechanics, not an excuse for unreadable baseline warnings.
68. Screen-space effects must be configurable, with an explicitly minimal-effects mode.
69. Water should be high-quality in appearance while remaining lightweight for isometric gameplay.
70. Graphics menu: named presets PLUS individually adjustable advanced settings.
71. First animation/VFX priority: upgrade all three currently playable class presentations (Novice, Physically Blessed Novice, Magically Touched Novice).
72. Visual overhaul can span multiple substantial milestones; do not arbitrarily constrain ambition to a quick polish pass.

### Technical/planning consequences
- Cinematic art direction must accommodate a scalable render-cost profile; design effects for graceful reduction on lower settings.
- Keep gameplay telegraphs distinct from decorative VFX and retain visibility under fog, weather, nighttime, high spell density and minimum postprocessing.
- Slow day/night progression and region-authored weather remain planned features; not claims that they already exist.
- Unique animation per class is an end-state art requirement; reusable rigs/animation infrastructure remain permitted.
- Interview Round 6 covers asset selection, production sequencing, review and source-control workflow.

## Round 6 — Asset selection, production workflow and approval (approved 2026-10-09)
73. Decide asset reuse versus Blender modeling case by case based on quality, effort and editability.
74. Freely mix assets and art styles across packs; visual contrasts are part of Diceworld lore.
75. Extensive freedom to remodel, combine, recolor and animate licensed models.
76. Give balanced initial attention to playable class models, important Cornberg structures/well, dungeon bosses/puzzle tree and special equipment.
77. Evaluate each existing handmade Blender asset on its individual merits; neither automatic retention nor automatic replacement.
78. Keep source/original third-party assets separately; put adapted DiceFree versions in the project with traceability and licenses.
79. Review with screenshots, occasional playable checks and milestone demonstrations.
80. Within approved design, art decisions may be made independently without case-by-case permission.
81. Small landmark relocations permitted if existing functionality and navigation are verified.
82. First milestone should provide a small, polished representative sample of every area/workstream rather than finish one area first.
83. Afterward prioritize the changes with the biggest visual impact.
84. Evolve the existing Blender/Unity art pipeline incrementally, not build a new pipeline before visible work.
85. Finish each committed asset to polished quality before moving on; avoid vast inventories of unfinished placeholders.
86. Improve animation systems as needed to achieve high-quality individual class animation.
87. Make graphics preset scalability a first-class requirement from day one.
88. Roadmap should combine umbrella milestone, focused area/character issues and separate environment, character, animation and rendering workstreams.
89. Merge into main only after each milestone is validated AND personally approved by Axel.
90. Success means balanced, convincing improvement: strong screenshots, production-quality Cornberg/dungeon, distinctive classes and monsters, and a lively lit/animated world.

### Production governance
- Planning phase ONLY until user separately authorizes implementation. Creating docs and GitHub planning issues is approved, but importing/modifying scene content is not.
- Use milestone gates: scope -> curate with license/compliance -> small finished sample -> screenshot/playtest -> automated regressions -> explicit approval -> merge.
- Never claim remote or manual visual tests were performed when only data/automated tests ran.
- Preserve source files, manifests, Blender files and modified Unity exports with clear provenance.
