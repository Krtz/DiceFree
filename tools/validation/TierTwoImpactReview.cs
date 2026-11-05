using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Development;
using DiceFree.Progression;
using DiceFree.UI;
using UnityEngine;

// Isolated Development Room pose/timing fixture, not a regional combat victory.
public static class TierTwoImpactReview
{
    public static async Task<string> Run()
    {
        var dev=DevelopmentRoomController.Current;if(dev==null)throw new InvalidOperationException("Development Room required.");var p=dev.player;var rows=new List<object>();Animator oldGreatblade=null;
        foreach(string id in new[]{"class.greatblade","class.mage"})
        {
            if(!dev.SelectClass(id))throw new InvalidOperationException("Class fixture rejected.");var motion=p.GetComponent<PlayerMotionPresentation>();await Until(()=>motion.ControllerClassId==id,"controller");
            p.GetComponent<TraversalInput>().enabled=false;p.GetComponent<CombatInput>().enabled=false;p.Health.Invulnerable=true;
            var model=p.GetComponent<NovicePresentationDriver>().VisualAnimator;if(id=="class.greatblade")oldGreatblade=model;
            if(id=="class.mage"&&oldGreatblade.runtimeAnimatorController is AnimatorOverrideController)throw new InvalidOperationException("Session override was not restored on class exit.");
            var target=CombatActor.All.First(a=>a.Stats.Definition?.stableId=="dev.practice-target");target.Stats.SetEncounterMaximumHp(1000000);target.Health.Restore();
            if(!p.Motor.Teleport(new Vector3(0,0,-8))||!target.Motor.Teleport(new Vector3(0,0,-5.8f)))throw new InvalidOperationException("Timing placement.");await Task.Delay(120);
            var basic=p.GetComponent<BasicAttack>();float normalized=-1;string state=null;Action<CombatActor,DamageResult> connected=(t,r)=>{if(t==target){normalized=model.GetCurrentAnimatorStateInfo(0).normalizedTime%1;state=motion.State;}};
            basic.Connected+=connected;
            try{if(!basic.Order(target))throw new InvalidOperationException("Basic order.");await Until(()=>normalized>=0,"actual basic impact");}
            finally{basic.Cancel();basic.Connected-=connected;}
            float expected=id=="class.greatblade"?.52f:.48f/.85f;if(Mathf.Abs(normalized-expected)>.2f)throw new InvalidOperationException("Attack pose doesn't follow impact: "+id+" normalized="+normalized);
            if(state!=(id=="class.greatblade"?"ArmedAttack":"Cast"))throw new InvalidOperationException("Wrong basic gesture.");
            rows.Add(new{classId=id,atRealBasicImpactNormalized=normalized,chosenImpactNormalized=expected,state});
        }
        var result=Newtonsoft.Json.JsonConvert.SerializeObject(new{passed=true,classes=rows.ToArray(),greatbladeOverrideRestored=true,damageTimingUnchanged=true},Newtonsoft.Json.Formatting.Indented);File.WriteAllText("Logs/TierTwoImpactReview.json",result);return result;
    }
    static async Task Until(Func<bool> f,string detail){var end=DateTime.UtcNow.AddSeconds(25);while(!f()){if(DateTime.UtcNow>end)throw new InvalidOperationException("Timed out "+detail);await Task.Delay(10);}}
}
