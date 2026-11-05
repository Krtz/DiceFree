# Loadout hand poses and wolf bounty — November 5

Branch: `fix/loadout-hand-poses-wolf-bounty`, based on `1b0fb07`. All 54 pre-existing modified tracked files are preserved byte-for-byte. The work changes cosmetic posing and the existing wolf quest; existing item permissions and two-handed/offhand exclusion remain authoritative.

## Equipment drives the pose

`Equipment.ResolvedDefinition` and `UsesTwoHandedStance` expose the usable loadout. Explicit main-hand item metadata takes priority; Greatblade's unequipped default greatsword remains two-handed. Equipping a two-handed item still clears the offhand through the existing equipment rules.

Greatblade now uses its owned single-handed motion controller when a single-handed main/offhand loadout is selected, and the installed two-handed motion override for a two-handed/default greatsword. Switching stance refreshes the controller and HUD preview. An independent offhand pose holds a resolved shield/focus on its own side, before item attachments follow the bones. This does not move the actor, change attack timing or retune damage.

Hidden Wolf-Fang/default greatsword helpers no longer move either hand. The active two-handed Wolf-Fang grip fits the handle inside both arms' reach before solving the hands. Physical and Magical model setup no longer clones the procedural sword from another class with stale rig references; each form's sword binds to its own animator. Greatblade's prepared model reference is remapped when cloned, preventing a second default body/weapon in the HUD.

HUD bone poses run after animation every frame, followed by held props. Menu previews use the same loadout choice and apply bone poses before props. No new commercial clips or packages are introduced, and no original animation assets are rewritten.

## Another 15 Wolves

The existing quest remains `quest.cornberg.repeat-wolf-hunt`, offered by **Mira in Cornberg**. It requires **15 total Ironpine wolves**, with regular and Alpha Wolves sharing the same counter. Turn-in grants **2,920 XP**, a fixed reference of twenty regular level-15 Grey Wolves at 146 XP each. Alpha and higher-level wolf kill XP still vary normally; the bounty is not a dynamic sum of the fifteen kills.

The old Slime Surge completion gate is removed. Quest dialogs now let the player choose between available, active and ready offers from the same NPC, so an active main quest or earlier repeatable offer cannot hide the wolf hunt. World interaction labels hide while the dialog is open. The stable ID, version, saved counter and repeat/reset behavior are retained; no objective migration is necessary.

## Evidence and limits

`HandPoseWolfReview` passed **415** checks in a disposable Cornberg profile. All five classes switched **main/offhand → two-handed → main/offhand**. Checks cover independent hand separation, inactive-helper no-ops, rig-local sword references, walking leg motion, actual connected basic attacks, HUD framing, one Greatblade HUD model and read-only menu poses. Measurements are taken after rendering, because ordinary async continuations can run before LateUpdate fitting. Observed two-handed support-hand error was below 1 cm across the five forms.

The wolf review used scripted fixture deaths and restored enemy lives, including both normal and Alpha families. Two actual accept/turn-in cycles each awarded exactly 2,920 XP, duplicate turn-ins were rejected, the quest reset, and a four-kill partial cycle survived a class save/reload. A separate restored active-main-quest record exercises choosing the wolf offer alongside another active quest. Class creation, item grants, placement and maintained attack orders are test fixtures, not progression or combat-balance acceptance.

Clean closer-camera world views and menu renders are under `VisualBaselines/HandPoseWolf`. The Windows smoke additionally checks the real offer selector and Greatblade/Mage hand loadouts, alongside the existing milestone flows. Final build, exit, preservation and static-check records are in `docs/validation/hand-poses-wolf-bounty-20261105.json`.

The seven-scene Windows development build at `Builds/BriarbackMilestone-20261105-111202/DiceFree.exe` succeeded with zero errors and zero warnings (450,233,471 bytes). Its disposable-profile smoke passed the new offer/loadout checks, existing loading, presets, quests, boss, class skills, anchor return, resume and Development Room flows, and eight fort walks covering 216.6 m. The player quit through the normal menu with exit code 0; the runtime error-pattern scan found no matches. Five standalone captures are saved under `VisualBaselines/HandPoseWolf/Windows`. Architecture, managed-reference and dependency-policy checks passed. All 54 original modified tracked files and seven protected build settings were verified byte-identical after the build.

No push or merge is included.
