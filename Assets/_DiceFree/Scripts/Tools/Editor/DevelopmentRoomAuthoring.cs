using System;
using System.Collections.Generic;
using System.Linq;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Core;
using DiceFree.Development;
using DiceFree.Persistence;
using DiceFree.Visuals;
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
    public static class DevelopmentRoomAuthoring
    {
        const string Data = "Assets/_DiceFree/Settings/Development";
        [CliCommand("dicefree.development.author","Create the main-menu development room and World 2 transition test scene.")]
        public static object Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play Mode first.");
            if (EditorSceneManager.GetSceneManagerSetup().Any(value => SceneManager.GetSceneByPath(value.path).isDirty))
                throw new InvalidOperationException("An open scene has unsaved changes.");
            Ensure(Data);
            var sourceScene = EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/Cornberg.unity");
            var sourcePlayer = Object.FindAnyObjectByType<TraversalInput>();
            var sourceCamera = Object.FindAnyObjectByType<ExplorationCamera>();
            if (sourcePlayer == null || sourceCamera == null) throw new InvalidOperationException("Cornberg player/camera missing.");
            var first = BuildMap("DevelopmentRoom",Vector3.zero,false,out var session);
            CreateSession(session,sourcePlayer,sourceCamera,first);
            Save(first.gameObject.scene,"DevelopmentRoom");
            var second = BuildMap("World2TransitionTest",new Vector3(40000,0,40000),true,out _);
            Save(second.gameObject.scene,"World2TransitionTest");
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/Cornberg.unity");
            return new { marker="DICEFREE_DEVELOPMENT_ROOM_AUTHORED",developmentScene=true,world2TestScene=true };
        }

        [CliCommand("dicefree.development.build-windows","Build the complete current scene list including the development room and World 2 transition test.")]
        public static object BuildWindows()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play Mode first.");
            var scenes=EditorBuildSettings.scenes.Where(value=>value.enabled).Select(value=>value.path).ToArray();
            if(!scenes.Contains("Assets/_DiceFree/Scenes/DevelopmentRoom.unity") ||
                !scenes.Contains("Assets/_DiceFree/Scenes/World2TransitionTest.unity") ||
                scenes[0]!="Assets/_DiceFree/Scenes/StartMenu.unity")
                throw new InvalidOperationException("Foundation build scene list is incomplete.");
            BuildWarningRepairs.Run();
            string directory=System.IO.Path.GetFullPath("Builds/World1Foundation");
            System.IO.Directory.CreateDirectory(directory);
            string executable=System.IO.Path.Combine(directory,"DiceFree.exe");
            bool previousDevelopment = EditorUserBuildSettings.development;
            UnityEditor.Build.Reporting.BuildReport report;
            try
            {
                // Pipeline's build processor consults this toggle for custom scripted builds.
                EditorUserBuildSettings.development = true;
                report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                    scenes=scenes,locationPathName=executable,target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development
                });
            }
            finally { EditorUserBuildSettings.development = previousDevelopment; }
            var result=new{marker="DICEFREE_WORLD1_FOUNDATION_BUILD",result=report.summary.result.ToString(),
                errors=report.summary.totalErrors,warnings=report.summary.totalWarnings,
                sceneCount=scenes.Length,executable,bytes=report.summary.totalSize};
            System.IO.File.WriteAllText(System.IO.Path.Combine(directory,"build-report.json"),JsonUtility.ToJson(
                new BuildRecord{result=result.result,errors=result.errors,warnings=result.warnings,sceneCount=scenes.Length,executable=executable},true));
            if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)
                throw new InvalidOperationException("Foundation Windows build failed: "+report.summary.result);
            return result;
        }
        [Serializable] private sealed class BuildRecord{
            public string result,executable;
            public int errors,warnings,sceneCount;
        }

        static DevelopmentMap BuildMap(string name,Vector3 origin,bool second,out GameObject session)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            var root = new GameObject(name + " map");
            var map = root.AddComponent<DevelopmentMap>();
            map.worldId = second ? "world.test.2" : "world.test.1";
            map.spawn = origin + new Vector3(0,0,-8);
            map.bounds = new Rect(origin.x-26,origin.z-20,52,40);
            var floor = Material(name+"Floor",second?new Color(.20f,.14f,.30f):new Color(.14f,.23f,.29f));
            var trim = Material(name+"Trim",second?new Color(.64f,.39f,.91f):new Color(.24f,.68f,.78f));
            Cube("Test floor",root.transform,origin+Vector3.down*.25f,new Vector3(48,.5f,34),floor,8);
            Cube("North wall",root.transform,origin+new Vector3(0,1.5f,17),new Vector3(48,3,.6f),trim,9);
            Cube("South wall",root.transform,origin+new Vector3(0,1.5f,-17),new Vector3(48,3,.6f),trim,9);
            Cube("West wall",root.transform,origin+new Vector3(-24,1.5f,0),new Vector3(.6f,3,34),trim,9);
            Cube("East wall",root.transform,origin+new Vector3(24,1.5f,0),new Vector3(.6f,3,34),trim,9);
            for(int x=-20;x<=20;x+=5) Cube("Floor grid",root.transform,origin+new Vector3(x,.012f,0),new Vector3(.025f,.02f,32),trim,2);
            for(int z=-15;z<=15;z+=5) Cube("Floor grid",root.transform,origin+new Vector3(0,.012f,z),new Vector3(46,.02f,.025f),trim,2);
            var stone = Cube("Test revive stone",root.transform,map.spawn+new Vector3(-3,.65f,0),new Vector3(.7f,1.3f,.7f),trim,11);
            map.anchor = stone.AddComponent<ResurrectionAnchor>();
            map.anchor.Configure(second?"anchor.dev-world2":"anchor.dev-room");
            Label(root.transform,origin+new Vector3(0,3,12),second?"WORLD 2 / TRANSITION TEST":"WORLD 1 / DEVELOPMENT ROOM");
            Label(root.transform,origin+new Vector3(-8,2,7),"TRAINING TARGETS");
            for(int i=0;i<3;i++) Dummy(root.transform,origin+new Vector3(-12+i*4,0,3),trim);
            var portal = Cube(second?"Return portal":"World 2 test portal",root.transform,origin+new Vector3(17,1.5f,7),new Vector3(2.2f,3,.6f),trim,11);
            var travel = portal.AddComponent<DevelopmentPortal>();
            travel.targetScene = second?"DevelopmentRoom":"World2TransitionTest";
            travel.ConfigureName(second?"Return to World 1 test room":"World 2 transition test");
            Label(root.transform,portal.transform.position+Vector3.up*2,second?"RETURN TO WORLD 1":"WORLD 2 PORTAL");
            var light = new GameObject("Test map sun").AddComponent<Light>();
            light.transform.SetParent(root.transform);light.type=LightType.Directional;light.intensity=1.1f;
            light.transform.rotation=Quaternion.Euler(55,-35,0);
            if (!second)
            {
                Label(root.transform,origin+new Vector3(13,3,-6),"GREATBLADE VISUAL PREVIEW");
                for(int i=0;i<2;i++)
                {
                    var preview=new GameObject(i==0?"Greatblade player preview":"Retired swordswoman preview");
                    preview.transform.SetParent(root.transform);preview.transform.position=origin+new Vector3(11+i*4,0,-6);
                    preview.AddComponent<GreatbladePrototypeVisual>();
                }
            }
            Bake(root,new Bounds(origin,new Vector3(55,12,43)),name);
            session = null;
            if (!second)
            {
                session = new GameObject("Development session");
                session.SetActive(false);
                var bootstrap=new GameObject("Development room bootstrap").AddComponent<DevelopmentRoomBootstrap>();
                bootstrap.sessionTemplate=session;
            }
            return map;
        }

        static void CreateSession(GameObject session,TraversalInput sourcePlayer,ExplorationCamera sourceCamera,DevelopmentMap map)
        {
            var player = Object.Instantiate(sourcePlayer.gameObject,session.transform);
            player.name = "Development Echo";
            player.transform.position=map.spawn;
            var camera = Object.Instantiate(sourceCamera.gameObject,session.transform);
            var exploration = camera.GetComponent<ExplorationCamera>();
            exploration.Configure(player.transform);
            exploration.ConfigureMapBounds(map.bounds);
            var marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            marker.name="Test move destination";marker.transform.SetParent(session.transform);
            marker.transform.localScale=new Vector3(.5f,.035f,.5f);marker.SetActive(false);
            Object.DestroyImmediate(marker.GetComponent<Collider>());
            player.GetComponent<TraversalInput>().Configure(camera.GetComponent<Camera>(),marker.transform);
            player.GetComponent<RespawnAtAnchor>().Configure(map.anchor.transform);
            player.GetComponent<RespawnAtAnchor>().ConfigureRegisteredAnchors(map.anchor);
            // Remove references to unrelated Cornberg scene objects while retaining
            // asset data and all references inside the cloned player/camera rig.
            foreach(var component in player.GetComponentsInChildren<Component>(true).Concat(camera.GetComponentsInChildren<Component>(true)))
            {
                if(component==null)continue;
                var serialized=new SerializedObject(component);var property=serialized.GetIterator();
                while(property.Next(true))
                {
                    if(property.propertyType!=SerializedPropertyType.ObjectReference)continue;
                    var referenced=property.objectReferenceValue;
                    if(referenced is Component value && value.gameObject.scene==sourcePlayer.gameObject.scene)
                        property.objectReferenceValue=null;
                    else if(referenced is GameObject obj && obj.scene==sourcePlayer.gameObject.scene)
                        property.objectReferenceValue=null;
                }
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
            // Configure again after external-reference cleanup.
            exploration.Configure(player.transform);
            player.GetComponent<TraversalInput>().Configure(camera.GetComponent<Camera>(),marker.transform);
            player.GetComponent<RespawnAtAnchor>().Configure(map.anchor.transform);
            player.GetComponent<RespawnAtAnchor>().ConfigureRegisteredAnchors(map.anchor);
            var fogObject=new GameObject("Development fog");fogObject.transform.SetParent(session.transform);
            var fog=fogObject.AddComponent<WorldFogOfWar>();
            fog.Configure(player.transform,camera.GetComponent<Camera>(),new Vector2(map.bounds.xMin,map.bounds.yMin),
                new Vector2(map.bounds.xMax,map.bounds.yMax),28);
            var controls=session.AddComponent<DevelopmentRoomController>();
            controls.player=player.GetComponent<CombatActor>();controls.explorationCamera=exploration;controls.fog=fog;
        }

        static void Dummy(Transform parent,Vector3 at,Material mat)
        {
            string path=Data+"/Practice target.asset";
            var stats=AssetDatabase.LoadAssetAtPath<ActorDefinition>(path);
            if(stats==null){stats=ScriptableObject.CreateInstance<ActorDefinition>();AssetDatabase.CreateAsset(stats,path);}
            stats.stableId="dev.practice-target";stats.displayName="Training Target";stats.baseHp=10000;stats.experienceReward=0;
            stats.tuning=AssetDatabase.LoadAssetAtPath<CombatTuning>("Assets/_DiceFree/Settings/Combat/Cornberg provisional tuning.asset");
            EditorUtility.SetDirty(stats);
            var go=new GameObject("Training target");go.layer=10;go.transform.SetParent(parent);go.transform.position=at;
            var agent=go.AddComponent<NavMeshAgent>();agent.radius=.5f;agent.height=2;
            go.AddComponent<TraversalMotor>();go.AddComponent<ActorStats>().Configure(stats);go.AddComponent<Health>();
            go.AddComponent<CombatActor>().Configure(1,.5f,"dev.target");
            go.AddComponent<CombatStatusController>();
            var collider=go.AddComponent<CapsuleCollider>();collider.radius=.5f;collider.height=2;collider.center=Vector3.up;
            var visual=Cube("Target body",go.transform,at+Vector3.up,new Vector3(.8f,2,.8f),mat,2);
            Object.DestroyImmediate(visual.GetComponent<Collider>());
            go.AddComponent<DiceFree.UI.ActorFeedback>().Configure(visual.transform);
        }

        static void Ensure(string path){if(AssetDatabase.IsValidFolder(path))return;string parent=System.IO.Path.GetDirectoryName(path).Replace('\\','/');Ensure(parent);AssetDatabase.CreateFolder(parent,System.IO.Path.GetFileName(path));}
        static Material Material(string name,Color color){var path=Data+"/"+name+".mat";var value=AssetDatabase.LoadAssetAtPath<Material>(path);if(value==null){value=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(value,path);}value.color=color;EditorUtility.SetDirty(value);return value;}
        static GameObject Cube(string name,Transform parent,Vector3 position,Vector3 size,Material material,int layer){var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.layer=layer;go.transform.SetParent(parent);go.transform.position=position;go.transform.localScale=size;go.GetComponent<Renderer>().sharedMaterial=material;return go;}
        static void Label(Transform parent,Vector3 at,string text){var label=new GameObject("Test sign").AddComponent<TextMesh>();label.transform.SetParent(parent);label.transform.position=at;label.transform.rotation=Quaternion.Euler(50,180,0);label.text=text;label.fontSize=36;label.characterSize=.1f;label.anchor=TextAnchor.MiddleCenter;label.gameObject.AddComponent<DevelopmentSign>();}
        static void Bake(GameObject root,Bounds bounds,string name){Physics.SyncTransforms();var sources=new List<NavMeshBuildSource>();NavMeshBuilder.CollectSources(root.transform,(1<<8)|(1<<9),NavMeshCollectGeometry.PhysicsColliders,0,new List<NavMeshBuildMarkup>(),sources);var settings=NavMesh.GetSettingsByID(0);settings.agentRadius=.675f;settings.agentHeight=1.8f;var data=NavMeshBuilder.BuildNavMeshData(settings,sources,bounds,Vector3.zero,Quaternion.identity);if(data==null)throw new InvalidOperationException("Test map navigation bake failed.");string path=Data+"/"+name+"NavMesh.asset";var saved=AssetDatabase.LoadAssetAtPath<NavMeshData>(path);if(saved==null){AssetDatabase.CreateAsset(data,path);saved=data;}else{EditorUtility.CopySerialized(data,saved);Object.DestroyImmediate(data);}root.AddComponent<WorldNavigation>().Configure(saved);EditorUtility.SetDirty(saved);}
        static void Save(Scene scene,string name){string path="Assets/_DiceFree/Scenes/"+name+".unity";if(!EditorSceneManager.SaveScene(scene,path))throw new InvalidOperationException("Test scene save failed.");var scenes=EditorBuildSettings.scenes.ToList();if(!scenes.Any(value=>value.path==path))scenes.Add(new EditorBuildSettingsScene(path,true));EditorBuildSettings.scenes=scenes.ToArray();}
    }
}
