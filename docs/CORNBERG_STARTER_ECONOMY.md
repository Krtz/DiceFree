# Cornberg starter economy (2026-10-08)

Local branch: `feature/54-world-dungeon-codex`. No commit, push or merge.

`item.bronze-dagger` / **BRONZE DAGGER** is handcrafted MainHand equipment. Its only stat is **+1 flat Basic Attack Damage**: both raw autoattack endpoints gain exactly 1 after attribute and percentage scaling, before critical/defense/resistance resolution. Spells do not receive it. The existing Slime Orb maximum-only term is unchanged. HUD damage and item tooltip include the dagger contribution.

The generic imported `NPCs/NPC_Merchant - 3D` at `(17, -0.22, -18)` now has a reusable `ItemShop` interaction and a click collider. Right-click or the existing nearby interaction binding opens the shop through `Interactor`; the existing Close interaction binding or Close button closes it. The panel displays item, current gold, price and provisional status. Buying uses `GoldWallet.Spend` then `CarriedInventory.Grant`; unresolved/invalid/out-of-range and insufficient-funds requests grant nothing. A synchronous transaction guard rejects reentrant purchases; each intentional funded re-buy creates a distinct owned instance. Opening never grants gear. The offer's serialized **15 gold** default is **PROVISIONAL**, subject to balance. There is no shop run-state restriction. Only Cornberg authors the shop.

The dagger is actual Unity 3D production content, authored deterministically by the installer: a closed faceted broad bronze blade, short bronze guard/pommel and dark grip, with URP materials. Its prefab binds to the Novice's existing `RightHandWeapon` socket using `EquipmentPresentation`. Existing gear bindings are retained; any existing baseline socket visual is hidden only while the dagger is equipped. No imported FBX, starter sword asset or other gear asset is overwritten. No image generation was used.

The item definition is added once to the Cornberg hero's carried-inventory catalog. SlimeDungeon, DungeonRewardRoom and NoviceMountainTrial carry that same hero/session additively; the installer also updates any authored inventories/heroes found in those maps, without creating replacements. Their current authored maps contain no separate inventory actors. Reload restores owned IDs through existing persistence; authoring/opening scenes never grants an item.

## Editor commands

All commands target the existing connected Editor:

```powershell
unity command set_autotick --enable true --project-path T:/TEMP/DiceFree-Options
unity command recompile --project-path T:/TEMP/DiceFree-Options
unity command recompile_status --project-path T:/TEMP/DiceFree-Options
unity command dicefree.economy.install --project-path T:/TEMP/DiceFree-Options
unity command dicefree.economy.validate --project-path T:/TEMP/DiceFree-Options
unity command dicefree.architecture.validate --project-path T:/TEMP/DiceFree-Options
unity command dicefree.managed-references.validate --project-path T:/TEMP/DiceFree-Options
unity command dicefree.economy.playtest --project-path T:/TEMP/DiceFree-Options
unity command dicefree.economy.status --project-path T:/TEMP/DiceFree-Options
unity command dicefree.slime.validate --project-path T:/TEMP/DiceFree-Options
unity command dicefree.slime.playtest --project-path T:/TEMP/DiceFree-Options
unity command dicefree.slime.status --project-path T:/TEMP/DiceFree-Options
unity command dicefree.slime.playtest --regent true --project-path T:/TEMP/DiceFree-Options
unity command dicefree.slime.playtest --wipe true --project-path T:/TEMP/DiceFree-Options
```

Installer requires stopped Edit Mode and clean/saved scenes. It is incremental and idempotent: it retains imported merchant art, existing item bindings and serialized offer balance; adds no duplicate shop, catalog entry or dagger binding. It does not call either Cornberg scene builder or art-batch installer, rebuild scenery, mountains or navigation, or alter named NPCs. Runtime shopping lives in the Application assembly, preserving the existing assembly dependency policy.

## Verified results

- `recompile_status`: `completed`, no reported compilation errors.
- Install: `CORNBERG_ECONOMY_INSTALLED`, `catalogs: 1`, `heroes: 1`.
- Validate: `CORNBERG_ECONOMY_VALIDATED`, `price: 15`, `merchant: (17.00, -0.22, -18.00)`, `renderers: 4`.
- Architecture: `DICEFRE_ARCHITECTURE_OK`.
- Managed references: `DICEFREE_MANAGED_REFERENCE_OK`.
- Economy isolated-save Play Mode: `CORNBERG_ECONOMY_PLAYTEST_OK`, `checks: 31`, `error: ""`.
- Economy test covers fresh empty ownership, merchant NavMesh approach, right-click ray / Interactor activation, no opening grant, insufficient funds, reentrant purchase rejection, payment, repeat purchase, +1 minimum/maximum at +50% basic-attack scaling, unchanged spell raw damage, equip/unequip visibility, damage restoration, equipped/owned persistence through scene reload, and catalog/ownership retention across both additive dungeon maps. It writes an actual Unity camera render to `Logs/CornbergEconomyEquipped.png`; the image was visually inspected and shows the bronze dagger in the Novice's hand beside the merchant.
- Dungeon data: `SLIME_DUNGEON_DATA_OK`, `rolls: 20000`, `rare: 1025`, `two: 4752`.
- Normal dungeon lifecycle: `status: passed`, `success: true`, `phase: 14`, `error: ""`.

All Play Mode checks use newly generated temporary save roots through `PersistenceTestGuard`. They do not edit user saves. These are scripted integration checks, not a manual balancing playthrough, network verification or approved final economy balance.

## Remaining equipment art

**Slime Orb, Slime Shield, Slimy Farmer's Gloves and Slimy Tophat still have ItemDefinition assets only, with no dedicated authored/bound equipment presentations.** This economy slice does not claim those drop visuals are complete. Their reward definitions, stats and dungeon mechanics are unchanged.

## Changed paths

- `Assets/_DiceFree/Scenes/Cornberg.unity`
- `Assets/_DiceFree/Settings/Items/Bronze Dagger.asset` and `.meta`
- `Assets/_DiceFree/Art/Weapons/BronzeDagger.meta`
- `Assets/_DiceFree/Art/Weapons/BronzeDagger/Blade.asset` and `.meta`
- `Assets/_DiceFree/Art/Weapons/BronzeDagger/BronzeDagger.prefab` and `.meta`
- `Assets/_DiceFree/Art/Weapons/BronzeDagger/Materials.meta`
- `Assets/_DiceFree/Art/Weapons/BronzeDagger/Materials/Warm bronze.mat` and `.meta`
- `Assets/_DiceFree/Art/Weapons/BronzeDagger/Materials/Dark leather grip.mat` and `.meta`
- `Assets/_DiceFree/Scripts/Combat/EquipmentStats.cs`
- `Assets/_DiceFree/Scripts/Combat/ActorStats.cs`
- `Assets/_DiceFree/Scripts/Combat/DamageResolver.cs`
- `Assets/_DiceFree/Scripts/UI/CharacterStatsHud.cs`
- `Assets/_DiceFree/Scripts/UI/HudTooltip.cs`
- `Assets/_DiceFree/Scripts/Application/ItemShop.cs` and `.meta`
- `Assets/_DiceFree/Scripts/Application/ShopPanel.cs` and `.meta`
- `Assets/_DiceFree/Scripts/Tools/Editor/CornbergEconomyAuthoring.cs` and `.meta`
- `Assets/_DiceFree/Scripts/Tools/Editor/CornbergEconomyValidation.cs` and `.meta`
- `docs/CORNBERG_STARTER_ECONOMY.md`

Ignored local validation capture: `Logs/CornbergEconomyEquipped.png`.
