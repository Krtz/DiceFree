# DiceFree — Epic Visual Master Plan

Status: MASTER DIRECTION / implementation sequencing, 2026-10-09.
Decision owner: Axel. Based on the approved 90-question visual interview, current design documents, and the new class/gear visual philosophy. This plan organizes work; it does not authorize an unreviewed merge to main.
Related authoritative documents: VISUAL_ART_DIRECTION.md; VISUAL_OVERHAUL_ROADMAP.md; ART_PIPELINE.md; CLASS_PROGRESSION.md; SLIME_DUNGEON.md; CORNBERG_COMBAT.md; GAME_VISION.md.

## The North Star

DiceFree is a genuine, playable, isometric co-op fantasy RPG. An Echo begins small: a humble Novice in a cozy farming region. Across branching advancements, discoveries, six faces of the world, extraordinary encounters and distinctive gear, the Echo grows into something legendary. Visual intensity rises WITH progression, rather than treating starter content like endgame. Beauty at level 1 is essential; excessive spectacle is not.

Every major design choice should pass four questions:
1. Will a player remember this class, character, location or encounter?
2. Is its silhouette, purpose and gameplay communication clear at the actual isometric camera?
3. Does it look polished in the running game, on both low and high settings?
4. Does it preserve gameplay, navigation, interactions, saves, co-op and performance?

Target feeling: Warcraft III / Twilight's Eve ORPG's iconic class fantasy and progression, with modern authoring quality, tactile animation, clear FFXIV-like boss telegraphs and a distinct Diceworld identity. References are mood targets, not models or art to copy.

## Decisions — locked visual philosophy

### 1. Class identity is the protagonist
- A class must be recognizable at gameplay zoom WITHOUT nameplate, skill effects or equipment tooltip.
- Each playable class eventually has its own distinct model/presentation. Shared technical rigs, skeleton conventions and VFX modules are allowed; identical-looking classes are not.
- Every advancement creates a meaningfully different silhouette, posture, animation vocabulary, weapon handling and effects language. Complexity and ornament rise at higher tiers.
- Current three-class art priority is simultaneous: Novice, Physically Blessed Novice, Magically Touched Novice.
- The level-1 Novice looks like a capable but ordinary local person. The physically blessed form communicates embodied strength and momentum; the magically touched form communicates arcane potential and casting discipline. These are initial art briefs, not new gameplay mechanics.
- Future archetypes (e.g., Ranger, Wizard, Berserker) require class-specific briefs when their gameplay identity is finalized, not generic recolors of the three current presentations.
- Background NPCs may share modular models; named narrative and advancement-mentor NPCs deserve authored character designs scaled to their significance.

### 2. Gear visibility — selective, not all-or-nothing
Do NOT require a distinct mesh for every small upgrade, and do NOT postpone all visual rewards until the last 1% of content.

Visibility priority:
- Always-important identity: main weapon, offhand/shield when mechanically relevant, and class model/silhouette.
- Selectively visible as art improves: headwear, major chest/shoulder silhouettes, back/cape pieces, some boots/gloves and authored sets.
- Normally subtle/invisible: ordinary rings, amulets and minor numeric upgrades, unless a particular item's identity warrants a visual exception.
- Special named items can have unique meshes, animations or transformations regardless of rarity. Legendary/mythic rarity alone is NOT a blanket glow rule.
- The highest-tier items should be rare visual EVENTS: signature weapon transformations, striking equipment forms, animated ornament or VFX, not indiscriminate bloom.
- Transmog and player-chosen appearance win over literal stat-item mirroring. No dyes are assumed. Preserve existing equipment slots, valid gear types per class, save data and transmog rules.

Progression ladder:
- Novice / starter: sturdy believable clothes, simple readable weapons and minimal magical ornament.
- First branch (Novice level 10 to Tier I level 1): immediately noticeable change in body language and class-defining silhouette.
- Later branches (Tier I level 30, Tier II level 60, Tier III level 120, Tier IV level 200 advancement thresholds): rising model detail and class identity; dramatic gear, spellwork and signature silhouettes at higher tiers.
- Secret unlocks and the level-200 Novice can break the expected ladder intentionally. Advancement remains a branching manifestation, not an irreversible cosmetic overwrite.

### 3. World identity, not asset quantity
- Build visually distinct places the player navigates and remembers: the original Cornberg healing well, readable homes and services, northern farmland, bridges/riverbanks, eastern Slime Forest, dungeon entrances, puzzle landmarks and memorable boss arenas.
- Early Cornberg remains a grounded, cozy hamlet with intentionally varied buildings; unusual slimes and the established healing well are the restrained supernatural accents.
- Landscapes may contain realistic or stylized assets alongside exaggerated fantasy characters. The mix is permitted; intentional composition, material balance and lighting unify a location.
- A region is COMPLETE only when whole playable routes look good, not because a prop directory contains hundreds of files.
- Six world faces should have distinct signature shapes, colors, landmarks, enemy families and atmospheric treatment, designed progressively as their gameplay is known.

