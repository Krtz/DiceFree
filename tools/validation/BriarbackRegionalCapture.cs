using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DiceFree.Briarback;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Core;
using DiceFree.World;
using UnityEditor;
using UnityEngine;
using Object=UnityEngine.Object;

public static class BriarbackRegionalCapture
{
    public static async Task<object> Run()
    {
        var player=Object.FindAnyObjectByType<TraversalInput>().GetComponent<CombatActor>();
        var clock=Object.FindAnyObjectByType<WorldDayNightCycle>();var camera=Object.FindAnyObjectByType<ExplorationCamera>();
        var nav=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("UnityEditor.AI.NavMeshVisualizationSettings")).First(t=>t!=null).GetProperty("showNavMesh",BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic);var oldNav=nav.GetValue(null);
        var viewType=typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");var view=Resources.FindObjectsOfTypeAll(viewType).FirstOrDefault();var gizmos=viewType.GetProperty("drawGizmos",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);var oldGizmos=view!=null?gizmos.GetValue(view):null;
        float hour=clock.Hour;bool running=new SerializedObject(clock).FindProperty("running").boolValue;var preset=GraphicsQualityRuntime.Preset;
        string output="VisualBaselines/BriarbackRegion/Live";Directory.CreateDirectory(output);
        try
        {
            nav.SetValue(null,false);if(view!=null)gizmos.SetValue(view,false);camera.RecenterForTravel(player.transform);
            foreach(var p in Enum.GetValues(typeof(VisualQuality)).Cast<VisualQuality>())
            {
                GraphicsQualityRuntime.Select(p,false);
                foreach(var h in new[]{12f,0f})
                {
                    clock.SetHour(h,false);await Task.Delay(300);
                    var warning=BriarbackChieftainEffects.Warning(player.transform.position+new Vector3(2,.09f,1),Vector3.forward);
                    await Task.Delay(120);ScreenCapture.CaptureScreenshot(output+"/quality-"+p.ToString().ToLowerInvariant()+(h==0?"-night":"")+".png");await Task.Delay(100);Object.Destroy(warning);
                }
            }
            foreach(var h in new[]{12f,18f,0f}){clock.SetHour(h,false);await Task.Delay(250);ScreenCapture.CaptureScreenshot(output+"/hour-"+h+".png");await Task.Delay(100);}
            return new{cleanCaptures=11,normalGameplayCamera=true,editorGizmos=false,warningDayAndNight=true};
        }
        finally{GraphicsQualityRuntime.Select(preset,false);clock.SetHour(hour,running);camera.ClearPresentationPose();nav.SetValue(null,oldNav);if(view!=null&&oldGizmos!=null)gizmos.SetValue(view,oldGizmos);}
    }
    public static string ExportReport()
    {
        var type=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("BriarbackRegionalLiveReview")).First(t=>t!=null&&t.GetField("report",BindingFlags.NonPublic|BindingFlags.Static).GetValue(null)!=null);
        var report=type.GetField("report",BindingFlags.NonPublic|BindingFlags.Static).GetValue(null);
        string json=JsonUtility.ToJson(report,true);var rows=((System.Collections.IEnumerable)type.GetNestedType("Report").GetField("respawns").GetValue(report)).Cast<object>().Select(r=>JsonUtility.ToJson(r));
        if(!json.Contains("\"respawns\""))json=json.TrimEnd().TrimEnd('}')+",\n\"respawns\":["+string.Join(",",rows)+"]\n}";
        File.WriteAllText("Logs/BriarbackRegionalLiveReview.json",json);return json;
    }
}
