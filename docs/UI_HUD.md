# UI and HUD

## Philosophy

DiceFree's default HUD should feel closest to **Warcraft III**:
- game world remains readable;
- important combat information is always available;
- the player portrait/model and core command/ability area sit at the bottom;
- the HUD is more information-rich than Diablo/League but less screen-dominating than a typical MMO layout.

Long-term, the HUD should be **modular, movable and scalable**.

The framework should support radically different player-created layouts, including styles resembling:
- Warcraft III;
- Diablo;
- World of Warcraft;
- RuneScape;
- other compact or information-heavy arrangements.

The POC only needs one strong default layout. Do not build a complete HUD editor yet.

## Default layout

### Bottom player panel

The default WC3-like player panel sits along the bottom.

It contains:
- player portrait or real-time 3D model;
- current/max HP as text directly under/around the portrait/model;
- class resource;
- level/class identity;
- buffs/debuffs where appropriate;
- nearby ability/command area.

HP numeric format:
`current / max`

Example:
`735 / 920`

### Ability area

Design the default lower-right ability/command area around a **maximum visible grid of 12 slots**:
- 3 rows;
- 4 columns.

This does **not** mean classes should have 12 active abilities.

The extra grid capacity allows room for:
- active class abilities;
- ultimate;
- context/class mechanics;
- consumables;
- utility/interact commands;
- temporary abilities;
- future special actions.

Unused slots do not need to be visibly cluttered.

### Party frames

Default party frames are in the **top-left**.

The player's own frame is included alongside allies.

Party frames should be able to show:
- HP;
- resource where useful;
- important buffs/debuffs;
- threat indicator;
- resurrection sickness;
- clickable targeting for support/healing.

Detailed exact threat numbers can remain optional.

### Target frame

Current enemy/target frame appears **top-center**.

Default content:
- name;
- level;
- HP;
- important status information.

Bosses can use a larger/dedicated boss presentation in the same broad area.

### Minimap

Default minimap:
- square;
- top-right.

Quest tracker sits beneath it.

### Quest tracker

Default:
- top-right beneath minimap;
- shows tracked quest objectives;
- can be hidden in options.

Exact number of simultaneously pinned quests is not yet decided.

## World-space HP/nameplates

Characters/enemies can have a nameplate/HP bar above the 3D model.

Default content:
- name;
- level;
- HP bar.

Player options must independently control display behavior for:
- own units/self;
- allies;
- enemies.

At minimum, HP-bar visibility supports:
- **Always**
- **Only when hurt**
- **Never**

Additional name/level/status options can be added later.

This system should not assume every world-space unit always needs a permanent UI label.

## Modularity architecture

HUD pieces should be implemented as separate reusable panels/widgets rather than one giant fixed HUD script.

Examples:
- player frame;
- ability grid;
- target frame;
- party frames;
- minimap;
- quest tracker;
- buffs/debuffs;
- nameplates;
- boss frames;
- notifications;
- chat/log if later used.

Long-term each panel should be capable of:
- moving;
- scaling;
- hiding;
- changing presentation style;
- potentially switching between alternate templates.

Do not hard-code gameplay logic to one screen coordinate.

## HUD profiles

Long-term direction: allow multiple layout/profile styles.

Potential examples:
- WC3-style default;
- Diablo-like;
- WoW-like;
- RuneScape-like;
- fully customized.

These are layout inspirations, not promises to reproduce another game's exact UI.

Full HUD-editor UX is deferred until after core gameplay works.

## POC requirements

For Cornberg POC, implement enough UI architecture to prove:
- bottom player portrait/model panel;
- current/max HP;
- class resource;
- ability/command grid structure;
- top-left party/self frame area;
- top-center target frame;
- top-right square minimap area;
- quest tracker below minimap;
- world-space name/level/HP bars;
- settings hooks/data model for visibility preferences.

POC does **not** need:
- polished HUD skins;
- full drag/drop editor;
- multiple complete layout presets;
- final art;
- every optional panel.

## Open questions

- portrait: static portrait vs live 3D model implementation details;
- exact bottom-panel proportions;
- ability-slot hotkeys and command mapping;
- buff/debuff placement;
- target-of-target;
- cast bars;
- interaction prompts;
- XP bar placement;
- currency display;
- inventory/character-sheet access;
- chat/combat log presentation;
- minimap icon rules;
- quest-tracker pin count;
- boss-frame details;
- UI edit-mode UX.
