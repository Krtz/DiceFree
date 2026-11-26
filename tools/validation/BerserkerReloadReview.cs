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
void Report(string status)=>System.IO.File.WriteAllText(System.IO.Path.Combine(qa,"berserker-reload.json"),"{\"status\":"+Q(status)+",\"phase\":"+Q(phase)+",\"checks\":"+checks.Count+",\"passed\":["+string.Join(",",checks.Select(Q))+"],\"error\":"+Q(failure)+",\"unityErrors\":["+string.Join(",",errors.Select(Q))+"]}");
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
 try
 {
 Phase("Fresh process disk reload of completed Berserker");Component menu=null;await Until(()=>(menu=All("DiceFree.Advancement.StartMenuController").FirstOrDefault())!=null&&!Busy(),"Menu");Need((bool)Call(menu,"TryPlayManifestation","class.berserker"),"Load Berserker manifestation from disk");await Until(()=>(hero=All("DiceFree.Characters.TraversalInput").FirstOrDefault())!=null&&!Busy()&&(bool)Read(Part(hero,"DiceFree.Persistence.ManifestationPersistence"),"Ready"),"Reloaded hero ready");hero=Part(hero,"DiceFree.Combat.CombatActor");Quiet();journal=Part(hero,"DiceFree.Quests.QuestJournal");inventory=Part(hero,"DiceFree.Items.CarriedInventory");xp=Part(hero,"DiceFree.Progression.ExperienceProgression");var persistence=Part(hero,"DiceFree.Persistence.ManifestationPersistence");var controller=Part(hero,"DiceFree.Advancement.AdvancementController");var caster=Part(hero,"DiceFree.Skills.BerserkerCaster");var ranks=Part(hero,"DiceFree.Skills.BerserkerProgression");var resources=Part(hero,"DiceFree.Combat.ActorResourceController");var equipment=Part(hero,"DiceFree.Items.Equipment");
 Need((int)Read(xp,"Level")==150,"Berserker level 150 survives disk reload");Need(new[]{35,30,30,25,30}.SequenceEqual(Enumerable.Range(0,5).Select(i=>(int)Call(ranks,"Rank",i)))&&(int)Read(ranks,"UnspentPoints")==0,"Exact 150 allocated ranks reload without duplication");Need((float)Read(caster,"Rage")==0&&(float)Read(caster,"HasteRemaining")==0&&!(bool)Read(caster,"Busy"),"Transient Rage, haste and cast state reset on reload");Need((bool)Call(resources,"Has","resource.rage")&&!(bool)Call(resources,"Has","resource.momentum"),"Rage profile remains independent of parent Momentum");Need((bool)Read(equipment,"HasGreatWeapon"),"Saved two-handed greataxe resolves after reload");
 var axeItem=((System.Collections.IEnumerable)Read(inventory,"Items")).Cast<object>().First(i=>(string)Read(i,"definitionId")=="item.berserker.ashscar-greataxe");
 var sword=((System.Collections.IEnumerable)Read(inventory,"Definitions")).Cast<object>().First(d=>Read(d,"greatWeaponFamily").ToString()=="TwoHandedSword"&&(((string[])Read(d,"allowedClassIds")).Length==0||((string[])Read(d,"allowedClassIds")).Contains("class.berserker")));
 var swordItem=Call(inventory,"Grant",(string)Read(sword,"stableId"));Need((bool)Call(equipment,"Equip",(string)Read(swordItem,"instanceId"),E("DiceFree.Items.EquipmentSlot","MainHand")),"Berserker can equip a real existing two-handed sword");await System.Threading.Tasks.Task.Delay(300);var weapon=hero.GetComponentsInChildren(T("DiceFree.UI.BerserkerWeaponPresentation")).Cast<Component>().Single(c=>((Behaviour)c).enabled);Need(((GameObject)Read(weapon,"sword")).activeInHierarchy&&!((GameObject)Read(weapon,"axe")).activeInHierarchy,"Equipped sword switches the held weapon appearance");Need(Mathf.Abs((float)Read(caster,"DamageMultiplier")-1.125f)<.002f,"Sword receives same 12.5 percent mastery as axe");
 var prior=((System.Collections.IEnumerable)Read(inventory,"Definitions")).Cast<object>().ToArray();var itemType=T("DiceFree.Items.ItemDefinition");var temporary=new System.Collections.Generic.List<object>();
 object Fixture(string id,string slot,int hands,string family,float chance=0,float multiplier=0){var d=ScriptableObject.CreateInstance(itemType);Set(d,"stableId",id);Set(d,"displayName",id);Set(d,"slot",E("DiceFree.Items.EquipmentSlot",slot));Set(d,"handsRequired",hands);Set(d,"greatWeaponFamily",E("DiceFree.Items.GreatWeaponFamily",family));Set(d,"allowedClassIds",new[]{"class.berserker"});var stats=Activator.CreateInstance(T("DiceFree.Combat.EquipmentStats"));Set(stats,"criticalChancePercent",chance);Set(stats,"criticalDamageMultiplier",multiplier);Set(d,"stats",stats);temporary.Add(d);return d;}
 Fixture("item.qa.berserker.onehand","MainHand",1,"Unspecified");Fixture("item.qa.berserker.offhand","OffHand",1,"Unspecified");Fixture("item.qa.berserker.crit-axe","MainHand",2,"TwoHandedAxe",100,2.25f);var defs=Array.CreateInstance(itemType,prior.Length+temporary.Count);for(int i=0;i<prior.Length;i++)defs.SetValue(prior[i],i);for(int i=0;i<temporary.Count;i++)defs.SetValue(temporary[i],prior.Length+i);Call(inventory,"Configure",defs);
 var one=Call(inventory,"Grant","item.qa.berserker.onehand");Need(!(bool)Call(equipment,"Equip",(string)Read(one,"instanceId"),E("DiceFree.Items.EquipmentSlot","MainHand")),"Reject otherwise class-eligible one-handed main weapon");Call(equipment,"Unequip","MainHand");var off=Call(inventory,"Grant","item.qa.berserker.offhand");Need(!(bool)Call(equipment,"Equip",(string)Read(off,"instanceId"),E("DiceFree.Items.EquipmentSlot","OffHand")),"Reject offhand even with an empty main slot");
 var crit=Call(inventory,"Grant","item.qa.berserker.crit-axe");Need((bool)Call(equipment,"Equip",(string)Read(crit,"instanceId"),E("DiceFree.Items.EquipmentSlot","MainHand")),"Equip explicit external crit axe fixture");var rule=Call(Read(hero,"Stats"),"ExternalCriticalRule");Need((float)Read(rule,"Chance")==1&&(float)Read(rule,"RequestedMultiplier")==2.25f,"Equipment supplies its explicit crit chance and multiplier");Need((bool)Call(equipment,"Equip",(string)Read(axeItem,"instanceId"),E("DiceFree.Items.EquipmentSlot","MainHand")),"Restore genuine starter greataxe");Need(Call(Read(hero,"Stats"),"ExternalCriticalRule")==null,"Unequipping the crit item removes all external crit chance");foreach(var item in new[]{one,off,crit,swordItem})Call(inventory,"Remove",(string)Read(item,"instanceId"));var restore=Array.CreateInstance(itemType,prior.Length);for(int i=0;i<prior.Length;i++)restore.SetValue(prior[i],i);Call(inventory,"Configure",restore);foreach(var d in temporary)UnityEngine.Object.Destroy((UnityEngine.Object)d);
 Phase("Humanoid render, two-hand grip and preserved parent");await System.Threading.Tasks.Task.Delay(500);var animator=weapon.GetComponent<Animator>();Need(animator.avatar!=null&&animator.avatar.isValid&&animator.isHuman,"Active player actually uses the valid humanoid avatar");var left=animator.GetBoneTransform(HumanBodyBones.LeftHand);var right=animator.GetBoneTransform(HumanBodyBones.RightHand);var shaft=(Transform)Read(weapon,"weaponRoot");Need(Vector3.Distance(left.position,shaft.TransformPoint(Vector3.up*.32f))<.12f,"Left hand grips the two-handed shaft");Need(Vector3.Distance(right.position,shaft.position)<.06f,"Weapon follows the animated right hand");Need(animator.GetComponentsInChildren<SkinnedMeshRenderer>().All(s=>s.sharedMesh.boneWeights.Any(w=>w.weight0>0)&&s.bounds.size.magnitude<4),"All three body meshes have working weights and bounded deformation");await Capture("berserker-delivery",11);
 Need((bool)Call(controller,"TrySwitch","class.greatblade"),"Reload Greatblade parent from disk");journal=Part(hero,"DiceFree.Quests.QuestJournal");Need((int)Read(xp,"Level")==60&&Status("quest.berserker.trial-of-fury")=="Completed","Parent level and completed Bristlebreaker quest persist independently");Need((bool)Call(controller,"TrySwitch","class.berserker"),"Return to saved Berserker");Need((bool)Call(persistence,"Flush"),"Save cleaned verification profile");Need((bool)Call(Part(hero,"DiceFree.Dungeons.ReturnToStartMenu"),"Return"),"Reload review returns normally to menu");await Until(()=>All("DiceFree.Advancement.StartMenuController").Length>0&&!Busy(),"Final menu");Need(errors.Count==0,"No runtime errors through reload, equipment and render review");Report("passed");
 }
 catch(Exception e){failure=e.ToString();Report("failed");}
 finally{Application.logMessageReceived-=Log;}
}
void Log(string message,string stack,LogType type){if(type is LogType.Error or LogType.Exception or LogType.Assert)errors.Add(message);}
Report("scheduled");Application.runInBackground=true;Application.logMessageReceived+=Log;_ = ReviewReload();return "Berserker disk reload and held-weapon review scheduled";