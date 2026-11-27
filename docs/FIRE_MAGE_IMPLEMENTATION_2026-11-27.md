# Fire Mage implementation — 27 November 2026

Mage now advances into a separate level-one **Fire Mage** manifestation. The Mage parent retains its level and purchased skills. Fire Mage implements the final approved design through Round 27 in `docs/classes/MAGE_TIER3_DESIGN.md`, including six skills, 150 exact rank gates, shared Mana rules, Heat, the volcano mentor and the solo movement trial. Future Pyromancer, Frost Mage, Cryomancer and Hydromancer concepts remain outside this implementation.

## Finding the mentor and advancing

**Maelis Cinderweave** teaches from a safe camp beside the road at the base of Ember Crown in southeast Ashfall Reach, World 2. Her authored position is `(64052.50, 8.49, 63643.75)`; the interaction approach is `(64052.50, 8.10, 63641.75)`. World 2 retains its existing dragon-story access gate. There is no class-specific bypass portal.

A **Mage level 60+** can accept **The Ember Walk**, enter the separate solo course through Maelis, reach the far beacon alive, and return to her to turn in the trial. Clearing the course saves the success flag before extraction. Reloading does not require clearing it again. Normal advancement creates a new level-one class with one earned point and all six skills at rank zero; it does not convert the level-60 Mage into a level-60 Fire Mage. The normal manifestation anchor behavior applies when the new class activates. Repeated acceptance and turn-in cannot duplicate the class.

The mentor supports teacher discovery, quest markers and atlas markers through the existing interfaces. Fire Mage is registered in the main and development class catalogs, menu previews, class presentation bindings, skill state persistence and the player ability bar. Mage-specific Slime Orbs, the Azure Slime Orb and Starfall Wand also accept Fire Mage; universal gear keeps its normal behavior.

## Skills, resources and formulas

The exact approved table provides **35 / 25 / 30 / 30 / 15 / 15 ranks** across Fireball, Flame Ward, Ignite, Living Bomb, Overheat Mastery and Infernal Presence. One point is earned per class level 1–150. No gates were regenerated or substituted.

For each skill's own purchased rank `r`:

| Skill | Raw scaling / behavior | Mana | Cooldown, first → final rank |
| --- | --- | --- | --- |
| Fireball | `30 + 8r + (0.3 + 0.04r) × INT`; single target, 0.2s cast | `20 + 15r` | 14 → 7s |
| Flame Ward | Self-only instant Mana redirection; details below | `10 + 6r`, plus redirected damage | 28 → 20s |
| Ignite | Eight ticks in eight seconds, each `9.5 + 68(r−1)/29 + [0.085 + 0.34(r−1)/29] × INT`; instant | `30 + 15r` | 14 → 10s |
| Living Bomb | Three-second fuse, 3m AoE `57 + 408(r−1)/29 + [0.51 + 2.04(r−1)/29] × INT`; 0.2s cast | `50 + 15r` | 22 → 15s |
| Overheat Mastery | Ordinary successful Fire casts generate `10 + rank` Heat, including Ward | — | Passive |
| Infernal Presence | Permanent 15m Fire-resistance aura: allies including self `+0.5pp × rank`, enemies `−0.5pp × rank`; strongest identical aura only | — | Passive |

All four cooldown curves interpolate linearly across their own ranks. Named tooltips show scaling, current raw damage or shield amount, Mana cost, recovery, empowerment behavior and next rank gate. Fire Mage abilities have **no innate critical hits** and do not activate the future spell-crit equipment exception.

Heat has a maximum of 100 and its own visible HUD bar alongside Mana. At 100, the next accepted eligible Fire spell consumes 100 Heat and pays its usual Mana cost. That empowered spell grants no same-cast Heat. Ordinary casts generate Heat when released; interrupted windups do not generate it. After ten seconds without a successful Fire spell, Heat decays at ten points per second, preserving fractions. Death and manifestation loading clear Heat and transient spell state.

