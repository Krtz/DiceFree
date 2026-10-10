using System;
using System.Linq;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
namespace DiceFree.EditorTools
{
    internal static class CornbergForestSightFix
    {
        private const string ScenePath="Assets/_DiceFree/Scenes/Cornberg.unity";
        private static void Require(bool okay,string message)
        {
            if(!okay)throw new InvalidOperationException(message);
        }
        private static Transform[] All(Scene scene) =>
            scene.GetRootGameObjects().SelectMany(root=>
                root.GetComponentsInChildren<Transform>(true)).ToArray();

        [CliCommand("dicefree.forest.sight.install",
            "Turn the Brewery and General Goods to face the road; reinforce solid tree trunks.")]
        public static object Install()
        {
            Require(!EditorApplication.isPlayingOrWillChangePlaymode,"Edit mode required");
            for(int i=0;i<SceneManager.sceneCount;i++)
                Require(!SceneManager.GetSceneAt(i).isDirty,"Save unsaved scene before edit");
            var old=EditorSceneManager.GetSceneManagerSetup();
            try
            {
                var scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
                var all=All(scene);
                int buildingsRotated=0;
                foreach(var spec in new[]{("Brewery",new Vector2(-4,-14)),
                                            ("General goods",new Vector2(17,-13))})
                {
                    var house=all.Single(t=>t.name==spec.Item1);
                    var foundation=house.GetComponentsInChildren<Renderer>(true)
                        .First(r=>r.name=="Fieldstone foundation");
                    Require(Vector2.Distance(new Vector2(foundation.bounds.center.x,
                        foundation.bounds.center.z),spec.Item2)<.5f,
                        "Found wrong house "+spec.Item1);
                    var door=house.GetComponentsInChildren<Renderer>(true)
                        .First(r=>r.name=="Door - exterior blockout");
                    if(door.bounds.center.z>=spec.Item2.y+2)continue;
                    Vector3 pivot=new Vector3(spec.Item2.x,0,spec.Item2.y);
                    Quaternion turn=Quaternion.Euler(0,180,0);
                    foreach(Transform child in house)
                    {
                        // Vats, tables, and benches are separate landscape dressing.
                        // Only the building shell and associated sign should rotate.
                        if(child.name.StartsWith("Working brew vat")||
                           child.name.StartsWith("Community table")||
                           child.name.StartsWith("Communal bench"))continue;
                        child.position=pivot+turn*(child.position-pivot);
                        child.rotation=turn*child.rotation;
                    }
                    Require(door.bounds.center.z>spec.Item2.y+2f,
                        "Door still faces away from road "+spec.Item1);
                    buildingsRotated++;
                }
                // The brewery's preexisting vats stood in front of the newly
                // road-facing entrance. Put the brewing equipment alongside the
                // building instead, leaving a clear approach to the door.
                var brewery=all.Single(t=>t.name=="Brewery");
                var vats=brewery.Cast<Transform>().Where(t=>t.name=="Working brew vat")
                    .OrderBy(t=>t.position.x).ToArray();
                Require(vats.Length==3,"Three brewery vats expected");
                for(int i=0;i<vats.Length;i++)
                    vats[i].position=new Vector3(2.5f,vats[i].position.y,
                        -16.6f+i*2.8f);
                var brewDoor=brewery.GetComponentsInChildren<Renderer>(true)
                    .First(r=>r.name=="Door - exterior blockout").bounds.center;
                Require(vats.All(v=>Vector2.Distance(
                    new Vector2(v.position.x,v.position.z),
                    new Vector2(brewDoor.x,brewDoor.z))>3.2f),
                    "Brewery entrance obstructed by vat");
                int activeTrees=0,collisionsAdjusted=0,missing=0;
                foreach(var t in all.Where(t=>t.name.Contains("Tree_") &&
                         t.GetComponentsInChildren<Renderer>(true).Length>=2 &&
                         t.gameObject.activeInHierarchy))
                {
                    var c=t.GetComponentsInChildren<CapsuleCollider>(true)
                        .FirstOrDefault(x=>x.name=="Playtest trunk collision" &&
                                         x.enabled&&x.gameObject.activeInHierarchy);
                    if(c==null)
                    {
                        // Repair an actual tree whose mesh was left without a
                        // matching gameplay trunk (not an invisible canopy collider).
                        var go=new GameObject("Playtest trunk collision");
                        go.layer=9;
                        go.transform.SetParent(t,false);
                        go.transform.localPosition=Vector3.zero;
                        c=go.AddComponent<CapsuleCollider>();
                        c.center=new Vector3(0,1.2f,0);
                        c.height=2.4f;
                        c.radius=.75f;
                        var carved=go.AddComponent<NavMeshObstacle>();
                        carved.shape=NavMeshObstacleShape.Capsule;
                        carved.center=c.center;
                        carved.height=c.height;
                        carved.radius=.75f;
                        carved.carving=true;
                        missing++;
                    }
                    activeTrees++;
                    float worldScale=Mathf.Max(.001f,Mathf.Max(
                        Mathf.Abs(c.transform.lossyScale.x),Mathf.Abs(c.transform.lossyScale.z)));
                    // A trunk is solid even if the decorative branches are wider.
                    // Minimum diameter 1.55m (not the full many-metre leaf canopy).
                    float desiredWorldRadius=.78f;
                    if(c.radius*worldScale<desiredWorldRadius)
                    {
                        c.radius=desiredWorldRadius/worldScale;
                        EditorUtility.SetDirty(c);
                        collisionsAdjusted++;
                    }
                    var obstacle=t.GetComponentsInChildren<NavMeshObstacle>(true)
                        .FirstOrDefault(o=>o.enabled&&o.gameObject.activeInHierarchy);
                    if(obstacle!=null&&obstacle.shape==NavMeshObstacleShape.Capsule)
                    {
                        float oScale=Mathf.Max(.001f,Mathf.Max(
                            Mathf.Abs(obstacle.transform.lossyScale.x),
                            Mathf.Abs(obstacle.transform.lossyScale.z)));
                        if(obstacle.radius*oScale<desiredWorldRadius)
                        {
                            obstacle.radius=desiredWorldRadius/oScale;
                            EditorUtility.SetDirty(obstacle);
                        }
                    }
                }
                Require(activeTrees>=600,
                    "Too few active physical trees: "+activeTrees);
                EditorSceneManager.MarkSceneDirty(scene);
                Require(EditorSceneManager.SaveScene(scene),"Save Cornberg failed");
                return new{success=true,buildingsRotated,activeTrees,
                    collisionsAdjusted,missingTreeCollidersRepaired=missing,minimumTreeTrunkDiameter=1.56f};
            }
            finally{EditorSceneManager.RestoreSceneManagerSetup(old);}
        }

