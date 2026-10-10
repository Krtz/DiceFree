using System;
using System.Linq;
using DiceFree.World;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
namespace DiceFree.EditorTools {
internal static class CornbergScreenshotsFix {
const string ScenePath="Assets/_DiceFree/Scenes/Cornberg.unity";
static Transform[] All(Scene s)=>s.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).ToArray();
static void Assert(bool ok,string text){if(!ok)throw new InvalidOperationException(text);}
[CliCommand("dicefree.visual.screenshots-fix","Repair photographed bridge tree, guard placement, south house entrances and missing URP path materials.")]
public static object Install(){
 Assert(!EditorApplication.isPlayingOrWillChangePlaymode,"Exit Play Mode");
 for(int i=0;i<SceneManager.sceneCount;i++)Assert(!SceneManager.GetSceneAt(i).isDirty,"Unsaved scene exists");
 var prior=EditorSceneManager.GetSceneManagerSetup();
 try{
 var scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
 var all=All(scene);
 // The spruce and its original gameplay trunk were a single authored Woodland tree,
 // whose trunk stood at (-10,27) directly at the northern bridge entrance.
 var spruce=all.FirstOrDefault(t=>t.name=="Woodland tree"&&
   t.GetComponentsInChildren<Renderer>(true).Any(r=>r.name=="Trunk" &&
   Vector2.Distance(new Vector2(r.bounds.center.x,r.bounds.center.z),new Vector2(-10f,27f))<1));
 Assert(spruce!=null,"Photo bridge spruce tree not found");
 var treeTrunk=spruce.GetComponentsInChildren<Renderer>().First(r=>r.name=="Trunk").bounds.center;
 if(treeTrunk.x>-15) spruce.position+=new Vector3(-19f,0,19f);
 var newTrunk=spruce.GetComponentsInChildren<Renderer>().First(r=>r.name=="Trunk").bounds.center;
 Assert(Vector2.Distance(new Vector2(newTrunk.x,newTrunk.z),new Vector2(-16,26))>12f,
  "Spruce still obstructs the north bridge");
 // South crossing at z=-9: the static guard was born inside the bridge railing.
 var guard=all.FirstOrDefault(t=>t.name.Contains("NPC_Guard")&&
    Vector2.Distance(new Vector2(t.position.x,t.position.z),new Vector2(-18,-7))<3);
 Assert(guard!=null,"South bridge guard NPC not found");
 var navigation=all.Select(t=>t.GetComponent<WorldNavigation>()).FirstOrDefault(v=>v!=null);
 Assert(navigation!=null&&navigation.Data!=null,"Baked nav mesh missing");
 var handle=NavMesh.AddNavMeshData(navigation.Data);
 try{
  if(guard.position.x<-14){
   Assert(NavMesh.SamplePosition(new Vector3(-10,0,-8),out var hit,3f,NavMesh.AllAreas),
     "Safe eastern bridge bank not walkable");
   guard.position=hit.position;
  }
 }finally{if(handle.valid)handle.Remove();}
 // Only ordinary southern residences: keep occupied Brewery/General Goods service NPC
 // and their interaction anchors unchanged.
 var centres=new[]{new Vector2(-5,-29),new Vector2(12,-29),new Vector2(36,-26)};
 int rotated=0;
 foreach(var centre in centres){
  var house=all.FirstOrDefault(t=>(t.name=="Cottage"||t.name.Contains("Abandoned house - locked"))&&
    t.GetComponentsInChildren<Renderer>().Any(r=>r.name=="Fieldstone foundation"&&
     Vector2.Distance(new Vector2(r.bounds.center.x,r.bounds.center.z),centre)<1f));
  Assert(house!=null,"Missing southern house at "+centre);
  var door=house.GetComponentsInChildren<Renderer>().First(r=>r.name=="Door - exterior blockout");
  if(door.bounds.center.z>=centre.y)continue;
  Vector3 pivot=new Vector3(centre.x,0,centre.y);
  Quaternion flip=Quaternion.Euler(0,180,0);
  foreach(Transform child in house){
    child.position=pivot+flip*(child.position-pivot);
    child.rotation=flip*child.rotation;
  }
  Assert(door.bounds.center.z>centre.y+1,"Door still faces away from road");
  rotated++;
 }
 // Two photographed vivid-pink path strips have completely NULL material slots.
 var road=all.SelectMany(t=>t.GetComponents<Renderer>()).FirstOrDefault(r=>r.name=="Mountain to east road");
 Assert(road!=null&&road.sharedMaterial!=null,"Valid road material missing");
 int repaired=0;
 foreach(string name in new[]{"North meadow farm path","Eastern woodland fighting path"}){
   var renderer=all.SelectMany(t=>t.GetComponents<Renderer>()).FirstOrDefault(r=>r.name==name);
   Assert(renderer!=null,"Photo magenta path missing: "+name);
   renderer.sharedMaterial=road.sharedMaterial;
   repaired++;
 }
 EditorSceneManager.MarkSceneDirty(scene);
 Assert(EditorSceneManager.SaveScene(scene),"Scene save failed");
 return new{success=true,treeBefore=treeTrunk.ToString("F1"),treeAfter=newTrunk.ToString("F1"),guardNow=guard.position.ToString("F1"),rotated,repaired};
 }finally{EditorSceneManager.RestoreSceneManagerSetup(prior);}
}
[CliCommand("dicefree.visual.screenshots-validate","Verify tree is off bridge, guard on bank, house doors road-facing, no invalid road materials.")]
public static object Validate(){
var prior=EditorSceneManager.GetSceneManagerSetup();
try{
var s=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
var all=All(s);
var trunk=all.SelectMany(t=>t.GetComponents<Renderer>()).Where(r=>r.name=="Trunk")
 .Where(r=>Vector2.Distance(new Vector2(r.bounds.center.x,r.bounds.center.z),new Vector2(-16,26))<7).ToArray();
Assert(trunk.Length==0,"Trunk still blocks northern bridge");
var guard=all.First(t=>t.name.Contains("NPC_Guard"));
Assert(guard.position.x>-14,"Guard still inside bridge");
foreach(var center in new[]{new Vector2(-5,-29),new Vector2(12,-29),new Vector2(36,-26)}){
var group=all.First(t=>(t.name=="Cottage"||t.name.Contains("Abandoned house - locked"))&&
 t.GetComponentsInChildren<Renderer>().Any(r=>r.name=="Fieldstone foundation"&&
 Vector2.Distance(new Vector2(r.bounds.center.x,r.bounds.center.z),center)<1));
Assert(group.GetComponentsInChildren<Renderer>().First(r=>r.name=="Door - exterior blockout").bounds.center.z>center.y,
  "South house door wrongly oriented");
}
foreach(var name in new[]{"North meadow farm path","Eastern woodland fighting path"})
Assert(all.SelectMany(t=>t.GetComponents<Renderer>()).First(r=>r.name==name).sharedMaterial!=null,"Path still pink");
return new{success=true,bridgeTreeMoved=true,bridgeGuardRelocated=true,southHousesFacingRoad=3,pinkPathsFixed=2};
}finally{EditorSceneManager.RestoreSceneManagerSetup(prior);}
}
}
}