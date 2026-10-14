# Magically Touched / Blessed model recommendation

Status update: the user approved this candidate and its smaller/slimmer class adaptation is now integrated. See [MAGICAL_CLOTH_MODEL_IMPLEMENTATION.md](MAGICAL_CLOTH_MODEL_IMPLEMENTATION.md) for current implementation and validation. The candidate-only notes below describe the earlier review stage.

Recommend the owned **Polytope Low Poly Modular Armors — Free** modular male human using its **cloth00** body, legs, gauntlets and boots, a visible face, a short cape and blue/violet colours. Pair it with a modestly scaled **Adventure Forge Fantasy Staff Lite**, with a soft gem glow. This gives the starter caster a recognisable silhouette while leaving room for a more elaborate advanced class.

This is a candidate review, not an installed replacement. No paid generation, new package import, licensed-source publication, retargeting or gameplay model switch was performed.

## Evidence from the local owned cache

| Candidate | Observed source content | Fit / integration cost |
|---|---|---|
| Polytope modular armours, free | Modular male FBX, cloth00 body/boots/gauntlets/legs, head/hair variants, capes, skin deformers and a complete named humanoid bone chain | Best starter silhouette; select cloth rather than plate, convert materials and validate a Unity Humanoid avatar |
| Fantasy Staff Lite | Metalstaff01 FBX/prefab, gem/rings/casing and 512 texture variant | Suitable simple staff; older Unity 5 materials need conversion |
| Fantasy Garment for UMA | Male/female robe and dress meshes, mage robe recipes, textures, UMA slots and mesh-hide data | Strong robe art, but UMA dependencies or offline extraction/retargeting make it a larger integration |
| Installed Blink free human | Existing Physical human and spellcasting animation sources | Lowest reuse cost, but less distinct robe silhouette |
| SopaDopa Character Constructor | Large modular armour-oriented collection | More work and less convincing starter-caster fit |

Review used the local asset catalogue and the actual cached package members, not only catalogue keyword matches. Polytope source was extracted only under ignored Library/MagicalAssetReview. Its ASCII FBX was statically inspected; Blender 5.2 could not import that ASCII format. This does not establish a broken Unity source, and no Unity 6 avatar, live animation, portrait or material compatibility result is claimed.

Polytope archive SHA-256:
2876c4f7a65ae86b2c6708f6254042ced2cd788bd6128980ac49b8986784ee73

Primary listings checked:
- [Polytope modular armours](https://assetstore.unity.com/packages/3d/characters/low-poly-modular-armors-free-lowpoly-medieval-fantasy-series-199890): standard Unity Asset Store EULA, public listing reports URP compatibility for Unity 2022.3.62; not a Unity 6000.6 runtime verification.
- [Fantasy Staff Lite](https://assetstore.unity.com/packages/3d/props/weapons/fantasy-staff-lite-54999): standard Unity Asset Store EULA, older Unity 5-era source.

Next integration should first make a local URP preview and Humanoid avatar, verify the existing caster animations and weapon sockets, then replace the presentation while preserving class progression and combat. Keep all licensed sources and derived art ignored, as with the Physical and Greatblade model resources.
