using System;using System.Collections;using System.IO;using System.Linq;
using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Unity.Pipeline.Commands;
using DiceFree.Development;using DiceFree.Persistence;using DiceFree.UI;using DiceFree.Visuals;using DiceFree.Items;using DiceFree.Combat;using DiceFree.Characters;using DiceFree.Input;using DiceFree.Advancement;
namespace DiceFree.EditorTools {
[InitializeOnLoad] public static class PhysicalModelValidation {
const string Key="DiceFree.PhysicalModel.";static IEnumerator flow;static double deadline;static int checks;
static PhysicalModelValidation()=>EditorApplication.playModeStateChanged+=Changed;
static void Need(bool ok,string message){if(!ok)throw new InvalidOperationException(message);checks++;}
[CliCommand("dicefree.physical.model.test","Check live starter-armour animation, sockets, portrait and class switching in an isolated Echo.")]
public static object Start(){
if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play Mode first.");
SessionState.SetString(Key+"scene",EditorSceneManager.GetActiveScene().path);SessionState.SetBool(Key+"options",EditorSettings.enterPlayModeOptionsEnabled);SessionState.SetInt(Key+"flags",(int)EditorSettings.enterPlayModeOptions);
SessionState.SetBool(Key+"cleanup",true);SessionState.SetBool(Key+"resume",false);SessionState.SetString(Key+"status","running");SessionState.SetString(Key+"error","");
PersistenceTestGuard.UseIsolatedSaveRootForNextPlay(Path.Combine(Path.GetTempPath(),"DiceFree-PhysicalModel-"+Guid.NewGuid().ToString("N")));
EditorSettings.enterPlayModeOptionsEnabled=false;EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/DevelopmentRoom.unity");EditorApplication.EnterPlaymode();return Status();}
[CliCommand("dicefree.physical.model.status","Read live Physical model validation.")]
public static object Status()=>new{status=SessionState.GetString(Key+"status","idle"),checks=SessionState.GetInt(Key+"checks",0),error=SessionState.GetString(Key+"error","")};
[CliCommand("dicefree.physical.model.resume","Finish after capturing the Physical model.")]
public static object Resume(){SessionState.SetBool(Key+"resume",true);return Status();}
static void Changed(PlayModeStateChange state){
if(state==PlayModeStateChange.EnteredPlayMode && SessionState.GetString(Key+"status","")=="running"){checks=0;deadline=EditorApplication.timeSinceStartup+240;flow=Flow();EditorApplication.update+=Tick;Application.logMessageReceived+=Log;}
if(state==PlayModeStateChange.EnteredEditMode&&SessionState.GetBool(Key+"cleanup",false)){SessionState.SetBool(Key+"cleanup",false);EditorApplication.update-=Tick;Application.logMessageReceived-=Log;EditorSettings.enterPlayModeOptions=(EnterPlayModeOptions)SessionState.GetInt(Key+"flags",0);EditorSettings.enterPlayModeOptionsEnabled=SessionState.GetBool(Key+"options",false);PersistenceTestGuard.ClearValidationOverrides();string scene=SessionState.GetString(Key+"scene","");if(!string.IsNullOrEmpty(scene))EditorSceneManager.OpenScene(scene);}}
static void Log(string m,string s,LogType t){if(t==LogType.Error||t==LogType.Exception)Finish(false,m+"\n"+s);}
static void Tick(){if(!EditorApplication.isPlaying)return;try{if(EditorApplication.timeSinceStartup>deadline)throw new TimeoutException("Physical model test timed out.");if(flow!=null&&!flow.MoveNext())Finish(true,null);}catch(Exception e){Finish(false,e.ToString());}}
static IEnumerator Flow(){
while(DevelopmentRoomController.Current==null||DevelopmentRoomController.Current.player==null)yield return null;
var dev=DevelopmentRoomController.Current;var player=dev.player;while(!player.GetComponent<ManifestationPersistence>().Ready||dev.Map==null)yield return null;
player.GetComponent<TraversalInput>().enabled=false;player.GetComponent<CombatInput>().enabled=false;player.Health.Invulnerable=true;
var holder=new GameObject("Missing licensed pack fixture");holder.SetActive(false);
var fallback=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(PhysicalBlessedAuthoring.PhysicalPrefabPath),holder.transform);
var fallbackArt=fallback.GetComponent<PhysicalBlessedVisual>();var fallbackSerialized=new SerializedObject(fallbackArt);fallbackSerialized.FindProperty("resourcePath").stringValue="PhysicalLicensed/UninstalledFixture";fallbackSerialized.ApplyModifiedPropertiesWithoutUndo();holder.SetActive(true);
Need(!fallbackArt.LicensedModelLoaded,"Unavailable licensed pack did not retain fallback");
var fallbackAnimator=ClassPresentationSwitcher.FindVisualAnimator(fallback.transform);Need(fallbackAnimator!=null&&fallbackAnimator.isActiveAndEnabled&&fallbackAnimator.avatar.isValid,"Portable fallback animator missing");
Need(fallback.GetComponentsInChildren<Renderer>().Any(r=>r.enabled)&&fallback.GetComponentsInChildren<Collider>(true).Length==0,"Portable fallback lost visible art or gained colliders");UnityEngine.Object.Destroy(holder);
Need(dev.SelectClass(PhysicalBlessedAuthoring.ClassId),"Physical class selection");yield return null;yield return null;
var switcher=player.GetComponent<ClassPresentationSwitcher>();var root=switcher.ActiveVisualRoot;var art=root.GetComponent<PhysicalBlessedVisual>();var driver=player.GetComponent<NovicePresentationDriver>();var animator=driver.VisualAnimator;
Need(art!=null&&art.LicensedModelLoaded,"Licensed Physical model did not load");
Need(animator==art.VisualAnimator&&animator.avatar.isValid&&animator.isHuman,"Driver uses wrong or invalid animator");
Need(!animator.applyRootMotion&&root.GetComponentsInChildren<Collider>(true).Length==0,"Presentation owns movement or colliders");
Need(root.GetComponentsInChildren<Renderer>().Where(r=>r.enabled).All(r=>r.sharedMaterials.All(m=>m!=null&&m.shader.name=="Universal Render Pipeline/Lit")),"Visible unsupported material");
Need(!root.GetComponentsInChildren<Transform>(true).Single(t=>t.name=="Starter_Helmet").gameObject.activeSelf,"Helmet obscures the face");
float initialTime=animator.GetCurrentAnimatorStateInfo(0).normalizedTime;float until=Time.time+.4f;while(Time.time<until)yield return null;
Need(animator.GetCurrentAnimatorStateInfo(0).normalizedTime>initialTime,"Idle animation does not tick");
var bounds=new Bounds();bool first=true;foreach(var r in root.GetComponentsInChildren<Renderer>().Where(r=>r.enabled)){if(first){bounds=r.bounds;first=false;}else bounds.Encapsulate(r.bounds);}
File.WriteAllText("Library/PhysicalModelPose.txt","rootScale="+root.localScale+" rotation="+root.localEulerAngles+" bounds="+bounds+" head="+animator.GetBoneTransform(HumanBodyBones.Head).position+" clips="+string.Join(",",animator.runtimeAnimatorController.animationClips.Select(c=>c.name+":"+c.isHumanMotion)));
Need(bounds.size.y>1.5f&&bounds.size.y<2.8f,"Physical model height outside player scale");
Vector3 start=player.transform.position;Need(player.Motor.MoveTo(start+Vector3.forward*3),"Walking request");float maxSpeed=0;until=Time.time+1;while(Time.time<until){maxSpeed=Mathf.Max(maxSpeed,driver.LastDrivenSpeed);yield return null;}
Need(Vector3.Distance(player.transform.position,start)>.5f&&maxSpeed>.1f,"Player movement does not drive the model");player.Motor.Stop();
driver.PlayAttackAnimation();until=Time.time+.12f;while(Time.time<until)yield return null;
Need(animator.GetCurrentAnimatorStateInfo(0).IsName("UnarmedAttack")||animator.IsInTransition(0),"Basic attack animation did not trigger");
until=Time.time+1.5f;while(Time.time<until)yield return null;
var equipment=player.GetComponent<Equipment>();var inventory=player.GetComponent<CarriedInventory>();var ep=player.GetComponent<EquipmentPresentation>();
foreach(string id in new[]{"item.bronze-dagger","item.slime.slime-shield","item.slime.slimy-tophat","item.slime.slimy-farmers-gloves"}){
var definition=inventory.Definitions.FirstOrDefault(d=>d.stableId==id);Need(definition!=null,"Missing gear definition "+id);
var item=inventory.Grant(id);Need(equipment.Equip(item.instanceId,definition.slot),"Equip "+id);yield return null;
var visuals=ep.Bindings.Where(b=>b.definition==definition).SelectMany(b=>b.visuals).Where(g=>g!=null&&g.transform.IsChildOf(root)).ToArray();
Need(visuals.Length>0&&visuals.All(g=>g.activeInHierarchy),"No Physical gear visuals "+id);
Need(visuals.All(g=>g.transform.IsChildOf(animator.transform)),"Equipment remains on the hidden fallback skeleton "+id);
Need(equipment.Unequip(definition.slot.ToString()),"Unequip "+id);yield return null;
Need(visuals.All(g=>!g.activeSelf),"Gear did not hide "+id);
}
yield return null;yield return null;
var portrait=player.GetComponent<HudPortraitRenderer>();Need(portrait!=null&&portrait.Texture!=null&&portrait.Texture.IsCreated(),"HUD portrait missing");
var studio=player.transform.Find("HUD Portrait Studio");var clone=studio.GetComponentsInChildren<PhysicalBlessedVisual>(true).Single();
Need(clone.LicensedModelLoaded&&clone.GetComponentsInChildren<Animator>(true).Count(a=>a.enabled)==1,"Portrait duplicated the licensed model or animated the fallback");
Need(dev.SelectClass("class.novice"),"Switch to Novice");yield return null;Need(!root.gameObject.activeSelf&&switcher.CurrentClassId=="class.novice","Physical model leaked into Novice");
Need(dev.SelectClass(PhysicalBlessedAuthoring.ClassId),"Return to Physical");yield return null;Need(switcher.ActiveVisualRoot==root&&root.GetComponentsInChildren<PhysicalBlessedVisual>(true).Length==1,"Physical switch duplicated presentation");
Need(player.Motor.Teleport(new Vector3(0,0,-8)),"Visual placement");player.transform.rotation=Quaternion.Euler(0,225,0);dev.explorationCamera.RecenterForTravel(player.transform);
typeof(HudPortraitRenderer).GetMethod("Rebuild",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(portrait,null);yield return null;
var portraitCamera=studio.GetComponentInChildren<Camera>();
Need(Vector3.Dot(portraitCamera.transform.forward,-studio.forward)>.999f,"Rotated portrait does not face the character");
var mesh=new Mesh();int outside=0;try{foreach(var skin in studio.GetComponentsInChildren<SkinnedMeshRenderer>()){if(!skin.enabled)continue;skin.BakeMesh(mesh);foreach(var vertex in mesh.vertices){var viewport=portraitCamera.WorldToViewportPoint(skin.transform.TransformPoint(vertex));if(viewport.z<=0||viewport.x<.025f||viewport.x>.975f||viewport.y<.025f||viewport.y>.975f)outside++;}}}finally{UnityEngine.Object.Destroy(mesh);}
Need(outside==0,"Rotated portrait crops the visible body");
SessionState.SetInt(Key+"checks",checks);SessionState.SetString(Key+"status","visual_ready");while(!SessionState.GetBool(Key+"resume",false))yield return null;
}
static void Finish(bool ok,string error){flow=null;EditorApplication.update-=Tick;Application.logMessageReceived-=Log;SessionState.SetInt(Key+"checks",checks);SessionState.SetString(Key+"status",ok?"passed":"failed");SessionState.SetString(Key+"error",error??"");File.WriteAllText("Library/PhysicalModelResults.json",JsonUtility.ToJson(new Record{passed=ok,checks=checks,error=error},true));EditorApplication.ExitPlaymode();}
[Serializable]class Record{public bool passed;public int checks;public string error;}
}}
