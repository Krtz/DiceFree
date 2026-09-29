using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using DiceFree.Characters;
using DiceFree.Core;
using DiceFree.World;

namespace DiceFree.EditorTools
{
    [InitializeOnLoad]
    public static class CornbergValidation
    {
        private const string Running = "DiceFree.CornbergValidation";
        private static readonly Vector3[] Stops = {
            new(-17,0,-9), new(1,0,0), new(-3,0,9), new(13,0,11), new(17,0,-18),
            new(36,0,-32), new(47,0,-10), new(64,0,34), new(81,0,55), new(95,0,34),
            new(86,0,-3), new(110,0,37), new(-42,3,-30)
        };
        private static TraversalMotor motor;
        private static int stage;
        private static float deadline, started;
        private static Vector3 origin;
        private static Keyboard keyboard;
        private static Mouse mouse;
        private static bool commandSent;
        // Visible ground below the quest tracker even in Unity's 640x480 batch Game View.
        private static readonly Vector3 ClickDestination = new(5,0,-8);

        static CornbergValidation()
        {
            EditorApplication.playModeStateChanged += OnPlayState;
        }

        [MenuItem("DiceFree/Validation/Check Cornberg navigation")]
        public static void ValidateNavigation()
        {
            EditorSceneManager.OpenScene(CornbergSceneBuilder.ScenePath);
            var world = UnityEngine.Object.FindFirstObjectByType<WorldNavigation>();
            Require(world != null && world.Data != null, "Missing navigation asset.");
            var instance = NavMesh.AddNavMeshData(world.Data);
            try
            {
                var start = Sample(new Vector3(-42,3,-30));
                foreach (var stop in Stops)
                {
                    var path = new NavMeshPath();
                    Require(NavMesh.CalculatePath(start,Sample(stop),1,path) && path.status == NavMeshPathStatus.PathComplete,
                        "Disconnected destination: " + stop);
                }
                Require(NavMesh.Raycast(Sample(new Vector3(-12,0,35)), new Vector3(-24,0,35), out _, 1), "Stream is traversable without a bridge.");
                Require(!NavMesh.SamplePosition(new Vector3(17,0,-13),out _,0.3f,1), "General store footprint is walkable.");
                var beyond = new Vector3(126,0,65);
                if (NavMesh.SamplePosition(beyond,out var hit,2,1))
                {
                    var path = new NavMeshPath(); NavMesh.CalculatePath(start,hit.position,1,path);
                    Require(path.status != NavMeshPathStatus.PathComplete,"Development boundary can be bypassed.");
                }
                foreach(var root in EditorSceneManager.GetActiveScene().GetRootGameObjects())
                foreach(var transform in root.GetComponentsInChildren<Transform>(true))
                    Require(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject)==0,"Missing script: "+transform.name);
                Debug.Log("CORNBERG_NAVIGATION_OK: 13 destinations, return to mountain, stream, building and development boundary.");
            }
            finally { instance.Remove(); }
        }

        public static void RunAll()
        {
            try
            {
                ValidateNavigation();
                SessionState.SetBool("DiceFree.DisablePersistence", true);
                SessionState.SetBool(Running,true);
                EditorApplication.EnterPlaymode();
            }
            catch(Exception exception) { Debug.LogException(exception); EditorApplication.Exit(1); }
        }

        public static void BuildAndRun()
        {
            CornbergSceneBuilder.Create();
            RunAll();
        }

        public static void RunControls()
        {
            SessionState.SetBool("DiceFree.ControlsOnly",true);
            RunAll();
        }

        private static void OnPlayState(PlayModeStateChange state)
        {
            if(!SessionState.GetBool(Running,false)) return;
            if(state==PlayModeStateChange.EnteredPlayMode)
            {
                motor=UnityEngine.Object.FindFirstObjectByType<TraversalInput>().GetComponent<TraversalMotor>();
                // Traversal is an isolated regression pass; combat has a separate active-encounter suite.
                foreach (var enemy in UnityEngine.Object.FindObjectsByType<DiceFree.AI.AggroBehaviour>(FindObjectsSortMode.None))
                    enemy.gameObject.SetActive(false);
                stage=SessionState.GetBool("DiceFree.ControlsOnly",false) ? Stops.Length : 0;
                SessionState.SetBool("DiceFree.ControlsOnly",false);
                commandSent=false; deadline=Time.realtimeSinceStartup+30;
                InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
                InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
                keyboard=InputSystem.AddDevice<Keyboard>();
                Time.timeScale=3;
                EditorApplication.update+=Tick;
                Application.logMessageReceived+=OnLog;
            }
        }

