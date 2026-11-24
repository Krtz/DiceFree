# Dragon rewards and the World Two expedition

Implemented on `feat/verdant-wilds`, 2026-11-24. This continues the existing
Ancient Road Dragon finale into the saved full-size World Two scene.

## Level 55 dragon rewards

The four chest pieces are rare. The gloves and boots are uncommon. They have
stable item IDs, individual library-derived icons, source attribution to the
Ancient Road Dragon, ordinary bag tooltips and equipment support. Roles describe
the stat focus; these items have no class restriction.

| Item | Slot / focus | Attributes | Other bonuses |
| --- | --- | --- | --- |
| Wyrmfang Cuirass | Chest / AGI melee | AGI 24, STR 8, VIT 18 | P DEF 24, M DEF 16, attack speed 5%, basic damage +4 |
| Roostguard Bulwark | Chest / STR tank | STR 24, VIT 28, SPI 4 | P DEF 36, M DEF 20, regeneration 4 |
| Embermender Vestments | Chest / SPI healer | SPI 24, INT 10, VIT 18 | P DEF 14, M DEF 30, regeneration 4 |
| Jadescale Arcanist Robe | Chest / INT caster | INT 24, SPI 8, VIT 18 | P DEF 14, M DEF 30, regeneration 2 |
| Rooststalker's Grips | Hands / melee | AGI 12, STR 8, VIT 10 | P DEF 10, M DEF 8, attack speed 6%, basic damage +3 |
| Dragontrail Greaves | Feet / travel | All five attributes +6 | P DEF 10, M DEF 10, movement speed 3%, regeneration 1 |

The ordinary pool now contains eight items, including the existing signet and
pendant. The existing 5% rare Ancient Road Mantle branch is unchanged. Ordinary
rolls still offer one item or two distinct items through the existing treasury
choice flow. No dungeon reward, party eligibility, or encounter-entry rules were
changed.

## Main-story continuation

After turning in **The Ancient Road Dragon**, travel through the existing south
Ironpine pass and physical World Two transition. Meet **Expedition Warden Ivara**
inside Cinderwatch's southern gate, beside the green banner and supply cart.
All three quests have a minimum level of 35 and require the preceding quest.

| Quest | Objective | Reward |
| --- | --- | --- |
| A Foothold Beyond the Pines | Defeat 8 Ashscale spearhands or ridgebows; return to Ivara | 8,000 XP, 120 gold |
| The Two Watchfires | Work at the Basalt Needle and Ember Crown survey cases in either order; return to Ivara | 18,000 XP, 220 gold |
| An Unfinished Record | Deliver the report to Envoy Tamsin in Willowford and acknowledge her conversation | 10,000 XP, 150 gold |

Ashscale kills can credit the existing repeatable bounty at the same time. The
first quest uses the existing nearby-party kill credit. Each survey is a
three-second interaction; repeating one location cannot replace visiting both.
Distinct-site progress survives journal restoration. The cases use the existing
work-objective atlas integration and display a beacon while relevant. Tamsin has
a conversation quest marker. Her final reward requires acknowledging the actual
conversation, not merely approaching her.

Ivara's instructions identify the trail ends at both landmarks. Tamsin stands
north of Willowford's spring and east of the inn, beside an expedition banner.
Both NPCs and both cases have reachable interaction approaches. The navigation
bake also checks every existing arrival, settlement, Willowford and pip-trail
waypoint from Cinderwatch.

## Story limits

The dragon remains dead in the main narrative. The new chapter establishes the
Order's field expedition and gathers maps and observations. It does not explain
how the original DiceBound dragon appeared outside the Dice or invent a link
between the Ashscales and the slain dragon. Tamsin forwards the record to the
Verdant Council; the next investigation is still to be designed. The Trimery
clearing remains reserved for World Three.

## Asset-library use and reproduction

- Chest, glove and boot icons adapt meshes from the locally imported Polytope
  Studio modular armour library. Original jade inlays, horn details and role
  colours are rendered into standalone PNGs. The private source FBX is not a
  runtime dependency of these items and is not added to Git.
- Ivara and Tamsin reuse the already tracked Cornberg Guard and Herbalist FBXs.
- Supply carts, crates, barrels and lanterns reuse the already tracked
  `VisualBatch01` models. Banners and survey folios use small native shapes.
- No new third-party package is admitted. Exact paths and source hashes are in
  the validation receipt.

Editor authoring: `tools/authoring/DragonLootExpansionAuthoring.cs` and
`tools/authoring/WorldTwoExpeditionAuthoring.cs`. Copy these into a local Editor
folder with their existing project dependencies and invoke `Batch()` through
Unity CLI. The expedition authoring replaces only its own named scene group;
it does not run the obsolete small World Two expansion authoring.

Unity saves can reorder large scene documents. The optional
`tools/validation/NormalizeUnitySceneOrder.cjs` preserves baseline object order
while checking that every authored record and field value remains present.
Existing serialized default `retaliationOnly: 0` fields were emitted by Unity;
their effective AI behaviour is unchanged.

## Validation

The full 14-scene Windows development build succeeded with zero build errors
and one existing collision-prebake warning. The main checkout imported and
compiled successfully. Fifty gameplay checks passed in a disposable copied
save: all new equipment, real dragon waiting room/kill/treasury, seeded reward
branches, quest gates, actual Ashscale credit, both survey orders supported by
distinct-site progress, timed interactions, duplicate protection, conversation
completion, exact XP/gold payouts, and journal restoration.

The run deliberately surveyed Ember Crown first. A subsequent visual review
reset the survey quest in the disposable save; a separate actual restart passed
19 more checks, including saved item ownership/equipment, the restored active
survey, both completed conversations, and a final completed-chapter disk flush.
That run surveyed the Needle first. Direct gameplay-camera renders cover the two
NPC locations and both survey cases. These world-only renders exclude overlay
UI; they are not evidence of manual UI acceptance or multiplayer playtesting.

Evidence: [dragon-expedition-2026-11-24](validation/dragon-expedition-2026-11-24/README.md).
