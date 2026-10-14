# Magically Touched cloth caster

Magically Touched now uses the approved Polytope free modular male human, selected cloth00 body/legs/gauntlets/boots, head01, hair20 and short 01_A cape. Indigo cloth, violet cape, dark hair and worn boots distinguish it from the broader Physically Blessed starter armour. A modest Adventure Forge Fantasy Staff Lite is held as its baseline weapon.

The live idle-pose measurement is **1.647 m** tall versus Physical's **1.843 m**: about **11% shorter**. Shoulder bone span is **0.295 m** versus **0.405 m**: about **27% narrower**. These are visual measurements; character movement, gameplay colliders, reach and class statistics are unchanged. No player dye system was introduced.

## Integration

- MagicallyTouchedVisual loads the optional ignored MagicalLicensed/MagicalHero resource once per form. The original authored presentation remains the fallback when that local resource is absent.
- Existing Humanoid controller drives idle, walking and attack. Root motion is disabled; presentation has no colliders.
- Source FBX already imported with a valid Unity Humanoid avatar. Derived prefab uses supported URP/Lit materials. Source models remain untouched.
- Existing gear sockets transfer to the new rig; bronze dagger receives a Magical binding. The plain test gear is generated for the active rig as before.
- Equipping a mainhand hides the baseline staff; unequipping restores it. Back-slot gear replaces the baseline cape. Equipment remains the sole state authority.
- PlainGearAttachment now has its own Unity script file so authored staff attachments survive prefab serialization. Portrait cloning remaps the bone reference and applies the same pose hook used for plain gear.
- No save-schema, kit, class-advancement or resource tuning changes.

Authoring: dicefree.magical.model.author. Do not regenerate the form through the older MagicalTouchedAuthoring presentation builder, which would replace the broader class integration.

## Provenance and licence

Selected members were copied from the user's owned Unity Asset Store package cache after archive/member SHA-256 verification, with a closed path/GUID/hash roster and post-copy byte verification. No vendor scripts or paid provider jobs were admitted. The local game-dev CLI was unavailable, so direct verified archive-member admission was used; this is not a claim of a Game Development Studio lock receipt.

Both public publisher listings identify the standard Unity Asset Store EULA. Source packs and derived models/materials remain ignored, matching the existing Physical/Greatblade policy. Only implementation, metadata receipts and rendered screenshots are committed. [Source receipt](assets/Magical-source-receipt-2026-10-14.json).

- [Polytope source](https://assetstore.unity.com/packages/3d/characters/low-poly-modular-armors-free-lowpoly-medieval-fantasy-series-199890), archive SHA-256 2876c4f7a65ae86b2c6708f6254042ced2cd788bd6128980ac49b8986784ee73.
- [Adventure Forge staff](https://assetstore.unity.com/packages/3d/props/weapons/fantasy-staff-lite-54999), archive SHA-256 df3bd8aeb3db102cba2f63bb37d4bb6f47e32b5749abcc10cc23d89c53ba68bb.

## Validation

Focused isolated model test passed **78 checks**: unavailable-source fallback, actual smaller body/shoulder measurements, valid animator, supported materials, missing-script/collider checks, ticking idle, walking and attack, eleven fitting slots, staff/cape suppression/restoration, bronze dagger on the new rig, class switching without duplicate art and unclipped body in the rotated portrait.

The default Game view was inspected. The comparison image is an Editor asset preview of the derived models at their authored scales, with Physical on the left and Magical on the right. It is separate from live gameplay measurements and the Game-view screenshot.

Existing gameplay regressions and the final Windows build are recorded alongside this document after completion. No full human playthrough, final weapon-specific animation or final art fitting claim is made.

The existing five-skill/Mana/save/menu-reload Magical suite passed, the full all-class gear/flexible-skills suite passed 232 checks, and all ten data/architecture/progression regressions passed with the final model implementation.

Final Windows Development build succeeded: seven scenes, 0 errors, 0 warnings, 348,117,424 reported bytes. Executable: T:\TEMP\DiceFree-Options\Builds\World1Foundation\DiceFree.exe.

Standalone startup smoke passed: the isolated non-graphical Windows player remained responsive in StartMenu after 22 seconds with advancing frames. This confirms startup only, not graphical standalone gameplay or performance.