        [CliCommand("dicefree.forest.sight.validate",
            "Check 2 new road-facing doors and the live solid trunk collider diameter.")]
        public static object Validate()
        {
            var old=EditorSceneManager.GetSceneManagerSetup();
            try
            {
                var s=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
                var all=All(s);
                foreach(var spec in new[]{("Brewery",-14f),("General goods",-13f)})
                {
                    var h=all.Single(t=>t.name==spec.Item1);
                    var door=h.GetComponentsInChildren<Renderer>(true)
                        .First(r=>r.name=="Door - exterior blockout");
                    Require(door.bounds.center.z>spec.Item2+2,
                        "Door not facing road "+spec.Item1);
                }
                var vats=all.Single(t=>t.name=="Brewery").Cast<Transform>()
                    .Where(t=>t.name=="Working brew vat").ToArray();
                Require(vats.Length==3&&vats.All(v=>v.position.x>1.8f),
                    "Brewing vats still block entrance");
                int trees=0;
                foreach(var tree in all.Where(t=>t.name.Contains("Tree_")&&
                          t.GetComponentsInChildren<Renderer>(true).Length>=2 &&
                          t.gameObject.activeInHierarchy))
                {
                    var c=tree.GetComponentsInChildren<CapsuleCollider>(true)
                        .FirstOrDefault(v=>v.name=="Playtest trunk collision"&&v.enabled);
                    Require(c!=null,"Tree missing solid trunk "+tree.name);
                    float scale=Mathf.Max(Mathf.Abs(c.transform.lossyScale.x),
                        Mathf.Abs(c.transform.lossyScale.z));
                    Require(c.radius*scale>.77f,"Small collision "+tree.name);
                    trees++;
                }
                Require(trees>=600,"Too few physical trees");
                return new{success=true,doorsFacingRoad=2,solidTrees=trees};
            }
            finally{EditorSceneManager.RestoreSceneManagerSetup(old);}
        }
    }
}