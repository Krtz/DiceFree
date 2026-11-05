using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using DiceFree.Advancement;
using DiceFree.AI;
using DiceFree.Briarback;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Core;
using DiceFree.Items;
using DiceFree.Persistence;
using DiceFree.Progression;
using DiceFree.Quests;
using DiceFree.UI;
using DiceFree.Visuals;
using DiceFree.World;
using UnityEngine;
using UnityEngine.AI;
using Object=UnityEngine.Object;
public static class HandPoseWolfReview
{
 static int checks;static string phase;static readonly List<object> rows=new();
 static void Need(bool value,string why){checks++;if(!value)throw new InvalidOperationException(why);}
 static async Task Until(Func<bool> test,string why){var end=DateTime.UtcNow.AddSeconds(30);while(!test()){if(DateTime.UtcNow>end)throw new TimeoutException(why);await Task.Delay(20);}}
 static async Task AfterPoseFrame()
 {
  var ready=new TaskCompletionSource<bool>();
  void Complete(UnityEngine.Rendering.ScriptableRenderContext context,List<Camera> cameras)=>ready.TrySetResult(true);
  UnityEngine.Rendering.RenderPipelineManager.endContextRendering+=Complete;
  _=Task.Delay(5000).ContinueWith(_=>ready.TrySetException(new TimeoutException("Rendered pose frame")));
  try{await ready.Task;}finally{UnityEngine.Rendering.RenderPipelineManager.endContextRendering-=Complete;}
 }
 static void Move(CombatActor a,Vector3 p){Need(NavMesh.SamplePosition(p,out var hit,2,NavMesh.AllAreas)&&a.Motor.Teleport(hit.position),"Fixture placement "+p);}
 static long TotalXp(ExperienceProgression xp){long sum=xp.CurrentXp;for(int i=1;i<xp.Level;i++)sum+=30+3L*(i-1)*(i-1);return sum;}
 public static string Inspect()=>Newtonsoft.Json.JsonConvert.SerializeObject(CombatActor.All.GroupBy(a=>a.Stats.Definition.familyId).Select(g=>new{family=g.Key,count=g.Count()}));
 public static string AttackState(){var p=Object.FindAnyObjectByType<TraversalInput>().GetComponent<CombatActor>();var b=p.GetComponent<BasicAttack>();return Newtonsoft.Json.JsonConvert.SerializeObject(new{b.enabled,b.State,b.Hits,b.Misses,p.CanAct,p.Alive,position=p.transform.position.ToString(),target=b.Target?.transform.position.ToString(),inRange=b.Target!=null&&b.InRange(b.Target),sight=b.Target!=null&&p.HasSightOf(b.Target),travelling=p.Motor.Travelling});}
 public static string PoseState(){var p=Object.FindAnyObjectByType<TraversalInput>().GetComponent<CombatActor>();var root=p.GetComponent<ClassPresentationSwitcher>().ActiveVisualRoot;var a=p.GetComponent<NovicePresentationDriver>().VisualAnimator;var eq=p.GetComponent<Equipment>();var pose=a.GetComponent<IndependentOffhandPose>();return Newtonsoft.Json.JsonConvert.SerializeObject(new{classId=p.Stats.Definition.stableId,root=root.name,animator=a.name,pose=pose!=null,poseEnabled=pose?.enabled,allPoses=root.GetComponentsInChildren<IndependentOffhandPose>(true).Select(x=>new{x.name,x.enabled}),stance=eq.UsesTwoHandedStance,offhand=eq.ResolvedDefinition(EquipmentSlot.OffHand)?.stableId,gap=Vector3.Distance(a.GetBoneTransform(HumanBodyBones.LeftHand).position,a.GetBoneTransform(HumanBodyBones.RightHand).position),left=a.GetBoneTransform(HumanBodyBones.LeftHand).position.ToString(),right=a.GetBoneTransform(HumanBodyBones.RightHand).position.ToString()});}
 public static async Task<string> HudProbe()
 {
  var p=Object.FindAnyObjectByType<TraversalInput>().GetComponent<CombatActor>();var eq=p.GetComponent<Equipment>();var inv=p.GetComponent<CarriedInventory>();Need(eq.Equip(inv.Items.First(i=>i.definitionId==WolfGreatswordVisual.ItemId).instanceId,EquipmentSlot.MainHand),"Probe equip");await Task.Delay(300);await AfterPoseFrame();
  var hud=p.GetComponent<HudPortraitRenderer>();var clone=(GameObject)typeof(HudPortraitRenderer).GetField("clone",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(hud);var camera=(Camera)typeof(HudPortraitRenderer).GetField("portraitCamera",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(hud);SaveTexture(hud.Texture,"Logs/HandPoseHudProbe.png");
  return Newtonsoft.Json.JsonConvert.SerializeObject(new{camera=camera.transform.position.ToString(),rotation=camera.transform.eulerAngles.ToString(),animators=clone.GetComponentsInChildren<Animator>(true).Select(a=>new{a.name,a.enabled,a.isActiveAndEnabled}),renderers=clone.GetComponentsInChildren<Renderer>().Select(r=>new{r.name,r.enabled,bounds=r.bounds.ToString(),view=camera.WorldToViewportPoint(r.bounds.center).ToString()}),wolfAnimators=clone.GetComponentsInChildren<WolfGreatswordPose>(true).Select(w=>new{w.name,insideClone=((Animator)typeof(WolfGreatswordPose).GetField("animator",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(w)).transform.IsChildOf(clone.transform)})});
 }
 public static async Task<string> Probe()
 {
  var p=Object.FindAnyObjectByType<TraversalInput>().GetComponent<CombatActor>();var target=CombatActor.All.First(a=>a.Stats.Definition.stableId=="briarback.raider");var b=p.GetComponent<BasicAttack>();b.enabled=true;b.Cancel();Move(p,new Vector3(270,0,415));Move(target,p.transform.position+Vector3.forward*(p.Radius+target.Radius+.2f));b.Order(target);var sample=new List<object>();for(int i=0;i<10;i++){await Task.Delay(200);sample.Add(new{b.State,b.Hits,b.Misses,b.enabled,p.CanAct,p.Alive,inRange=b.InRange(target),sight=p.HasSightOf(target),position=p.transform.position.ToString(),target=target.transform.position.ToString(),speed=p.Stats.AttackSpeed});}return Newtonsoft.Json.JsonConvert.SerializeObject(sample);
 }
 public static async Task<string> Run()
 {
  checks=0;rows.Clear();string failure=null;var camera=Object.FindAnyObjectByType<ExplorationCamera>();
  try{
   phase="isolated setup";CombatActor p=null;await Until(()=>(p=Object.FindAnyObjectByType<TraversalInput>()?.GetComponent<CombatActor>())!=null&&p.Motor.Ready&&p.GetComponent<ManifestationPersistence>().Ready,"Player ready");
   p.GetComponent<TraversalInput>().enabled=false;p.GetComponent<CombatInput>().enabled=false;p.Health.Invulnerable=true;
   foreach(var b in Object.FindObjectsByType<MonoBehaviour>().Where(b=>b is AggroBehaviour||b is BasicAttack||b is QuilboarCombat||b is BriarbackChieftainEncounter))b.enabled=false;
   foreach(var a in CombatActor.All)if(a.Motor!=null&&a.Motor.Ready)a.Motor.Stop();
   var save=p.GetComponent<ManifestationPersistence>();var catalog=(ClassCatalog)typeof(ManifestationPersistence).GetField("classCatalog",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(save);
   bool Activate(string id)=>save.CurrentClassId==id||save.TryActivateExisting(id,out _)||save.TryForkAndActivate(catalog.Resolve(id),out _);
   var journal=p.GetComponent<QuestJournal>();var wolfQuest=journal.Definitions.Single(q=>q.stableId=="quest.cornberg.repeat-wolf-hunt");var giver=Object.FindObjectsByType<QuestGiver>().Single(g=>g.Offers.Contains(wolfQuest));
   phase="Mira wolf offer and two repeat cycles";Need(wolfQuest.stages[0].count==15&&wolfQuest.repeatable&&wolfQuest.rewardXp==2920,"Wolf objective/reward");Need(journal.CanAccept(wolfQuest)&&wolfQuest.completedQuestIds.Length==0,"Wolf quest still gated by Slime Surge");
   // UI-only fixture: another active offer must not hide the repeatable hunt.
   var dialogueState=journal.CaptureState();var primary=dialogueState.Single(q=>q.questId==giver.Quest.stableId);primary.status=QuestStatus.Active;primary.stage=primary.count=0;journal.RestoreState(dialogueState);
   var ordinary=CombatActor.All.Where(a=>a!=p&&a.Stats.Definition.familyId=="ironpine.wolf").ToArray();var alpha=CombatActor.All.FirstOrDefault(a=>a.Stats.Definition.familyId=="ironpine.alpha-wolf");Need(ordinary.Length>0&&alpha!=null,"Mixed wolf fixture roster");
   // Repeated fixtures restore a new life on existing actors; no extra scene wolves are spawned.
   var wolves=Enumerable.Range(0,15).Select(i=>i==14?alpha:ordinary[i%ordinary.Length]).ToArray();
   var xp=p.GetComponent<ExperienceProgression>();
   for(int cycle=0;cycle<2;cycle++){
    Move(p,giver.ApproachPosition);var interaction=p.GetComponent<Interactor>();Need(interaction.Order(giver),"Mira dialogue order");await Until(()=>interaction.Active==giver,"Mira dialogue");var panel=Object.FindAnyObjectByType<QuestInteractionPanel>();Need(panel.OfferChoices.Length>=2&&panel.OfferChoices.Contains(wolfQuest)&&panel.SelectOffer(wolfQuest)&&panel.SelectedQuest==wolfQuest,"Wolf offer cannot be selected alongside the active main quest");if(cycle==0)await Capture(p,"wolf-bounty",false);Need(giver.Accept(p,wolfQuest),"Mira wolf accept");interaction.Cancel();
    for(int i=0;i<15;i++){
     var target=wolves[i];target.Health.Restore();target.Health.Invulnerable=false;Move(p,target.transform.position+Vector3.back*2);target.Health.ApplyPacket(p,new DamageResult{mitigated=1000000});Need(journal.GetProgress(wolfQuest.stableId).count==i+1,"Shared regular/alpha wolf count");
     if(cycle==1&&i==3){Need(save.Flush(),"Mid-cycle wolf save");Need(Activate("class.physically-blessed-novice")&&Activate("class.novice"),"Wolf quest reload");Need(journal.GetProgress(wolfQuest.stableId).count==4&&journal.GetProgress(wolfQuest.stableId).status==QuestStatus.Active,"Wolf progress lost on reload");}
    }
    Need(journal.GetProgress(wolfQuest.stableId).status==QuestStatus.ReadyToTurnIn,"Wolf completion");Move(p,giver.ApproachPosition);long before=TotalXp(xp);Need(giver.TurnIn(p,wolfQuest),"Wolf turn-in");Need(TotalXp(xp)-before==2920,"Exact wolf turn-in XP");Need(!giver.TurnIn(p,wolfQuest)&&journal.GetProgress(wolfQuest.stableId).status==QuestStatus.Available,"Wolf duplicate reward/reset");
   }
   rows.Add(new{wolfQuest=wolfQuest.stableId,npc=giver.name,killsPerCycle=15,cycles=2,xpPerTurnIn=2920,mixedRegularAndAlpha=true,midCycleReload=true});
   phase="five-class hand poses";var dummy=CombatActor.All.First(a=>a.Stats.Definition.stableId=="briarback.raider");dummy.Stats.SetEncounterMaximumHp(1000000);dummy.Health.Restore();
   foreach(string id in new[]{"class.novice","class.physically-blessed-novice","class.greatblade","class.magically-touched-novice","class.mage"}){
    Need(Activate(id),"Class fixture "+id);var motion=p.GetComponent<PlayerMotionPresentation>();await Until(()=>motion.ControllerClassId==id,"Class presentation");
    var eq=p.GetComponent<Equipment>();var inv=p.GetComponent<CarriedInventory>();eq.Restore(Array.Empty<EquippedItem>());var sword=inv.Grant("item.plain.mainhand");var buckler=inv.Grant("item.plain.offhand");var two=inv.Grant(WolfGreatswordVisual.ItemId);
    foreach(bool both in new[]{false,true,false}){
     p.GetComponent<BasicAttack>().Cancel();Move(p,new Vector3(270,0,415));Move(dummy,p.transform.position+Vector3.forward*2.8f);
     Need(eq.Equip(both?two.instanceId:sword.instanceId,EquipmentSlot.MainHand),"Main equip "+id);if(!both)Need(eq.Equip(buckler.instanceId,EquipmentSlot.OffHand),"Offhand equip "+id);motion.RefreshForEquipment();await Task.Delay(220);
     await AfterPoseFrame();var model=p.GetComponent<NovicePresentationDriver>().VisualAnimator;var left=model.GetBoneTransform(HumanBodyBones.LeftHand);var right=model.GetBoneTransform(HumanBodyBones.RightHand);var root=p.GetComponent<ClassPresentationSwitcher>().ActiveVisualRoot;
     Need(eq.UsesTwoHandedStance==both&&motion.UsesTwoHandedStance==both,"Resolved stance "+id);if(both)Need(eq.ResolvedDefinition(EquipmentSlot.OffHand)==null,"Two-hand left an offhand");
     else{
      var before=left.position;var rotation=left.rotation;
      foreach(var hidden in root.GetComponentsInChildren<WolfGreatswordPose>(true).Where(x=>!x.gameObject.activeInHierarchy))hidden.ApplyPortraitPose();
      foreach(var grip in root.GetComponentsInChildren<GreatbladeTwoHandGrip>(true))grip.ApplyPose();
      Need(Vector3.Distance(before,left.position)<.0001f&&Quaternion.Angle(rotation,left.rotation)<.01f,"Hidden two-hand helper changed the independent hand "+id);
      Need(Vector3.Distance(left.position,right.position)>.22f,"Independent hands still clasped "+id);
      if(id=="class.greatblade")Need(!(model.runtimeAnimatorController is AnimatorOverrideController),"One-hand Greatblade still uses two-hand clips");
     }
     var wolf=root.GetComponentsInChildren<WolfGreatswordPose>(true).Single();float supportError=both?Vector3.Distance(left.position,wolf.transform.TransformPoint(new Vector3(0,-.22f,0))):0;
     Need(((Animator)typeof(WolfGreatswordPose).GetField("animator",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(wolf)).transform.IsChildOf(root),"Sword references another form's rig "+id);
     if(both)Need(wolf.gameObject.activeInHierarchy&&supportError<.01f,"Two-hand support misses handle "+id+": "+supportError);
     var hud=p.GetComponent<HudPortraitRenderer>();var clone=(GameObject)typeof(HudPortraitRenderer).GetField("clone",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(hud);var hudModel=ClassPresentationSwitcher.FindVisualAnimator(clone.transform);
     var hudCamera=(Camera)typeof(HudPortraitRenderer).GetField("portraitCamera",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(hud);Need(Vector3.Distance(hudCamera.transform.position,hudModel.GetBoneTransform(HumanBodyBones.Hips).position)<hudCamera.farClipPlane,"HUD body clipped by stale sword bounds "+id);
     Need(clone.GetComponentsInChildren<WolfGreatswordPose>(true).All(w=>((Animator)typeof(WolfGreatswordPose).GetField("animator",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(w)).transform.IsChildOf(clone.transform)),"HUD sword uses a live-world rig "+id);
     if(id=="class.greatblade")Need(clone.GetComponentsInChildren<Animator>().Count(a=>a.isActiveAndEnabled)==1,"Duplicated Greatblade HUD model");
     if(!both)Need(Vector3.Distance(hudModel.GetBoneTransform(HumanBodyBones.LeftHand).position,hudModel.GetBoneTransform(HumanBodyBones.RightHand).position)>.22f,"HUD hands clasped "+id);
     var armGap=Vector3.Distance(left.position,right.position);var beforeLeg=model.GetBoneTransform(HumanBodyBones.LeftLowerLeg).rotation;Need(p.Motor.MoveTo(Point(p.transform.position+Vector3.right*3)),"Walk order");await Task.Delay(180);Need(motion.State=="Locomotion"&&Quaternion.Angle(beforeLeg,model.GetBoneTransform(HumanBodyBones.LeftLowerLeg).rotation)>.1f,"Loadout walk frozen "+id);p.Motor.Stop();
     var basic=p.GetComponent<BasicAttack>();basic.enabled=true;Move(p,new Vector3(270,0,415));Move(dummy,p.transform.position+Vector3.forward*(p.Radius+dummy.Radius+.2f));Need(basic.InRange(dummy)&&p.HasSightOf(dummy),"Clear in-range attack fixture");bool hit=false;Action<CombatActor,DamageResult> handler=(t,d)=>{if(t==dummy)hit=true;};basic.Connected+=handler;
     try{Need(basic.Order(dummy),"Basic order");await Until(()=>{if(basic.Target==null)basic.Order(dummy);return hit;},"Actual basic impact "+id);}catch(Exception e){throw new InvalidOperationException(e.Message+"; "+AttackState(),e);}finally{basic.Cancel();basic.Connected-=handler;}
     Need(motion.UsesTwoHandedStance==both,"Attack lost loadout stance");
     if(id=="class.greatblade"||id=="class.mage"){Move(dummy,new Vector3(310,0,415));await Capture(p,id,both);}
     // Menu preview is read-only and resolves the exact same equipped instances.
     var host=new GameObject("Isolated loadout menu preview");var stage=host.AddComponent<MenuPortraitStage>();
     try{Need(stage.Get(id,new ManifestationSave{classId=id,inventory=inv.Items,equipment=eq.Slots},Resources.Load<MenuPresentationCatalog>("MenuPresentationCatalog"))!=null,"Menu preview create");await Until(()=>stage.Portraits.Single().rendered,"Menu render");var portrait=stage.Portraits.Single();var menuEq=portrait.root.GetComponentInChildren<Equipment>();Need(menuEq.UsesTwoHandedStance==both,"Menu stance "+id);var anim=portrait.root.GetComponentsInChildren<Animator>().First();var ml=anim.GetBoneTransform(HumanBodyBones.LeftHand);var mr=anim.GetBoneTransform(HumanBodyBones.RightHand);if(!both)Need(Vector3.Distance(ml.position,mr.position)>.22f,"Menu hands clasped "+id);if(id=="class.greatblade")SaveTexture(portrait.texture,"VisualBaselines/HandPoseWolf/menu-"+(both?"two-handed":"main-offhand")+".png");}
     finally{Object.Destroy(host);}
     rows.Add(new{classId=id,twoHanded=both,idleHandSeparation=armGap,twoHandSupportError=supportError,walkAndActualAttack=true,menu=true});
    }
   }
   phase="passed";
  }catch(Exception error){failure=error.ToString();}
  finally{if(camera!=null)camera.ClearPresentationPose();}
  var json=Newtonsoft.Json.JsonConvert.SerializeObject(new{passed=failure==null,phase,checks,failure,rows},Newtonsoft.Json.Formatting.Indented);File.WriteAllText("Logs/HandPoseWolfReview.json",json);if(failure!=null)throw new InvalidOperationException(json);return json;
 }
 static Vector3 Point(Vector3 p){Need(NavMesh.SamplePosition(p,out var hit,2,NavMesh.AllAreas),"Nav point");return hit.position;}
 static async Task Capture(CombatActor p,string id,bool both){
  var nav=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("UnityEditor.AI.NavMeshVisualizationSettings")).First(t=>t!=null).GetProperty("showNavMesh",BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic);var oldNav=nav.GetValue(null);
  var viewType=typeof(UnityEditor.EditorWindow).Assembly.GetType("UnityEditor.GameView");var view=Resources.FindObjectsOfTypeAll(viewType).FirstOrDefault();var gizmos=viewType.GetProperty("drawGizmos",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);var oldGizmos=view!=null?gizmos.GetValue(view):null;
  var c=Object.FindAnyObjectByType<ExplorationCamera>();try{nav.SetValue(null,false);if(view!=null)gizmos.SetValue(view,false);var rotation=Quaternion.Euler(28,180,0);c.SetPresentationPose(p.transform.position+Vector3.up*1.3f-rotation*Vector3.forward*6,rotation,45);await Task.Delay(180);Directory.CreateDirectory("VisualBaselines/HandPoseWolf");ScreenCapture.CaptureScreenshot("VisualBaselines/HandPoseWolf/"+id+"-"+(both?"two-handed":"main-offhand")+".png");await Task.Delay(100);}finally{c.ClearPresentationPose();nav.SetValue(null,oldNav);if(view!=null&&oldGizmos!=null)gizmos.SetValue(view,oldGizmos);}
 }
 static void SaveTexture(RenderTexture texture,string path){Directory.CreateDirectory(Path.GetDirectoryName(path));var previous=RenderTexture.active;Texture2D image=null;try{RenderTexture.active=texture;image=new Texture2D(texture.width,texture.height,TextureFormat.RGBA32,false);image.ReadPixels(new Rect(0,0,texture.width,texture.height),0,0);image.Apply();File.WriteAllBytes(path,image.EncodeToPNG());}finally{RenderTexture.active=previous;if(image!=null)Object.Destroy(image);}}
}
