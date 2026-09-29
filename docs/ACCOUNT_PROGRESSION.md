# Account Progression

DiceFree distinguishes manifestation-specific progression from Echo/account progression.

## Advancement creates manifestations

Class advancement creates a new **class manifestation/save-state** rather than overwriting the parent.

The Echo has at most **one persistent save-state per class ID**.

Example:
- a level-10 Novice chooses Physically Blessed;
- the Novice save remains playable;
- the first Physically Blessed advancement creates its one level-1 Physically Blessed save-state;
- the same Novice can later create the one Magically Touched save-state.

This makes class branching a literal Echo/timeline mechanic.

At branch creation:
- the player immediately continues the current game as the child manifestation;
- **all manifestation-specific state is snapshotted** into the child;
- this includes equipped gear, carried inventory, carried gold/currency, quest/world progression and relevant class/skill state;
- the parent keeps the original state and the child receives a duplicate snapshot;
- parent and child diverge independently from that point;
- Echo-wide systems remain shared.

When the preserved parent is loaded later, it resumes immediately before the blessing/branch choice that created the child.

Roster model:
- there is no small fixed character-slot cap;
- roster capacity naturally follows the number of classes;
- duplicate save-states for the same class are not created;
- the class/Ways selection UI can be used to choose which class timeline to continue;
- Archive is the preferred cleanup/organization mechanism;
- deleting a class-local save must not remove Echo-wide milestones/discoveries.

Permanent deletion behavior:
- Way discovery/unlocks remain Echo-wide;
- Echo-wide milestones earned through that class remain;
- the class save itself is gone;
- recreating it requires advancing into that class again from an appropriate earlier-tier/prerequisite save.

Archive remains the preferred routine cleanup mechanism.

## Echo-wide class evidence

Reaching meaningful class/tier milestones creates permanent Echo-wide evidence.

Examples:
- reached Tier 2 Wizard;
- completed a specific class advancement;
- discovered a hidden Way.

These records:
- can satisfy convergence/secret-class requirements;
- survive deletion/cleanup of the manifestation that originally earned them;
- are not the same as currently owning a manifestation of that class.

Class/Way discovery is also Echo-wide once earned.

See `docs/CLASS_REQUIREMENTS_AND_DISCOVERY.md`.

## Per-class resurrection point

Each class save tracks its own **latest registered resurrection point**.

Rules:
- starting/loading that class normally spawns it at its latest registered resurrection point;
- class switching can only be initiated through a resurrection stone;
- switching to another class uses that class save's own registered point;
- one class changing its resurrection point does not change another class save's point;
- when a new class save is first created by advancement, it **inherits the parent class's currently registered resurrection point**;
- if a saved resurrection point is unavailable/invalid in the current game/session, **Cornberg** is the fallback.

## Manifestation-specific

Examples:
- class lineage;
- class level;
- equipped gear;
- class kit/resources;
- carried currency;
- many quests/events.

## Echo-wide

Examples:
- shared bank;
- banked currency;
- profession knowledge/progression;
- selected recipes/discoveries;
- account-bound items;
- some quests/events.

Rules are decided case by case.

## Currency split

Gold and future currencies can have both carried and banked states.

### Carried
- manifestation-specific;
- used while adventuring;
- may lose a percentage on death;
- cannot be remotely deposited.

### Banked
- Echo-wide;
- safe from normal death loss;
- deposited manually in town;
- should support Deposit All.

Future currencies/materials may use different safety rules.

Some resources can be always safe.

Some can use carried/stored risk.

The currency system should support this generically rather than hard-coding gold assumptions.

## Switching and inventory

Class inventories remain separate when switching manifestations.

Switching at a resurrection stone:
- does not automatically open the bank;
- does not transfer carried items/equipment between classes;
- does not pull items from the shared bank.

If the newly selected class needs banked gear, the player must physically go to a bank and withdraw it under the normal rules.

## Shared bank

- all items account-bound;
- generous capacity;
- items can be remotely sent/deposited from anywhere;
- items can be withdrawn only in town;
- currencies require physical town banking unless a future currency explicitly defines different rules.

## Professions

Profession progression is Echo-wide.

## Quest persistence

World/systemic discoveries are good Echo-wide candidates.

Class progression, story execution and ordinary questing are usually manifestation-specific unless designed otherwise.

## Open

- class-save archive/delete/recreate UX;
- achievements;
- codex;
- pets;
- cosmetics;
- difficulty unlocks;
- exact slot count.


## Save ownership / persistence

Echo-wide and manifestation-specific data have explicit ownership even if stored in one physical save package.

Loading a manifestation always returns it to its registered resurrection point rather than restoring an arbitrary world coordinate.

Important operations such as advancement and bank/inventory transfer must be transactional/atomic.

See `docs/PERSISTENCE_AND_SAVES.md`.
