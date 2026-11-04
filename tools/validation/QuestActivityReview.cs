using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Items;
using DiceFree.Persistence;
using DiceFree.Progression;
using DiceFree.Quests;
using DiceFree.UI;
using UnityEngine;
using Object=UnityEngine.Object;

public static class QuestActivityReview
{
    public static async Task<string> Run()
    {
        CombatActor p=null;var end=DateTime.UtcNow.AddSeconds(30);
        while((p=Object.FindAnyObjectByType<TraversalInput>()?.GetComponent<CombatActor>())==null||!p.GetComponent<ManifestationPersistence>().Ready||p.GetComponent<QuestActivityFeed>()==null)
        {Need(DateTime.UtcNow<end,"Player/feed not ready");await Task.Delay(20);}
        p.GetComponent<TraversalInput>().enabled=false;p.GetComponent<CombatInput>().enabled=false;
        var journal=p.GetComponent<QuestJournal>();var feed=p.GetComponent<QuestActivityFeed>();var wallet=p.GetComponent<GoldWallet>();var xp=p.GetComponent<ExperienceProgression>();
        var bounty=journal.Definitions.Single(q=>q.stableId=="quest.briarback.repeat-bounty");
        int start=feed.PublishedMessages;Need(journal.Accept(bounty),"Bounty acceptance");Need(feed.PublishedMessages==start+1,"Accepted quest should produce one message");Need(!journal.Accept(bounty)&&feed.PublishedMessages==start+1,"Rejected acceptance replayed feedback");
        var state=journal.CaptureState();var record=state.Single(r=>r.questId==bounty.stableId);record.status=QuestStatus.ReadyToTurnIn;record.stage=bounty.stages.Length-1;record.count=bounty.stages.Last().count;
        journal.RestoreState(state);Need(feed.PublishedMessages==start+1,"Restored history replayed gameplay feedback");
        int level=xp.Level,currentXp=xp.CurrentXp;long gold=wallet.Gold;bool injected=false;
        Action failOnce=()=>{if(!injected){injected=true;throw new InvalidOperationException("Expected QA reward observer failure");}};
        wallet.Changed+=failOnce;bool rolledBack=false;
        try{journal.TurnIn(bounty);}catch(InvalidOperationException error){Need(error.Message.Contains("Expected QA"),"Unexpected reward failure");rolledBack=true;}
        finally{wallet.Changed-=failOnce;}
        Need(rolledBack&&injected,"Reward fault injection did not execute");
        var restored=journal.GetProgress(bounty.stableId);Need(restored.status==QuestStatus.ReadyToTurnIn&&restored.count==20&&restored.stage==0,"Repeat reward rollback lost ready counters");
        Need(wallet.Gold==gold&&xp.Level==level&&xp.CurrentXp==currentXp,"Failed reward leaked XP/gold");Need(feed.PublishedMessages==start+1,"Failed reward reported completion");
        Need(journal.TurnIn(bounty),"Retry turn-in rejected");Need(feed.PublishedMessages==start+2,"Successful repeat turn-in should publish exactly once");Need(!journal.TurnIn(bounty)&&feed.PublishedMessages==start+2,"Duplicate turn-in replayed feedback");
        Need(journal.GetProgress(bounty.stableId).status==QuestStatus.Available&&journal.GetProgress(bounty.stableId).count==0,"Successful repeat did not reset");
        journal.RestoreState(journal.CaptureState());Need(feed.PublishedMessages==start+2,"Reload replayed completed/available history");
        var result=new{passed=true,acceptedMessage=true,noRejectedReplay=true,noRestoreReplay=true,failedRewardObserverInjected=true,entireReadyRecordRestored=true,rewardsRolledBack=true,noFalseCompletionMessage=true,retryExactlyOnce=true,repeatCount=20,fixtureQuestProgress=true};
        string json=Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented);File.WriteAllText("Logs/QuestActivityReview.json",json);return json;
    }
    static void Need(bool ok,string detail){if(!ok)throw new InvalidOperationException(detail);}
}
