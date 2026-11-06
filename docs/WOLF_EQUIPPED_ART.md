# Wolf dungeon equipment — 2026-11-06

The six wolf dungeon drops now have original equipped models matching their illustrated inventory identities. The models appear on the active Humanoid, in the HUD portrait and in saved-character menu previews.

| Item | Equipped presentation |
| --- | --- |
| Moonfang Signet | Silver band with a small wolf-head setting and blue stone eyes, following the right middle finger |
| Nightpelt Vest | Fitted dark leather cuirass, layered irregular grey fur collar and hem, crossed lacing, silver rivets, fang clasps, wolf-face chest medallion and blue belt stone |
| Moonwell Staff | Full-length leather-bound shaft, silver crescent around a carved wolf head, green moonstone, fur binding and hanging fangs; two actual hand grips |
| Starfall Wand | Silver wolf face, blue faceted moonstone in three claw prongs, fur grip and suspended fangs; one hand |
| Packhunter's Fang | Swept canine blade with a raised silver ridge, dark fuller, wolf-face guard, curved ivory fang guards and fur pommel; one hand |
| Duskwake Wand | Ivory canine skull with recessed dark eye sockets, violet stone in hooked prongs, fur grip and hanging fangs; one hand |

## Production and integration

The meshes and small repeating fur/leather surface textures are authored in native Unity C# in `WolfRewardEquipmentVisuals.cs`. They are original project geometry, not new third-party assets or paid-provider outputs. Each item combines its sculpted solids into one mesh, with shared palette materials per presenter. Geometry has no colliders and uses the Ignore Raycast layer, so it cannot interfere with targeting or navigation. Runtime meshes, textures and materials are released with their presenter.

`PlainEquipmentVisuals` prepares the set for each active form and for the existing read-only menu stage. Physical and magical licensed-model adapters leave these procedural items to the new presenter, avoiding cloned attachments that reference another class's skeleton. Repeated preparation does not duplicate props.

`EquipmentPresentation` remains the sole visibility authority. Item IDs, stats, eligibility, icon PNGs, normal drop odds, reward choices and save schema are unchanged. Main-hand items suppress the class's default weapon; one-handed wands retain an independent focus. Nightpelt replaces the Mage's procedural chest coat while retaining the hat and cape.

One-handed weapons follow the animated wrist from their fitted rest grip. The staff fits both grips within each arm's inner and outer reach before solving the arms, then re-measures the imported rig's nonuniform scale across five fitting passes. The same pose interfaces run before bone-following props in HUD and menu previews. Source provenance/hashes are in `assets/wolf-equipped-art.json`.

## Verification

`WolfEquippedReview.cs` uses an isolated profile to check all six models on five playable forms, preserved equipment eligibility, zero prop colliders, idempotent preparation, dominant/support hand fitting, class-default suppression, saved full Mage outfit, actual menu portrait and saved resume. It captures both gameplay-distance and closer views in `VisualBaselines/WolfEquipped`.

`WolfEquipmentMotionReview.cs` checks actual walking and basic attacks with the Physically Blessed dagger, Magically Touched staff and Mage wand, including rendered-frame attachment errors. `WindowsWolfEquipmentSmoke.cs` exercises the actual standalone models on five forms, the unchanged ten-wave dungeon and item claim, and saved equipped armour/ring reload in an isolated QA profile.

Final results:

- Editor equipment review: **210 checks passed**, six models, five forms, actual equipped menu portrait and saved resume, zero Unity errors.
- Motion review: three real walking/attack fixtures passed, 12 rendered attack samples each. Maximum dominant attachment error was zero; staff support error was **0.000128m**. Dagger/wand rotation followed the attack wrist.
- Architecture, managed-reference, assembly-ownership and dependency-policy checks passed.
- Windows build `Builds/BriarbackMilestone-20261106-195401/DiceFree.exe`: eight scenes, 476,426,989 bytes, **zero errors and warnings**. The isolated standalone smoke passed all five forms, six model resources, normal ten-wave reward claim and saved equipped reload, with zero Unity errors. Normal Quit exited PID 27324 with code 0.
- All 54 pre-existing tracked changes, seven protected settings and six original item-definition files retained their original hashes. Scene/navigation assets and recovery backups were not modified.

Combined evidence is in `VisualBaselines/WolfEquipped/validation.json`, alongside curated standalone weapon, gameplay and saved-menu captures. Work is local on `art/wolf-reward-equipped-identity`; nothing is pushed or merged.

Natural full-clear dungeon balance remains a separate playtesting task; scripted dungeon clears do not establish class balance.
