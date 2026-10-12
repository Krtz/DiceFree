# Trimery — Village 2 design v0.1
Date: 2026-10-12
Status: Name, economy and major progression route confirmed by user. Environmental cause, size and precise layout still under design. **Do not implement new scenes yet.**

## Confirmed canon
- Settlement name: **Trimery** (replaces provisional Timberwatch/Mossford/Briarhold names).
- Trimery is a **lumber-production and lumber-trading village**. Its primary external market is the **Big City** later in World 1.
- The ordinary direct trade road from Trimery to the Big City is **blocked due to an as-yet-undecided event**. Exact cause must be decided with user, not invented in code or quests.
- Because of the blockage, the player **must proceed through Biome 3, the hostile humanoid quilboar region**, to reach the Big City by an alternative route.
- Later, a quest located in the **Big City** allows reopening the Trimery–city route and reconnecting its timber trade. Reopening can lead to world-state changes, but specific outcomes must be approved.
- Ironpine **quillrats** are animals and remain distinct from the humanoid **quilboars** in Biome 3.
- The user requests asset-first environment/NPC art: adapt existing downloaded medieval building, cabin, timber, wagons and market packs before generating original assets.

## Existing map and gameplay constraints
- Existing Ironpine road ends at an approach clearing near **X223/Z149**, with an Alpha Wolf roaming ground close to **X212/Z143**. Village services and revive stone must not be exposed to Alpha aggro.
- MSQ Runner quest from Cornberg has not yet implemented `area.world1.next-settlement-arrival`. A settlement arrival event can close that journey once the user approves the NPC/story.
- A Dice revive stone in Trimery should become the last activated home point for the universal Return Home skill, with its own stable anchor ID.
- Existing NPC/service systems should be reused: banker, general goods, supplies/crafts, NPC interactions and navigation.
- World progression: Cornberg → Ironpine → Trimery → forced Biome 3 quilboar route → Big City → city quest → restored direct Trimery trade road.
- Direct trade road and quilboar detour should both be **visibly identifiable at Trimery**. The blocked road must look like a meaningful physical obstruction, not an invisible game-wall.

## Proposed spatial first pass (not yet approved)
- **Village approach:** southern / southwestern road arrival from Ironpine with readable signage and safe separation from the Alpha.
- **Lumberworks:** a working sawmill, timber stacks, loading yard, wagons with lumber and loggers at work. Give Trimery a visual profession, not just generic medieval houses.
- **Village center:** one notable lumber merchant/trade overseer, Dice revive stone, inn or gathering house, banker/general vendor and forge/workshop.
- **Direct city highway:** east / northeast major timber-cart road, currently visually blocked. Leave enough space for reopening as a persistent future world-state change.
- **Detour to Biome 3:** a separate rough trail out of Trimery heading into quilboar territory and eventually the Big City.
- **Optional flavor:** empty timber wagons in storage, idle hired teamsters, anxious merchant ledger, missing shipments. These are proposals, not approved canon.

## Local asset pack shortlist from existing catalog
- `Medieval Village Building Kit` — 370 pack files, roofs/walls/wood construction.
- `Stylized Medieval Building Pack 72 Modular Meshes Low Poly` — 548 files incl URP prefab variants.
- `The Wood Cabin` — 265 files, LOD and timber lodge architecture.
- `Low-Poly Medieval Market` — 298 files, crates/supply stalls and market assets.
- `Medieval Containers Pack with Wagon` — 134 files, wagons/timber hauling props.

Select art by inspecting prefabs, material compatibility and distribution licenses before importing. Mix/modify styles as needed; preserve local licensed assets outside public repository.

## Next decisions from user
1. **Why is the direct highway blocked?** Landslide / ruined bridge / magical disturbance / creature threat / other. The blocked route should be consistent with reopening later via a quest in the Big City.
2. **Size and atmosphere:** small 5–8-building working lumber hamlet or a more substantial 10–15-building trade village? Is it safe inside a palisade or more open and rustic?
3. **Reopening payoff (later):** shortcut alone vs visibly restored trade flow, wagons/NPCs and new merchant stock. Do not assume this until decided.

Status of implementation: **design-only**. No changes to Unity scenes, village actors or quests.
