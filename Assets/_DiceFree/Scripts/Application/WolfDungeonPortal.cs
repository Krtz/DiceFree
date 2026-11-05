using DiceFree.Combat;
using DiceFree.World;
using UnityEngine;
namespace DiceFree.Dungeons
{
    public sealed class WolfDungeonPortal : InteractionTarget
    {
        public WolfDungeonTuning tuning;
        public string Feedback {get;private set;}
        float feedbackUntil;
        public override void Interact(CombatActor actor)
        {
            if(!CanInteract(actor)||tuning==null)return;
            var run=WolfDungeonRun.Current;
            if(run==null)run=new GameObject("Wolf Survival session").AddComponent<WolfDungeonRun>();
            Feedback=run.Enter(actor,tuning,ApproachPosition);feedbackUntil=Time.unscaledTime+5;
            if(run.Participants.Count==0)Destroy(run.gameObject);
        }
        void OnGUI(){if(Time.unscaledTime<feedbackUntil&&!string.IsNullOrEmpty(Feedback))GUI.Box(new Rect(15,186,510,30),Feedback);}
    }
}
