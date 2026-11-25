// Unity CLI eval_file; use a disposable copied save in the development player.
var qa=System.IO.Path.Combine(System.IO.Directory.GetParent(Application.dataPath).FullName,"QA");System.IO.Directory.CreateDirectory(qa);
var checks=new System.Collections.Generic.List<string>();var errors=new System.Collections.Generic.List<string>();
Component hero=null,journal=null,inventory=null,wallet=null,xp=null;string phase="startup",failure=null;
Type T(string n)=>AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType(n)).First(t=>t!=null);
Component[] All(string n)=>UnityEngine.Object.FindObjectsByType(T(n)).Cast<Component>().ToArray();
Component Part(Component c,string n)=>c.GetComponent(T(n));
object Read(object c,string n)=>c?.GetType().GetProperty(n,System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic)?.GetValue(c)??c?.GetType().GetField(n,System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic)?.GetValue(c);
void Set(object c,string n,object v){var p=c.GetType().GetProperty(n,System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic);if(p?.CanWrite==true)p.SetValue(c,v);else c.GetType().GetField(n,System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic).SetValue(c,v);}
object Call(object c,string n,params object[] a){var m=c.GetType().GetMethods(System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic).Where(m=>m.Name==n).First(m=>{var p=m.GetParameters();return p.Length>=a.Length&&p.Take(a.Length).Select((v,i)=>a[i]==null||v.ParameterType.IsInstanceOfType(a[i])).All(v=>v)&&p.Skip(a.Length).All(v=>v.IsOptional);});return m.Invoke(c,m.GetParameters().Select((v,i)=>i<a.Length?a[i]:v.DefaultValue).ToArray());}
object Static(string name,string property)=>T(name).GetProperty(property,System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.Static).GetValue(null);
object E(string type,string name)=>Enum.Parse(T(type),name);
string Q(string s)=>s==null?"null":"\""+s.Replace("\\","\\\\").Replace("\"","\\\"").Replace("\r"," ").Replace("\n"," ")+"\"";
void Report(string status)=>System.IO.File.WriteAllText(System.IO.Path.Combine(qa,"crucible-review.json"),"{\"status\":"+Q(status)+",\"phase\":"+Q(phase)+",\"checks\":"+checks.Count+",\"passed\":["+string.Join(",",checks.Select(Q))+"],\"error\":"+Q(failure)+",\"unityErrors\":["+string.Join(",",errors.Select(Q))+"]}");
void Need(bool value,string name){if(!value)throw new Exception(name);checks.Add(name);Report("running");}
void Phase(string name){phase=name;Report("running");}
bool Busy()=>(bool)Static("DiceFree.ApplicationFlow.LoadingScreen","Busy")||(bool)Static("DiceFree.ApplicationFlow.WorldFaceCrossing","Busy");
async System.Threading.Tasks.Task Until(Func<bool> condition,string label,int seconds=80){var end=DateTime.UtcNow.AddSeconds(seconds);while(!condition()){if(DateTime.UtcNow>end)throw new TimeoutException(label);await System.Threading.Tasks.Task.Delay(50);}}
object Quest(string id)=>((System.Collections.IEnumerable)Read(journal,"Definitions")).Cast<object>().Single(q=>(string)Read(q,"stableId")==id);
object Progress(string id)=>Call(journal,"GetProgress",id);
string Status(string id)=>Read(Progress(id),"status").ToString();
void SetQuest(string id,string status,int count)
{
 var records=(Array)Call(journal,"CaptureState");var record=records.Cast<object>().Single(r=>(string)Read(r,"questId")==id);
 Set(record,"status",E("DiceFree.Quests.QuestStatus",status));Set(record,"stage",0);Set(record,"count",count);Set(record,"alternatives",Array.CreateInstance(T("DiceFree.Quests.ObjectiveCount"),0));Call(journal,"RestoreState",records);
}
long TotalXp(){long amount=(int)Read(xp,"CurrentXp");var curve=Read(xp,"curve");for(int level=1;level<(int)Read(xp,"Level");level++)amount+=(int)Call(curve,"ToNextLevel",level);return amount;}
long Gold()=>(long)Read(wallet,"Gold");
Component View()=>All("DiceFree.Core.ExplorationCamera").Single();
Vector3 Nav(Vector3 point){if(Physics.Raycast(point+Vector3.up*120,Vector3.down,out var ground,280,1<<8))point=ground.point;if(!UnityEngine.AI.NavMesh.SamplePosition(point,out var result,3,1))throw new Exception("No navigation: "+point);return result.position;}
void Warp(Vector3 point){if(!(bool)Call(Read(hero,"Motor"),"TeleportAcrossMaps",Nav(point)))throw new Exception("Hero placement failed");Call(View(),"RecenterForTravel",hero.transform);Call(All("DiceFree.World.WorldFogOfWar").Single(),"RefreshVisionNow");}
void Quiet(){foreach(var ai in All("DiceFree.AI.AggroBehaviour"))((Behaviour)ai).enabled=false;foreach(var attack in All("DiceFree.Combat.BasicAttack"))Call(attack,"Cancel");Set(Read(hero,"Health"),"Invulnerable",true);}
void Hit(Component victim)
{
 var packet=Activator.CreateInstance(T("DiceFree.Combat.DamageResult"));Set(packet,"raw",1000000f);Set(packet,"mitigated",1000000f);
 Call(Read(victim,"Health"),"ApplyPacket",hero,packet);
}
async System.Threading.Tasks.Task Capture(string name,float zoom=30)
{
 Set(View(),"ZoomDistance",zoom);Call(View(),"RecenterForTravel",hero.transform);await System.Threading.Tasks.Task.Delay(450);
 if(!Application.isBatchMode)ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(qa,name+".png"));
 else
 {
  // Batch players do not refresh the screen buffer. Render world geometry
  // explicitly; screen-space overlay UI is intentionally absent in this path.
  var camera=Camera.main;var target=new RenderTexture(1600,900,24);var old=camera.targetTexture;var active=RenderTexture.active;Texture2D texture=null;
  try{camera.targetTexture=target;camera.Render();RenderTexture.active=target;texture=new Texture2D(1600,900,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,1600,900),0,0);texture.Apply();System.IO.File.WriteAllBytes(System.IO.Path.Combine(qa,name+"-world-only.png"),texture.EncodeToPNG());}
  finally{camera.targetTexture=old;RenderTexture.active=active;UnityEngine.Object.Destroy(target);if(texture!=null)UnityEngine.Object.Destroy(texture);}
 }
 await System.Threading.Tasks.Task.Delay(180);
}
async System.Threading.Tasks.Task Work(Component target)
{
 var interactor=Part(hero,"DiceFree.World.Interactor");Warp((Vector3)Read(target,"ApproachPosition"));await System.Threading.Tasks.Task.Delay(100);
 Need((bool)Read(target,"isActiveAndEnabled")&&(bool)Call(target,"Available",hero),"Survey target is available: "+target.name);
 Need((bool)Call(interactor,"Order",target),"Issue normal survey interaction: "+target.name);
 await Until(()=>Read(target,"Worker") as Component==hero,"Survey work begins");
 await Until(()=>Read(target,"Worker")==null,"Survey work completes",15);
}
void Log(string message,string stack,LogType type){if(type is LogType.Error or LogType.Exception or LogType.Assert)errors.Add(message);}
object run=null,arena=null;object[] bosses=Array.Empty<object>(),gates=Array.Empty<object>();Component buddy=null;
Vector3 Cell(int i)=>new Vector3(72000,0,72000+i*48);
void ArenaView(){Call(View(),"ConfigureMapBounds",new Rect(71976,71976,48,288));Call(All("DiceFree.World.WorldDayNightCycle").Single(),"SetInterior",true);}
void StopBoss(Component boss){((Behaviour)Part(boss,"DiceFree.AI.AggroBehaviour")).enabled=false;((Behaviour)Part(boss,"DiceFree.Combat.BasicAttack")).enabled=false;Call(Read(Read(boss,"Actor"),"Motor"),"Stop");}
async System.Threading.Tasks.Task StartRun()
{
 var portal=All("DiceFree.Dungeons.CrimsonCruciblePortal").Single(p=>p.gameObject.scene.name=="Cornberg");Warp((Vector3)Read(portal,"ApproachPosition"));Call(portal,"Interact",hero);await Until(()=>(run=Static("DiceFree.Dungeons.CrimsonCrucibleRun","Current"))!=null&&(bool)Read(run,"StagingReady"),"Crucible waiting room");Need((bool)Call(run,"StartNow",hero),"Initiator starts through the normal shared waiting room");await Until(()=>(arena=All("DiceFree.Dungeons.CrucibleArena").FirstOrDefault())!=null&&!Busy()&&(bool)Read(((Array)Read(arena,"bosses")).GetValue(0),"Armed"),"First cell armed");bosses=((Array)Read(arena,"bosses")).Cast<object>().ToArray();gates=((Array)Read(arena,"gates")).Cast<object>().ToArray();ArenaView();Quiet();
}
async System.Threading.Tasks.Task Mechanics(Component boss,int index)
{
 StopBoss(boss);var opponent=Read(boss,"Actor") as Component;Warp(opponent.transform.position+Vector3.back*6);Call(Read(hero,"Effects"),"ClearTransient");var health=Read(hero,"Health");Set(health,"Invulnerable",false);Call(health,"Restore",1f);
 float damage=0;Action<float> record=v=>damage+=v;var packet=System.Linq.Expressions.Expression.Parameter(T("DiceFree.Combat.DamageResult"),"packet");var changed=health.GetType().GetEvent("PacketResolved");var handler=System.Linq.Expressions.Expression.Lambda(changed.EventHandlerType,System.Linq.Expressions.Expression.Invoke(System.Linq.Expressions.Expression.Constant(record),System.Linq.Expressions.Expression.Field(packet,"applied")),System.Linq.Expressions.Expression.Parameter(T("DiceFree.Combat.CombatActor"),"source"),packet).Compile();changed.AddEventHandler(health,handler);
 try
 {
  float speed=(float)Read(Read(hero,"Motor"),"Speed");Call(boss,"BeginCast",hero);Need((float)Read(boss,"CastRemaining")>0,"Boss windup has a visible cast timer: "+index);await Capture("cell-"+(index+1)+"-windup",34);await Until(()=>(float)Read(boss,"CastRemaining")<=0,"Special impact",8);await System.Threading.Tasks.Task.Delay(100);Need(damage>0,"Real special attack resolves damage in cell "+(index+1));
  if(index==3)Need((float)Read(Read(hero,"Motor"),"Speed")<speed*.8f,"Plant roots apply the authored 35 percent slow");
  Call(Read(hero,"Effects"),"ClearTransient");damage=0;Warp(opponent.transform.position+Vector3.back*5);Call(boss,"BeginCast",hero);var safe=Cell(index)+new Vector3(15,0,-14);Warp(safe);await Until(()=>(float)Read(boss,"CastRemaining")<=0,"Dodged impact",8);await System.Threading.Tasks.Task.Delay(80);Need(damage==0,"Leaving the locked warning avoids damage in cell "+(index+1));
  Warp(opponent.transform.position+Vector3.back*5);Call(boss,"BeginCast",hero);Need((bool)Call(boss,"TryInterruptCast")&&(float)Read(boss,"CastRemaining")==0,"Special can be interrupted cleanly in cell "+(index+1));
 }
 finally{changed.RemoveEventHandler(health,handler);Set(health,"Invulnerable",true);Call(health,"Restore",1f);Call(Read(hero,"Effects"),"ClearTransient");}
}
async System.Threading.Tasks.Task ClearRun(bool mechanics)
{
 for(int i=0;i<6;i++)
 {
  int index=i;if(i>0)
  {
   await System.Threading.Tasks.Task.Delay(700);var at=Nav(Cell(i)+Vector3.back*12);Need((bool)Call(Read(hero,"Motor"),"MoveTo",at),"Order movement through newly opened gate "+i);await Until(()=>Vector3.Distance(hero.transform.position,at)<1.2f,"Reach next cell",45);await Until(()=>(bool)Read(bosses[index],"Armed"),"Next boss arms");
  }
  var boss=(Component)bosses[i];Need((int)Read(run,"Stage")==i,"Ordered progression reaches cell "+(i+1));Need(bosses.Skip(i+1).All(b=>!((Component)b).gameObject.activeSelf),"Future bosses remain dormant at cell "+(i+1));if(i<5)Need(!(bool)Read(gates[i],"Open"),"Next gate remains closed until its own boss dies: "+(i+1));
  StopBoss(boss);if(mechanics)await Mechanics(boss,i);
  int items=((Array)Read(inventory,"Items")).Length;Hit(Read(boss,"Actor") as Component);await Until(()=>(int)Read(run,"Stage")==index+1,"Boss defeat advances exactly one cell");Need(((Array)Read(inventory,"Items")).Length==items,"Intermediate boss cannot grant treasury equipment early: "+(i+1));if(i<5)Need((bool)Read(gates[i],"Open")&&gates.Skip(i+1).All(g=>!(bool)Read(g,"Open")),"Only the defeated boss's gate opens: "+(i+1));
 }
}
Component MakeBuddy()
{
 var go=new GameObject("QA Crucible party member");go.SetActive(false);go.transform.position=hero.transform.position+Vector3.right*2;
 var stats=go.AddComponent(T("DiceFree.Combat.ActorStats"));Call(stats,"Configure",Read(Read(hero,"Stats"),"Definition"),55);go.AddComponent<UnityEngine.AI.NavMeshAgent>();go.AddComponent(T("DiceFree.Characters.TraversalMotor"));go.AddComponent(T("DiceFree.Combat.Health"));var body=go.AddComponent(T("DiceFree.Combat.CombatActor"));Call(body,"Configure",0,.45f,null);go.AddComponent(T("DiceFree.Combat.BasicAttack"));go.AddComponent(T("DiceFree.Combat.CombatStatusController"));go.AddComponent(T("DiceFree.Items.CarriedInventory"));go.AddComponent(T("DiceFree.Items.GoldWallet"));((Behaviour)go.AddComponent(T("DiceFree.Characters.TraversalInput"))).enabled=false;var member=go.AddComponent(T("DiceFree.Quests.LocalPartyMember"));Set(member,"partyId","qa.crucible");var original=Part(hero,"DiceFree.Quests.LocalPartyMember")??hero.gameObject.AddComponent(T("DiceFree.Quests.LocalPartyMember"));Set(original,"partyId","qa.crucible");go.SetActive(true);return body;
}
async System.Threading.Tasks.Task ReviewCrucible()
{
 try
 {
 Phase("Copied save and equipment coverage");Component menu=null;await Until(()=>(menu=All("DiceFree.Advancement.StartMenuController").FirstOrDefault())!=null&&!Busy(),"Menu ready");Need((bool)Call(menu,"TryPlayManifestation","class.psionic-knight"),"Load copied Knight profile");await Until(()=>(hero=All("DiceFree.Characters.TraversalInput").FirstOrDefault())!=null&&!Busy()&&(bool)Read(Part(hero,"DiceFree.Persistence.ManifestationPersistence"),"Ready"),"Saved hero ready");hero=Part(hero,"DiceFree.Combat.CombatActor");journal=Part(hero,"DiceFree.Quests.QuestJournal");inventory=Part(hero,"DiceFree.Items.CarriedInventory");wallet=Part(hero,"DiceFree.Items.GoldWallet");xp=Part(hero,"DiceFree.Progression.ExperienceProgression");Quiet();((Behaviour)Part(hero,"DiceFree.Characters.CombatInput")).enabled=false;((Behaviour)Part(hero,"DiceFree.UI.WorldHoverFeedback")).enabled=false;
 var rewards=Resources.LoadAll("DiceFree/DungeonRewards/CrimsonCrucible",T("DiceFree.Items.ItemDefinition"));Need(rewards.Length==17&&rewards.All(r=>(int)Read(r,"itemLevel")==55&&(Sprite)Read(r,"icon")!=null),"All seventeen level 55 rewards have shipped icons");Need(new[]{"Head","Shoulders","Legs","Back"}.All(s=>rewards.Count(r=>Read(r,"slot").ToString()==s)>=4),"Each sparse slot has all four role variants");var equipment=Part(hero,"DiceFree.Items.Equipment");foreach(var item in rewards){var instance=Call(inventory,"Grant",(string)Read(item,"stableId"));Need((bool)Call(equipment,"Equip",(string)Read(instance,"instanceId"),Read(item,"slot")),"New reward equips: "+(string)Read(item,"displayName"));}
 var portal=All("DiceFree.Dungeons.CrimsonCruciblePortal").Single(p=>p.gameObject.scene.name=="Cornberg");Need((bool)Call(Part(hero,"DiceFree.World.InteractionRegistry"),"Contains",portal),"Crimson Mire entrance is discoverable through the normal interaction registry");Warp((Vector3)Read(portal,"ApproachPosition"));await Capture("crimson-mire-entrance",32);
 Phase("Solo cells, attack mechanics, gates and treasury");await StartRun();Need(bosses.Length==6&&gates.Length==5,"Six bosses and five sequential gates load");await System.Threading.Tasks.Task.Delay(750);var closedPath=new UnityEngine.AI.NavMeshPath();Need(!UnityEngine.AI.NavMesh.CalculatePath(hero.transform.position,Cell(1),1,closedPath)||closedPath.status!=UnityEngine.AI.NavMeshPathStatus.PathComplete,"Closed gate carving blocks navigation to the next cell");Need(!(bool)Call(Part(hero,"DiceFree.Combat.BasicAttack"),"Order",Part((Component)bosses[1],"DiceFree.Combat.CombatActor")),"Dormant future boss cannot be attacked");
 var pool=Read(arena,"rewards");var state=UnityEngine.Random.state;var seen=new System.Collections.Generic.HashSet<string>();int rare=0,doubleDrop=0;try{UnityEngine.Random.InitState(62055);for(int i=0;i<6000;i++){var roll=(Array)Call(pool,"Roll");var ids=roll.Cast<object>().Select(d=>(string)Read(d,"stableId")).ToArray();if(ids.Contains("item.crimson-crucible.victors-shroud")){rare++;if(roll.Length!=1)throw new Exception("Rare branch shape changed");}else if(roll.Length==2){doubleDrop++;if(ids.Distinct().Count()!=2)throw new Exception("Duplicate ordinary offer");}foreach(var id in ids)seen.Add(id);}}finally{UnityEngine.Random.state=state;}Need(seen.Count==17&&rare>200&&rare<420&&doubleDrop>1000,"All rewards are reachable with normal rare and distinct-double offer rules");
 await ClearRun(true);Component room=null;await Until(()=>(room=All("DiceFree.Dungeons.DungeonRewardRoom").FirstOrDefault())!=null&&(bool)Call(room,"HasChoice",hero)&&!Busy(),"Normal treasury opens",100);int count=((Array)Read(inventory,"Items")).Length;Need((bool)Call(room,"Choose",hero,0),"Final victory grants a normal treasury choice");Need(((Array)Read(inventory,"Items")).Length==count+1&&!(bool)Call(room,"Choose",hero,0),"Exactly one item can be claimed for the solo clear");await Until(()=>Static("DiceFree.Dungeons.CrimsonCrucibleRun","Current")==null&&!Busy(),"Solo session releases");Need(Vector3.Distance(hero.transform.position,(Vector3)Read(portal,"ApproachPosition"))<5,"Solo reward returns the hero to the Crimson Mire entrance");
 Phase("Shared waiting room and private party rewards");buddy=MakeBuddy();await System.Threading.Tasks.Task.Delay(400);Warp((Vector3)Read(portal,"ApproachPosition"));var error=T("DiceFree.Dungeons.CrimsonCrucibleRun").GetMethod("Enter").Invoke(null,new object[]{hero});Need(error==null,"Party initiator enters staging");run=Static("DiceFree.Dungeons.CrimsonCrucibleRun","Current");await Until(()=>(bool)Read(run,"StagingReady"),"Party staging ready");error=T("DiceFree.Dungeons.CrimsonCrucibleRun").GetMethod("Enter").Invoke(null,new object[]{buddy});Need(error==null,"Second party member joins the same waiting room");Need(!(bool)Call(run,"StartNow",buddy),"Only the initiator can start the waiting room");Need((bool)Call(run,"StartNow",hero),"Initiator launches both party members");await Until(()=>(arena=All("DiceFree.Dungeons.CrucibleArena").FirstOrDefault())!=null&&!Busy()&&(bool)Read(((Array)Read(arena,"bosses")).GetValue(0),"Armed"),"Party arena");bosses=((Array)Read(arena,"bosses")).Cast<object>().ToArray();gates=((Array)Read(arena,"gates")).Cast<object>().ToArray();ArenaView();Quiet();Set(Read(buddy,"Health"),"Invulnerable",true);Need(Vector3.Distance(hero.transform.position,buddy.transform.position)<6,"Both members reach the first cell together");var firstActor=Read(bosses[0],"Actor");var hp=(float)Read(Read(firstActor,"Health"),"Maximum");Need(hp>14000&&hp<17000,"Boss HP scales for the two-person roster");
 await ClearRun(false);await Until(()=>(room=All("DiceFree.Dungeons.DungeonRewardRoom").FirstOrDefault())!=null&&(bool)Call(room,"HasChoice",hero)&&(bool)Call(room,"HasChoice",buddy)&&!Busy(),"Both private treasure offers",100);Need((int)Read(room,"Pending")==2,"Each living participant receives a separate reward offer");var own=((Array)Read(Part(buddy,"DiceFree.Items.CarriedInventory"),"Items")).Length;Need((bool)Call(room,"Choose",buddy,0)&&((Array)Read(Part(buddy,"DiceFree.Items.CarriedInventory"),"Items")).Length==own+1,"Second member claims their own equipment");Need((bool)Call(room,"HasChoice",hero),"Second member's claim preserves the initiator's offer");long beforeGold=Gold();Need((bool)Call(room,"Choose",hero,-1)&&Gold()==beforeGold+180,"Initiator can choose the standard XP/gold alternative");await Until(()=>Static("DiceFree.Dungeons.CrimsonCrucibleRun","Current")==null&&!Busy(),"Party session releases");UnityEngine.Object.Destroy(buddy.gameObject);buddy=null;
 Phase("Abandon, menu exit and fresh-run reset");await StartRun();Need((int)Read(run,"Stage")==0&&gates.All(g=>!(bool)Read(g,"Open")),"A new attempt resets every boss and gate");Call(run,"RequestLeave",hero);await Until(()=>Static("DiceFree.Dungeons.CrimsonCrucibleRun","Current")==null&&!Busy(),"Abandon cleanup");Need(Read(Part(hero,"DiceFree.Foundation.RunLoadoutLock"),"Owner")==null,"Abandon releases the loadout lock");await StartRun();Need((bool)Call(Part(hero,"DiceFree.Dungeons.ReturnToStartMenu"),"Return"),"Return to menu is supported during the gauntlet");await Until(()=>All("DiceFree.Advancement.StartMenuController").Length>0&&!Busy(),"Menu after dungeon exit");Need(Static("DiceFree.Dungeons.CrimsonCrucibleRun","Current")==null,"Menu exit releases dungeon occupancy");Need(errors.Count==0,"No runtime errors during complete solo, party and exit review");Report("passed");
 }
 catch(Exception e){failure=e.ToString();Report("failed");}
 finally{Application.logMessageReceived-=Log;if(buddy!=null)UnityEngine.Object.Destroy(buddy.gameObject);}
}
Report("scheduled");Application.runInBackground=true;Application.logMessageReceived+=Log;_ = ReviewCrucible();return "Crimson Crucible solo, party, gear and lifecycle review scheduled.";