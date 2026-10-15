using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using DiceFree.Advancement;
using DiceFree.Combat;
using DiceFree.Items;
using DiceFree.Persistence;
using DiceFree.Ironpine;
using DiceFree.UI;
using DiceFree.Visuals;
using DiceFree.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Pipeline.Commands;
using Object=UnityEngine.Object;
namespace DiceFree.EditorTools
{
 [InitializeOnLoad] public static class MenuWolfValidation
 {
  const string Key="DiceFree.MenuWolf.";
  static readonly Stack<IEnumerator> steps=new();
  static int checks;static double deadline;
  [Serializable] sealed class Report { public string status,error;public int checks,portraits,credits,drops; }
  sealed class Roll:IRandomSource {public double value;public double NextUnit()=>value;}
  static Report report;
  static MenuWolfValidation()=>EditorApplication.playModeStateChanged+=Changed;
  static void Need(bool ok,string text){if(!ok)throw new InvalidOperationException(text);checks++;}
  [CliCommand("dicefree.menu-wolf.test","Verify read-only equipped tier cards, credits, two-hand gear and real Alpha Wolf death/pickup/save flow.")]
  public static object Start(){
   if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play Mode.");
   SessionState.SetString(Key+"scene",EditorSceneManager.GetActiveScene().path);
   SessionState.SetBool(Key+"options",EditorSettings.enterPlayModeOptionsEnabled);
   SessionState.SetInt(Key+"flags",(int)EditorSettings.enterPlayModeOptions);
   SessionState.SetString(Key+"status","running");SessionState.SetString(Key+"error","");SessionState.SetInt(Key+"checks",0);SessionState.SetString(Key+"phase","initializing");
   SessionState.SetBool(Key+"cleanup",true);SessionState.SetBool(Key+"resume",false);SessionState.SetBool(Key+"automatic",false);
   string root=Path.Combine(Path.GetTempPath(),"DiceFree-MenuWolf-"+Guid.NewGuid().ToString("N"));
   var catalog=Resources.Load<MenuPresentationCatalog>("MenuPresentationCatalog");
   var profile=new EchoSave{revision=1,writtenUtc=DateTime.UtcNow.ToString("O")};
   foreach(var form in catalog.forms){
    var items=catalog.items.Where(i=>i.modelTestingItem).Select(i=>new ItemInstance{instanceId=Guid.NewGuid().ToString("D"),definitionId=i.stableId}).ToList();
    items.Add(new ItemInstance{instanceId=Guid.NewGuid().ToString("D"),definitionId=WolfGreatswordVisual.ItemId});
    var equipped=items.Where(i=>catalog.items.Single(d=>d.stableId==i.definitionId).modelTestingItem)
     .Select(i=>new EquippedItem{instanceId=i.instanceId,slotId=catalog.items.Single(d=>d.stableId==i.definitionId).slot.ToString()}).ToList();
    if(form.classId=="class.greatblade"){
     equipped.RemoveAll(s=>s.slotId=="MainHand"||s.slotId=="OffHand");
     equipped.Add(new EquippedItem{slotId="MainHand",instanceId=items.Last().instanceId});
    }
    var state=new ManifestationSave{classId=form.classId,level=30,inventory=items.ToArray(),equipment=equipped.ToArray(),gold=1200};
    profile.sections.Add(new SaveSection{id=ManifestationRoster.SectionIdFor(form.classId),version=SaveMigrations.ManifestationVersion,json=JsonUtility.ToJson(state)});
   }
   profile.sections.Add(new SaveSection{id=ManifestationRoster.SectionIdFor("class.future"),version=SaveMigrations.ManifestationVersion,json=JsonUtility.ToJson(new ManifestationSave{classId="class.future",level=1})});
   profile.sections.Add(new SaveSection{id="future:opaque",version=7,json="{\"preserved\":true}"});
   ManifestationRoster.Write(profile,new ManifestationRosterSave{activeClassId="class.greatblade"});
   EchoIdentity.WriteName(profile,"Krtz");
   new LocalEchoStore(root).Commit(profile);SessionState.SetString(Key+"root",root);
   PersistenceTestGuard.UseIsolatedSaveRootForNextPlay(root);
   EditorSettings.enterPlayModeOptionsEnabled=false;EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/StartMenu.unity");
   EditorApplication.EnterPlaymode();return Status();
  }
  [CliCommand("dicefree.menu-wolf.verify","Run the same checks without visual capture pauses.")]
  public static object Verify(){var result=Start();SessionState.SetBool(Key+"automatic",true);return result;}
  [CliCommand("dicefree.menu-wolf.status","Read menu and loot verification.")]
  public static object Status()=>new{status=SessionState.GetString(Key+"status","idle"),phase=SessionState.GetString(Key+"phase",""),checks=SessionState.GetInt(Key+"checks",0),error=SessionState.GetString(Key+"error","")};
  [CliCommand("dicefree.menu-wolf.resume","Continue after menu visual inspection.")]
  public static object Resume(){SessionState.SetBool(Key+"resume",true);return Status();}
  static void Changed(PlayModeStateChange s){
   if(s==PlayModeStateChange.EnteredPlayMode&&SessionState.GetString(Key+"status","")=="running"){
    checks=0;report=new Report();steps.Clear();steps.Push(Flow());deadline=EditorApplication.timeSinceStartup+300;
    EditorApplication.update+=Tick;Application.logMessageReceived+=Log;
   }
   if(s==PlayModeStateChange.EnteredEditMode&&SessionState.GetBool(Key+"cleanup",false)){
    SessionState.SetBool(Key+"cleanup",false);EditorApplication.update-=Tick;Application.logMessageReceived-=Log;
    PersistenceTestGuard.ClearValidationOverrides();EditorSettings.enterPlayModeOptions=(EnterPlayModeOptions)SessionState.GetInt(Key+"flags",0);
    EditorSettings.enterPlayModeOptionsEnabled=SessionState.GetBool(Key+"options",false);
    string path=SessionState.GetString(Key+"scene","");if(!string.IsNullOrEmpty(path))EditorSceneManager.OpenScene(path);
   }
  }
  static void Log(string m,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception)Finish(false,m+"\n"+stack);}
  static void Tick(){if(!EditorApplication.isPlaying)return;try{
   if(EditorApplication.timeSinceStartup>deadline)throw new TimeoutException("Menu/wolf test timed out.");
   while(steps.Count>0){var s=steps.Peek();if(!s.MoveNext()){steps.Pop();continue;}if(s.Current is IEnumerator e){steps.Push(e);continue;}return;}
   Finish(true,null);
  }catch(Exception e){Finish(false,e.ToString());}}
  static void Finish(bool pass,string error){
   EditorApplication.update-=Tick;Application.logMessageReceived-=Log;
   report??=new Report();report.status=pass?"passed":"failed";report.error=error;report.checks=checks;
   SessionState.SetString(Key+"status",report.status);SessionState.SetString(Key+"error",error??"");SessionState.SetInt(Key+"checks",checks);
   File.WriteAllText("Library/MenuWolfResults.json",JsonUtility.ToJson(report,true));EditorApplication.ExitPlaymode();
  }
  static IEnumerator Pause(string phase){
   if(SessionState.GetBool(Key+"automatic",false))yield break;
   SessionState.SetString(Key+"phase",phase);SessionState.SetInt(Key+"checks",checks);SessionState.SetBool(Key+"resume",false);
   while(!SessionState.GetBool(Key+"resume",false)){deadline=EditorApplication.timeSinceStartup+300;yield return null;}
  }
  static IEnumerator Frames(int n){for(int i=0;i<n;i++)yield return null;}
  static IEnumerator Flow(){
   StartMenuController menu=null;
   while(menu==null||!menu.HasLoadedProfile){menu=Object.FindAnyObjectByType<StartMenuController>();yield return null;}
   var invalidHost=new GameObject("Invalid preview test");
   var invalidStage=invalidHost.AddComponent<MenuPortraitStage>();
   Need(invalidStage.Get("class.novice",new ManifestationSave{inventory=new ItemInstance[]{null}},Resources.Load<MenuPresentationCatalog>("MenuPresentationCatalog"))==null&&invalidStage.Portraits.Count==0,"Malformed preview blocked the menu");
   Object.Destroy(invalidHost);yield return Frames(2);
   string root=SessionState.GetString(Key+"root",""),path=new LocalEchoStore(root).Path;
   string original=File.ReadAllText(path);var profile=new LocalEchoStore(root).Load();
   var catalog=Resources.Load<MenuPresentationCatalog>("MenuPresentationCatalog");
   Need(menu.SelectedTier==2&&menu.Entries.Length==5,"Default last-active tier / preserved entries");
   Need(menu.EntriesForTier(0).Length==1&&menu.EntriesForTier(1).Length==2&&menu.EntriesForTier(2).Length==1&&menu.EntriesForTier(5).Length==0,"Tier grouping");
   Need(menu.EntriesForTier(-1).Length==1&&!menu.EntriesForTier(-1)[0].available&&!menu.TryPlayManifestation("class.future"),"Unknown class was activated or lost");
   menu.Refresh();
   foreach(var f in catalog.forms)menu.PortraitStage.Get(f.classId,ManifestationRoster.ReadManifestation(profile,f.classId),catalog);
   yield return Frames(30);
   foreach(var p in menu.PortraitStage.Portraits){
    Need(p.rendered&&p.texture.IsCreated(),"Portrait did not render "+p.classId);
    Need(p.root.GetComponentsInChildren<CombatActor>(true).Length==0&&p.root.GetComponentsInChildren<ManifestationPersistence>(true).Length==0&&p.root.GetComponentsInChildren<Health>(true).Length==0,"Live gameplay in portrait");
    var expected=ManifestationRoster.ReadManifestation(profile,p.classId);
    var ids=expected.equipment.Select(s=>expected.inventory.Single(i=>i.instanceId==s.instanceId).definitionId).OrderBy(x=>x).ToArray();
    Need(ids.SequenceEqual(p.equippedIds.OrderBy(x=>x)),"Wrong equipped portrait gear "+p.classId);
    var prev=RenderTexture.active;RenderTexture.active=p.texture;var tex=new Texture2D(p.texture.width,p.texture.height,TextureFormat.RGBA32,false);
    tex.ReadPixels(new Rect(0,0,tex.width,tex.height),0,0);tex.Apply();
    int visible=tex.GetPixels32().Count(c=>c.a>10);
    Need(visible>1000,"Empty portrait "+p.classId);
    File.WriteAllBytes("Library/MenuPortrait-"+p.classId+".png",tex.EncodeToPNG());Object.Destroy(tex);RenderTexture.active=prev;
   }
   report.portraits=menu.PortraitStage.Portraits.Count;
   foreach(var p in menu.PortraitStage.Portraits) {
    var loadout=p.root.GetComponentInChildren<Equipment>();
    var inventory=loadout.GetComponent<CarriedInventory>();
    var testWolf=inventory.Items.Single(i=>i.definitionId==WolfGreatswordVisual.ItemId);
    var old=loadout.Slots;
    Need(loadout.Equip(testWolf.instanceId,EquipmentSlot.MainHand)&&loadout.IsEquipped(testWolf.instanceId),"All-class two-hand equip "+p.classId);
    loadout.Restore(old);
   }
   menu.SelectTier(1);yield return Frames(4);yield return Pause("tier1_ready");
   menu.SelectTier(2);yield return Frames(4);yield return Pause("greatblade_ready");
   menu.OpenOptions(OptionsPanel.OptionsTab.Credits);yield return Frames(4);
   var options=Object.FindAnyObjectByType<OptionsPanel>();
   Need(Object.FindObjectsByType<OptionsPanel>().Length==1&&!options.ShowLauncher,"Duplicate menu Options host");
   Need(options.Open&&options.SelectedTab==OptionsPanel.OptionsTab.Credits,"Credits navigation");
   Need(CreditsView.Book!=null&&CreditsView.Book.entries.Length==40&&CreditsView.Entries.Length>=35,"Missing in-use credits");
   Need(CreditsView.Book.notices.Single().text==File.ReadAllText("Assets/_DiceFree/Shaders/Compatibility/LICENSE.md"),"Shader notice missing");
   report.credits=CreditsView.Entries.Length;
   yield return Pause("credits_ready");options.Close();
   Need(File.ReadAllText(path)==original,"Opening tier cards/options changed save bytes");
   Need(menu.TryPlayManifestation("class.greatblade"),menu.Status);
   CombatActor player=null;
   while(player==null){player=Object.FindAnyObjectByType<DiceFree.Characters.TraversalInput>()?.GetComponent<CombatActor>();yield return null;}
   var persistence=player.GetComponent<ManifestationPersistence>();
   while(!persistence.Ready||!player.Motor.Ready)yield return null;
   player.Health.Invulnerable=true;
   var inv=player.GetComponent<CarriedInventory>();var eq=player.GetComponent<Equipment>();
   var wolf=inv.Items.Single(i=>i.definitionId==WolfGreatswordVisual.ItemId);
   var shield=inv.Items.Single(i=>inv.Resolve(i.definitionId).slot==EquipmentSlot.OffHand);
   var sword=inv.Items.Single(i=>inv.Resolve(i.definitionId).slot==EquipmentSlot.MainHand&&i.definitionId!=WolfGreatswordVisual.ItemId);
   Need(eq.IsEquipped(wolf.instanceId)&&!eq.IsEquipped(shield.instanceId),"Saved two-hand load");
   Need(eq.Unequip("MainHand"),"Unequip wolf");
   var before=player.Stats.Attributes;float basic=player.Stats.BasicAttackFlatBonus;
   Need(eq.Equip(shield.instanceId,EquipmentSlot.OffHand)&&eq.Equip(wolf.instanceId,EquipmentSlot.MainHand),"Two-hand equip");
   Need(!eq.IsEquipped(shield.instanceId)&&inv.Find(shield.instanceId)!=null&&!eq.Equip(shield.instanceId,EquipmentSlot.OffHand),"Two-hand conflict lost item or allowed shield");
   Need(Mathf.Approximately(player.Stats.Attributes.strength-before.strength,5)&&Mathf.Approximately(player.Stats.BasicAttackFlatBonus-basic,10),"Wolf exact stats");
   var conflict=eq.Slots.Concat(new[]{new EquippedItem{slotId="OffHand",instanceId=shield.instanceId}}).ToArray();
   eq.Restore(conflict);Need(!eq.IsEquipped(shield.instanceId),"Restore kept conflicting offhand");
   Need(eq.Equip(sword.instanceId,EquipmentSlot.MainHand)&&eq.Equip(shield.instanceId,EquipmentSlot.OffHand),"One-handed gear did not free offhand");
   Need(eq.Equip(wolf.instanceId,EquipmentSlot.MainHand),"Re-equip wolf");
   yield return Frames(6);
   var visual=player.GetComponent<ClassPresentationSwitcher>().ActiveVisualRoot;
   Need(visual.Find("Wolf-Fang Greatsword").gameObject.activeInHierarchy,"Wolf visual missing");
   var animals=Object.FindObjectsByType<IronpineWildlife>();
   Need(animals.Length==22,"Wildlife population");
   Need(animals.Where(a=>a.Kind!=WildlifeKind.AlphaWolf).All(a=>a.GetComponent<FixedWorldDrop>()==null),"Unrequested wildlife loot");
   var alpha=animals.Single(a=>a.Kind==WildlifeKind.AlphaWolf).Actor;var drop=alpha.GetComponent<FixedWorldDrop>();
   Need(drop!=null&&drop.item.stableId==WolfGreatswordVisual.ItemId&&Mathf.Approximately(drop.chance,.1f)&&drop.item.handsRequired==2&&drop.item.allowedClassIds.Length==0,"Alpha loot definition");
   var roll=new Roll();drop.SetRandom(roll);int drops=0;WorldEquipmentPickup pickup=null;
   drop.Dropped+=p=>{drops++;pickup=p;};
   roll.value=.099999;alpha.Health.Invulnerable=false;alpha.Health.Restore();
   alpha.Health.ApplyDamage(player,new DamageResult{mitigated=1000000});
   Need(drops==1&&drop.RollCount==1&&pickup.Item==drop.item,"Below-10-percent death roll failed");
   typeof(FixedWorldDrop).GetMethod("OnDefeat",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(drop,new object[]{new ActorDefeated(999,alpha,player,player)});
   Need(drops==1&&drop.RollCount==1,"Duplicate defeat rolled twice");
   roll.value=drop.chance;alpha.Health.Restore();alpha.Health.ApplyDamage(player,new DamageResult{mitigated=1000000});
   Need(drops==1&&drop.RollCount==2,"10-percent boundary incorrectly dropped");
   roll.value=.9;alpha.Health.Restore();alpha.Health.ApplyDamage(player,new DamageResult{mitigated=1000000});
   Need(drops==1&&drop.RollCount==3,"Failed roll dropped");
   roll.value=0;alpha.Health.Restore();alpha.Health.ApplyDamage(player,new DamageResult{mitigated=1000000});
   Need(drops==2&&drop.RollCount==4,"Respawn did not rearm drop");
   report.drops=drops;
   bool near=false;for(int i=0;i<12&&!near;i++){float a=i*Mathf.PI/6;near=player.Motor.Teleport(pickup.transform.position+new Vector3(Mathf.Cos(a),0,Mathf.Sin(a))*.9f)&&pickup.CanInteract(player);}
   Need(near,"Loot cannot be approached");int owned=inv.Items.Length;
   Need(pickup.TryPickup(player)&&inv.Items.Length==owned+1&&!pickup.TryPickup(player),"Pickup duplication");
   Need(persistence.Flush(),"Gear save failed");
   var saved=new LocalEchoStore(root).Load();var state=ManifestationRoster.ReadManifestation(saved,"class.greatblade");
   Need(state.equipment.Any(s=>s.instanceId==wolf.instanceId)&&state.inventory.Count(i=>i.definitionId==WolfGreatswordVisual.ItemId)==2,"Wolf ownership/equip not persisted");
   Need(saved.sections.Single(s=>s.id=="future:opaque").json=="{\"preserved\":true}"&&ManifestationRoster.HasManifestation(saved,"class.future"),"Opaque future state lost");
   Need(player.GetComponent<DiceFree.Dungeons.ReturnToStartMenu>().Return(),"Return to menu failed");
   menu=null;while(menu==null||!menu.HasLoadedProfile){menu=Object.FindAnyObjectByType<StartMenuController>();yield return null;}
   yield return Frames(8);Need(menu.SelectedTier==2&&menu.EntriesForTier(2).Single().active,"Return lost active tier");
   Need(Object.FindObjectsByType<CombatActor>().Length==0,"Gameplay actors survived menu return");
   SessionState.SetString(Key+"phase","complete");
  }
 }
}
