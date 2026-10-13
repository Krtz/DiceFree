using System;
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
    public static class TrimerySiteRecon
    {
        [CliCommand("dicefree.trimery.site.inspect",
            "Survey existing NavMesh, terrain and locally licensed art around Trimery.")]
        public static object Inspect()
        {
            var prior=EditorSceneManager.GetSceneManagerSetup();
            try
            {
                var scene=EditorSceneManager.OpenScene(
                    "Assets/_DiceFree/Scenes/Cornberg.unity",OpenSceneMode.Single);
                var nav=scene.GetRootGameObjects().SelectMany(g=>
                    g.GetComponentsInChildren<WorldNavigation>(true)).First();
                var instance=NavMesh.AddNavMeshData(nav.Data);
                try
                {
                    var positions=new[]{new Vector2(223,149),new Vector2(240,155),
                        new Vector2(248,174),new Vector2(247,200),
                        new Vector2(218,178),new Vector2(268,174),
                        new Vector2(247,230),new Vector2(202,181)};
                    var samples=positions.Select(point=>
                    {
                        bool found=NavMesh.SamplePosition(
                            CornbergLandscape.Ground(point.x,point.y),
                            out var hit,2,NavMesh.AllAreas);
                        bool ground=Physics.Raycast(
                            new Vector3(point.x,25,point.y),Vector3.down,
                            out var hitGround,60,1<<8);
                        return new{x=point.x,z=point.y,
                            navFound=found,
                            navY=found?hit.position.y:0f,
                            ground=ground,
                            groundY=ground?hitGround.point.y:0f,
                            terrainY=CornbergLandscape.Height(point.x,point.y)};
                    }).ToArray();
                    var prefabs=new[]{
                        "Assets/Medieval Village Building Pack/Prefabs/Wall_Wood_2x2.prefab",
                        "Assets/Medieval Village Building Pack/Prefabs/Roof_Thatch35.prefab",
                        "Assets/Medieval Village Building Pack/Prefabs/Wood_Pillar_Thick.prefab",
                        "Assets/Low-Poly Medieval Market/Prefabs/stand_01.prefab",
                        "Assets/Low-Poly Medieval Market/Prefabs/stand_02.prefab",
                        "Assets/the_wood_cabin/Prefabs/THE_WOOD_CABIN_LOD0.prefab"
                    }.Select(path=>{
                        var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(path);
                        return new{path,loaded=prefab!=null,
                            renderers=prefab==null?0:prefab.GetComponentsInChildren<Renderer>(true).Length,
                            size=prefab==null?"":prefab.GetComponentsInChildren<Renderer>(true)
                                .Select(x=>x.bounds.size.magnitude)
                                .DefaultIfEmpty(0f).Max().ToString("0.0")};
                    }).ToArray();
                    var routeSamples=new[]{
                        new Vector2(223,149),new Vector2(248,153),
                        new Vector2(248,161),new Vector2(248,169),
                        new Vector2(248,174),new Vector2(248,179),new Vector2(248,182.4f),
                        new Vector2(249.5f,182.4f),new Vector2(251.5f,182.4f),
                        new Vector2(248,185),new Vector2(248,195),
                        new Vector2(248,205),new Vector2(248,215),
                        new Vector2(248,230),new Vector2(238,180),
                        new Vector2(230,180),new Vector2(220,180)};
                    var route=routeSamples.Select(v=>
                    {
                        bool success=NavMesh.SamplePosition(
                            CornbergLandscape.Ground(v.x,v.y),out var hit,
                            3f,NavMesh.AllAreas);
                        var overlaps=scene.GetRootGameObjects()
                            .SelectMany(g=>g.GetComponentsInChildren<Collider>(true))
                            .Where(c=>c.enabled && !c.isTrigger &&
                                c.gameObject.layer==9 &&
                                c.bounds.Contains(CornbergLandscape.Ground(v.x,v.y)+
                                    Vector3.up*.9f))
                            .Take(8).Select(c=>c.name).ToArray();
                        return new{point=v.ToString(),found=success,
                            nearest=success?hit.position.ToString():"",
                            delta=success?Vector3.Distance(
                                hit.position,CornbergLandscape.Ground(v.x,v.y)):0f,
                            obstacles=overlaps};
                    }).ToArray();
                    var pairPaths=new[]{
                        (a:new Vector2(223,149),b:new Vector2(248,161)),
                        (a:new Vector2(248,161),b:new Vector2(248,185)),
                        (a:new Vector2(248,161),b:new Vector2(248,230)),
                        (a:new Vector2(248,185),b:new Vector2(220,180))};
                    var pathResults=pairPaths.Select(pair=>
                    {
                        bool from=NavMesh.SamplePosition(
                            CornbergLandscape.Ground(pair.a.x,pair.a.y),
                            out var start,3f,NavMesh.AllAreas);
                        bool to=NavMesh.SamplePosition(
                            CornbergLandscape.Ground(pair.b.x,pair.b.y),
                            out var end,3f,NavMesh.AllAreas);
                        var path=new NavMeshPath();
                        bool navigable=from && to && NavMesh.CalculatePath(
                            start.position,end.position,NavMesh.AllAreas,path);
                        return new{from=pair.a.ToString(),to=pair.b.ToString(),
                            path=navigable?path.status.ToString():"Unavailable",
                            corners=path.corners.Length};
                    }).ToArray();
                    return new{success=true,samples,prefabs,route,pathResults};
                }
                finally{instance.Remove();}
            }
            finally{if(prior.Length>0)EditorSceneManager.RestoreSceneManagerSetup(prior);}
        }
    }
}
