# DiceFree Downloaded 3D Asset Catalog

**Inventory date:** 2026-10-14  
**Inspection location:** `T:\TEMP\ART_ASSETS` on the connected Windows PC (Kratz)  
**Branch:** `poc/cornberg`  
**Scope:** 19 ZIP files and 1 partial Chrome download; no Unity imports or source-file changes performed.

This is the **technical and creative inventory** of downloaded assets, not a license grant or an approved game-content list. Track source attribution and legal status in [ASSET_CREDITS.md](ASSET_CREDITS.md). ZIPs were inspected in place (including nested ZIP member listings); FBX/Alembic rigs and actual Unity imports have **not** been validated.

## At-a-glance

- **8 direct GLBs:** all have **0 glTF skins and 0 glTF animations** (they are static as packaged). Each has around **456k–933k glTF POSITION vertices**, and approximately **849k–1.5M triangles**. These are high-resolution source candidates, **not** low-cost animated game characters. A skin/animation is not magically supplied just because the file extension is GLB.
- **10 FBX-bearing archives:** black dragon, blood priest, corrupted archangel, corrupted fallen priest, dark necromancer, fallen paladin, Kindred, Truffle Man, Viking house and wolf guy — **10 total**. Six of these are inside **nested** ZIPs (five Pigcraft-style character FBXs plus Kindred). Rigging/animation availability must be tested in Blender/Unity.
- **1 Alembic (`.abc`) archive:** Warrior Girl. Not a standard rigged GLB/FBX Unity character; needs a separate compatibility review.
- **1 `.glb.crdownload`:** incomplete Chrome download, **exclude**.
- No obvious README/LICENSE/credit text file was found in any outer archive; nested ZIPs contain model/texture files and Kindred instructions PDFs, but no identified license text. **Check actual source pages**, not just ZIP contents.

## Asset inventory

**Counts:** Direct-GLB vertex figures come from glTF accessor `POSITION.count` summed over mesh primitives; triangles from indexed triangle primitives. These are **not measured Unity-renderer vertices** and imported totals can differ. **Size** is ZIP file size, approximately MiB. `Unverified` refers to original license/provenance, not whether the ZIP opens.

