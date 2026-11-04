using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DiceFree.AI;
using DiceFree.Briarback;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Core;
using DiceFree.Persistence;
using DiceFree.Progression;
using DiceFree.Quests;
using DiceFree.UI;
using DiceFree.World;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using Object=UnityEngine.Object;

// Cosmetic acceptance fixtures; damage/quest state below is explicitly scripted,
// unlike the separate nine real equipped camp encounters.
public static class CombatFeedbackLiveReview
{
    public static async Task<string> Run()
    {
        File.WriteAllText("Logs/CombatFeedbackLiveReview.json","{\"passed\":false,\"phase\":\"running\"}");
        try{return await Review();}
        catch(Exception error){File.WriteAllText("Logs/CombatFeedbackLiveReview.json",Newtonsoft.Json.JsonConvert.SerializeObject(new{passed=false,error=error.ToString()}));throw;}
    }
    static async Task<string> Review()
    {
        CombatActor p=null;await Until(()=>(p=Object.FindAnyObjectByType<TraversalInput>()?.GetComponent<CombatActor>())!=null&&p.GetComponent<ManifestationPersistence>().Ready&&p.Motor.Ready,"player");
        p.GetComponent<TraversalInput>().enabled=false;p.GetComponent<CombatInput>().enabled=false;
        foreach(var b in Object.FindObjectsByType<MonoBehaviour>().Where(b=>b is AggroBehaviour||b is BasicAttack||b is QuilboarCombat||b is BriarbackChieftainEncounter))b.enabled=false;
        foreach(var actor in CombatActor.All)if(actor.Motor.Ready)actor.Motor.Stop();
        var xp=p.GetComponent<ExperienceProgression>();while(xp.Level<30)xp.Grant(xp.RequiredXp);p.Health.Restore();p.Health.Invulnerable=false;
        var camera=Object.FindAnyObjectByType<ExplorationCamera>();
        var nav=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("UnityEditor.AI.NavMeshVisualizationSettings")).First(t=>t!=null).GetProperty("showNavMesh",BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic);var oldNav=nav.GetValue(null);nav.SetValue(null,false);
        var viewType=typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");var view=Resources.FindObjectsOfTypeAll(viewType).FirstOrDefault();var gizmos=viewType.GetProperty("drawGizmos",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);var oldGizmos=view!=null?gizmos.GetValue(view):null;if(view!=null)gizmos.SetValue(view,false);
        string output="VisualBaselines/CombatFeedback/Live";Directory.CreateDirectory(output);
        try
        {
            Need(p.Motor.Teleport(Point(new Vector3(270,0,415))),"Feedback setup");camera.RecenterForTravel(p.transform);
            var target=Object.FindObjectsByType<QuilboarCombat>().First(q=>q.role==QuilboarRole.Raider).GetComponent<CombatActor>();
            Need(target.Motor.Teleport(Point(p.transform.position+Vector3.forward*3)),"Target setup");target.Health.Restore();
            var feedback=p.GetComponent<ActorFeedback>();var targetFeedback=target.GetComponent<ActorFeedback>();var bars=Object.FindAnyObjectByType<OverheadUnitHealthBars>();
            await Task.Delay(150);
            p.Health.ApplyPacket(target,new DamageResult{mitigated=80});await Task.Delay(80);
            foreach(int value in new[]{11,17,23}){target.Health.ApplyPacket(p,new DamageResult{mitigated=value});await Task.Delay(85);}
            Need(targetFeedback.CombatText.Entries.Count>=3,"Rapid separate hits were overwritten");
            p.Health.Heal(18);p.Health.AddTemporaryShield("qa.feedback",80,3);float previous=p.Health.Current;
            var packet=p.Health.ApplyPacket(target,new DamageResult{mitigated=35});
            Need(packet.absorbed==35&&p.Health.Current==previous,"Absorption fixture must leave HP unchanged");
            p.Effects.TryApplyStun(1,true);await Task.Delay(60);
            Need(feedback.CombatText.Entries.Any(e=>e.kind==CombatTextKind.Healing)&&feedback.CombatText.Entries.Any(e=>e.kind==CombatTextKind.Shield),"Heal/absorption presentation missing");
            Need(bars.RecentHealthFraction(target)>=target.Health.Current/target.Health.Maximum,"Invalid recent-damage bar");
            await Capture(output+"/hit-heal-absorb-gameplay.png");
            await Task.Delay(1100);p.Health.Restore();await Task.Delay(30);Need(feedback.CombatText.Entries.Count==0,"Restore didn't clear old feedback");
            var basic=p.GetComponent<BasicAttack>();basic.enabled=true;p.Effects.ApplyAccuracyPenalty("qa.feedback.miss",1,6);int misses=basic.Misses;Need(basic.Order(target),"Miss attack order");
            await Until(()=>basic.Misses>misses,"Actual rolled basic miss");await Task.Delay(30);Need(targetFeedback.CombatText.Entries.Any(e=>e.kind==CombatTextKind.Miss),"Actual miss not presented");await Capture(output+"/actual-basic-miss.png");basic.Cancel();basic.enabled=false;p.Effects.ClearTransient();
            var journal=p.GetComponent<QuestJournal>();var hunt=journal.Definitions.Single(q=>q.stableId=="quest.briarback.four-elements-hunt");var elder=journal.Definitions.Single(q=>q.stableId=="quest.briarback.chieftains-bane");
            var states=journal.CaptureState();var huntState=states.Single(s=>s.questId==hunt.stableId);huntState.status=QuestStatus.Active;huntState.stage=2;huntState.count=0;
            var elderState=states.Single(s=>s.questId==elder.stableId);elderState.status=QuestStatus.ReadyToTurnIn;elderState.count=1;
            journal.RestoreState(states);Need(QuestReadout.Objective(hunt,journal.GetProgress(hunt.stableId)).StartsWith("Step 3 of 4"),"Hunt step readout");Need(QuestReadout.TurnIn(elder).Contains("Trimery Village Elder"),"Named elder return");
            await Capture(output+"/quest-tracker.png");var panel=p.GetComponent<QuestJournalPanel>();panel.Show();await Capture(output+"/quest-journal.png");panel.Close();
            var hud=Object.FindAnyObjectByType<EncounterHud>();Need(!hud.Visible,"Boss HUD appeared out of combat");
            var chief=Object.FindAnyObjectByType<BriarbackChieftainEncounter>();var boss=chief.GetComponent<CombatActor>();
            Need(p.Motor.Teleport(Point(boss.transform.position+Vector3.forward*4.5f)),"Boss HUD setup");p.Health.Invulnerable=true;camera.RecenterForTravel(p.transform);
            chief.enabled=true;boss.GetComponent<BasicAttack>().enabled=true;Need(boss.GetComponent<BasicAttack>().Order(p),"Boss target fixture");
            await Until(()=>chief.Winding&&hud.Visible,"Boss HUD windup");Need(ReferenceEquals(hud.Current,chief)&&chief.CastRemaining>0&&chief.CastRemaining<=1.5f&&chief.LivingGuards==2,"Boss readout disagrees with encounter: same="+ReferenceEquals(hud.Current,chief)+", remaining="+chief.CastRemaining.ToString("R")+", guards="+chief.LivingGuards);
            await Capture(output+"/boss-smash-readout.png");
            chief.enabled=false;boss.GetComponent<BasicAttack>().Cancel();await Task.Delay(50);Need(!hud.Visible,"Disabled boss left stale HUD");
            p.Health.Invulnerable=false;p.Health.ApplyPacket(null,new DamageResult{mitigated=100000});await Task.Delay(50);Need(!hud.Visible,"Death left stale encounter HUD");
            var respawn=p.GetComponent<RespawnAtAnchor>();Need(!respawn.CanReturn&&DeathRespawnHud.HasFallenPlayer,"Death overlay/delay state missing");await Capture(output+"/death-return.png");await Until(()=>respawn.CanReturn,"Normal return delay");Need(respawn.Return()&&p.Alive&&!DeathRespawnHud.HasFallenPlayer,"Actual anchor return failed");await Task.Delay(80);Need(p.GetComponent<PlayerMotionPresentation>().State!="Death","Actual return left corpse animation");
            var result=new{passed=true,separateHits=true,healing=true,absorption=true,realBasicMiss=true,restoreClearsFeedback=true,questStepAndNamedReturn=true,journalChecklist=true,bossHealthAndActualWindup=true,guards=2,disabledEncounterHides=true,deathCleanup=true,actualDelayedAnchorReturn=true,fixtureDamage=true};
            string json=Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented);File.WriteAllText("Logs/CombatFeedbackLiveReview.json",json);return json;
        }
        finally{camera.ClearPresentationPose();nav.SetValue(null,oldNav);if(view!=null&&oldGizmos!=null)gizmos.SetValue(view,oldGizmos);}
    }
    static Vector3 Point(Vector3 p){Need(NavMesh.SamplePosition(p,out var h,2,NavMesh.AllAreas),"No NavMesh "+p);return h.position;}
    static async Task Capture(string path){ScreenCapture.CaptureScreenshot(path);await Task.Delay(80);}
    static async Task Until(Func<bool> c,string detail){var end=DateTime.UtcNow.AddSeconds(30);while(!c()){Need(DateTime.UtcNow<end,"Timed out: "+detail);await Task.Delay(15);}}
    static void Need(bool ok,string detail){if(!ok)throw new InvalidOperationException(detail);}
}