- Empowered **Fireball** leaves an independently overlapping 4m field with six ticks at seconds 1–6, each exactly 25% of that Fireball's raw direct damage.
- Empowered **Ignite** has three times normal tick potency. Same-caster recasts restart its eight-tick timer and retain the stronger captured raw potency; separate casters own separate effects.
- Empowered **Living Bomb** deals twice normal initial explosion damage and attaches one generation of ordinary, full-area bombs to nearby enemies. Bomb instances stack independently. Carrier death immediately detonates every attached bomb. Secondary bombs can react to carrier death but never propagate another generation. A detonation queue handles chained deaths without mutating the active iteration.
- **Flame Ward** redirects 30→65% of post-shield damage for 4→10 seconds at 1.9→1.3 Mana per prevented HP. Insufficient Mana provides only the protection affordable. Its empowered form adds a separate Fire shield of `50 + (0.5 + 0.1r) × INT` for ten seconds. The global elemental shield consumes 0.8 capacity per matching Fire damage prevented or 1.2 for other eligible damage, then the Mana ward handles the remainder. Expiration does not heal.

Delayed damage captures offensive rank, INT and penetration when applied. Target defense, resistance, incoming modifiers and shields resolve when each packet lands. Effects, subscriptions, material instances and temporary visuals are removed on death, disable or manifestation reset. Invalid targets spend nothing and start no recovery; interrupted accepted casts retain their cost and cooldown.

Starting attributes are **VIT 19, STR 7, AGI 12, INT 33, SPI 13**, with growth **2 / 0.75 / 1 / 6 / 1.5**. Bare level-one HP is 295. Mana uses `160 + 5 × current INT` maximum and `4 + 0.1 × current SPI` regeneration per second: 325 and 5.3/s without level-one equipment or penalties. Basic attacks inherit the parent Mage configuration with a 1× main-stat coefficient.

## The Ember Walk

The course is one short continuous A→B route through interlocking basalt ridges, pale paving and molten lava. It has a separately baked NavMesh, safe entrance and exit beacons, repeated warning/burn/safe patterns, amber warning plates and flame jets. The route requires going around its bends; static navigation verifies more than 180m of travel rather than a direct exit shortcut. There is no timer, boss, target-shooting objective or multiplayer party requirement.

- Lava applies nominal **15% maximum HP raw Fire damage immediately on genuine entry**, then once per second while exposed. Safe-ground exit stops the exposure; genuine re-entry applies a fresh immediate hit. Membership uses a union of safe regions, avoiding false contact resets at internal seams.
- Active flame applies nominal **25% maximum HP raw Fire damage**, with a fixed 1.5-second re-hit interval. Warning-only phases cause no damage. Both hazards use the existing magical Fire defense, resistance, shields and incoming modifiers exactly once, rather than fixed post-mitigation HP loss.
- Living players keep walking after mistakes. Actual death retries at the entrance with full HP and Mana. Spent potions remain spent and the accepted quest remains active.
- Active class abilities, movement spells and Return Home cannot bypass the course. Walking and ordinary consumables remain available. Abandon and normal exit release the ability/loadout locks. World 2 now remains loaded while its hero is in an instance, preserving the mentor and return point.

## Presentation and validation

The character now uses the user-selected orange **Magician** from the local library's magician.zip. Its original painted atlas, pointed hat, coat, bag and staff are preserved. A new 22-bone humanoid rig and T bind pose support the native Magical controller's idle, locomotion, cast, guard, attack, hit and death states. The original staff uses a dedicated holding-arm pose beside the body during live animation and portrait posing, leaving the other casting arm unchanged. The staff tilts away from the head while alive and follows the hand rotation on death. Maelis uses the same form. Existing Fire Mage saves and the form prefab GUID remain compatible. The original Dawnwarden/Priest assets remain available to their other users. Infernal Presence has an orange/gold ground sigil. New native shaders supply molten cracks, stone paving and soft additive flame particles; the flame material is included explicitly through Resources.

Authoring is reproducible through `tools/authoring/FireMageAuthoring.cs`. The dedicated review scripts and evidence are in [the validation folder](validation/fire-mage-2026-11-27/).

