# Cornberg Q4 implementation

Issue #45 adds visible Cornberg work gloves to the existing guaranteed reward.
Q4 eligibility, objectives, reward transaction, **200 XP / 100 gold / one glove**,
and +5% Attack Speed only remain unchanged. Quest completion does not equip the
appearance; the player must equip the owned instance in Hands. Appearance reads
stable item/equipment state, never quest state. The focused issue #45 harness
checks turn-in/reload cannot regrant, and equipped/unequipped reloads preserve
the same glove instance ID. See [CORNBERG_ITEMS.md](CORNBERG_ITEMS.md).

Implemented and validated on `poc/cornberg` on 2026-10-01.

## Content

`quest.cornberg.break-slime-surge` requires completed
`story.cornberg.runner-returns`. The different farmer is
`npc.cornberg.surge-farmer`, provisionally displayed as **Mira**, near (12, 0, -8).
Name, capsule presentation, position and dialogue are reversible blockout choices,
not permanent lore. The farmer remains present before/after the quest.

`AmbientBark` uses authored mundane lines, a 7m proximity, 18-second cooldown,
4-second display and no immediate repeat. Its random source is injectable. Barks
do not open conversations or publish quest credit. Formal quest interaction keeps
the existing range/LOS/approach checks and exposes Q4 only when eligible.

## Objectives and credit

The existing sequential quest model gains one `Any` objective group with stable
leaf IDs (`surge.elite`, `surge.population`). Each leaf retains its own count.
Either leaf satisfies the group and makes the quest ReadyToTurnIn; neither grants
rewards until the farmer turn-in. Active objectives alone consume events, so
pre-accept kills are not retroactive and later kills cannot replay completion.

The population leaf needs 30 defeats tagged
`quest-credit.cornberg.surge-slime`. Road, Named Forest and elite definitions opt
in; Crop Slime deliberately does not. Future dungeon definitions can opt in
without quest code changes. Tags are copied into semantic defeat facts.

QuestJournal consumes semantic defeat events separately from owner-only XP.
`QuestCreditPolicy` permits the owner or a same-party recipient within an inclusive
50m Euclidean distance of the defeat position. `IQuestPartyMembership` is the
provider-neutral seam; `LocalPartyMember` is a local fixture/session adapter, not
networking. Existing quests default to owner credit. Recipient quests must be active.

## Elite and world loot

`enemy.forest-elite-slime` reuses the Slime archetype in the existing northern
deep-forest clearing near (77, 0, 53). The clue deliberately gives no exact marker.
Provisional fixed tuning: level 8, 240 HP, 12 base bump damage, 1.6s interval,
0.5s windup, 0.6m reach, 3.4m/s movement, 8m awareness, 16m leash, 80 XP.
No player scaling or special boss mechanics. Existing respawn restores the same
actor at home after **450 seconds**.

`FixedWorldDrop` makes one injected random roll per defeated life, with authored
20% chance for `item.cornberg.forest-shoes`. Success creates a transient world
pickup. `WorldEquipmentPickup` permits any valid nearby manifestation with the
item definition to claim it, with no killer reservation. Only pickup creates a
GUID owned instance. Uncollected pickups are not saved. The consumed latch is set
before publishing inventory changes, preventing a second claim.

## Main reward and pacing

Provisional shared reward: **200 XP, 100 gold, one work-glove instance**
(`item.cornberg.work-gloves`, unchanged +5% Attack Speed only).
`FixedRewardDefinition`/`FixedRewardGrant` prevalidate item/catalog/wallet bounds,
coordinate completion with grant and defer persistence during the synchronous
mutation. A save captures completion, XP, gold and ownership together. Repeated
turn-in/reload cannot regrant. Item creation remains outside QuestJournal.

Measured from level 6 / 0 XP, without changing old enemy XP:

| Route | Incidental XP | Quest XP | Result |
|---|---:|---:|---|
| Elite | 80 | 200 | Level 9, 10 XP |
| 30 Road Slimes | 30 × 20 = 600 | 200 | Level 13, 30 XP |

The mass route exceeds the old level 9–10 target. This is pacing evidence for
Axel, not a reason to suppress normal XP. Shoe drops are separate from main reward.

Automated real attack-loop fixtures: solo level 6 died with elite HP 58; level 8
won with approximately 25 HP; level 9 won with approximately 52 HP. Frame timing
can slightly change remaining HP. A stationary two-level-6 packet/timing fixture
won in 9.89 seconds with about 23.68 tank HP. This is a math fixture, not a
networked cooperative playthrough; movement/latency/group behavior needs playtesting.

## Persistence

Schema and manifestation record advance **v3 → v4** for durable per-alternative
progress. Existing v1/v2 paths also upgrade. Existing records receive empty
alternative arrays; inventory, equipment, gold, identity and unknown records remain.
The genuine item-bearing `echo-v3.json` fixture proves migration. Current records
are not repeatedly migrated. Profile Inspector displays stable alternative IDs/counts.

## Validation status

Focused PlayMode validation passed for both routes, prerequisite, ambient barks,
approach/remote rejection, Crop exclusion, pre-accept kills, duplicate turn-in,
seven reload checkpoints, nearby 49/50/>50m, non-party/inactive exclusions,
no shared XP, real solo combat, actual 450-second respawn, deterministic loot
threshold and non-killer FFA pickup. It also covers partial kills followed by elite,
both completion orders, reward prevalidation/reentrancy and prevention of partial
save writes. Both routes grant the same main reward exactly once.

Passed existing item/equipment/gold plus separate-process item reload, road speed,
trivial aggro, combat/death/Return/well, Q1, Q2 (six reloads), Q3 (four), Runner
Returns (four), all 13 traversal routes/both controls, fresh/injured/dead/legacy
save suites, real v1/v2/v3 migrations, recovery/backups/stale writers and Profile
Inspector. Existing quest fixtures now select the Q1 farmer explicitly instead
of whichever QuestGiver Unity enumerates first. No gameplay assertion was weakened.

Q2 caught the first elite placement overlapping the Road Slime clearing. The new
elite alone moved to the separate deep clearing; Q2 and traversal passed afterward.
The OR representation was made shallow to avoid Unity's recursive serialization
depth warning; final compilation and validation are free of that warning.

Windows development build succeeded: **172,036,002 bytes**. Two isolated 12-second
standalone startup/reload smokes passed at schema 4, revisions 13→14, Q4 Completed,
100 gold, one glove and one picked-up shoe, stable GUIDs/XP and no runtime errors.
Smoke state includes extra fixture elite farming; route pacing is reported above.

The scene change is additive farmer/elite authoring and the Q4 journal reference.
No geography, navigation bake, existing enemy XP/stats or movement tuning changed.
The next step is Axel's Q4 pacing/playtest review, especially the level-13 mass
route. MSQ5 is a separate future authorized increment, not part of this patch.

No manual human playthrough is claimed. Known editor SearchDatabase #17 is
identified separately. MSQ5, dungeon, networking and the mature item/effect
framework are not implemented.
