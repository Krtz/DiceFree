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

public static class BriarbackFortReview
{
    public static string Start()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("Clean Edit Mode required.");
        var guard=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("DiceFree.EditorTools.PersistenceTestGuard")).First(t=>t!=null);
        guard.GetMethod("UseIsolatedSaveRootForNextPlay").Invoke(null,new object[]{Path.Combine(Path.GetTempPath(),"DiceFree-FortReview-"+Guid.NewGuid())});
        UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/Cornberg.unity");EditorApplication.EnterPlaymode();return "Started isolated fort review.";
    }
    public static async Task<object> Run()
    {
        CombatActor player=null;
        await Until(()=>(player=Object.FindAnyObjectByType<TraversalInput>()?.GetComponent<CombatActor>())!=null&&player.Motor.Ready&&player.GetComponent<ManifestationPersistence>().Ready,"Player");
        player.GetComponent<TraversalInput>().enabled=false;player.GetComponent<CombatInput>().enabled=false;player.Health.Invulnerable=true;
        foreach(var b in Object.FindObjectsByType<MonoBehaviour>().Where(b=>b is AggroBehaviour||b is BasicAttack||b is QuilboarCombat||b is BriarbackChieftainEncounter))b.enabled=false;
        foreach(var actor in CombatActor.All)if(actor.Motor!=null&&actor.Motor.Ready)actor.Motor.Stop();
        var navType=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("UnityEditor.AI.NavMeshVisualizationSettings")).First(t=>t!=null);
        var nav=navType.GetProperty("showNavMesh",BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic);var oldNav=nav.GetValue(null);nav.SetValue(null,false);
        var viewType=typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");var view=Resources.FindObjectsOfTypeAll(viewType).FirstOrDefault();var gizmos=viewType.GetProperty("drawGizmos",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);var oldGizmos=view!=null?gizmos.GetValue(view):null;if(view!=null)gizmos.SetValue(view,false);
        var camera=Object.FindAnyObjectByType<ExplorationCamera>();
        string output="VisualBaselines/BriarbackFort";Directory.CreateDirectory(output);
        float meters=0;int walks=0;
        async Task Capture(string name,int settle=180){await Task.Delay(settle);ScreenCapture.CaptureScreenshot(output+"/"+name+".png");await Task.Delay(80);}
        try
        {
            Need(player.Motor.Teleport(Point(270,430,0)),"Gate start");camera.RecenterForTravel(player.transform);await Capture("gate-gameplay");
            foreach(var p in new[]{new Vector3(270,0,450),new Vector3(270,0,460),new Vector3(270,0,466),new Vector3(242,0,441),new Vector3(270,5.2f,460),new Vector3(294,5.2f,460),new Vector3(294,5.2f,476),new Vector3(282,5.2f,484)})
            {
                var target=Point(p.x,p.z,p.y);Need(player.Motor.MoveTo(target),"Move "+p);var previous=player.transform.position;var deadline=DateTime.UtcNow.AddSeconds(30);
                while(Vector3.Distance(player.transform.position,target)>.9f){Need(DateTime.UtcNow<deadline,"Walk blocked "+p+" from "+player.transform.position);await Task.Delay(25);meters+=Vector3.Distance(previous,player.transform.position);previous=player.transform.position;}
                player.Motor.Stop();walks++;
                if(p.x==270&&p.y>4)await Capture("bridge-gameplay");
                if(p.x==282)await Capture("arena-gameplay");
            }
            var chief=Object.FindAnyObjectByType<BriarbackChieftainEncounter>(FindObjectsInactive.Include);var boss=chief.GetComponent<CombatActor>();var attack=chief.GetComponent<BasicAttack>();
            Need(player.Motor.Teleport(Point(boss.transform.position.x,boss.transform.position.z+4.5f,5.2f)),"Smash setup");player.Motor.SetMotionAllowed(false);camera.RecenterForTravel(player.transform);
            chief.enabled=true;attack.enabled=true;Need(attack.Order(player),"Smash target");
            await Until(()=>chief.Winding,"Smash windup");
            var closeRotation=Quaternion.Euler(52,45,0);
            camera.SetPresentationPose((boss.transform.position+player.transform.position)*.5f+Vector3.up*1.5f-closeRotation*Vector3.forward*22,closeRotation,50);
            var warning=GameObject.Find("Chieftain smash - red fracture warning");var footprint=warning.transform.Find("Danger footprint").GetComponent<MeshFilter>().sharedMesh.bounds;
            Need(Mathf.Abs(footprint.size.x-1)<.001f&&Mathf.Abs(footprint.size.z-6)<.001f,"Warning changed its footprint");
            Need(warning.GetComponentsInChildren<Collider>().Length==0,"Warning has collision");
            Need(warning.GetComponentsInChildren<MeshFilter>().All(f=>f.sharedMesh.normals.All(n=>n.y>.9f)),"Warning accents face away from the player camera");
            await Capture("smash-warning",40);
            await Until(()=>GameObject.Find("Chieftain smash - dust and fracture")!=null,"Smash impact");await Capture("smash-impact",0);
            await Task.Delay(650);Need(GameObject.Find("Chieftain smash - dust and fracture")==null,"Impact effect leaked");
            boss.Health.Restore();attack.Cancel();
            player.Motor.SetMotionAllowed(true);Need(player.Motor.Teleport(Point(boss.transform.position.x,boss.transform.position.z+12,5.2f)),"Axe setup");player.Motor.SetMotionAllowed(false);camera.RecenterForTravel(player.transform);
            boss.Health.ApplyPacket(player,new DamageResult{mitigated=10});Need(attack.Order(player),"Axe target");
            GameObject axe=null;await Until(()=>(axe=GameObject.Find("Briarback Chieftain homing axe"))!=null,"Axe launch");
            Need(axe.transform.Find("Spinning forged axe")!=null&&axe.GetComponent<TrailRenderer>()!=null&&axe.GetComponentsInChildren<Collider>().Length==0,"Axe art missing or has collision");
            camera.SetPresentationPose((boss.transform.position+player.transform.position)*.5f+Vector3.up*1.5f-closeRotation*Vector3.forward*25,closeRotation,50);
            await Capture("axe-flight",130);
            await Until(()=>GameObject.Find("Briarback Chieftain homing axe")==null,"Axe terminal collision");
            boss.Health.Restore();await Task.Delay(550);
            Need(Object.FindObjectsByType<BriarbackChieftainEffects>().Length==0,"Combat cosmetic objects leaked after completion");
            return new{passed=true,walks,walkedMeters=meters,underpass=true,overpass=true,arena=true,smashFootprint="6x1",axeArt=true,fxCleanup=true};
        }
        finally{camera.ClearPresentationPose();nav.SetValue(null,oldNav);if(view!=null&&oldGizmos!=null)gizmos.SetValue(view,oldGizmos);}
    }
    static Vector3 Point(float x,float z,float y){Need(NavMesh.SamplePosition(new Vector3(x,y,z),out var hit,1.6f,NavMesh.AllAreas),"No NavMesh "+x+","+z);return hit.position;}
    static async Task Until(Func<bool> condition,string detail){var until=DateTime.UtcNow.AddSeconds(30);while(!condition()){Need(DateTime.UtcNow<until,"Timed out "+detail);await Task.Delay(15);}}
    static void Need(bool ok,string text){if(!ok)throw new InvalidOperationException(text);}
}
