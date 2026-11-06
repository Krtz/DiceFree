# Arborium guild forestry commissions — 2026-11-07

The Adventurers' Guild now has a playable three-quest chain linking Arborium's reception and planning room with Trimery's sawmill, southern timberline and ruined western highway. The user authorized continued game development after proposing a guild-led forestry route toward World 2.

## Playable chain

Speak to the **Guild Steward inside the Adventurers' Guild reception**, beside the commission board. The existing outdoor Registrar still handles the original arrival report. All playable classes can accept these commissions; later offers require the previous commission's explicit turn-in.

| Quest | Objectives | Reward |
| --- | --- | --- |
| The Timber Commission | Speak to the Guild Foreman beside Trimery's sawmill; return to the Guild Steward | 1,500 XP, 75 gold |
| Room to Work | Clear five blue-marked deadfalls around Trimery's southern timberline, in any order; report to the Foreman; return to the Steward | 2,500 XP, 100 gold |
| A Road Worth Reopening | Survey the wreckage beside Trimery's western gate, the southern timberline and the existing large map table on the Guild's fourth floor; return to the Steward | 1,800 XP, 125 gold |

Work begins through the normal world interaction controls. Deadfall clearing takes three seconds; recording a survey takes 1.8 seconds. A progress bar and Cancel button appear. Moving, attacking, losing the ability to act, dying or taking HP damage interrupts the work without credit. Completed deadfalls become cut logs, stacks, stumps and wood chips. Recorded survey sheets gain visible map markings. Blue rotating/bobbing markers identify unfinished sites while their commission is active.

Each site records its own durable objective credit. Repeating a site cannot replace an unfinished one, and partial progress survives a menu exit or saved resume. The visible before/after state follows the current manifestation's quest history; it is not an account-wide or network-synchronized world alteration.

## Continuity

The Ogre Roadwarden deliberately destroyed the western highway under orders. He remains approachable only from the Arborium side; who ordered him and why are still unknown. This chain does not add a different cause, a magic stone reacting to the Echo, a new mastermind or a Roadwarden encounter. Neither the shortcut nor World 2 passage opens in this milestone. The survey and cleared work areas give later restoration/expedition content a playable foundation.

The original north → west → south Briarback detour, existing city services, gates, furnishings and navigation remain in place. The final survey reuses the Guild's existing map table. Two original-style NPC copies and two commission boards are added; class mentors and the Registrar are not moved.

## Implementation and verification

- `ObjectiveKind.All` and `Interact` are appended to the enum, preserving the previous serialized ordinals. The shallow All group stores individual leaf counts in the existing quest-progress format. Legacy Any objectives keep their existing either/or behavior.
- `TimedInteractionTarget` emits a semantic completion event after interruptible work. `QuestWorkGate` controls quest eligibility and repeated-site rejection. The UI and visual adapter consume those systems without owning quest completion.
- Quest assets are in `Settings/Arborium/GuildForestry`; `tools/authoring/GuildForestryAuthoring.cs` contains bounded authoring and refinement. All new timber/map/board geometry uses the installed first-party city/oak palette; no new third-party package is introduced.
- The initial Editor acceptance passed **2,015 checks**, all 120 five-site permutations, duplicate-site rejection, legacy Any regression, movement/damage/cancel interruptions, exact quest rewards, partial and completed saved resume, and nine actual walks totaling **259.2m**, including the stairs to the fourth floor. Zero Unity errors were observed. The final standalone run verifies the refined existing-map-table position.
- Architecture, managed references, assembly ownership and dependency policy passed.

The original 41,733 scene nodes remain. Only the player component list/journal, two existing floor child lists and scene-root list gain new references; existing scene-node data is otherwise unchanged. Navigation remains byte-identical. The scene recovery is retained locally under `Settings/Recovery/Cornberg-before-guild-forestry-20261106.unity`.

Final Windows build: `Builds/BriarbackMilestone-20261107-000052/DiceFree.exe`, eight scenes, 476,593,880 bytes, **zero errors and warnings**. The isolated standalone acceptance passed all three quests, five clearing sites, three surveys, interruption handling, partial and completed saved resume, exact rewards, and six actual walks totaling **247.4m**, including the refined fourth-floor map table. It observed zero Unity errors. Normal Quit exited PID 31004 with code 0. All 54 original WIP files and seven protected settings are preserved.

Combined results and curated standalone captures are in `VisualBaselines/GuildForestry`. Work is local on `feature/arborium-guild-forestry`; no push or merge is included.
