# Greatblade first in-Unity prototype — implementation notes (2026-10-13)

**Branch:** `feature/visual-overhaul-vis00`. The prototype is real Unity content; the full Greatblade combat/advancement implementation is **not finished**.

## What was implemented

- **Level gates and skill rank schedule scaffold** in `Assets/_DiceFree/Scripts/Progression/GreatbladeProgressionRules.cs`: Wide Arc, Spin Dash, Greatsword Mastery and Heavy Rhythm available at L1; Overhead Breaker L6; Battle Cry later. Uneven rank cadence, evolving Mastery bonuses at ranks 2–4, Heavy Rhythm gain/decay scaling. **No threshold Momentum buffs.** Tier IV+ is required for reserved ultimate slot 9. This is static rule metadata and tests, not a connected playable skills progression yet.
- **One shared Greatblade base character** for eventual playable class **and** retired swordswoman NPC in Cornberg (same underlying model, with different equipment support planned).
- **Female Warrior Princess** asset source `Assets/AllStarCharacterLibrary/Models/Characters/A03.FBX`. Its auto Humanoid import failed, so `GreatbladeHumanoidRig` maps 21 bones and builds a validated Unity Humanoid Avatar saved as **LOCAL LICENSED** `Assets/_DiceFree/Resources/GreatbladeLicensed/GreatbladeFemaleAvatar.asset`.
- **Two-handed animations** from `FREE - Modular Character - Fantasy RPG Human Male` / Blink (Idle, WalkForward, MeeleeAttack_TwoHanded, SpinToWin_TwoHanded) assigned to local animator controller. Retargeting capability/clip humanoid metadata validated; **actual animation style/hand placement still needs visual QA**.
- **Oversized sword** from `Free Low Poly Swords`: `Assets/PurePoly/Free_Swords/Prefabs/PP_Sword_0363.prefab`, attached to the female rig's **R Hand** bone. Sword geometry scaled to approximate 2.75 world units for a Warcraft III-like exaggerated silhouette; pose, grip and orientation should be visually reviewed and adjusted if needed.
- Shared LOCAL resource prefab `Assets/_DiceFree/Resources/GreatbladeLicensed/GreatbladeHero.prefab` used by runtime visual component `DiceFree.Visuals.GreatbladePrototypeVisual`. Public clones without the original Asset Store art get an original code-drawn humanoid with large sword, without missing serialized vendor references.
- Tracked scene `Assets/_DiceFree/Scenes/GreatbladePrototype.unity` has a camera, lights, stage and **two identically sourced Greatblade appearances** side by side: one labelled playable class prototype, the other retired swordswoman. Open this scene in the Unity Editor, then enter Play Mode to instantiate visuals. The visual component has a preview-spin property; actual skill combat is not enabled here.

## Tests

- Unity 6000.6.3f1 recompile: **zero errors, zero warnings**.
- `dicefree.greatblade.art-survey`: female mesh contains 6 SkinnedMeshRenderers; all Blink 2H animation clips and selected sword prefab found.
- `dicefree.greatblade.rig-human`: **21 mapped human bones**, **64 total skeleton bones**, successful valid Humanoid Avatar.
- `dicefree.greatblade.prototype.install`: preview scene and local vendor prefab authored successfully, with `avatarHumanoid=true`, four animation states and shared model resource.
- `dicefree.greatblade.prototype.validate`: **DICEFREE_GREATBLADE_VISUAL_AND_PROGRESS_OK**, verifies valid female humanoid avatar, real source clips with Humanoid motion, right-hand sword socket, shared scene model, Greatblade level gates (L1/L6), 2 passive roles, rank cadence and no Tier II/III ultimate.
- **No full visual in Game View, gear attachment under animation, actual Spin Dash movement/damage, skillbar rank spending or world NPC mentor quest was tested/implemented**. Avoid claiming those exist.

## Class kit and art production

See `docs/GREATBLADE_PROTOTYPE_DESIGN_V03.md` for the user-approved six initial abilities and evolving passive design. An actual Greatblade avatar, two-handed equipment item and the retired mentor should eventually bind the shared visual base to class/equipment state, without duplicating assets. A live ground-targeted Spin Dash still needs collision-safe movement and AoE hits along the traveled route. A real Overhead Breaker still needs Momentum consumption, and Greatsword Mastery rank 4's shield vs heal is undecided.

## Locally licensed art + public repository

These are **gitignored, local-only**: `Assets/AllStarCharacterLibrary/`, `Assets/Blink/`, `Assets/PurePoly/`, `Assets/_DiceFree/Resources/GreatbladeLicensed/`. Their original downloaded Unity packages also remain in the user's Asset Store cache. **Never git-add them**. Tracked editor authoring script imports/adapts sources if present; tracked scene/runtime adapter fall back gracefully on public clones.

**Do not overwrite active DiceFree profiles** (`EditorProfiles/primary-echo.json`); no class/save schema or existing Cornberg gameplay code has been modified by this visual prototype. **Never merge to main** without approval.
