# Trimery healer placeholder — 2026-10-17

## Scope
User reported no healer in Trimery and requested a placeholder NPC, followed
by designing Tier II Priest and an advancement quest from that healer.

Implemented: the living-player HP service and first-party placeholder art.
Design only: Priest kit, Grace spending and advancement trial. Do not claim
the class or quest is available in this build. See
PRIEST_TIER2_PROPOSAL_2026_10_17.md for confirmed rules versus proposals.

## In game
- Ivory-and-blue rounded priest blob with eyes, gold emblem and wooden staff.
- Beside the village green at x244/z190, facing the north road.
- Roadside interaction point x246/z190.
- Label: Trimery Priest - free healing.
- Right-click to approach/treat, or use the existing nearby interaction key.
- Free HP treatment for living faction-0 players through Health.Heal.
- No resurrection, gold charge, Mana/Grace refill or class unlock.
- No automatic healing aura. Existing resurrection stone stays unchanged.
- Heals use normal received-healing modifiers and presentation events.
- Existing shields and statuses are not cleared.
- Visual-only breathing/idle reuses VillageVisualIdle.
- Layer-10 selection capsule; no solid-world/nav obstacle layer-9 geometry.
- Original primitive art/materials, no new third-party asset or license.

## Implementation
- World/HealerInteraction.cs uses existing range, life and line-of-sight gates.
- TrimeryHealerAuthoring installs only its own named root and materials.
- The original Trimery installer includes this service on a fresh village.
- No scene/prefab/asset YAML hand edits; scene authored using Unity Editor APIs.
- Semantic scene comparison against HEAD: 28,720 original serialized blocks,
  zero removed; 42 added and one existing parent child-list changed.
  Unity's textual diff also reorders existing blocks; no old objects deleted.

## Live verification
dicefree.trimery.healer.test passed 48 checks in an isolated save:
- Actual frame progression: nine frames during the sampled half-second.
- Five actual walking checkpoints from the green to treatment and back.
- Registry membership, right-click ray selection and the same nearest-target
  interaction path used by the keyboard binding.
- Damaged living player healed to full; normal heal presentation event fired.
- Full HP does not emit a fake heal.
- Existing temporary shield retained.
- Hostile faction excluded; disabled healer excluded and re-enabled membership
  restored; out-of-range calls do not heal; no passive healing area.
- Actual lethal damage fixture stays dead after treatment: no resurrection.
- Supported URP materials, click layer, navigation layers and nonblank render.
- Actual main-camera village render inspected: Library/Trimery-Healer.png.
This is focused runtime verification, not a new full game playthrough.
The screenshot is a camera render; overlay interaction labels are not included.

Reproduce:
- dicefree.trimery.healer.install (Edit Mode; saves Cornberg)
- dicefree.trimery.healer.test
- dicefree.trimery.healer.status

Build/startup and repository receipt recorded below after completion.

## Windows build
Seven-scene Windows Development build succeeded: zero errors, zero warnings.
Executable: Builds/World1Foundation/DiceFree.exe; use the complete folder.
The player remained running for a 15-second isolated batch/nographics startup
smoke with zero matched runtime errors. This is not a graphical playthrough.
Evidence: docs/validation/TrimeryHealer-WindowsBuild.json and
TrimeryHealer-WindowsStartup.json.

Work is on integration/world1-foundation; main unchanged. Existing unrelated
PackageManagerSettings.asset and Assets/Settings* are preserved and excluded.

Post-build regressions also passed: architecture ownership, architecture policy
self-tests, managed-reference compatibility, Trimery village validation and
Briarback region validation. JSON receipts are stored with the other evidence.
