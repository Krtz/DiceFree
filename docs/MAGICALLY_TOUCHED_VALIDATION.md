# Magically Touched Novice validation (#52)

Validated on 2026-10-07 in Unity 6000.6.3f1 on `feature/52-magically-touched`, stacked directly on completed #51 commit `77716b2f4a7b102407bd582862fd5c3231c74bac`.

## Implemented slice

- Magically Touched Novice is a real playable Tier-1 manifestation rather than the #50 shell.
- Dedicated Novice-family presentation:
  - slightly smaller overall silhouette;
  - subtly reduced chest/arm/leg proportions;
  - dedicated dark caster-like shirt material;
  - shared valid humanoid avatar/controller;
  - no gameplay collider/root-motion ownership in the presentation.
- Settled primary stats:
  - level 1: 10 VIT / 5 STR / 5 AGI / 13 INT / 13 SPI;
  - average growth: +1 VIT / +0.5 STR / +0.5 AGI / +2 INT / +2 SPI.
- Ranged magical basic attack uses whichever is higher of INT/SPI.
- Shared Mana resource framework from #51 now supports both Tier-1 classes with class-specific profiles.
- Magically Touched Mana prototype:
  - maximum = 80 + 2 x INT + SPI;
  - regeneration/s = 5 + 0.15 x INT + 0.15 x SPI;
  - level-1 base profile = 119 maximum / 8.9 per second before Mana Attunement;
  - current Mana uses the Persist retention policy.
- Five six-rank skills with one class skill point per Magically Touched level:
  - Magic Sand;
  - Mend;
  - Fire Elemental Imbuement;
  - Ice Burst;
  - Mana Attunement.
- Magic Sand is a Nature-element INT spell with +7.5 percentage-point miss chance per rank for 5 seconds.
- Mend is an ordinary SPI-only self/ally heal and does not resurrect.
- Fire Elemental Imbuement applies a source-keyed temporary Fire basic-attack augment. The base hit still resolves normally; same-source reapplication refreshes/replaces instead of stacking duplicate copies.
- Ice Burst is a delayed ground-targeted Ice AoE with a 0.75-second prototype warning, 3.5 m radius, and 2% Movement Speed slow per rank for 4 seconds.
- Mana Attunement grants +20 maximum Mana, +0.75 personal Mana/s and +0.25 nearby ally Mana/s per rank in an 8 m radius.
- Multiple Mana auras use strongest-applicable-aura semantics rather than additive stacking.
- Skill ranks, current Mana and class-local passive state persist with the Magically Touched manifestation.
- Cooldowns, delayed-cast coroutine state, temporary buffs/debuffs and targeting state remain transient and reset on manifestation load.

## Confirm-cast targeting

During #52 playtesting, the existing skill bars were found to auto-cast unit-target abilities on the ordinary selected target. The prototype now uses a shared WC3/League/Dota-style normal/confirm cast flow instead.

`SkillTargetingController` is class-agnostic and provides:
- hostile-unit targeting;
- friendly-unit/self targeting;
- ground targeting;
- a cursor reticle while targeting;
- left-click confirmation;
- right-click or Escape cancellation;
- cancellation on modal/input suppression, focus loss, class/session reset;
- input consumption so a confirmation/cancel click does not also issue movement or ordinary target-selection commands.

Ordinary selected targets remain useful for basic attacks and Tab targeting but are no longer consumed implicitly by abilities. The low-level Novice, Physical and Magical casters also refuse implicit selected targets, so the rule is enforced beneath the UI as well.

Current class behavior:
- Novice STR stun + Magic Sand: hostile reticle;
- Novice AGI buff + SPI heal: friendly reticle;
- Heavy Strike: hostile reticle;
- Arrow Rain: ground reticle;
- Guard + Quickening: immediate self-casts;
- Magical Magic Sand: hostile reticle;
- Mend + Fire Elemental Imbuement: friendly reticle;
- Ice Burst: ground reticle.

