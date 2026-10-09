# DiceFree — Starter Skills VFX and Tier 1 Identity Pass

Status: VISUAL-OVERHAUL BRANCH ONLY (2026-10-09). Not merged into main.

## Intention

Early abilities should feel satisfying, recognizable and responsive without competing with later-tier fantasy. No combat balance, skill ranges, cooldowns, damage, collision, root motion, targeting or boss telegraph semantics are changed.

## Castable skill FX (12)

| Class | Skill | Visual |
|---|---|---|
| Novice | Strength Melee Stun | Restrained warm strike flash and short motes |
| Novice | Magic Sand | Warm amber motes supplementing the original sand projectile |
| Novice | Agility Attack Speed Buff | Light golden radial cue on recipient |
| Novice | Spirit Heal | Fresh green support cue alongside original healing light |
| Physically Blessed | Heavy Strike | Oxblood-orange slash, compact strike sparks, shock ring |
| Physically Blessed | Guard | Steel-blue shield contour and ward cue |
| Physically Blessed | Quickening | Amber kinetic sparks and quick buff cue |
| Physically Blessed | Arrow Rain | Light-blue area boundary + individual hit-wave accents; original arrows retained |
| Magically Touched | Magic Sand | Lavender dust burst on target |
| Magically Touched | Mend | Soft mint healing sparks/ring |
| Magically Touched | Fire Elemental Imbuement | Small orange spiral and ember buff cue |
| Magically Touched | Ice Burst | Cyan warning ring followed by radial frost rays and icy spark impact |

Passive skills receive no cast-only effect. Their activated/proc presentation is future work.

Effect budget: generally 7–24 short-lived particles per cast, with limited temporary lines (no new gameplay colliders, camera shake, screen-covering bloom or light spam). The URP Particle System module is enabled in Packages/manifest.json, and source-controlled Resources materials + glow texture prevent standalone shader stripping. The materials/texture are original and editable.

Downloaded third-party effects were inventoried in the existing AssetCatalog. We have NOT copied full packs into the repo: shader compatibility, unnecessary sample scripts/scenes and source redistribution/licensing must be reviewed per asset before selectively adapting them. Their highest-spectacle effects are more appropriate for later classes.

## Tier I looks

Both Tier I presentations inherit the current Novice skin/hair/cloth textures rather than the old grey material overrides.

- Physically Blessed: dark oxblood upper outfit, steel shoulder guards, close-fitting bracers and a small breastplate emblem.
- Magically Touched: indigo clothing, caster mantle/collar, soft-blue shoulders and cuffs, small arcane focus, book at hip and a sculpted back mantle.
- Accessories attach to imported humanoid bones without adding colliders or modifying equipment socket contracts.
- The editor authoring command `dicefree.visual.tier1.install` can reapply the look if class prefabs are regenerated.

## Verified

- Unity compile succeeded with 0 errors, 0 warnings after adding Particle System.
- `dicefree.visual.skills.capture`: 12 illustrated cues; 186 sample particles in isolated preview render.
- `dicefree.visual.tier1.validate`: both upgraded humanoid prefabs present with 9 signature pieces each and no art colliders.
- `dicefree.magical.validate`: passed.
- `dicefree.magical.playmode-test`: passed.
- `dicefree.physical.playmode-test`: passed.
- `dicefree.novice-skills.playmode-test`: passed.
- Isolated images: `Assets/_DiceFree/Art/Validation/Previews/StarterSkillsVfx_Unity.png` and `TierOneClassIdentities_Unity.png`, actually inspected.
- `dicefree.physical.validate` still reports a Cornberg player Mana-wiring mismatch from the broader class setup. Do not consider this issue fixed by visual work.
- These captures are static Editor previews plus automated game flows, not evidence of a complete manual gameplay presentation review.

## Next polish targets

Selective use of vetted URP VFX assets, proper skill launch/arrival trajectories and trails, class-specific cast animation timing, physical cloak and magical robe refinement, higher-quality material textures, spell presentation over varying combat lighting and actual low/high FPS captures. Preserve headroom for later class tiers.
