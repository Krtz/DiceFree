# Channel bars and overhead unit HP — 2026-10-13

**Implementation:** `feature/visual-overhaul-vis00`. Runtime UI auto-installs from `HudLayoutManager.Awake`; no Cornberg scene serialization or character save changes are required.

## Generic channel framework

- New `DiceFree.UI.IChannelingAbility` interface: `IsChanneling`, `ChannelLabel`, `ChannelDuration` and `ChannelRemaining`. Future channeled spells implement the same read-only contract on their caster. Spell mechanics (interruptions, rooted movement, cooldown, valid target/destination) remain owned by the actual spell, never the HUD.
- `ReturnHomeAbility` now implements the interface, exposing its existing 10-second **Return to Revive Stone** channel. No gameplay change to the settled 10-second cast, 10-minute cooldown, death reset, aggro/dungeon veto, cancellation or teleport destination.
- New `ChannelBarHud`: theme-aware **movable/resizable HUD channel bar** near the central lower HUD, automatically displays spell title, fill as elapsed portion of total cast duration, and remaining seconds. It hides immediately upon cancellation, interruption or completion; does not block world clicks while hidden. Shows a placeholder in HUD layout-edit mode so you can move/resize it; uses standard theme assets.
- An additional **small channel progress bar above the unit's head** appears for any player, ally or enemy implementing the interface. It remains visible even when that unit's HP bar is set to Never. Future boss channels are therefore legible without new rendering code.

## Overhead HP

- New `OverheadUnitHealthBars` centrally projects accurate *current/maximum* HP bars above **all living combat units** (self, allies, enemies), proportionally colored cyan / green / red with compact numeric values.
- Respects world fog visibility and distance/frustum culling: does **not expose undiscovered monsters**. Height follows the character model/capsule with a short cache, including wolves and other differently sized assets. Performance is one lightweight centralized GUI pass.
- Drawn behind interactive HUD; does **not** create world colliders or intercept clicks.
- Existing `ActorFeedback` still draws actor name/level, damage float numbers and respawn countdown. Removed duplicate HP numbers from those nameplates to avoid two independent HP displays.

## Options → HUD / Themes

**Three independent settings**, each with **Always / Only when hurt / Never**:
1. Your character — default **Only when hurt**
2. Allies — default **Only when hurt**
3. Enemies — default **Always**

'Only when hurt' means HP below maximum; returns to hidden at full HP. 'Never' hides only the HP bar, not an active channel indicator. Settings apply immediately and survive application restarts.

- Stored in existing `GameplayPreferences` JSON (`Settings/gameplay.json`), bump from version 1 to **2**.
- Version-1 settings load with their original auto-retaliate, cast range and targeting behavior intact. Missing health bar settings are explicitly assigned new defaults. No Echo save schema change.
- The preferences are saved atomically, with the existing .bak behavior.

## Source

- `Assets/_DiceFree/Scripts/UI/IChannelingAbility.cs`
- `Assets/_DiceFree/Scripts/UI/ChannelBarHud.cs`
- `Assets/_DiceFree/Scripts/UI/OverheadUnitHealthBars.cs`
- `Assets/_DiceFree/Scripts/UI/ReturnHomeAbility.cs`
- `Assets/_DiceFree/Scripts/UI/HudLayoutManager.cs`
- `Assets/_DiceFree/Scripts/UI/OptionsPanel.cs`
- `Assets/_DiceFree/Scripts/UI/ActorFeedback.cs`
- `Assets/_DiceFree/Scripts/Input/GameplayPreferences.cs`
- `Assets/_DiceFree/Scripts/Tools/Editor/HealthChannelUiValidation.cs`

## Verified

- Main Unity 6000.6.3f1 recompilation: **zero errors, zero warnings**.
- `unity command dicefree.ui.health-channel.validate`: **DICEFREE_HEALTH_CHANNEL_DATA_OK** — all 3 setting rules, teleport contract, legacy v1 settings migration preserving other settings, v2 save/reload round-trip, run with disposable temporary settings directory.
- Isolated Unity Play Mode `HealthChannelPlaymodeProbe` (validation sandbox, separated Echo/settings paths): **DICEFREE_HEALTH_CHANNEL_UI_OK**. Verified actual runtime HUD installation, target modes, full/hurt/never logic, successful Return Home channel, visible channel progress, interruption clears and releases click-blocker, fake future channeled ability also displays through generic interface.
- Existing main `dicefree.hud.validate` and `dicefree.ui.skills.inspect` passed after integration.
- Previous pending **`dicefree.xp.quadratic.validate` also passed** after syncing the previous GitHub-only XP changes to this local project (schema v6; no active saves altered).
- The isolated headless Unity editor sometimes logs an unrelated `UnityEditor.Search.SearchDatabase ArgumentOutOfRangeException`; both validation markers still passed.

**Manual Game-view QA remains:** check HP/head alignment for very large Alpha wolf, slimes and minimap scaling; check channel gauge overlap at minimum HUD scale, 1080p and high resolution; visually inspect the actual packaged HUD theme art. The automated tests verify runtime state and data, not full graphical quality.

No merging to `main` without approval. Preserve locally licensed art, unsaved shader/VFX work and ProjectSettings; do not commit unrelated changes.
