# Slime Dungeon equipment art and Cornberg ambient life

Local work on `feature/54-world-dungeon-codex`, at `T:/TEMP/DiceFree-Options`. No commit, push, merge or branch change.

## Content and ownership

`Assets/_DiceFree/Art/Equipment/SlimeDungeon/` contains reusable Unity mesh/material assets and four prefabs authored by `VillageArtAuthoring`. These are closed, faceted, handcrafted 3D radial-profile and extruded-polygon meshes, with URP Lit materials. No generated images, Unity primitive placeholder meshes, or gameplay colliders are used.

- `SlimeOrb.prefab`: translucent emerald gel shell, suspended slime core with eyes, internal azure bubbles and a magical cradle.
- `SlimeShield.prefab`: substantial closed kite shield with iron rim/rivets, oak face, translucent slime coating, slime medallion and rear grip.
- `SlimyFarmersGloves.prefab`: separate practical oversized left/right gloves, leather palms, four fingers and thumb, cuffs, slimy fingertips/cuff coating.
- `SlimyTophat.prefab`: wide solid brim, tall flared emerald-black felt crown, translucent green hatband, clasp and hanging slime droplets.

The installer locates existing item definitions by the four `item.slime.*` stable IDs and never rewrites their stats, odds or class restrictions. It adds one `EquipmentPresentation.Binding` per definition, preserving the existing Work Gloves, Farmer's Pants and Bronze Dagger bindings. Each new binding contains visuals for all three authored humanoid forms. Bone-parented neutral sockets follow the real left/right hands and head; the existing `RightHandWeapon` is untouched. Gloves split from the reusable pair prefab into individual bone-attached scene visuals. OffHand eligibility remains under `Equipment.IsResolved` and the original allowed class IDs. Active class form roots control hierarchy visibility. `baselineVisuals` targets only explicit baseline hat objects; it never suppresses a whole body or all same-slot art.

`EquipmentPresentation.Refresh` resolves all bound item visibility before applying explicit baseline suppression. This keeps baseline handling independent of binding order and retains a selected item's visual if it appears in another binding's baseline list.

The existing Cornberg scene is modified additively. Imported generic NPC scene instances are unpacked locally only to allow a dedicated visual child pivot; source FBX art, material assets, root transforms, interactions and colliders remain intact. Other relevant maps are inspected for authored heroes and saved only if bindings/catalogs need additions. They currently share the Cornberg hero/session.

## Ambient life

`VillageVisualIdle` animates only the dedicated `VillageIdleVisual` child of each of the nine generic NPC role models. Absolute sine-based sampling prevents accumulation. Defaults are 0.65 degrees sway, 3 mm world-vertical bob, 0.15% breathing and an occasional smooth glance of at most 2.1 degrees. Local baseline position/rotation/scale are restored on disable. Per-instance deterministic seeds avoid synchronized NPCs. No Animator, combat rig, root collider or navigation transform is animated.

`VillageHomeWander` applies only to the generic Villager, two Farmers and Herbalist, provided they have no interaction target. Default radius is 2.5 m, speed 1.2 m/s and idle pauses 2–7 seconds, all serialized and configurable. Each destination is sampled on the existing NavMesh. Only complete paths whose entire corner sequence stays inside the home disk are accepted. Continuous movement raycasts each segment, faces travel, preserves the original vertical offset, and never warps. Missing NavMesh or unreachable destinations leave the NPC stationary. Banker, Merchant, Blacksmith, Alchemist and Guard remain stationary, with visual idle only. Named quest NPCs and combat actors are excluded.

No terrain or NavMesh rebuild, economy change, gameplay balance change or user-save write is part of authoring. Neither `CornbergSceneBuilder.Build` nor `CornbergArtBatchInstall.Install` is invoked.

## Commands

All commands target the connected Editor. Authoring and data validation require stopped Edit Mode and saved scenes.

```powershell
unity command set_autotick --enable true --project-path T:/TEMP/DiceFree-Options
unity command recompile --project-path T:/TEMP/DiceFree-Options
unity command recompile_status --project-path T:/TEMP/DiceFree-Options
unity command dicefree.village-art.install --project-path T:/TEMP/DiceFree-Options
unity command dicefree.village-art.validate --project-path T:/TEMP/DiceFree-Options
unity command dicefree.village-art.playtest --project-path T:/TEMP/DiceFree-Options
unity command dicefree.village-art.status --project-path T:/TEMP/DiceFree-Options
unity command dicefree.slime.validate --project-path T:/TEMP/DiceFree-Options
unity command dicefree.economy.validate --project-path T:/TEMP/DiceFree-Options
```

The isolated-save test uses `PersistenceTestGuard` and a fresh temporary save root, tests allowed/disallowed class equip, same-slot replacement/unequip, retention of older equipment, hand attachments, visual idle/restore, real NavMesh walking/home bounds/no teleports, missing-NavMesh fallback and stationary/interactable merchant. Its Unity camera renders are written to ignored `Logs/VillageArt_class.*.png`. It automatically exits Play Mode.

## Verification results

**Verified on 2026-10-08:** `dicefree.village-art.install` succeeded, including a repeated idempotency run. It detected **1 hero, 3 humanoid forms, 9 idle NPCs and 4 wandering NPCs**. `dicefree.village-art.validate` passed: **4 distinct prefabs, 55 authored mesh parts, 4 new item bindings**, original gear retained. Unity's recompile status was up-to-date with **zero reported compilation errors**. The isolated-save `dicefree.village-art.playtest` **passed 19,301 checks**, including equipment/class gating, hand-bone attachments, appearance changes, older gear swapping, real NavMesh wandering within radius, missing-NavMesh stationary behavior, idle pose restoration, and merchant immobility/interactability; it also captured screenshots in `Logs/VillageArt_class.*.png`. `dicefree.economy.validate`, its independent **31-check Play Mode test**, and `dicefree.slime.validate` also passed. These are automated checks, not final manual cosmetic approval.

The initial editor import briefly timed out; the editor was recovered and a namespace typo in the new validation script was corrected before installation. The final installed state passed the checks above.

## Manual polish boundaries

Generic NPC meshes are existing static role models: wandering translates/turns the model and does not add a leg-walking skeleton. The idle is deliberately tiny to keep feet/hands plausible. Final art direction, shield/hat silhouettes at the normal gameplay camera distance, clipping during every possible attack pose, and walk-cycle polish still merit a human review. Automated validation does not establish final game balance or multiplayer behavior.