The full advancement/combat review ran in `FireMage-20261127-000208`: **242 runtime checks**, including the real World 2 transition, mentor quest, abandonment, potion persistence, hazard cadence, entrance death retry, normal pathfinding through the entire course, saved success, actual class award, all 150 purchases, damage snapshots, bombs, Ward, Heat, interruption and parent preservation. The final delivery adds only corrected hand-attachment scale and raised warning plates; a binary receipt verifies that every DiceFree runtime DLL matches the full combat build. **355 static checks** and a fresh-process reload/presentation review cover the final build. The main checkout imported with exit code zero and no C# errors. The Windows build succeeded with zero build errors.

Final evidence totals **627 checks: 355 static, 242 advancement/combat, 22 fresh-process reload and 8 live visual checks**. The final visual review re-entered the authored course, verified the warning plates above the walkway, observed live flame particles and returned normally to camp and menu.

These are automated disposable-profile checks, not manual balance acceptance or live network co-op acceptance. The advancement trial is deliberately solo. Batch captures show world geometry and omit screen-space overlay UI. Unrelated package-manager settings and recovery files were preserved.

Final Windows development build: `T:\TEMP\DiceFree-Roadwarden-QA\Builds\FireMage-20261127-000732\DiceFree.exe`.

## Orange Magician delivery checkpoint

The selected model replacement is validated separately in [the Magician evidence folder](validation/fire-mage-magician-2026-11-27/): **355 class static checks, 41 model/animation checks and 24 standalone runtime checks**. The runtime review loads the existing level-150 Fire Mage and all 150 learned ranks, renders idle/cast/walk portraits through the game's actual materials, verifies live and disabled-portrait staff grips, casts Fireball with unchanged damage/Mana/Heat, preserves the level-60 Mage parent, visits matching Maelis through the ordinary World 2 crossing, saves and returns to class selection without Unity errors.

The source retains 4,874 triangles and its original 512px painted atlas. Blender preparation, native FBX import and authoring are reproducible through `tools/authoring/prepare_fire_mage_magician.py`, `FireMageMagicianAuthoring.cs` and `FireMageAuthoring.RefreshPresentation`. The editable rig is stored under `SourceArt/FireMageMagician`. Source hashes and canonical package admission are recorded; the supplied asset's original author/license are recorded as unverified, with the user's explicit asset-selection authorization, rather than assigning an invented license.

Latest Windows development build: `T:\TEMP\DiceFree-Roadwarden-QA\Builds\FireMage-20261127-154934\DiceFree.exe` (16 scenes, zero build errors). The earlier 627-check combat/trial milestone above remains historical; this presentation checkpoint reran 420 relevant checks. Screenshots from the batch player omit overlay UI. Manual balance and live co-op acceptance are unchanged.
## Staff clipping correction

The initial Magician pose placed the long staff against the head because the shared casting animation brings its hand in front of the body. `FireMageStaffPresentation` now solves a dedicated two-joint holding-arm pose beside the body, without changing bone lengths or the other arm's casting animation. It derives the outward staff tilt from the actual holding side. Portrait bone posing runs before prop posing; the death animation releases the upright holding constraint.

[The correction evidence](validation/fire-mage-staff-2026-11-27/) includes **560 geometry samples**: seven living motion states, twenty times per state and four headings. Actual head/hat geometry is separated from the staff mesh by at least **0.071m** across those samples. Each sample also verifies the grip, unchanged arm lengths, unchanged casting hand and repeatable portrait posing. Death alignment is checked separately. The replacement Windows build passes the 355 class checks, 41 model checks and 28 standalone checks, including front/side idle and casting captures, saved-class compatibility, real Fireball and matching Maelis.

Latest corrected Windows build: `T:\TEMP\DiceFree-Roadwarden-QA\Builds\FireMage-20261127-160557\DiceFree.exe`. The preceding Magician checkpoint and its screenshots document the earlier delivery. Use the corrected captures and build for current presentation review. Batch captures omit overlay UI; these checks do not replace manual balance acceptance.