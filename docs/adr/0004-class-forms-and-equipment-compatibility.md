# ADR-0004: Class-authored body forms and equipment compatibility

Status: Accepted
Date: 2026-10-01

## Context

DiceFree Ways can change the Echo's species, proportions and even body plan. The project must not assume that every playable class is a human on one universal skeleton.

Settled examples include a Tier-2 Wood-Elf Ranger and High-Elf Wizard. Future classes may be more radical, including possible Orc-like or non-biped forms.

Equipment also needs to remain manageable. Automatically fitting every visual asset to every possible body would multiply art/rig complexity and weaken deliberate class silhouettes.

## Decision

Each class/Way has one authored body/sex presentation.

A Way may be visibly male, visibly female or androgynous. There is no requirement for multiple body-sex variants of every class.

As lineages specialize, their body/model may become increasingly distinct in the direction of the class fantasy.

Gameplay identity and systems remain form-agnostic. Character presentation resolves through a class/form profile that maps semantic concepts to concrete visuals/rigs.

The semantic equipment slot model remains shared across every form.

Equipment appearances do not automatically morph, scale or swap meshes to fit arbitrary forms. Items/appearances use explicit class/form-family compatibility.

Equipment may occupy multiple semantic slots. The first canonical case is a two-handed weapon occupying Main Hand and Off Hand.

## Consequences

Positive:
- strong authored silhouettes and species identity;
- no combinatorial requirement to fit every item to every rig;
- radical future forms do not require new combat/inventory/quest engines;
- equipment restrictions become meaningful class identity;
- multi-slot equipment generalizes beyond hard-coded two-handed checks.

Tradeoffs:
- some visual items need lineage/form-specific versions;
- cross-class equipment pools may be narrower;
- rig families and semantic socket maps need deliberate authoring;
- animation/content reuse must be chosen where compatible rather than forced universally.