## Production priorities and milestone gates

### VIS-00 — Resolve production blockers and create honest visual baselines
Current assets and scene data are not treated as a final art pass. Keep the previously approved art-direction decisions.
Deliver:
- Unity startup/Package Manager/remote control reliable on repeat launches.
- Stable Editor screenshot captures from named camera points: village well, bridge, house fronts, northern farms, eastern forest, dungeon, three current classes; baseline gameplay footage where necessary.
- Track real model scale, coordinate/rotation conventions, ground snap, river/water masks, world-space building mesh bounds and imported prefab child orientation.
- Asset inventory with source, license, Blender file, FBX, material mappings, Unity GUIDs, transform origin and status.
- Baseline performance and current navigation/quest/ability regression checks.
Gate: capture and scene-inspection tooling work repeatedly; no unverified world placement. Do not merge without owner approval.

### VIS-01 — One convincing, polished VERTICAL SLICE of all visual workstreams
Produce small finished pieces, not 100 unfinished props:
1. Central Cornberg landmark: visually finished healing well, correctly oriented roof and existing functional healing area unchanged; building composition and grass/stone/road framing.
2. Nature slice: a navigable section of starter forest with 3+ visually distinct polished tree silhouettes, bushes/roots/groundcover, river edge and meaningful tree size variation.
3. Class slice: gameplay camera lineup of ALL three current class presentations, one polished idle, walk and attack/cast behavior each, signature class silhouette.
4. Dungeon slice: woodland puzzle-tree/five-ring sample and a readable slime-boss stage or entrance sample.
5. Render slice: consistent lighting/material treatment and working low/high scalability; telegraphs remain clear.
Gate: side-by-side same-camera captures, short gameplay video, low/high views, gameplay regressions, tracked Blender sources, owner approval. This milestone proves the art direction.

### VIS-02 — Starter region nature and Cornberg production pass
- REPLACE placeholder tree visuals, rather than blindly ADD trees. Inventory existing tree GameObjects/positions first.
- Retain gameplay roots, navmesh, colliders, occlusion/interaction behavior and scripted references. Swap only visual children or documented prefab presentation; no tree should move just because its model changed.
- Create an approved biome set: multiple deciduous trees, several conifers, mature/young variants, distinct canopy density and seasonal/color variants where biome-appropriate. Deterministic variation from stable tree IDs; do not randomly re-roll every scene load.
- Correct local scale, pivots, normals, FBX axes, trunk grounding and shadow footprint; specifically test branches over paths and camera occlusion.
- Whole existing starter-region tree inventory gets replaced IN STAGES: village core -> road/bridge -> farms -> eastern forest -> perimeter. Preserve legible trails and avoid water, routes and quest interaction areas.
- Build architectural identity: banker/shop/alchemist/blacksmith/farmers through actual building bounds and meaningful entrance placement, not free-standing trim in front of imaginary facades.
- Polish river surfaces, bank transitions, roads, fields and the healing-well storytelling; create several polished walkable viewpoints.
Gate: original and upgraded tree counts reconciled, no newly obstructed path, no trees in water, no floating objects, consistent day/night materials, performance within agreed budget, full Cornberg playthrough.

### VIS-03 — Slime Forest, Dungeon and royal secret
- Preserve recognizable forest identity while making the dungeon denser, strange and increasingly impressive.
- Respect existing five-ring tree puzzle, ordinary/secret triggers, blue slime/blue-tree clues, miniboss, boss mechanics and reward sanctuary.
- Normal route: peaceful entrance -> rooted puzzle landmark -> simple Big Slime clearing -> natural amphitheater with comical top-hatted boss.
- Secret route: distinct transformation and a genuinely royal Slime Regent domain, earned by discovery rather than casual visual noise.
- Unique slime animation language: transparent, squishy, personality-rich and inspired by DiceBound without mechanically altering attacks.
Gate: solo and party testing; boss mechanic/telegraph readability on all settings; both dungeon routes and reward flow verified.

### VIS-04 — Classes, NPCs, animation and gear polish
Begin class prototyping during VIS-01, then invest in production-grade character work here:
- Three current playable class-specific presentations, each with identity sheet, orthographic/isometric silhouettes, tested scale, rig, texture/material set and appropriate hitbox.
- Animation minimum for each: breathing/secondary idle, locomotion, stop/start, basic attack, cast where applicable, hit/reaction, defeat/death, and advancement entrance/reveal.
- Separate class-specific poses, center of mass, animation timing, spell shapes and distinctive weapon/cast gestures; visually test transitions while retaining mechanical responsiveness.
- NPCs: proper walking/idle for all normal village actors; service NPCs remain at their post and animate in place; important NPCs receive additional gestures; preserve talk/quest targeting.
- Equipment: first weapon and shield, class gear silhouette pieces, significant named gear, and transmog cross-class and save/load checks.
- Character art must be previewed close-up AND at standard playable camera distance. Use animation previews plus real combat footage, not editor mesh renders alone.
Gate: no root-motion gameplay drift, no broken hands/gear sockets, readable character identity at gameplay scale, actual NPC walking/casting and class interactions tested.

