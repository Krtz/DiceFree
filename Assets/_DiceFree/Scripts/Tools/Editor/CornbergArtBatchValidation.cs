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

namespace DiceFree.EditorTools
{
    public static class CornbergArtBatchValidation
    {
        [CliCommand("dicefree.cornberg.art-validate",
            "Validate real Blender models, opaque/transparent materials, populated Cornberg, mountain map and navigation.",
            Tags = new[] { "tests", "world", "art", "cornberg" })]
        public static object Validate()
        {
            var cornberg=EditorSceneManager.OpenScene(CornbergSceneBuilder.ScenePath);
            Require(cornberg.IsValid(), "Cornberg scene missing.");
            var root=GameObject.Find("Cornberg 3D Art Batch");
            Require(root!=null, "3D art batch was not saved in Cornberg.");

            var oldTrees=UnityEngine.Object.FindObjectsByType<Transform>(
                FindObjectsInactive.Include,FindObjectsSortMode.None)
                .Where(t=>t.name=="Woodland tree").ToArray();
            int treeReplacements=oldTrees.Count(t=>t.GetComponentsInChildren<MeshRenderer>(true)
                    .Any(r=>r.gameObject.name.EndsWith("_mesh")));
            Require(treeReplacements>=300,
                "Most blockout trees were not replaced with Blender meshes.");

            var newForest=GameObject.Find("Expanded northeast slime woodland");
            Require(newForest!=null && newForest.transform.childCount>=100,
                "Expanded northeast forest does not contain 3D trees.");
            var mountain=GameObject.Find("Novice Advancement Mountain - 3D gate");
            Require(mountain!=null, "Level-10 mountain 3D model missing.");
            var gate=mountain.GetComponent<MountainAdvancementGate>();
            Require(gate!=null, "Level-10 mountain opening controller missing.");
            Require(mountain.GetComponentsInChildren<Transform>(true)
                .Any(t=>t.name=="DoorLeft") &&
                mountain.GetComponentsInChildren<Transform>(true).Any(t=>t.name=="DoorRight"),
                "Mountain needs individually animatable door leaves.");

            var player=UnityEngine.Object.FindAnyObjectByType<TraversalInput>();
            Require(player!=null && player.GetComponent<MountainTrialTraveller>()!=null,
                "Player mountain-map travel/session component missing.");
            Require(player.GetComponent<DiceFree.Persistence.ManifestationPersistence>()!=null,
                "Existing Echo save ownership lost.");
            Require(UnityEngine.Object.FindObjectsByType<QuestGiver>(
                FindObjectsInactive.Include,FindObjectsSortMode.None).Length>0,
                "Existing quest NPC/quest progression disappeared.");

            var npcs=GameObject.Find("NPC role models - visual kit");
            Require(npcs!=null && npcs.transform.childCount>=9,
                "Expected generic farmer, banker, merchant, alchemist and blacksmith models.");

            var actors=UnityEngine.Object.FindObjectsByType<ActorStats>(
                FindObjectsInactive.Include,FindObjectsSortMode.None)
                .Where(a=>a.Definition!=null && a.Definition.stableId.Contains("slime")).ToArray();
            int crops=actors.Count(a=>a.Definition.stableId=="enemy.crop-slime");
            Require(crops>=5, "Expected crops in both Cornberg fields.");
            Require(actors.Length>=30, "Expanded slime forest is under-populated.");
            int modelledSlimes = actors.Count(a=>a.GetComponentsInChildren<MeshRenderer>(true)
                .Any(r=>r.gameObject.name.Contains("Gel", StringComparison.OrdinalIgnoreCase)));
            Require(modelledSlimes>=27,
                "Slime 3D visuals under count " + modelledSlimes + "/" + actors.Length +
                ": renderers=" + string.Join(",", actors.Take(3)
                    .SelectMany(a=>a.GetComponentsInChildren<Renderer>(true))
                    .Select(r=>r.gameObject.name).Distinct().Take(18)));

            var slimeMat=AssetDatabase.LoadAssetAtPath<Material>(
                "Assets/_DiceFree/Materials/CornbergBatch/Gel_0.mat");
            Require(slimeMat!=null && slimeMat.HasProperty("_Surface") &&
                slimeMat.GetFloat("_Surface")>0.5f &&
                slimeMat.GetColor("_BaseColor").a<0.7f &&
                slimeMat.renderQueue>=3000,
                "Slime gel is not a transparent URP runtime material.");

            var land=GameObject.Find("Cornberg valley");
            var mesh=land==null?null:land.GetComponent<MeshFilter>()?.sharedMesh;
            Require(mesh!=null && mesh.bounds.max.x>240 && mesh.bounds.max.z>180,
                "World surface not expanded to the northeast.");

            var nav=UnityEngine.Object.FindAnyObjectByType<WorldNavigation>();
            Require(nav!=null && nav.Data!=null,"Expanded Cornberg navigation missing.");
            var navInstance=NavMesh.AddNavMeshData(nav.Data);
            try
            {
                Require(NavMesh.SamplePosition(new Vector3(170,0,105),out _,3f,1),
                    "New forest travel corridor is not navigable.");
            }
            finally {if(navInstance.valid)navInstance.Remove();}

            var map=AssetDatabase.LoadAssetAtPath<SceneAsset>(MountainTrialMapAuthoring.TrialScene);
            var trialNav=AssetDatabase.LoadAssetAtPath<NavMeshData>(MountainTrialMapAuthoring.NavAsset);
            Require(map!=null && trialNav!=null,"Separate advancement map/NavMesh missing.");
            var trialInstance = NavMesh.AddNavMeshData(trialNav);
            try
            {
                Require(NavMesh.SamplePosition(new Vector3(10000,0.2f,10000),
                    out _,3f,NavMesh.AllAreas),
                    "Trial scene NavMesh has no navigable entry at the authored spawn.");
            }
            finally {if(trialInstance.valid)trialInstance.Remove();}
            Require(EditorBuildSettings.scenes.Any(s=>s.path==MountainTrialMapAuthoring.TrialScene && s.enabled),
                "Advancement map is not listed in build scenes.");

            Debug.Log("DICEFREE_CORNBERG_ART_OK: Blender source models, 3D tree replacement, " +
                "slime population/transparent gel, NPC kit, mountain gate, separate trial and expanded navigation passed.");
            return new{success=true, marker="DICEFREE_CORNBERG_ART_OK",
                treeReplacements, extraTrees=newForest.transform.childCount,
                slimes=actors.Length, crops, npcs=npcs.transform.childCount,
                trialMap=MountainTrialMapAuthoring.TrialScene};
        }

        private static void Require(bool result,string message)
        {
            if(!result)throw new InvalidOperationException(message);
        }
    }
}
