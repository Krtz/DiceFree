// Unity CLI eval_file in a development player, with a disposable copied save.
var qa=System.IO.Path.Combine(System.IO.Directory.GetParent(Application.dataPath).FullName,"QA");
System.IO.Directory.CreateDirectory(qa);
var checks=new System.Collections.Generic.List<string>();var errors=new System.Collections.Generic.List<string>();
Component hero=null;string phase="startup",failure=null;
Type T(string n)=>AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType(n)).First(t=>t!=null);
Component[] All(string n)=>UnityEngine.Object.FindObjectsByType(T(n)).Cast<Component>().ToArray();
Component Part(Component c,string n)=>c.GetComponent(T(n));
object Read(object c,string n)=>c?.GetType().GetProperty(n,System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic)?.GetValue(c)??c?.GetType().GetField(n,System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic)?.GetValue(c);
void Set(object c,string n,object v){var p=c.GetType().GetProperty(n,System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic);if(p?.CanWrite==true)p.SetValue(c,v);else c.GetType().GetField(n,System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic).SetValue(c,v);}
object Call(object c,string n,params object[] a){var m=c.GetType().GetMethods(System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic).Where(m=>m.Name==n).First(m=>{var p=m.GetParameters();return p.Length>=a.Length&&p.Take(a.Length).Select((v,i)=>a[i]==null||v.ParameterType.IsInstanceOfType(a[i])).All(v=>v)&&p.Skip(a.Length).All(v=>v.IsOptional);});return m.Invoke(c,m.GetParameters().Select((v,i)=>i<a.Length?a[i]:v.DefaultValue).ToArray());}
object E(string type,string name)=>Enum.Parse(T(type),name);
string Q(string s)=>s==null?"null":"\""+s.Replace("\\","\\\\").Replace("\"","\\\"").Replace("\r"," ").Replace("\n"," ")+"\"";
void Report(string state)=>System.IO.File.WriteAllText(System.IO.Path.Combine(qa,"aura-review.json"),"{\"status\":"+Q(state)+",\"phase\":"+Q(phase)+",\"checks\":"+checks.Count+",\"passed\":["+string.Join(",",checks.Select(Q))+"],\"error\":"+Q(failure)+",\"unityErrors\":["+string.Join(",",errors.Select(Q))+"]}");
void Need(bool v,string label){if(!v)throw new Exception(label);checks.Add(label);Report("running");}
void Phase(string value){phase=value;Report("running");}
bool Busy()=>(bool)T("DiceFree.ApplicationFlow.LoadingScreen").GetProperty("Busy").GetValue(null);
async System.Threading.Tasks.Task Until(Func<bool> check,string label,int seconds=70){var end=DateTime.UtcNow.AddSeconds(seconds);while(!check()){if(DateTime.UtcNow>end)throw new TimeoutException(label);await System.Threading.Tasks.Task.Delay(50);}}
Component[] Visible()=>All("DiceFree.Combat.GroundAuraVisual").Where(v=>(bool)Read(v,"Visible")).ToArray();
Component Style(string style)=>Visible().Single(v=>Read(v,"Style").ToString()==style);
Component View()=>All("DiceFree.Core.ExplorationCamera").Single();
void Quiet()
{
 foreach(var ai in All("DiceFree.AI.AggroBehaviour"))((Behaviour)ai).enabled=false;
 foreach(var attack in All("DiceFree.Combat.BasicAttack"))Call(attack,"Cancel");
 Set(Read(hero,"Health"),"Invulnerable",true);
 var feedback=Part(hero,"DiceFree.UI.WorldHoverFeedback");if(feedback!=null)((Behaviour)feedback).enabled=false;
 var input=Part(hero,"DiceFree.Characters.CombatInput");if(input!=null)((Behaviour)input).enabled=false;
}
Vector3 Nav(Vector3 point){if(Physics.Raycast(point+Vector3.up*80,Vector3.down,out var ground,180,1<<8))point=ground.point;if(!UnityEngine.AI.NavMesh.SamplePosition(point,out var hit,3,1))throw new Exception("No walkable ground");return hit.position;}
void Warp(Vector3 point){Need((bool)Call(Read(hero,"Motor"),"TeleportAcrossMaps",Nav(point)),"Place hero on walkable aura review ground");Call(View(),"RecenterForTravel",hero.transform);}
void ClearClasses()
{
 foreach(var caster in hero.GetComponents<MonoBehaviour>().Where(c=>c.GetType().Name.EndsWith("SkillCaster")||c.GetType().Name=="PsionEvolutionCaster"))
  if(caster.GetType().GetMethod("ResetForManifestationLoad")!=null)Call(caster,"ResetForManifestationLoad");
 foreach(var progression in hero.GetComponents<MonoBehaviour>().Where(c=>T("DiceFree.Skills.IClassSkillState").IsInstanceOfType(c)))Call(progression,"SetClassActive",false);
}
Component Activate(string progression,string classId)
{
 ClearClasses();var value=Part(hero,"DiceFree.Skills."+progression);Call(value,"SetClassActive",true);
 Call(Part(hero,"DiceFree.Combat.ActorResourceController"),"RestoreForClass",classId,null);return value;
}
async System.Threading.Tasks.Task Capture(string name,float distance=22)
{
 Set(View(),"ZoomDistance",distance);Call(View(),"RecenterForTravel",hero.transform);await System.Threading.Tasks.Task.Delay(400);
 ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(qa,name+".png"));await System.Threading.Tasks.Task.Delay(150);
}
void CheckGeometry(Component aura,string label)
{
 var renderer=aura.GetComponent<MeshRenderer>();var material=renderer.sharedMaterial;
 Need(renderer.gameObject.layer==30&&material.shader.name=="DiceFree/Ground Aura Sigil"&&material.shader.isSupported,label+" uses its shipped aura shader on the presentation layer");
 Need(aura.GetComponentsInChildren<Collider>().Length==0,label+" has no physical hitbox or navigation blocker");
 Need(aura.GetComponent<MeshFilter>().sharedMesh.vertexCount==81,label+" uses a ground-conforming patch");
 Need(Mathf.Abs(aura.transform.lossyScale.x-1)<.01f&&Mathf.Abs(aura.transform.lossyScale.z-1)<.01f,label+" keeps its physical visual size independent of hero scale");
}
void Log(string message,string stack,LogType type){if(type is LogType.Error or LogType.Exception or LogType.Assert)errors.Add(message);}
async System.Threading.Tasks.Task Review()
{
 try
 {
  if(All("DiceFree.Characters.TraversalInput").Length==0)
  {
   Component menu=null;await Until(()=>(menu=All("DiceFree.Advancement.StartMenuController").FirstOrDefault())!=null&&!Busy(),"Menu ready");Need((bool)Call(menu,"TryPlayManifestation","class.psionic-knight"),"Load copied Knight save");
  }
  else Need(true,"Continue isolated copied Knight save");
  await Until(()=>(hero=All("DiceFree.Characters.TraversalInput").FirstOrDefault())!=null&&!Busy()&&(bool)Read(Part(hero,"DiceFree.Persistence.ManifestationPersistence"),"Ready"),"Hero loaded");hero=Part(hero,"DiceFree.Combat.CombatActor");Quiet();
  Call(All("DiceFree.World.WorldDayNightCycle").Single(),"SetHour",15f,false);Warp(new Vector3(-14,0,-18));
  var resources=Part(hero,"DiceFree.Combat.ActorResourceController");
  Phase("Mana Attunement");var magical=Activate("MagicalSkillProgression","class.magically-touched-novice");await System.Threading.Tasks.Task.Delay(200);
  Need(!Visible().Any(),"Unlearned auras stay hidden");Need((bool)Call(magical,"Spend","skill.magical.mana-attunement"),"Learn Mana Attunement through its real progression");await Until(()=>Visible().Any(v=>Read(v,"Style").ToString()=="Mana"),"Mana visible");
  CheckGeometry(Style("Mana"),"Mana Attunement");var source=Part(hero,"DiceFree.Combat.ResourceRegenerationAura");float bonus=(float)Read(source,"BonusPerSecond"),range=(float)Read(source,"Radius");
  await Capture("mana-attunement");await Capture("mana-attunement-gameplay",40);
  var aura=Style("Mana");float before=aura.GetComponent<Renderer>().sharedMaterial.GetFloat("_Phase");await System.Threading.Tasks.Task.Delay(250);
  Need(aura.GetComponent<Renderer>().sharedMaterial.GetFloat("_Phase")>before+.05f,"Aura animation advances in the running player");
  ((Behaviour)aura).enabled=false;await System.Threading.Tasks.Task.Delay(60);Need(!(bool)Read(aura,"Visible")&&(float)Read(source,"BonusPerSecond")==bonus&&(float)Read(source,"Radius")==range,"Hiding presentation preserves aura regeneration and range");((Behaviour)aura).enabled=true;
  ((Behaviour)source).enabled=false;await System.Threading.Tasks.Task.Delay(70);Need(!Visible().Any(),"Disabling an aura source hides its ground effect");((Behaviour)source).enabled=true;await System.Threading.Tasks.Task.Delay(300);
  Call(hero,"Conceal",0f);await System.Threading.Tasks.Task.Delay(70);Need(!Visible().Any(),"Concealment hides ground auras");Call(hero,"Reveal");
  Phase("Elemental Weaving");var mage=Activate("MageSkillProgression","class.mage");Need((bool)Call(mage,"Spend",E("DiceFree.Progression.MageProgressionRules+Kind","ElementalWeaving")),"Learn Elemental Weaving");await Until(()=>Visible().Any(v=>Read(v,"Style").ToString()=="Elements"),"Elements visible");
  Need(Visible().Length==1,"Changing class removes the previous aura's visible sigil");CheckGeometry(Style("Elements"),"Elemental Weaving");await Capture("elemental-weaving");
  Phase("Hold the Line");var defender=Activate("DefenderSkillProgression","class.defender");Need((bool)Call(defender,"Spend",E("DiceFree.Skills.DefenderSkill","HoldTheLine")),"Learn Hold the Line");await Until(()=>Visible().Any(v=>Read(v,"Style").ToString()=="Defender"),"Defender visible");CheckGeometry(Style("Defender"),"Hold the Line");await Capture("hold-the-line");
  Phase("Speed Presence");var knight=Activate("PsionicKnightProgression","class.psionic-knight");Need((bool)Call(knight,"Spend",4),"Learn Knight Speed Presence");await Until(()=>Visible().Any(v=>Read(v,"Style").ToString()=="Speed"),"Speed visible");CheckGeometry(Style("Speed"),"Speed Presence");await Capture("speed-presence");
  var old=hero.transform.position;Need((bool)Call(Read(hero,"Motor"),"MoveTo",Nav(old+new Vector3(3,0,0))),"Issue a real walking order with an active aura");await Until(()=>Vector3.Distance(hero.transform.position,old)>1,"Hero moves with aura");Call(Read(hero,"Motor"),"Stop");await System.Threading.Tasks.Task.Delay(250);
  Need(Vector3.Distance(Style("Speed").transform.position,hero.transform.position)<.01f,"Aura follows the moving hero without lagging behind");
  var patch=Style("Speed").GetComponent<MeshFilter>().sharedMesh;var center=Style("Speed").transform.TransformPoint(patch.vertices[40]);
  Need(Physics.Raycast(center+Vector3.up,Vector3.down,out var surface,2,1<<8)&&Mathf.Abs(center.y-surface.point.y-.045f)<.04f,"Aura conforms to the actual walkable ground");
  var mirage=Activate("MirageMageProgression","class.mirage-mage");Need((bool)Call(mirage,"Spend",4),"Learn Mirage Mage Speed Presence");await Until(()=>Visible().Any(v=>Read(v,"Style").ToString()=="Speed"),"Mirage Speed visible");Need(Visible().Length==1,"Both tier-three psion classes use the same single Speed Presence sigil");
  Phase("Empowered Ice Aegis");var priest=Activate("PriestSkillProgression","class.priest");Need((bool)Call(priest,"Spend",E("DiceFree.Skills.PriestSkill","IceAegis"))&&(bool)Call(priest,"Spend",E("DiceFree.Skills.PriestSkill","Grace")),"Learn Priest shield and Grace");
  var priestCaster=Part(hero,"DiceFree.Skills.PriestSkillCaster");Call(resources,"Restore","resource.mana",100000f);
  Need((bool)Call(priestCaster,"Cast",E("DiceFree.Skills.PriestSkill","IceAegis"),hero),"Cast normal Ice Aegis");await System.Threading.Tasks.Task.Delay(120);Need(!Visible().Any(v=>Read(v,"Style").ToString()=="Frost"),"Normal shields do not imply the empowered damage aura");
  Call(priestCaster,"ResetForManifestationLoad");Call(resources,"Restore","resource.grace",10000f);Need((bool)Call(priestCaster,"ArmGrace"),"Arm Grace");Need((bool)Call(priestCaster,"Cast",E("DiceFree.Skills.PriestSkill","IceAegis"),hero),"Cast real empowered Ice Aegis");await Until(()=>Visible().Any(v=>Read(v,"Style").ToString()=="Frost"),"Frost visible");CheckGeometry(Style("Frost"),"Empowered Ice Aegis");await Capture("empowered-ice-aegis");
  Call(Read(hero,"Health"),"RemoveTemporaryShield",(string)Read(priestCaster,"shieldId"));await System.Threading.Tasks.Task.Delay(120);Need(!Visible().Any(v=>Read(v,"Style").ToString()=="Frost"),"Breaking the Priest shield removes its aura sigil");
  Phase("Radiation");ClearClasses();var radiation=T("DiceFree.Combat.MageSpellVfx").GetMethod("Radiation").Invoke(null,new object[]{hero,3f});await Until(()=>Visible().Any(v=>Read(v,"Style").ToString()=="Radiation"),"Radiation visible");CheckGeometry(Style("Radiation"),"Radiation");await Capture("radiation");await Until(()=>!Visible().Any(v=>Read(v,"Style").ToString()=="Radiation"),"Radiation expires",10);Need(!Visible().Any(),"Timed radiation sigil expires with its parent effect");
  Phase("Night and death lifecycle");knight=Activate("PsionicKnightProgression","class.psionic-knight");Call(knight,"Spend",4);await Until(()=>Visible().Any(),"Night aura visible");Call(All("DiceFree.World.WorldDayNightCycle").Single(),"SetHour",22f,false);await Capture("speed-presence-night",40);
  var hp=Read(hero,"Health");Set(hp,"Invulnerable",false);var packet=Activator.CreateInstance(T("DiceFree.Combat.DamageResult"));Set(packet,"raw",10000000f);Set(packet,"mitigated",10000000f);Call(hp,"ApplyPacket",null,packet);await System.Threading.Tasks.Task.Delay(100);Need(!(bool)Read(hero,"Alive")&&!Visible().Any(),"Real hero death immediately hides aura sigils");Call(hp,"Restore",1f);await System.Threading.Tasks.Task.Delay(300);Need(Visible().Any(v=>Read(v,"Style").ToString()=="Speed"),"A living restored aura owner regains its sigil");
  Phase("World Two travel");Set(hp,"Invulnerable",true);
  var journal=Part(hero,"DiceFree.Quests.QuestJournal");var definition=((System.Collections.IEnumerable)Read(journal,"Definitions")).Cast<object>().Single(d=>(string)Read(d,"stableId")=="quest.arborium.ancient-road-dragon");
  var stages=(Array)Read(definition,"stages");var records=(Array)Call(journal,"CaptureState");var record=records.Cast<object>().Single(r=>(string)Read(r,"questId")=="quest.arborium.ancient-road-dragon");
  Set(record,"stage",stages.Length-1);Set(record,"count",Read(stages.GetValue(stages.Length-1),"count"));Set(record,"status",E("DiceFree.Quests.QuestStatus","Completed"));Call(journal,"RestoreState",records);
  var pass=All("DiceFree.ApplicationFlow.IronpineSouthPass").Single(p=>p.gameObject.scene.name=="Cornberg");Warp((Vector3)Read(pass,"ApproachPosition"));await Until(()=>(bool)Read(pass,"IsOpen")&&!(bool)Read(hero,"InCombat"),"Pass ready");Call(pass,"Interact",hero);
  await Until(()=>hero.transform.position.x>63000&&!Busy(),"World Two arrival",90);Quiet();await System.Threading.Tasks.Task.Delay(500);
  Need(Visible().Any(v=>Read(v,"Style").ToString()=="Speed"),"Aura survives the real World Two scene crossing");
  var worldAura=Style("Speed");var worldCenter=worldAura.transform.TransformPoint(worldAura.GetComponent<MeshFilter>().sharedMesh.vertices[40]);
  Need(Physics.Raycast(worldCenter+Vector3.up,Vector3.down,out var worldGround,2,1<<8)&&Mathf.Abs(worldCenter.y-worldGround.point.y-.045f)<.05f,"Aura remains grounded at World Two's distant world coordinates");
  Call(All("DiceFree.World.WorldDayNightCycle").Single(),"SetHour",15f,false);await Capture("speed-presence-world-two",40);
  Need(errors.Count==0,"No Unity errors during aura review");Report("passed");
 }
 catch(Exception error){failure=error.ToString();Report("failed");}
 finally{Time.timeScale=1;Application.logMessageReceived-=Log;}
}
Report("scheduled");Application.runInBackground=true;Application.logMessageReceived+=Log;_ = Review();return "Aura eligibility, six styles, source lifecycle, terrain, and real ability checks scheduled.";
