# Physically Blessed Novice validation (#51)

Validated on 2026-10-07 in Unity 6000.6.3f1 on `feature/51-physically-blessed`, stacked directly on #50 commit `66160948e615b15feb2732c306f0cd57c29b02ad`.

## Implemented slice

- Physically Blessed Novice is now a real playable Tier-1 manifestation rather than a #50 actor shell.
- Dedicated presentation prefab derived from the accepted Novice humanoid family:
  - slightly larger global silhouette;
  - modestly stronger chest/arm/leg proportions;
  - dedicated shirt material;
  - shared valid humanoid avatar/controller;
  - no presentation-owned gameplay colliders or root motion.
- Settled primary stats:
  - level 1: 12 VIT / 12 STR / 12 AGI / 5 INT / 5 SPI;
  - average growth: +1 VIT / +2 STR / +2 AGI / +0.5 INT / +0.5 SPI per level.
- Adaptive physical basic attack uses the higher of STR/AGI and normal accuracy.
- Reusable class-resource framework plus the first authored profile:
  - Mana stable ID `resource.mana`;
  - Physical maximum = 60 + INT;
  - regeneration/s = 4 + 0.1 x SPI;
  - level-1 Physical therefore starts at 65 Mana / 4.5 Mana per second;
  - Physical Mana explicitly uses the Persist retention policy.
- Five Physical skills, six ranks each, with one skill point per Physical level:
  - Heavy Strike;
  - Guard;
  - Quickening;
  - Arrow Rain;
  - Martial Aptitude.
- Heavy Strike uses higher STR/AGI physical scaling and ordinary stun resistance/immunity rules.
- Guard adds source-keyed temporary Physical + Magical incoming-damage reduction.
- Quickening adds source-keyed temporary Attack Speed + Movement Speed.
- Arrow Rain is a 12 m ground-targeted, 3 m-radius, three-wave physical burst with simple spectral-arrow prototype presentation.
- Martial Aptitude adds durable class-skill-derived basic-attack damage and Physical Defense while the Physical manifestation is active.
- Four active skills use the existing shared 1-4 Input System actions; Martial Aptitude is passive.
- Prototype Physical allocation panel, skill bar, and reusable resource bar are wired into Cornberg.
- Class presentation automatically switches between Novice and Physical when manifestation identity changes.
- Manifestation persistence now validates/captures/restores class resources while preserving unresolved resource records.
- Physical skill ranks and Mana remain independent from the preserved Novice manifestation.
- Ordinary ability cooldowns, Guard/Quickening effects, Arrow Rain coroutine state and other combat-session state remain transient.

## Runtime serialization bug caught by validation

The first Play Mode run found a real Unity asset-serialization defect: `ClassResourceProfile` was initially declared as a second ScriptableObject inside `ResourceDefinition.cs`. Unity could display/use the asset in Edit Mode, but the generated asset serialized with `m_Script: {fileID: 0}`, so the runtime `ActorResourceController` received a null profile.

The fix was structural rather than a workaround:
- `ClassResourceProfile` now lives in its own correctly named `ClassResourceProfile.cs`;
- the authored Physical Mana profile reserialized with a real MonoScript GUID;
- a cold Editor restart, static validation and full Play Mode rerun all resolve the real asset correctly.

## Focused validation

Passed:

- `DICEFREE_PHYSICAL_AUTHORED_OK`
- `DICEFREE_PHYSICAL_DATA_OK`
- `DICEFREE_PHYSICAL_PLAYMODE_OK`
- `DICEFRE_ARCHITECTURE_OK`
- `DICEFRE_ARCH_POLICY_SELFTEST_OK`
- `DICEFREE_MANAGED_REFERENCE_OK`
- `DICEFREE_CONTROLS_OK`

The dedicated #51 Play Mode harness proves:

1. level-10 Novice advances into a level-1 Physical manifestation;
2. Novice skills deactivate and Physical progression activates;
3. the dedicated Physical presentation and adaptive basic attack become live;
4. Mana initializes from class stats and updates at level 5;
5. four active ranks + Martial Aptitude consume exactly five level-5 points;
6. Martial Aptitude rank 1 grants +5% basic-attack damage and +1.5 Physical Defense;
7. Heavy Strike damages a real Cornberg hostile, spends Mana, starts cooldown and respects ordinary 50% stun resistance;
8. Guard spends Mana and produces 15% Physical + Magical damage reduction with refresh-not-stack semantics;
9. Quickening spends Mana and produces +12% Attack Speed / +4% Movement Speed at rank 1;
10. Arrow Rain spends Mana, hits two real Cornberg hostiles in its first wave and continues through the short three-wave burst;
11. skill ranks and Mana serialize into the Physical manifestation;
12. leaving Play Mode writes the current regenerated Mana rather than freezing an earlier checkpoint value;
13. StartMenu shows the saved level-5 Physical manifestation and selects it;
14. Cornberg reloads Physical with ranks, persisted Mana, Martial Aptitude and dedicated presentation intact;
15. ordinary skill cooldowns reset across the load boundary.

The final focused Play Mode run was repeated after a cold Unity Editor restart and passed.

## Existing regressions

Passed after the new resource/combat/presentation layers:

- `DICEFREE_ADVANCEMENT_PLAYMODE_OK`
- `DICEFREE_NOVICE_SKILLS_PLAYMODE_OK`
- `DICEFREE_CONTROLS_PLAYMODE_OK`
- `ISSUE36_SAVE_RELOAD_OK`
- items/inventory/equipment/gold regression
- Q4 surge/reward/elite/world-drop regression
- `ISSUE36_ROUTE12_OK`
- `ISSUE36_RUNNER_OK`

No existing assertions were weakened to make #51 pass.

## Windows Development build

`DICEFREE_PHYSICAL_PLAYER_BUILD_OK` passed:

- Unity: 6000.6.3f1
- target: Windows x64 Development
- result: Succeeded
- errors: 0
- warnings: 5 (existing prototype/build-environment warnings)
- total output: 188,590,953 bytes
- first scene: `Assets/_DiceFree/Scenes/StartMenu.unity`
- scene count: 2
- executable: `%TEMP%/DiceFree-Physical-Issue51/DiceFree.exe`

## Explicitly still provisional / out of scope

- All current coefficients, Mana costs, cooldowns and resource formulas are playtest tuning rather than immutable final balance.
- The Physical presentation is the intended modest Tier-1 evolution of the Novice family, not a later highly specialized Fighter/Rogue/Ranger/Tank silhouette.
- Final skill icons, bespoke VFX, audio and attack animations remain future presentation work.
- Full weapon-family/equipment-permission implementation is not part of #51.
- The current Physical skill/allocation/resource UI is prototype UI; production HUD work remains its own milestone.
- Level-30 descendants remain design/future implementation work.
- Magically Touched Novice remains a separate next class slice.
- No merge to `poc/cornberg` or `main` is implied by this document.
