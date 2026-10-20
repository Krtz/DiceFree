using System;using System.IO;using System.Linq;using System.Collections;using System.Collections.Generic;
using DiceFree.Advancement;using DiceFree.Characters;using DiceFree.Combat;using DiceFree.Development;using DiceFree.Dungeons;using DiceFree.Foundation;using DiceFree.Gameplay;using DiceFree.Input;using DiceFree.Items;using DiceFree.Persistence;using DiceFree.Progression;using DiceFree.Quests;using DiceFree.Skills;using DiceFree.UI;using DiceFree.Visuals;using DiceFree.World;
using UnityEngine;using UnityEngine.SceneManagement;using UnityEditor;using UnityEditor.SceneManagement;using Unity.Pipeline.Commands;
using Object=UnityEngine.Object;using Kind=DiceFree.Progression.MageProgressionRules.Kind;
namespace DiceFree.EditorTools
{
 [InitializeOnLoad] public static class MageVfxValidation
 {
  const string Key="DiceFree.MageVfx.";static readonly Stack<IEnumerator> steps=new();static double deadline;static int checks;
  [Serializable] sealed class Report{public string status,error;public int checks,captures,peakParticles;public float maxEffectSpan;}
  static Report report;
  static MageVfxValidation()=>EditorApplication.playModeStateChanged+=Changed;
  static void Need(bool value,string text){if(!value)throw new InvalidOperationException(text);checks++;}
  static void Phase(string p){SessionState.SetString(Key+"phase",p);SessionState.SetInt(Key+"checks",checks);}
  [CliCommand("dicefree.mage.vfx.test","Verify selected Mage particle effects, actual casts, snapshots and VFX ownership cleanup.")]
  public static object Start(){
   if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play Mode.");
   SessionState.SetString(Key+"scene",EditorSceneManager.GetActiveScene().path);SessionState.SetBool(Key+"options",EditorSettings.enterPlayModeOptionsEnabled);SessionState.SetInt(Key+"flags",(int)EditorSettings.enterPlayModeOptions);
   SessionState.SetBool(Key+"cleanup",true);SessionState.SetBool(Key+"resume",false);SessionState.SetString(Key+"status","running");SessionState.SetString(Key+"phase","initializing");SessionState.SetString(Key+"error","");SessionState.SetInt(Key+"checks",0);
   var root=Path.Combine(Path.GetTempPath(),"DiceFree-Mage-"+Guid.NewGuid().ToString("N"));PersistenceTestGuard.UseIsolatedSaveRootForNextPlay(root);
   SessionState.SetString(Key+"root",root);EditorSettings.enterPlayModeOptionsEnabled=false;
   EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/Cornberg.unity");EditorApplication.EnterPlaymode();return Status();
  }
  [CliCommand("dicefree.mage.vfx.status","Read focused Mage verification.")]
  public static object Status()=>new{status=SessionState.GetString(Key+"status","idle"),phase=SessionState.GetString(Key+"phase",""),checks=SessionState.GetInt(Key+"checks",0),error=SessionState.GetString(Key+"error","")};
  [CliCommand("dicefree.mage.vfx.resume","Finish Mage verification after visual capture.")]
  public static object Resume(){SessionState.SetBool(Key+"resume",true);return Status();}
  static void Changed(PlayModeStateChange s){
   if(s==PlayModeStateChange.EnteredPlayMode&&SessionState.GetString(Key+"status","")=="running"){
    checks=0;report=new Report();steps.Clear();steps.Push(Flow());deadline=EditorApplication.timeSinceStartup+300;EditorApplication.update+=Tick;Application.logMessageReceived+=Log;
   }
   if(s==PlayModeStateChange.EnteredEditMode&&SessionState.GetBool(Key+"cleanup",false)){
    SessionState.SetBool(Key+"cleanup",false);EditorApplication.update-=Tick;Application.logMessageReceived-=Log;PersistenceTestGuard.ClearValidationOverrides();DevelopmentSession.End();
    EditorSettings.enterPlayModeOptions=(EnterPlayModeOptions)SessionState.GetInt(Key+"flags",0);EditorSettings.enterPlayModeOptionsEnabled=SessionState.GetBool(Key+"options",false);
    string path=SessionState.GetString(Key+"scene","");if(!string.IsNullOrWhiteSpace(path))EditorSceneManager.OpenScene(path);
   }
  }
  static void Log(string text,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception)Finish(false,text+"\n"+stack);}
  static void Tick(){if(!EditorApplication.isPlaying)return;try{
   if(EditorApplication.timeSinceStartup>deadline)throw new TimeoutException("Mage validation timed out in "+SessionState.GetString(Key+"phase",""));
   while(steps.Count>0){var step=steps.Peek();if(!step.MoveNext()){steps.Pop();continue;}if(step.Current is IEnumerator child){steps.Push(child);continue;}return;}
   Finish(true,null);
  }catch(Exception e){Finish(false,e.ToString());}}
  static void Finish(bool pass,string error){
   EditorApplication.update-=Tick;Application.logMessageReceived-=Log;report??=new Report();report.status=pass?"passed":"failed";report.error=error;report.checks=checks;
   SessionState.SetString(Key+"status",report.status);SessionState.SetString(Key+"error",error??"");SessionState.SetInt(Key+"checks",checks);
   File.WriteAllText("Library/MageVfxResults.json",JsonUtility.ToJson(report,true));EditorApplication.ExitPlaymode();
  }
  static IEnumerator Frames(int n){for(int i=0;i<n;i++)yield return null;}
  static IEnumerator Wait(float seconds){float until=Time.time+seconds;while(Time.time<until)yield return null;}
  static IEnumerator Flow(){
   CombatActor p=null;while(p==null){p=Object.FindAnyObjectByType<TraversalInput>()?.GetComponent<CombatActor>();yield return null;}
   while(!p.GetComponent<ManifestationPersistence>().Ready)yield return null;
   DevelopmentSession.Begin();yield return SceneManager.LoadSceneAsync("DevelopmentRoom");
   while(DevelopmentRoomController.Current==null||DevelopmentRoomController.Current.Map==null)yield return null;
   var room=DevelopmentRoomController.Current;p=room.player;while(!p.GetComponent<ManifestationPersistence>().Ready||!p.Motor.Ready)yield return null;
   Need(room.SelectClass(MageProgressionRules.ClassId),"Mage selection");yield return Frames(5);
   p.GetComponent<TraversalInput>().enabled=false;p.GetComponent<CombatInput>().enabled=false;p.GetComponent<BasicAttack>().enabled=false;
   p.Health.Invulnerable=true;GameplayPreferences.Current.SetAutoRetaliate(false);
   p.GetComponent<ExperienceProgression>().RestoreState(42,0);
   p.GetComponent<MageSkillProgression>().RestoreState(MageProgressionRules.Skills.Select(r=>new SkillRankState(MageSkillProgression.Id(r.kind),r.rankLevels.Length)).ToArray());
   var caster=p.GetComponent<MageSkillCaster>();var mana=p.GetComponent<ActorResourceController>();
   foreach(string key in MageSpellVfx.ResourceKeys){
    var prefab=Resources.Load<GameObject>("MageVfxLicensed/"+key);Need(prefab!=null,"Missing selected pack effect "+key);
    Need(prefab.GetComponentsInChildren<ParticleSystem>(true).Length>0,"No particles "+key);
    Need(prefab.GetComponentsInChildren<Collider>(true).Length==0&&prefab.GetComponentsInChildren<MonoBehaviour>(true).Length==0,"Publisher gameplay components "+key);
    Need(prefab.GetComponentsInChildren<Light>(true).Length==0,"Publisher point lights "+key);
    Need(prefab.GetComponentsInChildren<Renderer>(true).All(r=>r.sharedMaterials.All(m=>m!=null&&m.shader.isSupported&&!m.shader.name.Contains("InternalError"))),"Unsupported material "+key);
    if(key=="RadiationAura")Need(prefab.GetComponentsInChildren<ParticleSystem>(true).All(ps=>{var c=ps.main.startColor.color;return c.g>c.r&&c.g>c.b;}),"Radiation particle palette is not green");
    if(key=="SandLoop")Need(prefab.GetComponentsInChildren<ParticleSystem>(true).All(ps=>ps.main.startSize.constant<=.46f&&ps.main.startSpeed.constant<=.56f&&ps.main.startLifetime.constant<=1.21f),"Quicksand dust exceeds its presentation profile");
    if(key=="SandBurst")Need(prefab.GetComponentsInChildren<ParticleSystem>(true).All(ps=>ps.main.startSize.constant<=.66f&&ps.main.startSpeed.constant<=.51f),"Quicksand burst exceeds its presentation profile");
   }
   var targets=CombatActor.All.Where(a=>a!=p&&a.Stats.Definition.stableId=="dev.practice-target").ToArray();Need(targets.Length==3,"Targets");
   foreach(var t in targets){t.Stats.SetEncounterMaximumHp(100000);t.Health.Restore();t.Health.Invulnerable=false;t.Motor.Stop();if(t.TryGetComponent<BasicAttack>(out var basic))basic.enabled=false;t.Effects.ConfigureControl(0,false);}
   Need(p.Motor.Teleport(new Vector3(-10,0,-8))&&targets[0].Motor.Teleport(new Vector3(-10,0,-2))&&targets[1].Motor.Teleport(new Vector3(-7,0,0))&&targets[2].Motor.Teleport(new Vector3(-4,0,-1)),"Spell positions");
   room.explorationCamera.RecenterForTravel(p.transform);yield return Frames(8);
   int frame=Time.frameCount;yield return Wait(.5f);Need(Time.frameCount>frame+5,"Game frozen");
   var focus=new Vector3(-8,1,-4);
   var cameraGo=new GameObject("Temporary Mage VFX review camera");var review=cameraGo.AddComponent<Camera>();
   review.CopyFrom(Camera.main);review.enabled=false;review.orthographic=true;review.orthographicSize=6.5f;review.nearClipPlane=.1f;review.farClipPlane=80;
   review.transform.position=focus+new Vector3(9,11,-12);review.transform.LookAt(focus);
   try{
    Phase("Ember travel and impact");caster.ResetForManifestationLoad();mana.RefillAll();float hp=targets[0].Health.Current;
    Need(caster.Cast(Kind.EmberBolt,targets[0]),caster.Feedback);yield return Wait(.12f);
    Need(Mathf.Abs(hp-targets[0].Health.Current)<.01f,"VFX applied early damage");Capture(review,"Ember-Travel");
    Need(Object.FindObjectsByType<MageSpellVfx>().Any(v=>v.name.Contains("EmberProjectile")),"No travelling fireball");
    yield return Wait(.35f);Need(targets[0].Health.Current<hp,"Projectile impact damage missing");Capture(review,"Ember-Impact");CaptureGameplay("Ember-Impact");
    yield return Wait(.9f);
    Phase("Frost and chain lightning");caster.ResetForManifestationLoad();mana.RefillAll();
    Need(caster.Cast(Kind.FrostSnare,targets[0]),caster.Feedback);Need(targets[0].Effects.Stunned,"Freeze changed");yield return Wait(.18f);Capture(review,"Frost");CaptureGameplay("Frost");
    caster.ResetForManifestationLoad();yield return Frames(2);mana.RefillAll();
    var before=targets.Select(t=>t.Health.Current).ToArray();Need(caster.Cast(Kind.ArcLightning,targets[0]),caster.Feedback);yield return Wait(.06f);
    Need(targets.Select((t,i)=>t.Health.Current<before[i]).All(v=>v),"Chain damage changed");
    Need(Object.FindObjectsByType<LineRenderer>().Any(l=>l.name=="White-hot lightning core"&&l.positionCount>=5),"Straight or missing lightning");
    Capture(review,"Arc-Lightning");CaptureGameplay("Arc-Lightning");yield return Wait(.4f);
    Phase("Ground field and radioactive affliction");caster.ResetForManifestationLoad();mana.RefillAll();
    Need(caster.CastQuicksand(targets[0].transform.position),caster.Feedback);yield return Wait(.35f);Capture(review,"Quicksand");CaptureGameplay("Quicksand");
    var ring=Object.FindObjectsByType<LineRenderer>().Single(l=>l.name=="True Quicksand radius");
    Need(Mathf.Abs(ring.GetPosition(0).magnitude-caster.Tuning.quicksandRadius)<.01f,"Visual/gameplay radius mismatch");
    caster.ResetForManifestationLoad();yield return Frames(2);mana.RefillAll();Need(caster.Cast(Kind.HalfLife,targets[0]),caster.Feedback);
    yield return Wait(.35f);Capture(review,"Half-Life");CaptureGameplay("Half-Life");var aura=Object.FindObjectsByType<MageSpellVfx>().SingleOrDefault(v=>v.name=="Mage VFX - RadiationAura");Need(aura!=null,"No persistent radiation");
    Need(aura.transform.parent==targets[0].transform&&aura.gameObject.scene==targets[0].gameObject.scene,"Radiation presentation is not parented to its target scene");
    Need(Object.FindObjectsByType<LineRenderer>().Count(l=>l.name.StartsWith("Radiation "))>=3,"Radiation palette/orbit readability treatment missing");
    // A refresh replaces its old visual immediately rather than stacking particle emitters.
    caster.ResetForManifestationLoad();yield return Frames(2);mana.RefillAll();Need(caster.Cast(Kind.HalfLife,targets[0]),caster.Feedback);
    yield return Wait(.1f);targets[0].Health.ApplyDamage(p,new DamageResult{mitigated=200000});yield return Frames(3);
    Need(!Object.FindObjectsByType<MageSpellVfx>().Any(v=>v.name=="Mage VFX - RadiationAura"),"Radiation remains on corpse");
    targets[0].Health.Restore();caster.ResetForManifestationLoad();yield return Frames(2);
    Phase("Math shield and ownership cleanup");mana.RefillAll();Need(caster.Cast(Kind.MathBarrier),caster.Feedback);yield return Wait(.25f);Capture(review,"Math-Barrier");CaptureGameplay("Math-Barrier");
    var barrier=Object.FindObjectsByType<MageSpellVfx>().SingleOrDefault(v=>v.name=="Mage VFX - MathShield");Need(barrier!=null&&barrier.transform.parent==p.transform&&barrier.gameObject.scene==p.gameObject.scene,"Math Barrier presentation is not parented to its owner scene");
    Need(Object.FindObjectsByType<LineRenderer>().Count(l=>l.name=="Math Barrier orbit")==3,"Geometric math shell missing");
    Need(caster.CastQuicksand(targets[0].transform.position)&&caster.Cast(Kind.HalfLife,targets[0]),"Concurrent fields");yield return Frames(3);
    var active=Object.FindObjectsByType<MageSpellVfx>();
    Need(active.All(v=>v.GetComponentsInChildren<Collider>(true).Length==0),"Visual creates collision");
    Need(active.All(v=>v.GetComponentsInChildren<Light>(true).Length==0),"Visual creates dynamic point lights");
    caster.enabled=false;yield return Frames(3);Need(Object.FindObjectsByType<MageSpellVfx>().Length==0,"VFX leaked after caster disable");caster.enabled=true;
    mana.RefillAll();Need(caster.Cast(Kind.MathBarrier),caster.Feedback);caster.ResetForManifestationLoad();yield return Frames(3);
    Need(Object.FindObjectsByType<MageSpellVfx>().Length==0,"VFX leaked on manifestation reset");
    // Return to a lower-tier form while a fireball is in flight.
    mana.RefillAll();Need(caster.Cast(Kind.EmberBolt,targets[0]),caster.Feedback);caster.ResetForManifestationLoad();yield return Wait(.8f);
    Need(Object.FindObjectsByType<MageSpellVfx>().Length==0,"Reset projectile or impact leaked");
   }finally{Object.Destroy(cameraGo);}
  }
  static void Capture(Camera camera,string name){
   var target=RenderTexture.GetTemporary(1024,640,24);var old=camera.targetTexture;var active=RenderTexture.active;var picture=new Texture2D(1024,640,TextureFormat.RGB24,false);
   try{
    camera.targetTexture=target;camera.Render();RenderTexture.active=target;picture.ReadPixels(new Rect(0,0,1024,640),0,0);picture.Apply();
    Need(picture.GetPixels().Average(c=>c.grayscale)>.02f,"Blank spell capture");File.WriteAllBytes("Library/MageVfx-"+name+".png",picture.EncodeToPNG());report.captures++;
    int particles=Object.FindObjectsByType<ParticleSystem>().Sum(p=>p.particleCount);report.peakParticles=Mathf.Max(report.peakParticles,particles);
    var fx=Object.FindObjectsByType<MageSpellVfx>();
    foreach(var root in fx.Where(v=>v.transform.parent==null)){
     var renderers=root.GetComponentsInChildren<Renderer>().Where(r=>r.enabled).ToArray();if(renderers.Length==0)continue;
     var bounds=renderers[0].bounds;foreach(var r in renderers.Skip(1))bounds.Encapsulate(r.bounds);
     report.maxEffectSpan=Mathf.Max(report.maxEffectSpan,Mathf.Max(bounds.size.x,bounds.size.y,bounds.size.z));
    }
   }finally{camera.targetTexture=old;RenderTexture.active=active;RenderTexture.ReleaseTemporary(target);Object.Destroy(picture);}
  }
  static void CaptureGameplay(string name){
   var camera=Camera.main;Need(camera!=null,"Gameplay camera unavailable for Mage VFX capture");Capture(camera,"Gameplay-"+name);
  }

 }
}
