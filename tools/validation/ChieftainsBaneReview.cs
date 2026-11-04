using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DiceFree.Briarback;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Persistence;
using DiceFree.Progression;
using DiceFree.Quests;
using DiceFree.Items;
using DiceFree.World;
using UnityEditor;
using UnityEngine;
using Object=UnityEngine.Object;

public static class ChieftainsBaneReview
{
    const string Id="quest.briarback.chieftains-bane";
    public static async Task<object> Run()
    {
        CombatActor p=null;await Until(()=>(p=Object.FindAnyObjectByType<TraversalInput>()?.GetComponent<CombatActor>())!=null&&p.Motor.Ready&&p.GetComponent<ManifestationPersistence>().Ready,"player");
        p.GetComponent<TraversalInput>().enabled=false;p.GetComponent<CombatInput>().enabled=false;p.Health.Invulnerable=true;
        foreach(var b in Object.FindObjectsByType<MonoBehaviour>().Where(b=>b is DiceFree.AI.AggroBehaviour||b is BasicAttack||b is QuilboarCombat||b is BriarbackChieftainEncounter))b.enabled=false;
        var journal=p.GetComponent<QuestJournal>();var quest=journal.Definitions.Single(q=>q.stableId==Id);var elder=Object.FindObjectsByType<QuestGiver>().Single(g=>g.Quest==quest);var boss=Object.FindAnyObjectByType<BriarbackChieftainEncounter>().GetComponent<CombatActor>();var wallet=p.GetComponent<GoldWallet>();var xp=p.GetComponent<ExperienceProgression>();
        Need(elder.GetComponentsInChildren<InteractionTarget>(true).Length==1,"Elder has competing inherited interactions");
        Need(journal.EarnedTitle==null,"Fresh character has title");boss.Health.ApplyPacket(p,new DamageResult{mitigated=1000000});Need(journal.GetProgress(Id).status==QuestStatus.Available&&journal.EarnedTitle==null,"Kill before acceptance counted");boss.Health.Restore();
        Need(p.Motor.Teleport(elder.ApproachPosition+Vector3.forward*4),"town setup");var interaction=p.GetComponent<Interactor>();Need(interaction.Order(elder),"elder walk");await Until(()=>interaction.Active==elder,"elder interaction");Need(elder.Accept(p,quest),"quest acceptance");interaction.Cancel();Need(!elder.TurnIn(p,quest),"early turn-in allowed");
        var wrong=Object.FindObjectsByType<QuilboarCombat>().First(q=>q.role==QuilboarRole.Raider).GetComponent<CombatActor>();wrong.Health.ApplyPacket(p,new DamageResult{mitigated=1000000});Need(journal.GetProgress(Id).status==QuestStatus.Active&&journal.GetProgress(Id).count==0,"Other Quillboar counted as boss");
        Need(p.Motor.Teleport(boss.transform.position+Vector3.forward*3),"boss fixture setup");boss.Health.ApplyPacket(p,new DamageResult{mitigated=1000000});Need(journal.GetProgress(Id).status==QuestStatus.ReadyToTurnIn&&journal.EarnedTitle==null,"Title awarded on kill or no boss credit");
        long gold=wallet.Gold;int level=xp.Level,experience=xp.CurrentXp;Need(!elder.TurnIn(p,quest),"Remote elder turn-in allowed");Need(p.Motor.Teleport(elder.ApproachPosition),"elder return");Need(elder.TurnIn(p,quest),"valid turn-in");Need(journal.GetProgress(Id).status==QuestStatus.Completed&&journal.EarnedTitle=="Chieftain's Bane","Completed title missing");Need(wallet.Gold==gold+quest.reward.gold,"Currency wrong");Need(!elder.TurnIn(p,quest)&&wallet.Gold==gold+quest.reward.gold,"Repeated reward granted");
        var save=p.GetComponent<ManifestationPersistence>();Need(save.DisplayName.EndsWith(" the Chieftain's Bane",StringComparison.Ordinal),"Live title missing");Need(save.Flush(),"save failed");
        var json=File.ReadAllText(save.SavePath);Need(json.Contains(Id),"Completed quest missing from save");Need(QuestHonorifics.Resolve(Array.Empty<QuestProgress>(),"Novice")=="Novice","Title leaked to another character");
        string name=save.DisplayName;string savePath=save.SavePath;Need(p.GetComponent<DiceFree.Dungeons.ReturnToStartMenu>().Return(),"return/save");DiceFree.Advancement.StartMenuController menu=null;await Until(()=>(menu=Object.FindAnyObjectByType<DiceFree.Advancement.StartMenuController>())!=null&&menu.HasLoadedProfile&&!DiceFree.ApplicationFlow.LoadingScreen.Busy,"saved menu");Need(menu.Entries.Single(e=>e.active).displayName==name,"Menu title missing");Need(menu.TryPlayManifestation("class.novice"),"resume");p=null;await Until(()=>(p=Object.FindAnyObjectByType<TraversalInput>()?.GetComponent<CombatActor>())!=null&&p.GetComponent<ManifestationPersistence>().Ready,"resumed character");Need(p.DisplayName==name&&p.GetComponent<QuestJournal>().GetProgress(Id).status==QuestStatus.Completed,"Title/completion did not persist");
        var result=new{passed=true,questId=Id,elderApproachable=true,noCreditBeforeAcceptance=true,chieftainOnly=true,noTitleBeforeTurnIn=true,remoteTurnInRejected=true,exactlyOnceReward=true,completedTitle=name,savePath,menuAndReloadTitle=true,xpReward=quest.rewardXp,goldReward=quest.reward.gold,tuningPlaceholders=true,fixtureDamageIsNotCombatBalance=true};
        Directory.CreateDirectory("Logs");File.WriteAllText("Logs/ChieftainsBaneReview.json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));return result;
    }
    static async Task Until(Func<bool> c,string detail){var end=DateTime.UtcNow.AddSeconds(35);while(!c()){Need(DateTime.UtcNow<end,"Timed out: "+detail);await Task.Delay(20);}}
    static void Need(bool ok,string message){if(!ok)throw new InvalidOperationException(message);}
}
