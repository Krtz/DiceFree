# Plain test equipment and flexible skill kits

Cornberg's existing merchant now sells one plain item for each of the eleven equipment slots, **100 gold each** (1,100 gold for the full set). The existing bronze dagger offer is retained. Buy and sell lists scroll. No items or gold are granted to normal players.

All plain items are common, level 1, unrestricted by class, unbound and sellable. Development Room also exposes them through its existing grant-item list. Original primitive fitting visuals follow each active class's humanoid bones and appear in the portrait. They deliberately prioritise visible slot coverage over finished art.

| Slot | Item | Basic bonus |
|---|---|---|
| Head | Plain Cap | +1 INT |
| Shoulders | Plain Shoulder Pads | +1 STR |
| Chest | Plain Breastplate | +1 physical defence |
| Hands | Plain Cuffs | +1 AGI |
| Legs | Plain Leg Pads | +1 VIT |
| Feet | Plain Boots | +1% movement speed |
| Main hand | Plain Sword | +1 basic attack damage |
| Off hand | Plain Buckler | +1 physical defence |
| Ring | Plain Ring | +1 SPR |
| Amulet | Plain Pendant | +1 magical defence |
| Back | Plain Short Cape | +1 VIT |

The explicit modelTestingItem flag permits this fitting buckler on Greatblade so every slot can be tested on every class. Ordinary Greatblade offhands remain prohibited. The plain sword hides Greatblade's baseline weapon while equipped. Equipment ownership, bonuses, visibility and persistence remain owned by the existing inventory/equipment systems; fitting geometry has no gameplay colliders.

Authoring command: dicefree.plain-gear.author. It preserves other catalog entries/offers, adds runtime fitting presentation to player roots, extends input through InputActionAsset, saves through the Editor, and reopens saved scenes to verify catalogue persistence.

## Flexible skill presentation

AbilityBarSource delegates class-specific definitions, views, upgrades and activation to IClassAbilityProvider. Built-in adapters preserve existing targeting, queueing, learning economies and combat mechanics. Custom providers can register or be discovered on the player.

- Authored skill order is preserved; passive positions are not fixed.
- Eight regular skills per page, any number of pages.
- An optional provider-selected ultimate is pinned separately.
- Default keys 1–8 activate the current page; T activates the optional ultimate.
- Shared Attack, Interact and Return Home stay separate.
- Physical and Magical definitions can contain varying numbers of actives/passives and positive per-definition rank caps.
- Upgrade validation still rejects foreign definitions, duplicate IDs and cap overspending.
- Existing InputAction map/action/binding identities and old binding paths are preserved.
- No save schema change and no existing kit balance change.

New skill effects still require their class/provider mechanics. The refactor removes fixed kit size and placement assumptions; it does not manufacture arbitrary combat effects.

## Verification

The combined isolated gameplay suite passed **232 checks**, including 11 funded merchant purchases, insufficient-gold rejection, all-slot equipment on Novice/Physical/Magical/Greatblade, fitting attachment ownership, stat deltas and disk-save round trips. It exercised 13 mixed active/passive Physical definitions with varying rank caps, real extra-slot casts, and a registered 20-skill provider across three pages with a pinned ultimate and upgrade/cast callbacks.

Greatblade's focused combat suite passed 75 checks. Physical model suite passed 47 checks; the all-slot set was then granted/equipped and captured in its isolated Development Room. Magical five-skill/Mana/save/menu-reload suite passed. Ten static data/architecture/progression validators passed. Input identity comparison preserved all 66 existing map/action/binding records.

See validation records and screenshots alongside this document. These are automated functional checks and Editor Game-view inspections; no full human playthrough or finished-art claim is made.

Magical model recommendation: [MAGICAL_MODEL_REVIEW_2026-10-14.md](MAGICAL_MODEL_REVIEW_2026-10-14.md).

Final Physical five-skill/Mana/save/menu-reload and Development Room/World 2 round-trip regressions also passed. The Windows Development build succeeded with seven scenes, 0 errors and 0 warnings (344,700,296 reported bytes). Executable: T:\TEMP\DiceFree-Options\Builds\World1Foundation\DiceFree.exe.

Standalone startup smoke passed: the isolated non-graphical Windows player remained responsive in StartMenu after 27 seconds with advancing frames. This confirms startup only, not a graphical standalone playthrough or performance result.
