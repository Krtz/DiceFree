# Physically Blessed starter-armour presentation — 2026-10-14

Physically Blessed Novice now uses the approved Blink modular fantasy human in starter leather/padded armour: trousers, boots, bracers and small shoulder pieces, with the optional helmet hidden. This replaces its visible T-shirt/underwear presentation in Cornberg and the Development Room. Its class mechanics and advancement requirements remain intact.

## Source and reproducible adaptation

- Owned local catalog: T:\TEMP\DiceFree-AssetCatalog\packages.json.
- Pack: FREE - Modular Character - Fantasy RPG Human Male, Blink, version 2.0.
- [Publisher listing](https://assetstore.unity.com/packages/3d/characters/humanoids/humans/free-modular-character-fantasy-rpg-human-male-228952): Standard Unity Asset Store EULA, verified 2026-10-14. [EULA](https://unity.com/legal/as-terms).
- Cached package SHA-256: 863F36A5FCC425904ED366D85B4D16274DFC664EAF0010BCD2387141BFC074A6.
- Source prefab: Assets/Blink/Art/Characters/Stylized/Humans/Prefabs_Humans/HumanMale_Character_Free.prefab.
- Derived resource: Assets/_DiceFree/Resources/PhysicalLicensed/PhysicalHero.prefab. Source and derived licensed art remain ignored; the repository contains integration code and the original portable fallback.
- Author through dicefree.physical.model.author in a stopped, clean Editor. This creates separate URP Lit material copies preserving the source texture/tint, retains the valid Humanoid Avatar, and retargets the existing owned Novice idle/locomotion/unarmed-attack controller. Vendor originals are untouched.
- Root motion is disabled; no presentation collider is added. Actor footprint, navigation, stats, skills, save schema and quest data are unchanged.

PhysicalBlessedVisual loads the local resource when first activated, hides the fallback body, and transfers existing Physical item sockets to matching bones of the new skeleton. The bronze dagger gains a Physical visual using its existing item artwork. Equipment remains the sole item and stat authority. The original form remains visible and animated if the local resource is absent.

ClassPresentationSwitcher selects the enabled animator after form activation. Portrait clones reuse the already prepared model rather than spawning duplicates. Portrait framing uses actual baked visible geometry, including equipment; it keeps the face in view after actor rotation.

## Validation

The final focused isolated model test passed 47 assertions: unavailable-resource fallback, valid Humanoid avatar, driver selection, URP materials, hidden helmet, no colliders/root motion, advancing idle frames, actual walking and driven speed, attack transition, dagger/shield/tophat/glove equip and unequip, new-skeleton attachments, portrait identity, class switching without duplicates, and unclipped baked portrait geometry after rotation.

Existing Physical Play Mode regression passed advancement, all five skills, Mana, durable menu reload and restored class state.

The actual Game view was inspected at gameplay zoom and a temporary close-up. The equipped capture shows the dagger, shield, slime gloves and tophat. Portrait framing was corrected for the taller model and character rotation.

- [Gameplay capture](screenshots/PhysicalModel-InGame.png)
- [Equipped close-up](screenshots/PhysicalModel-Equipped.png)
- Focused results: Library/PhysicalModelResults.json.
- Greatblade advancement quest: 35 assertions passed. The fixture now selects the active gameplay portal instead of an arbitrary decorative entrance.
- Greatblade combat: 75 assertions passed, including class/parent equipment switching.
- Ten data/architecture/managed-reference/XP/skill/presentation regression validators passed.
- Windows x64 Development build: seven scenes, Succeeded, 0 errors, 0 warnings, 344,658,024 reported bytes. Executable: T:\TEMP\DiceFree-Options\Builds\World1Foundation\DiceFree.exe.
- Non-graphical Windows startup smoke: StartMenu loaded, player responding and advancing frames after more than 23 seconds; no engine-startup failure. This does not substitute for graphical Windows playtesting.
- Committed verification records: docs/validation/PhysicalModel*-2026-10-14.json.

This is a playable art integration. The existing adaptive basic-attack animation remains shared across equipment types; bespoke weapon-specific animation and final cosmetic/transmog fitting remain future work. A full human playthrough has not been performed.
