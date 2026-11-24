// Run with Unity CLI eval_file in a development player using an isolated save copy.
var qa=System.IO.Path.Combine(System.IO.Directory.GetParent(Application.dataPath).FullName,"QA");
System.IO.Directory.CreateDirectory(qa);
var checks=new System.Collections.Generic.List<string>();var errors=new System.Collections.Generic.List<string>();
Component player=null,feedback=null;string phase="startup",failure=null;
Type T(string n)=>AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType(n)).First(t=>t!=null);
Component[] All(string n)=>UnityEngine.Object.FindObjectsByType(T(n)).Cast<Component>().ToArray();
Component Part(Component c,string n)=>c.GetComponent(T(n));
object Read(object c,string n)=>c?.GetType().GetProperty(n,System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic)?.GetValue(c)??c?.GetType().GetField(n,System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic)?.GetValue(c);
void Set(object c,string n,object v){var p=c.GetType().GetProperty(n,System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic);if(p?.CanWrite==true)p.SetValue(c,v);else c.GetType().GetField(n,System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic).SetValue(c,v);}
object Call(object c,string n,params object[] a){var m=c.GetType().GetMethods(System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic).Where(m=>m.Name==n).First(m=>{var p=m.GetParameters();return p.Length>=a.Length&&p.Take(a.Length).Select((v,i)=>a[i]==null||v.ParameterType.IsInstanceOfType(a[i])).All(v=>v)&&p.Skip(a.Length).All(v=>v.IsOptional);});return m.Invoke(c,m.GetParameters().Select((v,i)=>i<a.Length?a[i]:v.DefaultValue).ToArray());}
object Static(string t,string n,params object[] a){var m=T(t).GetMethods().Where(m=>m.IsStatic&&m.Name==n).First(m=>m.GetParameters().Length==a.Length&&m.GetParameters().Select((p,i)=>a[i]==null||p.ParameterType.IsInstanceOfType(a[i])).All(v=>v));return m.Invoke(null,a);}
string Q(string s)=>s==null?"null":"\""+s.Replace("\\","\\\\").Replace("\"","\\\"").Replace("\r"," ").Replace("\n"," ")+"\"";
void Report(string status)=>System.IO.File.WriteAllText(System.IO.Path.Combine(qa,"pointer-feedback-review.json"),"{\"status\":"+Q(status)+",\"phase\":"+Q(phase)+",\"checks\":"+checks.Count+",\"passed\":["+string.Join(",",checks.Select(Q))+"],\"error\":"+Q(failure)+",\"unityErrors\":["+string.Join(",",errors.Select(Q))+"]}");
void Need(bool value,string label){if(!value)throw new Exception(label);checks.Add(label);Report("running");}
void Phase(string value){phase=value;Report("running");}
bool Busy()=>(bool)T("DiceFree.ApplicationFlow.LoadingScreen").GetProperty("Busy").GetValue(null)||(bool)T("DiceFree.ApplicationFlow.WorldFaceCrossing").GetProperty("Busy").GetValue(null);
async System.Threading.Tasks.Task Until(Func<bool> condition,string label,int seconds=70){var end=DateTime.UtcNow.AddSeconds(seconds);while(!condition()){if(DateTime.UtcNow>end)throw new TimeoutException(label);await System.Threading.Tasks.Task.Delay(50);}}
Vector3 Nav(Vector3 p){if(Physics.Raycast(p+Vector3.up*80,Vector3.down,out var ground,180,1<<8))p=ground.point;if(!UnityEngine.AI.NavMesh.SamplePosition(p,out var hit,4,1))throw new Exception("No navigation at "+p);return hit.position;}
void Warp(Vector3 p){if(!(bool)Call(Read(player,"Motor"),"TeleportAcrossMaps",Nav(p)))throw new Exception("Teleport failed");Call(All("DiceFree.Core.ExplorationCamera").First(),"RecenterForTravel",player.transform);Call(All("DiceFree.World.WorldFogOfWar").Single(),"RefreshVisionNow");}
void Quiet(){foreach(var ai in All("DiceFree.AI.AggroBehaviour")){((Behaviour)ai).enabled=false;var attack=Part(ai,"DiceFree.Combat.BasicAttack");if(attack!=null)Call(attack,"Cancel");}Set(Read(player,"Health"),"Invulnerable",true);}
object Pick(Vector2 point)=>Static("DiceFree.World.WorldTargetPicker","Pick",player,Camera.main,point);
object Shape(Component target)=>Static("DiceFree.World.WorldSelectionShape","For",target);
Bounds BoundsOf(Component target){var shape=Shape(target);object[] args={default(Bounds)};if(!(bool)shape.GetType().GetMethod("TryBounds").Invoke(shape,args))throw new Exception("No selection bounds for "+target.name);return (Bounds)args[0];}
bool Covered(Vector2 point)=>(bool)Static("DiceFree.UI.HudPointerBlocker","Covers",point);
Vector2 PointFor(Component target, bool outsideBody=false)
{
 var bounds=BoundsOf(target);var center=Camera.main.WorldToScreenPoint(bounds.center);var corners=new[]{bounds.min,bounds.max,new Vector3(bounds.min.x,bounds.max.y,bounds.max.z),new Vector3(bounds.max.x,bounds.min.y,bounds.min.z)}.Select(Camera.main.WorldToScreenPoint).ToArray();
 float x0=corners.Min(v=>v.x)-5,x1=corners.Max(v=>v.x)+5,y0=corners.Min(v=>v.y)-5,y1=corners.Max(v=>v.y)+5;
 var samples=new System.Collections.Generic.List<Vector2>{center};for(float y=y0;y<=y1;y+=2)for(float x=x0;x<=x1;x+=2)samples.Add(new Vector2(x,y));
 foreach(var p in samples)
 {
  if(Covered(p)||p.x<0||p.y<0||p.x>=Screen.width||p.y>=Screen.height||Read(Pick(p),"Target") as Component!=target)continue;
  if(outsideBody&&Physics.RaycastAll(Camera.main.ScreenPointToRay(p),1500,1<<10).Any(h=>h.collider.GetComponentInParent(T("DiceFree.Combat.CombatActor"))==target))continue;
  return p;
 }
 throw new Exception("No unobstructed pointer point for "+target.name+" outsideBody="+outsideBody);
}
void Mouse(Vector2 position,bool left=false)
{
 var stateType=T("UnityEngine.InputSystem.LowLevel.MouseState");var state=Activator.CreateInstance(stateType);
 stateType.GetField("position").SetValue(state,position);stateType.GetField("buttons").SetValue(state,(ushort)(left?1:0));
 var mouse=T("UnityEngine.InputSystem.Mouse").GetProperty("current").GetValue(null);
 var method=T("UnityEngine.InputSystem.InputSystem").GetMethods().Single(m=>m.Name=="QueueStateEvent"&&m.IsGenericMethodDefinition&&m.GetParameters().Length==3);
 method.MakeGenericMethod(stateType).Invoke(null,new object[]{mouse,state,-1d});
 Set(feedback,"focused",true);
}
System.Threading.Tasks.Task Hover(Vector2 point)
{
 // Assert against the same frame used to choose this point: NPCs and posed
 // model silhouettes keep moving while a real pointer stays in place.
 Mouse(point);Call(feedback,"RefreshPointer",point);return System.Threading.Tasks.Task.CompletedTask;
}
async System.Threading.Tasks.Task Capture(string name,Vector2 point)
{
 var target=Read(feedback,"CurrentTarget") as Component;
 Set(feedback,"SoftwareCursor",true);Mouse(point);await System.Threading.Tasks.Task.Delay(300);Call(feedback,"RefreshPointer",point);
 if(target!=null){point=PointFor(target);Call(feedback,"RefreshPointer",point);}
 ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(qa,name+".png"));await System.Threading.Tasks.Task.Delay(200);
 Set(feedback,"SoftwareCursor",false);
}
void Log(string message,string trace,LogType type){if(type is LogType.Error or LogType.Exception or LogType.Assert)errors.Add(message);}
async System.Threading.Tasks.Task Review()
{
 var fixtures=new System.Collections.Generic.List<GameObject>();
 try
 {
  if(All("DiceFree.Characters.TraversalInput").Length==0)
  {
   Component menu=null;await Until(()=>(menu=All("DiceFree.Advancement.StartMenuController").FirstOrDefault())!=null&&!Busy(),"Menu ready");
   Need((bool)Call(menu,"TryPlayManifestation","class.psionic-knight"),"Load copied Knight manifestation");
  }
  else Need(true,"Continue isolated copied Knight manifestation");
  await Until(()=>(player=All("DiceFree.Characters.TraversalInput").FirstOrDefault())!=null&&!Busy()&&(bool)Read(Part(player,"DiceFree.Persistence.ManifestationPersistence"),"Ready"),"Hero ready");
  player=Part(player,"DiceFree.Combat.CombatActor");feedback=Part(player,"DiceFree.UI.WorldHoverFeedback");Quiet();Call(All("DiceFree.World.WorldDayNightCycle").First(),"SetHour",15f,false);
  Need(feedback!=null,"Player composes shared hover feedback at runtime");
  // The hidden QA window intentionally has no OS focus. Drive its public pointer
  // entry point explicitly; normal background focus handling correctly clears it.
  ((Behaviour)feedback).enabled=false;
  var combat=Part(player,"DiceFree.Characters.CombatInput");var selection=Part(player,"DiceFree.Combat.TargetSelection");var skill=Part(player,"DiceFree.UI.SkillTargetingController");
  Phase("Enemy geometry and real input");
  var enemy=All("DiceFree.Combat.CombatActor").Where(a=>(int)Read(a,"Faction")!=(int)Read(player,"Faction")&&(bool)Read(a,"Alive")&&a.gameObject.scene.name=="Cornberg").OrderBy(a=>Vector3.Distance(player.transform.position,a.transform.position)).First();
  Set(Read(enemy,"Health"),"Invulnerable",true);Warp(enemy.transform.position+new Vector3(-4,0,-4));await System.Threading.Tasks.Task.Delay(350);
  var enemyPoint=PointFor(enemy);await Hover(enemyPoint);
  Need(Read(feedback,"CurrentTarget") as Component==enemy&&Read(feedback,"CurrentRelation").ToString()=="Enemy","Enemy hover has a red cursor and the same target as picking");
  var red=(Color)Read(feedback,"CursorOutlineColor");Need(red.r>.8f&&red.g<.3f,"Enemy cursor outline is red");
  System.IO.File.WriteAllBytes(System.IO.Path.Combine(qa,"cursor-enemy.png"),((Texture2D)Read(feedback,"CursorTexture")).EncodeToPNG());
  Need(Call(combat,"Pick",enemyPoint) as Component==enemy&&Call(skill,"PickActor",enemyPoint) as Component==enemy,"Attack selection and unit spells agree on the hovered enemy");
  Mouse(enemyPoint,true);await System.Threading.Tasks.Task.Delay(180);Mouse(enemyPoint);await System.Threading.Tasks.Task.Delay(120);
  Need(Read(selection,"Selected") as Component==enemy,"Real left mouse input selects the highlighted enemy");
  enemyPoint=PointFor(enemy);
  Need((bool)Call(combat,"ContextAttack",enemyPoint)&&Read(Part(player,"DiceFree.Combat.BasicAttack"),"Target") as Component==enemy,"Context attack orders the highlighted enemy");Call(Part(player,"DiceFree.Combat.BasicAttack"),"Cancel");Call(Read(player,"Motor"),"Stop");
  await System.Threading.Tasks.Task.Delay(350);enemyPoint=PointFor(enemy);await Hover(enemyPoint);
  await Capture("enemy-hover",enemyPoint);
  var overlays=enemy.GetComponentsInChildren<Renderer>().Where(r=>r.gameObject.layer==30).ToArray();
  Need(overlays.Any(r=>r.name=="Hover silhouette")&&overlays.Any(r=>r is LineRenderer),"Enemy displays silhouette and physical footprint ring");
  Need(overlays.All(r=>r.GetComponent<Collider>()==null),"Visual hitbox feedback adds no physical colliders");
  Need(overlays.Where(r=>r.name=="Hover silhouette").All(r=>r.sharedMaterials.All(m=>m.shader.name=="DiceFree/World Hover Outline"&&m.shader.isSupported)),"Outline shader is present and supported in the Windows player");
  Phase("Animated friendly model and geometry outside its capsule");
  var heroPoint=PointFor(player,true);await Hover(heroPoint);
  Need(Read(feedback,"CurrentTarget") as Component==player&&Read(feedback,"CurrentRelation").ToString()=="Friendly","Visible animated hero geometry outside the body collider is clickable and green");
  Need(Call(combat,"Pick",heroPoint) as Component==player&&Call(skill,"PickActor",heroPoint) as Component==player,"Selection and friendly spells use the same visible-model geometry");
  var green=(Color)Read(feedback,"CursorOutlineColor");Need(green.g>.8f&&green.r<.3f,"Friendly cursor outline is green");
  System.IO.File.WriteAllBytes(System.IO.Path.Combine(qa,"cursor-friendly.png"),((Texture2D)Read(feedback,"CursorTexture")).EncodeToPNG());
  var skins=player.GetComponentsInChildren<SkinnedMeshRenderer>().Where(r=>r.name=="Hover silhouette").ToArray();
  Need(skins.Length>0&&skins.All(r=>r.sharedMesh!=null&&r.bones.Length>0&&r.transform.parent.GetComponent<SkinnedMeshRenderer>().sharedMesh==r.sharedMesh),"Animated outlines share the original model mesh and bones");
  await Capture("friendly-hover",heroPoint);
  var parameter=System.Linq.Expressions.Expression.Parameter(T("DiceFree.Combat.CombatActor"));
  var confirm=System.Linq.Expressions.Expression.Lambda(typeof(Func<,>).MakeGenericType(T("DiceFree.Combat.CombatActor"),typeof(bool)),System.Linq.Expressions.Expression.Constant(true),parameter).Compile();
  Call(skill,"BeginFriendly","QA friendly targeting",confirm,20f);Call(skill,"UpdateHover",heroPoint);Call(feedback,"RefreshPointer",heroPoint);
  Need((bool)Read(skill,"Active")&&!Cursor.visible&&Read(skill,"HoveredActor") as Component==player&&Read(feedback,"CurrentRelation").ToString()=="Friendly","Friendly skill reticle retains green relation feedback and owns cursor visibility");
  Call(skill,"Cancel");Call(feedback,"RefreshPointer",heroPoint);Need(Cursor.visible,"Leaving skill targeting restores the normal coloured cursor");
  Phase("Occlusion, concealment, and HUD protection");
  var blocker=GameObject.CreatePrimitive(PrimitiveType.Cube);fixtures.Add(blocker);blocker.name="QA pointer occluder";blocker.layer=9;
  var ray=Camera.main.ScreenPointToRay(heroPoint);var center=BoundsOf(player).center;float depth=Vector3.Distance(ray.origin,center);
  blocker.transform.position=ray.GetPoint(depth-2);blocker.transform.localScale=Vector3.one*1.5f;Physics.SyncTransforms();await System.Threading.Tasks.Task.Delay(60);
  Need(Read(Pick(heroPoint),"Actor") as Component!=player&&Call(skill,"PickActor",heroPoint) as Component!=player,"Solid walls block both pointer selection and unit spell picking");
  UnityEngine.Object.Destroy(blocker);await System.Threading.Tasks.Task.Delay(80);
  Call(enemy,"Conceal",0f);await System.Threading.Tasks.Task.Delay(50);Need(Read(Pick(enemyPoint),"Actor") as Component!=enemy,"Concealed enemies cannot be discovered by hover");Call(enemy,"Reveal");
  await Hover(new Vector2(100,100));Need(Read(feedback,"CurrentTarget")==null&&Read(feedback,"CurrentRelation").ToString()=="Neutral","Hovering the HUD clears world highlighting and coloured cursor");
  System.IO.File.WriteAllBytes(System.IO.Path.Combine(qa,"cursor-neutral.png"),((Texture2D)Read(feedback,"CursorTexture")).EncodeToPNG());
  Phase("Friendly NPC and persistent selection");
  var registry=Part(player,"DiceFree.World.InteractionRegistry");var targets=((System.Collections.IEnumerable)Read(registry,"Targets")).Cast<Component>().ToArray();
  var npc=targets.Where(n=>n.gameObject.scene.name=="Cornberg"&&n.GetComponentInChildren<Animator>()!=null&&n.transform.position.x<80).OrderBy(n=>n.transform.position.sqrMagnitude).First();
  Warp((Vector3)Read(npc,"ApproachPosition")+new Vector3(-1.2f,0,-1.2f));await System.Threading.Tasks.Task.Delay(350);
  var npcPoint=PointFor(npc);await Hover(npcPoint);
  Need(Read(feedback,"CurrentTarget") as Component==npc&&Read(feedback,"CurrentRelation").ToString()=="Friendly","Friendly NPCs receive green hover feedback");
  Need((bool)Call(Part(player,"DiceFree.World.Interactor"),"ContextInteract",Camera.main.ScreenPointToRay(npcPoint)),"Right-click interaction accepts the highlighted NPC");Call(Part(player,"DiceFree.World.Interactor"),"Cancel");
  Mouse(npcPoint,true);await System.Threading.Tasks.Task.Delay(150);Mouse(npcPoint);await System.Threading.Tasks.Task.Delay(100);
  Need(Read(Part(player,"DiceFree.UI.InspectSelection"),"Npc") as Component==npc,"Real left mouse input inspects the highlighted NPC");
  await Capture("npc-hover",npcPoint);
  await Hover(new Vector2(Screen.width*.5f,Screen.height-40));
  Need(npc.GetComponentsInChildren<LineRenderer>().Any(r=>r.gameObject.layer==30&&r.enabled),"Selected NPC retains its footprint when the pointer moves away");
  var mapCamera=All("DiceFree.UI.MinimapHud").SelectMany(c=>c.GetComponentsInChildren<Camera>()).FirstOrDefault();
  if(mapCamera==null)mapCamera=UnityEngine.Object.FindObjectsByType<Camera>().First(c=>c.name.Contains("Minimap")||c.name.Contains("minimap"));
  Need((mapCamera.cullingMask&(1<<30))==0,"Hover outlines and footprint rings stay out of the minimap");
  Phase("Usable objects and collision-only dungeon entrances");
  Need(targets.Where(n=>n.GetType().Name is "ItemShop" or "BankInteraction").All(n=>Call(feedback,"Relation",n).ToString()=="Friendly"),"Merchants and bankers, including unrigged village models, are friendly green targets");
  var usable=targets.Single(n=>n.name=="Cornberg prepared remains");
  Warp((Vector3)Read(usable,"ApproachPosition")+new Vector3(-3,0,-3));await System.Threading.Tasks.Task.Delay(350);
  var usablePoint=PointFor(usable);await Hover(usablePoint);Need(Read(feedback,"CurrentRelation").ToString()=="Interactable","Usable objects receive gold hover feedback");
  System.IO.File.WriteAllBytes(System.IO.Path.Combine(qa,"cursor-interactable.png"),((Texture2D)Read(feedback,"CursorTexture")).EncodeToPNG());await Capture("usable-hover",usablePoint);
  foreach(var portal in targets.Where(n=>n!=null&&new[]{"ShamanDungeonPortal","AncientRoadDragonLairPortal"}.Contains(n.GetType().Name)))
  {
   Warp((Vector3)Read(portal,"ApproachPosition")+new Vector3(-4,0,-4));await System.Threading.Tasks.Task.Delay(350);
   var portalCenter=portal.GetComponentInChildren<Collider>().bounds.center;
   bool picked=false;
   for(int i=0;i<8&&!picked;i++)
   {
    float angle=i*Mathf.PI*.25f;var origin=portalCenter+new Vector3(Mathf.Cos(angle)*2,.2f,Mathf.Sin(angle)*2);
    picked=Read(Static("DiceFree.World.WorldTargetPicker","Pick",player,new Ray(origin,(portalCenter-origin).normalized)),"Target") as Component==portal;
   }
   Need(picked,"Collision-only entrance stays clickable: "+portal.name);
  }
  Phase("World Two hover feedback");
  var journal=Part(player,"DiceFree.Quests.QuestJournal");var definitions=((System.Collections.IEnumerable)Read(journal,"Definitions")).Cast<object>();
  var definition=definitions.Single(x=>(string)Read(x,"stableId")=="quest.arborium.ancient-road-dragon");var stages=(Array)Read(definition,"stages");var records=(Array)Call(journal,"CaptureState");
  var record=records.Cast<object>().Single(x=>(string)Read(x,"questId")=="quest.arborium.ancient-road-dragon");Set(record,"stage",stages.Length-1);Set(record,"count",Read(stages.GetValue(stages.Length-1),"count"));Set(record,"status",Enum.Parse(T("DiceFree.Quests.QuestStatus"),"Completed"));Call(journal,"RestoreState",records);
  var pass=All("DiceFree.ApplicationFlow.IronpineSouthPass").Single(c=>c.gameObject.scene.name=="Cornberg");Warp((Vector3)Read(pass,"ApproachPosition"));await Until(()=>(bool)Read(pass,"IsOpen")&&!(bool)Read(player,"InCombat"),"Southern pass ready");Call(pass,"Interact",player);
  await Until(()=>player.transform.position.x>63000&&!Busy(),"World Two arrival",90);Quiet();
  var ashscale=All("DiceFree.Combat.CombatActor").Where(a=>a.gameObject.scene.name=="WorldTwoArrival"&&(string)Read(a,"FamilyId")=="ashscale.lizardman").OrderBy(a=>Vector3.Distance(player.transform.position,a.transform.position)).First();
  Warp(ashscale.transform.position+new Vector3(-4,0,-4));await System.Threading.Tasks.Task.Delay(350);var ashPoint=PointFor(ashscale);await Hover(ashPoint);
  Need(Read(feedback,"CurrentTarget") as Component==ashscale&&Read(feedback,"CurrentRelation").ToString()=="Enemy","Shared pointer feedback works after travelling to World Two");await Capture("world-two-enemy-hover",ashPoint);
  Phase("Cursor lifecycle");
  Call(feedback,"OnDisable");Need(Read(feedback,"CurrentTarget")==null&&Cursor.visible,"Disabling hover feedback restores the normal cursor");
  await System.Threading.Tasks.Task.Delay(100);Need(errors.Count==0,"No Unity runtime errors during hover and input checks");Report("passed");
 }
 catch(Exception error){failure=error.ToString();Report("failed");}
 finally{foreach(var fixture in fixtures)if(fixture!=null)UnityEngine.Object.Destroy(fixture);Application.logMessageReceived-=Log;}
}
Application.runInBackground=true;Application.logMessageReceived+=Log;_ = Review();return "Pointer geometry, coloured cursors, input, occlusion, and lifecycle review scheduled.";
