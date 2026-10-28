# Resurrection dice validation — 2026-10-28

Package: `pkg_54772145534bcf4795d23561` (CosmicWonder, Simple Boardgame Dice, CC-BY-4.0).

- `game-dev package verify`: closed manifest hashes pass before admission and after Unity integration.
- `game-dev vendor admit`: no blockers; copied package hashes verified; lock and receipt retained.
- Unity 6000.6.3f1 compilation completed. `DiceAnchorAuthoring.Validate` confirms both scene anchors and their imported mesh/pip wiring.
- `DiceAnchorAuthoring.CheckPlay` passed in an isolated save: two saved-anchor landings, two Return Home calls, two selections, four pip states, four light states, four emission states. Both selected/inactive combinations were captured through actual Unity cameras.
- Architecture and architecture-policy validators passed against working source.
- Existing anchor transforms/ranges and collision/NavMesh preserved. Cornberg recovery scene saved locally before authoring. Unrelated pre-existing WIP remains unstaged.
- Canonical package files use a scoped `-text` Git attribute so checkout line-ending conversion cannot invalidate the hash roster. Unity derivative digest and provenance are in `SourceArt/ResurrectionDiceUnityDerivative.json`.

Review captures: `VisualBaselines/ResurrectionDice/anchor.cornberg.png`, `anchor.trimery.png` (Edit Mode), and corresponding `.active.png` / `.inactive.png` captures (isolated Play Mode). Existing runtime scene lighting is visible; these close-ups do not establish normal gameplay-distance readability or final art approval.

No Windows build, multiplayer test, or actual death/respawn timing test was performed.
`dicefree.managed-references.validate` also passed (76.06 seconds, including local recovery scenes).
