using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using DiceFree.AI;
using DiceFree.Briarback;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Core;
using DiceFree.Items;
using DiceFree.Persistence;
using DiceFree.Progression;
using DiceFree.Quests;
using DiceFree.Skills;
using DiceFree.UI;
using DiceFree.World;
using UnityEngine;
using UnityEngine.AI;
using Object=UnityEngine.Object;
public static class NovemberFeedbackReview
{
 static int checks;static string phase;static readonly List<object> evidence=new();
 static void Need(bool ok,string why){checks++;if(!ok)throw new InvalidOperationException(why);}
 static async Task Until(Func<bool> test,string why){var end=DateTime.UtcNow.AddSeconds(35);while(!test()){if(DateTime.UtcNow>end)throw new TimeoutException(why);await Task.Delay(20);}}
 static Vector3 Point(Vector3 p){Need(NavMesh.SamplePosition(p,out var hit,3,NavMesh.AllAreas),"Navigation "+p);return hit.position;}
 static void Move(CombatActor p,Vector3 point){Need(p.Motor.Teleport(Point(point)),"Teleport fixture");Physics.SyncTransforms();}
 static long Xp(ExperienceProgression xp){long result=xp.CurrentXp;for(int l=1;l<xp.Level;l++)result+=30+3L*(l-1)*(l-1);return result;}
 static async Task Capture(string name){Directory.CreateDirectory("VisualBaselines/NovemberFeedback");await Task.Delay(80);ScreenCapture.CaptureScreenshot("VisualBaselines/NovemberFeedback/"+name+".png");await Task.Delay(80);}
 public static async Task<string> Run()
 {
  checks=0;evidence.Clear();string failure=null;
  try{
   phase="isolated setup";CombatActor p=null;await Until(()=>(p=Object.FindAnyObjectByType<TraversalInput>()?.GetComponent<CombatActor>())!=null&&p.Motor.Ready&&p.GetComponent<ManifestationPersistence>().Ready,"Player ready");
   p.GetComponent<TraversalInput>().enabled=false;p.GetComponent<CombatInput>().enabled=false;p.Health.Invulnerable=true;
   foreach(var b in Object.FindObjectsByType<MonoBehaviour>().Where(b=>b is AggroBehaviour||b is BasicAttack||b is QuilboarCombat||b is BriarbackChieftainEncounter))b.enabled=false;
   foreach(var a in CombatActor.All)if(a.Motor!=null&&a.Motor.Ready)a.Motor.Stop();
   var save=p.GetComponent<ManifestationPersistence>();var respawn=p.GetComponent<RespawnAtAnchor>();var stones=Object.FindObjectsByType<ResurrectionDiceVisual>();
   phase="visible stone attunement and death return";
   foreach(string id in new[]{"anchor.trimery","anchor.cornberg","anchor.trimery"}){
    var die=stones.Single(d=>d.Anchor.StableId==id);var edge=die.transform.position+Vector3.back*4;edge.y=die.Anchor.transform.position.y;Move(p,edge);await Task.Delay(750);
    Need(respawn.AnchorId==id,"Visible stone edge did not activate "+id);await Task.Delay(250);Need(die.Glowing&&stones.Count(d=>d.Glowing)==1,"Selected stone glow mismatch");
    p.Health.Invulnerable=false;p.Health.ApplyPacket(null,new DamageResult{mitigated=1000000});await Until(()=>respawn.CanReturn,"Death return delay");Need(respawn.Return(),"Anchor return");Need(Vector3.Distance(p.transform.position,die.Anchor.transform.position)<2.1f,"Wrong respawn destination");p.Health.Invulnerable=true;
   }
   Need(save.Flush(),"Selected anchor save");evidence.Add(new{stoneEdges=true,deathReturns=3,selectedAnchor=respawn.AnchorId});
   phase="return home";Move(p,new Vector3(248,0,230));var home=p.GetComponent<ReturnHomeAbility>();await Until(()=>home.CanBegin,"Return Home threat expiry");Need(home.TryBegin(),home.Feedback);await Until(()=>!home.Casting,"Return Home channel");Need(respawn.AnchorId=="anchor.trimery"&&Vector3.Distance(p.transform.position,stones.Single(d=>d.Anchor.StableId=="anchor.trimery").Anchor.transform.position)<2.1f,"Return Home ignored selected stone");
   phase="Briarback road speed";Move(p,new Vector3(250,0,415));float normal=p.Motor.Speed;Need(p.GetComponent<SurfaceTravel>().Current==null,"Off-road comparison fixture");int roadCount=0;
   foreach(var road in TravelSurface.Active.Where(s=>s.StableId.StartsWith("surface-area.briarback.")).ToArray()){
    var mesh=road.GetComponent<MeshFilter>();var vertices=mesh.sharedMesh.vertices;var tri=mesh.sharedMesh.triangles;bool found=false;
    for(int i=0;i<tri.Length&&!found;i+=3){var spot=mesh.transform.TransformPoint((vertices[tri[i]]+vertices[tri[i+1]]+vertices[tri[i+2]])/3);if(!NavMesh.SamplePosition(spot,out var hit,.6f,NavMesh.AllAreas)||!road.Contains(hit.position))continue;Move(p,hit.position);await Task.Delay(40);Need(Mathf.Abs(p.Motor.Speed/normal-1.15f)<.01f,"Road bonus "+road.name);found=true;roadCount++;}
    Need(found,"No navigable road sample "+road.name);
   }
   Need(roadCount==5,"Five roads missing");Move(p,new Vector3(250,0,415));Need(Mathf.Abs(p.Motor.Speed/normal-1)<.01f,"Road bonus leaked off road");evidence.Add(new{roads=roadCount,bonusPercent=15});
   phase="bounty legacy migration";var journal=p.GetComponent<QuestJournal>();var bounty=journal.Definitions.Single(q=>q.stableId=="quest.briarback.repeat-bounty");
   foreach(int count in new[]{0,14,15,19,20}){journal.RestoreState(new[]{new QuestProgress{questId=bounty.stableId,definitionVersion=1,status=count==20?QuestStatus.ReadyToTurnIn:QuestStatus.Active,count=count}});var state=journal.GetProgress(bounty.stableId);Need(state.definitionVersion==2&&state.count==Mathf.Min(15,count)&&state.status==(count>=15?QuestStatus.ReadyToTurnIn:QuestStatus.Active),"Legacy bounty count "+count);}
   journal.RestoreState(Array.Empty<QuestProgress>());
   phase="15 actual bounty kills and exact turn-in XP";var giver=Object.FindObjectsByType<QuestGiver>().Single(g=>g.Offers.Contains(bounty));Move(p,giver.ApproachPosition);Need(giver.Accept(p,bounty),"Bounty accept");
   var enemies=Object.FindObjectsByType<QuilboarCombat>().Where(q=>q.role==QuilboarRole.Raider||q.role==QuilboarRole.SpearThrower||q.role==QuilboarRole.Totemcaller).Take(15).ToArray();Need(enemies.Length==15,"Bounty fixtures");
   for(int i=0;i<15;i++){var target=enemies[i].GetComponent<CombatActor>();target.Health.Restore();Move(p,target.transform.position+Vector3.back*2);target.Health.Invulnerable=false;target.Health.ApplyPacket(p,new DamageResult{mitigated=1000000});Need(journal.GetProgress(bounty.stableId).count==i+1,"Bounty kill count");}
   Need(journal.GetProgress(bounty.stableId).status==QuestStatus.ReadyToTurnIn,"Bounty readiness");Move(p,giver.ApproachPosition);long before=Xp(p.GetComponent<ExperienceProgression>());Need(giver.TurnIn(p,bounty),"Bounty turn-in");Need(Xp(p.GetComponent<ExperienceProgression>())-before==5160,"Exact bounty XP");Need(!giver.TurnIn(p,bounty)&&journal.GetProgress(bounty.stableId).status==QuestStatus.Available,"Bounty duplicate/reset");
   phase="Four Elements exact turn-in reward";var hunt=journal.Definitions.Single(q=>q.stableId=="quest.briarback.four-elements-hunt");giver=Object.FindObjectsByType<QuestGiver>().Single(g=>g.Offers.Contains(hunt));Move(p,giver.ApproachPosition);Need(giver.Accept(p,hunt),"Hunt accept");
   foreach(var role in new[]{QuilboarRole.FireShaman,QuilboarRole.EarthShaman,QuilboarRole.LightningShaman,QuilboarRole.IceShaman}){var target=Object.FindObjectsByType<QuilboarCombat>().First(q=>q.role==role).GetComponent<CombatActor>();Move(p,target.transform.position+Vector3.back*2);target.Health.Invulnerable=false;target.Health.ApplyPacket(p,new DamageResult{mitigated=1000000});}
   Move(p,giver.ApproachPosition);before=Xp(p.GetComponent<ExperienceProgression>());Need(giver.TurnIn(p,hunt),"Hunt turn-in");Need(Xp(p.GetComponent<ExperienceProgression>())-before==2500&&!giver.TurnIn(p,hunt),"Exact one-time 2500 XP");evidence.Add(new{bountyKills=15,bountyTurnInXp=5160,huntTurnInXp=2500,legacyMigrations=5});
   phase="Mage equip and channel lifecycle";var catalog=(ClassCatalog)typeof(ManifestationPersistence).GetField("classCatalog",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).GetValue(save);
   Need(save.TryForkAndActivate(catalog.Resolve("class.magically-touched-novice"),out _),"Parent fixture");Need(save.TryForkAndActivate(catalog.Resolve("class.mage"),out _),"Mage fixture");
   var xp=p.GetComponent<ExperienceProgression>();while(xp.Level<35)xp.Grant(xp.RequiredXp);var skills=p.GetComponent<MageSkillProgression>();foreach(var kind in new[]{MageProgressionRules.Kind.Quicksand,MageProgressionRules.Kind.EmberBolt,MageProgressionRules.Kind.HalfLife})Need(skills.Spend(kind),"Skill learn");
   var inventory=p.GetComponent<CarriedInventory>();var orb=inventory.Grant("item.slime.slime-orb");Need(p.GetComponent<Equipment>().Equip(orb.instanceId,EquipmentSlot.OffHand),"Mage Slime Orb equip");Need(inventory.Resolve(BriarbackMantleVisual.ItemId).icon!=null,"Mantle icon missing");
   var caster=p.GetComponent<MageSkillCaster>();Need(caster.Tuning.attacks.Take(5).All(a=>a.baseDamage==20),"Mage spell base damage");
   var dummy=enemies[0].GetComponent<CombatActor>();dummy.Stats.SetEncounterMaximumHp(1000000);dummy.Health.Restore();Move(p,new Vector3(270,0,415));Move(dummy,p.transform.position+Vector3.forward*3);
   void Begin(){caster.ResetForManifestationLoad();p.GetComponent<ActorResourceController>().RefillAll();p.Effects.ClearTransient();p.GetComponent<BasicAttack>().Cancel();Move(p,new Vector3(270,0,415));Move(dummy,p.transform.position+Vector3.forward*3);Need(caster.CastQuicksand(dummy.transform.position),caster.Feedback);Need(caster.IsChanneling&&caster.ChannelDuration==5,"Channel not started");}
   Begin();await Task.Delay(1100);Need(caster.IsChanneling,"Channel ended early");Need(Object.FindAnyObjectByType<ChannelBarHud>().TryActive(out var shown)&&shown.ChannelLabel=="Quicksand","Shared channel bar");await Capture("quicksand-channel");float hp=dummy.Health.Current;Need(p.Motor.MoveTo(Point(p.transform.position+Vector3.right*3)),"Move cancel");await Task.Delay(100);Need(!caster.IsChanneling,"Moving did not interrupt");p.Motor.Stop();await Task.Delay(1200);Need(dummy.Health.Current>=hp-.1f,"Cancelled field kept damaging");
   Begin();Need(caster.Cast(MageProgressionRules.Kind.EmberBolt,dummy),"Replacement spell");Need(!caster.IsChanneling,"Other spell did not interrupt");
   Begin();p.Effects.TryApplyStun(.3f,true);await Task.Delay(80);Need(!caster.IsChanneling,"Stun did not interrupt");await Task.Delay(350);
   Begin();p.GetComponent<BasicAttack>().Order(dummy);await Task.Delay(80);Need(!caster.IsChanneling,"Attack did not interrupt");p.GetComponent<BasicAttack>().Cancel();
   Begin();p.Health.Invulnerable=false;p.Health.ApplyPacket(null,new DamageResult{mitigated=1000000});Need(!caster.IsChanneling,"Death did not interrupt");await Until(()=>respawn.CanReturn,"Mage return");Need(respawn.Return(),"Mage respawn");p.Health.Invulnerable=true;
   var queue=p.GetComponent<PlayerCommandQueue>();Begin();Need(queue.SubmitMove(Point(p.transform.position+Vector3.right*3),false),"Replacement queued move");Need(!caster.IsChanneling,"Unshifted queued order did not interrupt");queue.ClearOrders();
   Begin();var channelPosition=p.transform.position;Need(queue.SubmitMove(Point(p.transform.position+Vector3.right*3),true),"Appended move");await Task.Delay(200);Need(caster.IsChanneling&&!p.Motor.Travelling,"Appended move interrupted channel");await Task.Delay(5000);Need(!caster.IsChanneling,"Channel did not finish");Need(p.Motor.Travelling||Vector3.Distance(channelPosition,p.transform.position)>.1f,"Appended move did not resume");queue.ClearOrders();Need(Object.FindObjectsByType<MageSpellVfx>().All(v=>v.name!="Quicksand field"),"Channel visual leaked");
   Need(save.Flush(),"Mage Orb save");Need(save.TryActivateExisting("class.novice",out _),"Novice restore");Need(respawn.AnchorId=="anchor.trimery","Novice anchor did not survive class reload");Need(save.TryActivateExisting("class.mage",out _),"Mage reload");Need(p.GetComponent<Equipment>().Slots.Any(s=>inventory.Find(s.instanceId)?.definitionId=="item.slime.slime-orb"),"Orb not persisted");evidence.Add(new{mageOrbEquippedAndReloaded=true,spellBaseDamage=20,channelSeconds=5,interrupts=new[]{"move","spell","stun","attack","death"},channelCompletes=true});
   phase="tree fade restoration";var tree=Object.FindObjectsByType<TreeCameraOccluder>().First(t=>t.GetComponentsInChildren<Renderer>().Any(r=>r.enabled&&r.sharedMaterial!=null&&r.sharedMaterial.HasProperty("_BaseColor")));var leaf=tree.GetComponentsInChildren<Renderer>().First(r=>r.enabled&&r.sharedMaterial!=null&&r.sharedMaterial.HasProperty("_BaseColor"));var originals=leaf.sharedMaterials;
   var fader=new GameObject("Isolated tree fade fixture").AddComponent<CameraOcclusionFader>();var focus=tree.transform.position+Vector3.up;var center=leaf.bounds.center;fader.UpdateOcclusion(focus,center+(center-focus).normalized*20);Need(leaf.sharedMaterial!=originals[0]&&leaf.sharedMaterial.color.a<=.18f,"Tree insufficiently transparent");Need(leaf.sharedMaterial.GetFloat("_ZWrite")==0&&!leaf.sharedMaterial.IsKeywordEnabled("_ALPHATEST_ON"),"Foliage fade mode");fader.UpdateOcclusion(center+Vector3.right*100,center+Vector3.right*100+Vector3.up);Need(leaf.sharedMaterials.SequenceEqual(originals),"Tree opacity not restored");Object.Destroy(fader.gameObject);evidence.Add(new{treeMaximumOpacity=.18f,alphaCutoutDisabledWhileFaded=true,originalMaterialsRestored=true});phase="passed";
  }catch(Exception e){failure=e.ToString();}
  string json=Newtonsoft.Json.JsonConvert.SerializeObject(new{passed=failure==null,phase,checks,failure,evidence},Newtonsoft.Json.Formatting.Indented);File.WriteAllText("Logs/NovemberFeedbackReview.json",json);if(failure!=null)throw new InvalidOperationException(json);return json;
 }
}
