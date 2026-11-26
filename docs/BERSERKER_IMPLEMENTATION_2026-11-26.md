# Berserker implementation — 2026-11-26

Greatblade now advances into a separate level-one Tier III Berserker. The existing Greatblade keeps its level, skills, inventory and Momentum timeline. Berserker starts with no purchased ranks, one skill point, an empty 100-point Rage pool and a bound Ashscar Greataxe. It has no simultaneous Mana or Momentum pool.

## Finding and unlocking the class

Gorren Ashscar is a weathered Orc Warrior camped off the road north of Trimery, near the Quillboar frontier, at `(240.10, 0.13, 238.00)`. A Greatblade of level 60 or higher can accept **Trial of Fury**. Enter Crimson Crucible through its normal waiting room, defeat the first two guardians, then defeat **Bristlebreaker in cell three**. An eligible living quest owner participating in that cell receives credit even when another party member lands the killing blow. Credit is saved before the next gate opens. The normal Abandon action is sufficient afterward; bosses four through six and the final treasury are unnecessary. Return to Gorren to unlock Berserker.

The mentor participates in teacher discovery, quest markers and atlas icons. The class has its own menu preview, portrait, resource profile, skill provider, manifestation save state and development-room registration. Repeated turn-in cannot duplicate the class or starter item.

## Combat and progression

All five skills start at rank zero. The approved staggered gates allow one point per class level through level 150, with exact caps of 35 / 30 / 30 / 25 / 30.

| Skill | Maximum rank | Behavior | Rage | Recovery |
| --- | ---: | --- | ---: | ---: |
| Wide Arc | 35 | Ordinary-accuracy frontal cleave; no critical hits | 10 | 7 seconds |
| Spin Dash | 30 | Navigate through enemies along a reachable route; hit each enemy once; gain 2 Rage per successful unique target, capped at 20 per cast | 0 | 15 seconds |
| Rage Breaker | 30 | Guaranteed hit against a valid melee target; only explicit external critical grants apply; successful hits grant `2 × rank` percent attack speed for four seconds | 30 | 12 seconds |
| Anger Management | 25 | Great-weapon mastery, continuously updated missing-HP damage and improved Rage generation/retention | — | Passive |
| Coffee Shield | 30 | Five-second shield with the magical-coffee element and an amber ground sigil; expiration does not heal | 25 | 25 seconds |

For `n = rank − 1`, raw ability damage before mitigation is:

- Wide Arc: `12 + 4n + (1.8 + 0.10n) × STR`.
- Spin Dash: `8 + 3n + (1.2 + 0.08n) × STR`.
- Rage Breaker: `25 + 8n + (2.5 + 0.16n) × STR`.

Anger Management multiplies basics and all three damaging abilities once by `(1 + W) × (1 + B)`. `W = 0.005 × rank` requires an actual equipped eligible two-handed axe or sword. `B = 0.4 × rank / 25 × clamp01((1 − currentHP/maxHP) / 0.99)`. At rank 25 this gives 1.125× at full HP with a great weapon, or 1.575× at one percent HP. Healing immediately reduces the missing-HP bonus. Coffee Shield absorbs `maxHP × (0.05 + 0.30 × (rank − 1) / 29)`.

Successful basics generate `1 + ceil(0.1 × Anger rank)` Rage, including valid hits fully absorbed by an enemy shield. Incoming hostile damage generates `100 × (actual HP damage + shield absorption) / own maxHP`. Self, friendly, missed and invulnerable damage do not generate Rage. After combat grace, out-of-combat decay falls from 5 to 0.1 Rage per second across Anger ranks 0–25 and preserves fractional values. Death and manifestation loading clear Rage, haste, shields and active casts.

Recovery starts when an accepted action ends. Interrupted actions retain their cost and begin recovery on interruption. Rejected targeting spends nothing and starts no cooldown. Spin Dash releases temporary movement speed, passage and avoidance changes on completion, interruption, death or class switch. Skill descriptions include spell names, scaling, current raw totals, costs, cooldowns and rank gates.

Level-one attributes are VIT 25, STR 34, AGI 10, INT 5 and SPI 7, growing by 3 / 6 / 1.2 / 0.5 / 0.8 per level. The established 15 HP per VIT produces **385 HP without equipment**. Basic attack core damage is **34 without equipment**, from `1 × STR` with zero class flat damage; the starter axe adds 3. Eligible inherited armor can increase these values. Berserker accepts two-handed axes and swords and rejects one-handed main weapons and offhands. The existing universal Wolf-Fang Greatsword is classified as a two-handed sword.

## Art and animation

Both Gorren and the playable class use the approved **Orc Warrior by Callan McEntee**, with a weathered material treatment on the mentor. Source receipt and attribution accompany the admitted canonical package. The original spear and display plinth were removed; existing UVs and weights were retained. The source control rig was baked into a clean 71-bone humanoid bind pose, normalized to 2.08 metres, with 8,528 triangles and three 2048-pixel textures. The Unity avatar is valid and human. Existing humanoid idle, walking, two-handed attack, spin, hit and death clips drive it. A custom steel-and-bronze greataxe and sword alternative follow the animated right hand, with the left hand constrained onto the shaft. Equipped weapon family controls which is shown.

Preparation is reproducible through `tools/authoring/prepare_berserker_orc.py`; Unity data authoring lives in `tools/authoring/BerserkerAuthoring.cs`.

## Delivered validation

Windows development build: `T:\TEMP\DiceFree-Roadwarden-QA\Builds\Berserker-20261126-162640\DiceFree.exe`.

- **392 static checks**, including every rank gate and level budget, class stats, formulas, resource profile, item compatibility, scene registration, mentor reachability and humanoid clip/avatar validity.
- **398 player runtime checks**, including actual normal-waiting-room entry, a local second participant landing the Bristlebreaker killing blow, saved quest credit before returning to Gorren, all 150 purchases, parent skill preservation, damage/accuracy/crit rules, shields, Rage, cooldowns, interruption, movement cleanup and death.
- **25 fresh-process reload checks**, including disk persistence, resource reset, real sword equipment, weapon appearance, one-handed/offhand rejection, explicit item critical grants, humanoid skinning and two-handed grip.
- Windows build succeeded with zero build errors; the main checkout imported with exit code zero and no C# compile errors. Source receipts compare the main checkout against the tested QA project. Unrelated package-manager settings and recovery files were preserved.

Reports and captures are in [the evidence folder](validation/berserker-2026-11-26/). These are automated checks using disposable copied saves, including local party participation. They do not establish manual balance acceptance or live network co-op acceptance. Batch-rendered screenshots show world geometry and omit screen-space overlay UI.
