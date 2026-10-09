using System;
using System.Collections.Generic;
using System.Linq;
using DiceFree.Combat;
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
    // Applies ONLY to the saved Cornberg scene; never regenerates the starter village.
    public static class CornbergExpansionAuthoring
    {
        const string ScenePath = "Assets/_DiceFree/Scenes/Cornberg.unity";
        const string FieldRoot = "05 - Corn fields and pasture";
        const string MarkerName = "Northern fields relocation v1";
        const string EasternRoot = "10 - Eastern Slime Forest extension";
        const string SlimeId = "enemy.crop-slime";
        const string RoadId = "enemy.road-slime";
        const string EliteId = "enemy.forest-elite-slime";
        static readonly Vector3 SouthShift = new Vector3(-35f, 0, 69f);
        static readonly Vector3 NorthShift = new Vector3(-11f, 0, 42f);
        static readonly Vector2[] RoadSpawns = {
            new(92,21), new(102,30), new(112,40), new(118,53),
            new(97,73), new(107,92), new(128,94), new(121,23)
        };

        static T[] All<T>(Scene s) where T:Component =>
            s.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<T>(true)).ToArray();
        static void Check(bool condition,string reason) {
            if(!condition) throw new InvalidOperationException("COR NBERG EXPANSION: "+reason);
        }
        static Transform Named(Scene scene,string name) =>
            All<Transform>(scene).FirstOrDefault(t=>t.name==name);
        static GameObject Group(string name, Transform parent) {
            var t=parent.Find(name);
            if(t!=null) return t.gameObject;
            var g=new GameObject(name);g.transform.SetParent(parent,false);return g;
        }
        static float Dist(Vector3 a, Vector3 b) {
            var dx=a.x-b.x;var dz=a.z-b.z;return Mathf.Sqrt(dx*dx+dz*dz);
        }
        static Vector3 Walkable(Vector3 want,float max=6) {
            if(!NavMesh.SamplePosition(want,out var found,max,NavMesh.AllAreas))
                throw new InvalidOperationException("Not on baked NavMesh: "+want);
            return found.position;
        }
        static NavMeshDataInstance Navigation(Scene scene) {
            var nav=All<WorldNavigation>(scene).FirstOrDefault();
            Check(nav!=null&&nav.Data!=null,"World navigation data missing");
            return NavMesh.AddNavMeshData(nav.Data);
        }

        [CliCommand("dicefree.cornberg-expansion.install","Move northern crop fields; expand east forest/slimes; compass and zoom.")]
        public static object Install() {
            Check(!EditorApplication.isPlayingOrWillChangePlaymode,"Stop Play Mode first");
            var scene=SceneManager.GetSceneByPath(ScenePath);
            bool opened=!scene.isLoaded;
            if(opened) scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Additive);
            int relocated=0,roadCount=0,treeCount=0;
            var nav=Navigation(scene);
            try {
                var root=Named(scene,"Cornberg - World 1 starter pocket");
                Check(root!=null,"Existing Cornberg world root missing");
                var fields=Named(scene,FieldRoot);
                Check(fields!=null,"Existing farm plots not found");
                // The initial northern-meadow pass was already saved into this scene. Preserve it.
                bool previouslyInstalled = fields.Find("Northern field west - original south plot")!=null &&
                    fields.Find("Northern field east - original north plot")!=null &&
                    Named(scene,"Cornberg eastern forest expansion v1")!=null;
                if(previouslyInstalled) {
                    foreach(var camera in All<ExplorationCamera>(scene))camera.ConfigureZoom(8,110,.60f);
                    EditorSceneManager.MarkSceneDirty(scene);
                    Check(EditorSceneManager.SaveScene(scene),"Couldn't save current Cornberg layout");
                    return new{alreadyInstalled=true,northernFields=2,localRoadSlimes=Named(scene,"Eight local road slimes").childCount,
                        easternTrees=Named(scene,"Seed 5409 - eastern trees").childCount};
                }
                if(fields.Find(MarkerName)==null) {
                    var pieces=Enumerable.Range(0,fields.childCount).Select(i=>fields.GetChild(i)).ToArray();
                    Check(pieces.Count(t=>t.name=="Tilled field")==2,"Expected two intact original farm plots");
                    foreach(var piece in pieces) {
                        Vector3 shift=Mathf.Abs(piece.position.z+12f)<Mathf.Abs(piece.position.z-15f) ? SouthShift:NorthShift;
                        piece.position+=shift;
                        if(PrefabUtility.IsPartOfPrefabInstance(piece))
                            PrefabUtility.RecordPrefabInstancePropertyModifications(piece);
                        relocated++;
                    }
                    Group(MarkerName,fields);
                }
                var crops=All<ActorStats>(scene).Where(a=>a.Definition!=null&&a.Definition.stableId==SlimeId).ToArray();
                Check(crops.Length==5,"Expected exactly five existing crop Slimes; got "+crops.Length);
                foreach(var actor in crops) {
                    if(actor.transform.position.z>=46) continue;
                    // Each old crop actor follows its original field plot (Z=-12 or Z=15).
                    var old=actor.transform.position;
                    var shift=Mathf.Abs(old.z+12f)<Mathf.Abs(old.z-15f)?SouthShift:NorthShift;
                    actor.transform.position=Walkable(old+shift,7);
                    if(PrefabUtility.IsPartOfPrefabInstance(actor.transform))
                        PrefabUtility.RecordPrefabInstancePropertyModifications(actor.transform);
                    relocated++;
                }
                var elite=All<ActorStats>(scene).SingleOrDefault(a=>a.Definition?.stableId==EliteId);
                Check(elite!=null,"Existing level-8 elite slime missing");
                var cave=Named(scene,"Dark sealed entrance");
                Check(cave!=null,"Original Slime Dungeon cave missing");
                if(Dist(elite.transform.position,cave.position)<23) {
                    elite.transform.position=Walkable(new Vector3(116,0,76),7);
                    if(PrefabUtility.IsPartOfPrefabInstance(elite.transform))
                        PrefabUtility.RecordPrefabInstancePropertyModifications(elite.transform);
                }
                var eastern=Group(EasternRoot,root);
                var treeRoot=Group("Forest trees",eastern.transform);
                var roadRoot=Group("Local road slimes",eastern.transform);
                var roadTemplate=All<ActorStats>(scene).FirstOrDefault(a=>a.Definition!=null&&a.Definition.stableId==RoadId);
                Check(roadTemplate!=null,"Existing road slime template missing");
                for(int i=0;i<RoadSpawns.Length;i++) {
                    var name="Eastern Road Slime - "+(i+1);
                    if(roadRoot.transform.Find(name)!=null)continue;
                    var p=RoadSpawns[i];
                    var world=Walkable(new Vector3(p.x,0,p.y),7);
                    Check(Dist(world,cave.position)>19&&Dist(world,elite.transform.position)>10,
                        "New road slime would be too near cave/elite: "+name);
                    var clone=Object.Instantiate(roadTemplate.gameObject,world,roadTemplate.transform.rotation,roadRoot.transform);
                    clone.name=name;
                    roadCount++;
                }
                var sourceTrees=All<Transform>(scene).Where(t=>t.name.StartsWith("Trees/Tree_",StringComparison.Ordinal)&&t.GetComponentsInChildren<Renderer>(true).Length>0).GroupBy(t=>t.name).Select(g=>g.First()).ToArray();
                Check(sourceTrees.Length>=4,"Not enough existing tree variants to reuse");
                var occupied=All<Transform>(scene).Where(t=>t.name.StartsWith("Trees/Tree_",StringComparison.Ordinal)||t.name.StartsWith("Eastern forest tree",StringComparison.Ordinal)).Select(t=>t.position).ToList();
                // New forest grows EAST, with clear fighting lanes along the road.
                var random=new System.Random(80124);
                int index=0;
                for(int x=94;x<=142;x+=8)
                for(int z=10;z<=100;z+=11) {
                    index++;
                    var name="Eastern forest tree "+index.ToString("D3");
                    if(treeRoot.transform.Find(name)!=null)continue;
                    float px=x+(float)(random.NextDouble()-.5)*4;
                    float pz=z+(float)(random.NextDouble()-.5)*4;
                    var proposed=new Vector3(px,0,pz);
                    if(Dist(proposed,cave.position)<22||Dist(proposed,elite.transform.position)<11)continue;
                    if(RoadSpawns.Any(p=>Dist(proposed,new Vector3(p.x,0,p.y))<7))continue;
                    if(occupied.Any(p=>Dist(proposed,p)<5.8f))continue;
                    if(!NavMesh.SamplePosition(proposed,out var hit,3,NavMesh.AllAreas))continue;
                    var template=sourceTrees[(index+random.Next(sourceTrees.Length))%sourceTrees.Length];
                    var clone=Object.Instantiate(template.gameObject,hit.position,template.rotation,treeRoot.transform);
                    clone.name=name;
                    treeCount++;occupied.Add(hit.position);
                }
                foreach(var camera in All<ExplorationCamera>(scene))camera.ConfigureZoom(8,110,.60f);
                EditorSceneManager.MarkSceneDirty(scene);
                Check(EditorSceneManager.SaveScene(scene),"Couldn't save Cornberg scene");
                AssetDatabase.SaveAssets();
                return new{marker=MarkerName,fieldObjectsRelocated=relocated,cropSlimes=crops.Length,newRoadSlimes=roadCount,newTrees=treeCount,elite=elite.transform.position.ToString(),cave=cave.position.ToString()};
            }
            finally {nav.Remove();if(opened)EditorSceneManager.CloseScene(scene,true);}
        }

        [CliCommand("dicefree.cornberg-expansion.validate","Validate north field, east enemies, forest, compass and cave clearance.")]
        public static object Validate() {
            Check(!EditorApplication.isPlayingOrWillChangePlaymode,"Stop Play Mode first");
            var s=SceneManager.GetSceneByPath(ScenePath);
            bool opened=!s.isLoaded;
            if(opened)s=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Additive);
            var nav=Navigation(s);
            try {
                var fields=Named(s,FieldRoot);
                Check(fields!=null&&(fields.Find(MarkerName)!=null ||
                    fields.Find("Northern field west - original south plot")!=null),"Farm relocation marker missing");
                var plots=fields.GetComponentsInChildren<Transform>(true).Where(t=>t.name=="Tilled field").ToArray();
                Check(plots.Length==2,"Missing farm plots");
                Check(plots.All(t=>t.position.z>=49&&t.position.z<=64&&t.position.x>=9&&t.position.x<=46),"Field plot outside northern clearing");
                var crops=All<ActorStats>(s).Where(a=>a.Definition?.stableId==SlimeId).ToArray();
                Check(crops.Length==5&&crops.All(c=>c.transform.position.z>45&&c.transform.position.x<56),"Crop slime left inside Cornberg");
                var roadRoot=Named(s,"Eight local road slimes") ?? Named(s,EasternRoot)?.Find("Local road slimes");
                Check(roadRoot!=null&&roadRoot.childCount>=6,"Missing local eastern road-slime encounters");
                Check(roadRoot.Cast<Transform>().All(t=>t.GetComponent<ActorStats>()?.Definition?.stableId==RoadId),"Wrong eastern enemy definitions");
                var elite=All<ActorStats>(s).Single(a=>a.Definition?.stableId==EliteId).transform;
                var cave=Named(s,"Dark sealed entrance");
                Check(Dist(elite.position,cave.position)>23,"Elite still blocks dungeon cave");
                var trees=Named(s,"Seed 5409 - eastern trees") ?? Named(s,EasternRoot)?.Find("Forest trees");
                Check(trees!=null&&trees.childCount>=6,"Eastern forest extension not installed");
                var quest=AssetDatabase.FindAssets("t:QuestDefinition",new[]{"Assets/_DiceFree/Settings/Progression"})
                    .Select(g=>AssetDatabase.LoadAssetAtPath<DiceFree.Quests.QuestDefinition>(AssetDatabase.GUIDToAssetPath(g)))
                    .FirstOrDefault(q=>q!=null&&q.stableId=="quest.cornberg.crop-slimes");
                Check(quest!=null&&quest.offer.ToLowerInvariant().Contains("north")&&quest.locationHint.ToLowerInvariant().Contains("north"),
                    "Farmer no longer points north");
                foreach(var cam in All<ExplorationCamera>(s))
                    Check(Mathf.Approximately(new SerializedObject(cam).FindProperty("zoomSensitivity").floatValue,.60f),"Zoom not set to 0.60");
                foreach(var crop in crops)Check(NavMesh.SamplePosition(crop.transform.position,out _,4,NavMesh.AllAreas),"Crop off navmesh: "+crop.name);
                return new{result="CORNBERG_EXPANSION_DATA_OK",fieldCenters=plots.Select(t=>t.position.ToString()).ToArray(),cropSlimes=crops.Length,easternRoadSlimes=roadRoot.childCount,easternTrees=trees.childCount,elite=elite.position.ToString(),cave=cave.position.ToString()};
            }
            finally{nav.Remove();if(opened)EditorSceneManager.CloseScene(s,true);}
        }
    }
}
