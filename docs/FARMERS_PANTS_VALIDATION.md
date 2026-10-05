# Issue #46 validation evidence - 2026-10-05

Branch `art/46-farmers-pants`, continued from existing partial work on glove
head `e7ce68909307159737fa83b57cd503a1f7a25389`. No merge. Issue remains open.
All Unity CLI calls targeted `T:\TEMP\DiceFree-FarmersPants`, Editor port 7801,
Unity 6000.6.3f1. The separate glove Editor on 7800 was not targeted.

| Check | Evidence |
| --- | --- |
| Blender reproducibility | Two Blender 5.2.2 LTS runs matched geometryWeightsSha256 `8a22ff06453727d91ecfa992c8832944ebd7f882fc2c4eb630be28486174e22d`; 19,532 vertices / 39,048 triangles; manifold, outward and nondegenerate checks passed |
| Import and binding | `FARMERS_PANTS_PREPARED`, `FARMERS_PANTS_ART_OK`; four skins on actual Novice Hips/LeftUpLeg/LeftLeg/RightUpLeg/RightLeg; no added Animator/collider |
| Rig | `FARMERS_PANTS_POSES_OK` for five samples each Idle/Locomotion/UnarmedAttack; ankle/thigh/knee proximity and locomotion deformation; `FARMERS_PANTS_KNEE_FLEX_OK` separately tests synthetic 25-degree knee flex |
| Generic attributes | `EQUIPMENT_PRIMARY_ATTRIBUTES_OK allFive=true sources=2`; all five attributes sum two sources and restore on removal |
| Exact item stats | Only +1 Vitality and +1 Physical Defense; +15 maximum HP and +0.1 HP/sec regeneration; other core attributes/Defense/AttackSpeed/MoveSpeed unchanged; missing HP preserved |
| Presentation and persistence | `FARMERS_PANTS_EQUIPMENT_OK`, `FARMERS_PANTS_STATS_BASELINE_FEET_RELOAD_OK`; repeated equip/unequip, exact baseline mesh/material/enabled/active restoration, disable/re-enable, inactive baseline fixture, Configure restoration, three reloads, owned identity, unresolved inert records and equipped-item removal |
| Feet/Hands coexistence | Explicit foot-socket fixtures with current forest-shoe definition; independent Legs/Feet replacement; actual gloves visible concurrently |
| Glove regression | `WORK_GLOVES_ART_OK`, `WORK_GLOVES_EQUIPMENT_OK`; original Q4 reward/equip/stat/reload assertions retained, binding selected by Hands slot |
| Architecture | `DICEFRE_ARCHITECTURE_OK`, `DICEFRE_ARCH_POLICY_SELFTEST_OK`, `DICEFREE_MANAGED_REFERENCE_OK`; rerun after final changes |
| Preview | `FARMERS_PANTS_PREVIEW_OK`; regenerated and visually inspected PNG at `Assets/_DiceFree/Art/Validation/Previews/FarmersPants_Unity.png` |
| Windows Development | `DICEFRE_NOVICE_CORNBERG_PLAYER_BUILD_OK result=Succeeded`; 188,417,911 bytes; output `%TEMP%/DiceFree-Novice-Cornberg-Issue43/DiceFree.exe` |

Build response exceeded the CLI 30-second limit. Editor log confirms final
success. Build summary reports four errors and five warnings: Pipeline main
thread request timeouts occurred while the synchronous build was busy; retained
warnings include missing runtime Pipeline configuration and DEVELOPMENT_BUILD
UAC0009. This is build evidence, not a standalone playthrough or clean zero-error
build claim. Logs and build output remain local and excluded from the commit.
An incidental preview attempt during Play Mode was rejected; the final preview
was subsequently regenerated successfully in Edit Mode.

Item is prototype/testing only with source `prototype.test`; no acquisition
content or save-schema change. Generic zero-default AttributeValues keeps old
gloves/shoes inert for primary attributes. Feet checks use fixtures, not the
separate shoe art branch. Existing animation clips lack independent knee flex;
synthetic validation is labelled separately. Human art approval, cloth polish,
mesh optimization, other forms and unsampled animations remain outside scope.
