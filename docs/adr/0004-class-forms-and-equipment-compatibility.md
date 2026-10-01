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

A descendant Way may fully transform the body/species/form again rather than permanently inheriting its parent's species/rig.

Gameplay identity and systems remain form-agnostic. Character presentation resolves through a class/form profile that maps semantic concepts to concrete visuals/rigs.

The semantic equipment slot model remains shared across every form.

Visible wearable equipment normally replaces the class form's baseline presentation for that occupied slot.

Character form is not presentation-only: each form may author different gameplay collision/selection/navigation footprint dimensions appropriate to its body.

Attack/basic-attack/ability reach remains separately authored and is not automatically derived from body size.

Required world progression must remain accessible to every intended playable form. Large forms may use explicit navigation/clearance forgiveness at authored bottlenecks rather than being soft-locked by their normal footprint.

Equipment appearances do not automatically morph, scale or swap meshes to fit arbitrary forms. Items/appearances use explicit class/form-family compatibility.

Descendants inherit parent equipment-family compatibility/permissions by default, with explicit additions/removals/overrides allowed as the body/class identity changes.

Equipment may occupy multiple semantic slots. The first canonical case is a two-handed weapon occupying Main Hand and Off Hand.

Equipping a multi-slot item automatically unequips conflicting equipment to carried inventory when capacity permits. If displaced gear cannot be retained safely, the equip fails atomically rather than dropping/destroying items.

Visible slot presentation follows authored layering: Chest/Head/Hands/Legs/Feet replace their baseline slot presentation; Shoulders and Back remain independent layers over the current torso/chest state.

Advancement transformation presentation is optional per Way. The manifestation/class transition remains authoritative even when a bespoke visual transformation is used.

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
- rig families, semantic socket maps and authored hitbox/footprint profiles need deliberate authoring;
- animation/content reuse must be chosen where compatible rather than forced universally.
