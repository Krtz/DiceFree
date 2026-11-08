using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DiceFree.AI;
using DiceFree.Briarback;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Core;
using DiceFree.Persistence;
using DiceFree.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;

public static class WorldVisualReview
{
    private static readonly (string name, Vector3 point)[] Sites =
    {
        ("trimery-south-gate", new Vector3(248,0,150)),
        ("trimery-sawmill", new Vector3(239,0,186.5f)),
        ("trimery-mill-yard", new Vector3(225,0,181)),
        ("trimery-square", new Vector3(249,0,177)),
        ("trimery-west-highway", new Vector3(215,0,180)),
        ("trimery-north-gate", new Vector3(248,0,204)),
        ("cornberg-green", new Vector3(0,0,1)),
        ("cornberg-bridge", new Vector3(-13,0,-9)),
        ("cornberg-forge", new Vector3(0,0,10))
    };

    public static string Start()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode || SceneManager.GetActiveScene().isDirty)
            throw new InvalidOperationException("Clean Edit Mode required.");
        var guard=AppDomain.CurrentDomain.GetAssemblies()
            .Select(a=>a.GetType("DiceFree.EditorTools.PersistenceTestGuard")).First(t=>t!=null);
        guard.GetMethod("UseIsolatedSaveRootForNextPlay").Invoke(null,new object[]{
            Path.Combine(Path.GetTempPath(),"DiceFree-WorldVisual-"+Guid.NewGuid())});
        EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/Cornberg.unity");
        EditorApplication.EnterPlaymode();
        return "Started isolated World 1 visual review.";
    }

    public static string Run(string label)
    {
        _=CaptureAndVerify(label);
        return "Review running. Read Logs/WorldVisual-"+label+".json.";
    }

    private static async Task CaptureAndVerify(string label)
    {
        string directory="VisualBaselines/World1Visual/"+label;
        string report="Logs/WorldVisual-"+label+".json";
        var errors=new List<string>();
        var captures=new List<string>();
        int checks=0;
        float meters=0;
        string phase="startup";
        void Observe(string message,string stack,LogType type)
        {
            if(type is LogType.Error or LogType.Exception or LogType.Assert)errors.Add(message);
        }
        void Need(bool condition,string message){checks++;if(!condition)throw new InvalidOperationException(message);}
        void Record(string status,string error=null)
        {
            File.WriteAllText(report,Newtonsoft.Json.JsonConvert.SerializeObject(new {status,phase,checks,meters,captures,errors,error},Newtonsoft.Json.Formatting.Indented));
        }
        Application.logMessageReceived+=Observe;
        object gizmoValue=null;
        PropertyInfo gizmos=null;
        Object view=null;
        try
        {
            Directory.CreateDirectory(directory);
            Record("running");
            var player=Object.FindAnyObjectByType<TraversalInput>()?.GetComponent<CombatActor>();
            await Until(()=>player!=null&&player.Motor.Ready&&player.GetComponent<ManifestationPersistence>().Ready,25,
                "Player is ready.");
            var viewType=typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");
            view=Resources.FindObjectsOfTypeAll(viewType).FirstOrDefault();
            gizmos=viewType.GetProperty("drawGizmos",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);
            if(view!=null&&gizmos?.CanWrite==true){gizmoValue=gizmos.GetValue(view);gizmos.SetValue(view,false);}
            player.Health.Invulnerable=true;
            player.GetComponent<TraversalInput>().enabled=false;
            player.GetComponent<CombatInput>().enabled=false;
            foreach(var behavior in Object.FindObjectsByType<MonoBehaviour>())
                if(behavior is AggroBehaviour or BasicAttack or QuilboarCombat or BriarbackChieftainEncounter)
                    behavior.enabled=false;
            var camera=Object.FindAnyObjectByType<ExplorationCamera>();
            var clock=Object.FindAnyObjectByType<WorldDayNightCycle>();
            clock.SetHour(11,false);
            GraphicsQualityRuntime.Select(VisualQuality.High,false);

            Vector3 Ground(Vector3 requested)
            {
                Need(NavMesh.SamplePosition(requested,out var hit,2f,-1),"No NavMesh at "+requested);
                return hit.position;
            }
            void Place(Vector3 requested)
            {
                Need(player.Motor.Teleport(Ground(requested)),"Cannot place hero at "+requested);
                Physics.SyncTransforms();
                camera.RecenterForTravel(player.transform);
            }
            async Task Shot(string name)
            {
                await Task.Delay(520);
                string path=directory+"/"+name+".png";
                if(File.Exists(path))File.Delete(path);
                ScreenCapture.CaptureScreenshot(path);
                await Until(()=>File.Exists(path)&&new FileInfo(path).Length>50000,8,"Capture "+name);
                captures.Add(path);
                Record("running");
            }

            phase="daylight from gameplay camera";
            foreach(var site in Sites)
            {
                Place(site.point);
                await Shot(site.name);
            }
            phase="evening and low graphics";
            clock.SetHour(19.5f,false);
            Place(Sites[0].point);
            await Shot("trimery-gate-evening");
            Place(Sites[1].point);
            await Shot("trimery-mill-evening");
            clock.SetHour(11,false);
            Place(Sites.Single(site=>site.name=="cornberg-green").point);
            await Shot("cornberg-green-evening");
            clock.SetHour(11,false);
            GraphicsQualityRuntime.Select(VisualQuality.Low,false);
            await Shot("cornberg-green-low");
            Place(Sites[1].point);
            await Shot("trimery-sawmill-low");
            GraphicsQualityRuntime.Select(VisualQuality.High,false);

            if(label!="before")
            {
                phase="physical walk and services";
                var dressing=GameObject.Find("Trimery handcrafted presentation");
                Need(dressing!=null,"Trimery visual root present");
                Need(dressing.GetComponentsInChildren<Collider>().Length==0 &&
                    dressing.GetComponentsInChildren<InteractionTarget>().Length==0,
                    "Trimery dressing has no colliders or quest interactions");
                Need(dressing.GetComponentsInChildren<Renderer>().All(r=>r.sharedMaterials.All(m=>m!=null&&m.shader.isSupported)),
                    "Every Trimery material is supported.");
                var wheel=GameObject.Find("Hand-cranked sawmill flywheel");
                Need(wheel!=null&&wheel.GetComponent<Animation>().isPlaying,"Sawmill wheel animation is playing.");
                var wheelRotation=wheel.transform.localRotation;
                await Task.Delay(300);
                Need(Quaternion.Angle(wheelRotation,wheel.transform.localRotation)>2,"Sawmill wheel rotates at runtime.");
                var green=GameObject.Find("Cornberg handcrafted village green");
                Need(green!=null&&green.GetComponentsInChildren<Collider>().Length==0,"Cornberg decorative meshes have no colliders.");
                var roads=dressing.GetComponentsInChildren<MeshFilter>().Concat(green.GetComponentsInChildren<MeshFilter>())
                    .Where(f=>f.name.Contains("packed earth -",StringComparison.Ordinal)).ToArray();
                Need(roads.Length==7&&roads.All(f=>f.sharedMesh.uv.Length==f.sharedMesh.vertexCount&&
                    f.GetComponent<Renderer>().sharedMaterial.GetTexture("_BaseMap")!=null),
                    "All seven road strips retain their geometry and have mapped earth grain.");
                var water=green.GetComponentsInChildren<Renderer>().Where(r=>r.sharedMaterials.Any(m=>m.shader.name=="DiceFree/Cornberg Water")).ToArray();
                Need(water.Length==2&&water.All(r=>ShaderUtil.GetShaderMessages(r.sharedMaterial.shader).All(m=>m.severity.ToString()!="Error")),
                    "The well and river use a supported compiled water shader.");
                var firs=GameObject.Find("Trimery edge - layered firs");
                Need(firs!=null&&firs.transform.childCount>=6&&firs.GetComponentsInChildren<TreeCameraOccluder>().Length==firs.transform.childCount,
                    "Each fir keeps an independent camera visibility test.");
                var wolfMaterials=AssetDatabase.FindAssets("t:Material",new[]{"Assets/_DiceFree/Settings/Ironpine"})
                    .Select(AssetDatabase.GUIDToAssetPath).Select(AssetDatabase.LoadAssetAtPath<Material>)
                    .Where(m=>m!=null&&m.name.StartsWith("Art_GreyWolf_",StringComparison.Ordinal)).ToArray();
                Need(wolfMaterials.Length>=24&&wolfMaterials.All(m=>m.shader.name.StartsWith("Universal Render Pipeline/")),
                    "All three grey wolf palettes use URP materials.");
                Place(new Vector3(0,0,1));
                player.Health.Restore(.18f);
                float wounded=player.Health.Current;
                var well=Object.FindObjectsByType<HealingArea>().Single(a=>a.name=="Cornberg magical well healing area");
                Need(well.Contains(player),"The existing Cornberg well effect still reaches the hero.");
                await Until(()=>player.Health.Current>wounded+3,4,"Cornberg well heals in Play Mode.");
                Place(new Vector3(248,0,145));
                Time.timeScale=3;
                foreach(var requested in new[]{
                    new Vector3(248,0,157),new Vector3(248,0,177),new Vector3(240,0,186.5f),
                    new Vector3(248,0,183),new Vector3(225,0,180),new Vector3(215,0,180),
                    new Vector3(233,0,180),new Vector3(248,0,201),new Vector3(248,0,211)})
                {
                    var goal=Ground(requested);
                    var path=new NavMeshPath();
                    Need(NavMesh.CalculatePath(player.transform.position,goal,-1,path)&&path.status==NavMeshPathStatus.PathComplete,
                        "Complete route to "+goal);
                    Need(player.Motor.MoveTo(goal),"Walk toward "+goal);
                    var before=player.transform.position;
                    await Until(()=>Vector3.Distance(player.transform.position,goal)<1.05f,30,"Walk to "+goal);
                    meters+=Vector3.Distance(before,player.transform.position);
                    player.Motor.Stop();
                }
                Need(player.GetComponent<RespawnAtAnchor>().AnchorId=="anchor.trimery","Trimery stone still activates on foot.");
                phase="both Cornberg bridges";
                async Task Cross(Vector3 start,Vector3 finish)
                {
                    Place(start);
                    var destination=Ground(finish);
                    var path=new NavMeshPath();
                    Need(NavMesh.CalculatePath(player.transform.position,destination,-1,path)&&path.status==NavMeshPathStatus.PathComplete,
                        "Complete Cornberg crossing at "+start.z);
                    Need(player.Motor.MoveTo(destination),"Cross Cornberg bridge at "+start.z);
                    var from=player.transform.position;
                    await Until(()=>Vector3.Distance(player.transform.position,destination)<1.05f,20,"Reach the far bridge bank.");
                    meters+=Vector3.Distance(from,player.transform.position);
                    player.Motor.Stop();
                }
                await Cross(new Vector3(-24,0,-9),new Vector3(-14,0,-9));
                await Cross(new Vector3(-22,0,26),new Vector3(-11,0,26));
            }
            Need(errors.Count==0,"Unity errors: "+string.Join(" | ",errors));
            phase="complete";
            Record("passed");
        }
        catch(Exception error){Record("failed",error.ToString());Debug.LogError(error);}
        finally
        {
            Time.timeScale=1;
            Application.logMessageReceived-=Observe;
            if(gizmoValue!=null&&view!=null)gizmos.SetValue(view,gizmoValue);
        }
    }

    private static async Task Until(Func<bool> condition,int seconds,string message)
    {
        var deadline=DateTime.UtcNow.AddSeconds(seconds);
        while(!condition())
        {
            if(DateTime.UtcNow>=deadline)throw new TimeoutException(message);
            await Task.Delay(25);
        }
    }
}
