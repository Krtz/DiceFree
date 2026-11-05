using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Core;
using DiceFree.Items;
using DiceFree.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using Object=UnityEngine.Object;
public static class NovemberFeedbackVisualReview
{
 public static async Task<string> Run()
 {
  var p=Object.FindAnyObjectByType<TraversalInput>().GetComponent<CombatActor>();var camera=Object.FindAnyObjectByType<ExplorationCamera>();
  var nav=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("UnityEditor.AI.NavMeshVisualizationSettings")).First(t=>t!=null).GetProperty("showNavMesh",BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic);var oldNav=nav.GetValue(null);
  var viewType=typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");var view=Resources.FindObjectsOfTypeAll(viewType).FirstOrDefault();var gizmos=viewType.GetProperty("drawGizmos",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);var oldGizmos=view!=null?gizmos.GetValue(view):null;
  bool cameraEnabled=camera.enabled;var fader=camera.GetComponent<CameraOcclusionFader>();bool fadeEnabled=fader.enabled;var bag=p.GetComponent<InventoryPanel>();
  const string folder="VisualBaselines/NovemberFeedback/Clean";Directory.CreateDirectory(folder);
  async Task Capture(string name){await Task.Delay(100);ScreenCapture.CaptureScreenshot(folder+"/"+name+".png");await Task.Delay(80);}
  try{
   nav.SetValue(null,false);if(view!=null)gizmos.SetValue(view,false);
   var caster=p.GetComponent<MageSkillCaster>();caster.ResetForManifestationLoad();p.GetComponent<ActorResourceController>().RefillAll();
   if(!p.Motor.Teleport(new Vector3(270,0,415)))throw new InvalidOperationException("Channel capture placement");camera.RecenterForTravel(p.transform);
   if(!caster.CastQuicksand(p.transform.position+Vector3.forward*3))throw new InvalidOperationException(caster.Feedback);await Task.Delay(300);await Capture("quicksand-channel");caster.CancelChannel();
   var inventory=p.GetComponent<CarriedInventory>();inventory.Grant("item.briarback.bristlehide-mantle");bag.Show();await Capture("mantle-inventory");bag.Close();
   bool captured=false;
   var direction=Vector3.ProjectOnPlane(Camera.main.transform.forward,Vector3.up).normalized;
   foreach(var tree in Object.FindObjectsByType<TreeCameraOccluder>().OrderBy(t=>Vector3.Distance(t.transform.position,p.transform.position))){
    var spot=tree.transform.position+direction*2.5f;if(!NavMesh.SamplePosition(spot,out var hit,1.5f,NavMesh.AllAreas)||!p.Motor.Teleport(hit.position))continue;
    camera.RecenterForTravel(p.transform);await Task.Delay(220);
    if(!tree.GetComponentsInChildren<Renderer>().Any(r=>r.sharedMaterials.Any(m=>m!=null&&m.name.Contains("camera fade"))))continue;
    await Capture("tree-faded-gameplay");
    camera.enabled=false;fader.enabled=false;await Capture("tree-opaque-comparison");
    fader.enabled=true;fader.UpdateOcclusion(p.transform.position+Vector3.up*1.5f,Camera.main.transform.position);await Capture("tree-faded-comparison");captured=true;break;
   }
   if(!captured)throw new InvalidOperationException("No actual hero-obscuring tree found.");
   return "Clean channel, inventory icon and tree opacity comparison captured at the gameplay camera.";
  }finally{camera.enabled=cameraEnabled;fader.enabled=fadeEnabled;bag.Close();nav.SetValue(null,oldNav);if(view!=null&&oldGizmos!=null)gizmos.SetValue(view,oldGizmos);}
 }
}
