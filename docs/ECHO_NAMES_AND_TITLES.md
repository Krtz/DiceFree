# Echo names and class titles

The player's visible name is the Echo's name followed by `the` and the current class definition's display name: `Krtz the Novice`, `Bob the Magically Touched Novice`, or `ChatGPT the Greatblade`. Existing class names are retained.

StartMenu has an Echo name field for new characters and a Save name control for existing Echoes. Names belong to the entire Echo, so every saved manifestation shares the name. Advancement and manifestation selection change the class title automatically. Greatblade follows this rule when its playable class is integrated; this change does not implement its combat.

The name is an optional `echo.identity` version-1 save section. The Echo and manifestation schema versions stay at 6; existing unnamed profiles display `Echo` until named. Renaming commits a cloned profile as a new atomic revision and retains every manifestation and opaque section. The runtime uses an optional Foundation identity contract instead of adding a Gameplay-to-Persistence assembly dependency.

The selected-unit inspector, overhead nameplate, stats header and StartMenu roster display the same name. Enemy and NPC labels retain their authored identity. Overhead nameplates measure their text width to accommodate the combined name and title.

Validation:
- Live Unity recompilation completed without compilation errors.
- Isolated `EchoIdentityCheck20261013` passed with `DICEFREE_ECHO_NAME_SAVE_OK`: unnamed-save default, trimmed Unicode name round-trip, preserved stable IDs and progression, opaque-section preservation, invalid-name rejection, and unsupported identity-version rejection.
- The isolated advancement Play Mode harness passed with `DICEFREE_ADVANCEMENT_PLAYMODE_OK`: named Echo creation, both first class titles, roster reload, renaming across all saved manifestations, and loading the renamed Physical character. Its fixture chooses unrestricted equipment and its final schema assertion follows `SaveMigrations.CurrentSchema`, replacing stale assumptions.
- No normal user save is used by either validation.
