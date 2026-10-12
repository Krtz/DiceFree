# World 1 follow-on design interview — Village 2, Tier II classes and Wolf Dungeon
Date: 2026-10-12. **Status: DISCUSSION DRAFT — no world/class/dungeon authoring yet.**

## Existing approved foundations (not new decisions)

- Village 2 location is reserved beyond the northeastern Ironpine approach (near the clearing centered around world X223/Z149); Ironpine road extends toward it. Village 2's identity, services, quests, architecture and NPC roles remain open.
- Ironpine Forest is level 12–20 wildlife territory: Grey and Dark Wolves, Quillrats, one very dangerous Alpha, road encounters that can be bypassed. No wolf/quillrat item drops in the overworld yet.
- Tier I is **Physically Blessed Novice** or **Magically Touched Novice**, starting at level 1 after first advancement from level-10 Novice.
- **Tier II advancement begins once a Tier I manifestation reaches level 30**; each class branch creates a new level-1 manifestation. The parent class/timeline remains playable. No free arbitrary switching mid-dungeon.
- Intended later classes include Ranger (wood elf), Wizard (high elf) and Berserker (orc), but their exact branching/requirements are still open. One sex per class in initial art pipeline, enlarged/expressive equipment silhouette, class-specific resources and combat kits.
- Dungeon/raid framework targets 1–4 players, physical entrances, one live instance per dungeon stable ID, a 60-second staging period with test dummy, roster/equipment lock at run start, and wipe-based reset by default.
- New **asset-first policy** applies to models, NPCs, villages, UI, monster animations, and dungeon props. Look at downloaded packs, then edit/retarget instead of starting from scratch.

## Proposed starting direction: Village 2

- Narrative purpose: a credible, modest inhabited settlement beyond an openly dangerous forest, distinct from pastoral Cornberg; functional rest/economy/progression waypoint before the next threat area.
- The road reaches a village **with a Dice revive stone** so the shared Return Home skill can remember the latest activated stone. Village details/quest pickup and world map placement should be decided before world authoring.
- Potential identities (choose one or hybrid):
  A. *Timberwatch*: rough logging/hunter frontier village, watchtowers, wolves at its edge.
  B. *Mossford*: riverbank trade village, old wooden bridges, caravan inn, wary travellers.
  C. *Briarhold*: fortified forest-edge palisade, militia training ground, hints of nearby quilboars.
  D. User's different direction.
- Quest and level curve: the new settlement's intro likely picks up the Cornberg Runner quest, becomes a safe node after the level-12–20 Ironpine journey, with later humanoid **quilboars beyond the village**, not the quillrat animals in Ironpine.

## Proposed starting direction: Tier II

- Tier II should feel like a major kit/class/race silhouette change at Tier I level 30 rather than a +damage rename. Dedicated model, movement/attack animations, class-specific resources, four or more active skills, passive and advancement test; current action bar leaves capacity for universal commands and future skills.
- Initial branch candidates for user to approve:
  - **Physically Blessed → Ranger (wood elf)**: mobile physical ranged combat, traps/marksmanship, positioning.
  - **Physically Blessed → Berserker (orc)**: relentless melee aggression, rage/tempo management.
  - **Magically Touched → Wizard (high elf)**: stronger spell patterns/area control, distinct resource or casting decisions.
- Additional Magical branch(es), hybrid classes, secret Tier II routes and the official first available set require user direction. Keep the known class aesthetics as intentions, not presumed final decisions.

## Proposed starting direction: Wolf wave-survival dungeon

- Fantasy: an Ironpine cave, moonlit ravine or abandoned hunter outpost where a pack surrounds the party. Survive escalating timed/triggered **waves** with wolf runners, Grey/Dark variants and unique pack mechanics, then an authored final encounter.
- Wolf/Alpha ordinary drops remain disabled. Dungeon victory can later feed its own rewards through the existing private chest-style reward room, if user approves.
- Difficulty must work solo and 1–4 players. Wave composition, pacing, downed player behavior, respawn limits, bonus objectives and final victory condition are **not chosen yet**. A failed party run resets the waves under normal dungeon rules.
- Possible formats to decide:
  A. **Ten authored waves** with a short break between waves, final unique wolf boss.
  B. **Timed siege**: survive 8–12 minutes of steadily escalating packs, final boss or extraction.
  C. **Defense objective**: protect a revive stone / beacon from wolves while fighting.
  D. **Endless optional score mode** after a finite clear.
- Unique boss could be an elder wolf or pack Matriarch rather than reusing the roaming Ironpine Alpha identically. Name/lore/abilities must be designed with user before implementation.
- Audio/arena and animals should use the downloaded pack assets first, with retargeting/editing and original placeholders only when necessary.

## Design interview — awaiting user input

1. **Village 2:** Which identity should anchor the settlement: frontier logging/hunters, river traders, fortified militia village, or your own? Is it a true safe hub like Cornberg or a more dangerous outpost?
2. **Tier II branches:** Do you want to begin with the three established ideas (**Ranger, Berserker, Wizard**), and should Magically Touched have a second non-Wizard branch ready at the same time?
3. **Wolf dungeon:** What is the clear condition: fixed waves, timed survival, or defending an object? Should a new named wolf boss end the run, or should the escalating waves themselves be the boss?
4. **Wolf dungeon place/reward:** Cave vs ruined hunter compound vs outdoor forest arena? Standard chest-selection reward room on success or something different?

**Nothing in these proposals is canon until user approves.** Do not build Village 2, Tier II class assets/kits, or the wolf dungeon from the proposals alone.

## Concrete local asset-pack leads (catalog inspected, **not imported yet**)

| Next content | Promising downloaded packs to evaluate first | Adaptation work to expect |
|---|---|---|
| Village 2 | `Medieval Village Building Kit`, `Stylized Medieval Building Pack 72 Modular Meshes Low Poly`, `Low-Poly Medieval Market`, `The Wood Cabin`, `Medieval Containers Pack with Wagon` | Rescale to Cornberg, recolor, combine modular houses/fences/props, optimize collisions and URP materials |
| Village 2 residents | `Low Poly Medieval Peasants - Free`, `FREE - Modular Character - Fantasy RPG Human Male`, `MedievalAssassinGirl` | Retarget walks/gestures, make readable specialists/merchants rather than duplicating every NPC |
| Tier II classes | `Elven Assassin Low poly`, `Low Poly Fantasy Warrior`, `Low Poly Modular Armors - Free`, `Free Stylised Fantasy Weapons`, `Fantasy Staff Lite` | Test elf/orc race silhouettes, recolor/kitbash/retarget character assets, then build class-specific models |
| Wolf wave dungeon | `EMBERHOLD - FREE - Low Poly Modular Dungeon Kit`, `Stylized Dungeon Props Pack`, `Low Poly Pine Forest`, `Realistic Furry Wolf FREE sample REALISTIC FOREST ANIMALS` | Choose cave/outdoor arena after user decision, build blocking/sightlines, reuse wolf rigs and compose wave encounters |

These are **catalog leads, not final visual choices**. License/import validation and actual asset previews come before production. Existing assets may be mixed stylistically as established Diceworld lore.
