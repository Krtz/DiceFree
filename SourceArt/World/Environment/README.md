# Textured trees and outdoor lighting — 2026-10-16

Axel requested richer trees, a day/night cycle and moving sun/moon shadows.

Selected existing local pack: **Stylized Forest Kit Samples**, Symphonie Studio / Unity China Publisher Hub.
Store: https://assetstore.unity.com/packages/3d/environments/stylized-forest-kit-samples-403082
License: Standard Unity Asset Store EULA, https://unity.com/legal/as-terms (store listing checked 2026-10-16).
The locally downloaded package SHA-256 is recorded in tree_source_receipt.json, with the exact source model/texture byte roster.

Only M_Oak01.fbx and eight oak texture images were curated from the existing licensed package. No demo scene, script, shader graph, HDRP asset, or project setting was imported. The recorded roster is authoritative. Sources remain in ignored Assets/_DiceFree/LocalArt/TreesLicensed/Source; generated URP materials and four prefabs remain in ignored Resources/TreesLicensed. Neither original package bytes nor derived licensed geometry/textures belong in the public source repository.

The installed game-dev CLI was unavailable. This is a curated Unity-native integration, not a canonical game-dev vendoring/validation receipt. The roster records exact selected bytes and package identity; runtime tests separately verify Unity import, rendering and gameplay behavior.

Rebuild local adapted prefabs with WorldEnvironmentAuthoring.BuildTrees or dicefree.environment.install after placing the licensed sources at the recorded destination. The authoring code builds ordinary URP/Lit cutout materials, maps base/normal/AO textures, retains the three mesh LODs and normalizes the prefab height. Four texture palettes share the same mesh. Instance height and narrower crown proportions fit the existing world. Existing conifers remain as biome variety.

Scene adapters contain no serialized licensed model/material reference. At runtime they resolve optional resource prefabs, hide the existing original tree renderers and retain every gameplay collider/NavMesh obstacle. Without those resources, the original scene art remains visible and functional. Tree camera fading retains leaf alpha cutout, and Briarback tree pivots are grounded so proximity fading works.

Credits source: SourceArt/Credits/credit_manifest.json; regenerate with dicefree.credits.author.
