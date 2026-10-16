using System;using System.Collections;using System.IO;using System.Linq;using System.Collections.Generic;
using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Unity.Pipeline.Commands;
using DiceFree.Advancement;using DiceFree.Characters;using DiceFree.Combat;using DiceFree.Foundation;using DiceFree.Input;using DiceFree.Items;using DiceFree.Persistence;using DiceFree.Progression;using DiceFree.Skills;using DiceFree.UI;using DiceFree.Visuals;using DiceFree.World;
using Object=UnityEngine.Object;
namespace DiceFree.EditorTools
{
    [InitializeOnLoad] public static class GearAndSkillsValidation
    {
        const string Key="DiceFree.GearSkills.";static IEnumerator flow;static double deadline;static int checks;
        static GearAndSkillsValidation()=>EditorApplication.playModeStateChanged+=Changed;
        static void Need(bool ok,string message){if(!ok)throw new InvalidOperationException(message);checks++;}
        [CliCommand("dicefree.gear-skills.test","Validate funded shop purchases, all eleven slots on all four classes, persistence, and extensible skill layouts.")]
        public static object Start()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play Mode first.");
            if(EditorSceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("Save the scene first.");
            SessionState.SetString(Key+"scene",EditorSceneManager.GetActiveScene().path);SessionState.SetBool(Key+"options",EditorSettings.enterPlayModeOptionsEnabled);SessionState.SetInt(Key+"flags",(int)EditorSettings.enterPlayModeOptions);
            SessionState.SetBool(Key+"cleanup",true);SessionState.SetBool(Key+"resume",false);SessionState.SetString(Key+"status","running");SessionState.SetString(Key+"error","");
            PersistenceTestGuard.UseIsolatedSaveRootForNextPlay(Path.Combine(Path.GetTempPath(),"DiceFree-GearSkills-"+Guid.NewGuid().ToString("N")));
            EditorSettings.enterPlayModeOptionsEnabled=false;EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/Cornberg.unity");EditorApplication.EnterPlaymode();return Status();
        }
        [CliCommand("dicefree.gear-skills.status","Read all-slot gear and flexible skill verification.")]
        public static object Status()=>new{status=SessionState.GetString(Key+"status","idle"),checks=SessionState.GetInt(Key+"checks",0),phase=SessionState.GetString(Key+"phase",""),error=SessionState.GetString(Key+"error","")};
        [CliCommand("dicefree.gear-skills.resume","Finish verification after visual capture.")]
        public static object Resume(){SessionState.SetBool(Key+"resume",true);return Status();}
        static void Changed(PlayModeStateChange state)
        {
            if(state==PlayModeStateChange.EnteredPlayMode&&SessionState.GetString(Key+"status","")=="running"){checks=0;deadline=EditorApplication.timeSinceStartup+240;flow=Flow();EditorApplication.update+=Tick;Application.logMessageReceived+=Log;}
            if(state==PlayModeStateChange.EnteredEditMode&&SessionState.GetBool(Key+"cleanup",false)){SessionState.SetBool(Key+"cleanup",false);EditorApplication.update-=Tick;Application.logMessageReceived-=Log;DevelopmentSession.End();EditorSettings.enterPlayModeOptions=(EnterPlayModeOptions)SessionState.GetInt(Key+"flags",0);EditorSettings.enterPlayModeOptionsEnabled=SessionState.GetBool(Key+"options",false);PersistenceTestGuard.ClearValidationOverrides();string scene=SessionState.GetString(Key+"scene","");if(!string.IsNullOrEmpty(scene))EditorSceneManager.OpenScene(scene);}
        }
        static void Log(string text,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception)Finish(false,text+"\n"+stack);}
        static void Tick(){if(!EditorApplication.isPlaying)return;try{if(EditorApplication.timeSinceStartup>deadline)throw new TimeoutException("Gear/skills verification timed out.");if(flow!=null&&!flow.MoveNext())Finish(true,null);}catch(Exception e){Finish(false,e.ToString());}}
        static void Phase(string value)=>SessionState.SetString(Key+"phase",value);
        static IEnumerator Flow()
        {
            CombatActor player=null;while(player==null){player=Object.FindAnyObjectByType<TraversalInput>()?.GetComponent<CombatActor>();yield return null;}
            var save=player.GetComponent<ManifestationPersistence>();while(!save.Ready||!player.Motor.Ready)yield return null;
            player.GetComponent<TraversalInput>().enabled=false;player.GetComponent<CombatInput>().enabled=false;GameplayPreferences.Current.SetAutoRetaliate(false);
            var inv=player.GetComponent<CarriedInventory>();var eq=player.GetComponent<Equipment>();var wallet=player.GetComponent<GoldWallet>();var ep=player.GetComponent<EquipmentPresentation>();var controller=player.GetComponent<AdvancementController>();var xp=player.GetComponent<ExperienceProgression>();var bar=player.GetComponent<AbilityBarSource>();
            var shop=Object.FindObjectsByType<ItemShop>().Single(s=>s.name=="NPCs/NPC_Merchant - 3D");
            Phase("merchant purchases");
            bool approached=false;
            for(int i=0;i<12&&!approached;i++){float angle=i*Mathf.PI/6;approached=player.Motor.Teleport(shop.ApproachPosition+new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle))*1.6f)&&shop.CanInteract(player);}
            Need(approached,"No reachable merchant interaction point");
            var offers=shop.Offers.Select((offer,index)=>new{offer,index}).Where(x=>x.offer?.item?.modelTestingItem==true).ToArray();
            Need(offers.Length==11&&offers.Select(x=>x.offer.item.slot).Distinct().Count()==11,"Merchant is missing an equipment slot");
            Need(offers.All(x=>x.offer.goldPrice==100&&!x.offer.provisionalPrice&&x.offer.item.allowedClassIds.Length==0),"Plain prices or class permissions");
            Need(inv.Items.Length==0&&eq.Slots.Length==0,"Fresh Echo was granted unsolicited equipment");
            wallet.Restore(99);Need(!shop.Purchase(player,offers[0].index,out _)&&wallet.Gold==99&&inv.Items.Length==0,"Unfunded purchase changed ownership/gold");
            wallet.Restore(1100);
            foreach(var offer in offers){long before=wallet.Gold;Need(shop.Purchase(player,offer.index,out var message),message);Need(wallet.Gold==before-100,"Incorrect debit");}
            Need(wallet.Gold==0&&inv.Items.Length==11&&eq.Slots.Length==0,"Purchase set, debit or implicit equip");
            Need(!shop.Purchase(player,offers[0].index,out _)&&inv.Items.Length==11,"Unfunded duplicate purchase succeeded");
            DevelopmentSession.Begin();
            foreach(string id in new[]{"class.novice","class.physically-blessed-novice","class.magically-touched-novice","class.greatblade","class.mage"})
            {
                Phase("all slots: "+id);
                if(controller.HasManifestation(id))Need(controller.TrySwitch(id),controller.Feedback);
                else{
                    var edge=controller.Definitions.Single(e=>e.targetClass.stableId==id);
                    Need(controller.TrySwitch(edge.sourceClass.stableId),"Source manifestation switch");xp.RestoreState(edge.requiredLevel,0);
                    Need(string.IsNullOrEmpty(edge.requiredQuestId)?controller.TryAdvance(edge):controller.TryAdvanceForDevelopment(edge),controller.Feedback);
                }
                player.Stats.SetEncounterMaximumHp(100000);player.Health.Restore();player.Health.Invulnerable=true;
                yield return null;yield return null;yield return null;
                foreach(var slot in eq.Slots)eq.Unequip(slot.slotId);
                var before=player.Stats.Attributes;
                foreach(var offer in offers)
                {
                    var item=inv.Items.Single(i=>i.definitionId==offer.offer.item.stableId);
                    Need(eq.Equip(item.instanceId,offer.offer.item.slot),"Equip "+id+"/"+offer.offer.item.slot);
                }
                yield return null;yield return null;yield return null;
                var root=player.GetComponent<ClassPresentationSwitcher>().ActiveVisualRoot;
                Need(eq.Slots.Length==11&&eq.Slots.All(eq.IsResolved),"Not all eleven slots resolve on "+id);
                var after=player.Stats.Attributes;
                Need(Mathf.Approximately(after.strength-before.strength,1)&&Mathf.Approximately(after.agility-before.agility,1)&&Mathf.Approximately(after.intelligence-before.intelligence,1)&&Mathf.Approximately(after.spirit-before.spirit,1)&&Mathf.Approximately(after.vitality-before.vitality,2),"Plain primary stat totals on "+id);
                foreach(var offer in offers)
                {
                    var visual=ep.Bindings.Where(b=>b.definition==offer.offer.item).SelectMany(b=>b.visuals).Single(g=>g!=null&&g.transform.IsChildOf(root));
                    Need(visual.activeInHierarchy&&visual.GetComponentsInChildren<Renderer>().Any(r=>r.enabled),"Missing visible gear "+id+"/"+offer.offer.item.slot);
                    foreach(var attachment in visual.GetComponentsInChildren<PlainGearAttachment>())
                    {
                        var bone=(Transform)new SerializedObject(attachment).FindProperty("bone").objectReferenceValue;
                        Need(bone!=null&&bone.IsChildOf(root),"Gear references another class's skeleton");
                    }
                }
                Need(root.GetComponentsInChildren<Collider>(true).Length==0,"Gear added a gameplay collider");
                Need(save.Flush(),"Gear save failed");
            }
            Need(controller.TrySwitch("class.novice")&&eq.Slots.Length==11&&controller.TrySwitch("class.greatblade")&&eq.Slots.Length==11,"All-slot manifestation roundtrip");
            var disk=new LocalEchoStore(Path.GetDirectoryName(save.SavePath)).Load();
            foreach(string id in new[]{"class.novice","class.physically-blessed-novice","class.magically-touched-novice","class.greatblade","class.mage"})
                Need(ManifestationRoster.ReadManifestation(disk,id).equipment.Length==11,"On-disk equipped slots missing "+id);
            Phase("flexible definitions and layouts");
            Need(controller.TrySwitch("class.physically-blessed-novice"),"Physical skill fixture switch");yield return null;
            xp.RestoreState(40,0);
            var physical=player.GetComponent<PhysicalSkillProgression>();var original=physical.Definitions.ToArray();var oldRanks=physical.CaptureState();var clones=new List<PhysicalSkillDefinition>();
            try
            {
                for(int i=0;i<8;i++){var d=Object.Instantiate(original[i<6?1:4]);d.stableId="qa.flex."+i;d.displayName="Fixture "+i;d.maxRank=i+2;clones.Add(d);}
                physical.RestoreState(Array.Empty<SkillRankState>());physical.Configure(original.Concat(clones).ToArray());
                physical.RestoreState(physical.Definitions.Select(d=>new SkillRankState(d.stableId,1)).ToArray());
                Need(bar.AbilityCount==13&&bar.PageCount==2,"Larger kit did not page");
                Need(bar.View(4).passive&&bar.View(5).Exists&&!bar.View(5).passive,"Authored mixed ordering was not preserved");
                Need(bar.Activate(5),"Fifth-plus active could not cast");player.GetComponent<PhysicalSkillCaster>().ResetForManifestationLoad();
                Need(bar.SetPage(1)&&bar.View(3).passive&&bar.View(4).passive,"Multiple passives missing from page two");
                Need(!bar.Activate(3)&&bar.Upgrade(4),"Passive activation/upgrade rules on page two");
                Need(bar.View(9).Exists&&bar.View(10).Exists&&bar.View(11).Exists&&!bar.View(8).Exists,"Paging changed shared commands or invented an ultimate");
                Need(!bar.SetPage(2),"Invalid page accepted");
                Need(physical.CanRestore(new[]{new SkillRankState(clones[7].stableId,clones[7].maxRank)},40)&&!physical.CanRestore(new[]{new SkillRankState(clones[7].stableId,clones[7].maxRank+1)},40),"Variable rank caps were not enforced");
                var foreign=Object.Instantiate(original[1]);foreign.stableId="qa.foreign";Need(!physical.Spend(foreign),"Unregistered skill could spend points");Object.Destroy(foreign);
            }
            finally
            {
                physical.RestoreState(Array.Empty<SkillRankState>());physical.Configure(original);physical.RestoreState(oldRanks);
                foreach(var clone in clones)Object.Destroy(clone);bar.SetPage(0);player.GetComponent<PhysicalSkillCaster>().ResetForManifestationLoad();
            }
            var custom=new FutureProvider(physical.ClassId);bar.Register(custom);
            Need(bar.PageCount==3&&bar.View(8).name=="Fixture Ultimate","Custom provider/ultimate was not registered");
            Need(bar.SetPage(2)&&bar.View(0).name=="Future 16"&&bar.Activate(0)&&custom.activations==1,"Third-page activation failed");
            Need(bar.Upgrade(0)&&custom.upgrades==1&&bar.Upgrade(8),"Custom rank economy was ignored");
            Need(!bar.Activate(1)&&bar.Activate(8)&&custom.activations==2,"Future passive or ultimate activation");
            bar.Register(ClassAbilityProviders.ExistingKits(player.gameObject).Single(p=>p.ClassId==physical.ClassId));bar.SetPage(0);
            foreach(string action in new[]{"Gameplay/Class skill 5","Gameplay/Class skill 6","Gameplay/Class skill 7","Gameplay/Class skill 8","Gameplay/Class ultimate"})
                Need(InputBindings.Current.Asset.FindAction(action,false)!=null,"New rebindable skill action missing "+action);
            Need(controller.TrySwitch("class.magically-touched-novice"),"Final visual class");yield return null;yield return null;
            Need(bar.SkillPage==0&&bar.AbilityCount==5,"Class change did not reset the page");
            bool near=false;for(int i=0;i<12&&!near;i++){float angle=i*Mathf.PI/6;near=player.Motor.Teleport(shop.ApproachPosition+new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle))*1.6f)&&shop.CanInteract(player);}
            Need(near&&player.GetComponent<Interactor>().Order(shop),"Open merchant UI");
            Object.FindAnyObjectByType<DiceFree.Core.ExplorationCamera>()?.RecenterForTravel(player.transform);
            Need(save.Flush(),"Final profile save");
            SessionState.SetInt(Key+"checks",checks);SessionState.SetString(Key+"status","visual_ready");while(!SessionState.GetBool(Key+"resume",false))yield return null;
        }
        sealed class FutureProvider:IClassAbilityProvider
        {
            public string ClassId{get;}public int Count=>20;public int UltimateIndex=>19;public int UnspentPoints=>100;public int activations,upgrades;
            public FutureProvider(string id){ClassId=id;}
            public AbilityBarSource.SlotView View(int index,string key)=>new(index==19?"Fixture Ultimate":"Future "+index,key,1,12,0,true,"Custom future kit",null,"",index==17,true);
            public bool Upgrade(int index){upgrades++;return true;}public string UpgradeTooltip(int index)=>"Custom rank budget";
            public bool Activate(int index,SkillTargetingController t,IQueuedOrders q,bool append){activations++;return true;}
        }
        static void Finish(bool ok,string error)
        {
            flow=null;EditorApplication.update-=Tick;Application.logMessageReceived-=Log;SessionState.SetString(Key+"status",ok?"passed":"failed");SessionState.SetString(Key+"error",error??"");SessionState.SetInt(Key+"checks",checks);
            File.WriteAllText("Library/GearAndSkillsResults.json",JsonUtility.ToJson(new Record{passed=ok,checks=checks,error=error},true));EditorApplication.ExitPlaymode();
        }
        [Serializable]class Record{public bool passed;public int checks;public string error;}
    }
}
