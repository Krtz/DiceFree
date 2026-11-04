using System;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Core;
using DiceFree.World;
using UnityEditor;
using UnityEngine;
using Object=UnityEngine.Object;

public static class BriarbackAmbienceReview
{
    public static async Task<object> Run()
    {
        Need(Application.isPlaying, "Run the isolated landmark review Start entry first.");
        var deadline=DateTime.UtcNow.AddSeconds(20);
        while(Object.FindObjectsByType<ElementalLandmarkAmbience>().Length!=4)
        {
            Need(DateTime.UtcNow<deadline,"Landmark ambience didn't start.");
            await Task.Delay(30);
        }
        var effects=Object.FindObjectsByType<ElementalLandmarkAmbience>().OrderBy(e=>e.name).ToArray();
        int windChecks=0,emissionChecks=0,pinnedChecks=0,cleanupChecks=0;
        foreach(var effect in effects)
        {
            var cloth=effect.GetComponentsInChildren<MeshFilter>().Single(m=>m.name is "Oath cloth" or "Trail cloth");
            var original=AssetDatabase.LoadAssetAtPath<Mesh>(
                "Assets/_DiceFree/Art/World/Briarback"+Element(effect.name)+"/"+cloth.name+".asset");
            Need(original!=null && cloth.sharedMesh!=original,"Cloth must use an instance, not mutate source geometry.");
            var originalVertices=original.vertices;
            var first=cloth.sharedMesh.vertices;
            var renderer=effect.GetComponentsInChildren<Renderer>().Single(r=>r.name is "Amber flame" or "Element core");
            var baseline=renderer.sharedMaterial.GetColor("_EmissionColor");
            var block=new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block);
            var firstEmission=block.GetColor("_EmissionColor");
            await Task.Delay(550);
            var second=cloth.sharedMesh.vertices;
            Need(first.Where((v,i)=>Vector3.Distance(v,second[i])>.0001f).Any(),"Cloth has no wind movement.");
            windChecks++;
            for(int i=0;i<originalVertices.Length;i++)
            {
                float x=originalVertices[i].x;
                if(Mathf.Abs(x+4.33f)<.002f || Mathf.Abs(x-3.67f)<.002f)
                {
                    Need(Vector3.Distance(second[i],originalVertices[i])<.0001f,"Pole edge isn't pinned.");
                    pinnedChecks++;
                }
            }
            Need(originalVertices.SequenceEqual(original.vertices),"The source mesh changed.");
            Need(renderer.sharedMaterial.GetColor("_EmissionColor")==baseline,"Shared material emission was mutated.");
            renderer.GetPropertyBlock(block);
            if(Element(effect.name)!="Earth")
            {
                Need(firstEmission!=block.GetColor("_EmissionColor"),"Elemental emission has no variation.");
                emissionChecks++;
            }
            effect.enabled=false;
            Need(cloth.sharedMesh==original,"Disabling ambience did not restore the source mesh.");
            cleanupChecks++;
            effect.enabled=true;
        }
        await Task.Delay(100);
        var particles=effects.SelectMany(e=>e.GetComponentsInChildren<ParticleSystem>()).ToArray();
        Need(particles.Length==1 && particles[0].main.maxParticles==12 && particles[0].isPlaying,
            "Expected only the one contained fire ember emitter.");
        Need(particles[0].GetComponent<ParticleSystemRenderer>().mesh.normals.All(n=>n.sqrMagnitude>.9f),
            "Ember normals are invalid for the lit material.");
        Need(effects.All(e=>e.GetComponentsInChildren<Collider>().Length==0),"Ambience added a collider.");
        return new{passed=true,windChecks,pinnedChecks,emissionChecks,cleanupChecks,containedEmitters=1,maximumEmbers=12};
    }
    static string Element(string name)=>name.StartsWith("Ember",StringComparison.Ordinal)?"Fire":
        name.StartsWith("Splitroot",StringComparison.Ordinal)?"Earth":
        name.StartsWith("Stormcall",StringComparison.Ordinal)?"Lightning":"Ice";
    static void Need(bool condition,string detail){if(!condition)throw new InvalidOperationException(detail);}

    public static async Task<object> Capture()
    {
        Need(Application.isPlaying,"Isolated Play Mode required.");
        var actor=Object.FindAnyObjectByType<TraversalInput>().GetComponent<CombatActor>();
        var camera=Object.FindAnyObjectByType<ExplorationCamera>();
        var site=GameObject.Find("Fire Shaman settlement - provisional").transform;
        Vector3 originalPosition=actor.transform.position;
        bool invulnerable=actor.Health.Invulnerable;
        var cameraPosition=camera.transform.position;
        var cameraRotation=camera.transform.rotation;
        var native=camera.GetComponent<Camera>();
        float fov=native.fieldOfView;
        bool presentation=camera.PresentationOverride;
        var navType=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("UnityEditor.AI.NavMeshVisualizationSettings")).First(t=>t!=null);
        var nav=navType.GetProperty("showNavMesh",BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic);
        var oldNav=nav.GetValue(null);
        var gameViewType=typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");
        var gameView=Resources.FindObjectsOfTypeAll(gameViewType).FirstOrDefault();
        var gizmos=gameViewType.GetProperty("drawGizmos",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);
        var oldGizmos=gameView!=null ? gizmos.GetValue(gameView) : null;
        var output=Path.Combine(Directory.GetParent(Application.dataPath).FullName,"Logs","BriarbackAmbienceFrames");
        Directory.CreateDirectory(output);
        try
        {
            nav.SetValue(null,false);
            if(gameView!=null)gizmos.SetValue(gameView,false);
            actor.Health.Invulnerable=true;
            Need(actor.Motor.Teleport(site.position+new Vector3(2,0,-7)), "No player approach for recording.");
            var rotation=Quaternion.Euler(52,45,0);
            camera.SetPresentationPose(site.position+Vector3.up*1.5f-rotation*Vector3.forward*22,rotation,50);
            await Task.Delay(500);
            for(int i=0;i<40;i++)
            {
                ScreenCapture.CaptureScreenshot(Path.Combine(output,i.ToString("D4")+".png"));
                await Task.Delay(100);
            }
            Need(File.Exists(Path.Combine(output,"0039.png")),"Last recording frame missing.");
            return new {frames=40,sampleIntervalMilliseconds=100,output};
        }
        finally
        {
            actor.Motor.Teleport(originalPosition);
            actor.Health.Invulnerable=invulnerable;
            if(presentation) camera.SetPresentationPose(cameraPosition,cameraRotation,fov);
            else camera.ClearPresentationPose();
            nav.SetValue(null,oldNav);
            if(gameView!=null && oldGizmos!=null)gizmos.SetValue(gameView,oldGizmos);
        }
    }
}
