# Arborium arrival and balance — November 5

This begins the approved capital on `feature/arborium-arrival-and-balance`. Arborium is a first playable arrival section, with street and district foundations for later work. The previous design rounds remain authoritative for the name, World Tree, district ordering, magical-order governance and approach directions.

## Balance

- Cornberg's magical well heals **25 HP/sec**, five times its previous 5. Radius and ordinary regeneration retain their existing rules.
- Heavy Strike adds **10 raw damage per rank**, from 10 at rank 1 to 60 at rank 6, before defense/resistance. Its coefficient, Mana cost, cooldown and stun rules are unchanged. Authoring and validation use the same values.
- The optional **Chieftain's Bane** turn-in awards **10,000 XP**. Its gold, durable title, stable quest ID and one-time completion remain unchanged. Existing completed records do not receive a retroactive reward.

## First city section

Arborium surrounds the existing World Tree at approximately X98/Z305; the landmark's position and original silhouette are retained. The capital has a 78 m outer radius, a monumental northern gate, copper-roofed towers, limestone walls, a fortified Adventurers' Guild, Northern Exchange, market stalls, three concentric streets, radial avenues, practical enchanted lamps and sacred garden planting. Initial artisan, mage and noble buildings establish the approved ordering; they are architectural foundations rather than finished service districts.

The approach extends from Briarback's western trail: **north from Trimery, west through Briarback, then south through Arborium's northern gate**. Three former future-region boundary rocks are disabled only at the new trail opening. Other boundaries remain, and the destroyed direct western highway is preserved. The Chieftain and ritual dungeon are not city-entry prerequisites. The shared navigation asset is rebaked in place; recovery copies of the scene and previous navigation remain local.

The **Trimery Road Captain** by Trimery's north gate offers **Road to Arborium**. Enter the northern gate, then acknowledge the **Guild Registrar's** arrival conversation outside the guildhall. This completes the report and grants a provisional **1,500 XP** once. The existing trader/bank systems serve the new market trader and exchange banker. A copied, independently identified Dice stone activates **anchor.arborium**. The minimap names the city while the player is inside it. City buildings use the existing camera-occlusion system to keep the player visible.

All new city meshes, textures and materials are original procedural work from `tools/authoring/ArboriumArrivalAuthoring.cs`; no third-party packages or art were admitted. Reuse of existing NPC and Dice presentation follows the already-installed project assets.

## Validation and limits

`tools/validation/ArboriumArrivalReview.cs` uses a disposable profile, disables hostile AI/input and grants a temporary large health pool. It measures actual healing, checks Heavy Strike's raw packet addition at every rank, exercises an exact one-time elder turn-in with a scripted boss defeat, walks the approach and ring streets, acknowledges the Guild conversation and checks service interaction and stone activation. These fixtures do not establish city encounter balance or ordinary progression difficulty. Gameplay and presentation captures are under `VisualBaselines/Arborium`; the wider view is composition evidence, not a gameplay-camera substitute.

The Play Mode review passed 15 walk segments over **678.9 m**, measured well healing at **25.02 HP/sec**, and verified the exact 10,000 XP turn-in once. A separate final presentation review verified that the guild tower fades when blocking the default gameplay camera, that the confirmation button remains above the statistics HUD, and that the city stone carries its own name. Ordinary trees retain their 8 m occlusion range; tall city architecture uses 40 m and the World Tree 64 m.

Architecture, dependency policy, managed references and the existing Physical kit validation passed. The final seven-scene Windows Development build is `Builds/BriarbackMilestone-20261105-160518/DiceFree.exe` (452,802,291 bytes), with **zero errors and zero warnings**. Its expanded smoke passed the built city report, services, stone activation, city quest/anchor persistence after resume, balance values, original class/combat/quest/loading flows, eight fort walks covering 214.8 m and normal menu Quit with exit code 0. The runtime error-pattern scan found zero matches. Final captures are under `VisualBaselines/Arborium/Windows`; evidence is in `validation/arborium-arrival-balance-20261105.json`.

The earlier `155809` diagnostic build is preserved. Its floor mesh lacked a collision pre-baking flag, and its final fort test waypoint overlapped the frozen Chieftain. The final build enables pre-baking; the test now walks to adjacent open floor. No movement-rule change was made. All 54 original modified tracked files and seven protected build settings remained byte-identical. All 35,236 original scene node IDs were retained.

The remaining city work includes populated merchant/adventurer streets, guild interiors and follow-up quests, fuller artisan/mage/noble districts, class mentors including the deferred Psion design, a detailed World Tree art pass and the separately designed direct-road repair/Roadwarden encounter. The order's name and politics are still open. This milestone does not silently settle those decisions.

No push or merge is included. Existing unrelated tracked work, installed optional art, configuration and user saves are preserved separately.
