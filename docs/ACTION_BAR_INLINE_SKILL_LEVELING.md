# Action Bar v2: inline skill leveling, passives, and commands — 2026-10-12

Implementation branch: `feature/visual-overhaul-vis00`.

## The 12-slot bar

The existing movable/scalable `GenericActionBar` remains a two-row, six-column action bar. Its in-order (1-based) layout is:

| Slot | Meaning | Interaction |
|---|---|---|
| 1–4 | Four current active class skills | Cast/cursor-target through existing abilities; hotkeys 1–4 (remappable) |
| 5 | Current class passive/aura | **Never casts**, but always shows skill artwork, rank, passive marker, and complete rank-dependent hover description |
| 6 | Future class skill / another aura / passive | Intentionally empty until designed |
| 7 | Attack / Attack Move | Original X command (including Shift queue and minimap targeting) |
| 8 | Interact | Original I command |
| 9 | Return to Revive Stone | Original R living-Return spell, 10s rooted cast and 10m cooldown |
| 10–12 | Future actions | Empty (no misleading placeholder key numerals) |

This preserves the three playable classes' original data-driven active identities and skill definitions:
- Novice: STR Melee Stun, Magic Sand, AGI Attack Speed Buff, SPI Heal, plus *All-Stat Passive* in slot 5.
- Physically Blessed Novice: Heavy Strike, Guard, Quickening, Arrow Rain, plus *Martial Aptitude* passive in slot 5.
- Magically Touched Novice: Magic Sand, Mend, Fire Imbuement, Ice Burst, plus *Mana Attunement* passive in slot 5; its team mana-regeneration aura is part of the passive and shown in the hover details.

The class-specific icons use the previously installed 17 locally licensed Battle Skill Icon images when present, falling back to the game's original generated symbols on portable builds. Nothing is copied from an Asset Store package into public GitHub source.

## Skill points are spent ON the icons

- Skill points continue to come from the existing progression rank allocator: novice/physical/magical `UnspentPoints`, `Spend(stableId)`, `Changed` event, save/load, class-specific rank caps and stat/buff computations. **No duplicate skill-point store or save format**.
- Whenever `UnspentPoints > 0` and a class skill is below max rank, a green **+** appears in the **top-right corner** of its action-bar slot. It remains available even when the skill's rank is zero (unlearned).
- Click **+** to spend one point and learn/upgrade exactly that skill. The icon's rank display updates immediately; the green point-count reminder above the action bar decreases.
- Hovering **+** previews the NEXT rank using the actual `SkillTooltips.Describe` curves; hovering anywhere else on the icon still shows the current skill's properties and current rank. Passive and aura descriptions work even at rank zero.
- Upgrade clicks and regular skill casts have distinct, non-overlapping IMGUI hit regions; buying an upgrade **cannot accidentally cast** the skill.
- Passive icon is not a cast button and does not respond to active ability hotkeys. Shared command slots cannot upgrade or consume points.
- Maximum rank hides the **+**; when no unspent points remain, all **+** markers and the available-point reminder disappear.
- Existing Novice/Physical/Magical standalone skill allocation window components are retained solely for backward serialization compatibility but **no longer open, block pointer input, or appear in the HUD editor**. The obsolete `Novice skills menu` input action is retained for existing input binding saves but hidden from the Options keybind list.
- Class-specific tooltips no longer tell the player to use the old skill-allocation window.

## Main code

- `Assets/_DiceFree/Scripts/UI/AbilityBarSource.cs` — `View(0..8)`, passive identification, shared command positions, `canUpgrade`, `UnspentPoints`, `Upgrade`, and `UpgradeTooltip`. All rank and skill IDs resolve from the active class.
- `Assets/_DiceFree/Scripts/UI/GenericActionBar.cs` — real art, + overlays/hit regions, current rank badges, PASS/PASSIVE marker, count reminder, cooldown and Return cast shading, and hover previews.
- `NoviceSkillPanel.cs`, `PhysicalSkillPanel.cs`, `MagicalSkillPanel.cs` — old popups retired without removing serialized scene components.
- `SkillTooltips.cs` — updated Novice guidance; `OptionsPanel.cs` hides obsolete keybind.
- `Tools/Editor/SkillAndMinimapValidation.cs` — editing/runtime assertions expect in-bar allocation and no second skills window.

## Validation

- Current main Unity project: `unity recompile` **0 errors / 0 warnings**.
- Main editor `dicefree.ui.skills.inspect`: **15 definitions** with complete rank-dependent hover tooltips, obsolete menus nonblocking and hidden.
- Separate isolated Unity 6000.6.3f1 Play Mode with `-diceFreeSaveRoot T:/TEMP/DiceFree-AssetCatalog/PlaymodeSaves/SkillbarRevamp20261012` (user's actual profile not modified): `DICEFREE_ACTIONBAR_REVAMP_PLAYMODE_OK`.
- Play Mode explicitly exercised **all three classes**: each has four active icon views, one passive/aura icon with tooltip, rank-up through the shared `Upgrade(4)` and `Upgrade(0)` methods, correct decreasing unspent points, no castable passive, empty slot 6, and fixed 7–9 commands with real asset icons.
- Existing licensed skill-icon validator: 17 real local textures confirmed and shared class mapping intact.
- **Manual next check:** view at 1080p/1440p and minimum HUD scale, hover/click the + and passive icons, verify tooltip placement and color contrast. Unity's unrelated headless editor SearchDatabase indexing exception may appear in isolated logs without failing the test.

New Village 2, Tier II and wolf wave dungeon work is still design-only while the skill system gets this overhaul. Do not merge to `main` without user approval or commit any unrelated local Unity settings/assets.
