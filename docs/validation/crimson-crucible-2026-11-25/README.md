# Crimson Crucible evidence

- [137-check runtime report](runtime-review.json): complete solo and two-member gauntlets, attack packets, dodges, gate navigation, equipment and treasury.
- [17-check delivered-build lifecycle report](delivered-lifecycle.json): disk reload, stun, wipe, revive, fresh attempt and menu cleanup.
- [Build report](build-report.json): final Windows development player, 15 scenes, zero errors, one pre-existing collision-prebake warning.
- [Delivery receipt](delivery.json): exact executable path/hash, build identities and main import result.
- [Source receipt](source-receipt.json): 141 matching main/QA asset and source hashes.
- [Authoring summary](authoring.json), [loot coverage](loot-coverage.json), and [equipment audit before additions](equipment-before.json).

![Final entrance](entrance.png)

![Six boss cells during special windups](six-arenas.png)

![Level 55 rewards](loot-matrix.png)

World screenshots come from the gameplay camera rendered explicitly in a batch-mode player. They omit screen-space overlay UI. The six-cell sheet is from the full review build; the entrance and loot icons are final delivery assets. Automated local party coverage and accelerated boss kills are not manual balance acceptance or an online co-op test.

Reproduce using disposable copied saves, the local Unity CLI and `tools/validation/CrucibleReview.cs` / `CrucibleLifecycleReview.cs`. `tools/authoring/CrimsonCrucibleAuthoring.cs` authors the scene once; it rejects duplicate overworld entrances. `CrucibleLootAuthoring.cs` can refresh loot and icons. `PackageCrucibleEvidence.ps1` assembles these reports and verifies the source receipt. Read the Unity CLI skill before running these tools.
