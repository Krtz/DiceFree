using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using DiceFree.AI;
using DiceFree.Briarback;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Core;
using DiceFree.Items;
using DiceFree.Persistence;
using DiceFree.Progression;
using DiceFree.Skills;
using DiceFree.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using Object=UnityEngine.Object;

// Bounded normal-damage observation: dodge, pressure, retreat. Never a solo-boss victory claim.
public static class BriarbackEquippedBossReview
{
    static readonly List<object> rows=new();static string phase,failure;
    public static string Begin(){_=Observe();return "Normal equipped boss observations started; poll Logs/BriarbackEquippedBossReview.json.";}
    static void Save()=>File.WriteAllText("Logs/BriarbackEquippedBossReview.json",Newtonsoft.Json.JsonConvert.SerializeObject(new{phase,failure,normalTimeScale=Time.timeScale==1,invulnerable=false,limitation="Early classes at 40 in existing starter/Slime gear. Normal-damage dodge/retreat observations, not Tier II/party balance or boss defeats.",setupFixtures="Disposable profile; XP, item grants, starting position and between-trial restores. AI and damage unmodified in encounters.",trials=rows.ToArray()},Newtonsoft.Json.Formatting.Indented));
    static async Task Observe(){try{await Run();phase="passed";Save();}catch(Exception e){failure=e.ToString();phase="failed";Save();Debug.LogException(e);}}
    static async Task Run()
    {
        phase="setup";Save();var p=Object.FindAnyObjectByType<TraversalInput>().GetComponent<CombatActor>();Need(p.GetComponent<ManifestationPersistence>().Ready,"Player not ready");
        p.GetComponent<TraversalInput>().enabled=false;p.GetComponent<CombatInput>().enabled=false;
        var save=p.GetComponent<ManifestationPersistence>();var xp=p.GetComponent<ExperienceProgression>();var basic=p.GetComponent<BasicAttack>();var inventory=p.GetComponent<CarriedInventory>();var equipment=p.GetComponent<Equipment>();
        var chief=Object.FindAnyObjectByType<BriarbackChieftainEncounter>();var boss=chief.GetComponent<CombatActor>();var home=boss.GetComponent<AggroBehaviour>().Home;var camera=Object.FindAnyObjectByType<ExplorationCamera>();
        Directory.CreateDirectory("VisualBaselines/CombatFeedback/BossAssessment");
        for(int kind=0;kind<3;kind++)
        {
            string id=new[]{"class.novice","class.physically-blessed-novice","class.magically-touched-novice"}[kind];string error="";
            if(kind>0){Need(save.TryActivateExisting("class.novice",out error),error);var d=AssetDatabase.FindAssets("t:ActorDefinition",new[]{"Assets/_DiceFree/Settings"}).Select(AssetDatabase.GUIDToAssetPath).Select(AssetDatabase.LoadAssetAtPath<ActorDefinition>).Single(d=>d.stableId==id);Need(save.TryForkAndActivate(d,out error),error);}
            while(xp.Level<40)xp.Grant(xp.RequiredXp);Allocate(p,kind);
            foreach(var slot in equipment.Slots)equipment.Unequip(slot.slotId);
            var items=new List<string>{"item.bronze-dagger","item.cornberg.work-gloves","item.cornberg.farmers-pants","item.cornberg.forest-shoes","item.slime.slimy-tophat"};if(kind>0)items.Add(kind==1?"item.slime.slime-shield":"item.slime.slime-orb");
            foreach(string itemId in items){var d=inventory.Resolve(itemId);var item=inventory.Items.FirstOrDefault(i=>i.definitionId==itemId)??FixedItemGrant.Grant(inventory,d);Need(equipment.Equip(item.instanceId,d.slot),"Equip "+itemId);}
            foreach(var a in new[]{boss,chief.eliteWarrior,chief.eliteBrute}){a.GetComponent<AggroBehaviour>().ResetEncounter();a.Health.Restore();}
            p.Health.Restore();p.GetComponent<ActorResourceController>().RefillAll();Need(Time.timeScale==1&&!p.Health.Invulnerable,"Combat authority changed");
            float min=p.Health.Current,incoming=0,outgoing=0,max=p.Health.Maximum;int hits=0,casts=0,dodged=0,observed=0;bool guards=false,readout=false;var attackers=new HashSet<string>();
            Action<CombatActor,DamageResult> receive=(s,r)=>{incoming+=r.applied;hits++;min=Mathf.Min(min,p.Health.Current);if(s!=null)attackers.Add(s.name);};
            Action<CombatActor,DamageResult> deal=(s,r)=>{if(s==p)outgoing+=r.applied;};p.Health.Damaged+=receive;boss.Health.Damaged+=deal;
            float start=Time.realtimeSinceStartup;bool wasWinding=false;int smashAttempts=chief.SmashAttempts,smashImpacts=chief.SmashImpacts;bool survived=false;
            phase=id;Save();
            try
            {
                Need(p.Motor.Teleport(Point(home+Vector3.forward*5)),"Boss initial placement");camera.RecenterForTravel(p.transform);Need(basic.Order(boss),"Attack boss");
                while(p.Alive&&Time.realtimeSinceStartup-start<18&&p.Health.Current>max*.68f&&observed<2)
                {
                    guards|=chief.eliteWarrior.GetComponent<BasicAttack>().Target==p&&chief.eliteBrute.GetComponent<BasicAttack>().Target==p;
                    if(chief.Winding&&!wasWinding)
                    {
                        wasWinding=true;smashAttempts=chief.SmashAttempts;smashImpacts=chief.SmashImpacts;basic.Cancel();
                        var forward=Vector3.ProjectOnPlane(p.transform.position-boss.transform.position,Vector3.up).normalized;
                        var side=Vector3.Cross(Vector3.up,forward)*4;var point=p.transform.position+side;
                        if(point.x>292||point.x<272)point=p.transform.position-side;
                        Need(p.Motor.MoveTo(Point(point)),"Real dodge path");
                    }
                    if(chief.Winding)
                    {
                        var hud=Object.FindAnyObjectByType<EncounterHud>();
                        if(!readout&&hud.Visible&&chief.CastRemaining>0){readout=true;ScreenCapture.CaptureScreenshot("VisualBaselines/CombatFeedback/BossAssessment/"+kind+"-normal-smash.png");}
                    }
                    else
                    {
                        if(wasWinding){wasWinding=false;if(chief.SmashAttempts>smashAttempts){observed++;if(chief.SmashImpacts==smashImpacts)dodged++;}}
                        casts+=Cast(p,boss,kind);if(p.CanAct&&basic.Target==null)basic.Order(boss);
                    }
                    await Task.Delay(30);
                }
                basic.Cancel();
                if(p.Alive){Need(p.Motor.MoveTo(Point(new Vector3(294,5.2f,460))),"Boss real retreat path");await Until(()=>!p.Alive||Vector3.Distance(p.transform.position,home)>27,"retreat",25);}
                survived=p.Alive;
            }
            finally{basic.Cancel();p.Health.Damaged-=receive;boss.Health.Damaged-=deal;}
            float seconds=Time.realtimeSinceStartup-start;
            Need(p.Motor.Teleport(Point(new Vector3(270,0,390))),"Safe cleanup placement");p.Motor.Stop();
            await Until(()=>!new[]{boss,chief.eliteWarrior,chief.eliteBrute}.Any(a=>a.GetComponent<BasicAttack>().Target==p),"Encounter disengagement",45);
            rows.Add(new{classId=id,level=40,gear=items.ToArray(),maximumHp=max,minimumHp=min,incomingHits=hits,incomingDamage=incoming,bossDamage=outgoing,casts,observedSmashes=observed,dodgedSmashes=dodged,guardsEngaged=guards,readoutVisibleDuringRealWindup=readout,survivedRetreat=survived,elapsedIncludingRetreat=seconds,attackers=attackers.OrderBy(s=>s).ToArray()});Save();Need(readout&&guards,"Actual encounter/readout engagement not observed");p.Health.Restore();await Task.Delay(150);
        }
    }
    static void Allocate(CombatActor p,int kind){bool spent;do{spent=false;if(kind==0){var s=p.GetComponent<NoviceSkillProgression>();foreach(var d in s.Definitions)spent|=s.Spend(d.stableId);}if(kind==1){var s=p.GetComponent<PhysicalSkillProgression>();foreach(var d in s.Definitions)spent|=s.Spend(d);}if(kind==2){var s=p.GetComponent<MagicalSkillProgression>();foreach(var d in s.Definitions)spent|=s.Spend(d);}}while(spent);}
    static int Cast(CombatActor p,CombatActor target,int kind)
    {
        int count=0;
        if(kind==0)foreach(var d in p.GetComponent<NoviceSkillProgression>().Definitions.Where(d=>d.Active)){bool heal=d.kind==NoviceSkillKind.SpiritHeal;if(heal&&p.Health.Current>p.Health.Maximum*.65f)continue;bool support=heal||d.kind==NoviceSkillKind.AgilityAttackSpeedBuff;if(p.GetComponent<NoviceSkillCaster>().Cast(d,support?p:target))count++;}
        if(kind==1)foreach(var d in p.GetComponent<PhysicalSkillProgression>().Definitions.Where(d=>d.Active)){var c=p.GetComponent<PhysicalSkillCaster>();if(d.kind==PhysicalSkillKind.ArrowRain?c.CastArrowRain(d,target.transform.position):c.Cast(d,target))count++;}
        if(kind==2)foreach(var d in p.GetComponent<MagicalSkillProgression>().Definitions.Where(d=>d.Active)){bool heal=d.kind==MagicalSkillKind.Mend;if(heal&&p.Health.Current>p.Health.Maximum*.65f)continue;var c=p.GetComponent<MagicalSkillCaster>();if(d.kind==MagicalSkillKind.IceBurst?c.CastIceBurst(d,target.transform.position):c.Cast(d,d.kind==MagicalSkillKind.MagicSand?target:p))count++;}
        return count;
    }
    static Vector3 Point(Vector3 p){Need(NavMesh.SamplePosition(p,out var h,2,NavMesh.AllAreas),"No NavMesh "+p);return h.position;}
    static async Task Until(Func<bool> c,string detail,int seconds){var end=DateTime.UtcNow.AddSeconds(seconds);while(!c()){Need(DateTime.UtcNow<end,"Timed out "+detail);await Task.Delay(25);}}
    static void Need(bool ok,string detail){if(!ok)throw new InvalidOperationException(detail);}
}
