using System;
using System.Linq;
using DiceFree.Banking;
using DiceFree.Combat;
using DiceFree.Items;
using DiceFree.UI;
using DiceFree.Characters;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEngine;

namespace DiceFree.EditorTools
{
    internal static class BagAndMinimapValidation
    {
        private static void Check(bool good,string why)
        {
            if(!good)throw new InvalidOperationException(why);
        }

        [CliCommand("dicefree.items.drop.playtest",
            "Test right-click bag drop, real chest pickup and identity preservation.")]
        public static object DropPlaytest()
        {
            if(!EditorApplication.isPlaying)throw new InvalidOperationException("Play Mode only");
            var player=UnityEngine.Object.FindFirstObjectByType<TraversalInput>();
            Check(player!=null,"Player not spawned");
            var actor=player.GetComponent<CombatActor>();
            var inventory=player.GetComponent<CarriedInventory>();
            var bag=player.GetComponent<InventoryPanel>();
            Check(inventory!=null&&bag!=null,"Bag not found");
            var def=inventory.Definitions.FirstOrDefault(x=>x!=null&&!x.questItem);
            Check(def!=null,"No test item definition");
            var item=inventory.Grant(def.stableId);
            string id=item.instanceId;
            var ledger=player.GetComponent<WorldDroppedItemLedger>();
            Check(ledger!=null,"Persistent world-drop ledger not registered");
            Check(bag.RunItemAction(id,"Drop",out var message),
                "Bag drop failed: "+message);
            Check(inventory.Find(id)==null,"Dropped item remains in inventory");
            Check(ledger.Count==1 && ledger.CaptureJson().Contains(id),
                "Dropped item was not saved in the Echo's durable ledger");
            var snapshot=ledger.CaptureJson();
            ledger.RestoreJson(ledger.Version,snapshot);
            Check(ledger.Count==1,
                "World drop was not re-created when restoring saved state");
            var drop=UnityEngine.Object.FindObjectsByType<DroppedItemPickup>(
                FindObjectsSortMode.None).FirstOrDefault(x=>x.Item?.instanceId==id);
            Check(drop!=null,"World chest pickup does not contain dropped item");
            Check(drop.GetComponent<Collider>()!=null,"Dropped chest is not physical");
            Check(drop.Item.instanceId==id && drop.Item.definitionId==def.stableId,
                "Original item identity/definition not preserved");
            Check(drop.TryPickup(actor),"Cannot pick up dropped chest");
            var restored=inventory.Find(id);
            Check(restored!=null && restored.instanceId==id &&
                restored.definitionId==def.stableId&&restored.bound==item.bound,
                "Pickup changed identity or item properties");
            Check(!drop.TryPickup(actor),"Repeated pickup duplicated item");
            Check(inventory.Remove(id),"Could not clean up test grant");
            return new{success=true,returnedSameInstance=true,
                usableGroundChest=true,duplicatePickupRejected=true,
                saveRoundTrip=true,ledgerCleared=ledger.Count==0};
        }

        [CliCommand("dicefree.items.bank-from-bag.playtest",
            "Verify bag Send to Bank deposits through the real shared bank.")]
        public static object BankPlaytest()
        {
            if(!EditorApplication.isPlaying)throw new InvalidOperationException("Play Mode only");
            var player=UnityEngine.Object.FindFirstObjectByType<TraversalInput>();
            var inventory=player.GetComponent<CarriedInventory>();
            var bag=player.GetComponent<InventoryPanel>();
            var bank=player.GetComponent<EchoSharedBank>();
            Check(bank!=null,"Player has no shared bank");
            var def=inventory.Definitions.FirstOrDefault(x=>x!=null&&!x.questItem);
            Check(def!=null,"No test gear");
            var item=inventory.Grant(def.stableId);
            string baseline=bank.CaptureJson();
            try
            {
                Check(bag.RunItemAction(item.instanceId,"Send to Bank",out var message),
                    "Send to Bank failed: "+message);
                Check(inventory.Find(item.instanceId)==null &&
                    bank.Items.Any(i=>i.instanceId==item.instanceId),
                    "Bank transfer did not preserve item identity");
                return new{success=true,remoteDeposit=true,itemIdPreserved=true};
            }
            finally
            {
                bank.RestoreJson(bank.Version,baseline);
                if(inventory.Find(item.instanceId)!=null)
                    inventory.Remove(item.instanceId);
            }
        }

        [CliCommand("dicefree.items.bag-menu-preview",
            "Open inventory and its context menu for an actual Game-view screenshot.")]
        public static object BagMenuPreview()
        {
            if(!EditorApplication.isPlaying)throw new InvalidOperationException("Play Mode only");
            var player=UnityEngine.Object.FindFirstObjectByType<TraversalInput>();
            var inventory=player.GetComponent<CarriedInventory>();
            var equipment=player.GetComponent<Equipment>();
            var bag=player.GetComponent<InventoryPanel>();
            var item=equipment.BagItems.FirstOrDefault();
            if(item==null)
            {
                var definition=inventory.Definitions.FirstOrDefault(x=>x!=null&&!x.questItem);
                Check(definition!=null,"No preview item found");
                item=inventory.Grant(definition.stableId);
            }
            bag.Show();
            var flags=System.Reflection.BindingFlags.NonPublic|
                System.Reflection.BindingFlags.Instance;
            typeof(InventoryPanel).GetField("selectedItemId",flags)
                .SetValue(bag,item.instanceId);
            typeof(InventoryPanel).GetField("contextBounds",flags)
                .SetValue(bag,new Rect(120,290,158,114));
            return new{success=true,visible=true,
                definition=inventory.Resolve(item.definitionId)?.displayName,
                menu="Equip / Drop / Send to Bank"};
        }

        [CliCommand("dicefree.minimap.road-layer-check",
            "Ensure minimap renders original roads on layer 2 and excludes only fog.")]
        public static object MinimapRoads()
        {
            if(!EditorApplication.isPlaying)throw new InvalidOperationException("Play Mode only");
            var map=UnityEngine.Object.FindFirstObjectByType<MinimapHud>();
            var fog=UnityEngine.Object.FindFirstObjectByType<DiceFree.World.WorldFogOfWar>();
            Check(map!=null&&fog!=null,"Missing minimap or fog");
            var camera=UnityEngine.Object.FindObjectsByType<Camera>(
                FindObjectsSortMode.None).FirstOrDefault(c=>c.name=="HUD Minimap Camera");
            Check(camera!=null,"Minimap camera missing");
            Check((camera.cullingMask&(1<<2))!=0,"Road decoration layer is hidden");
            Check((camera.cullingMask&(1<<29))==0,"Fog overlay layer not excluded");
            var overlays=fog.GetComponentsInChildren<MeshRenderer>(true);
            Check(overlays.Any(r=>r.gameObject.layer==29),
                "Fog overlay still on shared road layer");
            return new{success=true,roadLayerVisible=true,
                dedicatedFogExcluded=true};
        }
    }
}
