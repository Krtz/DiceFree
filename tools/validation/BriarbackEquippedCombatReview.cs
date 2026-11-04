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
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using Object=UnityEngine.Object;

// A repeatable assessment, not automated claims of class/party balance approval.
// Uses real AI, time, equipment, damage, cooldowns and ordinary skill points.
// Position, XP, item grants and between-trial restores are isolated setup fixtures.
public static class BriarbackEquippedCombatReview
{
    static readonly List<object> rows=new();
    static string phase,failure;
    public static string Begin(){_=Observe();return "Equipped review started; poll Logs/BriarbackEquippedCombatReview.json.";}
    static void Save()=>File.WriteAllText("Logs/BriarbackEquippedCombatReview.json",Newtonsoft.Json.JsonConvert.SerializeObject(new{phase,failure,normalTimeScale=Time.timeScale==1,invulnerable=false,setupFixtures="Disposable profile, XP/items, initial placement and between-trial health/resource restore. No fixture damage or disabled AI in combat.",limitation="Three early classes with existing starter/Slime gear; one deterministic skill policy, one camp, no party or Tier II balance approval.",trials=rows.ToArray()},Newtonsoft.Json.Formatting.Indented));
    static async Task Observe(){try{await Run();phase="passed";Save();}catch(Exception e){failure=e.ToString();phase="failed";Save();Debug.LogException(e);}}
    static async Task Run()
    {
        phase="setup";Save();
        var p=Object.FindAnyObjectByType<TraversalInput>().GetComponent<CombatActor>();
        Need(p.GetComponent<ManifestationPersistence>().Ready,"Player not ready");
        p.GetComponent<TraversalInput>().enabled=false;p.GetComponent<CombatInput>().enabled=false;
        var xp=p.GetComponent<ExperienceProgression>();var save=p.GetComponent<ManifestationPersistence>();var basic=p.GetComponent<BasicAttack>();
        var inventory=p.GetComponent<CarriedInventory>();var equipment=p.GetComponent<Equipment>();
        var raider=Object.FindObjectsByType<QuilboarCombat>().Single(q=>q.role==QuilboarRole.Raider&&q.GetComponent<ActorStats>().Level==25).GetComponent<CombatActor>();
        var home=raider.GetComponent<AggroBehaviour>().Home;
        var camera=Object.FindAnyObjectByType<ExplorationCamera>();
        Directory.CreateDirectory("VisualBaselines/CombatFeedback/Assessment");
        for(int kind=0;kind<3;kind++)
        {
            string id=new[]{"class.novice","class.physically-blessed-novice","class.magically-touched-novice"}[kind];
            string error="";
            if(kind>0){Need(save.TryActivateExisting("class.novice",out error),error);var d=AssetDatabase.FindAssets("t:ActorDefinition",new[]{"Assets/_DiceFree/Settings"}).Select(AssetDatabase.GUIDToAssetPath).Select(AssetDatabase.LoadAssetAtPath<ActorDefinition>).Single(a=>a.stableId==id);Need(save.TryForkAndActivate(d,out error),error);}
            foreach(int level in new[]{25,30,35})
            {
                Need(Time.timeScale==1&&!p.Health.Invulnerable,"Combat authority changed");
                while(xp.Level<level)xp.Grant(xp.RequiredXp);
                Allocate(p,kind);
                foreach(var slot in equipment.Slots)equipment.Unequip(slot.slotId);
                var items=new List<string>{"item.bronze-dagger","item.cornberg.work-gloves","item.cornberg.farmers-pants","item.cornberg.forest-shoes","item.slime.slimy-tophat"};
                if(kind>0)items.Add(kind==1?"item.slime.slime-shield":"item.slime.slime-orb");
                foreach(string itemId in items){var d=inventory.Resolve(itemId);Need(d!=null,"Unknown gear "+itemId);var item=inventory.Items.FirstOrDefault(i=>i.definitionId==itemId)??FixedItemGrant.Grant(inventory,d);Need(equipment.Equip(item.instanceId,d.slot),"Equip "+itemId);}
                foreach(var q in Object.FindObjectsByType<QuilboarCombat>().Where(q=>Vector3.Distance(q.GetComponent<AggroBehaviour>().Home,home)<12))q.GetComponent<Health>().Restore();
                p.Health.Restore();p.GetComponent<ActorResourceController>().RefillAll();
                int incoming=0,outgoing=0,casts=0;float received=0,dealt=0,minHp=p.Health.Current;var attackers=new HashSet<string>();
                Action<CombatActor,DamageResult> onIncoming=(s,r)=>{incoming++;received+=r.applied;minHp=Mathf.Min(minHp,p.Health.Current);if(s!=null)attackers.Add(s.name);};
                Action<CombatActor,DamageResult> onOutgoing=(s,r)=>{if(s==p){outgoing++;dealt+=r.applied;}};
                p.Health.Damaged+=onIncoming;foreach(var a in CombatActor.All)if(a!=p)a.Health.Damaged+=onOutgoing;
                float start=Time.realtimeSinceStartup;float enemyStart=raider.Health.Current;float maxHp=p.Health.Maximum;
                try
                {
                    Need(p.Motor.Teleport(Point(home+Vector3.back*4)),"Combat placement");Need(basic.Order(raider),"Attack order");camera.RecenterForTravel(p.transform);
                    phase=id+"-level-"+level;Save();
                    bool captured=false;
                    while(p.Alive&&raider.Alive&&Time.realtimeSinceStartup-start<30&&p.Health.Current>maxHp*.30f)
                    {
                        casts+=Cast(p,raider,kind);
                        if(p.CanAct&&basic.Target==null&&raider.Alive)basic.Order(raider);
                        if(!captured&&Time.realtimeSinceStartup-start>4){ScreenCapture.CaptureScreenshot("VisualBaselines/CombatFeedback/Assessment/"+kind+"-"+level+".png");captured=true;}
                        await Task.Delay(100);
                    }
                }
                finally{basic.Cancel();p.Health.Damaged-=onIncoming;foreach(var a in CombatActor.All)if(a!=p)a.Health.Damaged-=onOutgoing;}
                float seconds=Time.realtimeSinceStartup-start;
                string outcome=!p.Alive?"player-defeated":!raider.Alive?"raider-defeated":p.Health.Current<=maxHp*.30f?"retreat-health-threshold":"retreat-time-limit";
                bool survivedRetreat=p.Alive;
                if(p.Alive)
                {
                    Need(p.Motor.MoveTo(Point(home+Vector3.back*34)),"Retreat path");
                    await Until(()=>!p.Alive||Vector3.Distance(p.transform.position,home)>28,"retreat",20);survivedRetreat=p.Alive;
                }
                Need(p.Motor.Teleport(Point(new Vector3(270,0,390))),"Safe fixture reset");p.Motor.Stop();
                await Until(()=>!CombatActor.All.Any(a=>a!=p&&a.Alive&&a.GetComponent<BasicAttack>()?.Target==p),"enemy return",45);
                rows.Add(new{classId=id,levelStart=level,levelEnd=xp.Level,gear=items.ToArray(),maximumHp=maxHp,minimumHp=minHp,incomingHits=incoming,incomingDamage=received,outgoingHits=outgoing,outgoingDamage=dealt,casts,combatSeconds=seconds,targetStartHp=enemyStart,targetRemainingHp=raider.Health.Current,outcome,survivedRetreat,attackers=attackers.OrderBy(a=>a).ToArray()});Save();
                p.Health.Restore();await Task.Delay(500);
            }
        }
    }
    static void Allocate(CombatActor p,int kind)
    {
        bool spent;
        do{spent=false;if(kind==0){var s=p.GetComponent<NoviceSkillProgression>();foreach(var d in s.Definitions)spent|=s.Spend(d.stableId);}if(kind==1){var s=p.GetComponent<PhysicalSkillProgression>();foreach(var d in s.Definitions)spent|=s.Spend(d);}if(kind==2){var s=p.GetComponent<MagicalSkillProgression>();foreach(var d in s.Definitions)spent|=s.Spend(d);}}while(spent);
    }
    static int Cast(CombatActor p,CombatActor target,int kind)
    {
        int count=0;
        if(kind==0)foreach(var d in p.GetComponent<NoviceSkillProgression>().Definitions.Where(d=>d.Active)){bool heal=d.kind==NoviceSkillKind.SpiritHeal;if(heal&&p.Health.Current>p.Health.Maximum*.65f)continue;bool support=heal||d.kind==NoviceSkillKind.AgilityAttackSpeedBuff;if(p.GetComponent<NoviceSkillCaster>().Cast(d,support?p:target))count++;}
        if(kind==1)foreach(var d in p.GetComponent<PhysicalSkillProgression>().Definitions.Where(d=>d.Active)){var c=p.GetComponent<PhysicalSkillCaster>();if(d.kind==PhysicalSkillKind.ArrowRain?c.CastArrowRain(d,target.transform.position):c.Cast(d,target))count++;}
        if(kind==2)foreach(var d in p.GetComponent<MagicalSkillProgression>().Definitions.Where(d=>d.Active)){bool heal=d.kind==MagicalSkillKind.Mend;if(heal&&p.Health.Current>p.Health.Maximum*.65f)continue;var c=p.GetComponent<MagicalSkillCaster>();if(d.kind==MagicalSkillKind.IceBurst?c.CastIceBurst(d,target.transform.position):c.Cast(d,d.kind==MagicalSkillKind.MagicSand?target:p))count++;}
        return count;
    }
    static Vector3 Point(Vector3 p){Need(NavMesh.SamplePosition(p,out var h,2,NavMesh.AllAreas),"No NavMesh "+p);return h.position;}
    static async Task Until(Func<bool> c,string detail,int seconds){var end=DateTime.UtcNow.AddSeconds(seconds);while(!c()){Need(DateTime.UtcNow<end,"Timed out: "+detail);await Task.Delay(30);}}
    static void Need(bool ok,string detail){if(!ok)throw new InvalidOperationException(detail);}
}
