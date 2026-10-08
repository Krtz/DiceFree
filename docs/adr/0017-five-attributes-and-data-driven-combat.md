# ADR 0017: Five Attributes and Combat/Defense Arithmetic

- Status: Accepted
- Date: 2026-10-08

## Decision

Five primary attributes (VIT, STR, AGI, INT, SPI) are determined by the class base values and authored per-level growth; players do not manually assign attribute points. Basic attacks, heals and abilities use explicit class/ability scaling. Physical and magical defense follow the authored diminishing-returns curve; defense reduction, penetration and resistances apply in the documented order. Elemental resistances are multiplicative to defenses, negative values are permitted, and deep inversion may cause healing-type effects to deal damage. Individual coefficients are mutable balance values, not new ADRs. See docs/STATS_AND_DAMAGE.md, docs/COMBAT.md, docs/ELEMENTS.md and docs/DECISION_LOG.md.

## Consequences

Game architecture follows this policy. Details, numerical tuning and per-feature authoring remain in the cited design files; changing the policy requires a superseding ADR.
