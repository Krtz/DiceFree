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
void Report(string status)=>System.IO.File.WriteAllText(System.IO.Path.Combine(qa,"fire-mage-staff.json"),"{\"status\":"+Q(status)+",\"phase\":"+Q(phase)+",\"checks\":"+checks.Count+",\"passed\":["+string.Join(",",checks.Select(Q))+"],\"error\":"+Q(failure)+",\"unityErrors\":["+string.Join(",",errors.Select(Q))+"]}");
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
object persistence=null,resources=null,controller=null,caster=null,skills=null,basic=null;
const string HeatId="resource.heat",Trial="quest.fire-mage.ember-walk";
var fixtures=new System.Collections.Generic.List<GameObject>();var owned=new System.Collections.Generic.List<UnityEngine.Object>();
object Def(string id)=>Call(persistence,"ResolveClass",id);
bool OutCall(string method,object argument){var args=new object[]{argument,null};return (bool)persistence.GetType().GetMethod(method).Invoke(persistence,args);}
void Select(string id){if((string)Read(persistence,"CurrentClassId")==id)return;bool exists=(bool)Call(controller,"HasManifestation",id);Need(OutCall(exists?"TryActivateExisting":"TryForkAndActivate",exists?(object)id:Def(id)),"Activate fixture class "+id);}
void Refs(){journal=Part(hero,"DiceFree.Quests.QuestJournal");inventory=Part(hero,"DiceFree.Items.CarriedInventory");xp=Part(hero,"DiceFree.Progression.ExperienceProgression");resources=Part(hero,"DiceFree.Combat.ActorResourceController");controller=Part(hero,"DiceFree.Advancement.AdvancementController");caster=Part(hero,"DiceFree.Combat.FireMageCaster");skills=Part(hero,"DiceFree.Skills.FireMageProgression");basic=Part(hero,"DiceFree.Combat.BasicAttack");}
void Ready(float heat=0){Call(resources,"RefillAll");Call(caster,"ResetForManifestationLoad");Call(resources,"SetCurrentForValidation",HeatId,heat);Call(Read(hero,"Effects"),"ClearTransient");Call(basic,"Cancel");Call(Read(hero,"Health"),"Restore",1f);Set(Read(hero,"Health"),"Invulnerable",true);}
Component Unit(string name,Vector3 at)
{
 var definition=UnityEngine.Object.Instantiate((UnityEngine.Object)Def("class.greatblade"));owned.Add(definition);Set(definition,"stableId","enemy.qa.fire-mage");Set(definition,"baseHp",500000f);Set(definition,"growth",Activator.CreateInstance(T("DiceFree.Combat.AttributeValues")));Set(definition,"physicalDefense",100f);Set(definition,"magicalDefense",100f);
 var go=new GameObject(name);go.SetActive(false);go.transform.position=at;fixtures.Add(go);var stats=go.AddComponent(T("DiceFree.Combat.ActorStats"));Call(stats,"Configure",definition,1);go.AddComponent<UnityEngine.AI.NavMeshAgent>().enabled=false;((Behaviour)go.AddComponent(T("DiceFree.Characters.TraversalMotor"))).enabled=false;go.AddComponent(T("DiceFree.Combat.Health"));var body=go.AddComponent(T("DiceFree.Combat.CombatActor"));Call(body,"Configure",1,.30f,null);go.AddComponent(T("DiceFree.Combat.CombatStatusController"));go.AddComponent(T("DiceFree.Combat.BasicAttack"));go.SetActive(true);((Behaviour)Part((Component)body,"DiceFree.Combat.BasicAttack")).enabled=false;return (Component)body;
}
var packets=new System.Collections.Generic.List<(string target,object packet)>();var hooks=new System.Collections.Generic.List<(object health,System.Reflection.EventInfo info,Delegate handler)>();
void Watch(Component body){var health=Read(body,"Health");var info=health.GetType().GetEvent("PacketResolved");Action<object,object> record=(source,packet)=>{if(source is Component c&&c.gameObject==hero.gameObject)packets.Add((body.name,packet));};var a=System.Linq.Expressions.Expression.Parameter(T("DiceFree.Combat.CombatActor"));var b=System.Linq.Expressions.Expression.Parameter(T("DiceFree.Combat.DamageResult"));var handler=System.Linq.Expressions.Expression.Lambda(info.EventHandlerType,System.Linq.Expressions.Expression.Invoke(System.Linq.Expressions.Expression.Constant(record),System.Linq.Expressions.Expression.Convert(a,typeof(object)),System.Linq.Expressions.Expression.Convert(b,typeof(object))),a,b).Compile();info.AddEventHandler(health,handler);hooks.Add((health,info,handler));}
void ClearFixtures(){foreach(var h in hooks)h.info.RemoveEventHandler(h.health,h.handler);hooks.Clear();foreach(var go in fixtures)if(go!=null)UnityEngine.Object.Destroy(go);fixtures.Clear();foreach(var o in owned)if(o!=null)UnityEngine.Object.Destroy(o);owned.Clear();packets.Clear();}
bool CriticalTriggered(object packet)=>Read(packet,"critical") is object c&&(bool)Read(c,"Triggered");
void Incoming(Component source,float amount,object element=null){var p=Activator.CreateInstance(T("DiceFree.Combat.DamageResult"));Set(p,"raw",amount);Set(p,"mitigated",amount);Set(p,"element",element);Call(Read(hero,"Health"),"ApplyPacket",source,p);}
async System.Threading.Tasks.Task PlaySaved(string id)
{
 Component menu=null;await Until(()=>(menu=All("DiceFree.Advancement.StartMenuController").FirstOrDefault())!=null&&!Busy(),"Menu");Need((bool)Call(menu,"TryPlayManifestation",id),"Load saved manifestation "+id);await Until(()=>(hero=All("DiceFree.Characters.TraversalInput").FirstOrDefault(c=>Part(c,"DiceFree.Persistence.ManifestationPersistence")!=null))!=null&&!Busy()&&(bool)Read(Part(hero,"DiceFree.Persistence.ManifestationPersistence"),"Ready"),"Hero ready");hero=Part(hero,"DiceFree.Combat.CombatActor");persistence=Part(hero,"DiceFree.Persistence.ManifestationPersistence");Refs();Quiet();((Behaviour)Part(hero,"DiceFree.Characters.CombatInput")).enabled=false;((Behaviour)Part(hero,"DiceFree.UI.WorldHoverFeedback")).enabled=false;
}
void CompleteTravelFixture()
{
 var id=(string)T("DiceFree.Dungeons.AncientRoadDragonLairPortal").GetField("QuestId").GetValue(null);var def=Quest(id);var records=(Array)Call(journal,"CaptureState");var state=records.Cast<object>().Single(r=>(string)Read(r,"questId")==id);var stages=(Array)Read(def,"stages");Set(state,"stage",stages.Length-1);Set(state,"count",(int)Read(stages.GetValue(stages.Length-1),"count"));Set(state,"status",E("DiceFree.Quests.QuestStatus","Completed"));Set(state,"alternatives",Array.CreateInstance(T("DiceFree.Quests.ObjectiveCount"),0));Call(journal,"RestoreState",records);
}
async System.Threading.Tasks.Task Cross(bool two)
{
 var portal=All("DiceFree.ApplicationFlow.IronpineSouthPass").Single(p=>p.gameObject.scene.name==(two?"Cornberg":"WorldTwoArrival"));if(two)await Until(()=>(bool)Read(portal,"IsOpen"),"Normal dragon-open gate",20);Warp((Vector3)Read(portal,"ApproachPosition"));Call(portal,"Interact",hero);await Until(()=>(hero.transform.position.x>63000)==two&&!Busy(),"Ordinary world transition",100);Quiet();
}
void Recast(){Array.Clear((float[])Read(caster,"readyAt"),0,6);Call(resources,"RefillAll");Call(resources,"SetCurrentForValidation",HeatId,0f);}
async System.Threading.Tasks.Task Wait(float seconds)=>await System.Threading.Tasks.Task.Delay((int)(seconds*1000));
float CurrentHp()=>(float)Read(Read(hero,"Health"),"Current");
float Mana()=>(float)Call(resources,"Current","resource.mana");
float Heat()=>(float)Read(caster,"Heat");
float Raw(object packet)=>(float)Read(packet,"raw");
async System.Threading.Tasks.Task Portrait(Animator source,string state,string file,bool side=false)
{
 var studio=UnityEngine.Object.Instantiate(source.transform.parent.gameObject);studio.transform.SetParent(null);studio.transform.position=new Vector3(95000,0,95000);studio.transform.rotation=Quaternion.identity;studio.SetActive(true);
 foreach(var c in studio.GetComponentsInChildren<MonoBehaviour>(true))c.enabled=false;foreach(var t in studio.GetComponentsInChildren<Transform>(true))t.gameObject.layer=29;
 var a=studio.GetComponentInChildren<Animator>();a.enabled=true;a.cullingMode=AnimatorCullingMode.AlwaysAnimate;a.Play(state,0,.35f);a.Update(0);a.enabled=false;
 var presentation=studio.GetComponentsInChildren<Component>(true).First(c=>c!=null&&c.GetType().FullName=="DiceFree.UI.FireMageStaffPresentation");Call(presentation,"ApplyPortraitPose");var grip=(Transform)Read(presentation,"staff");Need(Vector3.Distance(grip.position,a.GetBoneTransform(HumanBodyBones.RightHand).position)<.01f,"Disabled portrait rig updates staff for "+state);
 var rs=studio.GetComponentsInChildren<Renderer>().Where(r=>r.enabled).ToArray();var bounds=rs[0].bounds;foreach(var r in rs.Skip(1))bounds.Encapsulate(r.bounds);
 var cam=new GameObject("Magician proof camera").AddComponent<Camera>();cam.enabled=false;cam.cullingMask=1<<29;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.045f,.055f,.075f);cam.fieldOfView=30;cam.nearClipPlane=.03f;cam.farClipPlane=30;cam.transform.position=bounds.center+(side?new Vector3(1,.07f,.05f):new Vector3(.10f,.07f,1)).normalized*bounds.size.y*2.2f;cam.transform.LookAt(bounds.center);
 var target=new RenderTexture(1200,1200,24);var active=RenderTexture.active;bool fog=RenderSettings.fog;RenderSettings.fog=false;cam.targetTexture=target;cam.Render();RenderTexture.active=target;var texture=new Texture2D(1200,1200,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,1200,1200),0,0);texture.Apply();System.IO.File.WriteAllBytes(System.IO.Path.Combine(qa,file+".png"),texture.EncodeToPNG());RenderTexture.active=active;RenderSettings.fog=fog;UnityEngine.Object.Destroy(studio);UnityEngine.Object.Destroy(cam.gameObject);UnityEngine.Object.Destroy(target);UnityEngine.Object.Destroy(texture);await Wait(.1f);
}
async System.Threading.Tasks.Task Review()
{
 try
 {
 Phase("Saved class and rig");await PlaySaved("class.fire-mage");Need((int)Read(xp,"Level")==150,"Existing level150 save loads selected model");Need(new[]{35,25,30,30,15,15}.SequenceEqual(Enumerable.Range(0,6).Select(i=>(int)Call(skills,"Rank",i))),"All150 learned ranks preserved");Need(Heat()==0&&!(bool)Read(caster,"Busy"),"Class resources initialize cleanly");
 var a=hero.GetComponentsInChildren<Animator>().First(v=>v.enabled&&v.gameObject.activeInHierarchy);Need(a.isHuman&&a.avatar.isValid,"Standalone humanoid avatar valid");Need(a.GetComponentsInChildren<Transform>().Any(t=>t.name=="EmberMagician"),"Selected Magician body active");var p=hero.GetComponentsInChildren<Component>().First(c=>c!=null&&c.GetType().FullName=="DiceFree.UI.FireMageStaffPresentation");var grip=(Transform)Read(p,"staff");await Wait(.4f);Need(Vector3.Distance(grip.position,a.GetBoneTransform(HumanBodyBones.RightHand).position)<.02f,"Live original staff follows right hand");Need(a.GetComponentsInChildren<Renderer>().All(r=>r.bounds.size.magnitude<6),"No enlarged attachment or exploded skin in player");Need(p.GetType().GetInterfaces().Any(t=>t.Name=="IPortraitBonePose"),"Holding arm is posed before portrait props");Need(Vector3.Dot(a.GetBoneTransform(HumanBodyBones.RightHand).position-a.GetBoneTransform(HumanBodyBones.Head).position,a.transform.right)>.35f,"Live staff hand is beside the head");await Portrait(a,"Idle","staff-portrait");await Portrait(a,"Cast","staff-cast");await Portrait(a,"Locomotion","staff-walk");await Portrait(a,"Idle","staff-idle-side",true);await Portrait(a,"Cast","staff-cast-side",true);
 Warp(new Vector3(248,0,156));hero.transform.rotation=Quaternion.Euler(0,210,0);await Capture("staff-gameplay",9);
 Phase("Normal cast with selected rig");((Behaviour)resources).enabled=false;Ready();var enemy=Unit("Magician fireball target",Nav(hero.transform.position+hero.transform.forward*3));Watch(enemy);float manaBefore=Mana();Need((bool)Call(caster,"Cast",0,enemy),"Normal Fireball accepted with new rig");await Wait(1.2f);Need(packets.Count==1&&Mathf.Abs(Raw(packets[0].packet)-(float)Call(caster,"PreviewDamage",0,35))<.02f,"Fireball damage unchanged");Need(Heat()==25&&Mathf.Abs(manaBefore-Mana()-545)<.1f,"Fireball Mana and Heat unchanged");Need(Vector3.Distance(grip.position,a.GetBoneTransform(HumanBodyBones.RightHand).position)<.02f,"Staff tracks hand after real casting animation");ClearFixtures();
 Phase("Parent class and mentor");((Behaviour)resources).enabled=true;Select("class.mage");Refs();Need((int)Read(xp,"Level")==60&&Status(Trial)=="Completed","Parent level60 and advancement completion preserved");if(hero.transform.position.x<63000)await Cross(true);var teacher=All("DiceFree.Advancement.FireMageMentor").Single();Warp((Vector3)Read(teacher,"ApproachPosition"));Need(teacher.GetComponentsInChildren<Component>().Any(c=>c!=null&&c.GetType().FullName=="DiceFree.UI.FireMageStaffPresentation"),"Maelis uses matching Magician model");Need(!(bool)Call(teacher,"AcceptTrial",hero)&&!(bool)Call(teacher,"CompleteTrial",hero),"Mentor cannot duplicate awarded class");await Capture("staff-maelis-camp",13);Select("class.fire-mage");Refs();Need((int)Read(xp,"Level")==150,"Independent Fire Mage restores after parent");Need((bool)Call(persistence,"Flush"),"Selected model saves cleanly");Need((bool)Call(Part(hero,"DiceFree.Dungeons.ReturnToStartMenu"),"Return"),"Normal return to class selection");await Until(()=>All("DiceFree.Advancement.StartMenuController").Length>0&&!Busy(),"Menu return");await Wait(1);Need(errors.Count==0,"No Unity errors during model review");Report("passed");
 }
 catch(Exception e){failure=e.ToString();Report("failed");}
 finally{ClearFixtures();Application.logMessageReceived-=Log;}
}
void Log(string message,string stack,LogType type){if(type is LogType.Error or LogType.Exception or LogType.Assert)errors.Add(message);}
Report("scheduled");Application.runInBackground=true;Application.logMessageReceived+=Log;_ = Review();return "Staff holding pose runtime review scheduled";