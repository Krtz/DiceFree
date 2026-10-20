# Regions

DiceFree is planned around **six major overworld regions**, corresponding to the six outer faces of the Dice.

## NEW WORLD CONCEPT — opposite pip faces, contrasting themes (2026-10-21; PROVISIONAL)

**USER-BRAINSTORMED AND WANTED RECORDED BEFORE SLEEP:** DiceFree's opposite dice faces should express **OPPOSITE OR STRONGLY CONTRASTING environmental themes**. On a normal d6 the opposite faces sum to seven: **1 ↔ 6, 2 ↔ 5, and 3 ↔ 4**. The **opposition rule is the appealing design direction**; the six specific climates/creature populations beyond established Face 1 are **provisional**, NOT locked, and this note is NOT authorization to design or implement new worlds now.

| Die face | Current PROVISIONAL core biome/atmosphere idea | Opposite pair contrast |
| --- | --- | --- |
| **Face 1 — one pip** | **Green, lush, vibrant and full of life**, in line with existing pastoral World 1/Cornberg/Great Tree plan | **Face 1 ↔ Face 6: life vs death** |
| **Face 6 — six pips** | **Dark, dreary, bleak, heavily populated with undead** | Death/decay versus thriving Face 1 |
| **Face 2 — two pips** | **Tropical/hot, potentially volcanic** (volcanoes, humid warm areas) | **Face 2 ↔ Face 5: heat vs cold** |
| **Face 5 — five pips** | **Cold, snowy, wintry/icy** | Snow/ice opposite tropical volcanoes |
| **Face 3 — three pips** | **Desert, arid and dry** | **Face 3 ↔ Face 4: dryness vs water** |
| **Face 4 — four pips** | **Sea/water-dominant, oceanic and swampy**, with wetlands/marshes | Water/swamps opposite dry desert |

**Do NOT mistake this as six finalized worlds or a fixed world order beyond existing progression.** All later-face themes, sub-biomes, lore, climate boundaries, monsters and visual assets remain undecided. The user especially liked **pairwise opposition as a general governing idea**, even if specific face assignments/biomes change when those regions are designed.

### Relationship to older DiceBound-derived visual references

This is a **NEWER conceptual preference** than the older `DICEBOUND_WORLD_VISUAL_REFERENCES.md` examples. The corresponding **DiceBound Board 1–6 image references remain useful and their board-to-face NUMERIC ASSOCIATION remains unchanged**, but the old image's landscape **must not be treated as a locked primary DiceFree face biome** when it conflicts with this NEW PROVISIONAL opposite-face idea.

In particular:
- Older **Face 2 / Astral Road** was misty rocky forest; newly brainstormed Face 2 is **tropical volcanic**.
- Older **Face 3 / Fractured Road** was ruined fortress/graveyard; newly brainstormed Face 3 is **dry desert**.
- Older **Face 4 / Crown Road** was cursed swamp, compatible with the newer **water-dominant/sea/swamp** direction.
- Older **Face 5 / Oblivion Ringroad** was **volcanic**; the user now suggests **snow/ice instead**, putting the tropical-volcanic climate on Face 2. **Do not quietly keep Face 5 predominantly volcanic while claiming the new opposition concept is followed.**
- Older **Face 6 / End of Mathematics** had ashen gothic citadel; the new **dark undead** theme can borrow gothic visual DNA without specifying finished undead factions.
- Face 1 remains **lush and green**, preserving the existing Cornberg, Great Tree/Yggdrasil landmark and already planned World 1 terrain and towns.

Retain existing `biome blending between faces` and interconnected cube-world rules unless later revised. Note also **opposite faces on a cube are NOT adjoining borders**; the opposition is at a world/theme level, not necessarily an adjacent geographical seam. **No actual DiceFree maps, scenes, terrain, art, quests or assets should change from this brainstorm.** Production priority remains World 1 → Quillboar area → major city before substantial planning of other faces.



## DiceBound visual inheritance

Each DiceFree face takes its visual/environmental DNA from the matching DiceBound Board screen background **and** Normal combat background.

See `docs/DICEBOUND_WORLD_VISUAL_REFERENCES.md`.

