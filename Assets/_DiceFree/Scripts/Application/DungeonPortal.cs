using DiceFree.Combat;
using DiceFree.World;
using UnityEngine;
namespace DiceFree.Dungeons
{
    public sealed class DungeonPortal : InteractionTarget
    {
        public DungeonDefinition definition;
        public SlimeDungeonTuning tuning;
        public string Feedback {get;private set;}="";
        float feedbackUntil;
        public override void Interact(CombatActor actor)
        {
            if (!CanInteract(actor)) return;
            var run=SlimeDungeonRun.Current;
            if(run==null) run=new GameObject("Host dungeon session").AddComponent<SlimeDungeonRun>();
            Feedback=run.Enter(actor,definition,tuning,ApproachPosition);
            feedbackUntil=Time.unscaledTime+5;
        }
        void OnGUI(){if(Time.unscaledTime<feedbackUntil)GUI.Box(new Rect(15,150,420,30),Feedback);}
    }
}
