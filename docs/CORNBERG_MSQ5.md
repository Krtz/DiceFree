# Cornberg MSQ5 — Runner departure bridge

Implementation branch: `poc/cornberg`.

## Content boundary

`quest.cornberg.become-runner` (definition version 1), **Become Cornberg's Runner**,
requires completed `quest.cornberg.break-slime-surge`. There is no level requirement.
The existing weekly Runner remains `npc.cornberg.runner`, at the existing village
green position. No additional NPC, settlement, geography or navigation is added.

Provisional offer: “You've helped with the Slimes, but I keep thinking about that
road. I'm not ready to try again yet. Would you take Cornberg's message to the next
settlement for me? It's our usual village news. When there's important news to
bring back, we count on our runner for that too.”

This is ordinary self-preservation after the failed weekly run, not comic cowardice
or a larger threat. After acceptance the Runner thanks the Echo and wishes them
well. No next-town name, lore, exact distance or arrival is invented.

The active objective is **Travel to the next settlement and deliver Cornberg's
message.** Its future-facing semantic destination is
`area.world1.next-settlement-arrival`. No scene ReachArea has this ID. Consequently
Q5 deliberately remains Active at the current content boundary; no Cornberg road
endpoint completes it. Future destination/delivery authoring must decide how to
continue this stable quest. No reward is authored or granted, and there is no
physical message item: active quest state represents carrying the message.

## One NPC, contextual actions

`ContextualNpc` is the only discoverable world InteractionTarget on the Runner.
It selects the first available action from an explicit authored default order:
Q5 QuestGiver, then the preserved Runner Returns ConversationTarget. Action
components live on child objects without world colliders. Their `WorldTarget`
points to the router, so keyboard discovery skips them and direct orders route
through the same default choice. Click raycasts encounter the single root target.

Availability is distinct from range: the router can select an action before the
actor approaches, while existing range/LOS/alive checks still gate opening and
acceptance. Selection is refreshed on arrival and rechecked on interaction. A
previously opened action cannot be acknowledged after another action takes over.
Interactor.Active identifies the chosen action, allowing existing conversation
and quest panels to remain separate. InteractionPrompt resolves the same action
for `!`/`?` presentation.

Before Q3, the existing story gate still hides Runner presentation. After Q3 and
until Q4 completion, the original conversation/data and acknowledged TalkTo event
remain available. After Q4 completion, the Q5 quest action becomes the default;
after acceptance it remains the active quest conversation without `!`.

This is an ordered default-action seam, not a complete service menu. Future
vendor/crafting actions can implement availability through InteractionTarget;
multi-choice menus remain future work. No type/name-specific Runner branching
exists in Interactor or QuestJournal.

## Persistence and advancement boundary

Schema/manifestation **v4 stays unchanged**. The existing sequential ReachArea
record stores Q5's ID/version/status/stage/count. Old profiles gain its Available
record with Q4 gating; unknown records remain preserved. Active state survives
reload without reward replay. Q4 alternative counters, rewards and pacing remain
unchanged: elite route level 9/10 XP; 30 Road Slimes level 13/30 XP.

No level-10 mountain/blessing or advancement dependency is added. Q5 has no
class-name condition; future manifestation/advancement work must preserve normal
timeline quest state according to its authored transition policy. That framework
is not implemented by this bridge.

## Validation

Focused test entry point: `DiceFree.EditorTools.CornbergDepartureValidation.Run`,
batch Play Mode without `-quit`, with a fresh absolute `-diceFreeSaveRoot`.
It starts from a schema-v4 pre-Q5 profile with Q4 ready for turn-in, uses the real
Q4 reward to reach level 9/10 XP, and tests context selection, original story,
range, click approach, keyboard interaction, acceptance at levels 9 and 13,
absence of a destination trigger, every existing ReachArea, the current road end,
no rewards, unknown data and five scene reload checkpoints.

Validation passed: the focused Runner/Q5 Play Mode suite; Runner Returns; Q4
including its unchanged level-9/10-XP elite and level-13/30-XP mass-route pacing;
Q1, Q2, Q3; item/equipment/gold; schema-v1/v2/v3/v4 migration, reload,
recovery/backups/stale-writer; road speed; trivial aggro; combat/death/Return/well;
and all 13 traversal routes in both control modes. The focused suite covers the
Runner Returns story before Q4, Q4-only availability, level 9 and 13 acceptance,
click-to-approach and keyboard interaction, remote rejection, the non-existent
future destination, and five reload checkpoints. Legacy/item/dead/living reload
checks passed separately. Profile Inspector validation ran as part of the save
suite.

The Windows development build passed (`DiceFree.exe`, 172,039,389 bytes). Two
isolated standalone startup/reload runs preserved the same Echo and exact
manifestation state (Q4 completed with its one glove and 100 gold; Q5 Active),
while the save revision advanced from 9 to 10 to 11. No new runtime, save or
navigation errors appeared. Unity's existing editor SearchDatabase
`ArgumentOutOfRangeException` (#17) and unavailable editor licensing-token log
were observed; the SearchDatabase trace matches the known editor-only issue.
No human playthrough is claimed.

This increment changes no save schema (still v4), Q4 quest/reward/pacing or
authored enemy balance. MSQ5 completion/reward, the next settlement, mountain
blessing, advancement, dungeon, mounts and networking remain unimplemented.
