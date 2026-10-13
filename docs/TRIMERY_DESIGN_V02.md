# Trimery — Village 2 design v0.2 (road geography and DiceBound crossover)

> **Superseded mystery:** In [v0.3](TRIMERY_DESIGN_V03.md), the user confirmed the Ogre Roadwarden **deliberately destroyed the western highway on orders from an unknown issuer**. Do not use the rejected ancient-stone-reacts-to-Echo idea. v0.2 speculation about the Ogre merely guarding existing damage is historical.

Status: DESIGN DRAFT; do not build geometry, quests, or boss yet. Supersedes contradictory spatial ideas from `TRIMERY_DESIGN_V01.md`. This document distinguishes CONFIRMED user intent from an OPTIONAL STORY PITCH awaiting approval.

## Confirmed geography and identity

- **Trimery** is a lumber-producing/exporting village, a working frontier settlement with a believable **timber palisade and guarded entrance**. The exact building count, gate arrangements and local quests have not been approved.
- It has **three outward roads**, not two:
  1. **SOUTH** via Ironpine wolves to Cornberg (the player arrives here).
  2. **WEST** on the major timber-cart trade highway directly to the Big City. This road is physically unusable early in the game.
  3. **NORTH** through Biome 3's humanoid **quilboar** territory; this route eventually reaches the Big City and is the required initial path.
- The **BIG CITY** (not Trimery) is intended to form **rings/circles around an enormous central tree**, a key city landmark. Its internal geography/scale are for later planning.
- A later quest **in the Big City** will help reopen the blocked western road from Trimery, making a genuine world-state connection and shortening travel.
- The event which made the western road unusable has **not yet been decided**.
- Existing Ironpine approach near world X223/Z149 and the roaming Alpha near X212/Z143 remain constraints. Village safe areas and the Dice revive stone need plausible protection; avoid invisible collision walls.
- Follow asset-pack-first policy and maintain navigable roads and solid physical collision.

## Verified DiceBound crossover facts (actual source, NOT invented canon)

Repo `T:/TEMP/DiceBound/repo`, `runtime/js/board/registry.js` and `runtime/js/combat/enemies.js`:
- Board 1 is **Green Road**, its tile-50 miniboss is **Ogre Roadwarden**. Its listed special is **Roadwarden Rampage**; weakness is Metal.
- `docs/STORY_BIBLE.md` section 15 (explicitly **WORKING THEORY**) says Roadwarden's title matters; it appears to have **authority over the Road**, and some ancient stones recognize that authority. The possibility of a forgotten administration is unresolved story lore.
- `docs/CLASS_LORE.md` (working theory) connects defeating that miniboss to learning that **rules of the Road can be broken** and unlocking Sorcerer.
- `docs/ENCOUNTER_IDENTITY_IDEAS.md` (proposal, not shipped mechanics) explores a Roadwarden who demands a **Toll of Passage**, then executes a **Roadblock** special if refused.
- DiceFree `docs/GAME_VISION.md`: player is an **Echo** of the survivor of DiceBound after **The Last Equation**, exploring six outer faces of the Die. Exact relationship between in-Die guardians and outer-world creatures is NOT established.

## Optional story pitch: "The Toll That Broke the Road"

**Status: suggested narrative, needs user decision.**
- A busy timber convoy on Trimery's WEST road encountered an enormous ogre declaring itself the **Roadwarden** and demanding an inexplicable toll. A refusal, dispute or mistaken challenge was followed by a violent flash/rumble.
- Instead of a mundane broken bridge or landslide needing fictional river/mountains, a **localized physically broken section of old stone roadway** was left: jagged earth, displaced cobbles, cracked ancient boundary stones, and a strange impossible-looking seam. It is a real traversability obstacle with clear sightlines and fencing, **not an invisible barrier**.
- Some residents claim the ogre smashed the road; others insist the Ogre arrived *after* the damage and is guarding something on the other side. The mystery can unfold without explaining DiceBound to new players.
- Trimery has paused timber shipments. Loaded carts and frustrated teamsters are near its western palisade gate. Guards deter people from entering the broken road, but it should still be physically explorable as far as safe.
- The player instead travels NORTH through the quilboars, eventually reaches the Big City (around the giant tree), and receives a quest involving workers/engineers and potentially a scholar familiar with ancient road markers.
- The road reopens later through a **tangible change** (repair work, route reconnected, guarded wagons returning). If the Roadwarden is a physical boss, it can be confronted on the city-side of the collapse; killing it alone must not automatically repair broken ground.
- Subtle callbacks for DiceBound players: Ogre design resembling the Board-1 miniboss; a toll demand; a heavy **Roadblock** telegraph; one weathered stone bearing a strange **50**; ancient stone responding strangely to an Echo. Avoid explicit Board 1 exposition for new players.
- Foreshadow World 2 without committing its future antagonist: the road may have been **unmade or disconnected** rather than merely blasted apart, implying the laws governing DiceBound's Road may persist in the outer world. Whether the Roadwarden caused, responded to, or attempted to contain the phenomenon should remain unanswered initially.
- Important lore restraint: DiceBound's working theories are not yet canonical answers; preserve mystery instead of declaring the exact Ogre from inside the Die magically escaped into the outer world.

## Layout sketch — still a proposal

- South gate faces the Ironpine approach and is defended against roaming Alpha/wolves.
- West **wide cart gate** faces the Big City, with lumber yards, sawmill/storehouse, caravan office and stopped wagons nearby. The damaged western road is a visible local landmark outside or beyond the gate.
- North guarded frontier gate begins the quilboar trail (forest warning signs, scout tower and a more dangerous departure).
- Center: ordinary family homes, inn/communal hearth, general goods, banker and **Dice revive stone**, with plausible movement from any of the three gates. Palisade layout must respect click-to-move/NavMesh dimensions.
- Main sawmill may sit inside village if scale allows, or in a nearby fenced working annex with appropriate guard protection; choose based on art/collision budget and town scale.

## Open decisions to ask user next

1. Should Ogre Roadwarden be **physically seen** on the blocked WEST road already, or should villagers speak of an Ogre and the player not meet it until later from the city side?
2. Was the road destroyed by the Ogre's rampage, **or** did the Ogre appear to enforce/guard a mysterious broken "rule of passage" after a magical rupture? These create very different World 2 mysteries.
3. Does reopening require confronting the Roadwarden **plus** repairing the path, or is the Ogre an optional/later special encounter?
4. How large and busy should Trimery feel? e.g. ~10-15 structures, a sizeable timber yard and three guarded gates is a **proposal**, not a user-approved population/scale.

No gameplay/Unity scene implementation is authorized by this ideation draft. Preserve existing feature branch; never merge to main without approval.