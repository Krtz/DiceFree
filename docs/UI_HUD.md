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
- **live real-time 3D player model/portrait** reflecting current class and visible equipped gear where practical;
- current/max HP as text directly under/around the portrait/model;
- class resource;
- level/class identity;
- buffs/debuffs positioned around the player frame by default;
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
- portrait/class icon;
- name;
- level;
- HP bar + current/max;
- resource where useful;
- important buffs/debuffs;
- threat indicator;
- resurrection sickness;
- small cast bar;
- clickable targeting for support/healing.

Party-frame cast bars are independently toggleable.

Detailed exact threat numbers can remain optional.

### Target frame

Current enemy/target frame appears **top-center**.

Default content:
- name;
- level;
- HP;
- important status information.

Target buffs/debuffs appear directly below/around this frame by default.

A target-of-target frame is supported and is toggleable in options.

Bosses use a larger/dedicated top-center presentation in the same broad area.

Boss frames can show:
- name;
- level where meaningful;
- large HP bar;
- phase indicators where relevant;
- cast bar;
- important encounter statuses/mechanics;
- multiple boss bars when an encounter genuinely has several active bosses.

### Minimap

Default minimap:
- square;
- top-right;
- **fixed north-up orientation**.

Players can toggle rotating minimap behavior in options.

Quest tracker sits beneath it.

### Quest tracker

Default:
- top-right beneath minimap;
- shows tracked quest objectives;
- can be hidden in options.

Exact number of simultaneously pinned quests is not yet decided.

## Boss mechanic warnings and telegraphs

Important encounter mechanics should be communicated through **both world telegraphs and UI warnings** where appropriate.

Default philosophy:
- dangerous ground effects use obvious red danger/"ouch" circles, cones, lines or other world-space telegraphs;
- major mechanics may also trigger large center-screen warning text;
- audio/animation/cast bars should reinforce the mechanic;
- warnings should be strong enough to understand, but not spammed for every trivial attack.

## Quest markers and journal

Quest NPCs use the familiar universal markers:
- **!** = quest available;
- **?** = quest turn-in / completion interaction.

DiceFree does not need to reinvent these symbols just to be different.

The quest journal:
- stores active quests;
- shows objectives/details;
- lets players choose which quests are pinned to the HUD tracker.

Default tracked count should stay modest (roughly 3–5 visible quests), while the journal can contain more.

## Fog of war and shared vision

Map/minimap visibility follows a Warcraft III-style fog-of-war model.

Rules:
- unexplored terrain is black/hidden;
- reveal should be **fairly precise to where the manifestation has actually explored**, rather than unlocking huge rectangular/region chunks merely by entering an area;
- terrain becomes permanently revealed on that manifestation's map/minimap after it has been seen;
- **allied/party vision contributes to exploration while playing together**: terrain an ally genuinely reveals for the party can become explored for the current manifestation as well;
- map exploration remains manifestation-specific overall; another manifestation does not automatically inherit the first manifestation's revealed terrain;
- static discovered geography/objects can remain visible afterward;
- current dynamic activity/enemies/events require present vision;
- party/allied players share current vision.

### POI knowledge vs terrain reveal

Terrain reveal and point-of-interest knowledge are separate concepts.

Current direction:
- a manifestation still has to explore its own terrain;
- important POI **icons may be Echo-wide knowledge once discovered by any manifestation**, even while the surrounding terrain remains fogged for another manifestation;
- exact POI categories that share this way remain open;
- deliberately secret content can opt out of normal icon sharing/visibility.

This allows the Echo to remember that a bank, dungeon entrance or other important place exists without magically revealing the entire route/terrain for every manifestation.

Exact vision radii, stealth interactions, POI-sharing categories and static-marker rules remain tuning/design questions.

## Cast bars

DiceFree supports multiple presentation layers for casting.

### World-space cast bars

Units can show a small cast bar above their 3D model/nameplate.

Visibility is independently configurable for:
- self/own units;
- allies;
- enemies.

These bars are toggleable and should remain compact enough not to overwhelm the world view.

### Player cast bar

The player's own dedicated cast bar appears above the main bottom-center player information area by default.

It is a movable/scalable HUD widget.

### Target cast bar

The selected target's cast bar appears below the top-center target frame by default.

