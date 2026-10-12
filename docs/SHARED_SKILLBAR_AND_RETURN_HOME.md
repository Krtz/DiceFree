# Shared skillbar commands and Return Home — October 12, 2026

Branch: `feature/visual-overhaul-vis00`; implementation completed on user request.

## Shared action-bar layout

The existing movable **12-slot** `GenericActionBar` continues to show the four available class-specific active skills in slots 1–4. Three shared abilities occupy:

| Slot | Action | Binding | Art from locally licensed Battle Skill Icon pack |
|---|---|---|---|
| 5 | **Attack / Attack Move** | X | `S_Forward_steel_attack.png` |
| 6 | **Interact** | I | `S_fullsight.png` |
| 7 | **Return to Revive Stone** | R when alive | `S_Blue_Hole.png` |

The remaining five action slots remain available for future class-specific skills or other universal commands. The UI shows hotkeys, icon art, cooldown shading, hover tooltips and the Return cast countdown. Slot 5 delegates to the **same** player combat cursor as X, supporting hostile targeting, ground attack-move, Shift-queued commands and minimap targets. Slot 6 uses the same nearby world interaction function as I. Both preserve their established commands instead of introducing UI-only duplicate gameplay.

All three icons are original vendor PNGs installed **locally only** in gitignored `Assets/_DiceFree/Resources/AbilityIconsLicensed/`. The public repo contains `SkillIconLibrary` mappings and original code-drawn fallback sigils, not redistributed art.

The UI/Assembly-CSharp boundary is handled via `ISharedCombatCommands` on the existing `CombatInput`, rather than directly coupling the UI assembly to player-input classes.

## Return to Revive Stone (GitHub issue #38 is authoritative)

- Every class, including Novice and its two Tier I advancements, has Return available from its shared bar.
- **10-second rooted cast**. Attempting movement, Interact, Stop, Escape or Attack cancels; taking damage, dying, stun, active threat, or entering a dungeon cancels. Cancelled casts consume **no cooldown**.
- Cannot begin while **any enemy has active aggro on the player** (including a wolf/quillrat whose AI has acquired/chases the player but hasn't reached attack range), or during active dungeon occupancy.
- **10-minute cooldown** begins only after successful teleport. **Death, logout and scene/reload reset** the cooldown; no durable cooldown field is added to save data.
- The destination is the **most recently activated Dice revive/resurrection stone**, via `RespawnAtAnchor`. When the player walks within 3.5m of an active authored `ResurrectionAnchor`, the anchor is activated; `AnchorChanged` tells manifestation persistence to autosave the home ID. Future Village 2 stones are resolved by stable ID without hard-coding every village into the player prefab.
- While alive, **R casts Return Home**. While dead, **R retains the existing death-respawn/return behavior**; death is not a way of bypassing Return Home cooldown.
- Teleportation is an intentional travel skill, **not an unstuck**, collider-bypass or game-world collision recovery action. It doesn't refill HP, reset stats, alter equipment, or resurrect.
- The current pass uses the blue portal skill icon, an in-slot cast countdown/root and existing movement command cancellation. Additional in-world stone/channel VFX and sound are still an open presentation decision.

## Implementation

- `UI/AbilityBarSource.cs` / `UI/GenericActionBar.cs`: shared slots and activation, icon UI, countdown and tooltip.
- `UI/ISharedCombatCommands.cs` / `Characters/CombatInput.cs`: existing Attack cursor exposed to the HUD, living/dead R dispatch, target/ground attacks preserved.
- `UI/ReturnHomeAbility.cs`: rooted channel, damage/movement cancel, threat gate and transient cooldown.
- `Gameplay/IActiveThreatSource.cs` / `Application/IronpineWildlife.cs`: authoritatively expose active AI chase target for all wolves/quillrats.
- `World/RespawnAtAnchor.cs` / `Persistence/ManifestationPersistence.cs`: stone activation, save ID updates, safe alive teleport.
- `Characters/TraversalInput.cs`: movement/Stop/Interact/Escape cancel channel before issuing commands.
- `UI/SkillIconLibrary.cs` and `Tools/Editor/SkillIconAuthoring.cs`: three local pack icon bindings, original fallback sigils, importer/validator now checks 17 icons.

## Tested

- `unity recompile` on the connected main project: zero compiler errors and warnings.
- `dicefree.skill-icons.validate` on the main project: 17 unique textures and valid class icon mappings.
- Isolated Unity Play Mode: `COMMON_COMMANDS_PLAYMODE_OK`, verifying slot identity and actual art, Attack cursor, saved revive stone activation, 10-second rooted channel, cancellation with no cooldown, teleport near the last activated anchor, 600s cooldown, reset on death, and no post-teleport healing.
- Existing full action-bar/HUD validation was already green; repeat actual mouse/keyboard Game-view testing at ordinary HUD scale.

## Design follow-up

Return channel VFX, stone activation glow/sound, announcement and cooldown presentation may be polished after playtesting. Do not change the settled 10s cast or cooldown rules without explicit user decision.

Do not merge to `main` without approval. Preserve unrelated local VFX materials, PackageManager/ShaderGraph settings and untracked audio metadata.
