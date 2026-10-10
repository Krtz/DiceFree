# DiceFree — Slime Slam, Magic Sand, Self-targeting and Loot Chest Pass

Branch: `feature/visual-overhaul-vis00`. Do not merge into main without explicit approval.

## Gameplay

- Slime Slam **interval raised to 10 seconds** (previously 4.5); serialized tuning asset and new-instance default updated. Warning ring still lasts 3 seconds, preserving the mechanic.
- Slam anticipation starts as a grounded squash, followed by a visibly **horizontal arcing leap** to the snapshotted red ring. The attack root stays on its valid NavMesh until the exact impact frame, then teleports to the ring, resolves damage, and squashes on landing. Boss `SlimeVisualMotion` remains subordinate while `DungeonSlime.Busy`. Roll, Bounce, Divide, boss melee and other gameplay systems are unchanged.
- Both **Novice Magic Sand and Magically Touched Magic Sand gain +10 raw flat damage per skill rank**, applied before target defense in the same damage calculation as the spell's base scaling. No multiplier tripling; existing cast costs, miss effects and cooldowns unchanged.
- While a friendly-target heal/buff is awaiting a target, the **bottom-left 3D character portrait is a self-target confirmation button** with tooltip and "CLICK FOR SELF" hint. It calls the existing skill's normal target-confirm callback with the player actor; host targeting cancels on successful cast. Hostile / ground-target spells cannot self-cast by portrait. Applies to Novice heal/attack-speed buff and Magically Touched Mend/Fire Imbuement. Guard/Quickening already self-cast directly.

## Physical items and rewards

- **Animated Cartoon Treasure Chest**, from the licensed Unity Asset Store pack already available on the author's machine, used as the chest mesh in `Resources/WorldLoot`. Only FBX, textures and two materials imported; no third-party scripts, sample scenes or demo loot logic. Converted external materials to URP Lit and rendered the actual asset in Unity. The imported model contains 15 renderers. The linked screenshot `Assets/_DiceFree/Art/Validation/Previews/WorldTreasureChest_Unity.png` shows the original fallback model, not the licensed commercial model.
- Slime Dungeon's reward room now creates **one visible chest per rolled offered item**, plus a chest for the EXP/gold alternate. Claims happen via existing in-world right-click/Interact workflow, not via reward-menu buttons. Each choice belongs to a specific player; claiming one deletes the other choices and immediately returns the player to Cornberg. The room displays a brief instruction panel.
- Transient Cornberg `FixedWorldDrop` objects use the **same actual treasure chest model**, replacing the old primitive Cube; `WorldEquipmentPickup` keeps its existing item grant logic.
- Each chest uses one root box collider on world interaction layer 11; decorative model children are on non-interactable layer 2 and have colliders/scripts disabled. The `Interactor` registry explicitly receives dynamically spawned reward chests, so click-to-approach works even though they spawn after the scene is loaded.

## Verification / limitations

- Unity compiled with 0 errors and 0 warnings.
- `dicefree.novice-skills.playmode-test` passed.
- `dicefree.magical.playmode-test` passed.
- `dicefree.targeting.self-test` passed with explicit self-Mend mana/cooldown assertions and normal cancellation/hostile-selection checks.
- `dicefree.slime.validate` passed with new 10-second interval.
- **Normal Slime Dungeon playtest passed end-to-end**, including real reward chest approach / interaction / item grant / return. The secret Regent route also PASSED end-to-end, claiming the physical EXP/gold chest and returning to Cornberg.
- `dicefree.loot.chest.prepare` and `dicefree.loot.chest.capture` passed; actual chest render visually inspected.
- The old general `dicefree.targeting.playmode-test` currently fails at its Cornberg ground raycast test fixture; this is separate from the new isolated self-targeting test, and remains unresolved. Preserve that test as-is rather than watering down unrelated validation.
- The chest appears as a static red-and-gold 3D item on the ground for now. Animated opening, rarity-specific glow and longer reward-room cinematic are future polish. Current loot chests are **not** network-synchronized multiplayer drops; existing project networking guarantees remain unchanged.

## Public repository licensing
The public GitHub repository must not redistribute raw paid Asset Store meshes or textures. The licensed Animated Cartoon Treasure Chest is installed locally on the developer PC and is explicitly gitignored. Runtime uses it when installed; otherwise an original source-code-defined DiceFree chest with curved wooden lid, brass bindings and a lock is used automatically. No proprietary raw assets or imported demo scripts are committed.