The **numeric Board-to-Face visual-reference association** remains fixed (this does **not** fix each face's final dominant climate; see the newer opposite-pip concept above):
- Face 1 <- Board 1 / Green Road / welcoming woodland;
- Face 2 <- Board 2 / Astral Road / misty rocky forest;
- Face 3 <- Board 3 / Fractured Road / desolate ruined-fortress/graveyard mood;
- Face 4 <- Board 4 / Crown Road / enchanted cursed swamp and forgotten ruins;
- Face 5 <- Board 5 / Oblivion Ringroad / volcanic mountain fortress;
- Face 6 <- Board 6 / End of Mathematics / emberlit ashen gothic citadel.

These are inspiration anchors, not literal 1:1 recreations.

## Biome blending between faces

A face's DiceBound-derived theme is a **dominant regional identity**, not the only biome allowed on that face.

Faces may contain multiple biomes.

Preferred world logic:
- the central/interior portions of a face express that face's strongest visual identity;
- terrain closer to an edge can gradually pick up environmental influence from the neighboring face;
- corners can blend influences from three faces and are allowed to become especially strange;
- transitions should feel like one continuous world rather than six themed maps touching at hard seams.

Examples:
- a green Face 1 forest near an edge bordering a colder/mistier face might become rockier, foggier and less pastoral;
- a border toward a volcanic face might become drier, ashier or geothermally active before the gravity transition itself.

This is a worldbuilding/design principle, not a demand for uniform gradient blending everywhere. Mountains, rivers, climate, magic and local geography can create sharper transitions where appropriate.

## d6 identity

The one-dot face is the starting region, then two-dot through six-dot in intended progression order.

Regions should contain distinct landmarks arranged so the map/minimap can evoke the corresponding d6 pip arrangement.

Exactly how literally each pip position maps to a city/mountain/lake/etc. is still open.

## What inhabitants believe

The inhabitants **do not generally understand that their world is a die**.

To them, this is simply what a world is like.

People know:
- other regions/faces exist;
- roads/trade routes can cross between them;
- gravity/orientation behaves as their world normally behaves.

They do **not** normally interpret the whole world as a manufactured gaming die.

A fun lore possibility is an NPC who insists the world is literally a die and is widely regarded as a crank/lunatic.

## Trade and politics

NPCs routinely travel between faces where routes permit.

There can be:
- trade;
- diplomacy;
- wars;
- alliances;
- migration;
- roads/passes crossing boundaries;
- political relationships spanning multiple faces.

The six regions should feel connected, not like six sealed dimensions.

## Progression

Enemy levels are fixed by zone; no player scaling.

Later/endgame content can be hidden in any face, including the starter face.

## Towns and camps

Towns provide full services.

Camps provide subsets.

## Fast travel

Travel-point to travel-point.

Not all connections need to exist; missing links should have lore/world reasons.

## Face crossing

The fantasy remains physical adjacency between faces, but implementation remains open between seamless local-gravity traversal and segmented/streamed transitions.

Authored geography may restrict crossings to selected roads/passes.

## Projectiles and effects

Preferred fantasy: projectiles and terrain-bound effects behave coherently across face transitions if the implementation supports it.

Prototype before committing to technically expensive global behaviour.

## Respawning

Normal monsters respawn on timers. Named/elites respawn more slowly.

## Open questions

- How to combine newer opposite-face biome contrasts with older DiceBound source-image visual DNA, without making later-world climates immutable now.
- Regional level bands.
- How literally landmarks map to pip positions.
- Corner treatment where three faces meet.
- Seamless vs segmented traversal.
- Day/night, sky and horizon rules.


## Production priority: build Face 1 first

Do **not** attempt to fully design all six faces in parallel.

The current world-design priority is:
1. define the one-dot starter face;
2. build a small starting area;
3. prove town/quest/combat/dungeon traversal there;
4. expand that face outward in authored chunks;
5. only then use lessons from Face 1 to shape later faces.

The other five faces can retain high-level thematic placeholders until World 1 is working.

## Incremental world expansion

World 1 should initially expose only a deliberately small playable area.

Undeveloped future territory can be blocked using believable world geometry such as:
- dense forests;
- mountains;
- collapsed roads;
- cliffs;
- rivers;
- walls/gates;
- dangerous terrain;
- landslides;
- construction;
- story-appropriate obstacles.

As development expands the region, those blockers can be removed/reworked to open new roads and subregions.

This is both a production strategy and a world-design rule:
- avoid invisible walls where natural blockers work;
- keep new expansions connected to the existing geography;
- let roads, sightlines and landmarks hint that the world continues beyond the current playable boundary.

## Face 1 goals

Before seriously designing Face 2, Face 1 should have a coherent answer for:
- starting arrival point;
- starter town;
- first resurrection point;
- first bank/vendor/healer/crafter services;
- first outdoor combat loop;
- first enemy families;
- first side quests;
- first main-quest guidance;
- level-10 advancement path into Physically Blessed Novice / Magically Touched Novice;
- first dungeon entrance;
- first boss;
- first meaningful loot-room reward;
- first secret/late-game tease;
- travel-point rules;
- visual landmark/pip identity;
- at least one future expansion boundary that can later be opened naturally.
