# Multiplayer world lab verification — 2026-10-15

The local multiplayer proof passed **21 of 21 checks** using separate Windows player processes over loopback UDP. The host ran both test faces concurrently. The final run lasted approximately 90 seconds and used Unity 6000.6.3f1, NGO 2.13.3 and the resolved built-in Unity Transport 6.6.0.

Evidence: [standalone results](validation/NetworkLab-Standalone-2026-10-15.json), including authoritative snapshots at join, split worlds, simultaneous combat, return, reconnect and grace-expired rejoin.

## Verified behaviours

- Host and guest connect as distinct participants and exchange authoritative state.
- Independent movement is resolved by the host and replicated to the client.
- Out-of-bounds movement and attacks against targets on another face are rejected.
- The host continues moving while the guest performs its local cube flip.
- Guest travels to World 2 while the host remains on World 1.
- The existing BasicAttack/Health pipeline resolves combat on both occupied faces.
- Guest returns with the same authoritative actor identity and two completed crossings.
- Late join receives the current damaged-world state.
- Four players coexist; a fifth connection is rejected without replacing the roster.
- A disconnected actor remains vulnerable during its reserved grace period.
- Reconnecting before expiry resumes the same actor without restoring missing HP.
- Expiry removes the abandoned actor; reconnect after expiry follows fresh-join rules.
- All five process logs have no detected runtime exceptions, navigation startup errors or unsupported-fog errors.
- Both test faces remain loaded on the host throughout the run.

The fixture uses isolated level-1 Novices with 500 HP, 2000-HP training sentinels and basic attacks. Sentinels retaliate at close range. This is not an enemy balance or production AI test.

## Additional verification

- Original solo cube-edge regression: **95 checks passed**, including state conservation, control locks, failure recovery, scene unloading and automatic seam walking.
- Greatblade focused combat regression: **75 checks passed**.
- Original development menu/round-trip regression passed.
- Ten existing data/architecture checks passed: advancement, physical, magical, novice skills, architecture, architecture policy, managed references, quadratic XP, flexible skill UI and Greatblade prototype.
- World/Codex validation passed, including added overworld-expiry checks that protect dungeon participants and leases.
- Graphical Editor host plus a separate graphical Windows guest connected successfully. Telemetry confirmed the guest completed its flip while the host remained on World 1. The inspected screenshots cover the menu and Editor host; this is not a manual visual playthrough of the guest window.
- Live return to main menu passed DICEFREE_NETWORK_LAB_MENU_CLEANUP_OK: temporary root cleared, both maps unloaded, session cleared.

[Menu](screenshots/NetworkLab-Menu.png) · [Host with guest on World 2](screenshots/NetworkLab-SplitWorlds.png)

## Windows build

Final seven-scene Windows Development build succeeded with **0 errors and 0 warnings**, reporting **353,275,652 bytes**. Executable: `T:\TEMP\DiceFree-Options\Builds\World1Foundation\DiceFree.exe`. Copy the entire World1Foundation folder to the laptop.

[Build record](validation/NetworkLab-WindowsBuild-2026-10-15.json)

## Scope and next test

This proves a bounded multiplayer transport/simulation path and independent two-face travel on one computer. Both maps currently remain resident in every process. It does not establish six-world memory requirements or streaming performance.

Two-computer LAN reachability, real latency/loss, normal-world class/skill/effect replication, progression, collision under latency and host migration remain unverified or unimplemented. A restarted client process creates a new test identity; the reconnect check covers a connection drop within the same running client.

For laptop steps see [Multiplayer world lab](MULTIPLAYER_WORLD_LAB.md).
