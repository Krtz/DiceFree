# Village 2 — first planning round (2026-10-12)

**Status: Village 2 design STARTED; no authored buildings, NPCs, or quests yet.** Waiting for player choice on the village identity before committing geography or importing packs. Previous Cornberg and Ironpine map remain unchanged.

## What exists and is settled

- Reserved World 1 location: village approach clearing at approximately **X223, Z149**, northeast of the Ironpine winding road. The last authored path segment comes from **X208,Z130**. `docs/IRONPINE_FOREST_IMPLEMENTATION.md` and `docs/WORLD1_VILLAGE2_TIER2_WOLF_DUNGEON_INTERVIEW.md` document the existing approach.
- Cornberg's Runner main-story quest expects arrival at the next settlement; **the arrival trigger is not authored**. Village 2 needs an arrival landmark / NPC and a durable one-time `area.world1.next-settlement-arrival` trigger.
- The player has a newly implemented universal 10-second Return Home skill; Village 2 should contain an **authored Dice revive stone** with a unique stable ID, for saving as the last activated stone.
- Ironpine stays dangerous-but-traversable with 10 quillrats, 7 Grey Wolves, 4 Dark Wolves and 1 roaming Alpha (L12–20). The Alpha's fourth hunting ground is near **X212,Z143**, only about 13m from the planned village approach! Its aggro radius is 15m and roaming corridor 45m. This is the single biggest local layout consideration. Safe village planning must **not** allow the Alpha to aggro/breach the shops, bank or revive stone; choose a fortified gate/safe boundary, or shift village into a larger protected clearing farther NE and re-plan that hunting ground.
- **Humanoid quilboars are beyond Village 2**; the Ironpine quillrats are only animals, and should not be recast as humanoid tribal enemies.

## Asset-pack-first shortlist (actual local catalog)

- **Medieval Village Building Kit**, 370 files: wall/roof/door/window support modules, thatch roofs, wood beams, chimneys. Good for an authored frontier settlement, with asset editing/URP conversion and consistent snap grid.
- **Stylized Medieval Building Pack 72 Modular Meshes Low Poly**, 548 files: includes BIRP, **URP**, HDRP variants and modular walls, doors, roofs. Good for faster URP-compatible blockout; recolor/rework exaggerated detail in Blender where needed.
- **Low-Poly Medieval Market**, 298 files: food stalls, weapons, crates and themed market sections for a real trading corner rather than empty frontage.
- **The Wood Cabin**, 265 files: cabin kit with LODs, good for a prominent hunter/logging lodge.
- **Medieval Containers Pack with Wagon**, 134 files: wagon, chests/crates and transport props. Old Standard/SpeedTree material baggage needs selective import instead of wholesale import.
- NPC models: **Low Poly Medieval Peasants - Free**, **FREE - Modular Character - Fantasy RPG Human Male**, current DiceFree farmers/bankers/merchants as fallback; check licenses and animations before importing.

User decision: pack assets are first choice even if lower poly or a different style; edit them, mix with realistic Wolves if useful. Do not publish raw Asset Store assets to public GitHub.

## Candidate Village 2 identities — not yet chosen

A. **Timberwatch**, hunters and loggers: large timber lodge, sawmill/carts, timber houses, wolf warning watchposts, craft materials and a ranger/hunter connection. Forest hardship but cozy safe rest point.

B. **Mossford**, road-and-river traders: wagon road, market, inn, little river crossing, cargo storage, hints of deeper routes and commerce. Requires authoring water/river we haven't designed.

C. **Briarhold**, fortified border settlement: palisade, gates, guard training, blacksmith and watch platform. A stronger reason for safety near Alpha roaming, military quests and early encounters with humanoid quilboars beyond town.

D. A blend or an entirely different settlement idea.

## Proposed logical structure (neutral to identity, not final coordinates)

1. **Arrival approach** from the Ironpine road with sightlines, bypass options and a clear boundary between wildlife and village safety. Avoid invisible barriers.
2. **Arrival marker and NPC** to close Cornberg Runner's journey, giving the player a reason to explore the village. Implement quest state transition only after agreeing on the story and NPC.
3. **Village center / Dice revive stone** so a new teleport home point is available. At least one safe path from the entrance to the stone. Do not allow hostile Alpha aggro to persist within village services.
4. **Market/repair/craft corner** and **bank/general goods NPCs**, plus ordinary villagers with modest idle animations. Services can reuse framework from Cornberg.
5. **Far road departure** to next level region (humanoid quilboars), with a strong visual landmark. Do not author the next region yet.
6. **One distinctive building/landmark** that visually differentiates Village 2 from Cornberg (lodge, inn/market or timber palisade). Optional local sidequest leads toward wolf wave-survival dungeon, but don't implement that dungeon before choosing its format.
7. Keep village navigable via existing click-to-move/WASD NavMesh without trapping players between decorative colliders.

## Implementation phases once theme and safety are approved

- **V2-01**: scene clearing, ground and road, authored safe boundary, walkable NavMesh, camera/fog and arrival waypoint. Validate uninterrupted Cornberg→Ironpine→Village 2 travel.
- **V2-02**: choose and adapt licensed architecture assets (URP conversion, LOD, material variants, sensible collision). Make at least 5 distinct structures and landmark without overwhelming performance.
- **V2-03**: revise NPC placements, revive stone ID, basic bank/merchant services, save/reload check and full co-op-ready interactions.
- **V2-04**: close the existing Runner quest with first-arrival logic and begin a small village story that naturally points toward the next enemy zone and wolf dungeon. Do not invent quests without the player's consent.
- **V2-05**: polish ambient audio/animations/signposts, terrain and quest readability; real Game view playtest.

## First required user choices

1. **Identity**: Timberwatch hunters/loggers (A), Mossford traders (B), Briarhold palisade/guards (C), or blend/other (D)?
2. **Safety**: a fully safe settlement separated from Alpha/wolves, or a vulnerable frontier with occasional village-edge assaults but still safe services? The proximity of Alpha ground X212/Z143 makes this consequential.
3. **Village scale**: tiny 5–8-building hamlet vs medium 10–15-building village? Either should feel distinct from Cornberg.

All details beyond the listed settled foundations remain proposed until user answers. **No Village 2 scene changes in this pass.**
