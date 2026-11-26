# Basic auto-attack balance — 2026-11-26

User direction: main-stat coefficient is now 1 for Novice, Physically Blessed Novice, Magically Touched Novice, Greatblade, Defender, Thief, Mage, Priest and Acolyte. Existing flat damage, starting attributes, attack speed, damage channels, skill formulas and damage spread are preserved. Authoring helpers carry the same change.

Level 1 below means a fresh manifestation with no equipment, purchased skills or temporary effects. Core damage is the authored base plus attribute scaling; the actual raw range includes the separate class flat offsets, the 99% minimum attribute contribution, and the 95–105% class spread. Novice also retains its base-term 85–115% spread. These are pre-crit, pre-defense numbers rather than DPS.

| Class | Core formula | Level-1 core | Raw minimum–maximum |
| --- | --- | ---: | ---: |
| Novice | `1 + highest(VIT, STR, AGI, INT, SPI)` | 2 | 1.75–2.26 |
| Physically Blessed Novice | `1 + max(STR, AGI)` | 13 | 9.39–16.80 |
| Magically Touched Novice | `1 + max(INT, SPI)` | 14 | 9.38–18.90 |
| Greatblade | `STR` | 18 | 14.08–22.05 |
| Mage | `1 + INT` | 19 | 14.08–24.15 |
| Priest | `6 + SPI` | 24 | 18.83–29.40 |
| Acolyte | `1 + INT` | 17 | 16.00–17.85 |
| Thief | `5 + AGI` | 23 | 21.68–24.15 |
| Ranger | `5 + AGI` | 23 | 21.68–24.15 |
| Defender | `6 + STR` | 23 | 21.69–24.15 |
| Psion | `0.45 AGI + 0.45 INT` | 14.4 | 13.54–15.12 |
| Psionic Knight | `0.60 AGI + 0.40 INT` | 26.8 | 25.21–28.14 |
| Mirage Mage | `0.40 AGI + 0.60 INT` | 28 | 26.33–29.40 |
| Marksman | `8 + AGI` | 42 | 39.58–44.10 |

Verification: Unity imported/compiled the changed editor helpers and evaluated all 14 definitions using the actual `AttackDefinition.RawDamage` / `RawDamageRange` methods. The audit checked all nine requested coefficients and their weighted main-stat values. See [machine-readable results](level-one.json). No gameplay balance acceptance is implied.

Windows build: `T:\TEMP\DiceFree-Roadwarden-QA\Builds\Marksman-20261126-092148\DiceFree.exe`. Succeeded with 15 scenes, zero errors and 1 existing warning(s). [Build report](build-report.json) and [executable hash](delivery.json). This task did not rerun the earlier Marksman runtime combat suite.
