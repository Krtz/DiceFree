# DiceFree October 13 — Cornberg and Ironpine playtest polish

Branch: `feature/visual-overhaul-vis00`. Applies to the existing Cornberg scene; previous quest/story continuity and all local player saves remain intact.

## Implemented

1. **Real 3D quest punctuation** — `QuestWorldMarker` attaches at runtime to every `QuestGiver`. Gold animated/bobbing exclamation made from 3D mesh cylinders/sphere marks *new quest available*. Green animated curved 3D question mark means *ready to turn in*. Existing quest journal and prerequisites decide visibility. Non-interactive (IgnoreRaycast) model geometry, billboard toward camera.
2. **Raised overhead HP** — common model-height anchor raised by ~0.59 metres for player and monsters while retaining correct fog checks, per-category Always/When Hurt/Never preferences and floating numbers.
3. **Minimap orientation** — `Options → HUD / Themes → Minimap orientation` offers **North at top (default)** and **Rotate with camera**. Camera and fog projection rotate together. Clicking any map location (including attack-move, Shift queue and ordinary walking) correctly converts minimap-space input into world-space under both modes. Four compass markers show true north/south/east/west positions even on rotated maps. Existing `WorldPointForMapClick` north-up API preserved. Gameplay preferences migrated to v3, carrying forward all earlier preferences.
4. **Quillrat animation and projectiles** — imported Quillrat model gets a 180° facing correction so its head points in the root actor's actual +Z pathing/attack direction. Homing quills originate ~0.75 m along the actor's forward snout and at mouth height rather than body center/tail. AI, chase/leash, projectile timing/damage remain unchanged.
5. **Repeatable slime quest** — `Another Thirty Slimes` from Mira after `Break the Slime Surge`, available only after that quest's completion. 30 slime kills (family `enemy-family.slime`, including Crop/Road/Forest varieties) while quest is active, return to Mira; **120 XP reward** per completion, no new rare item or gold farming. Quest can be immediately accepted again after a successful turn-in. New general `QuestDefinition.repeatable` opt-in and transaction-safe reset in `QuestJournal.TurnIn`; all earlier quests remain one-time. Existing saves load missing quest definition as Available with prerequisite lock.
6. **Alpha Wolf attack buff** — applies a **1.35× multiplier only to BasicAttack auto-attack impacts**, on top of the existing +50% global wolf damage. Baseline auto output is equivalent to 2.025× vs previous 1.50× = **exactly 35% more normal autos**. Quillrats, ordinary wolves, Alpha pounce/lunge and scripted special damage are unaffected. Existing Frenzy contributions remain.
7. **Clickable NPC/enemy inspection** — left-click enemy/ally or Interactable NPC to select for a separate movable **lower-left 3D Target Inspector**; 3D RenderTexture camera clones the selected model into an isolated preview stage/layer. Combat targets show HP, attack range, physical/magical defense and movement speed based on their real actor stats; noncombat NPCs show their actual display name and role (not fake stats). Enemy attack-selection remains separate and right-click interactions still work. ESC target clear releases inspection. New `IInspectableVisual` interface avoids coupling the UI assembly to Ironpine gameplay types. Existing player's lower-left character portrait remains available.
8. **Novice fists** — attack interval **1.10 → 1.35 seconds** (about 18.5% fewer autos per minute at the same AttackSpeed modifier); damage and active skills are unaffected.
9. **Road obstruction** — Unity Editor inspection identified five physical spruce/pine/cypress tree trunks within 4.15 m of Ironpine road centerline, one near **X173.9/Z66.4** only 0.94 m from road center. Relocated their visible trees **and** capsule/NavMeshObstacle components to >4 m from the lane. Re-ran existing forest sight/solid trunk authoring pass; **702 solid trees** validated (140 corrected undersized colliders), and the continuous NavMesh route to Village 2 reserved clearing remains complete. Three tiny samples of the old NavMesh centerline at X121, X130 and X195 remain more than 0.6 m from the existing mesh and need **manual travel verification**. Do not claim the precise screenshot spot is proven fixed until playtested.
10. **NPC service placement** — moved existing 3D merchant by General Goods entrance to X17/Z−8.65 and existing brewer/alchemist by Brewery entrance to X−3/Z−8.7, using local ground height, preserving models and interactions. Both buildings already face the road; no geometry or shop inventories changed.

## Validation

- Full Unity 6000.6.3f1 project compilation: **0 errors, 0 warnings**.
- Isolated Play Mode marker `DICEFREE_OCT13_GAMEPLAY_OK`: Alpha 2.025× basic multiplier, 22 wildlife actors, quillrat rotated rig, Novice slower interval, raised HP anchor, minimap rotation/click conversion and default, live target 3D model and NPC render clones, animated QuestWorldMarker components, repeat quest authored conditions. Additional probe version exercises two complete disposable **repeat-quest accept/turn-in cycles**, and camera-up preference toggle.
- `dicefree.cornberg.repeat-slime.install`: installed quest asset and serialized authoring in saved Cornberg, once.
- `dicefree.oct13.move-vendors`: moved both 3D service NPCs.
- `dicefree.oct13.clear-road`: relocated five intrusive trees.
- `dicefree.ironpine.validate`: road and reserved Trimery approach connected (16 route corners).
- `dicefree.forest.sight.validate`: both house entrances face the road; 702 real solid tree trunks.
- `dicefree.ironpine-wildlife.validate`: population 10/7/4/1 and original nav spawn authoring intact.
- `dicefree.ui.health-channel.validate`: v1→v3 settings migration, unaffected options and minimap preference persistence passed after updating existing regression.
- Isolated Unity startup may log a known editor `SearchDatabase ArgumentOutOfRangeException` unrelated to successful marker; main project compiles.
- The isolated tests use a separate project and separate gameplay settings/Echo-save paths. **No user active character save touched.**

## Next in-game checks

Walk exactly where the red screenshot was taken, particularly along the tree-lined road near the X174/Z66 bend. The screenshot has no attached world coordinates, so this is the best supported obstruction fix, not an absolute guarantee. Test clicking moving quillrats and NPCs for accurate portraits, check their visible forward direction and quill launch, test quest marker status and finishing/re-accepting Mira's 30-Slime quest, try rotating map while moving and attack-moving on the minimap, and verify target inspector position on a short/wide window. Quest 120-XP repeat reward is provisional and can be rebalanced when leveling pacing has enough playtest evidence.

Preserve unrelated locally modified EarlySkillVfx materials, PackageManager/ShaderGraph settings and untracked licensed audio meta. Do not stage Unity's unrelated reserialized quillrat materials or merge into `main` without approval.
