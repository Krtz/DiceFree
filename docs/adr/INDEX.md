# Architecture Decision Record Index

Updated 2026-10-08. ADRs capture decisions future implementations must respect; the design documents remain authoritative for content details, algorithms, numerical tuning, class data and ongoing ideation. This sweep reviewed the design-document domains and converted the broad stable contracts; do not rewrite every rule or coefficient as an ADR.

| ADR | Decision |
| --- | --- |
| 0001 | Repository and build distribution |
| 0002 | Runtime modules and assembly definitions |
| 0003 | 3D source/export pipeline |
| 0004 | Class forms and equipment compatibility |
| 0005 | Manifestation roster and load selection |
| 0006 | Separate cube faces and one occupied session per dungeon ID |
| 0007 | Separate level-10 Novice mountain advancement map |
| 0008 | Southwest Cornberg, expanded northeast starter forest |
| 0009 | Character-first fantasy style; realistic scenery allowed |
| 0010 | Generic NPC role kit; selective important uniques |
| 0011 | Transparent DiceBound-inspired slime tier visuals |
| 0012 | First Slime Boss scope and defer dungeon design |
| 0013 | Reproducible Blender art/source and real Unity integration |
| 0014 | Echo-wide Codex semantics and discovery-only entries |
| 0015 | World transitions, map/session location boundaries |
| 0016 | Permanent class graph and one state per Echo manifestation |
| 0017 | Five attributes, defense curves and combat math |
| 0018 | Private dungeon loot choices and earned equipment |
| 0019 | Independently movable/resizable themed HUD |
| 0020 | Data-driven quest objectives and NPC crafting |
| 0021 | Hosted co-op and save/manifestation ownership |

## Design-document crosswalk

- **Vision/world:** `GAME_VISION.md`, `WORLD_AND_LORE.md`, `REGIONS.md`, `WORLD_1.md`, `DICEBOUND_WORLD_VISUAL_REFERENCES.md` → ADRs 0006, 0008, 0009, 0015, 0016.
- **Classes/attributes/combat:** `CLASS_PROGRESSION.md`, `CLASS_REQUIREMENTS_AND_DISCOVERY.md`, `STATS_AND_DAMAGE.md`, `COMBAT.md`, `ELEMENTS.md`, `SUMMONS_AND_COMPANIONS.md`, `docs/classes/` → ADRs 0004, 0007, 0016, 0017.
- **Dungeons/loot/encounters:** `DUNGEONS_AND_DEATH.md`, `ENEMIES_AND_ENCOUNTERS.md`, `LOOT_AND_EQUIPMENT.md`, `SLIME_DUNGEON.md` → ADRs 0006, 0011, 0012, 0018; individual dungeon layout remains unapproved and unimplemented.
- **Codex/quests/crafting:** `CODEX.md`, `QUESTS_AND_INTERACTIONS.md`, `CRAFTING_AND_PROFESSIONS.md` → ADRs 0014, 0020.
- **Network/account/persistence:** `MULTIPLAYER.md`, `ACCOUNT_PROGRESSION.md`, `PERSISTENCE_AND_SAVES.md`, `CHARACTER_IDENTITY.md` → ADRs 0005, 0006, 0015, 0016, 0021.
- **Interface/travel:** `UI_HUD.md`, `INPUT_CONTROLS.md`, `TRAVEL_MOUNTS_AND_WORLD_AGGRO.md` → ADRs 0015, 0019.
- **Engineering and art:** `ARCHITECTURE.md`, `TECHNICAL_DIRECTION.md`, `ART_PIPELINE.md`, `UNITY_PROJECT.md`, `UNITY_AUTOMATION.md`, `LOCAL_SETUP.md` → ADRs 0001–0003, 0009, 0013.
- **Implementation and validation:** `CORNBERG_*.md`, `*_VALIDATION.md`, `ARCHITECTURE_HARDENING.md` → record test evidence and current implementation; not separate policy ADRs.
- **Historical discussion:** `DECISION_LOG.md` is the chronology of choices and fine implementation semantics. `IDEA_INBOX.md` contains unsettled ideas; do not turn those into Accepted ADRs.

## Priority and conflicts

An Accepted ADR is a stable architectural decision. If an old prototype implementation document disagrees with an Accepted ADR (e.g. `UI_HUD.md` suggested deferring HUD editing, but 0019 deliberately requires it from the start), the ADR is the newer design commitment. Implementation details can still be staged and iterated.
