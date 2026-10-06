# Advancement + start-menu validation (#50)

Validated on 2026-10-06 in Unity 6000.6.3f1 on `feature/50-advancement`, based directly on completed Novice-skills commit `63593b1ff979b7c545edd81c718f34469a91947e`.

## Implemented slice

- Data-driven `ClassCatalog` with the current three prototype classes.
- Data-driven level-10 Novice advancement edges to:
  - Physically Blessed Novice (`class.physically-blessed-novice`)
  - Magically Touched Novice (`class.magically-touched-novice`)
- Tier-1 actor-definition shells use the settled starting/growth primary attributes only. Their final combat kits, Mana, model/presentation and class-specific attacks remain separate implementation work.
- Echo-level manifestation roster section `echo:manifestations` version 1 stores the active class ID and parent -> child branch history without bumping Echo/manifestation schema v5.
- Atomic manifestation fork:
  - parent state is saved/preserved;
  - child starts level 1 / XP 0;
  - child copies manifestation-owned quests/world state, resurrection anchor, gold, inventory and equipment;
  - Novice class-skill ranks do not numerically carry into Tier 1;
  - child item copies receive new instance IDs and equipped references are remapped;
  - duplicate target-class creation is rejected before mutation;
  - parent/child states diverge independently after the fork.
- Active manifestation can be loaded from the roster without mutating another class's durable state.
- Novice-specific skill runtime/UI disables cleanly when the live actor is a non-Novice manifestation.
- Common save-root resolution is shared by gameplay persistence and the start menu.
- Dedicated `StartMenu` boot scene:
  - fresh profile: `Start as Novice`;
  - existing Echo: list saved manifestations with class name and level;
  - last-active manifestation is marked;
  - selecting a saved manifestation updates active-roster metadata transactionally and enters Cornberg;
  - unresolved class content is preserved and shown unavailable rather than deleted.
- Build Settings now boot `StartMenu` before `Cornberg`.
- In-world advancement UI only presents eligible new blessings; arbitrary manifestation switching is intentionally not exposed as a gameplay button.

## Focused validation

Passed markers:

- `DICEFREE_ADVANCEMENT_AUTHORED_OK`
- `DICEFREE_ADVANCEMENT_DATA_OK`
- `DICEFREE_ADVANCEMENT_PLAYMODE_OK`
- `DICEFRE_ARCHITECTURE_OK`
- `DICEFRE_ARCH_POLICY_SELFTEST_OK`
- `DICEFREE_MANAGED_REFERENCE_OK`

The #50 Play Mode harness uses a fresh isolated save root and proves the real player flow:

1. boot the authored StartMenu with no save;
2. start a new Echo through the real `Start as Novice` path;
3. enter Cornberg as Novice;
4. prove level 9 is not eligible and level 10 exposes both Tier-1 edges;
5. establish nontrivial Novice skill, quest, gold, inventory and equipment state;
6. fork to Physical at level 1 and verify copied state/new item identity;
7. mutate Physical state, return to preserved Novice and prove the parent is unchanged;
8. reject duplicate Physical creation without touching the existing child;
9. fork the preserved Novice into Magical and verify an independent copied item identity/state;
10. mutate Magical and prove all three timelines diverge independently;
11. exit Play Mode;
12. boot the real StartMenu from the same save;
13. verify Novice level 10, Physical level 1 and Magical level 1 are all shown and Magical is marked last-active;
14. choose Physical through the actual menu API;
15. load Cornberg and prove the preserved Physical manifestation is the active saved state;
16. switch through all three durable records under validation and verify each retained its independent state.

## Existing regressions

Passed after the persistence/manifestation refactor:

- `DICEFREE_NOVICE_SKILLS_PLAYMODE_OK`
- `DICEFREE_CONTROLS_PLAYMODE_OK`
- existing items/inventory/equipment/gold regression (`items` suite)
- existing Q4 surge/reward/elite/world-drop regression (`q4` suite)
- `ISSUE36_ROUTE12_OK`
- `ISSUE36_RUNNER_OK`
- `ISSUE36_SAVE_RELOAD_OK`

The old save validator temporarily removes persistence to test shutdown/dead-save behavior. #50 adds a required advancement dependency, so the validator now removes its temporary advancement presentation/controller before removing persistence; the original persistence test semantics remain intact.

## Windows Development build

`DICEFREE_ADVANCEMENT_PLAYER_BUILD_OK` passed:

- Unity: 6000.6.3f1
- result: Succeeded
- errors: 0
- warnings: 5 (existing prototype/build-environment warnings)
- total output: 188,534,398 bytes
- first scene: `Assets/_DiceFree/Scenes/StartMenu.unity`
- scenes: StartMenu + Cornberg
- executable: `%TEMP%/DiceFree-Advancement-Issue50/DiceFree.exe`

A real built-executable smoke test used an isolated empty save root:
- the executable visibly booted to the start menu;
- `Start as Novice` was clicked in the built player;
- Cornberg loaded successfully as a level-1 Novice;
- the initial Echo save was written;
- relaunching the same executable/save visibly showed `Choose a manifestation`, Novice level 1, `Last active manifestation`, and `Play Novice`.

## Explicitly still provisional / out of scope

- Tier-1 class models/forms.
- Tier-1 Mana/resource implementation.
- Physically Blessed and Magically Touched skill kits/gameplay implementation.
- Final Tier-1 basic attacks, equipment permissions and combat balance.
- Final mountain blessing quest/ritual/NPC presentation.
- Final title/menu art, character preview cards, archive/delete/recreate UX and multiple-Echo profile UI.
- Production HUD.
- Multiplayer.
- No merge to `poc/cornberg` or `main` is implied by this document.
