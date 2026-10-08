using System;
using System.Collections.Generic;
using System.Linq;
using DiceFree.World;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;

namespace DiceFree.EditorTools
{
    /// <summary>Separate map shell for the level-10 Novice mountain advancement location.</summary>
    public static class MountainTrialMapAuthoring
    {
        public const string TrialScene = "Assets/_DiceFree/Scenes/NoviceMountainTrial.unity";
        public const string NavAsset = "Assets/_DiceFree/Settings/NoviceMountainTrialNavMesh.asset";

        [MenuItem("DiceFree/World/Author separate Novice Mountain trial")]
        [CliCommand("dicefree.mountain.author-trial",
            "Create separate advancement trial map scene and independent NavMesh.",
            Tags = new[] { "art", "world", "advancement" })]
        public static object Author()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject("Novice Mountain Trial - separate map");
            var center = new Vector3(10000f, 0f, 10000f);

            var platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
            platform.name = "Trial floor - navigable stone"; platform.layer = 8;
            platform.transform.SetParent(root.transform, false);
            platform.transform.position = center + new Vector3(0, -0.40f, 0);
            platform.transform.localScale = new Vector3(46, .8f, 46);
            platform.GetComponent<Renderer>().sharedMaterial = Material("Trial floor",new Color(.31f,.33f,.35f));

            for (int i=0;i<22;i++)
            {
                float a=i*Mathf.PI*2f/22;
                var p=center+new Vector3(Mathf.Cos(a)*25,2.9f,Mathf.Sin(a)*25);
                var wall=GameObject.CreatePrimitive(PrimitiveType.Cube);
                wall.name="Trial cavern cliff";
                wall.transform.SetParent(root.transform,false);
                wall.transform.position=p;
                wall.transform.rotation=Quaternion.Euler(4,i*360f/22,22f);
                wall.transform.localScale=new Vector3(9f,7f,6.2f);
                wall.layer=9;
                wall.GetComponent<Renderer>().sharedMaterial=Material("Trial cliff",new Color(.24f,.26f,.32f));
            }
            Shrine(root.transform,center+new Vector3(-5,0,8),"Physically Blessed",new Color(.92f,.54f,.22f));
            Shrine(root.transform,center+new Vector3(5,0,8),"Magically Touched",new Color(.35f,.67f,.99f));

            var entrance = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            entrance.name = "Return arch - approach (press E)";
            entrance.transform.SetParent(root.transform,false);
            entrance.transform.position=center+new Vector3(0,.06f,-14);
            entrance.transform.localScale=new Vector3(1.6f,.06f,1.6f);
            entrance.layer=2;
            UnityEngine.Object.DestroyImmediate(entrance.GetComponent<Collider>());
            entrance.GetComponent<Renderer>().sharedMaterial=Material("Trial return",new Color(.27f,.77f,.63f));

            var light=new GameObject("Mountain cavern illumination").AddComponent<Light>();
            light.transform.SetParent(root.transform,false);
            light.type=LightType.Directional;light.transform.rotation=Quaternion.Euler(50,-30,0);
            light.intensity=1.30f;
            var point =new GameObject("Trial altar light").AddComponent<Light>();
            point.transform.SetParent(root.transform,false);
            point.transform.position=center+Vector3.up*7;
            point.range=38;point.intensity=7;point.type=LightType.Point;
            point.color=new Color(.78f,.80f,1f);

            // Separate map has its own navigation. Cornberg remains loaded as session source.
            // NavMeshBuilder samples physics geometry, not merely renderer transforms.
            // Flush newly positioned/scaled primitives before collecting collider sources.
            Physics.SyncTransforms();
            var sources=new List<NavMeshBuildSource>();
            var bounds=new Bounds(center+Vector3.up*4,new Vector3(68,48,68));
            NavMeshBuilder.CollectSources(bounds,(1<<8)|(1<<9),
                NavMeshCollectGeometry.PhysicsColliders,0,
                new List<NavMeshBuildMarkup>(),sources);
            var settings=NavMesh.GetSettingsByID(0);
            settings.agentRadius=.50f;
            settings.agentHeight=1.8f;
            settings.agentClimb=.4f;
            var data=NavMeshBuilder.BuildNavMeshData(settings,sources,bounds,Vector3.zero,Quaternion.identity);
            if(data==null)throw new InvalidOperationException("Mountain trial NavMesh bake failed.");
            var saved=AssetDatabase.LoadAssetAtPath<NavMeshData>(NavAsset);
            if(saved==null){AssetDatabase.CreateAsset(data,NavAsset);saved=data;}
            else{EditorUtility.CopySerialized(data,saved);UnityEngine.Object.DestroyImmediate(data);}
            root.AddComponent<WorldNavigation>().Configure(saved);
            EditorUtility.SetDirty(saved);

            if(!EditorSceneManager.SaveScene(scene,TrialScene))
                throw new InvalidOperationException("Mountain Trial scene did not save.");
            var entries=EditorBuildSettings.scenes.ToList();
            if (!entries.Any(e=>e.path==TrialScene))
            {
                entries.Add(new EditorBuildSettingsScene(TrialScene,true));
                EditorBuildSettings.scenes=entries.ToArray();
            }
            AssetDatabase.SaveAssets();
            Debug.Log("DICEFREE_MOUNTAIN_TRIAL_MAP_OK: standalone scene, shrine and separate NavMesh.");
            return new{success=true,marker="DICEFREE_MOUNTAIN_TRIAL_MAP_OK",scene=TrialScene};
        }

        private static void Shrine(Transform root,Vector3 loc,string name,Color color)
        {
            var pedestal=GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pedestal.transform.SetParent(root,false);
            pedestal.name=name+" altar";
            pedestal.transform.position=loc+Vector3.up*.55f;
            pedestal.transform.localScale=new Vector3(1.5f,.55f,1.5f);
            pedestal.layer=9;
            pedestal.GetComponent<Renderer>().sharedMaterial=Material(name+" stone",new Color(.37f,.34f,.32f));
            var crystal=GameObject.CreatePrimitive(PrimitiveType.Cube);
            crystal.transform.SetParent(root,false);
            crystal.name=name+" Way crystal";
            crystal.transform.position=loc+Vector3.up*2.1f;
            crystal.transform.rotation=Quaternion.Euler(35,45,35);
            crystal.transform.localScale=new Vector3(.78f,1.5f,.78f);
            crystal.layer=2;
            UnityEngine.Object.DestroyImmediate(crystal.GetComponent<Collider>());
            crystal.GetComponent<Renderer>().sharedMaterial=Material(name+" glow",color,true);
            var label=new GameObject(name+" label").AddComponent<TextMesh>();
            label.transform.SetParent(root,false);
            label.transform.position=loc+new Vector3(0,3.1f,0);
            label.transform.rotation=Quaternion.Euler(45,180,0);
            label.text=name;
            label.characterSize=.12f;label.fontSize=48;
            label.anchor=TextAnchor.MiddleCenter;
        }

        private static Material Material(string name,Color color,bool emission=false)
        {
            const string root="Assets/_DiceFree/Materials/Blockout/";
            var path=root+"Mountain "+name+".mat";
            var material=AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material==null)
            {
                material=new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(material,path);
            }
            material.SetColor("_BaseColor",color);
            if (emission)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor",color*2f);
            }
            EditorUtility.SetDirty(material);
            return material;
        }
    }
}