The dedicated targeting regression proves that an already-selected hostile takes no damage and no resource is spent when the skill hotkey is pressed; only the explicit confirmation click performs the cast. It also proves RMB/Escape cancel ground targeting without Mana/cooldown consumption.

## Focused validation

Passed on the final code:

- `DICEFREE_MAGICAL_AUTHORED_OK`
- `DICEFREE_MAGICAL_DATA_OK`
- `DICEFREE_MAGICAL_PLAYMODE_OK`
- `DICEFREE_SKILL_TARGETING_OK`
- `DICEFREE_PHYSICAL_PLAYMODE_OK`
- `DICEFREE_NOVICE_SKILLS_PLAYMODE_OK`
- `DICEFREE_ADVANCEMENT_PLAYMODE_OK`
- `DICEFREE_CONTROLS_PLAYMODE_OK`
- `DICEFRE_ARCHITECTURE_OK`
- `DICEFRE_ARCH_POLICY_SELFTEST_OK`
- `DICEFREE_MANAGED_REFERENCE_OK`
- `ISSUE36_SAVE_RELOAD_OK`
- `ISSUE36_ROUTE12_OK`
- `ISSUE36_RUNNER_OK`
- items/inventory/equipment/gold regression
- Q4 surge/reward/elite/world-drop regression

The dedicated #52 Play Mode harness proves:
1. a preserved Novice can create Magically Touched while the already-existing Physical manifestation remains intact;
2. the real three-manifestation roster coexists and each class retains independent state;
3. Magically Touched activates its dedicated presentation and higher-INT/SPI ranged magical basic attack;
4. Mana maximum/regeneration and steep spell-cost curves match authored prototype data;
5. level-5 five-point allocation across four actives + Mana Attunement is valid;
6. Magic Sand damages a real hostile and applies the correct rank-1 accuracy penalty;
7. Mend performs an ordinary self/ally heal with SPI scaling;
8. Fire Elemental Imbuement adds Fire damage to normal basic attacks without replacing the base hit;
9. delayed Ice Burst damages multiple real Cornberg enemies and applies its movement slow;
10. Mana Attunement changes personal maximum/regeneration and strongest-wins ally aura behavior;
11. Magical ranks/current Mana serialize independently of the preserved Novice and Physical manifestations;
12. StartMenu displays/loads the saved Magically Touched manifestation;
13. Cornberg reload restores class, level, ranks, Mana/passive contribution and dedicated presentation;
14. temporary cooldowns, spell/session effects and targeting state are reset across the load boundary.

The Magical harness was hardened to explicitly unpause the Unity Editor and restore `Time.timeScale = 1` on Play Mode entry. This removed an intermittent remote-validation hang caused by inherited Editor pause/scaled-time state, not a gameplay spell deadlock. Novice/Physical validation harnesses were hardened similarly where needed.

## Windows Development build

`DICEFREE_MAGICAL_PLAYER_BUILD_OK` passed on the final caster/targeting code:

- Unity: 6000.6.3f1
- target: Windows x64 Development
- result: Succeeded
- errors: 0
- warnings: 5 (existing Unity 6.6 editor-tool deprecation/build-environment warnings)
- total output: 188,638,878 bytes
- first scene: `Assets/_DiceFree/Scenes/StartMenu.unity`
- scene count: 2
- executable: `%TEMP%/DiceFree-MagicallyTouched-Issue52/DiceFree.exe`

## Explicitly still provisional / out of scope

- Current coefficients, Mana costs, cooldowns, durations, resource formulas and aura values are playtest tuning, not final balance.
- Skill icons, bespoke VFX, audio and animations remain future presentation work.
- Current targeting reticle is functional prototype IMGUI; final range/radius previews, cursor art and quick/semi-quick cast options belong to later controls/HUD work.
- Full weapon/focus/equipment-family permissions are not implemented here.
- The current skill/allocation/resource UI is prototype UI; the proper generic HUD remains the next major UI milestone.
- Level-30 descendants remain future class implementation work.
- Multiplayer remains future work.
- #52 stays open/unmerged for Axel review until explicit merge authorization.
