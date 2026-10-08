# ADR 0021: Echo/Manifestation Persistence and Hosted Session Scope

- Status: Accepted
- Date: 2026-10-08

## Decision

Accounts may own multiple independent Echo profiles, and Echoes own many durable class manifestations. Account/Echo-wide unlocks are separate from manifestation-level progress, temporary buffs and per-run runtime state. Local saves favor robust recovery, versioned migration and autosave over adversarial anti-cheat; no hostile rollback policing is needed for this PvE title. Multiplayer is a 1–4 player hosted/session game, not a permanent MMO. Session map presence and the exclusive dungeon occupancy rules follow ADR 0006. Save/reload resets transient combat state rather than resuming arbitrary combat frames. See docs/PERSISTENCE_AND_SAVES.md, docs/MULTIPLAYER.md and docs/ACCOUNT_PROGRESSION.md.

## Consequences

Game architecture follows this policy. Details, numerical tuning and per-feature authoring remain in the cited design files; changing the policy requires a superseding ADR.
