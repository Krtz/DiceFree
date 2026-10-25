using System; using System.IO; using System.Linq; using System.Collections; using System.Collections.Generic;
using DiceFree.Core; using DiceFree.Characters; using DiceFree.Combat; using DiceFree.Persistence; using DiceFree.World;
using DiceFree.UI;
using UnityEngine; using UnityEngine.AI; using UnityEngine.InputSystem; using UnityEngine.InputSystem.LowLevel; using UnityEditor; using UnityEditor.SceneManagement; using Unity.Pipeline.Commands;
using Object = UnityEngine.Object;
namespace DiceFree.EditorTools
{
 [InitializeOnLoad] public static class TrimeryHealerValidation
 {
  const string Key="DiceFree.TrimeryHealer."; static readonly Stack<IEnumerator> steps=new(); static double deadline; static int checks;
  [Serializable] sealed class Report { public string status,error; public int checks,walked,frames; public float receivedHealing; public bool routedClick,clicked,nearest; }
  static Report report;
  static Mouse mouse; static InputSettings.BackgroundBehavior background; static InputSettings.EditorInputBehaviorInPlayMode editorInput;
  static TrimeryHealerValidation()=>EditorApplication.playModeStateChanged+=Changed;
  static void Need(bool condition,string message){if(!condition)throw new InvalidOperationException(message); checks++;}
  [CliCommand("dicefree.trimery.healer.test","Isolated live walking, click, keyboard-path, treatment and safety checks.")]
  public static object Start(){
   if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Edit Mode only");
   for(int i=0;i<UnityEngine.SceneManagement.SceneManager.sceneCount;i++)
    if(UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty)throw new InvalidOperationException("Save modified scenes first");
   SessionState.SetString(Key+"scene",EditorSceneManager.GetActiveScene().path);
   SessionState.SetBool(Key+"options",EditorSettings.enterPlayModeOptionsEnabled);SessionState.SetInt(Key+"flags",(int)EditorSettings.enterPlayModeOptions);
   SessionState.SetBool(Key+"background",Application.runInBackground);
   SessionState.SetBool(Key+"cleanup",true);SessionState.SetString(Key+"status","running");SessionState.SetString(Key+"error","");SessionState.SetInt(Key+"checks",0);
   string root=Path.Combine(Path.GetTempPath(),"DiceFree-Healer-"+Guid.NewGuid().ToString("N"));
   PersistenceTestGuard.UseIsolatedSaveRootForNextPlay(root);EditorSettings.enterPlayModeOptionsEnabled=false;
   EditorSceneManager.OpenScene(TrimeryVillageAuthoring.ScenePath);EditorApplication.EnterPlaymode();return Status();
  }
  [CliCommand("dicefree.trimery.healer.status","Read Trimery healer test result.")]
  public static object Status()=>new{status=SessionState.GetString(Key+"status","idle"),checks=SessionState.GetInt(Key+"checks",0),error=SessionState.GetString(Key+"error","")};
  static void Changed(PlayModeStateChange state){
   if(state==PlayModeStateChange.EnteredPlayMode&&SessionState.GetString(Key+"status","")=="running"){
    Application.runInBackground=true;
    background=InputSystem.settings.backgroundBehavior;editorInput=InputSystem.settings.editorInputBehaviorInPlayMode;
    InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
    InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
    mouse=InputSystem.AddDevice<Mouse>();
    checks=0;report=new Report();steps.Clear();steps.Push(Flow());deadline=EditorApplication.timeSinceStartup+180;
    EditorApplication.update+=Tick;Application.logMessageReceived+=Log;
   }
   if(state==PlayModeStateChange.EnteredEditMode&&SessionState.GetBool(Key+"cleanup",false)){
    SessionState.SetBool(Key+"cleanup",false);EditorApplication.update-=Tick;Application.logMessageReceived-=Log;
    Application.runInBackground=SessionState.GetBool(Key+"background",false);
    PersistenceTestGuard.ClearValidationOverrides();EditorSettings.enterPlayModeOptions=(EnterPlayModeOptions)SessionState.GetInt(Key+"flags",0);
    EditorSettings.enterPlayModeOptionsEnabled=SessionState.GetBool(Key+"options",false);
    var scene=SessionState.GetString(Key+"scene","");if(!string.IsNullOrWhiteSpace(scene))EditorSceneManager.OpenScene(scene);
   }
  }
  static void Log(string text,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception)Finish(false,text+"\n"+stack);}
  static void Tick(){if(!EditorApplication.isPlaying)return;try{
   if(EditorApplication.timeSinceStartup>deadline)throw new TimeoutException("Healer test timeout");
   while(steps.Count>0){var step=steps.Peek();if(!step.MoveNext()){steps.Pop();continue;}if(step.Current is IEnumerator child){steps.Push(child);continue;}return;}Finish(true,null);
  }catch(Exception e){Finish(false,e.ToString());}}
  static void Finish(bool pass,string error){
   EditorApplication.update-=Tick;Application.logMessageReceived-=Log;report??=new Report();report.status=pass?"passed":"failed";report.error=error;report.checks=checks;
   if(mouse!=null&&mouse.added)InputSystem.RemoveDevice(mouse);mouse=null;
   InputSystem.settings.backgroundBehavior=background;InputSystem.settings.editorInputBehaviorInPlayMode=editorInput;
   SessionState.SetString(Key+"status",report.status);SessionState.SetString(Key+"error",error??"");SessionState.SetInt(Key+"checks",checks);
   File.WriteAllText("Library/TrimeryHealerResults.json",JsonUtility.ToJson(report,true));EditorApplication.ExitPlaymode();
  }
  static IEnumerator Wait(float seconds){float end=Time.time+seconds;while(Time.time<end)yield return null;}
  static IEnumerator Walk(CombatActor actor,Vector3 point){
   Need(NavMesh.SamplePosition(point,out var hit,2,1),"No walkable healer approach "+point);
   Need(Vector3.Distance(hit.position,point)<1,"Healer approach snapped too far");
   Need(actor.Motor.MoveTo(hit.position),"Walk order rejected");
   float end=Time.time+15;while(Vector3.Distance(actor.transform.position,hit.position)>.65f&&Time.time<end)yield return null;
   Need(Vector3.Distance(actor.transform.position,hit.position)<=.65f,"Could not walk to healer point "+point);report.walked++;
  }
  static void Capture(Camera camera){
   var target=RenderTexture.GetTemporary(960,600,24);var old=camera.targetTexture;var active=RenderTexture.active;var image=new Texture2D(960,600,TextureFormat.RGB24,false);
   try{camera.targetTexture=target;camera.Render();RenderTexture.active=target;image.ReadPixels(new Rect(0,0,960,600),0,0);image.Apply();
    Need(image.GetPixels().Average(c=>c.grayscale)>.02f,"Healer capture blank");File.WriteAllBytes("Library/Trimery-Healer.png",image.EncodeToPNG());
   }finally{camera.targetTexture=old;RenderTexture.active=active;RenderTexture.ReleaseTemporary(target);Object.Destroy(image);}
  }
  static IEnumerator Flow(){
   CombatActor p=null;while(p==null){p=Object.FindAnyObjectByType<TraversalInput>()?.GetComponent<CombatActor>();yield return null;}
   var save=p.GetComponent<ManifestationPersistence>();while(!save.Ready||!p.Motor.Ready)yield return null;
   var controls=p.GetComponent<TraversalInput>();controls.enabled=true;p.GetComponent<CombatInput>().enabled=false;p.GetComponent<BasicAttack>().enabled=false;p.Health.Invulnerable=true;
   var healers=Object.FindObjectsByType<HealerInteraction>();Need(healers.Length==1,"Expected exactly one healer");var healer=healers[0];
   Need(healer.GetComponent<CombatActor>()==null,"Healer must not be hostile/combat target");
   Need(healer.GetComponent<Collider>().gameObject.layer==10,"Not click selectable");
   Need(healer.GetComponentsInChildren<Collider>().All(c=>c.gameObject.layer!=9),"Healer blocks world navigation");
   Need(healer.GetComponentsInChildren<Renderer>().All(r=>r.sharedMaterial!=null&&r.sharedMaterial.shader.isSupported),"Unsupported healer material");
   var registry=p.GetComponent<InteractionRegistry>();Need(registry.Contains(healer),"Healer absent from interaction registry");
   var playerCollider=p.GetComponent<CapsuleCollider>();Need(playerCollider!=null&&playerCollider.enabled&&!playerCollider.isTrigger,"Player gameplay collision capsule missing or invalid");
   Need(NavMesh.SamplePosition(CornbergLandscape.Ground(248,181),out var start,3,1),"Village arrival unreachable");
   Need(p.Motor.Teleport(start.position),"Village relocation");
    int frame=Time.frameCount;yield return Wait(.5f);report.frames=Time.frameCount-frame;Need(report.frames>1,"Runtime not advancing");
   var destination=CornbergLandscape.Ground(248,185);var screen=controls.WorldCamera.WorldToScreenPoint(destination);
   Need(screen.z>0&&screen.x>=0&&screen.x<Screen.width&&screen.y>=0&&screen.y<Screen.height,"Click-to-move destination is outside the live camera view");
   Need(!HudPointerBlocker.Covers(screen),"Click-to-move test point is covered by gameplay UI");
   var groundRay=controls.WorldCamera.ScreenPointToRay(screen);
   Need(Physics.Raycast(groundRay,out var groundHit,1500,(1<<8)|(1<<9),QueryTriggerInteraction.Ignore)&&groundHit.collider.gameObject.layer==8,"Click-to-move point does not hit navigable ground");
   var priorPosition=p.transform.position;InputSystem.QueueStateEvent(mouse,new MouseState{position=screen,buttons=2});InputSystem.Update();controls.SendMessage("Update");
   Need(p.Motor.Travelling,"Synthetic right-click did not reach TraversalInput and start a NavMesh route");
   InputSystem.QueueStateEvent(mouse,new MouseState{position=screen,buttons=0});InputSystem.Update();
   float moveEnd=Time.time+5;while(Vector3.Distance(priorPosition,p.transform.position)<.5f&&Time.time<moveEnd)yield return null;
   Need(Vector3.Distance(priorPosition,p.transform.position)>.5f,"Click-to-move path was accepted but the player did not move");report.routedClick=true;
   yield return Walk(p,destination);
   foreach(var point in new[]{new Vector2(248,185),new Vector2(248,190),new Vector2(246,190)})
    yield return Walk(p,CornbergLandscape.Ground(point.x,point.y));
   Need(healer.CanInteract(p),"Cannot interact at road approach");
   var interactor=p.GetComponent<Interactor>();p.Health.SetEncounterHp(p.Health.Maximum*.25f);
   float received=0;Action<float> onHeal=amount=>received+=amount;p.Health.Healed+=onHeal;
   try{
    var ray=new Ray(healer.transform.position+Vector3.up*6,Vector3.down);
    Need(interactor.ContextInteract(ray),"Healer right-click path rejected");report.clicked=true;yield return Wait(.1f);
    Need(interactor.Active==healer,"Click selected wrong service");
    Need(Mathf.Abs(p.Health.Current-p.Health.Maximum)<.01f,"Healer failed to restore HP");
    Need(received>p.Health.Maximum*.5f,"No visible heal feedback event");report.receivedHealing=received;
    p.Health.SetEncounterHp(p.Health.Maximum*.4f);
    Need(registry.NearestInRange(p)==healer,"Nearest interaction selects wrong NPC");
    Need(interactor.InteractNearest(),"Keyboard interaction path rejected");report.nearest=true;yield return Wait(.1f);
    Need(Mathf.Abs(p.Health.Current-p.Health.Maximum)<.01f,"Nearest treatment failed");
    float full=received;healer.Interact(p);Need(Mathf.Approximately(received,full),"Full HP produces spurious heal events");
    p.Health.AddTemporaryShield("healer-test",17,20);healer.Interact(p);Need(p.Health.TemporaryShield==17,"Treatment clears shields");
    p.Health.RemoveTemporaryShield("healer-test");
    p.Configure(1,p.Radius);Need(!healer.Available(p),"Enemy eligible for player service");p.Configure(0,p.Radius);
    healer.enabled=false;Need(!registry.Contains(healer)&&!healer.CanInteract(p),"Disabled healer remains active");
    healer.enabled=true;Need(registry.Contains(healer),"Healer does not re-register");
    yield return Walk(p,CornbergLandscape.Ground(248,198));
    p.Health.SetEncounterHp(p.Health.Maximum*.5f);float before=p.Health.Current;healer.Interact(p);
    Need(Mathf.Approximately(before,p.Health.Current),"Out-of-range treatment");
    yield return Wait(.1f);Need(p.Health.Current<p.Health.Maximum*.75f,"Healer has unintended passive healing area");
    yield return Walk(p,CornbergLandscape.Ground(246,190));
    var clock=Object.FindAnyObjectByType<WorldDayNightCycle>();clock?.SetHour(10,false);
    Object.FindAnyObjectByType<ExplorationCamera>().RecenterForTravel(p.transform);yield return Wait(.2f);Capture(Camera.main);
    // Real death packet; the isolated profile cannot affect the user's characters.
    p.Health.Invulnerable=false;p.Health.ApplyDamage(null,new DamageResult{mitigated=p.Health.Maximum*10});
    Need(!p.Alive,"Death fixture failed");Need(!healer.CanInteract(p),"Dead player eligible");healer.Interact(p);Need(!p.Health.Alive,"Treatment resurrects dead player");
   }finally{p.Health.Healed-=onHeal;}
  }
 }
}
