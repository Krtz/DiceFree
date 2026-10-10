# DiceFree: Warcraft III–Style Shift Command Queue and First Audio Pass

Implemented October 10, 2026 on `feature/visual-overhaul-vis00`.

## Command queue

- Hold **Shift** while right-clicking world ground (Classic mode) to append a movement waypoint. Shift + click on an enemy instead appends an auto-attack order, waiting until the target is defeated/cancelled before proceeding.
- Hold **Shift** while clicking the minimap to queue a world-space movement destination. Normal minimap clicks replace movement orders.
- Press the existing **attack-move** hotkey and Shift + left-click the destination to append attack-move. Attack-move still uses the pre-existing approach/combat routine and its ordinary enemy acquisition.
- Press an action-bar skill hotkey, then **Shift + left-click** a unit or ground to queue its use. The usual targeting range is checked when the queued action executes. Physical instant skills (Guard, Quickening) queue when their hotkey is pressed with Shift.
- **Ordinary commands replace** previous queued orders. **Stop/Escape** clears queued commands. Direct WASD or switching control schemes also clears them. Losing control/health, opening a modal, and suppression reset commands.
- A small "Queued orders: N" count appears above the action bar.
- Queue capped at 24 orders; actions timeout after 55 seconds if unreachable or if the cast cannot succeed (e.g. insufficient Mana / cooldown). Cooldown-limited queued spells are retried until usable or until timeout, instead of automatically discarding a valid queued intent.
- All commands execute through `TraversalMotor`, `CombatInput`, `BasicAttack` and existing skill caster delegates. No path bypass, teleports, or parallel movement systems.

## Audio implementation

Curated **16 audio clips** from locally downloaded packs, imported as audio only. No third-party scripts or demo scenes were imported:

- `RPG Essentials Sound Effects - FREE` (Leohpaz): grass/wood steps, attacks, impacts, hurt, heals, sand/ice/fire, buffs, encounter/level-up cue and UI cues.
- `Fantasy Menu SFX` (Chris M Audio): UI/queued-command selection sound.
- `SoundBits Free Sound FX Collection` (SoundBits Sound Effects): 16-second trimmed Cornberg birds/field ambience loop.
- Imported assets: `Assets/_DiceFree/Resources/Audio`. Re-encoded 24-bit PCM clips as 16-bit PCM, retaining the original 44.1kHz sampling; trimmed a long ambience clip with fades. Total 7,380,104 bytes for all 16 clips (down from about 55 MB).
- `PlayerAudioDirector` attaches to the player and owns separate audio sources for sound effects, footsteps and subtle looping ambience. It hooks skill cast events for Novice / Magically Touched / Physically Blessed, actual attack hits, damage, real heals and level-up, plus distance-driven walking sounds. Heal events are played once, avoiding double sound effects.
- Options now includes **Sound Effects** and **Ambience** sliders. These affect levels live and persist using PlayerPrefs.
- Mix intentionally restrained for starter classes; later advancement tiers should have more dramatic, bespoke layers. Test actual subjective balance with headphones/speakers. Not yet complete: music, biome-specific ambience routing, per-monster SFX, dedicated mixer buses and fade transitions.

## Catalog refresh

User's **offline** asset catalog was rebuilt at `T:\TEMP\DiceFree-AssetCatalog\catalog.html`; now **210 Unity packages / 34,391 indexed paths**, zero scanning errors. The 29 new packages include extensive audio libraries. The catalog and asset shortlist do **not** certify licensing or game redistribution permissions; verify each publisher/license before public release.

## Verified in Unity

- Recompile: **0 errors, 0 warnings**.
- `dicefree.commands.audio.catalog-check`: **PASS**, all 13 core expected AudioClips imported.
- `dicefree.commands.audio.runtime-smoke`: **PASS**, 16 AudioClips and active Cornberg ambient source; queue accepted two movement orders followed by an action.
- `dicefree.commands.audio.runtime-status`: **PASS**, player reached second destination and deferred action executed in order; queue cleared normally.
- `dicefree.commands.skills.enqueue-test` + `dicefree.commands.skills.queue-status`: **PASS**, movement followed by unit cast, ground cast, instant buff; callbacks executed **UGI**, exactly in sequence. SFX and ambience volume changes tested.
- `dicefree.commands.attack-move.enqueue-test` + `dicefree.commands.attack-move.queue-status`: **PASS**, attack-move completed before its deferred action.
- More nuanced user testing still needed for rapid Shift-click sequences, combat with many monsters, dynamically dying targets and long cooldowns.

Do not merge to `main` without user review.
