using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Core;
using DiceFree.Dungeons;
using DiceFree.Foundation;
using DiceFree.Items;
using DiceFree.Persistence;
using DiceFree.Progression;
using DiceFree.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;
public static class WolfRewardsReview
{
 static string phase;static int checks,rare,two;static readonly System.Collections.Generic.List<string> errors=new();
 static void Need(bool ok,string text){checks++;if(!ok)throw new InvalidOperationException(text);}
 static void Log(string message,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)errors.Add(message);}
 static void Phase(string text){phase=text;File.WriteAllText("Logs/WolfRewardsReview.json",Newtonsoft.Json.JsonConvert.SerializeObject(new{status="running",phase,checks}));}
 static async Task Until(Func<bool> condition,string text){var end=DateTime.UtcNow.AddSeconds(45);while(!condition()){Need(DateTime.UtcNow<end,"Timeout: "+text);await Task.Delay(25);}}
 static long Xp(CombatActor p){var xp=p.GetComponent<ExperienceProgression>();long total=xp.CurrentXp;for(int i=1;i<xp.Level;i++)total+=30+3L*(i-1)*(i-1);return total;}
 public static string Start()
 {
  if(EditorApplication.isPlayingOrWillChangePlaymode||SceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("Clean Edit Mode required");
  var guard=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("DiceFree.EditorTools.PersistenceTestGuard")).First(t=>t!=null);guard.GetMethod("UseIsolatedSaveRootForNextPlay").Invoke(null,new object[]{Path.Combine(Path.GetTempPath(),"DiceFree-WolfRewards-"+Guid.NewGuid())});EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/Cornberg.unity");EditorApplication.EnterPlaymode();return "Started isolated reward review";
 }
 public static string Run(){checks=rare=two=0;errors.Clear();Application.logMessageReceived+=Log;_=Execute();return "Started; inspect Logs/WolfRewardsReview.json";}
 static async Task Capture(string name){Directory.CreateDirectory("VisualBaselines/WolfRewards");await Task.Delay(180);ScreenCapture.CaptureScreenshot("VisualBaselines/WolfRewards/"+name+".png");await Task.Delay(150);}
 static async Task<CombatActor> Player(){CombatActor p=null;await Until(()=>(p=Object.FindAnyObjectByType<TraversalInput>()?.GetComponent<CombatActor>())!=null&&p.Motor.Ready&&p.GetComponent<ManifestationPersistence>().Ready&&!DiceFree.ApplicationFlow.LoadingScreen.Busy,"Ready player");p.GetComponent<TraversalInput>().enabled=false;p.GetComponent<CombatInput>().enabled=false;p.GetComponent<BasicAttack>().enabled=false;p.Health.Invulnerable=true;return p;}
 static async Task<WolfDungeonRun> ClearWolf(CombatActor p)
 {
  var portal=Object.FindAnyObjectByType<WolfDungeonPortal>();Need(p.Motor.TeleportAcrossMaps(portal.ApproachPosition),"Entrance fixture");portal.Interact(p);await Until(()=>WolfDungeonRun.Current?.StagingReady==true&&!DiceFree.ApplicationFlow.LoadingScreen.Busy,"Staging");var run=WolfDungeonRun.Current;Need(!run.ClaimRewards(p)&&!run.ClaimRewards(null),"No rewards before clear");Need(run.StartNow(p),"Start wolf run");
  for(int wave=1;wave<=10;wave++)
  {await Until(()=>run.Fighting&&run.Wave==wave,"Wave "+wave);Need(run.Composition.Points==wave+2,"Budget preserved");if(wave==10)Need(run.Wolves.Count==4&&run.Wolves.All(w=>w.Rank==WolfRank.Alpha),"Final four alphas");foreach(var wolf in run.Wolves.ToArray())wolf.Actor.Health.ApplyPacket(p,new DamageResult{mitigated=1000000});await Until(()=>run.Phase==WolfRunPhase.Intermission||run.Phase==WolfRunPhase.Victory,"Clear wave "+wave);if(wave<10)Need(run.StartNextWave(p),"Advance wolf wave");}
  Need(!run.ClaimRewards(null)&&run.ClaimRewards(p)&&!run.ClaimRewards(p),"Guarded reward travel");await Until(()=>run.Phase==WolfRunPhase.Rewards&&!DiceFree.ApplicationFlow.LoadingScreen.Busy&&!SceneManager.GetSceneByName("WolfSurvival").isLoaded,"Treasure room");Need(p.GetComponent<RunLoadoutLock>().InsideDungeon&&!p.GetComponent<RunLoadoutLock>().Locked,"Unlocked reward preparation retains run ownership");return run;
 }
 static async Task Claim(CombatActor p,DungeonRewardChest chest)
 {Need(chest.Available(p),"Owner chest available");Need(p.GetComponent<Interactor>().Order(chest),"Physical chest order");await Until(()=>WolfDungeonRun.Current==null&&!SceneManager.GetSceneByName("DungeonRewardRoom").isLoaded,"Claim and exit");Need(p.transform.position.x<1000&&!p.GetComponent<RunLoadoutLock>().InsideDungeon,"Overworld and unlocked");}
 static async Task<CombatActor> Resume(CombatActor p)
 {
  Need(p.GetComponent<ReturnToStartMenu>().Return(),"Return to menu");DiceFree.Advancement.StartMenuController menu=null;await Until(()=>(menu=Object.FindAnyObjectByType<DiceFree.Advancement.StartMenuController>())!=null&&!DiceFree.ApplicationFlow.LoadingScreen.Busy,"Menu");var entry=menu.Entries.First(e=>e.active);Need(menu.TryPlayManifestation(entry.classId),"Resume saved character");return await Player();
 }
 static void EquipmentChecks(ItemDefinition[] items,CombatActor p)
 {
  foreach(var id in new[]{"class.magically-touched-novice","class.physically-blessed-novice","class.mage","class.priest","class.thief","class.acolyte"})
  {
   var go=new GameObject("Equipment eligibility fixture");go.SetActive(false);var def=Object.Instantiate(p.Stats.Definition);def.stableId=id;go.AddComponent<ActorStats>().Configure(def);var inv=go.AddComponent<CarriedInventory>();inv.Configure(items);var equipment=go.AddComponent<Equipment>();go.SetActive(true);
   foreach(var item in items){var instance=inv.Grant(item.stableId);bool expected=item.allowedClassIds.Length==0||item.allowedClassIds.Contains(id);Need(equipment.Equip(instance.instanceId,item.slot)==expected,"Eligibility "+id+" / "+item.displayName);if(expected&&item.handsRequired==2)Need(equipment.UsesTwoHandedStance,"Healer staff uses both hands");equipment.Unequip(item.slot.ToString());}
   Object.Destroy(go);Object.Destroy(def);
  }
 }
 sealed class FakeRun:IDungeonRewardRun
 {public int finished;public void ObserveReward(CombatActor p,ItemDefinition i){}public void Drop(CombatActor p,ItemDefinition i){}public void ReturnPlayer(CombatActor p,bool entrance){}public void Finish(){finished++;}}
 static async Task Execute()
 {
  try
  {
   Phase("data and equipment");var p=await Player();var portal=Object.FindAnyObjectByType<WolfDungeonPortal>();var pool=portal.tuning.rewards;var items=Resources.LoadAll<ItemDefinition>("DiceFree/DungeonRewards/Wolf");Need(items.Length==6&&pool.items.Length==5&&pool.rare.slot==EquipmentSlot.Ring,"Six clarified drops");Need(items.All(i=>i.icon!=null&&i.icon.texture.width==512&&p.GetComponent<CarriedInventory>().Resolve(i.stableId)==i),"Icons and durable resource catalog");Need(items.Count(i=>i.slot==EquipmentSlot.Chest)==1&&items.Count(i=>i.slot==EquipmentSlot.MainHand)==4&&!items.Any(i=>i.slot==EquipmentSlot.Hands),"No duplicate thief item");EquipmentChecks(items,p);
   foreach(var item in items){var tex=new Texture2D(2,2);tex.LoadImage(File.ReadAllBytes(AssetDatabase.GetAssetPath(item.icon)));var pixels=tex.GetPixels32();Need(pixels.Count(c=>c.r>180&&c.b>180&&c.g<55)<pixels.Length/100,"No magenta shader icon "+item.name);Object.Destroy(tex);}
   var random=UnityEngine.Random.state;try{UnityEngine.Random.InitState(51823);for(int i=0;i<20000;i++){var offer=pool.Roll();if(offer.Contains(pool.rare)){Need(offer.Length==1,"Rare override alone");rare++;}else{Need(offer.Length==1||offer.Length==2&&offer[0]!=offer[1],"Normal unique offers");if(offer.Length==2)two++;}}}finally{UnityEngine.Random.state=random;}Need(Math.Abs(rare/20000f-.05f)<.01f&&Math.Abs(two/(20000f-rare)-.25f)<.02f,"Normal pool distributions");
   Phase("item reward, private chest, reentrant claim");await ClearWolf(p);var room=Object.FindAnyObjectByType<DungeonRewardRoom>();var foreign=CombatActor.All.First(a=>a!=p&&a.Alive);var chest=Object.FindObjectsByType<DungeonRewardChest>().First(c=>c.Available(p)&&c.OfferedItem!=null);Need(!chest.Available(foreign)&&!room.Choose(foreign,0),"Private owner only");Need(!room.Choose(p,-2)&&!room.Choose(p,99),"Invalid choice indices");var definition=chest.OfferedItem;var inv=p.GetComponent<CarriedInventory>();int beforeItems=inv.Items.Length;long beforeXp=Xp(p),beforeGold=p.GetComponent<GoldWallet>().Gold;bool reentrant=false;Action callback=()=>{reentrant=room.Choose(p,chest.ChoiceIndex);};inv.Changed+=callback;await Capture("editor-item-choices");await Claim(p,chest);inv.Changed-=callback;Need(!reentrant&&inv.Items.Length==beforeItems+1&&Xp(p)==beforeXp&&p.GetComponent<GoldWallet>().Gold==beforeGold,"Exactly one item, no bonus, reentrant claim rejected");var awarded=inv.Items.Single(i=>i.definitionId==definition.stableId);p=await Resume(p);Need(p.GetComponent<CarriedInventory>().Find(awarded.instanceId)?.definitionId==definition.stableId&&p.GetComponent<CarriedInventory>().Resolve(definition.stableId)!=null,"Award persists with resolved definition after resume");
   Phase("XP gold alternative");await ClearWolf(p);var bonus=Object.FindObjectsByType<DungeonRewardChest>().Single(c=>c.Available(p)&&c.ChoiceIndex==-1);beforeItems=p.GetComponent<CarriedInventory>().Items.Length;beforeXp=Xp(p);beforeGold=p.GetComponent<GoldWallet>().Gold;await Capture("editor-bonus-choices");await Claim(p,bonus);Need(p.GetComponent<CarriedInventory>().Items.Length==beforeItems&&Xp(p)-beforeXp==50&&p.GetComponent<GoldWallet>().Gold-beforeGold==25,"XP gold instead of an item exactly once");
   Phase("unclaimed reward menu cleanup");await ClearWolf(p);beforeItems=p.GetComponent<CarriedInventory>().Items.Length;beforeXp=Xp(p);beforeGold=p.GetComponent<GoldWallet>().Gold;p=await Resume(p);Need(WolfDungeonRun.Current==null&&!SceneManager.GetSceneByName("DungeonRewardRoom").isLoaded&&p.GetComponent<CarriedInventory>().Items.Length==beforeItems&&Xp(p)==beforeXp&&p.GetComponent<GoldWallet>().Gold==beforeGold,"Unclaimed reward not paid or restored");var entrance=Object.FindAnyObjectByType<WolfDungeonPortal>();p.Motor.TeleportAcrossMaps(entrance.ApproachPosition);entrance.Interact(p);await Until(()=>WolfDungeonRun.Current?.StagingReady==true&&!DiceFree.ApplicationFlow.LoadingScreen.Busy,"Reentry after unclaimed exit");WolfDungeonRun.Current.Abandon();await Until(()=>WolfDungeonRun.Current==null,"Staging exit");
   Phase("Slime Dungeon uses shared rewards");var slimePortal=Object.FindObjectsByType<DungeonPortal>().Single(v=>v.isActiveAndEnabled);p.Motor.TeleportAcrossMaps(slimePortal.ApproachPosition+Vector3.back*1.5f);slimePortal.Interact(p);await Until(()=>SlimeDungeonRun.Current?.StagingReady==true,"Slime staging");var slime=SlimeDungeonRun.Current;Need(slime.StartNow(p),"Start original dungeon");await Task.Delay(80);var miniboss=Object.FindObjectsByType<DungeonSlime>().Single(s=>s.Role==SlimeRole.Miniboss);miniboss.Actor.Health.ApplyPacket(p,new DamageResult{mitigated=1000000});await Until(()=>slime.MinibossDead,"Slime miniboss credit");slime.SolvePuzzle(false);var boss=Object.FindObjectsByType<DungeonSlime>().Single(s=>s.Role==SlimeRole.Boss);boss.Actor.Health.ApplyPacket(p,new DamageResult{mitigated=1000000});await Until(()=>slime.Completing,"Slime completion");typeof(SlimeDungeonRun).GetField("completionEnd",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(slime,Time.time+.05f);await Until(()=>Object.FindAnyObjectByType<DungeonRewardRoom>()?.HasChoice(p)==true,"Shared Slime treasure room");await Capture("editor-slime-regression");var slimeChest=Object.FindObjectsByType<DungeonRewardChest>().First(c=>c.Available(p)&&c.OfferedItem!=null);Need(slimeChest.OfferedItem.sourceId=="dungeon.slime","No wolf/slime pool mixup");Need(p.GetComponent<Interactor>().Order(slimeChest),"Original physical reward chest");await Until(()=>!SceneManager.GetSceneByName("DungeonRewardRoom").isLoaded,"Slime reward cleanup");
   Phase("independent private runs finish independently");pool=Object.FindAnyObjectByType<WolfDungeonPortal>().tuning.rewards;foreign=CombatActor.All.First(a=>a!=p&&a.Alive);var fixture=new GameObject("Independent reward-run fixture");room=fixture.AddComponent<DungeonRewardRoom>();var one=new FakeRun();var other=new FakeRun();room.Offer(p,pool,one,0,0);room.Offer(foreign,pool,other,0,0);Need(room.Choose(p,0)&&one.finished==1&&other.finished==0&&room.Pending==1,"First dungeon finishes while another owner remains");Need(!room.Choose(p,0)&&room.DiscardForSessionExit(foreign)&&other.finished==1&&room.Pending==0,"Second owner cleanup and duplicate rejection");Object.Destroy(fixture);await Task.Delay(120);
   Need(errors.Count==0,"Unity errors: "+string.Join(";",errors));phase="passed";File.WriteAllText("Logs/WolfRewardsReview.json",Newtonsoft.Json.JsonConvert.SerializeObject(new{status="passed",checks,items=6,rolls=20000,rare,two,privateChoices=true,itemAndBonusExclusive=true,reentrantClaimRejected=true,savedItemResolves=true,unclaimedMenuCleanup=true,slimeRewardRegression=true,independentRuns=true,unityErrors=errors.Count},Newtonsoft.Json.Formatting.Indented));Debug.Log("DICEFREE_WOLF_REWARDS_OK");
  }
  catch(Exception e){File.WriteAllText("Logs/WolfRewardsReview.json",Newtonsoft.Json.JsonConvert.SerializeObject(new{status="failed",phase,checks,error=e.ToString(),unityErrors=errors},Newtonsoft.Json.Formatting.Indented));Debug.LogError(e);}
  finally{Application.logMessageReceived-=Log;}
 }
}