### VIS-05 — Cinematic world with scalable rendering
- Distinct biome/region lighting, gentle day/night and region-authorized fog/weather.
- Light but appealing water, foliage sway, particles and subtle ambient life.
- Graphical presets: Low / Medium / High / Ultra plus advanced settings for shadows, VFX density, water reflections, AO/bloom, foliage, fog/weather and minimal effects.
- Performance before ornament: keep main-thread CPU, VRAM, draw calls, overdraw and GPU frame times under measured budgets for target hardware, adjusted from a real baseline.
- Boss telegraph clarity always wins over decorative weather or effects. Low settings must not delete danger information.
Gate: repeatable screenshot comparison on low/high; recorded FPS/frame timings on agreed machine(s); visual accessibility and settings persistence verified.

### VIS-06 — Six-region visual escalation and iconic later classes
- Author later-region identity sheets progressively when encounters/quests are ready; do not populate empty regions with speculative high-tier assets first.
- Design class mentors, secret unlock presentation, authored named gear/sets and monstrous family evolution.
- Build the capstone visuals AFTER the core game's visual standard is proven, preserving a gap in spectacle between a humble Novice and endgame legendary forms.

## Immediate next actionable sprint (small and testable)

1. Capture repeatable baseline at the village well, bridge, forest edge, farms AND one character-camera view; verify Editor automation stability after the Package Manager repair.
2. Inspect existing tree roots, meshes, scripts, bounds and material slots. Choose ONE short, safe forest road segment and swap existing tree visual children with 3-5 polished variants, not a scatter batch. No changes to obstacle roots or NavMesh.
3. On the same sprint, make first side-by-side silhouettes and animation clips for Novice, Physical and Magical forms; match visuals to existing working rigs without replacing gameplay character logic.
4. Correct the Blender healing-well export's orientation in isolation before proposing scene installation. Keep healing script and original object intact.
5. Review the actual game images/video and fix wrong rotations, water overlap, floating models or occlusion BEFORE accepting. Then expand segment by segment.

Priority: a few beautiful, functioning, tested assets > another 60 uninspected exports.
Known caution: the earlier 59 blindly placed props and upside-down custom well were removed; nine small curated props remained in the scene at the last visually verified pass. Treat that as a lesson, not a target to outnumber.

## Repeatable art-to-game contract

Each deliverable must carry:
1. **Design intention** — location/class identity, concept and asset role.
2. **Provenance** — original Blender .blend or verified reusable license and modified source, export manifest, Unity material/prefab metadata.
3. **Placement authority** — existing gameplay root, geometry bounds, ground/water checks, authored local placement, collision/NavMesh and occlusion policy.
4. **Presentation** — correct materials and normals in URP, default and low-quality views, named camera screenshot before/after, short animation demo if animated.
5. **Regression evidence** — compile, scene structure, interactions, navigation, quests, saves, class gear, and co-op as applicable.
6. **User review** — milestone preview and explicit approval prior to any merge into main.

Stop/rollback on ANY unexplained upside-down mesh, pink/unlit material, water clipping, new invisible obstruction, damaged NPC/quest access or unreadable telegraph. A green scene-validation marker is not proof of visual quality.

## Workstream tracker (operational)

A. World Art: tree replacement, terrain/river, fields, village, atmosphere.
B. Character Art: Novice/Physical/Magical, future models, important NPCs.
C. Animation & VFX: locomotion/combat, slimes, foliage, casts, telegraphs.
D. RPG Visual Rewards: weapons, named gear, transmog and authored transformations.
E. Rendering & Tools: URP materials, import/rig tooling, captures, platform/performance presets.
F. QA & Integration: route preservation, screenshot review, mechanics regression, merge gates.

Umbrella planning issues already exist: #57 master, #58 VIS-00, #59 VIS-01, #60 Cornberg, #61 Dungeon, #62 classes, #63 rendering, #64 future mentors. Add focused implementation/checklist issues beneath these rather than duplicate the roadmap. Existing #37-56 remain authoritative for their overlapping implementation scope.

## Success criteria — what 'epic' really means

A new player should be able to:
- recognize the three current classes at a glance;
- feel the first advancement visually and mechanically;
- remember Cornberg, its well, its forest road and its slime dungeon from their silhouettes and mood;
- spot a genuinely exceptional item and want to earn it;
- read danger and control movement even with all optional effects reduced;
- play without collision regressions, broken quests, unreadable NPCs or major FPS collapse.

We are not finished when the Blender library is huge. We are finished when the actual RPG feels coherent, alive, readable, rewarding and increasingly legendary.
