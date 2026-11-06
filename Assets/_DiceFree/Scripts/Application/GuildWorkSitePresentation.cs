using DiceFree.Combat;
using DiceFree.Quests;
using UnityEngine;

namespace DiceFree.World
{
    public sealed class GuildWorkSitePresentation : MonoBehaviour
    {
        [SerializeField] CombatActor player;
        [SerializeField] QuestWorkGate gate;
        [SerializeField] GameObject untouched,finished,beacon;
        void LateUpdate()
        {
            var journal=player!=null?player.GetComponent<QuestJournal>():null;
            bool complete=journal!=null&&gate.Completed(journal);
            if(untouched!=null)untouched.SetActive(!complete);
            if(finished!=null)finished.SetActive(complete);
            if(beacon!=null)beacon.SetActive(player!=null&&gate.WorkAvailable(player));
            if(beacon!=null&&beacon.activeSelf){beacon.transform.localRotation=Quaternion.Euler(0,Time.time*45,0);beacon.transform.localPosition=new Vector3(0,1.8f+Mathf.Sin(Time.time*2.5f)*.09f,0);}
        }
        public void Configure(CombatActor actor,QuestWorkGate condition,GameObject before,GameObject after,GameObject marker)
        {player=actor;gate=condition;untouched=before;finished=after;beacon=marker;}
    }
}
