# Dragon loot and World Two expedition validation

Unity 6000.6.3f1; Windows development player; disposable copied Knight save.

- `build-report.json`: all 14 scenes; succeeded, zero errors, one warning.
- `gameplay-review.json`: 50 passed runtime checks; no Unity errors during the
  review. Uses real command entry points and interaction workers; boss/enemy
  damage is injected to keep the scenario bounded.
- `reload-review.json`: separate actual player restart, saved reward instances
  and equipment, restored active/completed quest states, and final disk flush.
  The survey was intentionally reset solely for the preceding visual review.
- `loot-authoring.json` and `expedition-authoring.json`: authored stable IDs,
  counts and checked interaction positions.
- `source-receipt.json`: existing library inputs and their SHA-256 hashes.
- `main-import.json`: main-project import/compile result and log summary.
- `dragon-rewards.png`: six item icons, labelled by role.
- `cinderwatch-world.png`, `willowford-world.png`, `needle-world.png` and
  `crown-world.png`: direct renders from the exploration camera at distance 24.
  These exclude screen-space overlay UI. Initial automated screen-buffer
  captures were black in batch mode and are not included as evidence.

The tests also exercise the unchanged rare/single/double dungeon roll branches
with 6,000 deterministic draws. This is a bounded gameplay and persistence
check, not multiplayer or balance acceptance. Existing tree LOD registration
warnings remain outside this change.

Reproduction scripts live in `tools/validation/DragonExpeditionReview.cs` and
`DragonExpeditionReloadReview.cs`; invoke them with Unity CLI `eval_file` on the
corresponding isolated development-player save. The reload script expects the
post-visual-review state recorded above, not a user's normal save.
