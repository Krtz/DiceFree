# ADR 0005: Manifestation roster and load-time class selection

- Status: Accepted
- Date: 2026-10-06

## Context

DiceFree advancement does not replace a class. Advancing creates a new class manifestation while preserving the parent timeline, and each stable class ID can have at most one persistent manifestation per Echo.

The prototype previously had one Cornberg player object and one manifestation section, so loading implicitly meant loading whatever class the scene was authored as. #50 introduces the first real fork and therefore needs an authoritative answer to two questions:

1. how an Echo records which class manifestations exist and which one should load;
2. where the player chooses among those saved manifestations.

The solution must preserve old v5 saves, unknown content and atomic save semantics, and it must not make arbitrary mid-combat class swapping part of normal gameplay.

## Decision

Use a small Echo-level manifestation roster plus a dedicated load-time start menu.

### Persistence

- Manifestation durable state remains one section per stable class ID: `manifestation:<class-id>`.
- Add optional Echo section `echo:manifestations`, roster record version 1.
- The roster stores the active class ID plus parent -> child branch history.
- This is additive metadata inside Echo schema v5; it does not require an Echo/manifestation schema bump.
- A v5 save without the roster remains valid. Runtime derives a fallback active manifestation and writes roster metadata on a later successful commit.
- Unknown manifestation sections remain opaque/preserved.

### Advancement

- Advancement is a transaction over a cloned Echo profile.
- Commit the preserved parent, new child and roster update in one Echo revision before changing the live actor.
- Child level/XP becomes 1/0 and class-local skill state resets for the new class.
- Manifestation-owned world/quest/anchor/currency/inventory/equipment state is snapshotted.
- Copied inventory receives new item instance IDs; copied equipment references are remapped to those child IDs.
- Creating a target class that already has a manifestation is rejected without overwriting it.

### Selection UX

- The player build boots into a dedicated `StartMenu` scene.
- No save: offer `Start as Novice`.
- Existing Echo: show saved manifestations with class name and level and mark the last-active manifestation.
- Selecting a manifestation transactionally updates the active roster choice, then loads Cornberg.
- If saved class content is missing from the current build, show it as unavailable instead of silently removing it.
- Do not expose arbitrary manifestation swapping as an ordinary in-world button. The normal selection boundary is load/start time.

The temporary mountain/blessing selector remains in gameplay because it creates a **new** manifestation; it is not the roster selector.

## Consequences

### Positive

- Class identity is stable and save-addressable.
- Parent/child timelines can diverge safely.
- The start menu naturally scales from Novice to a large class web without a hard-coded slot count.
- Old v5 saves remain readable.
- Future multiplayer can treat the selected manifestation as explicit session input rather than inferring it from scene composition.
- Save ownership is clearer: Echo roster metadata vs manifestation-local state.

### Costs / follow-up

- The current menu is prototype IMGUI and needs production presentation later.
- Multiple-Echo profile selection, archive/delete/recreate UX and class preview cards remain future work.
- Tier-1 class shells are loadable before their real model/kit implementations land; they must remain clearly documented as shells.
- A future Return-to-Title / session-leave flow should route back through the same selection boundary rather than reintroducing arbitrary live class swapping.
