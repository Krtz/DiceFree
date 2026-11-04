# Echo names and class titles

The player's visible name is the Echo's name followed by `the` and the current class definition's display name: `Krtz the Novice`, `Bob the Magically Touched Novice`, or `ChatGPT the Greatblade`. Existing class names are retained.

StartMenu has an Echo name field for new characters and a Save name control for existing Echoes. Names belong to the entire Echo, so every saved manifestation shares the name. Advancement and manifestation selection change the class title automatically. Greatblade follows this rule when its playable class is integrated; this change does not implement its combat.

The name is an optional `echo.identity` version-1 save section. The Echo and manifestation schema versions stay at 6; existing unnamed profiles display `Echo` until named. Renaming commits a cloned profile as a new atomic revision and retains every manifestation and opaque section. The runtime uses an optional Foundation identity contract instead of adding a Gameplay-to-Persistence assembly dependency.

The selected-unit inspector, overhead nameplate, stats header and StartMenu roster display the same name. Enemy and NPC labels retain their authored identity. Overhead nameplates measure their text width to accommodate the combined name and title.

## Earned quest honorifics — 2026-11-04

An optional `QuestDefinition.rewardTitle` now overrides the class suffix after that quest is durably Completed. The Chieftain's Bane quest awards `Chieftain's Bane` only when the player returns to Trimery's village elder and turns it in. A boss kill alone does not grant it. Live labels and the saved roster display `<Echo> the Chieftain's Bane`; roster cards also show class and level separately. Characters without an earned honorific retain the original class suffix.

The award derives from version-supported completed quest records and the `Resources/HonorificQuests` catalog. It requires no new save section or schema version. Existing manifestation-fork quest-history behavior remains authoritative. See `CHIEFTAINS_BANE_AND_PLAYER_MOTION.md` for implementation and validation.

Validation:
- Live Unity recompilation completed without compilation errors.
- Isolated `EchoIdentityCheck20261013` passed with `DICEFREE_ECHO_NAME_SAVE_OK`: unnamed-save default, trimmed Unicode name round-trip, preserved stable IDs and progression, opaque-section preservation, invalid-name rejection, and unsupported identity-version rejection.
- The isolated advancement Play Mode harness passed with `DICEFREE_ADVANCEMENT_PLAYMODE_OK`: named Echo creation, both first class titles, roster reload, renaming across all saved manifestations, and loading the renamed Physical character. Its fixture chooses unrestricted equipment and its final schema assertion follows `SaveMigrations.CurrentSchema`, replacing stale assumptions.
- No normal user save is used by either validation.
