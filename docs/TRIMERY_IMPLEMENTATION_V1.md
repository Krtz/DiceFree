# Trimery V1 — first playable town implementation, 2026-10-13

**Branch:** `feature/visual-overhaul-vis00`. Unity 6000.6.3f1. Initial town authoring is in **the Cornberg World 1 scene**, not an inaccessible separate map. Does **not** imply World 1 Biome 3, Big City or Roadwarden encounter is complete.

## Confirmed geography and Roadwarden story
- Lumber-producing village Trimery, protected by a physically solid timber palisade and guarded entrance.
- Three outward roads: **SOUTH** to Ironpine/Cornberg, **WEST** destroyed highway toward the Big City, **NORTH** through future Biome 3 hostile humanoid quilboar territory, then toward the Big City.
- The Big City will be **ringed around an enormous central tree**; its full actual settlement is NOT authored by this patch.
- The **Ogre Roadwarden deliberately destroyed the western highway** in obedience to a mysterious order. The core mystery is **WHO ordered it** and why. The user confirmed he **remains at the ruined highway but will be accessible from the Big City side only**, not from Trimery. **No boss spawn, encounter, dialogue or actual city-side access was authored yet.** The story encounter and repairs are for later.
- No magic stone reacting to the player or Echo.

## Authored v1 town
- Town center around **World X248/Z180**, beyond the Ironpine approach at X223/Z149. The new navigable terrain occupies roughly X194..302 / Z131..249.
- **SOUTH gate** near X248/Z153, **WEST timber-cart gate** near X220/Z180, **NORTH gate** near X248/Z207.
- Real **timber palisade** with three gaps, six lookouts, six existing guard 3D actor models (two per gate).
- Seven buildings: Timber merchant hall, Trimery bank, Forester's inn, Forester's cabin, Millworkers' bunkhouse, Lumber guild office and Carpenter workshop.
- Lumberworks: covered sawmill, cutting table, log stacks and transport cargo. Market stands, crates, and two existing 3D caravan carts.
- **3D mesh-based portable fallback art** always present. Optional local licensed assets from the Asset Store used for two market stalls, crate and timber pillar/house framing. The code accesses them through `TrimeryArtSlot` resource IDs, not external asset GUIDs. Materials, textures and raw vendor prefabs are excluded from public GitHub through `.gitignore`.
- **Trimery D6 revive beacon** with stable ID `anchor.trimery`. The actual return/respawn destination was moved beside the physical D6 onto confirmed NavMesh at **X250/Z181.8**, rather than trapping the player inside the stone.
- **Merchant and banker** cloned from existing Cornberg service NPC systems. No duplicate game logic.
- The existing Cornberg Runner quest's `area.world1.next-settlement-arrival` ReachArea trigger is authored at the south entrance near X248/Z161, so entering Trimery can credit the Runner objective.
- The western road has solid **Roadwarden-smash rubble and wrecked logs** as a visible first-pass blocked road. More complete highway / unrepaired-to-repaired world-state art and true city-side encounter remain future work. The broad open terrain means we should recheck bypass possibilities before shipping a finished obstruction.

## Infrastructure
- Reused same cornberg.unity persistent world, rather than switching scenes. Added solid mesh ground (`Trimery continuous playable terrain.asset`) and three path mesh assets; this connects with Ironpine.
- Increased `CornbergSceneBuilder.Rebake()` NavMesh bounds **only if Trimery root exists**, preserving old-only Cornberg bake behavior. Rebaked existing `Settings/CornbergNavMesh.asset`.
- Dedicated idempotent Editor CLI commands:
  `dicefree.trimery.install` (first install),
  `dicefree.trimery.polish`,
  `dicefree.trimery.licensed.install`,
  `dicefree.trimery.fix-anchor`,
  `dicefree.trimery.rebake`,
  `dicefree.trimery.site.inspect`,
  `dicefree.trimery.validate`.
  Do not rerun initial installer after town exists.
- Original single-service validators adjusted to explicitly target Cornberg's existing merchant/banker, and merchant coordinates aligned to the previous user-requested move to the storefront door. Both old tests pass with the new Trimery NPCs also present.

## Validation performed
- Main Unity project `unity recompile`: 0 errors, 0 warnings.
- `dicefree.trimery.validate` returns `TRIMERY_VILLAGE_VALIDATED` with 3 gates, 6 guards, 7 buildings, 20 portable art slots, verified stone and Runner arrival, 12 solid road blockers, **six complete NavMesh paths** covering Ironpine approach, south gate, village services/stone, merchant, bank, north exit and west gate.
- Existing `dicefree.ironpine.validate`, `dicefree.cornberg-perimeter.validate`, `dicefree.economy.validate` and `dicefree.bank-balancing.validate` passed.
- Separate isolated Unity headless Play Mode marker **`TRIMERY_PLAYMODE_OK`** in `T:/TEMP/DiceFree-IronpineValidation/TrimeryPlaytestOct13d.log`. Tested player teleport onto Trimery NavMesh, automatically activating its revive stone (real proximity polling; test forces poll interval to expire), retained `anchor.trimery`, authored Runner arrival report triggered, merchant/trader and banker components available, and portable art slots configured. Uses isolated game-save/settings folders; **user's real saves untouched**. Known unrelated Unity Editor SearchDatabase indexing exception can appear in headless log without preventing these assertions.
- **Still needs real Game View visual QA and walking through palisade:** check model scale, colliders, user HUD at town, market textures, North/WEST route sightlines, and behavior on return/reload. Do not claim all decorations render beautifully or all services dialogue is fully tailored.

## Next steps (not implemented)
1. Player visual review of village, architecture scale and amount of lumber industry. Give characters proper names, lumber guild quests, trader-specific inventories, lumber harvesting/felling animations.
2. Test that Alpha Wolf cannot wander into Trimery. Guard towers/village safety is first-pass architecture, **not hostile faction combat behavior**.
3. Flesh out initial story from Runner arrival, timber trade disruption and northern detour into Biome 3; avoid making up canonical dialogue prematurely.
4. Author Biome 3 quilboar region and player progression, with a follow-on trail from current north road.
5. Big City ringed around central Great Tree, Roadwarden stationed near damaged highway **on city-access side only**; later Big City quest kills/negotiates Roadwarden, repairs road and restores trade. This is deferred by user.

## Repository safety
Commit only authored source, scene, navmesh data and portable fallback mesh assets. All user-installed raw Asset Store files under `Assets/Medieval Village Building Pack`, `Assets/Low-Poly Medieval Market`, `Assets/Containers pack`, `Assets/the_wood_cabin` and `Assets/_DiceFree/Resources/TrimeryLicensed` remain locally installed and ignored. Do not include unrelated EarlySkillVfx, Ironpine Quillrat materials, Audio/IronpineLicensed meta, ShaderGraph or PackageManager settings. Do not merge to `main` without approval.
