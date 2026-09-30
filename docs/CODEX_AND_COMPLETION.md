# Codex and Completion

## Principle

The Codex is an **Echo-wide discovery record**. It starts sparse and fills through actual play rather than revealing the world up front.

The Codex should reward curiosity and completion without becoming a mandatory power-grind checklist.

## Discovery

### Enemies

Enemy knowledge is organized as **monster families with variant subpages**.

Examples:
- Slime
  - Crop Slime
  - Road Slime
  - Armoured Slime

A monster/variant is discovered when the Echo **kills it at least once**.

Undiscovered monsters, variants, drops and mechanics should not be leaked merely to make completion percentages easier.

### Drops

A drop appears in the Codex only after the player has **actually seen that item drop**.

Rules:
- undiscovered drops are not shown as empty `???` slots;
- every discovered drop tracks its own **seen-drop counter**;
- the counter means only "times seen drop";
- it does not separately track looted/sold/banked/transmogged copies;
- once every drop in the authored table has been observed, the Codex may explicitly state **All drops discovered**.

### Boss mechanics

Boss mechanics are discovered when the player **sees the mechanic occur**.

Mechanics may be grouped by authored phase:
- Phase 1
- Intermission
- Phase 2
- Enrage
- other authored states

Once observed, the Codex normally describes the mechanic **plainly and usefully**. Individual encounters may deliberately use a more cryptic description as an exception.

## Dungeon entries

Dungeons have their own Codex entries.

Track at least:
- clears;
- wipes;
- bosses killed;
- player deaths inside;
- difficulty/mode variants completed;
- solo clears;
- no-death clears;
- fastest clear;
- loot/drop discoveries.

Fastest clear records are tracked **separately by party size**.

A dungeon defines its own modes. There is no requirement for a universal difficulty ladder; many dungeons may have only one mode.

A dungeon receives the normal **gold completion checkmark** when:
1. every authored mode has been completed; and
2. every authored dungeon drop has been observed.

Individual dungeons may additionally award bespoke titles, cosmetics, achievements or other rewards.

## Codex rewards

Codex completion/research rewards are content-authored and optional.

Possible rewards include:
- titles;
- cosmetics;
- achievements;
- a very small damage bonus against the **exact monster/variant**;
- combinations of the above;
- no reward beyond completion itself.

Combat bonuses should be **tiny and tightly capped**. They exist for fun/completion flavor, not as a required endgame optimization path.

A bonus against one mob does **not** automatically apply to its broader family. This avoids making unrelated overworld completion mandatory for later dungeon efficiency.

Rewards trigger **automatically** when their conditions are met. Do not require opening the Codex to claim accumulated rewards.

## Ownership

All Codex discovery and completion data is **Echo-wide** and shared across manifestations.

## Design constraints

- Do not expose hidden content through counters, blank slots or completion percentages unless deliberately authored.
- Codex state should use stable semantic IDs, not display names.
- Boss-mechanic observation should be driven by semantic encounter/mechanic events.
- Drop discovery should be driven by actual reward/drop observation, not by opening a data table.
- The Codex is a knowledge/completion system; it should not own combat, loot generation or encounter logic.

## Achievements

Achievements are **Echo-wide**.

The achievement UI should organize achievements into authored categories rather than one undifferentiated list.

Expected category examples include:
- Exploration;
- Combat;
- Dungeons;
- Classes / Ways;
- Professions;
- Collections;
- Secrets;
- other authored categories.

The exact taxonomy can grow with the game; an achievement should carry stable category metadata rather than relying on display-name grouping.

Hidden achievements are supported.

While hidden, their normal identity/requirements may be concealed as authored. Once a hidden achievement is earned/revealed, it becomes a normal permanently revealed achievement: **revealed means revealed**. Do not keep its name/description mysteriously hidden after completion unless a completely separate authored mechanic explicitly calls for that.

Achievements contribute to an overall **achievement score/points** value. The score is primarily a completion/bragging metric rather than generic player power.

Individual achievements may additionally grant authored rewards such as titles, cosmetics, mounts, recipes, unlocks or other content where appropriate.

