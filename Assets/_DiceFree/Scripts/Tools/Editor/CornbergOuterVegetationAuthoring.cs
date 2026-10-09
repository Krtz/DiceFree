using System;
using System.Collections.Generic;
using System.Linq;
using DiceFree.Core;
using DiceFree.World;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace DiceFree.EditorTools
{
    // A scoped follow-up to the saved Cornberg expansion. Never regenerates the village or forest.
    public static class CornbergOuterVegetationAuthoring
    {
        private const string Cornberg = "Assets/_DiceFree/Scenes/Cornberg.unity";
        private const string OuterName = "11 - Dense woodland outer perimeter";
        private const string InnerName = "07 - Natural boundary - expand northeast from here";
        private const string ExpandedName = "Expanded northeast slime woodland";
        private const string ConnectionName = "Woodland tree";
        private const float MinEast = 130f;
        private static T[] All<T>(Scene scene) where T:Component =>
            scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<T>(true)).ToArray();
        private static Transform Find(Scene scene,string name) =>
            All<Transform>(scene).FirstOrDefault(t=>t.name==name);
        private static void Require(bool ok,string reason)
        {
            if(!ok)throw new InvalidOperationException("CORNBERG_OUTER_WOODLAND: "+reason);
        }
        private static GameObject NewGroup(string name,Transform parent)
        {
            var existing=parent.Find(name);
            if(existing!=null)return existing.gameObject;
            var go=new GameObject(name);
            go.transform.SetParent(parent,false);
            return go;
        }
        private static NavMeshDataInstance Navigation(Scene s)
        {
            var n=All<WorldNavigation>(s).FirstOrDefault();
            Require(n!=null&&n.Data!=null,"Original WorldNavigation asset missing");
            return NavMesh.AddNavMeshData(n.Data);
        }
        private static bool IsInteriorRidge(Transform t) =>
            t.gameObject.activeSelf && t.name=="Mossy ridge" &&
            t.position.x>=91f && t.position.z>=26f;
        private static bool IsBlockingSeam(Transform model) =>
            model.name.StartsWith("Trees/Tree_",StringComparison.Ordinal) &&
            model.parent!=null && model.parent.name==ConnectionName &&
            model.parent.gameObject.activeSelf &&
            model.position.x>=120f && model.position.x<=149f &&
            ((model.position.z>=23f && model.position.z<=50f) ||
             (model.position.z>=59f && model.position.z<=78f));

        private static Vector3 Ground(float x,float z) => CornbergLandscape.Ground(x,z);

        [CliCommand("dicefree.cornberg-perimeter.install",
            "Clear obsolete dense inner woodland and enclose eastern expansion with dense 3D foliage.")]
        public static object Install()
        {
            Require(!EditorApplication.isPlayingOrWillChangePlaymode,"Stop Play Mode");
            var s=SceneManager.GetSceneByPath(Cornberg);
            bool opened=!s.isLoaded;
            if(opened)s=EditorSceneManager.OpenScene(Cornberg,OpenSceneMode.Additive);
            try
            {
                var root=Find(s,"Cornberg - World 1 starter pocket");
                var original=Find(s,InnerName);
                var expanded=Find(s,ExpandedName);
                Require(root!=null&&original!=null&&expanded!=null,
                    "Expected authored Cornberg terrain, old boundary and expanded forest");
                int oldRidges=0,oldSigns=0,seamTrees=0,newTrees=0;
                // The old slanting decorative ridge lies INSIDE the larger playable woodland.
                foreach(var t in original.Cast<Transform>().ToArray())
                {
                    if(IsInteriorRidge(t)) {t.gameObject.SetActive(false);oldRidges++;}
                    if(t.name=="Sign - Dense woodland"&&t.gameObject.activeSelf)
                    {t.gameObject.SetActive(false);oldSigns++;}
                }
                // Open two broad approaches; deactivate whole individual woodland-tree roots,
                // preserving the authored objects in the hierarchy for future edits.
                var blocked=All<Transform>(s).Where(IsBlockingSeam).Select(t=>t.parent)
                    .Distinct().ToArray();
                foreach(var tree in blocked){tree.gameObject.SetActive(false);seamTrees++;}
                var outer=NewGroup(OuterName,root);
                var east=NewGroup("Dense eastern tree edge",outer.transform);
                var north=NewGroup("Dense northern tree edge",outer.transform);
                var south=NewGroup("Dense southern tree edge",outer.transform);
                var hard=NewGroup("Physical outer edge",outer.transform);

                // Reuse DiceFree's ALREADY IMPORTED woodland assets, their mesh renderers,
                // trunk-only colliders, and the camera-occlusion and NavMeshObstacle components.
                var templates=expanded.Cast<Transform>().Where(t=>t.gameObject.activeSelf &&
                        t.name.StartsWith("Trees/Tree_",StringComparison.Ordinal) &&
                        t.GetComponentsInChildren<Renderer>(true).Length>0)
                    .GroupBy(t=>t.name).Select(g=>g.First()).OrderBy(t=>t.name).ToArray();
                Require(templates.Length>=6,"At least six original 3D woodland species required");
                var rng=new System.Random(91055);
                void Row(Transform parent,string prefix,Vector2 start,Vector2 end,int count)
                {
                    for(int i=0;i<count;i++)
                    {
                        var name=prefix+" "+i.ToString("D3");
                        if(parent.Cast<Transform>().Any(child=>child.name.StartsWith(name+" - ",StringComparison.Ordinal)))continue;
                        float t=(i+.12f+(float)rng.NextDouble()*.55f)/(count+.2f);
                        float x=Mathf.Lerp(start.x,end.x,t);
                        float z=Mathf.Lerp(start.y,end.y,t);
                        var location=Ground(x,z);
                        var template=templates[(i*7+rng.Next(templates.Length))%templates.Length];
                        var tree=Object.Instantiate(template.gameObject,location,
                            Quaternion.Euler(0,(float)rng.NextDouble()*360,0),parent);
                        tree.name=name+" - "+template.name;
                        float scale=.95f+(float)rng.NextDouble()*.5f;
                        tree.transform.localScale=template.localScale*scale;
                        // Original tree components handle trunk collision and camera fade.
                        Require(tree.GetComponentsInChildren<Collider>(true).Any(c=>c.enabled&&!c.isTrigger),
                            "Cloned tree lacks enabled trunk collision: "+name);
                        Require(tree.GetComponentsInChildren<NavMeshObstacle>(true).Any(c=>c.enabled),
                            "Cloned tree lacks nav obstacle: "+name);
                        newTrees++;
                    }
                }
                // Existing walkable forest reaches roughly X236/Z166. Border it outside
                // the encounters so the dense wall no longer divides the playable area.
                Row(east.transform,"East inside",new Vector2(243.2f,7),new Vector2(243.2f,180),30);
                Row(east.transform,"East outside",new Vector2(249.0f,7),new Vector2(249.0f,180),30);
                Row(north.transform,"North inside",new Vector2(MinEast,177.0f),new Vector2(248,177),21);
                Row(north.transform,"North outside",new Vector2(MinEast,184.0f),new Vector2(248,184),21);
                Row(south.transform,"South inside",new Vector2(MinEast,10.0f),new Vector2(248,10),21);
                Row(south.transform,"South outside",new Vector2(MinEast,3.0f),new Vector2(248,3),21);

                // A foliage wall should actually bound the area: invisible physical
                // edges prevent walking between the decorative trees.
                void Physical(string name, Vector3 center, Vector3 size)
                {
                    var go=NewGroup(name,hard.transform);
                    go.transform.position=center;
                    go.transform.rotation=Quaternion.identity;
                    go.transform.localScale=Vector3.one;
                    var collider=go.GetComponent<BoxCollider>();
                    if(collider==null)collider=go.AddComponent<BoxCollider>();
                    collider.isTrigger=false;
                    collider.size=size;
                    collider.center=Vector3.zero;
                    var obstacle=go.GetComponent<NavMeshObstacle>();
                    if(obstacle==null)obstacle=go.AddComponent<NavMeshObstacle>();
                    obstacle.shape=NavMeshObstacleShape.Box;
                    obstacle.center=Vector3.zero;
                    obstacle.size=size;
                    obstacle.carving=true;
                    obstacle.enabled=true;
                }
                Physical("East edge",new Vector3(248.2f,3.6f,94f),new Vector3(3.5f,8f,181f));
                Physical("North edge",new Vector3(186.5f,3.6f,183.2f),new Vector3(124f,8f,3.5f));
                Physical("South edge",new Vector3(186.5f,3.6f,4.4f),new Vector3(124f,8f,3.5f));

                EditorSceneManager.MarkSceneDirty(s);
                Require(EditorSceneManager.SaveScene(s),"Could not save Cornberg");
                AssetDatabase.SaveAssets();
                return new {status="CORNBERG_OUTER_WOODLAND_INSTALLED",oldRidgesDisabled=oldRidges,
                    oldSignsDisabled=oldSigns,seamTreesOpened=seamTrees,perimeterTreesCreated=newTrees,
                    totalPerimeterTrees=east.transform.childCount+north.transform.childCount+
                        south.transform.childCount,physicalWalls=hard.transform.childCount};
            }
            finally {if(opened) EditorSceneManager.CloseScene(s,true);}
        }

        [CliCommand("dicefree.cornberg-perimeter.validate",
            "Confirm old internal wall removed and new exterior vegetation barrier retained.")]
        public static object Validate()
        {
            var s=SceneManager.GetSceneByPath(Cornberg);
            bool opened=!s.isLoaded;
            if(opened)s=EditorSceneManager.OpenScene(Cornberg,OpenSceneMode.Additive);
            var n=Navigation(s);
            try
            {
                var old=Find(s,InnerName);
                var outer=Find(s,OuterName);
                Require(old!=null&&outer!=null,"Required natural boundary missing");
                Require(old.Cast<Transform>().All(t=>!IsInteriorRidge(t)),
                    "Old diagonal ridge still obstructs expanded region");
                Require(old.Cast<Transform>().All(t=>t.name!="Sign - Dense woodland"||
                    !t.gameObject.activeSelf),"Old inner dense woodland sign still displayed");
                var trees=outer.GetComponentsInChildren<Transform>(true).Where(t=>
                    t.name.StartsWith("East inside")||t.name.StartsWith("East outside")||
                    t.name.StartsWith("North inside")||t.name.StartsWith("North outside")||
                    t.name.StartsWith("South inside")||t.name.StartsWith("South outside")).ToArray();
                Require(trees.Length>=120&&trees.All(t=>t.gameObject.activeSelf),
                    "Outer thicket missing full tree enclosure");
                Require(trees.All(t=>t.GetComponentsInChildren<Collider>(true).Any(c=>c.enabled&&!c.isTrigger)),
                    "Outer vegetation missing tree trunk collision");
                var wall=outer.Find("Physical outer edge");
                Require(wall!=null&&wall.childCount==3 &&
                    wall.Cast<Transform>().All(t=>t.GetComponent<BoxCollider>()?.enabled==true&&
                    t.GetComponent<NavMeshObstacle>()?.carving==true),
                    "Outer tree border has passable physical gaps");
                var seam=All<Transform>(s).Where(t=>t.name.StartsWith("Trees/Tree_",StringComparison.Ordinal)&&
                    t.parent!=null&&t.parent.name==ConnectionName&&
                    t.position.x>=120&&t.position.x<=149&&
                    ((t.position.z>=23&&t.position.z<=50)||(t.position.z>=59&&t.position.z<=78))).ToArray();
                Require(seam.Count(t=>t.parent.gameObject.activeSelf)<5,
                    "Former dense tree seam still blocks access");
                var start=Vector3.zero;
                var end=Vector3.zero;
                Require(NavMesh.SamplePosition(new Vector3(112,0,43),out var hitStart,8,NavMesh.AllAreas),
                    "Western forest approach not walkable");
                Require(NavMesh.SamplePosition(new Vector3(175,0,45),out var hitEnd,8,NavMesh.AllAreas),
                    "Eastern fighting area not walkable");
                start=hitStart.position; end=hitEnd.position;
                var path=new NavMeshPath();
                Require(NavMesh.CalculatePath(start,end,NavMesh.AllAreas,path)&&
                    path.status==NavMeshPathStatus.PathComplete,
                    "Existing NavMesh cannot reach expanded forest");
                return new{status="CORNBERG_OUTER_WOODLAND_OK",
                    perimeterTrees=trees.Length,originalDiagonalOpen=true,
                    seamsRemainingActive=seam.Count(t=>t.parent.gameObject.activeSelf),
                    physicalEdges=wall.childCount,eastForestNavMesh=path.status.ToString()};
            }
            finally{n.Remove();if(opened)EditorSceneManager.CloseScene(s,true);}
        }
    }
}
