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
void Report(string status)=>System.IO.File.WriteAllText(System.IO.Path.Combine(qa,"world-two-routes-review.json"),"{\"status\":"+Q(status)+",\"phase\":"+Q(phase)+",\"checks\":"+checks.Count+",\"passed\":["+string.Join(",",checks.Select(Q))+"],\"error\":"+Q(failure)+",\"unityErrors\":["+string.Join(",",errors.Select(Q))+"]}");
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
Vector3 P(float x,float z)=>new(64000+x,0,64000+z);
async System.Threading.Tasks.Task Walk(Vector3 destination)
{
 var motor=Read(hero,"Motor");var at=Nav(destination);Need((bool)Call(motor,"MoveTo",at),"Walking order: "+destination);await Until(()=>Vector3.Distance(hero.transform.position,at)<1.2f,"Route walking",50);Need(Vector3.Distance(hero.transform.position,at)<1.2f,"Ordinary walking reaches route destination");
}
async System.Threading.Tasks.Task ReviewRoutes()
{
 try
 {
 Phase("Copied save and normal World Two crossing");var menu=All("DiceFree.Advancement.StartMenuController").Single();Need((bool)Call(menu,"TryPlayManifestation","class.psionic-knight"),"Load copied hero");await Until(()=>(hero=All("DiceFree.Characters.TraversalInput").FirstOrDefault())!=null&&!Busy()&&(bool)Read(Part(hero,"DiceFree.Persistence.ManifestationPersistence"),"Ready"),"Saved hero");hero=Part(hero,"DiceFree.Combat.CombatActor");Quiet();((Behaviour)Part(hero,"DiceFree.Characters.CombatInput")).enabled=false;((Behaviour)Part(hero,"DiceFree.UI.WorldHoverFeedback")).enabled=false;
 if(hero.transform.position.x<63000){var pass=All("DiceFree.ApplicationFlow.IronpineSouthPass").Single(p=>p.gameObject.scene.name=="Cornberg");Warp((Vector3)Read(pass,"ApproachPosition"));Call(pass,"Interact",hero);await Until(()=>hero.transform.position.x>63000&&!Busy(),"World Two crossing",100);}Quiet();
 var bounds=(Rect)T("DiceFree.World.AshfallReach").GetField("Bounds").GetValue(null);Call(View(),"ConfigureMapBounds",bounds);Call(All("DiceFree.World.WorldDayNightCycle").Single(),"SetHour",15f,false);
 var surfaces=All("DiceFree.World.TravelSurface").Where(s=>s.gameObject.scene.name=="WorldTwoArrival").ToArray();Need(surfaces.Length==18,"All sixteen road and paving surfaces plus two bridge decks are registered");Need(surfaces.All(s=>Mathf.Approximately((float)Read(Read(s,"Definition"),"speedBonusPercent"),15)),"All World Two routes use the standard 15 percent bonus");Need(surfaces.Select(s=>(string)Read(s,"StableId")).Distinct().Count()==18,"Every route instance has a distinct stable ID");
 Phase("Visible footprint coverage and actual actor speed");var travel=(Behaviour)Part(hero,"DiceFree.World.SurfaceTravel");var motor=Read(hero,"Motor");int sampled=0;
 foreach(var surface in surfaces)
 {
  var filter=(MeshFilter)Read(surface,"footprint");var mesh=filter.sharedMesh;var vertices=mesh.vertices;var triangles=mesh.triangles;Vector3? representative=null;int accepted=0;var missed=new System.Collections.Generic.List<string>();
  for(int i=0;i<triangles.Length;i+=Math.Max(3,(triangles.Length/100/3)*3))
  {
   var a=filter.transform.TransformPoint(vertices[triangles[i]]);var b=filter.transform.TransformPoint(vertices[triangles[i+1]]);var c=filter.transform.TransformPoint(vertices[triangles[i+2]]);if(Vector3.Cross(b-a,c-a).y<=.00001f)continue;
   var point=(a+b+c)/3;var probe=point;if(Physics.Raycast(point+Vector3.up*100,Vector3.down,out var floor,220,1<<8))probe=floor.point;
   if(!UnityEngine.AI.NavMesh.SamplePosition(probe,out var hit,.8f,1)||Vector2.Distance(new(point.x,point.z),new(hit.position.x,hit.position.z))>.25f)continue;
   if(!surfaces.Any(s=>(bool)Call(s,"Contains",hit.position)))missed.Add((hit.position-new Vector3(64000,0,64000)).ToString());
   else{accepted++;representative??=hit.position;}
  }
  Need(missed.Count==0,"Navigable visible footprint has continuous bonus: "+surface.name+"; gaps="+string.Join(";",missed.Take(4)));Need(accepted>0,"Road has navigable samples: "+surface.name);sampled+=accepted;
  Warp(representative.Value);float on=(float)Read(motor,"Speed");Need(Read(travel,"Current")!=null,"Actor detects the road: "+surface.name);travel.enabled=false;float off=(float)Read(motor,"Speed");travel.enabled=true;Need(Mathf.Abs(on/off-1.15f)<.001f,"Actual actor speed is exactly +15 percent: "+surface.name);await System.Threading.Tasks.Task.Delay(20);
 }
 Need(sampled>800,"More than 800 navigable road footprint samples passed");
 var props=T("DiceFree.World.AshfallReach");int centers=0;var gaps=new System.Collections.Generic.List<string>();
 foreach(string name in new[]{"ArrivalTrail","SettlementRoad","WillowfordRoad","RiverbankTrail","NeedlePath","CrownPath","SouthernCircuit","WesternTrail","AnvilreachRoad","AnvilreachNorthRoad"})
 {
  var points=((System.Collections.IEnumerable)props.GetProperty(name).GetValue(null)).Cast<Vector2>().ToArray();
  for(int i=1;i<points.Length;i++){int n=Mathf.CeilToInt(Vector2.Distance(points[i-1],points[i])/2);for(int j=0;j<=n;j++){
   var p=Vector2.Lerp(points[i-1],points[i],j/(float)Math.Max(1,n));var probe=P(p.x,p.y);if(Physics.Raycast(probe+Vector3.up*120,Vector3.down,out var floor,250,1<<8))probe=floor.point;
   if(!UnityEngine.AI.NavMesh.SamplePosition(probe,out var hit,1,1)||Vector2.Distance(p,new(hit.position.x-64000,hit.position.z-64000))>.35f)continue;
   if(!surfaces.Any(s=>(bool)Call(s,"Contains",hit.position)))gaps.Add(name+" "+p);centers++;
  }}await System.Threading.Tasks.Task.Delay(5);
 }
 Need(gaps.Count==0,"Every navigable two-metre centerline sample has road speed; gaps="+string.Join(";",gaps.Take(10)));Need(centers>1000,"More than 1000 route centerline samples checked");
 Warp(P(65,-90));Need(Read(travel,"Current")==null,"Off-road ground does not receive the bonus");
 Phase("Crossings and shared paving do not stack");Warp(P(92,-205));float stacked=(float)Read(motor,"Speed");travel.enabled=false;float baseline=(float)Read(motor,"Speed");travel.enabled=true;Need(Mathf.Abs(stacked/baseline-1.15f)<.001f,"Overlapping city roads and paving give one bonus");
 Warp(P(-168,-62));await Walk(P(-195,-62));Warp(P(-196,-318));await Walk(P(-169,-318));Warp(P(23,-205));await Walk(P(42,-205));await Walk(P(92,-205));await Capture("anvilreach-stocked-market",34);Warp(P(119,-211));await Capture("anvilreach-stocked-foundry",30);Warp(P(-186,-318));await Capture("steamroot-river-groves",36);Warp(P(-143,-438));await Capture("deepmoss-rooted-groves",36);Warp(P(5,-40));await Capture("cinderwatch-road",32);
 Phase("Library integration and existing services");var world=All("DiceFree.World.WorldNavigation").Single(c=>c.gameObject.scene.name=="WorldTwoArrival");var nodes=world.GetComponentsInChildren<Transform>();var replacements=nodes.Where(t=>t.name=="Library replacement").ToArray();Need(replacements.Length==1083,"Six stocked counters, 211 trees and 866 rocks are saved");Need(replacements.All(t=>t.GetComponentsInChildren<Renderer>().Any(r=>r.enabled)),"Every replacement has visible geometry");Need(replacements.All(t=>t.GetComponentsInChildren<MeshFilter>().All(f=>f.sharedMesh!=null)),"No replacement has a missing mesh");Need(replacements.All(t=>t.GetComponentsInChildren<Renderer>().All(r=>r.sharedMaterials.All(m=>m!=null&&m.shader!=null&&m.shader.name!="Hidden/InternalErrorShader"))),"Replacement materials have valid shipped shaders");Need(replacements.All(t=>t.GetComponentsInChildren<Collider>().Length==0),"Visual replacements add no changed navigation or collision footprints");
 var city=world.transform.Find("Anvilreach and the southern biomes/Anvilreach - city of the hearth");Need(city.Find("Anvilreach Banker")!=null&&city.Find("Master smith Branna")!=null&&city.Find("Mine foreman Dorrin")!=null,"City service roots are preserved");Need(All("DiceFree.Core.TreeCameraOccluder").Count(t=>t.transform.Find("Library replacement")!=null)==211,"All replaced trees retain canopy fading");Need(errors.Count==0,"No runtime errors through World Two route and art review");Report("passed");
 }
 catch(Exception e){failure=e.ToString();Report("failed");}
 finally{Application.logMessageReceived-=Log;}
}
void Log(string message,string stack,LogType type){if(type is LogType.Error or LogType.Exception or LogType.Assert)errors.Add(message);}
Report("scheduled");Application.runInBackground=true;Application.logMessageReceived+=Log;_ = ReviewRoutes();return "World Two routes and library review scheduled";