| Archive (ZIP) | Size | Actual content | Geometry / animation inspection | Possible DiceFree use | Licensing / disposition |
| --- | ---: | --- | --- | --- | --- |
| `anime-swordswoman-image-to-3d-supavoxel.zip` | 30.9 MiB | GLB + texture | **456,649 vertices; ~848,982 tris; no skin/animations** | Stylized swordswoman visual reference | **Hold:** original creator/license unknown; needs rig/LOD |
| `black-dragon-with-idle-animation.zip` | 14.1 MiB | FBX + five texture maps | ~5.0 MiB FBX; creator describes an idle-animated rig; **not independently imported** | Dragon creature/boss reference | **Hold:** [3DHaupt/3DHaupt dragon](https://sketchfab.com/3d-models/black-dragon-with-idle-animation-fb0053a2e59b43868e934c239bf4eb36) **CC BY-NC**, not a cleared commercial game asset |
| `blood-priest-in-crimson-ritual-robes.zip` | 59.2 MiB | Nested ZIP containing ~54.7 MB FBX + PBR textures | FBX rig/animation/geometry untested | Cultist / priest antagonist | **Hold:** original license/source URL not verified |
| `catrina-skeleton-bride-figure-meshi-vs-supavoxel.zip` | 44.1 MiB | GLB + PBR maps / badge | **521,601 vertices; ~993,270 tris; no skin/animations** | Undead boss / skeleton bride inspiration | **Candidate:** CC Attribution shown on download screenshot; original provenance/version still to verify |
| `coffee-cup-samurai-sloyd-vs-supavoxel.zip` | 80.7 MiB | GLB + PBR maps | **933,467 vertices; ~1,499,498 tris; no skin/animations** | Secret joke boss / cosmetic reference | **Hold:** license, generator/source-image rights unverified |
| `corrupted-archangel-six-wings-boss.zip` | 63.3 MiB | Nested ZIP containing ~56 MB FBX + PBR maps | FBX rig/geometry untested | Set-piece / raid boss | **Hold:** original license not verified; high-detail model |
| `corrupted-fallen-priest-dark-fantasy.zip` | 63.2 MiB | Nested ZIP containing ~58.1 MB FBX + PBR maps | FBX rig/geometry untested | Corrupted cleric enemy | **Hold:** source/license unverified |
| `cyberpunk-swordswoman-3d-ai-studio-vs-supavoxel.zip` | 75.6 MiB | GLB + PBR maps | **851,430 vertices; ~1,500,000 tris; no skin/animations** | Out-of-theme character reference / future tech region | **Hold:** source/license unverified; high-detail static |
| `dark-necromancer-corrupted-staff-skulls.zip` | 58.7 MiB | Nested ZIP containing ~53.4 MB FBX + PBR maps | FBX rig/geometry untested | Necromancer boss / NPC | **Candidate:** [Pigcraft source](https://sketchfab.com/3d-models/dark-necromancer-corrupted-staff-skulls-10c3bef4113b4e128ad41c9fdcab74e3) shows CC Attribution; verify exact version/provenance before import |
| `fallen-paladin-in-corrupted-black-plate-armor.zip` | 63.3 MiB | Nested ZIP containing ~56.4 MB FBX + PBR maps | FBX rig/geometry untested | Fallen knight / miniboss | **Hold:** original license unverified |
| `holy-knight-lady-character-hyper3d-vs-supavoxel.zip` | 95.5 MiB | GLB + PBR textures / badge | **907,394 vertices; ~1,496,154 tris; no skin/animations** | Holy warrior advanced class reference | **Hold:** source/license unverified; high-detail static |
| `kindred-league-of-legends-rigged.zip` | 75.5 MiB | Nested ZIP containing ~79.6 MB FBX, rig instructions PDFs + textures | Creator describes a rig, but animation/Unity compatibility untested | **Reference only**, not a DiceFree character | **Hold / do not ship:** third-party **League of Legends** character; creator's CC BY fan-model license does not clear underlying IP |
| `redhead-anime-swordswoman-png-to-stl-supavoxel.zip` | 38.9 MiB | GLB + texture | **579,534 vertices; ~1,071,218 tris; no skin/animations** | Swordswoman visual reference | **Hold:** source/license unverified; high-detail static |
| `samurai-cat-tripo-3d-vs-meshy-supavoxel.zip` | 108.4 MiB | GLB + PBR textures / badge | **879,416 vertices; ~1,499,452 tris; no skin/animations** | Hidden enemy / oddball boss | **Hold:** source/license unverified; high-detail static |
| `truffle-man.zip` | 17.2 MiB | FBX (~4.1 MiB) + eight PBR maps | Creator's model page: **~65k tris**; actual rig/animation unverified | **Strong early creature/boss candidate** | **Candidate:** [dotflare source](https://sketchfab.com/3d-models/truffle-man-e41c6781e4854a1fbe5cdf24a0029aab) lists **CC Attribution** |
| `viking-house.zip` | 113.1 MiB | FBX (~43.9 MiB) + **33** texture files | Scene geometry/material count untested; significant texture memory risk | Settlement environment / building reference | **Hold:** source and rights for reused photo textures unverified |
| `viking-shieldmaiden-3d-model.zip` | 44.2 MiB | GLB + textures | **582,431 vertices; ~1,119,964 tris; no skin/animations** | Barbarian / shieldmaiden class inspiration | **Hold:** source/license unverified; high-detail static |
| `warrior-girl.zip` | 17.1 MiB | **Alembic `.abc`**, texture maps | No GLB/FBX; Alembic may represent cached animated geometry but **no rig validated** | Reference until converter/Unity compatibility is proven | **Hold:** source/license unverified |
| `wolf-guy.zip` | 60.5 MiB | FBX (~1.5 MiB) + 12 texture maps | Some textures very large (two normal maps ~16–17 MiB each); rig/animation untested | Werewolf / forest humanoid enemy | **Hold:** source/license unverified |

### Ignored partial file

`warrior_girl.glb.crdownload` (~2.3 MiB) is a Chrome incomplete download, **not a valid deliverable**. Do not register it as a second Warrior Girl asset until download completion.

## Priority shortlist

1. **Truffle Man** — First practical import/visual probe. Size and polygon estimate are much more plausible for a fantasy creature, and source currently shows CC BY. Check rig, scale, PBR-to-URP conversion, material slots and provenance.
2. **Dark Necromancer** — Good potential boss, CC BY listed, but a large nested FBX; verify geometry, rig, and whether the aesthetic fits DiceFree before committing.
3. **Catrina Skeleton Bride** — Visually interesting high-detail undead boss candidate; static, ~1M triangles. Needs verified attribution/version, heavy optimization or LODs, and rigging if mobile.
4. **Fallen Paladin / Corrupted Archangel** — Boss visual candidates **only after license clearance** and FBX profiling.
5. **Viking House** — Environment test candidate after texture provenance and total draw calls/materials/mesh breakdown are checked.

**Do not ship:** Black Dragon under CC BY-NC without additional permission; Kindred without the rights-holder's authorization. Do not import any other asset for distribution while the license is unresolved.

## Next technical validation

For each licensed shortlist item: verify original source + exact license version; hash/archive provenance if moving to version control; open in Blender; inspect mesh count, actual vertices/triangles, materials/textures, armature/animations, UVs, normals, topology, disconnected components and scale; evaluate at real DiceFree camera; prepare LODs/rig if needed; import into an isolated Unity asset-test scene using a dedicated `Assets/_DiceFree/Art/ThirdParty/...` area; retain author files and credits alongside `SourceArt/ThirdParty/...`. Do **not** add 1+ GB of raw ZIPs to Git, or automatically integrate any of these assets into scenes/classes/bosses.

**Source evidence:** Remote Desktop Commander directory listing, ZIP central directories and nested archive listings, glTF JSON headers/accessors read directly from ZIP streams (2026-10-14), plus the linked Sketchfab creator listings for known licenses. ZIP member filenames and metadata do **not** prove lawful reuse.
