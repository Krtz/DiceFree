# Cornberg handcrafted item foundation

## Visible work gloves - issue #45

`item.cornberg.work-gloves` now has a paired worn-leather/canvas glove appearance
on the actual Cornberg Novice. A small `EquipmentPresentation` UI component
maps the authored Hands slot and `ItemDefinition` stable ID to the glove visual.
It resolves the equipped owned instance through the existing inventory/equipment
model and reacts to `Equipment.Changed`. It does not infer appearance from Q4,
names, or item array positions. Future appearances can use another authored
binding; no mature transmog/form-compatibility system is introduced.

The meshes share the actual Novice hand/forearm bones and Animator. They overlay
the baseline hands, so removing gloves reveals the same baseline mesh/material/
enabled state without destructive body segmentation. Presentation adds no
collider, actor, gameplay movement, reward or stat logic. Gameplay authority
remains on the existing root.

The item definition and formula are unchanged: authored `attackSpeedPercent: 5`
uses percentage points and produces exactly a **1.05 multiplier**, with zero
Defense or movement bonuses. Q4 still awards one owned instance plus its existing
200 XP/100 gold. Current save schema and instance/equipment records are unchanged.
See [source and reproduction notes](../SourceArt/Characters/CornbergWorkGloves/README.md)
for the necessary geometry-preserving Novice skin-weight correction and preview.

This prerequisite for Q4 adds items, carried gold and equipment, not Q4 or an
elite/drop system. Open Inventory with **B** or its button (I remains interaction). Development builds
and the editor expose explicit grant buttons; each click creates another copy.
Fresh profiles remain empty and no automatic startup rewards exist.

## Data and ownership

`ItemDefinition` is an authored ScriptableObject: stable content ID, name, slot,
item-level/rarity/source metadata, optional flavor and fixed `EquipmentStats`.
All eleven settled slots exist. No armor weights, generation, affixes, budgets,
sockets, sets, procs, bank, trading or broad class-eligibility engine are added.
Current items have no class restrictions; equipment checks ownership, resolved
definition and exact slot. Item level is metadata, not an invented level gate.

`CarriedInventory` owns GUID instances separate from definition IDs. Duplicate
definitions are allowed. Query/snapshot records are copies. `Equipment` retains
references to those owned instances; equipping never consumes/recreates them.
Removing an owned instance also removes its equipment reference. Overworld swaps
are allowed in combat. Dungeon locks remain future work.

`FixedItemGrant` is the small content-facing fixed-item grant boundary. The
caller owns reward eligibility/completion; loading restores records and never
invokes grants. Future quest reward orchestration can call this boundary with
XP/gold changes before the existing end-of-frame durable snapshot. No item code
was added to QuestJournal and no general transaction framework was invented.

## First definitions

| Stable ID | Provisional name | Slot | Exact gameplay package |
| --- | --- | --- | --- |
| `item.cornberg.work-gloves` | Cornberg Work Gloves | Hands | +5% Attack Speed only |
| `item.cornberg.forest-shoes` | Forest Travel Shoes | Feet | +1 Physical Defense, +1 Magical Defense, +1% Movement Speed |

Assets live in `Settings/Items`. Both currently use placeholder item level 1 and
`rarity.provisional`. Names, rarity, item level and art are not final lore/balance.
Source metadata records intended future content, not an implemented drop table.
The later shoe drop is 20% per elite kill; no roll/drop is implemented here.

## Runtime stats and units

Equipped sources use `equipment:<instance UUID>` in ActorStats. Recalculation
replaces/removes exact sources and aggregates in stable identity order. These
contributions are separate from transient combat modifiers and survive their reset.

Flat gear Defense joins class base and attribute-derived **underlying Defense**
before ordinary DefenseMath buffs/reductions/penetration. Other future passive or
permanent source membership remains open.

Equipment speed percentages sum within this small equipment package and multiply
the existing attribute-derived speed: gloves give baseline AttackSpeed × 1.05;
shoes give baseline physical MoveSpeed × 1.01. This is a final speed contribution,
not a coefficient/AGI change or BasicAttack special case. No general buff/mount
stacking formula is settled by this implementation.

