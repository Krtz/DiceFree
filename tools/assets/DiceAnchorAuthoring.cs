using System;
using System.IO;
using System.Linq;
using DiceFree.World;
using DiceFree.Persistence;
using DiceFree.EditorTools;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;

public static class DiceAnchorAuthoring
{
 const string Folder="Assets/_DiceFree/Art/World/ResurrectionDice";
 const string Model=Folder+"/CosmicWonderDice.fbx";
 static ResurrectionAnchor[] Anchors()=>Object.FindObjectsByType<ResurrectionAnchor>().OrderBy(a=>a.StableId).ToArray();
 static void Need(bool b,string s){if(!b)throw new InvalidOperationException(s);}
 static Material Mat(string name,Color color,bool emission=false)
 {
  string path=Folder+"/"+name+".mat";
  var m=AssetDatabase.LoadAssetAtPath<Material>(path);
  if(m==null){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}
  m.SetColor("_BaseColor",color);m.SetFloat("_Smoothness",.35f);
  if(emission){m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",Color.black);}
  EditorUtility.SetDirty(m);return m;
 }
 public static object Apply()
 {
  var scene=SceneManager.GetActiveScene();
  Need(!Application.isPlaying && !scene.isDirty && scene.path=="Assets/_DiceFree/Scenes/Cornberg.unity","Save Cornberg and stop Play Mode first");
  var anchors=Anchors();Need(anchors.Length==2,"Expected exactly two town anchors");
  Need(anchors.All(a=>a.GetComponentInChildren<ResurrectionDiceVisual>()==null),"Dice already authored");
  string backup="Assets/_DiceFree/Settings/Recovery/Cornberg-before-admitted-dice-20261028.unity";
  Need(!File.Exists(backup),"Recovery path already occupied");
  Need(EditorSceneManager.SaveScene(scene,backup,true),"Recovery copy failed");
  Directory.CreateDirectory(Folder);
  Need(!File.Exists(Model),"Derivative path already occupied");
  FileUtil.CopyFileOrDirectory(@"T:\TEMP\DiceFree-DiceAsset\CosmicWonderDice.fbx",Model);
  AssetDatabase.ImportAsset(Model,ImportAssetOptions.ForceSynchronousImport);
  var importer=(ModelImporter)AssetImporter.GetAtPath(Model);
  importer.importAnimation=false;importer.addCollider=false;importer.materialImportMode=ModelImporterMaterialImportMode.None;importer.SaveAndReimport();
  var model=AssetDatabase.LoadAssetAtPath<GameObject>(Model);Need(model!=null,"FBX import failed");
  var ivory=Mat("Dice ivory",new Color(.83f,.79f,.65f));
  var recess=Mat("Dice recessed pips",new Color(.045f,.055f,.06f));
  var pipMat=Mat("World pip",new Color(.08f,.18f,.17f),true);
  foreach(var anchor in anchors)
  {
   bool corn=anchor.StableId=="anchor.cornberg";
   var old=GameObject.Find(corn?"Ancient stone d6 - corner resting":"Trimery D6 revive beacon");
   Need(old!=null,"Old beacon missing: "+anchor.StableId);
   float size=corn?2.2f:2.5f;
   var originalPosition=old.transform.position;
   float bottom=corn?.03f:.43f;
   var root=new GameObject("Admitted CosmicWonder resurrection dice");root.transform.SetParent(anchor.transform,false);
   root.transform.position=new Vector3(originalPosition.x,bottom,originalPosition.z);
   var instance=(GameObject)PrefabUtility.InstantiatePrefab(model,root.transform);
   instance.name="CosmicWonder Simple Boardgame Dice";
   var renderers=instance.GetComponentsInChildren<Renderer>();Need(renderers.Length>0,"No imported renderers");
   var bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);
   float ratio=size/Mathf.Max(bounds.size.x,Mathf.Max(bounds.size.y,bounds.size.z));
   instance.transform.localScale*=ratio;
   bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);
   instance.transform.position+=root.transform.position-new Vector3(bounds.center.x,bounds.min.y,bounds.center.z);
   foreach(var r in renderers)r.sharedMaterials=new[]{ivory,recess};
   // Preserve the legacy collision/navigation footprint and anchor references; replace rendering only.
   foreach(var r in old.GetComponentsInChildren<Renderer>(true))r.enabled=false;
   if(corn)HideLegacyPips(old.transform.position);
   var plate=GameObject.CreatePrimitive(PrimitiveType.Cube);plate.name="World 1 top face";plate.transform.SetParent(root.transform,false);
   plate.transform.localPosition=new Vector3(0,size+.012f,0);plate.transform.localScale=new Vector3(size*.78f,.035f,size*.78f);
   Object.DestroyImmediate(plate.GetComponent<Collider>());plate.GetComponent<Renderer>().sharedMaterial=ivory;
   var pip=GameObject.CreatePrimitive(PrimitiveType.Sphere);pip.name="World 1 pip - active resurrection glow";pip.transform.SetParent(root.transform,false);
   pip.transform.localPosition=new Vector3(0,size+.055f,0);pip.transform.localScale=new Vector3(.45f,.11f,.45f);
   Object.DestroyImmediate(pip.GetComponent<Collider>());pip.GetComponent<Renderer>().sharedMaterial=pipMat;
   var lamp=new GameObject("Active pip light").AddComponent<Light>();lamp.transform.SetParent(root.transform,false);lamp.transform.localPosition=new Vector3(0,size+.4f,0);
   lamp.type=LightType.Point;lamp.color=new Color(.3f,1f,.8f);lamp.intensity=2;lamp.range=3;lamp.enabled=false;
   root.AddComponent<ResurrectionDiceVisual>().Configure(anchor,pip.GetComponent<Renderer>(),lamp);
  }
  AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);Need(EditorSceneManager.SaveScene(scene),"Scene save failed");
  return Validate();
 }
 static int HideLegacyPips(Vector3 oldCenter)
 {
  int count=0;
  foreach(var r in Object.FindObjectsByType<Renderer>())
   if(System.Text.RegularExpressions.Regex.IsMatch(r.name,@"^Pip [1-6]$") && Vector3.Distance(r.transform.position,oldCenter)<3)
   {r.enabled=false;count++;}
  return count;
 }
 public static object RepairLegacyPips()
 {
  Need(!Application.isPlaying,"Stop Play Mode first");
  int hidden=HideLegacyPips(GameObject.Find("Ancient stone d6 - corner resting").transform.position);
  var scene=SceneManager.GetActiveScene();EditorSceneManager.MarkSceneDirty(scene);Need(EditorSceneManager.SaveScene(scene),"Save failed");
  return new{legacyPipRenderersHidden=hidden};
 }
 public static object Validate()
 {
  var anchors=Anchors();Need(anchors.Length==2,"Expected both anchors");
  foreach(var a in anchors)
  {
   var v=a.GetComponentInChildren<ResurrectionDiceVisual>();Need(v!=null && v.Anchor==a && v.WorldPip!=null,"Missing pip wiring");
   Need(v.WorldPip.transform.position.y>a.transform.position.y+2,"Pip must be above the die");
   Need(v.GetComponentsInChildren<MeshFilter>().Any(m=>m.sharedMesh!=null && m.sharedMesh.vertexCount>10000),"Imported dice mesh missing");
  }
  return new{success=true,anchors=anchors.Select(a=>new{id=a.StableId,landing=a.transform.position.ToString(),activation=a.ActivationPosition.ToString(),radius=a.ActivationRadius}).ToArray()};
 }
 public static object StartPlay()
 {
  Need(!SceneManager.GetActiveScene().isDirty,"Save scene before test");
  var guard=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("DiceFree.EditorTools.PersistenceTestGuard")).First(t=>t!=null);
  guard.GetMethod("UseIsolatedSaveRootForNextPlay",System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.Static).Invoke(null,new object[]{Path.Combine(Path.GetTempPath(),"DiceFree-DiceVisual-"+Guid.NewGuid().ToString("N"))});
  EditorApplication.EnterPlaymode();return "isolated Play Mode starting";
 }
 public static object CheckPlay()
 {
  Need(Application.isPlaying,"Play Mode required");
  var respawn=Object.FindAnyObjectByType<RespawnAtAnchor>();Need(respawn!=null,"No player respawn component");
  var anchors=Anchors();var original=anchors.FirstOrDefault(a=>a.StableId==respawn.AnchorId);
  try{
   foreach(var selected in anchors){Need(respawn.LoadAtAnchor(selected.StableId),"Saved anchor landing failed");Need(respawn.TeleportHome(),"Return Home failed");Need(respawn.Activate(selected),"Activation failed");foreach(var a in anchors){var v=a.GetComponentInChildren<ResurrectionDiceVisual>();v.Refresh();Need(v.Glowing==(a==selected),"Wrong active pip");Need(v.GetComponentInChildren<Light>(true).enabled==(a==selected),"Wrong pip light");var block=new MaterialPropertyBlock();v.WorldPip.GetPropertyBlock(block);Need((block.GetColor("_EmissionColor").maxColorComponent>0)==(a==selected),"Wrong emission");}Capture();}
   return new{passed=true,anchorSelections=2,successfulLandings=2,successfulHomeReturns=2,pipStates=4,lightStates=4,emissionStates=4,isolatedSave=true};
  }finally{if(original!=null)respawn.Activate(original);}
 }
 public static object Capture()
 {
  Directory.CreateDirectory("VisualBaselines/ResurrectionDice");
  foreach(var a in Anchors())
  {
   var v=a.GetComponentInChildren<ResurrectionDiceVisual>();var target=v.transform.position+Vector3.up*1.2f;
   var go=new GameObject("Temporary dice review camera");var cam=go.AddComponent<Camera>();
   cam.transform.position=target+new Vector3(5,4,-6);cam.transform.LookAt(target);cam.fieldOfView=36;
   var rt=new RenderTexture(900,700,24);cam.targetTexture=rt;var previous=RenderTexture.active;
   try{cam.Render();RenderTexture.active=rt;var tex=new Texture2D(900,700,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,900,700),0,0);tex.Apply();File.WriteAllBytes("VisualBaselines/ResurrectionDice/"+a.StableId+(Application.isPlaying?(v.Glowing?".active":".inactive"):".edit")+".png",tex.EncodeToPNG());Object.DestroyImmediate(tex);}
   finally{RenderTexture.active=previous;cam.targetTexture=null;Object.DestroyImmediate(rt);Object.DestroyImmediate(go);}
  }
  return "Two actual Unity camera captures saved";
 }
}
