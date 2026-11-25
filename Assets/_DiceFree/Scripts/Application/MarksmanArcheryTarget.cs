using System;
using DiceFree.Combat;
using DiceFree.Persistence;
using DiceFree.Quests;
using DiceFree.World;
using UnityEngine;
namespace DiceFree.Advancement
{
    public sealed class MarksmanArcheryTarget : MonoBehaviour,IBasicAttackHitReceiver
    {
        public const string QuestId="quest.marksman.world-tour";
        [SerializeField]string settlementId,settlementName;
        [SerializeField]GameObject visual;
        CombatActor actor,visitor;Collider selection;float next;
        public string SettlementId=>settlementId;
        public string SettlementName=>settlementName;
        public string ContentId=>"target.marksman."+settlementId;
        void Awake(){actor=GetComponent<CombatActor>();selection=GetComponent<Collider>();actor.Health.Invulnerable=true;}
        public bool Eligible(CombatActor source)
        {
            if(source==null||!source.Alive||source.Stats.Definition.stableId!="class.ranger"||source.Stats.Level<60)return false;
            var state=source.GetComponent<QuestJournal>()?.GetProgress(QuestId);
            return state?.status==QuestStatus.Active&&ObjectiveProgress.Count(state,settlementId)==0;
        }
        void Update()
        {
            if(Time.time<next)return;next=Time.time+.15f;
            if(visitor==null)foreach(var candidate in CombatActor.All)if(candidate.GetComponent<ManifestationPersistence>()!=null){visitor=candidate;break;}
            bool show=Eligible(visitor);if(actor.enabled!=show)actor.enabled=show;if(selection!=null)selection.enabled=show;if(visual.activeSelf!=show)visual.SetActive(show);
        }
        // Only BasicAttack's successful bow impact calls this, never AoE/spell packets.
        public void OnBasicAttackHit(CombatActor source,DamageResult packet)=>ReceiveBowHit(source,packet);
        internal bool ReceiveBowHit(CombatActor source,DamageResult packet)
        {
            var bow=source!=null?source.GetComponent<BasicAttack>():null;
            if(!Eligible(source)||packet.Missed||bow?.Definition==null||bow.Definition.reach<3)return false;
            var journal=source.GetComponent<QuestJournal>();var before=journal.CaptureState();journal.CreditInteraction(ContentId);
            if(ObjectiveProgress.Count(journal.GetProgress(QuestId),settlementId)!=1)return false;
            if(source.GetComponent<ManifestationPersistence>()?.Flush()==true)return true;
            journal.RestoreState(before);return false;
        }
        public void Configure(string id,string title,GameObject model){settlementId=id;settlementName=title;visual=model;}
    }
}
