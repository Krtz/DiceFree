# Physically Blessed model candidates — 2026-10-14

User wants a more convincing Tier I model while keeping Greatblade visibly more advanced. This is candidate review only; the playable Physical presentation has not been replaced.

## Recommendation: Blink modular fantasy human starter armour

Owned local pack: FREE - Modular Character - Fantasy RPG Human Male.
Imported candidate: Assets/Blink/Art/Characters/Stylized/Humans/Prefabs_Humans/HumanMale_Character_Free.prefab.

Inspected in the running Unity Editor:
- Valid Humanoid Avatar.
- 21 SkinnedMeshRenderer components; selected prefab includes starter chest, belt, boots, gloves, legs, shoulders and optional helmet, plus modular head/hair/beard parts.
- Renderer bounds approximately 1.59 × 2.60 × 1.10 units in the default pose.
- Existing Blink idle/walk/attack animation sources are already installed; actual retargeted combat integration still needs validation if chosen.
- Preview rendered using temporary URP Lit material copies while preserving source textures/tint; vendor originals were not modified.

Suggested Tier I presentation: leather/padded chest, proper trousers and boots, bracers, simple shoulders, visible face, modest one-handed weapon when equipped. Scale it around current player height rather than inheriting the larger source bounds. Leave Greatblade's exaggerated sword and more elaborate silhouette distinct.

Preview: screenshots/physical-blink-starter-preview.png

## Other local candidates and limitations

| Candidate | Availability | Assessment |
|---|---|---|
| FREE Low Poly Human - RPG Character | Owned cached package; not imported in this workspace | Modular armour candidate; inspect/retarget before selecting. |
| Low Poly Fantasy Warrior | Owned cached package; not imported in this workspace | Potential more exaggerated martial silhouette; rig/material quality not yet checked. |
| Modular Fantasy knight character | Owned cached package | Potential future Defender; a full knight is less suitable for the flexible Tier I Physical novice. |
| warrior_girl.glb from ART_ASSETS | Local file inspected | 808 meshes, zero glTF skins, one animation. The zip source is Alembic (.abc). Not a ready Humanoid combat rig; substantially more adaptation than the installed Blink candidate. |

Downloaded source art remains local/ignored. These are technical and visual candidate findings, not permission to redistribute raw pack files or a completed in-game integration.
