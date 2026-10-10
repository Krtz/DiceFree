# DiceFree — Oct 10 Combat and VFX Tuning

Branch: `feature/visual-overhaul-vis00`. Not merged to `main`.

## Requested changes

- **Big Slime / Slime Boss / Slime Regent** get a real host-authoritative close-range autoattack (the pre-existing slime basic-attack definition and accuracy/damage pipeline). This is a separate, stationary boss-specific attack coroutine, because enabling the standard BasicAttack component would cause bosses to move/chase and would interfere with scripted mechanics. Attack interval respects authored basic attack interval with a 1.35-second minimum, and the short windup drives slime squash/stretch. Attacks do not occur during scripted Slam / Divide / Roll / Bounce.
- **Slam cooldown** reduced from 6.0 to **4.5 seconds** in the serialized slime tuning asset and new-asset default. The 3-second telegraph warning is unchanged.
- **Big Slime miniboss split fragments** now have exactly **10% of the parent miniboss's maximum HP each** (80 HP at the current 800-HP setting). Ordinary Slime Boss fragments remain at 300 HP and Regent fragments keep their 10% of Regent maximum HP. Puzzle/reunion/add behavior unchanged.
- **Novice Magic Sand** now has 18 swirling sand grains (previously 10), a more pronounced airborne arc and two golden spiral/spark effects at launch and impact. Damage, targeting and miss mechanics are unchanged.
- **Magically Touched** autoattack now launches an actual violet-blue Arcane Spark visual missile with orbiting wisps and a trail. Damage applies at the normal BasicAttack impact, not when the cosmetic projectile arrives; cosmetic missile has no physics colliders or gameplay authority.
- **Magically Touched** basic attack minimum/maximum flat variance configured **−4 / +4** in the serialized class definition and authoring pipeline. The damage resolver already handles this existing stat field.

## Quality checks

The branch's existing `dicefree.magical.validate` checks −4/+4, while `dicefree.slime.validate` checks the 4.5-second Slam and 10% mini-fragments, plus all boss basic attack definitions.
`dicefree.magical.playmode-test` passed. The visual screenshots `StarterSkillsVfx_Unity.png` and `ArcaneAutoattackMissile_Unity.png` were generated and inspected in Unity. The missile preview validated **0 colliders**.
**Both normal and secret-Regent full Slime Dungeon playtests PASSED**, including runtime autoattack assertions for Big Slime, Slime Boss and Slime Regent before the corresponding boss Divide phases. The 80-HP Big Slime fragments, normal boss fragments and secret Regent Roll/Bounce mechanics passed. The dungeon's 60-second staging is intentional.

## Future tuning

Big Slime's actual damage and boss melee interval still depend on the existing `Slime bump` attack definition. Consider separate miniboss/boss/Regent autoattack damage profiles as playtest data accumulates, along with synchronized audio, hit sparks and longer-range boss attack choreography. Avoid raising Novice sparkle intensity to endgame class levels.
