# Issue #45 validation evidence - 2026-10-05

Branch: `art/45-cornberg-work-gloves`, based on
`b407ee36a4ed1fcb505f7914d2f0f596ebeea032` / `art/43-novice-cornberg`.
Validation ran against the implementation working tree before its commit.
Unity commands' automatic Git-head field therefore names that base commit;
it must not be confused with the final implementation SHA reported at push.

| Target | Evidence |
| --- | --- |
| Reproducible Blender export | Blender 5.2.2 LTS; two clean runs matched `dfc8d252050dcf08149f94e13eaf9e52a15eda5f715f82ba1c3dc01935b80e10`; closed manifold/outward/nondegenerate mesh checks passed |
| Source counts | Four meshes, 6,480 vertices, 12,944 triangles |
| Unity import | Unity 6000.6.3f1; compilation completed without errors; `WORK_GLOVES_PREPARED` and `WORK_GLOVES_ART_OK`; 6,558 imported vertices / 12,944 triangles |
| Both hands / animation | Five samples each of Idle, Locomotion and UnarmedAttack; actual weighted-hand and glove centroids differ by less than 0.04 m; both gloves' baked vertices move with the shared rig |
| Equip / baseline | Real player Equipment equips/unequips three times; baseline mesh/material/enabled state retained; disabled presentation restores baseline and re-enable restores equipped appearance |
| Exact stat package | Definition still stores percentage points `5`; effective AttackSpeed is baseline x1.05; Physical/Magical Defense and MoveSpeed unchanged |
| Q4 exactly once | Real journal accept, elite defeat credit, turn-in and repeated turn-in; exactly one glove; quest completion alone leaves Hands unequipped |
| Persistence | Real save flush/scene reload while equipped and unequipped preserves ownership GUID and appearance; no reward replay; unknown definition remains inert; removing equipped glove restores baseline |
| Existing regressions | `ItemValidation` and full `CornbergSurgeValidation` passed using new connected-Editor entry points, with original assertions unchanged; Q4 includes both routes, seven reloads and actual 450-second respawn |
| Architecture / policy / managed references | `DICEFRE_ARCHITECTURE_OK`, `DICEFRE_ARCH_POLICY_SELFTEST_OK`, `DICEFREE_MANAGED_REFERENCE_OK` |
| Other regressions | `DICEFRE_NOVICE_PIPELINE_OK`, `DICEFRE_NOVICE_CORNBERG_PRESENTATION_OK`, `ISSUE36_SAVE_RELOAD_OK` (two-phase storage/migration/recovery) |
| Windows build | Existing `dicefree.art.novice.cornberg.build-windows`, detached job `9d226c7134294ac88ef23a12274fa78f`, completed successfully; 187,073,485 bytes, zero errors, two warnings |
| Standalone reload | 29.5 seconds against copied isolated item fixture; schema 4, revision 2 -> 3, four owned instances, three equipped references, same Hands GUID `fdac2b2a-63eb-479d-ab5c-44c90c3ead91`, 37 gold; no runtime errors |
| Human review image | `Assets/_DiceFree/Art/Validation/Previews/CornbergWorkGloves_Unity.png`; baseline, equipped idle, locomotion, attack |

The build warnings are the existing missing runtime Pipeline config (automation
is disabled in Player builds) and `InventoryPanel.cs` UAC0009 for
`DEVELOPMENT_BUILD`. Neither is a glove import/runtime error. Editor compile
cache was explicitly refreshed and rebuilt before accepting the stronger final
hand-centroid validation; a stale successful command response was not treated
as proof that new validation source had compiled.

Task logs remain local, excluded from Git:
`Logs/Editor.log`, `Logs/WorkGloves-Standalone.log` and
`Logs/WorkGloves-Smoke.json`. Builds and temporary saves are not committed.
Build output: `%TEMP%/DiceFree-Novice-Cornberg-Issue43/DiceFree.exe`.

## Review scope and limitations

The Novice skin-weight defect was documented before correction. The generator
now transforms mesh-local vertices into character space before nearest-bone
selection. A separate migration preserves the original source geometry rather
than rebuilding or segmenting it. `novice_weight_repair.json` records matching
before/after geometry hashes. Animation deformation changes intentionally to
make visible hands follow the actual bones; baseline geometry, materials,
clothing objects and gameplay footprint are unchanged.

Changed content consists of the glove source/export/materials/wrapper/preview,
small UI equipment binding, Cornberg scene composition, focused Editor checks,
connected entry points for existing item/Q4 tests, the necessary Novice source/
export weight correction, and related docs/handover. No item stat, Q4 reward,
attack-speed formula, quest tuning, navigation, save schema, or other branch is
changed. Import-generated unrelated project/URP settings are restored.

This is prototype workwear awaiting human approval. The Novice retains crude
single-bone clothing deformation; clothing seams, animation polish and mesh
optimization remain future art work. No manual playthrough, standalone visual
review, mature equipment-art/transmog system or cross-form fitting is claimed.
The focused source and Unity checks cover the sampled prototype clips; they do
not certify every future animation or body form. Issue #45 remains open/unmerged.