        private static void Tick()
        {
            try
            {
                if(!EditorApplication.isPlaying) return;
                Require(Time.realtimeSinceStartup<deadline,"Traversal timed out at stage "+stage);
                if(!motor.Ready) return;
                if(stage<Stops.Length)
                {
                    if(!commandSent)
                    {
                        Require(motor.MoveTo(Stops[stage]),"Motor rejected route "+stage);
                        commandSent=true;
                    }
                    if(Vector3.Distance(motor.transform.position,Sample(Stops[stage]))<0.6f)
                    {
                        Debug.Log("CORNBERG_ROUTE_OK "+stage);
                        if(stage==1) Capture("village",Camera.main);
                        if(stage==8) Capture("dungeon",Camera.main);
                        stage++; commandSent=false; deadline=Time.realtimeSinceStartup+30;
                    }
                    return;
                }
                if(stage==Stops.Length)
                {
                    motor.Stop();
                    Require(!motor.MoveTo(new Vector3(17,0,-13)),"Motor accepted a building interior.");
                    // Exercise real Input System bindings, not a second movement implementation.
                    InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.F6));
                    started=Time.time; origin=motor.transform.position; stage++;
                    return;
                }
                if(stage==Stops.Length+1)
                {
                    var controls=motor.GetComponent<TraversalInput>();
                    if(controls.Mode!=TraversalInput.ControlMode.Direct) return;
                    InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.W,Key.D));
                    if(Time.time-started<0.7f) return;
                    Require(Vector3.Distance(origin,motor.transform.position)>1,"WASD input did not move the shared motor.");
                    Require(Vector3.Distance(origin,motor.transform.position)<=motor.Speed*(Time.time-started)+0.5f,"Diagonal movement exceeded motor speed.");
                    InputSystem.QueueStateEvent(keyboard,new KeyboardState());
                    motor.Stop();
                    motor.GetComponent<NavMeshAgent>().Warp(Sample(new Vector3(1,0,0)));
                    InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.F6));
                    started=Time.time; stage++;
                    return;
                }
                if(stage==Stops.Length+2)
                {
                    if(Time.time-started<1) return;
                    Require(motor.GetComponent<TraversalInput>().Mode==TraversalInput.ControlMode.Classic,"F6 did not restore Classic mode.");
                    InputSystem.QueueStateEvent(keyboard,new KeyboardState());
                    mouse=InputSystem.AddDevice<Mouse>();
                    var point=Camera.main.WorldToScreenPoint(ClickDestination);
                    Require(!DiceFree.UI.HudPointerBlocker.Covers(new Vector2(point.x,point.y)),"Traversal test click is obscured by HUD.");
                    InputSystem.QueueStateEvent(mouse,new MouseState { position=new Vector2(point.x,point.y), buttons=2 });
                    stage++; started=Time.time;
                    return;
                }
                if(stage==Stops.Length+3)
                {
                    if(Time.time-started<0.3f) return;
                    Require(motor.Travelling || Vector3.Distance(motor.transform.position,Sample(ClickDestination))<0.6f,
                        "Right-click did not issue a path through the Input System.");
                    if(motor.Travelling) return;
                    motor.GetComponent<NavMeshAgent>().Warp(Sample(new Vector3(-12,0,35)));
                    origin=motor.transform.position;
                    motor.MoveDirect(Vector3.left,5f);
                    Require(Vector3.Distance(origin,motor.GetComponent<NavMeshAgent>().nextPosition)<8,"Direct movement crossed the stream.");
                    CaptureOverview();
                    Debug.Log("CORNBERG_PLAYMODE_OK: routes, click/WASD bindings, mode switching, diagonal speed, stream collision, invalid command rejection.");
                    Finish(0);
                }
            }
            catch(Exception exception) { Debug.LogException(exception); Finish(1); }
        }

        private static void OnLog(string message,string trace,LogType type)
        {
            if(type!=LogType.Exception && type!=LogType.Error) return;
            // Unity 6000.6.3f1 can race its editor-only search index on batch startup.
            // Record this specific engine fault without treating it as a gameplay failure.
            if (Application.isBatchMode && trace.Contains("UnityEditor.Search.SearchInit.IndexationOnStartup") &&
                trace.Contains("UnityEditor.Search.SearchDatabase"))
            {
                Debug.LogWarning("CORNBERG_EDITOR_SEARCH_WARNING: " + message);
                return;
            }
            Directory.CreateDirectory("Logs/Cornberg");
            File.AppendAllText("Logs/Cornberg/test-errors.txt",message+"\n"+trace+"\n");
            Finish(1);
        }
        private static void Finish(int code)
        {
            EditorApplication.update-=Tick; Application.logMessageReceived-=OnLog;
            SessionState.SetBool(Running,false); Time.timeScale=1;
            if(keyboard!=null) InputSystem.RemoveDevice(keyboard);
            if(mouse!=null) InputSystem.RemoveDevice(mouse);
            EditorApplication.Exit(code);
        }
        private static Vector3 Sample(Vector3 point)
        {
            Require(NavMesh.SamplePosition(point,out var hit,5,1),"No walkable ground near "+point);
            return hit.position;
        }
        private static void Require(bool condition,string message)
        {
            if(!condition) throw new InvalidOperationException(message);
        }

        private static void CaptureOverview()
        {
            var camera=Camera.main;
            camera.GetComponent<ExplorationCamera>().enabled=false;
            camera.transform.rotation=Quaternion.Euler(42,45,0);
            camera.transform.position=new Vector3(1,1.5f,0)-camera.transform.forward*34;
            Capture("village",camera);
            camera.transform.position=new Vector3(81,1.5f,55)-camera.transform.forward*34;
            Capture("dungeon",camera);
            camera.transform.position=new Vector3(-70,145,-115);
            camera.transform.LookAt(new Vector3(34,0,10));
            camera.fieldOfView=55;
            Capture("overview",camera);
            camera.transform.position=new Vector3(-25,12,-25);
            camera.transform.rotation=Quaternion.Euler(10,45,0);
            camera.fieldOfView=65;
            Capture("great-tree-vista",camera);
        }
        internal static void Capture(string name,Camera camera)
        {
            Directory.CreateDirectory("Logs/Cornberg");
            var previous=RenderTexture.active;
            var texture=RenderTexture.GetTemporary(1600,1000,24);
            RenderPipeline.SubmitRenderRequest(camera,new UniversalRenderPipeline.SingleCameraRequest { destination=texture });
            RenderTexture.active=texture;
            var image=new Texture2D(1600,1000,TextureFormat.RGB24,false);
            image.ReadPixels(new Rect(0,0,1600,1000),0,0); image.Apply();
            File.WriteAllBytes("Logs/Cornberg/"+name+".png",image.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(image); camera.targetTexture=null; RenderTexture.active=previous;
            RenderTexture.ReleaseTemporary(texture);
        }
    }
}
