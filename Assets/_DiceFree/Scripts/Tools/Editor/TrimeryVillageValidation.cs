using System;
using System.Linq;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Quests;
using DiceFree.World;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace DiceFree.EditorTools
{
    public static class TrimeryVillageValidation
    {
        private static void Need(bool condition,string message)
        {
            if(!condition)throw new InvalidOperationException(
                "TRIMERY VALIDATION: "+message);
        }
        private static T[] All<T>(Scene scene) where T:Component =>
            scene.GetRootGameObjects().SelectMany(g=>
                g.GetComponentsInChildren<T>(true)).ToArray();
        private static bool Route(Vector2 start,Vector2 finish)
        {
            bool a=NavMesh.SamplePosition(
                CornbergLandscape.Ground(start.x,start.y),
                out var source,3f,NavMesh.AllAreas);
            bool b=NavMesh.SamplePosition(
                CornbergLandscape.Ground(finish.x,finish.y),
                out var target,3f,NavMesh.AllAreas);
            var path=new NavMeshPath();
            return a && b && NavMesh.CalculatePath(
                source.position,target.position,NavMesh.AllAreas,path) &&
                path.status==NavMeshPathStatus.PathComplete;
        }
        [CliCommand("dicefree.trimery.validate",
            "Check authored settlement, three routes, stable revive stone, services, physical western road damage.")]
        public static object Validate()
        {
            var previous=EditorSceneManager.GetSceneManagerSetup();
            try
            {
                var scene=EditorSceneManager.OpenScene(
                    TrimeryVillageAuthoring.ScenePath,OpenSceneMode.Single);
                var all=All<Transform>(scene);
                var root=all.SingleOrDefault(t=>
                    t.name==TrimeryVillageAuthoring.RootName);
                Need(root!=null,"Trimery group missing from Cornberg scene.");
                var gates=all.Where(t=>t.IsChildOf(root)&&
                    (t.name.StartsWith("SOUTH -") ||
                     t.name.StartsWith("WEST -") ||
                     t.name.StartsWith("NORTH -"))).ToArray();
                Need(gates.Length==3,"Expected one guarded gate in each direction.");
                Need(gates.Select(x=>x.name.Split(' ')[0]).Distinct().Count()==3,
                    "Roads do not uniquely point South/West/North.");
                Need(all.Count(t=>t.IsChildOf(root) &&
                    t.name.StartsWith("Guard post "))==6,
                    "Each gate needs two real lookout towers.");
                Need(all.Count(t=>t.IsChildOf(root) &&
                    (t.name.StartsWith("Trimery South gate watch") ||
                     t.name.StartsWith("Trimery West gate watch") ||
                     t.name.StartsWith("Trimery North gate watch")))==6,
                    "Six village guards missing: "+string.Join(" / ",all.Where(t=>
                        t.name.Contains("watch") && t.IsChildOf(root))
                        .Select(t=>t.name)) );
                Need(all.Count(t=>t.IsChildOf(root)&&t.name==
                    "Timber structure")==7,"Expected seven timber buildings.");
                Need(all.Any(t=>t.IsChildOf(root)&&
                    t.name=="Working sawmill and lumber yard"),
                    "Trimery needs its own lumberworks.");
                Need(all.Count(t=>t.IsChildOf(root)&&
                    t.name.StartsWith("Cut timber log"))>=16,
                    "Lumberyard not stocked with cut timber.");
                var anchors=All<ResurrectionAnchor>(scene);
                var anchor=anchors.SingleOrDefault(a=>a.StableId==
                    TrimeryVillageAuthoring.AnchorId);
                Need(anchor!=null && Vector3.Distance(
                    anchor.transform.position,CornbergLandscape.Ground(250,181.8f))<1,
                    "Saved Trimery revive stone ID/location invalid.");
                var arrival=All<ReachArea>(scene).SingleOrDefault(a=>
                    a.StableId==CornbergDepartureSetup.DestinationId);
                Need(arrival!=null && arrival.Radius>=9f,
                    "Runner arrival trigger not linked to accepted quest.");
                var affected=All<InteractionTarget>(scene)
                    .Where(x=>x.transform.IsChildOf(root)).ToArray();
                Need(affected.Any(x=>x.name=="Trimery lumber trader") &&
                    affected.Any(x=>x.name=="Trimery banker"),
                    "Missing functional shop/bank interaction targets.");
                Need(affected.Any(x=>x.GetComponent<DiceFree.Items.ItemShop>()!=null),
                    "Trimery merchant does not have original trading component.");
                var terrain=All<MeshCollider>(scene).Where(m=>
                    m.transform.IsChildOf(root)&&m.gameObject.layer==8).ToArray();
                Need(terrain.Length==1 && terrain[0].sharedMesh!=null,
                    "Trimery continuous collision ground missing.");
                var obstacles=All<Collider>(scene)
                    .Where(x=>x.transform.IsChildOf(root)&&
                        x.name.StartsWith("Roadwarden smashed paving") &&
                        x.enabled && !x.isTrigger).ToArray();
                Need(obstacles.Length>=10,
                    "Blocked west road must have visible solid broken paving.");
                var art=All<TrimeryArtSlot>(scene)
                    .Where(x=>x.transform.IsChildOf(root)).ToArray();
                Need(art.Length>=16,
                    "Asset-backed or portable fallback lumber art missing.");
                Need(art.All(x=>!string.IsNullOrWhiteSpace(x.Resource)),
                    "Some vendor art slots have no portable resource identifier.");
                var navigation=All<WorldNavigation>(scene).FirstOrDefault();
                Need(navigation!=null && navigation.Data!=null,
                    "WorldNavigation missing/unsaved.");
                var instance=NavMesh.AddNavMeshData(navigation.Data);
                try
                {
                    Need(Route(new Vector2(223,149),new Vector2(248,161)),
                        "Ironpine approach cannot reach Trimery south gate.");
                    Need(Route(new Vector2(248,161),new Vector2(248,183)),
                        "South gate cannot reach the village services / revive stone.");
                    Need(Route(new Vector2(248,161),new Vector2(248,230)),
                        "South gate cannot exit north into planned quilboar biome.");
                    Need(Route(new Vector2(248,183),new Vector2(220,180)),
                        "Village core cannot reach west trade gate.");
                    Need(Route(new Vector2(248,183),new Vector2(258,183)),
                        "Village core cannot reach merchant.");
                    Need(Route(new Vector2(248,183),new Vector2(237,177)),
                        "Village core cannot reach banker.");
                }
                finally{instance.Remove();}
                Debug.Log("TRIMERY_VILLAGE_VALIDATED: three gated routes, 6 guards, 7 homes, sawmill, licensed art seams, safe stone, trader/banker, linked Runner arrival");
                return new{success=true,marker="TRIMERY_VILLAGE_VALIDATED",
                    gates=3,guards=6,buildings=7,
                    artSlots=art.Length,revive=anchor.StableId,
                    runnerArrival=arrival.StableId,
                    blockers=obstacles.Length,
                    routesChecked=6};
            }
            finally{if(previous.Length>0)
                EditorSceneManager.RestoreSceneManagerSetup(previous);}
        }
    }
}
