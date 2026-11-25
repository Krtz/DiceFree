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
void Report(string status)=>System.IO.File.WriteAllText(System.IO.Path.Combine(qa,"marksman-reload.json"),"{\"status\":"+Q(status)+",\"phase\":"+Q(phase)+",\"checks\":"+checks.Count+",\"passed\":["+string.Join(",",checks.Select(Q))+"],\"error\":"+Q(failure)+",\"unityErrors\":["+string.Join(",",errors.Select(Q))+"]}");
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
async System.Threading.Tasks.Task ReviewReload()
{
 try{
 var menu=All("DiceFree.Advancement.StartMenuController").Single();Need((bool)Call(menu,"TryPlayManifestation","class.marksman"),"Reload owned Marksman from the real save file");await Until(()=>(hero=All("DiceFree.Characters.TraversalInput").FirstOrDefault())!=null&&!Busy()&&(bool)Read(Part(hero,"DiceFree.Persistence.ManifestationPersistence"),"Ready"),"Reloaded hero");hero=Part(hero,"DiceFree.Combat.CombatActor");Quiet();((Behaviour)Part(hero,"DiceFree.Characters.CombatInput")).enabled=false;((Behaviour)Part(hero,"DiceFree.UI.WorldHoverFeedback")).enabled=false;
 var persistence=Part(hero,"DiceFree.Persistence.ManifestationPersistence");var controller=Part(hero,"DiceFree.Advancement.AdvancementController");var progress=Part(hero,"DiceFree.Skills.MarksmanProgression");var experience=Part(hero,"DiceFree.Progression.ExperienceProgression");var resource=Part(hero,"DiceFree.Combat.ActorResourceController");var caster=Part(hero,"DiceFree.Skills.MarksmanCaster");Need((int)Read(experience,"Level")==150,"Marksman class level survives disk reload");Need(Enumerable.Range(0,6).Select(i=>(int)Call(progress,"Rank",i)).SequenceEqual(new[]{30,25,20,30,20,25}),"All six exact rank caps survive disk reload");Need((int)Read(progress,"UnspentPoints")==0,"No skill point duplication on reload");Need(Mathf.Abs((float)Call(resource,"Maximum","resource.mana")-970)<.02f,"Reloaded Marksman has global INT-based Mana");Need((bool)Read(Part(hero,"DiceFree.Items.Equipment"),"UsesTwoHandedStance"),"Saved longbow remains equipped as two-handed");await Capture("marksman-saved-appearance",18);await System.Threading.Tasks.Task.Delay(200);Need(All("DiceFree.Combat.GroundAuraVisual").Any(v=>Read(v,"Style").ToString()=="Ranged"&&(bool)Read(v,"Visible")),"Ranged-damage aura sigil is visible after reload");
 var add=persistence.GetType().GetMethod("AddStarterGear",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance);var saveType=add.GetParameters()[0].ParameterType;object Blank(string id){var value=Activator.CreateInstance(saveType);Set(value,"classId",id);var inv=saveType.GetField("inventory").FieldType;var equip=saveType.GetField("equipment").FieldType;Set(value,"inventory",Array.CreateInstance(inv.GetElementType(),0));Set(value,"equipment",Array.CreateInstance(equip.GetElementType(),0));return value;}
 var defs=((System.Collections.IEnumerable)Read(Part(hero,"DiceFree.Items.CarriedInventory"),"Definitions")).Cast<object>().ToArray();Array Gear(params object[] items){var array=Array.CreateInstance(T("DiceFree.Items.ItemDefinition"),items.Length);for(int i=0;i<items.Length;i++)array.SetValue(items[i],i);return array;}
 var bow=defs.Single(d=>(string)Read(d,"stableId")=="item.trimery.ironpine-longbow");var off=defs.First(d=>Read(d,"slot").ToString()=="OffHand");var blank=Blank("class.marksman");bool rejected=false;try{add.Invoke(persistence,new[]{blank,(object)Gear(bow,off)});}catch(System.Reflection.TargetInvocationException e){rejected=e.InnerException is InvalidOperationException;}Need(rejected&&((Array)Read(blank,"inventory")).Length==0&&((Array)Read(blank,"equipment")).Length==0,"Conflicting two-handed plus offhand starter loadout rejects atomically");
 bool Compatible(object d)=>((string[])Read(d,"allowedClassIds")).Length==0||((string[])Read(d,"allowedClassIds")).Contains("class.psionic-knight");var one=defs.First(d=>Read(d,"slot").ToString()=="MainHand"&&(int)Read(d,"handsRequired")==1&&(int)Read(d,"itemLevel")==1&&Compatible(d));var side=defs.First(d=>Read(d,"slot").ToString()=="OffHand"&&(int)Read(d,"handsRequired")==1&&(int)Read(d,"itemLevel")==1&&Compatible(d));var separate=Blank("class.psionic-knight");add.Invoke(persistence,new[]{separate,(object)Gear(one,side)});Need(((Array)Read(separate,"equipment")).Length==2,"Existing one-handed main/offhand starter loadouts remain supported");
 Call(resource,"RefillAll");Need((bool)Call(caster,"CastStrafe"),"Reloaded class can activate Strafe");Set(Read(hero,"Health"),"Invulnerable",false);var lethal=Activator.CreateInstance(T("DiceFree.Combat.DamageResult"));Set(lethal,"raw",1000000f);Set(lethal,"mitigated",1000000f);Call(Read(hero,"Health"),"ApplyPacket",hero,lethal);Need(!(bool)Read(hero,"Alive")&&(float)Read(caster,"strafeUntil")==0&&!(bool)Read(caster,"IsChanneling"),"Death immediately clears Strafe and channels");Need((float)T("DiceFree.Skills.MarksmanCaster").GetMethod("AuraMultiplier").Invoke(null,new object[]{hero})==1,"Dead Marksman cannot supply its aura");Call(Read(hero,"Health"),"Restore",1f);Need((bool)Read(hero,"Alive"),"Copied test hero restores cleanly after death cleanup check");
 Need((bool)Call(controller,"TrySwitch","class.ranger"),"Reload preserved Ranger from disk");journal=Part(hero,"DiceFree.Quests.QuestJournal");Need((int)Read(experience,"Level")==60&&Status("quest.marksman.world-tour")=="Completed","Parent level and completed advancement survive disk reload");var state=Progress("quest.marksman.world-tour");Need(new[]{"cornberg","trimery","arborium","cinderwatch","willowford","anvilreach"}.All(id=>(int)T("DiceFree.Quests.ObjectiveProgress").GetMethod("Count").Invoke(null,new[]{state,(object)id})==1),"All six settlement checks survive disk reload");Need((int)Call(Part(hero,"DiceFree.Skills.RangerSkillProgression"),"Rank",E("DiceFree.Skills.RangerSkill","BowMastery"))==1,"Parent Ranger skill investment preserved independently");Need((bool)Call(controller,"TrySwitch","class.marksman"),"Switch back to the saved Marksman");Need((bool)Call(Part(hero,"DiceFree.Dungeons.ReturnToStartMenu"),"Return"),"Reload review returns normally to menu");await Until(()=>All("DiceFree.Advancement.StartMenuController").Length>0&&!Busy(),"Menu after reload review");Need(errors.Count==0,"No runtime errors through reload, equipment and death cleanup review");Report("passed");
 }catch(Exception e){failure=e.ToString();Report("failed");}finally{Application.logMessageReceived-=Log;}
}
void Log(string message,string stack,LogType type){if(type is LogType.Error or LogType.Exception or LogType.Assert)errors.Add(message);}
Application.logMessageReceived+=Log;_ = ReviewReload();return "Marksman actual disk reload and cleanup review scheduled";
