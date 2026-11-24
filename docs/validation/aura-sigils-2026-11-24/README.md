# Ground aura sigils — 24 November 2026

All current aura sources now have an animated sigil beneath their owner:

| Aura | Ground effect |
| --- | --- |
| Mana Attunement | Cyan braided crescents and a four-point mana star |
| Elemental Weaving | Six-colour elemental seal with interlocking triangles |
| Hold the Line | Brass fortification hexagon, blue shield and cross |
| Speed Presence | Violet psychic trails and silver rotating chevrons |
| Empowered Ice Aegis | Branching frost snowflake and icy hexagon beneath the shield recipient |
| Radiation | Toxic green rotating blades and unstable core beneath the affected unit |

The effects use slowly moving arcs, orbiting glyphs, pulsing brightness, and
colour-specific highlights. Both Psionic Knight and Mirage Mage use Speed
Presence. Normal Ice Aegis retains its shield presentation; its new frost sigil
appears only when Grace empowers the damaging aura.

![All six effects, cropped from actual gameplay captures](auras-preview.png)

These close-ups use a 22 m gameplay-camera zoom and are enlarged twofold for
comparison. The fixture uses the same Knight model to compare the presentations.
[Default 40 m camera view](mana-attunement-gameplay.png),
[night view](speed-presence-night.png), and
[World Two view](speed-presence-world-two.png) show normal gameplay framing.

## Integration

`GroundAuraVisual` provides the shared renderer and lifecycle. Persistent auras
attach it when their source component initializes. Eligibility follows learned
ranks and the existing active aura state. Priest shield effects and radiation
use their actual effect lifetime; breaking the Priest shield removes its sigil.
Dead, disabled, inactive, or concealed owners hide the presentation, and living
restored aura sources regain it.

The 8-by-8 mesh patch samples walkable terrain and follows the owner, including
slopes and the distant coordinates of World Two. Its compact 1.5–1.75 m visual
radius is decorative; gameplay aura ranges, stat bonuses, damage, and stacking
rules retain their existing values. The old Defender range-line presentation is
replaced by the shield motif.

The shader is procedural and uses a material shipped in `Resources/AuraVfx`.
There are no new image or model dependencies. Runtime meshes and material
instances are disposed with their effect. Presentation layer 30 keeps the sigils
out of pointer geometry, minimap renders, portraits, and world-face photographs.
No physical colliders, lights, or navigation obstacles are added.

## Verification

All **51 core runtime checks** and **4 additional travel checks** passed, with
zero captured Unity runtime errors. See [aura-review.json](aura-review.json) and
[aura-travel-review.json](aura-travel-review.json).

The isolated development player used a disposable copied save. Real progression
APIs learned each passive; the Priest fixture cast normal and empowered shields
through its actual spell entry point. Tests covered source activation, class
changes, concealment, movement, terrain alignment, shader inclusion, shield
breakage, timed radiation expiry, actual hero death/restoration, and the real
World Two scene crossing. Screenshots were inspected in daylight and at night.
These automated fixtures are not a manual playthrough of every class.

Windows build `AuraSigils-20261124-170012` includes all 14 enabled scenes and has
zero build errors. Its existing warning concerns eight meshes without pre-baked
collision; see [build-report.json](build-report.json). The main checkout's Unity
import also exited successfully. Unrelated local package settings and recovery
files were preserved and excluded from this change.

Reproduction tools: `tools/authoring/AuraVisualAuthoring.cs`,
`tools/validation/GroundAuraVisualReview.cs`, and
`tools/validation/MakeAuraContactSheet.ps1`.