CombatActor propagates stat changes to the existing TraversalMotor baseline.
Road retains its independent ×1.15 provisional factor; shoes on road therefore
give baseline ×1.01×1.15. Unequipping never removes the road contribution.

One world unit is one meter. Motors remain in m/s. `MovementUnits.DisplaySpeed`
multiplies by ten for UI: 5 m/s = 50 Move Speed; +1% = 5.05 m/s = 50.5.
The inventory panel shows current effective speed including road. Existing AGI
can make the actual Novice value slightly above the nominal 50.

## Wallet and persistence

`GoldWallet` owns non-negative integer (`long`) carried gold, checked grant,
CanSpend/Spend, restore and Changed events. Overflow rejects before mutation.
No death-loss amount, vendor or bank behavior is invented.

Echo schema and manifestation records advance to **v3**. Supported v1/v2 records
migrate to empty inventory/equipment and zero gold while preserving identities,
progression, quests and existing resource records. The genuine prior-build
`echo-v2.json` fixture complements `echo-v1.json`. Migration is detached/idempotent;
original files survive failure and successful replacement keeps backups.

ManifestationPersistence snapshots item instances, slot references and gold with
the existing level/quest state. Changed events use existing autosave debounce.
LocalEchoStore still owns file I/O, checksums, atomic replacement, recovery and
stale-writer protection. Inventory/equipment/wallet own no file I/O.

Unresolved definitions remain owned; unresolved/mismatched/unknown-slot references
remain inert and round-trip until their data resolves. Invalid duplicate IDs,
dangling references or negative gold stop loading/autosave instead of deleting
ownership. Unknown whole sections/unsupported record versions and unresolved
quests/resources retain the existing preservation contract. Arbitrary new fields
inside a recognized record require an explicit record-version change.

Profile Inspector now reports gold, instance/definition IDs, slot references and
unresolved item/equipment diagnostics. The inventory overlay reads stat labels
from definition data and distinguishes copies and slots.

## Validation

`ItemValidation.Run` requires an isolated `-diceFreeSaveRoot`; rerun that root with
`-diceFreeVerifyItems` to prove process reload. It checks duplicate instances,
invalid/wrong-slot rejection, repeated swaps, stat deltas, road composition,
combat swaps, death/Return/load reset, metric units, wallet changes, unresolved
records, diagnostics and no grants on reload. Existing v1 migration/recovery and
Q3/Runner v2 fixture coverage remain active. Final run results are recorded below.

No human/manual playthrough is claimed. Exact known SearchDatabase #17 is separate.

## Next boundary

The subsequent [Q4 slice](CORNBERG_Q4.md) connects semantic OR objectives,
a named farmer and an elite to this grant/persistence boundary. It adds a fixed
reward transaction and 20% free-for-all world shoe pickup. Its validation status
is recorded separately. Mounts and the mature #24 equipment framework remain deferred.

## Verified results — 2026-10-01

Passed focused grant/equip/death/Return/stat/road tests and a separate-process
item reload. Passed road/surface and trivial-aggro suites, combat math and
combat/death/Return/well, all 13 traversal routes/both controls, Q1, Q2 (six reload
checkpoints), Q3 (four), Runner Returns (four), and fresh/injured/dead/legacy save
runs including migration/recovery/backups/stale-writer/Profile Inspector checks.
Q3/Runner fixture writers retain explicit v2 records and exercise migration.

Q1 caught an inventory/interaction key collision during development. Inventory
now uses provisional B, leaving I interaction unchanged; Q1 passed after the fix.
No existing gameplay assertion was weakened. Version assertions now reference
the current schema/record constants.

Windows development build succeeded: **172,013,950 bytes**. Scene authoring is
four additive player components, 55 lines added/none removed. No geography,
navigation bake, existing enemy/quest tuning or physical-speed retuning occurred.
The save format intentionally advanced to v3 for this durable-state change.

Two isolated 12-second standalone startup/reload smokes passed: schema 3,
revisions 3 -> 4, the same four owned instance IDs, three equipped references
(including inert unknown data), 37 test gold and zero runtime errors.
These are automated smoke checks, not a manual playthrough.
