using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using DiceFree.Characters;
using DiceFree.Core;
using DiceFree.UI;
using DiceFree.World;
using static DiceFree.EditorTools.BlockoutShapes;

namespace DiceFree.EditorTools
{
    public static class CornbergSceneBuilder
    {
        public const string ScenePath = Root + "/Scenes/Cornberg.unity";
        public const string NavigationPath = Root + "/Settings/CornbergNavMesh.asset";

        [MenuItem("DiceFree/Blockout/Recreate Cornberg (replaces scene)")]
        public static void Recreate()
        {
            if (!Application.isBatchMode && !EditorUtility.DisplayDialog("Recreate Cornberg?",
                "This replaces the authored Cornberg scene with its initial blockout. Save manual changes elsewhere first.", "Recreate", "Cancel")) return;
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Create();
        }

        public static void Create()
        {
            Initialize(); ConfigureLayers();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var world = Group("Cornberg - World 1 starter pocket");
            CornbergLandscape.Create(world); CornbergVillage.Create(world); CornbergLandscape.Forest(world);
            Lighting();
            var navigation = new GameObject("World navigation").AddComponent<WorldNavigation>();
            Bake(navigation);
            var player = new GameObject("Echo - traversal placeholder");
            player.transform.position = CornbergLandscape.Ground(-42,-30);
            var agent = player.AddComponent<NavMeshAgent>();
            agent.enabled=false;
            agent.radius=0.45f; agent.height=1.8f; agent.baseOffset=0; agent.areaMask=1;
            player.AddComponent<TraversalMotor>();
            Shape("Echo body",PrimitiveType.Capsule,player.transform.position+Vector3.up*0.9f,
                new Vector3(0.7f,0.9f,0.7f),Glow,player.transform,false);
            Shape("Facing",PrimitiveType.Sphere,player.transform.position+new Vector3(0,1.2f,0.4f),
                Vector3.one*0.2f,Dark,player.transform,false);
            var camera = new GameObject("Exploration camera",typeof(Camera),typeof(AudioListener)).GetComponent<Camera>();
            camera.tag="MainCamera"; camera.fieldOfView=55; camera.nearClipPlane=0.2f; camera.farClipPlane=1500;
            camera.transform.rotation=Quaternion.Euler(42,45,0);
            camera.transform.position=player.transform.position+Vector3.up*1.5f-camera.transform.forward*34;
            camera.GetUniversalAdditionalCameraData().renderPostProcessing=true;
            var follow=camera.gameObject.AddComponent<ExplorationCamera>(); follow.Configure(player.transform);
            var marker=Shape("Move destination",PrimitiveType.Cylinder,Vector3.zero,new Vector3(0.8f,0.035f,0.8f),Glow,null,false);
            marker.SetActive(false);
            var input=player.AddComponent<TraversalInput>(); input.Configure(camera,marker.transform);
            new GameObject("Traversal controls").AddComponent<TraversalOverlay>().Configure(input,follow);
            EditorSceneManager.SaveScene(scene,ScenePath);
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};
            AssetDatabase.SaveAssets();
            Debug.Log("CORNBERG_BUILD_OK");
        }

        [MenuItem("DiceFree/Blockout/Rebake current Cornberg navigation")]
        public static void Rebake()
        {
            if(EditorSceneManager.GetActiveScene().path!=ScenePath) throw new System.InvalidOperationException("Open Cornberg before rebaking.");
            var navigation=Object.FindFirstObjectByType<WorldNavigation>();
            if(navigation==null) throw new System.InvalidOperationException("Missing World navigation object.");
            Bake(navigation); EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene()); AssetDatabase.SaveAssets();
        }

        private static void Bake(WorldNavigation navigation)
        {
            Physics.SyncTransforms();
            var sources=new List<NavMeshBuildSource>(); var obstacles=new List<NavMeshBuildSource>();
            var bounds=new Bounds(new Vector3(35,15,14),new Vector3(220,100,155));
            NavMeshBuilder.CollectSources(bounds,1<<8,NavMeshCollectGeometry.PhysicsColliders,0,new List<NavMeshBuildMarkup>(),sources);
            NavMeshBuilder.CollectSources(bounds,1<<9,NavMeshCollectGeometry.PhysicsColliders,1,new List<NavMeshBuildMarkup>(),obstacles);
            sources.AddRange(obstacles);
            var settings=NavMesh.GetSettingsByID(0); settings.agentRadius=0.5f; settings.agentHeight=1.8f;
            settings.agentClimb=0.4f; settings.agentSlope=40; settings.overrideVoxelSize=true; settings.voxelSize=0.12f;
            var data=NavMeshBuilder.BuildNavMeshData(settings,sources,bounds,Vector3.zero,Quaternion.identity);
            if(data==null) throw new System.Exception("Navigation bake failed.");
            var saved=AssetDatabase.LoadAssetAtPath<NavMeshData>(NavigationPath);
            if(saved==null) { AssetDatabase.CreateAsset(data,NavigationPath); saved=data; }
            else { EditorUtility.CopySerialized(data,saved); Object.DestroyImmediate(data); }
            navigation.Configure(saved); EditorUtility.SetDirty(navigation); EditorUtility.SetDirty(saved);
        }

        private static void ConfigureLayers()
        {
            var manager=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var layers=manager.FindProperty("layers");
            foreach(var entry in new[]{(8,"Walkable"),(9,"WorldSolid")})
            {
                var value=layers.GetArrayElementAtIndex(entry.Item1);
                if(!string.IsNullOrEmpty(value.stringValue)&&value.stringValue!=entry.Item2)
                    throw new System.InvalidOperationException("Layer already assigned: "+entry.Item1);
                value.stringValue=entry.Item2;
            }
            manager.ApplyModifiedProperties();
        }

        private static void Lighting()
        {
            var sun=new GameObject("Warm afternoon sun").AddComponent<Light>();
            sun.type=LightType.Directional; sun.intensity=1.8f; sun.color=new Color(1,0.94f,0.79f);
            sun.shadows=LightShadows.Soft; sun.transform.rotation=Quaternion.Euler(45,-35,0);
            RenderSettings.sun=sun; RenderSettings.ambientMode=AmbientMode.Trilight;
            RenderSettings.ambientSkyColor=new Color(0.63f,0.77f,0.87f);
            RenderSettings.ambientEquatorColor=new Color(0.48f,0.59f,0.45f);
            RenderSettings.ambientGroundColor=new Color(0.2f,0.25f,0.17f);
            RenderSettings.fog=true; RenderSettings.fogMode=FogMode.Linear;
            RenderSettings.fogColor=new Color(0.67f,0.8f,0.83f); RenderSettings.fogStartDistance=190; RenderSettings.fogEndDistance=850;
            var skyPath=Root+"/Materials/Blockout/Cornberg sky.mat";
            var sky=AssetDatabase.LoadAssetAtPath<Material>(skyPath);
            if(sky==null) {sky=new Material(Shader.Find("Skybox/Procedural")); AssetDatabase.CreateAsset(sky,skyPath);}
            sky.SetFloat("_AtmosphereThickness",0.8f); sky.SetFloat("_Exposure",1.15f);
            RenderSettings.skybox=sky;
        }
    }
}
