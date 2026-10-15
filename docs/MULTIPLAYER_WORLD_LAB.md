# Multiplayer world lab

A bounded direct/LAN multiplayer proof using the existing DevelopmentRoom and World2TransitionTest.
This is not network support for Cornberg or the complete game.

## Try it on two computers

1. Copy the entire Windows build folder, not just DiceFree.exe, onto both computers.
2. On the desktop: Main menu -> Multiplayer development test -> Host LAN test.
3. On the laptop: open the same menu, enter the desktop's reachable LAN IPv4 address, use the same port (default 7777), then Join test.
4. Move with WASD or left click outside the test panel. Space or right click orders an attack on the nearest training sentinel.
5. Walk to cube edge, then cross it. Leave the other player on the original face and confirm they can continue moving and fighting.
6. Use Disconnect / Reconnect within 30 seconds to resume the same actor. Disconnect leaves it vulnerable.
7. Main menu ends that process's lab session. Closing the host currently ends the connection; host migration is not implemented.

If Windows prompts for network access, allow the game on the intended private LAN. No router forwarding, cloud account, paid service or internet dependency is required for the LAN transport. Cross-computer reachability and firewall behaviour have not yet been verified.

Every launch creates a temporary, isolated development Echo. Real character progression is not imported into the lab. Test Novices have 500 HP, sentinels 2000 HP, and use the existing basic attack/damage pipeline. Skills, gear changes, XP, loot, quests, production-world AI and durable multiplayer progress are outside this proof.

## Architecture

- Unity Netcode for GameObjects 2.13.3, Unity Transport 6.6.0 (the Editor-resolved built-in version), direct UDP.
- Named NGO messages carry bounded command intents and authoritative snapshots. No second combat engine or homemade socket transport.
- Host and guest use the same command validation path. A connection controls only its bound actor.
- Host executes existing TraversalMotor, CombatActor, Health and BasicAttack components. Clients only render received state.
- Client commands cannot set HP, damage, actor identity or arbitrary map placement. Bounds, current face, target face, liveness, sequence and rate are checked.
- Four slots, including the host; disconnected slots remain reserved during the 30-second grace.
- An in-memory random reconnect credential binds a reconnect to its existing actor. It is not broadcast in state or reports. Restarting the client process creates a fresh identity; this is not account authentication.
- Both test maps remain resident in every process for this first proof. This deliberately does not settle future client streaming or six-world memory budgets.
- Per-participant face membership uses SessionMapRegistry. Expiry removes the overworld membership without changing dungeon leases.
- The cube presenter is reused locally for each participant. The host authorizes the source edge and completes the crossing. Other actors and maps continue simulating; no global pause, scene replacement or party teleport.
- The original solo development travel still unloads the old scene. Only the network lab retains both.
- Full snapshots provide each connected client a current view of the bounded lab state. They are not a complete migration checkpoint or host-election implementation.
- Guest/target proxy shapes are intentionally basic. This is a transport/simulation test, not final multiplayer art or animation.

## Reproduce local tests

Build through the live Unity command:

```powershell
unity command dicefree.development.build-windows --project-path T:\TEMP\DiceFree-Options --format json
powershell -NoProfile -ExecutionPolicy Bypass -File tools\validation\Invoke-NetworkWorldLab.ps1
```

The harness runs separate Windows processes, exchanges commands over NGO/Transport, and records JSON evidence under Library/NetworkLab-<UTC>. It checks movement, independent face travel, simultaneous combat, late joins, four-player capacity, overflow rejection, reconnect vulnerability, identity preservation and grace expiry.

The command-file automation is read only in the Editor or a Development player and must be explicitly enabled by command-line path arguments. Ordinary menu launches do not read command files.

## Remaining work

Two-computer LAN and real latency/loss tests; normal-world actor/skill/effect replication; player collision under latency; class/save ownership; quest/loot credit; disconnect during travel/death; host migration/checkpoint recovery; Steam/online lobby integration; production interest management and map streaming. These remain separate milestones.
