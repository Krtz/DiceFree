# Quests, Credit and Interactions

## Philosophy

Quest content should be authored from reusable objective, scope and credit policies rather than bespoke scripts wherever practical.

Custom quest logic remains possible for genuinely unusual content, but ordinary quests should be data-driven.

## Quest objective building blocks

Initial reusable objective types should support at least:
- **Kill** — defeat actors matching semantic IDs/tags;
- **Interact** — use an NPC/object/world interaction;
- **ReachArea** — enter a defined world region;
- **Collect** — obtain or turn in items/resources;
- **TalkTo** — complete an NPC conversation/interaction step;
- **UseItem** — use a qualifying item in a valid context;
- **CompleteDungeon** — complete a dungeon/run/event;
- composite **AND / OR** objective groups.

This lets authored quests express cases such as:
- kill 3 Crop Slimes;
- talk to the farmer AND inspect the road;
- clear the Slime dungeon OR kill an elite OR reach a large Slime-kill target.

Do not make quests inspect display names or UI state. Objectives consume semantic gameplay events and stable IDs/tags.

## Quest scopes / repeatability

Each quest/event declares its persistence scope explicitly.

Supported baseline scopes:

### Echo-once
Completed once for the whole Echo/account.

Use for:
- major discoveries;
- account-wide unlock events;
- secret-class prerequisites;
- one-time systemic milestones.

### Session-instance once
Completed once in the current hosted game/session instance.

Starting or joining another compatible game may allow the event again.

Use for authored session events, bosses or world occurrences that intentionally reset with a new game instance.

### Timeline-once
Completed once for the current class manifestation/save timeline.

Another class timeline can experience it independently.

This is the normal default for ordinary story/side quests unless designed otherwise.

### Repeatable
Can be completed repeatedly according to its authored reset rules.

The framework must not assume all repeatables use a real-world daily/weekly timer. Reset policy is content data.

## Authoritative events and consumer-specific credit

Combat/world systems emit semantic authoritative events.

Examples:
- actor defeated;
- interaction completed;
- area entered;
- item collected/used;
- dungeon completed.

The event records enough context for consumers to evaluate credit, such as:
- stable actor/content IDs and tags;
- killer/last-hit actor where relevant;
- threat/contribution participants where relevant;
- positions/range context where relevant;
- session/party ownership;
- source/target identity.

**The event itself does not decide one universal quest-credit rule.**

Different quests, XP rewards, achievements and mechanics may consume the same event with different credit policies.

## Kill / event credit policies

Content can select from policies including:

### Global
All eligible manifestations in the relevant session/party receive credit regardless of proximity.

### Nearby
Eligible manifestations within an authored radius receive credit.

### Threat participation
Eligible manifestations must have meaningfully participated in that enemy's combat through the threat/combat-participation model.

Generating qualifying threat can include damage, healing/support threat and other actions as defined by combat rules.

### Last hit
Only the qualifying actor/player responsible for the authoritative final hit receives credit.

Additional policies/compositions may be added later.

Do not bake one policy into `Health`, `BasicAttack` or enemy code.

## NPC and world interaction model

Interactable actors/objects expose one or more semantic **interaction actions**.

Examples:
- Talk;
- Quest;
- Shop;
- Bank;
- Craft/Blacksmith;
- Respec;
- Class advancement;
- Use/Open/Inspect.

### Default action

An interactable may define a default action.

If there is one obvious action, interaction can execute/open it directly.

Examples:
- a pure banker opens the bank;
- a simple quest NPC starts the relevant conversation.

### Interaction menu

If an interactable exposes multiple relevant actions, show a small interaction menu.

Example:
- Talk;
- Quest;
- Respec;
- Shop.

The framework should not require separate duplicate NPCs for each service.

Availability/visibility of individual actions can depend on quest state, Echo-wide state, class requirements, inventory, etc.

## Multiplayer world-state rule

Host state controls which world actors/events physically exist in the session.

Guest eligibility controls whether a physically possible action grants that guest progress.

If an objective requires an NPC/object/event that no longer exists in the host's world, that objective is simply unavailable in that session; the guest's own progression is not failed or overwritten.

See `docs/MULTIPLAYER.md`.


## Dungeon / encounter events

Dungeon and encounter systems emit semantic events through the same general event/requirement architecture.

Examples include:
- EnteredDungeon;
- DungeonRunStarted;
- BossEncounterStarted;
- BossDefeated;
- DungeonCompleted;
- DungeonCompletedWithoutDeath;
- SecretRoomDiscovered;
- EncounterCompleted;
- RunAbandoned;
- FullPartyWipe;
- other authored encounter milestones.

These events can be consumed by:
- quests;
- achievements;
- class/Way requirements;
- item/unlock requirements;
- hidden discoveries;
- analytics/debug tooling.

Requirements may combine dungeon events with arbitrary other predicates.

Example in principle:
- complete Dungeon X as a level-200 Novice;
- no party deaths;
- specific item equipped;
- specific secret interaction completed.

Do not encode these future combinations as dungeon-specific hard-coded managers.

## Quest-marker and objective presentation

Default quest-NPC presentation uses the classic readable convention:
- **!** for an available quest;
- **?** for a quest ready for relevant follow-up/turn-in.

For now, normal quest objectives should plainly tell the player what to do.

The quest framework should still allow individual authored quests to use intentionally vaguer clues/directions later, but vagueness is an exception rather than the default UX.

Do not remove the classic markers in pursuit of a universally "immersive" quest presentation.
