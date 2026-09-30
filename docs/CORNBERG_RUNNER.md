# Cornberg — Runner Returns

This additive story beat follows completed Q3 and stops before Q4. It implements
the settled WORLD_1 story: Cornberg's weekly runner tried to reach the next town,
could not get through because too many Slimes blocked the road, and returned.
There is no unrelated news or larger threat.

## Play and provisional presentation

After turning in Q3, find **Cornberg Runner** beside the village green near the
east road, at `(18, 0, 6)`. The capsule, display name and placement are reversible
blockout choices, not permanent lore. The NPC body and conversation are hidden
until Q3 is completed. Right-click to approach, or use I nearby. Read the dialogue
and select **I understand.** to acknowledge it. Closing or walking away does not
grant progress. The existing tracker shows the next instruction and completion.

Dialogue: “I set out on my usual weekly run to the next town, but I couldn't get
through. There were too many Slimes along the road. I had to turn back to Cornberg.”

No XP or other reward is added by this connecting conversation. Existing Q1–Q3
reward pacing and enemy/combat data are unchanged. The runner remains available
to repeat the information after completion; repeat conversations do not replay
progression. No arrival animation, travel simulation, weekly timer or Q4 is added.

## Semantic boundary

| Purpose | Stable ID |
| --- | --- |
| NPC | `npc.cornberg.runner` |
| Conversation | `conversation.cornberg.runner-blocked-road` |
| Story record | `story.cornberg.runner-returns` (definition version 1) |
| Prerequisite | `quest.cornberg.named-slime` completed |

`ConversationTarget` uses the existing Interactor movement, range, alive and LOS
checks. Acknowledge requires an open interaction with that target and rechecks
eligibility/range. It reports one `ConversationCompleted` fact per acknowledged
opening, carrying sequence, NPC ID, conversation ID, actor and position. Mere
opening/closing emits nothing. A later acknowledged opening is a new fact.

Generic conversation code contains no quest IDs or journal calls. The optional
`QuestConversationGate` adapts authored prerequisite/version availability and
local NPC presentation. `ConversationPanel` only displays dialogue and requests
acknowledgment. The emitter and progression consumer remain separate.

`QuestJournal` consumes owner-matching facts with a new `TalkTo` objective. The
NPC content ID is required by this definition; optional conversation ID narrows
the exact exchange. Definitions can opt into acceptance on the first matching
TalkTo and completion on final objective. Existing definitions default to their
original explicit acceptance/turn-in. The runner uses both flags with one objective
and zero XP. No runner names/IDs are hardcoded in the journal. Wrong NPC, wrong
conversation and pre-prerequisite facts do not grant retroactive progress.

This is a small ordered-objective extension, not the entire future quest, dialogue,
multiplayer or event framework. Future Q4 can depend on this completed stable story
ID; no Q4 completion policy is chosen here.

## Persistence

Existing schema/manifestation version 2 stores the same quest ID/version/status/
stage/count fields. No save-schema bump, migration or persistence I/O change.
Old Q3 profiles acquire an unstarted runner record. Prerequisites still gate it.
Unknown records/sections remain preserved. An open but unacknowledged dialogue is
transient; reload leaves it uncompleted. Acknowledgment accepts and completes in
the same synchronous event, then existing autosave captures the durable state.
Completed state rejects turn-in/acceptance replay. No reward on load.

## Validation

Run `DiceFree.EditorTools.CornbergRunnerValidation.Run` in batch Play Mode without
`-quit`, with a fresh absolute `-diceFreeSaveRoot`.

The focused fixture seeds an old completed-Q3 v2 profile without a runner record,
including unknown quest and opaque section. It checks pre-Q3 gate/semantic credit,
real approach, unopened/closed/remote rejection, wrong NPC and conversation IDs,
one event per acknowledgment, repeated conversations without duplicate completion,
unchanged level 6/0 XP and four real reload checkpoints: before interaction,
closed/unacknowledged, completed and repeated. Identity and unknown data survive.

Validation on 2026-09-30 passed:

- Focused runner suite with four reload checkpoints, including rejection after
  moving away from an already-open conversation.
- Q1; Q2 with six reload checkpoints; Q3 with four reload checkpoints and actual
  duel. Q3's old-profile fixture now explicitly contains only Q1/Q2 rather than
  accidentally pre-completing subsequently added story definitions.
- Combat/death/Return/well, existing stat/Defense/crit/element/hit tests, both
  movement modes and all 13 traversal routes.
- Fresh/injured/dead/legacy persistence, migration, recovery/backups, unknown data,
  stale-writer protection and Profile Inspector checks.
- Windows development build: 171,988,846 bytes.
- Eight isolated standalone startup/reload smokes: fresh, old-Q3, completed-runner
  and legacy-v1 profiles, twice each. Identity/progression persisted, revisions
  advanced, old profiles acquired runner state, and completed state did not replay.
  Two further launches of the final rebuilt executable retained completed runner
  state at level 6/0 XP. All ten launches logged no runtime errors.

The initial combat invocation used `-nographics` and failed its screenshot capture.
Rerunning with rendering enabled passed; no assertion was weakened. Known editor
SearchDatabase #17 remains separately recognized. No manual playthrough is claimed.

The scene adds 244 lines and removes none; all 8,457 existing serialized IDs remain.
Navigation, save schema, persistence implementation, combat math and existing
enemy/quest balance data are unchanged. Only the new runner definition is added.

## Q4 decisions still required

- Which completion alternatives are authored: dungeon, elite, mass kills, others?
- Which alternatives ship in the first Q4 slice; are all three available initially?
- What exact kill count and eligible enemies, and does prior progress count?
- What reward parity applies across alternatives; can only one reward be claimed?
- What level/XP payout should Q4 target? The later approximate level-10 Q5 target
  does not settle Q4's payout.

WORLD_1's approximately 30 kills is an example, not a locked requirement. This
increment deliberately implements none of these alternatives.
