using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using DiceFree.AI;
using DiceFree.Advancement;
using DiceFree.Briarback;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Core;
using DiceFree.Dungeons;
using DiceFree.Items;
using DiceFree.Persistence;
using DiceFree.Progression;
using DiceFree.Quests;
using DiceFree.Skills;
using DiceFree.UI;
using DiceFree.Visuals;
using DiceFree.World;
using UnityEngine;
using UnityEngine.AI;
using Object=UnityEngine.Object;
using G=DiceFree.Progression.GreatbladeProgressionRules.Kind;
using M=DiceFree.Progression.MageProgressionRules.Kind;

public static class TierTwoBriarbackReview
{
    static string phase,failure;static readonly List<object> trials=new(),motion=new();static int advancements;
    static void Save()=>File.WriteAllText("Logs/TierTwoBriarbackReview.json",Newtonsoft.Json.JsonConvert.SerializeObject(new{phase,failure,advancements,setupFixtures="Disposable profile; ordinary XP and existing-item grants, initial placement, between-trial restores. Dungeon miniboss/boss fixture damage is separate from regional combat. Five-ring Mage captures and mentor turn-ins use real runtime systems.",regionalCombat="Normal time, normal damage, no invulnerability and no disabled enemy AI until the separately labelled cosmetic phase.",limitation="One skill policy and existing item loadouts; no statistical or party balance approval.",trials=trials.ToArray(),motion=motion.ToArray()},Newtonsoft.Json.Formatting.Indented));
    public static string Begin(){_=Observe();return "Started Tier II field review; poll Logs/TierTwoBriarbackReview.json.";}
    static async Task Observe(){try{await Run();phase="passed";Save();}catch(Exception e){failure=e.ToString();phase="failed";Save();Debug.LogException(e);}}
    static async Task Run()
    {
        phase="initialize";Save();CombatActor p=null;await Until(()=>(p=Object.FindAnyObjectByType<TraversalInput>()?.GetComponent<CombatActor>())!=null&&p.Motor.Ready&&p.GetComponent<ManifestationPersistence>().Ready,"player");
        p.GetComponent<TraversalInput>().enabled=false;p.GetComponent<CombatInput>().enabled=false;
        var save=p.GetComponent<ManifestationPersistence>();var advance=p.GetComponent<AdvancementController>();var xp=p.GetComponent<ExperienceProgression>();var journal=p.GetComponent<QuestJournal>();
        Directory.CreateDirectory("VisualBaselines/TierTwo/Field");
        for(int kind=0;kind<2;kind++)
        {
            if(kind>0)Need(advance.TrySwitch("class.novice"),advance.Feedback);
            while(xp.Level<10)xp.Grant(xp.RequiredXp);
            string parent=kind==0?"class.physically-blessed-novice":"class.magically-touched-novice";
            Need(advance.TryAdvance(advance.Definitions.Single(d=>d.targetClass.stableId==parent)),advance.Feedback);while(xp.Level<30)xp.Grant(xp.RequiredXp);
            InteractionTarget mentor=kind==0?(InteractionTarget)Object.FindAnyObjectByType<GreatbladeMentor>():Object.FindAnyObjectByType<MageMentor>();
            var quest=kind==0?((GreatbladeMentor)mentor).TrainingQuest:((MageMentor)mentor).TrainingQuest;
            p.Health.Invulnerable=true;p.Health.Restore();Need(p.Motor.Teleport(mentor.ApproachPosition+Vector3.back*1.8f),"mentor approach");await Task.Delay(40);
            Need(kind==0?((GreatbladeMentor)mentor).AcceptTraining(p):((MageMentor)mentor).AcceptTraining(p),"mentor acceptance");
            phase="real-dungeon-trial-"+kind;Save();
            var portal=Object.FindObjectsByType<DungeonPortal>().Single(t=>t.isActiveAndEnabled&&t.definition.stableId=="dungeon.slime");Need(p.Motor.Teleport(portal.ApproachPosition+Vector3.back*1.5f),"portal placement");portal.Interact(p);
            await Until(()=>SlimeDungeonRun.Current!=null&&SlimeDungeonRun.Current.StagingReady,"dungeon staging");var run=SlimeDungeonRun.Current;Need(run.StartNow(p),"dungeon start");
            var mini=Object.FindObjectsByType<DungeonSlime>().Single(s=>s.Role==SlimeRole.Miniboss);mini.Actor.Health.ApplyPacket(p,new DamageResult{mitigated=1000000});
            if(kind==0){run.SolvePuzzle(false);await Task.Delay(40);var boss=Object.FindObjectsByType<DungeonSlime>().Single(s=>s.Role==SlimeRole.Boss);boss.Actor.Health.ApplyPacket(p,new DamageResult{mitigated=1000000});}
            else
            {
                var puzzle=Object.FindAnyObjectByType<SlimeTreePuzzle>();
                for(int i=0;i<5;i++){DungeonSlime slime=null;await Until(()=>(slime=Object.FindObjectsByType<DungeonSlime>().FirstOrDefault(s=>s.Role==SlimeRole.Puzzle&&!s.Captured&&s.Actor.Alive&&s.Actor.Motor.Ready))!=null,"puzzle spawn");Need(slime.Actor.Motor.Teleport(puzzle.rings[i].position),"ring position");int count=i+1;await Until(()=>puzzle.Filled==count,"real ring capture");}
            }
            Need(journal.GetProgress(quest.stableId).status==QuestStatus.ReadyToTurnIn&&save.CurrentClassId==parent,"objective/parent state");run.Abandon();await Until(()=>SlimeDungeonRun.Current==null,"dungeon return");
            p.Health.Restore();Need(p.Motor.Teleport(mentor.ApproachPosition+Vector3.back*1.8f),"mentor return");await Task.Delay(50);
            Need(kind==0?((GreatbladeMentor)mentor).CompleteTraining(p):((MageMentor)mentor).CompleteTraining(p),"real mentor turn-in");Need(xp.Level==1,"Tier II must start at one");advancements++;p.Health.Invulnerable=false;await Task.Delay(120);
            foreach(int level in new[]{25,35}){while(xp.Level<level)xp.Grant(xp.RequiredXp);Allocate(p,kind);await Camp(p,kind,level);}
        }
        phase="cosmetic-fixtures-ai-disabled";Save();p.Health.Invulnerable=true;
        foreach(var b in Object.FindObjectsByType<MonoBehaviour>().Where(b=>b is AggroBehaviour||b is BasicAttack||b is QuilboarCombat||b is BriarbackChieftainEncounter))if(b.gameObject!=p.gameObject)b.enabled=false;
        foreach(int kind in new[]{0,1}){Need(advance.TrySwitch(kind==0?GreatbladeProgressionRules.ClassId:MageProgressionRules.ClassId),advance.Feedback);await Motion(p,kind);}
        Need(save.Flush(),"final isolated save");
    }
    static void Allocate(CombatActor p,int kind){bool spent;do{spent=false;if(kind==0){foreach(var r in GreatbladeProgressionRules.Skills)spent|=p.GetComponent<GreatbladeSkillProgression>().Spend(r.kind);}else foreach(var r in MageProgressionRules.Skills)spent|=p.GetComponent<MageSkillProgression>().Spend(r.kind);}while(spent);}
    static async Task Camp(CombatActor p,int kind,int level)
    {
        Need(Time.timeScale==1&&!p.Health.Invulnerable,"normal combat authority");var inv=p.GetComponent<CarriedInventory>();var eq=p.GetComponent<Equipment>();foreach(var s in eq.Slots)eq.Unequip(s.slotId);
        var gear=new List<string>{"item.cornberg.work-gloves","item.cornberg.farmers-pants","item.cornberg.forest-shoes","item.slime.slimy-tophat","item.briarback.bristlehide-mantle",kind==0?"item.alpha-wolf.greatsword":"item.bronze-dagger"};
        foreach(string id in gear){var d=inv.Resolve(id);Need(d!=null,"gear "+id);var item=inv.Items.FirstOrDefault(i=>i.definitionId==id)??FixedItemGrant.Grant(inv,d);Need(eq.Equip(item.instanceId,d.slot),"equip "+id);}
        var target=Object.FindObjectsByType<QuilboarCombat>().Single(q=>q.role==QuilboarRole.Raider&&q.GetComponent<ActorStats>().Level==25).GetComponent<CombatActor>();var home=target.GetComponent<AggroBehaviour>().Home;
        foreach(var q in Object.FindObjectsByType<QuilboarCombat>().Where(q=>Vector3.Distance(q.GetComponent<AggroBehaviour>().Home,home)<12)){q.GetComponent<AggroBehaviour>().ResetEncounter();q.GetComponent<Health>().Restore();}
        p.Health.Restore();p.GetComponent<ActorResourceController>().RefillAll();var basic=p.GetComponent<BasicAttack>();int hits=0,casts=0;float received=0,dealt=0,min=p.Health.Current,max=p.Health.Maximum;var attackers=new HashSet<string>();
        Action<CombatActor,DamageResult> receive=(s,r)=>{hits++;received+=r.applied;min=Mathf.Min(min,p.Health.Current);if(s!=null)attackers.Add(s.name);};Action<CombatActor,DamageResult> deal=(s,r)=>{if(s==p)dealt+=r.applied;};p.Health.Damaged+=receive;foreach(var a in CombatActor.All)if(a!=p)a.Health.Damaged+=deal;
        float start=Time.realtimeSinceStartup;phase=(kind==0?"greatblade":"mage")+"-camp-"+level;Save();
        try
        {
            Need(p.Motor.Teleport(Point(home+Vector3.back*4)),"camp placement");Object.FindAnyObjectByType<ExplorationCamera>().RecenterForTravel(p.transform);Need(basic.Order(target),"camp attack");bool captured=false;
            while(p.Alive&&target.Alive&&Time.realtimeSinceStartup-start<35&&p.Health.Current>max*.3f)
            {
                if(kind==0)
                {
                    var c=p.GetComponent<GreatbladeSkillCaster>();if(!c.Busy){if(c.CastBattleCry())casts++;if(c.CastOverheadBreaker(target))casts++;else if(c.CastWideArc(target.transform.position))casts++;}
                }
                else
                {
                    var c=p.GetComponent<MageSkillCaster>();if(c.Cast(M.MathBarrier))casts++;if(c.Cast(M.HalfLife,target))casts++;if(c.Cast(M.FrostSnare,target))casts++;if(c.Cast(M.ArcLightning,target))casts++;if(c.Cast(M.EmberBolt,target))casts++;if(c.CastQuicksand(target.transform.position))casts++;
                }
                if(p.CanAct&&basic.Target==null&&!(p.GetComponent<GreatbladeSkillCaster>()?.Busy??false))basic.Order(target);
                if(!captured&&Time.realtimeSinceStartup-start>2){ScreenCapture.CaptureScreenshot("VisualBaselines/TierTwo/Field/"+kind+"-"+level+".png");captured=true;}await Task.Delay(90);
            }
        }
        finally{basic.Cancel();p.Health.Damaged-=receive;foreach(var a in CombatActor.All)if(a!=p)a.Health.Damaged-=deal;}
        float seconds=Time.realtimeSinceStartup-start;string outcome=!p.Alive?"player-defeated":!target.Alive?"raider-defeated":"retreated";if(kind==0)p.GetComponent<GreatbladeSkillCaster>().CancelCast();
        bool survived=p.Alive;if(p.Alive){Need(p.Motor.MoveTo(Point(home+Vector3.back*34)),"retreat path");await Until(()=>!p.Alive||Vector3.Distance(p.transform.position,home)>28,"real retreat");survived=p.Alive;}
        Need(p.Motor.Teleport(Point(new Vector3(270,0,415))),"safe cleanup");p.Motor.Stop();await Until(()=>!CombatActor.All.Any(a=>a!=p&&a.Alive&&a.GetComponent<BasicAttack>()?.Target==p),"enemy return");
        trials.Add(new{classId=p.Stats.Definition.stableId,level,gear=gear.ToArray(),maximumHp=max,minimumHp=min,incomingHits=hits,incomingDamage=received,outgoingDamage=dealt,casts,combatSeconds=seconds,outcome,survivedRetreat=survived,attackers=attackers.OrderBy(s=>s).ToArray()});Save();p.Health.Restore();
    }
    static async Task Motion(CombatActor p,int kind)
    {
        p.Health.Invulnerable=true;p.GetComponent<BasicAttack>().Cancel();Need(p.Motor.Teleport(Point(new Vector3(270,0,415))),"motion location");var driver=p.GetComponent<NovicePresentationDriver>();var presentation=p.GetComponent<PlayerMotionPresentation>();await Task.Delay(150);var root=driver.VisualRoot;var scale=root.localScale;var model=driver.VisualAnimator;Object.FindAnyObjectByType<ExplorationCamera>().RecenterForTravel(p.transform);
        Need(presentation.ControllerClassId==p.Stats.Definition.stableId&&model.isHuman,"Tier II motion controller");
        var leg=model.GetBoneTransform(HumanBodyBones.LeftUpperLeg);var before=leg.localRotation;var walkEnd=Point(p.transform.position+Vector3.forward*5);Need(p.Motor.MoveTo(walkEnd),"walk");await Task.Delay(160);Need(Quaternion.Angle(before,leg.localRotation)>1,"Tier II frozen walk");await Until(()=>Vector3.Distance(p.transform.position,walkEnd)<.5f,"walk settle");p.Motor.Stop();
        var angles=new List<float>();bool portableSpin=false;
        if(kind==0)
        {
            var xp=p.GetComponent<ExperienceProgression>();while(xp.Level<71)xp.Grant(xp.RequiredXp);var skills=p.GetComponent<GreatbladeSkillProgression>();var c=p.GetComponent<GreatbladeSkillCaster>();
            foreach(int rank in new[]{3,5,7})
            {
                skills.RestoreState(new[]{new SkillRankState(GreatbladeSkillProgression.Id(G.WideArc),1),new SkillRankState(GreatbladeSkillProgression.Id(G.SpinDash),1),new SkillRankState(GreatbladeSkillProgression.Id(G.OverheadBreaker),1),new SkillRankState(GreatbladeSkillProgression.Id(G.GreatswordMastery),rank)});c.ResetForManifestationLoad();Need(c.CastWideArc(p.transform.position+Vector3.forward*4),"cleave fixture");await Task.Delay(45);
                var line=p.transform.Find("Greatblade committed swing outline").GetComponent<LineRenderer>();float angle=Vector3.Angle(line.GetPosition(1)-line.GetPosition(0),line.GetPosition(24)-line.GetPosition(0));Need(Mathf.Abs(angle-(110+GreatbladeProgressionRules.MasteryArcWidth(rank)))<.1f,"Cleave outline doesn't match gameplay at rank "+rank);angles.Add(angle);Need(presentation.State=="ArmedAttack","Cleave gesture missing");ScreenCapture.CaptureScreenshot("VisualBaselines/TierTwo/Field/cleave-rank-"+rank+".png");await Task.Delay(35);await Until(()=>!c.Busy,"cleave finish");
            }
            c.ResetForManifestationLoad();Need(c.CastSpinDash(Point(p.transform.position+Vector3.forward*6)),"spin fixture");await Task.Delay(60);Need(presentation.State=="Spin Dash","Spin animation missing");ScreenCapture.CaptureScreenshot("VisualBaselines/TierTwo/Field/greatblade-spin.png");await Task.Delay(35);await Until(()=>!c.Busy,"spin finish");await Task.Delay(150);
            // Exercise original fallback motion on the session override only. Assets remain untouched.
            var session=model.runtimeAnimatorController as AnimatorOverrideController;if(session!=null){var pairs=new List<KeyValuePair<AnimationClip,AnimationClip>>();session.GetOverrides(pairs);for(int i=0;i<pairs.Count;i++)pairs[i]=new KeyValuePair<AnimationClip,AnimationClip>(pairs[i].Key,pairs[i].Key);session.ApplyOverrides(pairs);typeof(PlayerMotionPresentation).GetField("licensedSpin",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(presentation,false);}
            await Task.Delay(100);var rotation=root.localRotation;c.ResetForManifestationLoad();Need(c.CastSpinDash(Point(p.transform.position+Vector3.back*6)),"portable spin fixture");await Task.Delay(100);Need(Quaternion.Angle(root.localRotation,rotation)>5,"Fallback spin did not rotate visual");c.CancelCast();await Until(()=>presentation.State!="Spin Dash","spin cosmetic cancel");await Task.Delay(30);Need(Quaternion.Angle(root.localRotation,rotation)<.1f,"Spin visual rotation leaked after cancel");portableSpin=true;
        }
        else
        {
            p.GetComponent<MageSkillCaster>().ResetForManifestationLoad();p.GetComponent<ActorResourceController>().RefillAll();Need(p.GetComponent<MageSkillCaster>().Cast(M.MathBarrier),"barrier fixture");await Task.Delay(60);Need(presentation.State=="Guard","Mage barrier gesture missing");
        }
        ScreenCapture.CaptureScreenshot("VisualBaselines/TierTwo/Field/"+kind+"-skill.png");await Task.Delay(90);p.Effects.TryApplyStun(.15f,true);await Task.Delay(50);Need(model.speed==0,"Stun pause");await Task.Delay(180);Need(model.speed>0,"Stun recovery");
        p.Health.Invulnerable=false;p.Health.ApplyPacket(null,new DamageResult{mitigated=1000000});await Task.Delay(1250);Need(presentation.State=="Death"&&root.localScale==scale,"Tier II death/squash");Need(model.GetBoneTransform(HumanBodyBones.Hips).position.y>p.transform.position.y+.15f,"Tier II corpse sunk below ground");var returnPoint=p.GetComponent<RespawnAtAnchor>();await Until(()=>returnPoint.CanReturn,"death return");Need(returnPoint.Return()&&p.Alive,"actual anchor return");await Task.Delay(90);Need(presentation.State!="Death","Corpse animation didn't recover");
        motion.Add(new{classId=p.Stats.Definition.stableId,walk=true,skillGesture=true,stunRecovery=true,deathAndActualReturn=true,scalePreserved=true,cleaveAngles=angles.ToArray(),portableSpinCancel=portableSpin});Save();
    }
    static Vector3 Point(Vector3 point){Need(NavMesh.SamplePosition(point,out var h,2,NavMesh.AllAreas),"No NavMesh "+point);return h.position;}
    static async Task Until(Func<bool> condition,string detail){var end=DateTime.UtcNow.AddSeconds(40);while(!condition()){Need(DateTime.UtcNow<end,"Timed out: "+detail);await Task.Delay(20);}}
    static void Need(bool ok,string detail){if(!ok)throw new InvalidOperationException(detail);}
}
