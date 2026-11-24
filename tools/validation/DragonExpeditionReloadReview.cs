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
void Report(string status)=>System.IO.File.WriteAllText(System.IO.Path.Combine(qa,"dragon-expedition-reload-progress.json"),"{\"status\":"+Q(status)+",\"phase\":"+Q(phase)+",\"checks\":"+checks.Count+",\"passed\":["+string.Join(",",checks.Select(Q))+"],\"error\":"+Q(failure)+",\"unityErrors\":["+string.Join(",",errors.Select(Q))+"]}");
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
async System.Threading.Tasks.Task ReloadReview()
{
 try {
 phase="Actual player restart and copied-save reload";
 Component menu=null;await Until(()=>(menu=All("DiceFree.Advancement.StartMenuController").FirstOrDefault())!=null&&!Busy(),"Menu ready");Need((bool)Call(menu,"TryPlayManifestation","class.psionic-knight"),"Open existing copied Knight after actual player restart");
 await Until(()=>(hero=All("DiceFree.Characters.TraversalInput").FirstOrDefault())!=null&&!Busy()&&(bool)Read(Part(hero,"DiceFree.Persistence.ManifestationPersistence"),"Ready"),"Saved hero restored");hero=Part(hero,"DiceFree.Combat.CombatActor");journal=Part(hero,"DiceFree.Quests.QuestJournal");inventory=Part(hero,"DiceFree.Items.CarriedInventory");Quiet();
 Need(Status("quest.world2.first-foothold")=="Completed"&&Status("quest.world2.unfinished-record")=="Completed","Completed first and final quests survive actual disk reload");
 Need(Status("quest.world2.two-watchfires")=="Active"&&(int)Read(Progress("quest.world2.two-watchfires"),"count")==0,"Survey reset used for visual review survives actual disk reload as active progress");
 var ids=new[]{"wyrmfang-cuirass","roostguard-bulwark","embermender-vestments","jadescale-arcanist-robe","rooststalkers-grips","dragontrail-greaves"}.Select(s=>"item.ancient-road-dragon."+s).ToArray();var items=((System.Collections.IEnumerable)Read(inventory,"Items")).Cast<object>().ToArray();
 Need(ids.All(id=>items.Any(item=>(string)Read(item,"definitionId")==id)&&Call(inventory,"Resolve",id)!=null),"Every added dragon item retains its owned instance and definition after disk reload");
 var equipment=Part(hero,"DiceFree.Items.Equipment");foreach(var id in ids){var item=items.First(i=>(string)Read(i,"definitionId")==id);Need((bool)Call(equipment,"Equip",(string)Read(item,"instanceId"),Read(Call(inventory,"Resolve",id),"slot")),"Reloaded item remains equippable: "+id);}
 if(hero.transform.position.x<63000){var pass=All("DiceFree.ApplicationFlow.IronpineSouthPass").Single(p=>p.gameObject.scene.name=="Cornberg");Warp((Vector3)Read(pass,"ApproachPosition"));Call(pass,"Interact",hero);await Until(()=>hero.transform.position.x>63000&&!Busy(),"World Two arrival");Quiet();}
 var cases=All("DiceFree.World.TimedInteractionTarget").Where(c=>(string)Read(c,"ContentId") is "work.world2.needle-survey" or "work.world2.crown-survey").ToArray();Need(cases.Length==2&&cases.All(c=>(bool)Call(c,"Available",hero)),"Both survey sites remain available for the restored active quest");
 foreach(var target in cases)await Work(target);var ivara=All("DiceFree.Quests.QuestGiver").Single(g=>g.name=="Expedition Warden Ivara");Warp((Vector3)Read(ivara,"ApproachPosition"));Need((bool)Call(ivara,"TurnIn",hero,Quest("quest.world2.two-watchfires")),"Finish survey progress restored for visual QA");
 Need(new[]{"quest.world2.first-foothold","quest.world2.two-watchfires","quest.world2.unfinished-record"}.All(id=>Status(id)=="Completed"),"Copied QA profile ends with all three quests complete");Need((bool)Call(Part(hero,"DiceFree.Persistence.ManifestationPersistence"),"Flush"),"Completed chapter and equipped rewards flush to disk");Need(errors.Count==0,"No runtime errors during reload check");
 System.IO.File.WriteAllText(System.IO.Path.Combine(qa,"dragon-expedition-reload.json"),"{\"status\":\"passed\",\"checks\":"+checks.Count+",\"passed\":["+string.Join(",",checks.Select(Q))+"]}");
 }catch(Exception e){System.IO.File.WriteAllText(System.IO.Path.Combine(qa,"dragon-expedition-reload.json"),e.ToString());}finally{Application.logMessageReceived-=Log;}
}
Application.runInBackground=true;Application.logMessageReceived+=Log;_ = ReloadReview();return "Actual copied-save restart check scheduled.";