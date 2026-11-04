using System;
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
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using Object=UnityEngine.Object;

// Walks the dressed approaches with the real traversal motor in a disposable save.
// Opponent AI is frozen for this visual/navigation review; this is not a combat test.
public static class BriarbackGroundingReview
{
    public static string Start()
    {
        var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        Need(!EditorApplication.isPlayingOrWillChangePlaymode && !scene.isDirty && scene.name=="Cornberg",
            "Open clean Cornberg in Edit Mode.");
        var guard=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("DiceFree.EditorTools.PersistenceTestGuard")).First(t=>t!=null);
        guard.GetMethod("UseIsolatedSaveRootForNextPlay").Invoke(null,new object[]{
            Path.Combine(Path.GetTempPath(),"DiceFree-GroundingReview-"+Guid.NewGuid())});
        EditorApplication.EnterPlaymode();
        return "Started isolated grounding review.";
    }

    public static async Task<object> Run()
    {
        Need(Application.isPlaying,"Isolated Play Mode required.");
        CombatActor actor=null;
        await Until(()=>(actor=Object.FindAnyObjectByType<TraversalInput>()?.GetComponent<CombatActor>())!=null &&
            actor.GetComponent<ManifestationPersistence>().Ready && actor.Motor.Ready,30,"Player initialization");
        var camera=Object.FindAnyObjectByType<ExplorationCamera>();
        var output=Path.Combine(Directory.GetParent(Application.dataPath).FullName,"VisualBaselines","BriarbackGrounding");
        Directory.CreateDirectory(output);
        var frozen=Object.FindObjectsByType<MonoBehaviour>().Where(b=>b.enabled &&
            (b is AggroBehaviour || b is BasicAttack || b is QuilboarCombat)).ToArray();
        var oldPosition=actor.transform.position;
        bool oldInvulnerable=actor.Health.Invulnerable;
        var navType=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("UnityEditor.AI.NavMeshVisualizationSettings")).First(t=>t!=null);
        var nav=navType.GetProperty("showNavMesh",BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic);
        var oldNav=nav.GetValue(null);
        var gameViewType=typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");
        var gameView=Resources.FindObjectsOfTypeAll(gameViewType).FirstOrDefault();
        var gizmos=gameViewType.GetProperty("drawGizmos",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);
        var oldGizmos=gameView!=null ? gizmos.GetValue(gameView) : null;
        float walked=0;
        int routes=0;
        int startFrame=Time.frameCount;
        try
        {
            foreach(var behaviour in frozen) behaviour.enabled=false;
            foreach(var enemy in CombatActor.All.Where(a=>a!=actor && a.Motor!=null && a.Motor.Ready)) enemy.Motor.Stop();
            actor.Health.Invulnerable=true;
            nav.SetValue(null,false);
            if(gameView!=null) gizmos.SetValue(gameView,false);
            foreach(var element in new[]{"Fire","Earth","Lightning","Ice"})
            {
                var site=GameObject.Find(element+" Shaman settlement - provisional").transform;
                Need(site.Find("Elemental clearing ground and trail dressing").GetComponentsInChildren<Collider>().Length==0,
                    "Ground dressing has collision at "+element);
                Need(actor.Motor.Teleport(Point(site.position+new Vector3(0,0,-16))),"Approach setup at "+element);
                camera.RecenterForTravel(actor.transform);
                foreach(var offset in new[]{new Vector3(2,0,-7),new Vector3(2,0,1),new Vector3(7,0,-5)})
                {
                    var target=Point(site.position+offset);
                    Need(actor.Motor.MoveTo(target),"Move rejected at "+element+" "+offset);
                    var previous=actor.transform.position;
                    var deadline=DateTime.UtcNow.AddSeconds(20);
                    while(Vector3.Distance(actor.transform.position,target)>.8f)
                    {
                        Need(DateTime.UtcNow<deadline,"Walk blocked at "+element+" "+offset+" from "+actor.transform.position);
                        await Task.Delay(30);
                        walked+=Vector3.Distance(previous,actor.transform.position);
                        previous=actor.transform.position;
                    }
                    actor.Motor.Stop();
                    routes++;
                    if(offset.z==-7)
                    {
                        await Task.Delay(700);
                        var file=Path.Combine(output,element.ToLowerInvariant()+"-gameplay.png");
                        ScreenCapture.CaptureScreenshot(file);
                        await Task.Delay(300);
                        Need(File.Exists(file),"Missing live capture at "+element);
                    }
                }
            }
            Need(Time.frameCount-startFrame>40 && walked>70,"Game did not advance through actual walking.");
            return new {passed=true,walkedRoutes=routes,walkedMeters=walked,advancedFrames=Time.frameCount-startFrame,
                sites=4,opponentAiFrozen=true,output};
        }
        finally
        {
            actor.Motor.Stop();
            actor.Motor.Teleport(oldPosition);
            actor.Health.Invulnerable=oldInvulnerable;
            foreach(var behaviour in frozen) if(behaviour!=null) behaviour.enabled=true;
            camera.RecenterForTravel(actor.transform);
            nav.SetValue(null,oldNav);
            if(gameView!=null && oldGizmos!=null) gizmos.SetValue(gameView,oldGizmos);
        }
    }
    static Vector3 Point(Vector3 p)
    {
        Need(NavMesh.SamplePosition(p,out var hit,2,NavMesh.AllAreas),"No navigation at "+p);
        return hit.position;
    }
    static async Task Until(Func<bool> condition,int seconds,string detail)
    {
        var deadline=DateTime.UtcNow.AddSeconds(seconds);
        while(!condition()) { Need(DateTime.UtcNow<deadline,"Timed out: "+detail); await Task.Delay(30); }
    }
    static void Need(bool ok,string detail) { if(!ok) throw new InvalidOperationException(detail); }
}
