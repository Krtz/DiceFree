# Travel, Mounts, Roads and World Aggro

## Travel principle

DiceFree does not use dungeon fast travel. Reaching a dungeon means travelling through the world to its physical entrance.

World travel can become faster and more convenient through authored systems, while preserving the importance of geography.

## Mount collection

The mount collection is **Echo-wide**.

Mounts may be obtained from many different sources, including:
- quests;
- dungeon rewards/drops;
- achievements;
- vendors;
- secrets;
- rare content;
- other authored systems.

### Mount access gate

Owning mounts and being allowed to ride them are separate concerns.

A manifestation may use mounts when it satisfies one of:
- Tier 2 or above;
- Novice level 75 or above;
- Tier 1 level 75 or above.

The two level-75 routes are intended to be **hidden progression possibilities**, not necessarily advertised as ordinary unlock conditions.

### Mount speed

All ordinary mounts share the same baseline mount speed.

Choosing a mount is therefore primarily collection/appearance preference rather than a hidden power ranking.

General mount speed can be increased by progression systems.

Current design direction: **each collected mount may contribute a small permanent mount-speed increase**, with an illustrative value around **+2% per collected mount**. Exact value, curve and cap are balance data and remain intentionally open.

The implementation must not hard-code +2% as final tuning.

## Mounting rules

Mounting:
- has a cast time;
- is unavailable while the actor has active aggro/threat;
- cannot be used to instantly escape combat.

Taking damage while mounted dismounts the player.

Individual areas/content can disable mounts.

Default expectation:
- open world: mounts generally allowed;
- dungeons: mounts generally disabled;
- buildings/caves/special authored spaces: generally disabled unless explicitly allowed.

## Roads

Standing on an authored **road surface** grants a movement-speed bonus.

The road bonus:
- is determined by surface/location, not by checking whether the player's movement direction follows the road spline;
- stacks with mount movement speed;
- should be data-driven/tunable.

Framework should allow different road/surface types to define different bonuses later if needed.

## Trivial-enemy aggro

Lower-level enemies should stop automatically aggroing substantially higher-level players so travelling through old regions does not become constant nuisance combat.

Initial working rule:
- an ordinary enemy does not automatically aggro a player who is **10 or more levels above it**.

This value is provisional.

Build the aggro framework so threshold/scaling can later vary by:
- absolute level difference;
- proportional/scaled rules;
- enemy archetype;
- region;
- difficulty;
- explicit encounter overrides.

### Always-aggro exceptions

Authored enemies can ignore trivial-level suppression and still aggro regardless of level difference.

Examples:
- bosses;
- guards;
- territorial enemies;
- ambush predators;
- quest/event enemies;
- other explicit special cases.

## Rare overworld enemies

World regions may contain authored rare/named enemies.

Rare behavior is **case-by-case**:
- fixed spawn point;
- multiple possible spawn points;
- patrol route;
- other authored behavior.

Rare enemies are intentionally farmable and may have **long respawn timers**.

Difficulty varies by rare:
- some are intended to be soloable near the local level;
- some are deliberate "come back later / bring friends" encounters.

Possible rewards/purposes include:
- handcrafted loot;
- achievements;
- lore;
- bonus XP/gold;
- mounts/cosmetics;
- challenge only;
- other authored rewards.

There is no requirement that every rare use the same reward model or respawn behavior.
