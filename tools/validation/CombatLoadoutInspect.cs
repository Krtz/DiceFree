using System.Linq;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Items;
using DiceFree.Persistence;
using DiceFree.Briarback;
using UnityEngine;
public static class CombatLoadoutInspect
{
    public static object Run()
    {
        var p=Object.FindAnyObjectByType<TraversalInput>().GetComponent<CombatActor>();
        return Newtonsoft.Json.JsonConvert.SerializeObject(new {ready=p.GetComponent<ManifestationPersistence>().Ready,
            items=p.GetComponent<CarriedInventory>().Definitions.Where(d=>!d.consumable&&!d.modelTestingItem).Select(d=>new{d.stableId,d.displayName,d.itemLevel,slot=d.slot.ToString(),d.allowedClassIds,d.stats}).ToArray(),
            enemies=Object.FindObjectsByType<QuilboarCombat>().Select(q=>new{q.name,role=q.role.ToString(),level=q.GetComponent<ActorStats>().Level,hp=q.GetComponent<Health>().Maximum,position=q.transform.position.ToString()}).ToArray()});
    }
}
