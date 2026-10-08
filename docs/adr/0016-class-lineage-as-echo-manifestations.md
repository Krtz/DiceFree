# ADR 0016: Permanent Class Lineages and Echo Manifestations

- Status: Accepted
- Date: 2026-10-08

## Decision

Every Echo begins as Novice. Classes form a directed graph: branches may fork or converge and secret eligibility may depend on non-class conditions. At milestones 10, 30, 60, 120 and 200, advancement creates or activates a distinct class manifestation starting at level 1; the parent manifestation is retained, and at most one durable manifestation exists for each stable class ID per Echo. No experience carries into the new level-1 form. Advancement eligibility is authored, not derived from hard-coded symmetry between sibling classes. See docs/CLASS_PROGRESSION.md, docs/CLASS_REQUIREMENTS_AND_DISCOVERY.md and ADR 0005/0007.

## Consequences

Game architecture follows this policy. Details, numerical tuning and per-feature authoring remain in the cited design files; changing the policy requires a superseding ADR.
