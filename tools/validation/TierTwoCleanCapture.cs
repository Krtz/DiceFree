using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Core;
using DiceFree.Progression;
using UnityEditor;
using UnityEngine;
using Object=UnityEngine.Object;

public static class TierTwoCleanCapture
{
    public static async Task<string> Run()
    {
        var p=Object.FindAnyObjectByType<TraversalInput>().GetComponent<CombatActor>();var camera=Object.FindAnyObjectByType<ExplorationCamera>();
        var nav=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("UnityEditor.AI.NavMeshVisualizationSettings")).First(t=>t!=null).GetProperty("showNavMesh",BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic);var oldNav=nav.GetValue(null);
        var viewType=typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");var view=Resources.FindObjectsOfTypeAll(viewType).FirstOrDefault();var gizmos=viewType.GetProperty("drawGizmos",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);var oldGizmos=view!=null?gizmos.GetValue(view):null;
        string folder="VisualBaselines/TierTwo/CleanEditor";Directory.CreateDirectory(folder);string label=p.Stats.Definition.stableId;
        try
        {
            nav.SetValue(null,false);if(view!=null)gizmos.SetValue(view,false);camera.RecenterForTravel(p.transform);await Capture(folder+"/"+label+"-gameplay.png");
            var rotation=Quaternion.Euler(52,45,0);camera.SetPresentationPose(p.transform.position+Vector3.up*1.4f-rotation*Vector3.forward*18,rotation,50);await Capture(folder+"/"+label+"-closer.png");
            if(label==GreatbladeProgressionRules.ClassId){var c=p.GetComponent<GreatbladeSkillCaster>();c.ResetForManifestationLoad();if(!c.CastWideArc(p.transform.position+Vector3.forward*4))throw new InvalidOperationException(c.Feedback);await Capture(folder+"/"+label+"-cleave.png",90);}
            else{var c=p.GetComponent<MageSkillCaster>();c.ResetForManifestationLoad();p.GetComponent<ActorResourceController>().RefillAll();if(!c.Cast(MageProgressionRules.Kind.MathBarrier))throw new InvalidOperationException(c.Feedback);await Capture(folder+"/"+label+"-barrier.png",120);}
            return "Captured gameplay, closer and real skill poses with navigation/gizmo overlays temporarily disabled.";
        }
        finally{camera.ClearPresentationPose();nav.SetValue(null,oldNav);if(view!=null&&oldGizmos!=null)gizmos.SetValue(view,oldGizmos);}
    }
    static async Task Capture(string path,int delay=150){await Task.Delay(delay);ScreenCapture.CaptureScreenshot(path);await Task.Delay(60);}
}
