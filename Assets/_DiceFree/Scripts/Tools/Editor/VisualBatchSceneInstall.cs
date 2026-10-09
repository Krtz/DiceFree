using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Pipeline.Commands;

namespace DiceFree.EditorTools
{
    internal static class VisualBatchSceneInstall
    {
        const string ScenePath = "Assets/_DiceFree/Scenes/Cornberg.unity";
        const string ModelRoot = "Assets/_DiceFree/Art/World/VisualBatch01/Models/";
        const string GroupName = "VisualOverhaulBatch01_Decoration";
        static GameObject Import(string file, Vector3 position, float scale, float yaw, Transform parent)
        {
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelRoot+file+".fbx");
            if (model == null) throw new InvalidOperationException("Missing FBX: " + file);
            var go = (GameObject)PrefabUtility.InstantiatePrefab(model);
            go.name = "VIS01_"+file;
            go.transform.SetParent(parent, true);
            go.transform.position = position;
            go.transform.rotation = Quaternion.Euler(0f,yaw,0f);
            go.transform.localScale = Vector3.one * scale;
            foreach (var c in go.GetComponentsInChildren<Collider>(true)) UnityEngine.Object.DestroyImmediate(c);
            return go;
        }
        [CliCommand("dicefree.visual01.cornberg.install","Install nonblocking decorative Blender visual batch in Cornberg without affecting quest entities.")]
        public static object Install()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Requires Edit mode.");
            for (int i=0;i<SceneManager.sceneCount;i++) if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Save open scene changes before visual install.");
            var setup = EditorSceneManager.GetSceneManagerSetup();
            int count=0;
            try
            {
                var scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
                var existing=scene.GetRootGameObjects().FirstOrDefault(g=>g.name==GroupName);
                if(existing!=null) return new { success=true, alreadyInstalled=true, count=existing.transform.childCount };
                var group=new GameObject(GroupName);
                SceneManager.MoveGameObjectToScene(group,scene);
                // All placements decorative only; existing navmesh and colliders remain authoritative.
                var specs = new (string name,float x,float z,float scale,float yaw)[] {
                    ("crate_01",-10,7,0.9f,12),("crate_02",-8,7.5f,0.72f,32),
                    ("barrel_01",-12,9,0.95f,0),("barrel_03",-12.9f,9.5f,0.7f,0),
                    ("lantern_01",-7,9,0.72f,15),("sign_01",-5,12,1,20),
                    ("hay_01",23,49,1,0),("hay_02",25,49,1.10f,24),
                    ("cart_01",15,48,0.95f,72),("fence_01",16,51,1,0),
                    ("fence_02",18,51,1,0),("fence_03",20,51,1,0),
                    ("wheat_01",19,56,1.4f,0),("wheat_02",21,56,1.35f,18),
                    ("wheat_03",23,56,1.25f,41),("wheat_04",40,56,1.35f,0),
                    ("wheat_05",43,56,1.30f,25),
                    ("rock_01",69,46,1.35f,30),("rock_02",73,42,0.92f,9),
                    ("stump_01",84,42,1.0f,16),("log_01",87,43,1.15f,30),
                    ("mushroom_01",89,44,0.85f,0),("mushroom_02",91,44,0.85f,0),
                    ("tree_01",94,81,1.35f,22),("tree_03",99,83,1.25f,70),
                    ("pine_01",105,84,1.4f,20),("pine_02",112,83,1.25f,54),
                    ("rock_04",104,82,1.20f,45),("log_03",109,84,1.2f,23)
                };
                foreach(var v in specs) {Import(v.name,new Vector3(v.x,0,v.z),v.scale,v.yaw,group.transform);count++;}
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                return new {success=true,placed=count,scene=ScenePath,decorative=true};
            }
            finally {EditorSceneManager.RestoreSceneManagerSetup(setup);}
        }
        [CliCommand("dicefree.visual01.cornberg.validate","Count nonblocking Blender dressing assets in Cornberg.")]
        public static object Validate()
        {
            var setup=EditorSceneManager.GetSceneManagerSetup();
            try {
                var s=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
                var root=s.GetRootGameObjects().FirstOrDefault(g=>g.name==GroupName);
                if(root==null) throw new InvalidOperationException("Visual batch absent");
                int colliders=root.GetComponentsInChildren<Collider>(true).Length;
                if(colliders!=0 || root.transform.childCount!=29) throw new InvalidOperationException("Unexpected visual batch: count="+root.transform.childCount+", colliders="+colliders);
                return new {success=true,count=root.transform.childCount,colliders,marker="VISUAL01_CORNBERG_OK"};
            } finally {EditorSceneManager.RestoreSceneManagerSetup(setup);}
        }
    }
}