Boss encounters may use additional encounter-specific telegraph/cast presentation when needed.

## Buffs and debuffs

Player buffs/debuffs are anchored around the player frame by default.

Target buffs/debuffs are anchored around/below the top-center target frame.

These are modular widgets:
- movable;
- scalable;
- hideable;
- filterable.

Default filtering should prioritize:
- important class buffs;
- harmful debuffs;
- encounter/boss mechanics;
- effects the current character can remove/cleanse/purge.

Cleanse/purge capability is **not healer-exclusive**. Highlighting should be based on the current character's actual effect-removal capabilities.

Players may opt into showing all effects.

## XP bar

Default direction:
- long, thin XP bar across the bottom of the screen.

Exact thickness/segmentation is not final.

XP remains a separate progression display rather than being overloaded as a class-resource bar.

## Threat-state presentation

Target/party UI should expose threat state with simple color/icon indicators.

Default direction:
- green/safe = not currently threatened / low threat;
- yellow = rising/contested threat;
- red = currently has aggro / primary threat.

Exact colors and thresholds can be tuned later.

The same underlying threat values can drive party-frame and target-frame indicators.

Detailed numeric threat remains optional.

## Low-health feedback

Default low-health feedback should prioritize the player frame rather than covering the entire screen.

Default behavior:
- player 3D portrait/frame gains increasingly red/danger styling;
- portrait/model can visibly appear hurt/injured;
- current/max HP text becomes more red/emphasized as health drops.

Optional:
- bloody/red screen-edge vignette/border effect;
- this effect is configurable/toggleable because it can become visually intense.

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

## Ground loot labels

Overworld equipment drops are relatively scarce/meaningful, more Warcraft III-like than loot-fountain ARPGs.

Default presentation should therefore make individual drops readable without requiring aggressive filtering.

Direction:
- item appears clearly when dropped;
- nearby/hovered items can show labels;
- an optional modifier key can reveal ground loot labels where useful;
- rarity/identity should remain easy to read;
- do not assume screens will be covered in dozens of simultaneous drops.

Exact label behavior/filter controls remain tunable later.

## Carried gold display

Carried gold is shown as a small HUD information element by default because carried gold is the amount exposed to death loss.

Rules:
- carried gold visible by default;
- banked gold is not permanently shown on the HUD;
- HUD gold display can be hidden in options;
- other currencies/material resources can remain in menus unless a design later needs them persistent.

## Chat and logs

Default direction:
- bottom-left fading panel;
- tabs/modes can include Chat, System and Combat;
- panel fades mostly transparent/inactive when not being used.

Detailed combat telemetry may remain developer-only even if a normal player-facing combat log exists.

## Character/inventory windows

Character, inventory and similar major windows are overlay panels over the live world rather than full-screen pause screens.

Direction:
- movable;
- scalable where appropriate;
- gameplay continues in multiplayer;
- hotkeys are the primary access method, with limited visible system buttons also available.

## Contextual mouse cursor

Mouse cursor feedback follows Warcraft III-style contextual states.

At minimum support visually distinct cursor states for:
- move;
- attack;
- talk;
- loot;
- interact;
- unavailable/invalid.

The cursor should make click-to-move/context actions immediately understandable without requiring extra text.

## Interaction prompts

Nearby interactable objects/NPCs can show contextual prompts close to the relevant world object, for example:
- `E — Talk`
- `E — Interact`
- `E — Open`

A subtle screen-space fallback may be used when world-space readability is poor.

The player should not need to look away from the object they are trying to interact with.

## Menu/system buttons

The default HUD includes only a **small number of visible system/menu buttons**, Diablo-like in spirit.

Examples:
- main/system menu;
- inventory/character;
- map/journal as appropriate.

Most actions should also have hotkeys.

Do not fill the bottom HUD with a large permanent MMO-style row of system buttons.

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

- exact bottom-panel proportions;
- ability-slot hotkeys and command mapping;
- buff/debuff filtering/rules;
- exact target-of-target presentation;
- currency display;
- inventory/character-sheet access details;
- chat/combat log presentation;
- minimap icon rules;
- quest-tracker pin count;
- boss-frame details;
- UI edit-mode UX;
- precise XP-bar styling;
- exact system/menu button set.
