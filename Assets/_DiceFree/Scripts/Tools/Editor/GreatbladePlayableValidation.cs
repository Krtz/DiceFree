using System;
using System.Collections;
using System.IO;
using System.Linq;
using DiceFree.Advancement;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Development;
using DiceFree.Input;
using DiceFree.Persistence;
using DiceFree.Progression;
using DiceFree.Skills;
using DiceFree.UI;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;
using Kind=DiceFree.Progression.GreatbladeProgressionRules.Kind;
namespace DiceFree.EditorTools
{
    [InitializeOnLoad]
    public static class GreatbladePlayableValidation
    {
        const string Key="DiceFree.GreatbladePlayable.";
        static IEnumerator flow;static double deadline;static int checks;
        static GreatbladePlayableValidation()=>EditorApplication.playModeStateChanged+=Changed;
        static void Need(bool ok,string message){if(!ok)throw new InvalidOperationException(message);checks++;}
        [CliCommand("dicefree.greatblade.playable.test","Validate Greatblade combat, collision, advancement and saved rank switching using an isolated dev Echo.")]
        public static object Start()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play Mode first.");
            if(EditorSceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("Save scene edits first.");
            SessionState.SetString(Key+"scene",EditorSceneManager.GetActiveScene().path);
            SessionState.SetBool(Key+"options",EditorSettings.enterPlayModeOptionsEnabled);
            SessionState.SetInt(Key+"flags",(int)EditorSettings.enterPlayModeOptions);
            SessionState.SetBool(Key+"cleanup",true);SessionState.SetBool(Key+"resume",false);
            SessionState.SetString(Key+"status","running");SessionState.SetString(Key+"error","");
            string root=Path.Combine(Path.GetTempPath(),"DiceFree-Greatblade-"+Guid.NewGuid().ToString("N"));
            SessionState.SetString(Key+"saveRoot",root);PersistenceTestGuard.UseIsolatedSaveRootForNextPlay(root);
            EditorSettings.enterPlayModeOptionsEnabled=false;EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/DevelopmentRoom.unity");
            EditorApplication.EnterPlaymode();return Status();
        }
        [CliCommand("dicefree.greatblade.playable.status","Read Greatblade focused gameplay validation.")]
        public static object Status()=>new{status=SessionState.GetString(Key+"status","idle"),phase=SessionState.GetString(Key+"phase",""),error=SessionState.GetString(Key+"error",""),checks=SessionState.GetInt(Key+"checks",0)};
        [CliCommand("dicefree.greatblade.playable.resume","Finish the successful Greatblade test after visual capture.")]
        public static object Resume(){SessionState.SetBool(Key+"resume",true);return Status();}
        static void Changed(PlayModeStateChange value)
        {
            if(value==PlayModeStateChange.EnteredPlayMode&&SessionState.GetString(Key+"status","")=="running")
            {
                checks=0;deadline=EditorApplication.timeSinceStartup+180;UnityEngine.Application.runInBackground=true;
                flow=Flow();EditorApplication.update+=Tick;UnityEngine.Application.logMessageReceived+=Log;
            }
            if(value==PlayModeStateChange.EnteredEditMode&&SessionState.GetBool(Key+"cleanup",false))
            {
                SessionState.SetBool(Key+"cleanup",false);EditorApplication.update-=Tick;UnityEngine.Application.logMessageReceived-=Log;
                EditorSettings.enterPlayModeOptions=(EnterPlayModeOptions)SessionState.GetInt(Key+"flags",0);
                EditorSettings.enterPlayModeOptionsEnabled=SessionState.GetBool(Key+"options",false);
                PersistenceTestGuard.ClearValidationOverrides();
                string scene=SessionState.GetString(Key+"scene","");if(!string.IsNullOrEmpty(scene))EditorSceneManager.OpenScene(scene);
            }
        }
        static void Log(string message,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception)Finish(false,message+"\n"+stack);}
        static void Tick()
        {
            if(!EditorApplication.isPlaying)return;
            try
            {
                if(EditorApplication.timeSinceStartup>=deadline)throw new TimeoutException("Greatblade validation timed out.");
                if(flow!=null&&!flow.MoveNext())Finish(true,null);
            }
            catch(Exception e){Finish(false,e.ToString());}
        }
        static void Phase(string value)=>SessionState.SetString(Key+"phase",value);
        static IEnumerator Flow()
        {
            Phase("initialize");
            while(DevelopmentRoomController.Current==null||DevelopmentRoomController.Current.player==null)yield return null;
            var dev=DevelopmentRoomController.Current;var player=dev.player;
            var save=player.GetComponent<ManifestationPersistence>();
            while(!save.Ready||dev.Map==null||!player.Motor.Ready)yield return null;
            player.GetComponent<TraversalInput>().enabled=false;player.GetComponent<CombatInput>().enabled=false;
            GameplayPreferences.Current.SetAutoRetaliate(false);
            var advance=player.GetComponent<AdvancementController>();
            var xp=player.GetComponent<ExperienceProgression>();
            var edge=advance.Definitions.Single(x=>x.targetClass.stableId==GreatbladeProgressionRules.ClassId);
            Need(dev.SelectClass("class.physically-blessed-novice"),"Physical selection failed.");
            xp.RestoreState(29,0);Need(!advance.CanAdvance(edge),"Greatblade unlocked before Physical level 30.");
            xp.RestoreState(30,0);Need(advance.CanAdvance(edge),"Physical level 30 cannot advance.");
            var inventory=player.GetComponent<DiceFree.Items.CarriedInventory>();
            var equipment=player.GetComponent<DiceFree.Items.Equipment>();
            var shieldFixture=ScriptableObject.CreateInstance<DiceFree.Items.ItemDefinition>();
            shieldFixture.stableId="dev.greatblade-offhand-fixture";shieldFixture.displayName="Test shield";shieldFixture.slot=DiceFree.Items.EquipmentSlot.OffHand;
            inventory.Configure(inventory.Definitions.Concat(new[]{shieldFixture}).ToArray());
            var offhand=inventory.Grant(shieldFixture.stableId);
            Need(equipment.Equip(offhand.instanceId,DiceFree.Items.EquipmentSlot.OffHand),"Parent offhand fixture.");
            Need(advance.TryAdvance(edge),advance.Feedback);
            yield return null;yield return null;
            var skills=player.GetComponent<GreatbladeSkillProgression>();var caster=player.GetComponent<GreatbladeSkillCaster>();
            var resources=player.GetComponent<ActorResourceController>();var basic=player.GetComponent<BasicAttack>();
            var bar=player.GetComponent<AbilityBarSource>();
            Need(player.Stats.Level==1&&player.DisplayName.Contains("Greatblade"),"Class level/title after advancement.");
            Need(save.HasManifestation("class.physically-blessed-novice"),"Parent manifestation lost.");
            var childOffhand=inventory.Items.Single(i=>i.definitionId==shieldFixture.stableId);
            Need(!equipment.IsEquipped(childOffhand.instanceId),"Offhand must return to bag on Greatblade advancement.");
            Need(!equipment.Equip(childOffhand.instanceId,DiceFree.Items.EquipmentSlot.OffHand),"Greatblade accepted an offhand.");
            foreach(var rule in GreatbladeProgressionRules.Skills)
                Need(skills.Rank(rule.kind)==GreatbladeSkillProgression.StarterRank(rule.kind),"Free starter ranks or locked abilities incorrect.");
            Need(skills.UnspentPoints==0&&resources.Has(GreatbladeSkillCaster.MomentumId)&&caster.Momentum==0,"Starter point economy/resource initialization.");
            Need(Enumerable.Range(0,6).All(i=>bar.View(i).Exists)&&bar.View(4).passive&&bar.View(5).passive&&!bar.View(8).Exists,"Six mixed skill slots / Tier IV ultimate reservation.");
            Need(!skills.Spend(Kind.OverheadBreaker)&&!caster.CanPrepare(Kind.OverheadBreaker),"Breaker incorrectly usable at level 1.");
            Need(player.GetComponent<ClassPresentationSwitcher>().ActiveVisualRoot?.name=="GreatbladePresentation","Playable model did not activate.");
            Phase("cleave and basic Momentum");
            var targets=CombatActor.All.Where(a=>a.Stats.Definition?.stableId=="dev.practice-target").ToArray();
            Need(targets.Length==3,"Three dev practice targets required.");
            player.Health.Invulnerable=true;
            var positions=new[]{new Vector3(-9,0,2.5f),new Vector3(-7,0,2.5f),new Vector3(-8,0,-2.5f)};
            for(int i=0;i<3;i++){targets[i].Stats.SetEncounterMaximumHp(100000);targets[i].Health.Restore();Need(targets[i].Motor.Teleport(positions[i]),"Target placement.");}
            Need(player.Motor.Teleport(new Vector3(-8,0,0)),"Hero placement.");
            float backHp=targets[2].Health.Current,frontHp=targets[0].Health.Current;
            Need(caster.CastWideArc(new Vector3(-8,0,3)),caster.Feedback);
            float until=Time.time+3;while(caster.Busy&&Time.time<until)yield return null;
            Need(!caster.Busy&&caster.LastCastHits==2&&targets[0].Health.Current<frontHp&&targets[2].Health.Current==backHp,"Wide Arc cone should hit front pair only.");
            Need(caster.Momentum>0&&caster.Momentum<=caster.Tuning.areaMomentumCap*GreatbladeProgressionRules.MomentumGainFactor(1)+.01f,"Cleave Momentum / cap.");
            float before=caster.Momentum;int hits=basic.Hits;
            Need(basic.Order(targets[0]),"Basic attack rejected.");
            until=Time.time+6;while(basic.Hits==hits&&Time.time<until)yield return null;
            basic.Cancel();Need(basic.Hits>hits&&caster.Momentum>before,"Connecting basic attack failed to build Momentum.");
            caster.ResetForManifestationLoad();player.Effects.ApplyAccuracyPenalty("test.blind",1,3);
            Need(caster.CastWideArc(new Vector3(-8,0,3)),"Blind cleave setup.");
            until=Time.time+3;while(caster.Busy&&Time.time<until)yield return null;
            Need(caster.LastCastHits==0&&caster.Momentum==0,"Misses must not generate Momentum.");
            player.Effects.ClearTransient();
            Phase("collision-safe Spin Dash");
            caster.ResetForManifestationLoad();
            Need(player.Motor.Teleport(new Vector3(6,0,-10)),"Spin start placement.");
            for(int i=0;i<3;i++)Need(targets[i].Motor.Teleport(new Vector3(-12+i*4,0,8)),"Clear spin targets.");
            Need(caster.CastSpinDash(new Vector3(6,0,-2)),caster.Feedback);
            until=Time.time+5;while(caster.Busy&&Time.time<until)yield return null;
            Need(Vector3.Distance(player.transform.position,new Vector3(6,0,-2))<.5f,"Open route dash did not reach target.");
            caster.ResetForManifestationLoad();Need(player.Motor.Teleport(new Vector3(6,0,-10)),"Blocked dash start.");
            Need(targets[0].Motor.Teleport(new Vector3(6,0,-6)),"Blocker placement.");
            yield return null;yield return null;
            float blockerHp=targets[0].Health.Current;
            Need(caster.CastSpinDash(new Vector3(6,0,-2)),caster.Feedback);
            until=Time.time+5;while(caster.Busy&&Time.time<until)yield return null;
            Need(caster.LastCastHits==1&&targets[0].Health.Current<blockerHp,"Spin should hit blocker exactly once.");
            Need(player.transform.position.z<-6&&Vector3.Distance(player.transform.position,targets[0].transform.position)>=player.Radius+targets[0].Radius-.08f,"Spin ghosted through a living target.");
            caster.ResetForManifestationLoad();float momentum=caster.Momentum;
            Need(!caster.CastSpinDash(new Vector3(30,0,-2))&&caster.CooldownRemaining(Kind.SpinDash)==0&&caster.Momentum==momentum,"Invalid dash consumed cooldown/resource.");
            Need(player.Motor.Teleport(new Vector3(18,0,-10)),"Wall test placement.");
            Need(!caster.CastSpinDash(new Vector3(25,0,-10))&&caster.CooldownRemaining(Kind.SpinDash)==0,"Dash accepted inaccessible ground beyond wall.");
            Phase("rank gates, finisher, mastery and cry");
            xp.RestoreState(5,0);Need(!skills.Spend(Kind.OverheadBreaker),"Breaker learned before 6.");
            xp.RestoreState(6,0);Need(skills.Spend(Kind.OverheadBreaker)&&skills.Spend(Kind.GreatswordMastery),"Level 6 skills unavailable.");
            Need(!skills.Spend(Kind.GreatswordMastery),"Mastery rank 3 learned before level 12.");
            Need(!caster.CastOverheadBreaker(targets[0]),"Finisher ignored Momentum requirement.");
            Need(player.Motor.Teleport(new Vector3(6,0,-8)),"Finisher placement.");
            resources.Restore(GreatbladeSkillCaster.MomentumId,100);
            float victimHp=targets[0].Health.Current;
            Need(caster.CastOverheadBreaker(targets[0]),caster.Feedback);
            Need(caster.Momentum==0&&caster.LastMomentumSpent==100,"Finisher must consume ALL Momentum.");
            until=Time.time+3;while(caster.Busy&&Time.time<until)yield return null;
            Need(caster.LastCastHits==1&&targets[0].Health.Current<victimHp,"Finisher did not connect.");
            xp.RestoreState(18,0);
            Need(skills.Spend(Kind.GreatswordMastery)&&skills.Spend(Kind.GreatswordMastery)&&skills.Spend(Kind.BattleCry),"Mastery/cry gated ranks.");
            caster.ResetForManifestationLoad();resources.Restore(GreatbladeSkillCaster.MomentumId,100);
            Need(caster.CastOverheadBreaker(targets[0]),caster.Feedback);
            until=Time.time+3;while(caster.Busy&&Time.time<until)yield return null;
            Need(Mathf.Abs(player.Health.TemporaryShield-player.Health.Maximum*.05f)<.1f,"Rank-4 finisher shield missing.");
            player.Health.Invulnerable=false;float hp=player.Health.Current;
            var shielded=player.Health.ApplyPacket(targets[0],new DamageResult{mitigated=5,channel=DamageChannel.Physical});
            Need(shielded.absorbed==5&&shielded.applied==0&&player.Health.Current==hp,"Shield absorption damaged HP.");
            player.Health.Invulnerable=true;
            float baseDamage=basic.DamageRange.x;
            Need(caster.CastBattleCry()&&basic.DamageRange.x>baseDamage,"Battle Cry did not increase actual damage.");
            caster.ResetForManifestationLoad();
            Need(Mathf.Abs(basic.DamageRange.x-baseDamage)<.01f&&player.Health.TemporaryShield==0,"Transient buff/shield cleanup.");
            float speed=player.Stats.AttackSpeed;
            Need(player.Motor.Teleport(new Vector3(-8,0,0)),"Haste placement.");
            Need(caster.CastWideArc(new Vector3(-8,0,3)),caster.Feedback);
            until=Time.time+3;while(caster.Busy&&Time.time<until)yield return null;
            Need(player.Stats.AttackSpeed>speed,"Mastery cleave haste has no real effect.");
            caster.ResetForManifestationLoad();resources.Restore(GreatbladeSkillCaster.MomentumId,50);
            until=Time.time+6.5f;while(Time.time<until)yield return null;
            Need(caster.Momentum<50&&caster.Momentum>0,"Out-of-combat Momentum decay.");
            Phase("durable rank switching");
            var saved=skills.CaptureState();Need(save.Flush(),"Greatblade save failed.");
            Need(advance.TrySwitch("class.physically-blessed-novice"),advance.Feedback);
            yield return null;
            Need(!resources.Has(GreatbladeSkillCaster.MomentumId)&&player.Health.TemporaryShield==0,"Momentum/shield leaked to parent class.");
            Need(equipment.IsEquipped(offhand.instanceId),"Parent equipment snapshot was changed by child advancement.");
            Need(advance.TrySwitch(GreatbladeProgressionRules.ClassId),advance.Feedback);
            yield return null;
            Need(player.Stats.Level==18&&saved.All(s=>skills.CaptureState().Any(v=>v.stableId==s.stableId&&v.rank==s.rank)),"Saved Greatblade level/ranks did not roundtrip.");
            Need(caster.Momentum==0&&caster.CooldownRemaining(Kind.WideArc)==0,"Transient resource/cooldown persisted.");
            Need(!skills.CanRestore(new[]{new SkillRankState(GreatbladeSkillProgression.Id(Kind.OverheadBreaker),1)},1),"Invalid saved unlock passed validation.");
            var root=new LocalEchoStore(Path.GetDirectoryName(save.SavePath)).Load();
            Need(root!=null&&ManifestationRoster.ReadManifestation(root,GreatbladeProgressionRules.ClassId).classSkills.Length==6,"On-disk Greatblade skill record missing.");
            var home=player.GetComponent<ReturnHomeAbility>();
            Need(home.TryBegin(),"Return-home channel setup.");
            Need(bar.Activate(3)&&!home.Casting,"Greatblade activation must interrupt the shared return-home channel.");
            caster.ResetForManifestationLoad();
            Phase("visual capture");
            Need(player.Motor.Teleport(new Vector3(0,0,-8)),"Visual placement.");
            dev.explorationCamera.RecenterForTravel(player.transform);
            SessionState.SetInt(Key+"checks",checks);SessionState.SetString(Key+"status","visual_ready");
            while(!SessionState.GetBool(Key+"resume",false))yield return null;
        }
        static void Finish(bool ok,string error)
        {
            flow=null;EditorApplication.update-=Tick;UnityEngine.Application.logMessageReceived-=Log;
            SessionState.SetInt(Key+"checks",checks);SessionState.SetString(Key+"status",ok?"passed":"failed");SessionState.SetString(Key+"error",error??"");
            File.WriteAllText("Library/GreatbladePlayableResults.json",JsonUtility.ToJson(new Record{passed=ok,checks=checks,error=error},true));
            if(ok)Debug.Log("DICEFREE_GREATBLADE_PLAYABLE_OK");else Debug.LogWarning("Greatblade focused test failed: "+error);
            EditorApplication.ExitPlaymode();
        }
        [Serializable]class Record{public bool passed;public int checks;public string error;}
    }
}
