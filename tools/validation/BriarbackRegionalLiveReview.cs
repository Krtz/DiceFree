using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using DiceFree.AI;
using DiceFree.Briarback;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Core;
using DiceFree.Persistence;
using DiceFree.Progression;
using DiceFree.Skills;
using DiceFree.World;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using Object=UnityEngine.Object;

// Disposable profile. Real combat first; separately labelled scripted deaths exercise real 1x timers.
public static class BriarbackRegionalLiveReview
{
    [Serializable] public class Report
    {
        public string phase, failure, limitation="Single unarmed Novice baseline, not final class or party balance approval.";
        public bool passed, invulnerable, enemyAiUnmodified=true, normalTimeScale=true, retreatReset, guardsEngaged, smashDodged, pipelineClone, sourcePipelineUnchanged, qualityPersistence, invalidQualityFallback, regionalBlend, interiorRestoration, outdoorRestoration;
        public int level, incomingHits, outgoingHits, casts, presets;
        public float maximumHp, minimumHp, incomingDamage, outgoingDamage, combatSeconds, regionalFogEnd;
        public string combatOutcome;
        public List<TimerRow> respawns=new();
    }
    [Serializable] public class TimerRow { public string actor;public float expectedSeconds, observedSeconds, killedAt;public bool passed; }
    static Report report;
    public static string Begin(){_ = Observe();return "Started isolated regional review; poll Logs/BriarbackRegionalLiveReview.json.";}
    static async Task Observe(){try{await Run();}catch(Exception error){if(report!=null){report.failure=error.ToString();Save("failed");}Debug.LogException(error);}}
    static void Save(string phase)
    {
        report.phase=phase;Directory.CreateDirectory("Logs");string json=JsonUtility.ToJson(report,true);
        // The Editor dynamic compiler can omit a List of its dynamically emitted nested type.
        if(!json.Contains("\"respawns\""))json=json.TrimEnd().TrimEnd('}')+",\n\"respawns\":["+string.Join(",",report.respawns.Select(r=>JsonUtility.ToJson(r)))+"]\n}";
        File.WriteAllText("Logs/BriarbackRegionalLiveReview.json",json);
    }
    public static async Task<object> Run()
    {
        report=new Report();Save("waiting-for-player");
        CombatActor player=null;
        await Until(()=>(player=Object.FindAnyObjectByType<TraversalInput>()?.GetComponent<CombatActor>())!=null&&player.Motor.Ready&&player.GetComponent<ManifestationPersistence>().Ready,"player");
        Need(Time.timeScale==1,"Expected normal time scale");
        player.GetComponent<TraversalInput>().enabled=false;player.GetComponent<CombatInput>().enabled=false;
        Need(!player.Health.Invulnerable,"Normal combat must take damage");report.invulnerable=player.Health.Invulnerable;
        var xp=player.GetComponent<ExperienceProgression>();while(xp.Level<24)xp.Grant(xp.RequiredXp);
        var skills=player.GetComponent<NoviceSkillProgression>();var caster=player.GetComponent<NoviceSkillCaster>();
        foreach(var definition in skills.Definitions)
        {
            int ranks=definition.kind==NoviceSkillKind.AllStatPassive?8:definition.kind==NoviceSkillKind.StrengthMeleeStun?6:definition.kind==NoviceSkillKind.MagicSand?4:3;
            for(int i=0;i<ranks;i++)Need(skills.Spend(definition.stableId),"Allocate ordinary Novice points");
        }
        player.Health.Restore();report.level=xp.Level;report.maximumHp=player.Health.Maximum;report.minimumHp=player.Health.Current;
        var basic=player.GetComponent<BasicAttack>();
        var raider=Object.FindObjectsByType<QuilboarCombat>().Where(q=>q.role==QuilboarRole.Raider).OrderBy(q=>q.GetComponent<ActorStats>().Level).First().GetComponent<CombatActor>();
        Action<CombatActor,DamageResult> incoming=(s,r)=>{report.incomingHits++;report.incomingDamage+=r.applied;report.minimumHp=Mathf.Min(report.minimumHp,player.Health.Current);};
        Action<CombatActor,DamageResult> outgoing=(s,r)=>{if(s==player){report.outgoingHits++;report.outgoingDamage+=r.applied;}};
        player.Health.Damaged+=incoming;raider.Health.Damaged+=outgoing;
        float start=Time.realtimeSinceStartup;
        try
        {
            Need(player.Motor.Teleport(Point(raider.transform.position+Vector3.back*4)),"raider setup");Need(basic.Order(raider),"ordinary attack");Save("real-combat-level-24");
            while(player.Alive&&raider.Alive&&Time.realtimeSinceStartup-start<24&&player.Health.Current>player.Health.Maximum*.45f)
            {
                foreach(var d in skills.Definitions.Where(d=>d.Active))
                {
                    bool heal=d.kind==NoviceSkillKind.SpiritHeal;
                    if(heal&&player.Health.Current>player.Health.Maximum*.65f)continue;
                    bool support=heal||d.kind==NoviceSkillKind.AgilityAttackSpeedBuff;
                    if(caster.Cast(d,support?player:raider))report.casts++;
                }
                if(basic.Target==null&&raider.Alive&&player.Alive)basic.Order(raider);
                await Task.Delay(100);
            }
            report.combatSeconds=Time.realtimeSinceStartup-start;
            report.combatOutcome=!player.Alive?"Novice defeated":!raider.Alive?"Raider defeated":"Retreated at safety threshold / 24 seconds";
        }
        finally{basic.Cancel();player.Health.Damaged-=incoming;raider.Health.Damaged-=outgoing;}
        Need(report.incomingHits>0&&report.outgoingHits>0,"No real two-way combat packets");
        if(!player.Alive)throw new InvalidOperationException("Baseline player died before retreat assessment; see recorded combat result.");
        var aggro=raider.GetComponent<AggroBehaviour>();
        Need(player.Motor.MoveTo(Point(aggro.Home+Vector3.back*(aggro.Leash+10))),"real retreat path");
        await Until(()=>Vector3.Distance(player.transform.position,aggro.Home)>aggro.Leash+2,"retreat beyond leash");
        await Until(()=>!aggro.Returning&&raider.GetComponent<BasicAttack>().Target==null&&Vector3.Distance(raider.transform.position,aggro.Home)<.6f,"ordinary return home",50);
        Need(Mathf.Abs(raider.Health.Current-raider.Health.Maximum)<.01f,"return must heal");report.retreatReset=true;
        player.Motor.Stop();while(xp.Level<40)xp.Grant(xp.RequiredXp);player.Health.Restore();
        var chief=Object.FindAnyObjectByType<BriarbackChieftainEncounter>();var boss=chief.GetComponent<CombatActor>();
        Need(player.Motor.Teleport(Point(boss.transform.position+Vector3.forward*4)) ,"boss setup");
        Save("real-boss-guards-and-dodge");
        await Until(()=>chief.Winding,"natural boss smash");
        report.guardsEngaged=chief.eliteWarrior.GetComponent<BasicAttack>().Target==player&&chief.eliteBrute.GetComponent<BasicAttack>().Target==player;
        int hits=chief.SmashImpacts,attempts=chief.SmashAttempts;
        var forward=Vector3.ProjectOnPlane(player.transform.position-boss.transform.position,Vector3.up).normalized;
        Need(player.Motor.MoveTo(Point(player.transform.position+Vector3.Cross(Vector3.up,forward)*4)),"strafe path");
        await Until(()=>chief.SmashAttempts>attempts,"normal smash resolves");report.smashDodged=chief.SmashImpacts==hits&&player.Alive;
        var bossAggro=boss.GetComponent<AggroBehaviour>();
        // Real return route down the authored ascent, with all enemies still active.
        Need(player.Motor.MoveTo(Point(new Vector3(294,5.2f,460))),"boss retreat");
        await Until(()=>!player.Alive||(!bossAggro.Returning&&boss.GetComponent<BasicAttack>().Target==null&&Vector3.Distance(boss.transform.position,bossAggro.Home)<.6f),"boss return",55);
        Need(player.Alive,"Baseline Novice died while retreating from boss");
        Need(report.guardsEngaged&&report.smashDodged,"Guard engagement or dodge failed");
        player.GetComponent<BasicAttack>().Cancel();player.Motor.Stop();Need(player.Motor.Teleport(Point(new Vector3(270,0,390))),"safe timer observation position");player.Health.Restore();
        // These scripted deaths are lifecycle fixtures, explicitly not combat victories.
        var harrow=Object.FindObjectsByType<QuilboarCombat>().Single(q=>q.role==QuilboarRole.Harrowtusk).GetComponent<CombatActor>();
        var timerActors=new[]{boss,chief.eliteWarrior,chief.eliteBrute,harrow,raider};
        foreach(var a in timerActors)
        {
            if(!a.Alive)a.Health.Restore();
            var spawn=a.GetComponent<OverworldRespawn>();Need(spawn!=null,"Missing respawn fixture");
            a.Health.ApplyPacket(null,new DamageResult{mitigated=a.Health.Maximum+1});Need(!a.Alive,"Fixture death");
            report.respawns.Add(new TimerRow{actor=a.name,expectedSeconds=spawn.Definition.delaySeconds,killedAt=Time.realtimeSinceStartup});
        }
        Save("real-time-respawn-fixtures-running");
        await Visuals(player);
        while(report.respawns.Any(r=>!r.passed))
        {
            Need(Time.timeScale==1,"Timer test was accelerated");report.normalTimeScale&=Time.timeScale==1;
            for(int i=0;i<timerActors.Length;i++)
            {
                var row=report.respawns[i];if(row.passed)continue;var a=timerActors[i];float elapsed=Time.realtimeSinceStartup-row.killedAt;
                Need(elapsed<row.expectedSeconds+35,"Respawn timed out: "+a.name);
                if(!a.Alive){Need(elapsed<row.expectedSeconds+2,"Late respawn: "+a.name);continue;}
                row.observedSeconds=elapsed;Need(elapsed>=row.expectedSeconds-.1f,"Early respawn: "+a.name);
                Need(a.Health.Current==a.Health.Maximum&&a.GetComponent<BasicAttack>().Target==null&&!a.GetComponent<AggroBehaviour>().Returning,"Respawn state: "+a.name);
                Need(Vector3.Distance(a.transform.position,a.GetComponent<AggroBehaviour>().Home)<.6f,"Respawn location: "+a.name);
                var presentation=a.GetComponent<QuilboarPresentation>();await Task.Delay(50);Need(presentation.PresentationState!="Death"&&presentation.model.speed>0,"Animation failed to recover after respawn");row.passed=true;
            }
            Save("waiting-full-1x-respawn-timers");await Task.Delay(100);
        }
        report.passed=true;Save("passed");return report;
    }
    static async Task Visuals(CombatActor player)
    {
        Save("graphics-and-atmosphere");
        var clock=Object.FindAnyObjectByType<WorldDayNightCycle>();var atmosphere=Object.FindAnyObjectByType<BriarbackRegionalAtmosphere>();var camera=Object.FindAnyObjectByType<ExplorationCamera>();
        var previous=GraphicsQualityRuntime.Preset;string oldRoot=Environment.GetEnvironmentVariable("DICEFREE_EDITOR_SETTINGS_ROOT");
        var shared=UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline;string sharedBefore=EditorJsonUtility.ToJson(shared);
        var assigned=QualitySettings.renderPipeline;Need(assigned==GraphicsQualityRuntime.Pipeline&&assigned!=shared,"Quality must use a session copy");report.pipelineClone=true;
        float hour=clock.Hour;bool running=new SerializedObject(clock).FindProperty("running").boolValue;
        string output="VisualBaselines/BriarbackRegion/Live";Directory.CreateDirectory(output);
        var fog=Object.FindAnyObjectByType<WorldFogOfWar>();var overlayField=typeof(WorldFogOfWar).GetField("overlayRenderer",BindingFlags.Instance|BindingFlags.NonPublic);var overlay=overlayField?.GetValue(fog) as Renderer;bool overlayEnabled=overlay!=null&&overlay.enabled;
        try
        {
            Environment.SetEnvironmentVariable("DICEFREE_EDITOR_SETTINGS_ROOT",Path.Combine(Path.GetTempPath(),"DiceFree-QualityReview-"+Guid.NewGuid()));
            clock.SetHour(12,false);camera.RecenterForTravel(player.transform);await Task.Delay(250);
            report.regionalBlend=atmosphere.Weight>.99f&&BriarbackRegionalAtmosphere.Blend(new Vector3(248,0,201))==0&&BriarbackRegionalAtmosphere.Blend(new Vector3(248,0,230))>0&&BriarbackRegionalAtmosphere.Blend(new Vector3(248,0,230))<1;
            report.regionalFogEnd=RenderSettings.fogEndDistance;Need(report.regionalBlend&&Mathf.Abs(report.regionalFogEnd-230)<.1f,"Regional lighting blend");
            float outsideIntensity=clock.CelestialLight.intensity;clock.SetInterior(true);await Task.Delay(80);report.interiorRestoration=atmosphere.Weight==0&&!clock.MoonActive;
            clock.SetInterior(false);await Task.Delay(80);report.outdoorRestoration=atmosphere.Weight>.99f&&Mathf.Abs(outsideIntensity-clock.CelestialLight.intensity)<.001f;
            if(overlay!=null)overlay.enabled=false;
            foreach(VisualQuality preset in Enum.GetValues(typeof(VisualQuality)))
            {
                GraphicsQualityRuntime.Select(preset,true);GraphicsQualityRuntime.Reload();Need(GraphicsQualityRuntime.Preset==preset,"Persist/reload preset");
                var pipeline=GraphicsQualityRuntime.Pipeline;int index=(int)preset;
                Need(Mathf.Abs(pipeline.renderScale-new[]{.8f,1,1,1.15f}[index])<.001f&&pipeline.mainLightShadowmapResolution==new[]{512,1024,2048,4096}[index]&&pipeline.msaaSampleCount==new[]{1,2,2,4}[index],"Preset values");
                var warning=BriarbackChieftainEffects.Warning(player.transform.position+new Vector3(-2,.09f,0),Vector3.forward);
                await Capture(output+"/quality-"+preset.ToString().ToLowerInvariant()+".png");Object.Destroy(warning);report.presets++;
            }
            report.qualityPersistence=true;
            File.WriteAllText(GraphicsQualityRuntime.SettingsPath,"{\"version\":1,\"preset\":99}");GraphicsQualityRuntime.Reload();Need(GraphicsQualityRuntime.Preset==VisualQuality.High,"Unknown preset fallback");
            File.WriteAllText(GraphicsQualityRuntime.SettingsPath,"invalid-json");GraphicsQualityRuntime.Reload();report.invalidQualityFallback=GraphicsQualityRuntime.Preset==VisualQuality.High;
            foreach(var h in new[]{12f,18f,0f}){clock.SetHour(h,false);await Capture(output+"/hour-"+h+".png");}
            report.sourcePipelineUnchanged=EditorJsonUtility.ToJson(shared)==sharedBefore;Need(report.sourcePipelineUnchanged&&report.interiorRestoration&&report.outdoorRestoration&&report.invalidQualityFallback,"Graphics/lighting restoration");
        }
        finally{if(overlay!=null)overlay.enabled=overlayEnabled;Environment.SetEnvironmentVariable("DICEFREE_EDITOR_SETTINGS_ROOT",oldRoot);GraphicsQualityRuntime.Select(previous,false);clock.SetInterior(false);clock.SetHour(hour,running);camera.ClearPresentationPose();}
    }
    static async Task Capture(string path){await Task.Delay(250);ScreenCapture.CaptureScreenshot(path);await Task.Delay(100);}
    static Vector3 Point(Vector3 p){Need(NavMesh.SamplePosition(p,out var hit,2,NavMesh.AllAreas),"No NavMesh "+p);return hit.position;}
    static async Task Until(Func<bool> check,string detail,int seconds=30){var end=DateTime.UtcNow.AddSeconds(seconds);while(!check()){Need(DateTime.UtcNow<end,"Timed out: "+detail);await Task.Delay(20);}}
    static void Need(bool condition,string detail){if(!condition){if(report!=null){report.failure=detail;Save("failed");}throw new InvalidOperationException(detail);}}
}
