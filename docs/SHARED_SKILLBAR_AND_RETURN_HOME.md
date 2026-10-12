# Shared skillbar commands and Return Home — October 12, 2026

Branch: `feature/visual-overhaul-vis00`; implementation completed on user request.

## Shared action-bar layout (updated with Action Bar v2)

The movable **12-slot** `GenericActionBar` has four active class skills in slots 1–4, a passive/aura in slot 5 with a rank-dependent tooltip, and a reserved slot 6. Three permanent shared actions occupy **slots 7–9**:

| Slot | Action | Binding | Local Battle Skill Icon pack artwork |
|---|---|---|---|
| 7 | **Attack / Attack Move** | X | `S_Forward_steel_attack.png` |
| 8 | **Interact** | I | `S_fullsight.png` |
| 9 | **Return to Revive Stone** | R when alive | `S_Blue_Hole.png` |

Slots 10–12 remain empty for future abilities. The action bar shows hotkeys, art, cooldown shading, hover tooltips and the Return cast countdown. Slot 7 delegates to the **same** player combat cursor as X, supporting selected/ground targeting, Shift-queued attack moves and minimap targeting. Slot 8 uses the existing nearby object/NPC/chest interaction system.

**Skill allocation is now done in the action bar:** green '+' upgrade controls appear over learnable active skills and passives, with the normal class skill-point budget. No separate skill-allocation popup. See `docs/ACTION_BAR_INLINE_SKILL_LEVELING.md` for the complete update.

All three icon PNGs are installed **locally only** under gitignored `Assets/_DiceFree/Resources/AbilityIconsLicensed/`; portable code-generated alternatives remain. The UI/Assembly-CSharp boundary uses `ISharedCombatCommands`, rather than referencing character-specific input from a UI assembly.

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
