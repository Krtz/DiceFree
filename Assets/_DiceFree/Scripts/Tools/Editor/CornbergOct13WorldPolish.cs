using System;
using System.Collections.Generic;
using System.Linq;
using DiceFree.World;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace DiceFree.EditorTools
{
    public static class CornbergOct13WorldPolish
    {
        private const string ScenePath="Assets/_DiceFree/Scenes/Cornberg.unity";
        private static readonly Vector2[] Route={
            new(112,43),new(140,44),new(158,45),new(175,47),
            new(174,64),new(180,80),new(183,98),new(196,112),
            new(208,130),new(223,149)
        };
        private static T[] All<T>(Scene scene) where T:Component =>
            scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<T>(true)).ToArray();
        private static float DistanceToTrail(Vector2 p)
        {
            float best=float.MaxValue;
            for(int i=1;i<Route.Length;i++)
            {
                Vector2 a=Route[i-1],b=Route[i],d=b-a;
                float t=Mathf.Clamp01(Vector2.Dot(p-a,d)/Mathf.Max(.001f,d.sqrMagnitude));
                best=Mathf.Min(best,Vector2.Distance(p,a+d*t));
            }
            return best;
        }
        private static Vector3 Ground(Vector2 point)=>
            CornbergLandscape.Ground(point.x,point.y);
        [CliCommand("dicefree.oct13.road-colliders",
            "Diagnose blocking forest colliders, NavMesh gaps and shop NPC positions.")]
        public static object Inspect()
        {
            var setup=EditorSceneManager.GetSceneManagerSetup();
            try
            {
                var scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
                var colliders=All<Collider>(scene)
                    .Where(c=>c.enabled && c.gameObject.activeInHierarchy &&
                        c.gameObject.layer!=8 && !c.isTrigger)
                    .Select(c=>new{c,d=DistanceToTrail(new Vector2(
                        c.bounds.center.x,c.bounds.center.z))})
                    .Where(x=>x.d<4f && x.c.bounds.center.x>118f)
                    .OrderBy(x=>x.d).Take(55)
                    .Select(x=>new{name=x.c.name,root=x.c.transform.root.name,
                        world=x.c.transform.position.ToString(),
                        size=x.c.bounds.size.ToString(),
                        x=x.c.bounds.center.x,z=x.c.bounds.center.z,
                        roadDistance=x.d,type=x.c.GetType().Name})
                    .ToArray();
                var npcs=All<Transform>(scene).Where(t=>
                    t.name.StartsWith("NPCs/NPC_Merchant") ||
                    t.name.StartsWith("NPCs/NPC_Alchemist"))
                    .Select(t=>new{name=t.name,location=t.position.ToString()})
                    .ToArray();
                var world=All<WorldNavigation>(scene).FirstOrDefault();
                var navFaults=new List<string>();
                if(world!=null&&world.Data!=null)
                {
                    var registration=NavMesh.AddNavMeshData(world.Data);
                    try
                    {
                        for(int i=1;i<Route.Length;i++)
                            for(float along=0;along<=1f;along+=.08f)
                            {
                                Vector2 v=Vector2.Lerp(Route[i-1],Route[i],along);
                                Vector3 point=Ground(v);
                                if(!NavMesh.SamplePosition(point,out var hit,.6f,NavMesh.AllAreas))
                                    navFaults.Add(v.ToString());
                            }
                    }
                    finally{registration.Remove();}
                }
                return new{npcs,roadColliders=colliders,navFaultCount=navFaults.Count,
                    navGaps=navFaults.Take(22).ToArray()};
            }
            finally{if(setup.Length>0)EditorSceneManager.RestoreSceneManagerSetup(setup);}
        }
        [CliCommand("dicefree.oct13.clear-road",
            "Move five road-intruding forest trees and their real colliders/obstacles off Ironpine travel corridor.")]
        public static object ClearRoad()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Stop Play Mode before moving trees.");
            for(int i=0;i<SceneManager.sceneCount;i++)
                if(SceneManager.GetSceneAt(i).isDirty)
                    throw new InvalidOperationException("Save unsaved scene first.");
            var setup=EditorSceneManager.GetSceneManagerSetup();
            try
            {
                var scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
                int moved=0;
                var adjusted=new List<string>();
                foreach(var c in All<CapsuleCollider>(scene).Where(
                    c=>c.enabled && c.name=="Playtest trunk collision" &&
                    c.gameObject.activeInHierarchy &&
                    c.bounds.center.x>118f).ToArray())
                {
                    Vector2 point=new(c.bounds.center.x,c.bounds.center.z);
                    float original=DistanceToTrail(point);
                    if(original>=4.15f)continue;
                    Vector2 closest=Route[0];
                    Vector2 axis=Vector2.right;
                    float best=float.MaxValue;
                    for(int i=1;i<Route.Length;i++)
                    {
                        var a=Route[i-1];var b=Route[i];var delta=b-a;
                        float t=Mathf.Clamp01(Vector2.Dot(point-a,delta)/
                            Mathf.Max(.001f,delta.sqrMagnitude));
                        var projected=a+delta*t;
                        float dist=Vector2.Distance(projected,point);
                        if(dist>=best)continue;
                        best=dist;closest=projected;axis=delta.normalized;
                    }
                    var outward=new Vector2(-axis.y,axis.x);
                    var signed=Vector2.Dot(point-closest,outward);
                    if(Mathf.Abs(signed)<.04f)signed=point.x%2<1?-1f:1f;
                    Vector2 direction=outward*Mathf.Sign(signed);
                    Vector2 next=point+direction*(4.45f-original);
                    Transform tree=c.transform.parent;
                    if(tree==null || !tree.name.Contains("Tree_"))
                        throw new InvalidOperationException(
                            "Unexpected physical trunk parent "+c.name);
                    var old=tree.position;
                    var oldGround=Ground(new Vector2(old.x,old.z));
                    var newGround=Ground(new Vector2(old.x+next.x-point.x,
                        old.z+next.y-point.y));
                    tree.position=old+new Vector3(next.x-point.x,
                        newGround.y-oldGround.y,next.y-point.y);
                    EditorUtility.SetDirty(tree);
                    adjusted.Add(tree.name+" "+point+" -> "+next);
                    moved++;
                }
                Physics.SyncTransforms();
                if(moved<3 || moved>25)
                    throw new InvalidOperationException(
                        "Unexpected road collision count "+moved);
                EditorSceneManager.MarkSceneDirty(scene);
                if(!EditorSceneManager.SaveScene(scene))
                    throw new InvalidOperationException("Unable to save forest clearance");
                return new{success=true,movedTrees=moved,
                    remainingRoadblockColliders=All<CapsuleCollider>(scene).Count(c=>
                        c.enabled && c.name=="Playtest trunk collision" &&
                        c.bounds.center.x>118f &&
                        DistanceToTrail(new Vector2(c.bounds.center.x,
                            c.bounds.center.z))<4.05f),
                    moves=adjusted.ToArray()};
            }
            finally{if(setup.Length>0)EditorSceneManager.RestoreSceneManagerSetup(setup);}
        }
        [CliCommand("dicefree.oct13.move-vendors",
            "Move General Goods merchant and Brewery alchemist to road-facing doors.")]
        public static object MoveVendors()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Stop Play Mode before placement.");
            for(int i=0;i<SceneManager.sceneCount;i++)
                if(SceneManager.GetSceneAt(i).isDirty)
                    throw new InvalidOperationException("Save unsaved scenes first.");
            var setup=EditorSceneManager.GetSceneManagerSetup();
            try
            {
                var scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
                var all=All<Transform>(scene);
                int moved=0;
                foreach(var item in new[]{
                    (name:"NPCs/NPC_Merchant - 3D",spot:new Vector2(17f,-8.65f)),
                    (name:"NPCs/NPC_Alchemist - 3D",spot:new Vector2(-3f,-8.7f))})
                {
                    var obj=all.Single(t=>t.name==item.name);
                    var before=obj.position;
                    Vector3 ground=Ground(item.spot);
                    obj.position=new Vector3(item.spot.x,ground.y, item.spot.y);
                    // Keep the shop-facing front roughly toward the arriving road.
                    obj.rotation=Quaternion.LookRotation(
                        new Vector3(0,0,1),Vector3.up);
                    if(Vector3.Distance(before,obj.position)>.01f)moved++;
                    EditorUtility.SetDirty(obj);
                }
                EditorSceneManager.MarkSceneDirty(scene);
                if(!EditorSceneManager.SaveScene(scene))
                    throw new InvalidOperationException("Unable to save saved Cornberg scene.");
                return new{success=true,npcsPlaced=moved,
                    merchant="(17,-8.65)",brewer="(-3,-8.7)"};
            }
            finally{if(setup.Length>0)EditorSceneManager.RestoreSceneManagerSetup(setup);}
        }
    }
}
