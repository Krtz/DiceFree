using System;using System.Collections;using System.IO;using System.Linq;
using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Unity.Pipeline.Commands;
using DiceFree.Characters;using DiceFree.Development;using DiceFree.Persistence;using DiceFree.UI;using DiceFree.Visuals;using DiceFree.Items;using DiceFree.Combat;using DiceFree.Input;
namespace DiceFree.EditorTools{
[InitializeOnLoad]public static class MagicalModelValidation{
const string Key="DiceFree.MagicalModel.";static IEnumerator flow;static double deadline;static int checks;
static MagicalModelValidation()=>EditorApplication.playModeStateChanged+=Changed;
static void Need(bool ok,string m){if(!ok)throw new InvalidOperationException(m);checks++;}
[CliCommand("dicefree.magical.model.test","Check smaller caster silhouette, source fallback, animation, staff, gear and portrait in an isolated Echo.")]
public static object Start(){if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play Mode first.");
SessionState.SetString(Key+"scene",EditorSceneManager.GetActiveScene().path);SessionState.SetBool(Key+"options",EditorSettings.enterPlayModeOptionsEnabled);SessionState.SetInt(Key+"flags",(int)EditorSettings.enterPlayModeOptions);
SessionState.SetBool(Key+"cleanup",true);SessionState.SetBool(Key+"resume",false);SessionState.SetString(Key+"status","running");SessionState.SetString(Key+"error","");SessionState.SetInt(Key+"checks",0);
PersistenceTestGuard.UseIsolatedSaveRootForNextPlay(Path.Combine(Path.GetTempPath(),"DiceFree-MagicalModel-"+Guid.NewGuid().ToString("N")));
EditorSettings.enterPlayModeOptionsEnabled=false;EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/DevelopmentRoom.unity");EditorApplication.EnterPlaymode();return Status();}
[CliCommand("dicefree.magical.model.status","Read caster model validation.")]
public static object Status()=>new{status=SessionState.GetString(Key+"status","idle"),checks=SessionState.GetInt(Key+"checks",0),error=SessionState.GetString(Key+"error","")};
[CliCommand("dicefree.magical.model.resume","Finish after capturing caster art.")]
public static object Resume(){SessionState.SetBool(Key+"resume",true);return Status();}
static void Changed(PlayModeStateChange s){
if(s==PlayModeStateChange.EnteredPlayMode&&SessionState.GetString(Key+"status","")=="running"){checks=0;deadline=EditorApplication.timeSinceStartup+240;flow=Flow();EditorApplication.update+=Tick;Application.logMessageReceived+=Log;}
if(s==PlayModeStateChange.EnteredEditMode&&SessionState.GetBool(Key+"cleanup",false)){SessionState.SetBool(Key+"cleanup",false);EditorApplication.update-=Tick;Application.logMessageReceived-=Log;EditorSettings.enterPlayModeOptions=(EnterPlayModeOptions)SessionState.GetInt(Key+"flags",0);EditorSettings.enterPlayModeOptionsEnabled=SessionState.GetBool(Key+"options",false);PersistenceTestGuard.ClearValidationOverrides();var scene=SessionState.GetString(Key+"scene","");if(!string.IsNullOrEmpty(scene))EditorSceneManager.OpenScene(scene);}}
static void Log(string m,string s,LogType t){if(t==LogType.Error||t==LogType.Exception)Finish(false,m+"\n"+s);}
static void Tick(){if(!EditorApplication.isPlaying)return;try{if(EditorApplication.timeSinceStartup>deadline)throw new TimeoutException("Caster model test timed out.");if(flow!=null&&!flow.MoveNext())Finish(true,null);}catch(Exception e){Finish(false,e.ToString());}}
static Bounds Body(Transform root){var b=new Bounds();bool first=true;var mesh=new Mesh();try{foreach(var r in root.GetComponentsInChildren<SkinnedMeshRenderer>().Where(r=>r.enabled)){r.BakeMesh(mesh);foreach(var v in mesh.vertices){var p=r.transform.TransformPoint(v);if(first){b=new Bounds(p,Vector3.zero);first=false;}else b.Encapsulate(p);}}}finally{UnityEngine.Object.Destroy(mesh);}return b;}
static float Shoulders(Animator a)=>Vector3.Distance(a.GetBoneTransform(HumanBodyBones.LeftUpperArm).position,a.GetBoneTransform(HumanBodyBones.RightUpperArm).position);
static IEnumerator Flow(){
while(DevelopmentRoomController.Current==null||DevelopmentRoomController.Current.player==null)yield return null;
var dev=DevelopmentRoomController.Current;var p=dev.player;while(!p.GetComponent<ManifestationPersistence>().Ready||dev.Map==null)yield return null;
p.GetComponent<TraversalInput>().enabled=false;p.GetComponent<CombatInput>().enabled=false;p.Health.Invulnerable=true;
var holder=new GameObject("Missing caster source fixture");holder.SetActive(false);
var fallback=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(MagicalTouchedAuthoring.MagicalPrefabPath),holder.transform);
var so=new SerializedObject(fallback.GetComponent<MagicallyTouchedVisual>());so.FindProperty("resourcePath").stringValue="MagicalLicensed/AbsentFixture";so.ApplyModifiedPropertiesWithoutUndo();holder.SetActive(true);
Need(!fallback.GetComponent<MagicallyTouchedVisual>().LicensedModelLoaded,"Source absence must retain fallback");
Need(ClassPresentationSwitcher.FindVisualAnimator(fallback.transform)?.avatar?.isValid==true,"Fallback avatar missing");
Need(fallback.GetComponentsInChildren<Renderer>().Any(r=>r.enabled),"Fallback invisible");UnityEngine.Object.Destroy(holder);
Need(dev.SelectClass(PhysicalBlessedAuthoring.ClassId),"Physical comparison selection");yield return null;yield return null;
var switcher=p.GetComponent<ClassPresentationSwitcher>();var physicalRoot=switcher.ActiveVisualRoot;var physicalAnimator=ClassPresentationSwitcher.FindVisualAnimator(physicalRoot);var physical=Body(physicalRoot);float physicalShoulders=Shoulders(physicalAnimator);
Need(dev.SelectClass(MagicalTouchedAuthoring.ClassId),"Caster selection");yield return null;yield return null;
var root=switcher.ActiveVisualRoot;var art=root.GetComponent<MagicallyTouchedVisual>();var driver=p.GetComponent<NovicePresentationDriver>();var a=driver.VisualAnimator;
Need(art!=null&&art.LicensedModelLoaded,"Caster source not loaded");Need(a==art.VisualAnimator&&a.avatar.isValid&&a.isHuman,"Wrong/invalid caster animator");
Need(!a.applyRootMotion&&root.GetComponentsInChildren<Collider>(true).Length==0,"Caster art owns physics");
Need(root.GetComponentsInChildren<MonoBehaviour>(true).All(b=>b!=null),"Missing script in derived caster");
Need(root.GetComponentsInChildren<Renderer>().Where(r=>r.enabled).All(r=>r.sharedMaterials.All(m=>m!=null&&m.shader.name=="Universal Render Pipeline/Lit")),"Unsupported material");
var body=Body(root);float shoulders=Shoulders(a);
Need(body.size.y<physical.size.y*.95f&&body.size.y>physical.size.y*.65f,"Caster is not clearly shorter than Physical");
Need(shoulders<physicalShoulders*.95f,"Caster shoulders are not narrower");
File.WriteAllText("Library/MagicalModelSize.json",JsonUtility.ToJson(new Sizes{magicalHeight=body.size.y,physicalHeight=physical.size.y,magicalShoulders=shoulders,physicalShoulders=physicalShoulders},true));
var staff=root.GetComponentsInChildren<Transform>(true).Single(t=>t.name=="Caster Staff");var cape=root.GetComponentsInChildren<Transform>(true).Single(t=>t.name=="PT_Male_Armor_01_A_cape");
Need(staff.gameObject.activeInHierarchy&&staff.GetComponentsInChildren<Renderer>().Length>0,"Default staff invisible");
Need(root.GetComponentsInChildren<SkinnedMeshRenderer>().Where(r=>r.enabled).Count()==7,"Unexpected cloth body composition");
float before=a.GetCurrentAnimatorStateInfo(0).normalizedTime;float until=Time.time+.4f;while(Time.time<until)yield return null;Need(a.GetCurrentAnimatorStateInfo(0).normalizedTime>before,"Idle stalled");
var start=p.transform.position;Need(p.Motor.MoveTo(start+Vector3.forward*3),"Move request");float speed=0;until=Time.time+1;while(Time.time<until){speed=Mathf.Max(speed,driver.LastDrivenSpeed);yield return null;}Need(Vector3.Distance(start,p.transform.position)>.5f&&speed>.1f,"Walking not driven");p.Motor.Stop();
driver.PlayAttackAnimation();until=Time.time+.12f;while(Time.time<until)yield return null;Need(a.GetCurrentAnimatorStateInfo(0).IsName("UnarmedAttack")||a.IsInTransition(0),"Attack not driven");
until=Time.time+1.5f;while(Time.time<until)yield return null;
var inv=p.GetComponent<CarriedInventory>();var eq=p.GetComponent<Equipment>();var ep=p.GetComponent<EquipmentPresentation>();
foreach(var d in inv.Definitions.Where(d=>d!=null&&d.modelTestingItem)){
var i=inv.Grant(d.stableId);Need(eq.Equip(i.instanceId,d.slot),"Equip "+d.slot);yield return null;
var visuals=ep.Bindings.Where(b=>b.definition==d).SelectMany(b=>b.visuals).Where(g=>g!=null&&g.transform.IsChildOf(root)).ToArray();
Need(visuals.Length>0&&visuals.All(g=>g.activeInHierarchy),"Missing fitting visual "+d.slot);
Need(visuals.SelectMany(g=>g.GetComponentsInChildren<PlainGearAttachment>()).All(att=>att.enabled),"Fitting attachment disabled "+d.slot);
}
Need(!staff.gameObject.activeSelf,"Fitting sword did not hide staff");Need(!cape.gameObject.activeSelf,"Fitting back did not hide cape");
foreach(var d in inv.Definitions.Where(d=>d!=null&&d.modelTestingItem))Need(eq.Unequip(d.slot.ToString()),"Unequip "+d.slot);
yield return null;Need(staff.gameObject.activeSelf&&cape.gameObject.activeSelf,"Default staff/cape did not restore");
var dagger=inv.Definitions.Single(d=>d.stableId=="item.bronze-dagger");var owned=inv.Grant(dagger.stableId);Need(eq.Equip(owned.instanceId,dagger.slot),"Bronze dagger equip");yield return null;
Need(ep.Bindings.Where(b=>b.definition==dagger).SelectMany(b=>b.visuals).Any(g=>g!=null&&g.transform.IsChildOf(a.transform)&&g.activeInHierarchy),"Bronze dagger did not follow caster rig");Need(!staff.gameObject.activeSelf,"Dagger and staff overlap");Need(eq.Unequip(dagger.slot.ToString()),"Dagger unequip");
yield return null;Need(staff.gameObject.activeSelf,"Staff did not restore after dagger");
Need(dev.SelectClass("class.novice"),"Novice selection");yield return null;Need(!root.gameObject.activeSelf,"Caster leaks into Novice");
Need(dev.SelectClass(MagicalTouchedAuthoring.ClassId),"Caster return");yield return null;Need(switcher.ActiveVisualRoot==root&&root.GetComponentsInChildren<MagicallyTouchedVisual>(true).Length==1,"Duplicate caster");
Need(p.Motor.Teleport(new Vector3(0,0,-8)),"Preview placement");p.transform.rotation=Quaternion.Euler(0,225,0);dev.explorationCamera.ConfigureZoom(5,5,1);dev.explorationCamera.RecenterForTravel(p.transform);
var portrait=p.GetComponent<HudPortraitRenderer>();portrait.RequestRefresh();yield return null;yield return null;
Need(portrait.Texture!=null&&portrait.Texture.IsCreated(),"Portrait missing");var studio=p.transform.Find("HUD Portrait Studio");Need(studio.GetComponentsInChildren<MagicallyTouchedVisual>(true).Single().LicensedModelLoaded,"Portrait caster not prepared");
var camera=studio.GetComponentInChildren<Camera>();var mesh=new Mesh();int outside=0;try{foreach(var skin in studio.GetComponentsInChildren<SkinnedMeshRenderer>().Where(r=>r.enabled)){skin.BakeMesh(mesh);foreach(var v in mesh.vertices){var q=camera.WorldToViewportPoint(skin.transform.TransformPoint(v));if(q.z<=0||q.x<.02f||q.x>.98f||q.y<.02f||q.y>.98f)outside++;}}}finally{UnityEngine.Object.Destroy(mesh);}
Need(outside==0,"Portrait crops body");
SessionState.SetInt(Key+"checks",checks);SessionState.SetString(Key+"status","visual_ready");while(!SessionState.GetBool(Key+"resume",false))yield return null;
}
static void Finish(bool ok,string error){flow=null;EditorApplication.update-=Tick;Application.logMessageReceived-=Log;SessionState.SetInt(Key+"checks",checks);SessionState.SetString(Key+"status",ok?"passed":"failed");SessionState.SetString(Key+"error",error??"");File.WriteAllText("Library/MagicalModelResults.json",JsonUtility.ToJson(new Record{passed=ok,checks=checks,error=error},true));EditorApplication.ExitPlaymode();}
[Serializable]class Record{public bool passed;public int checks;public string error;}
[Serializable]class Sizes{public float magicalHeight,physicalHeight,magicalShoulders,physicalShoulders;}
}}
