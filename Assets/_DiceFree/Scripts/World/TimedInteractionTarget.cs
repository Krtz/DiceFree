using System;
using DiceFree.Combat;
using UnityEngine;

namespace DiceFree.World
{
    public interface IWorkAvailability { bool WorkAvailable(CombatActor actor); }
    public readonly struct InteractionCompleted
    {
        public readonly CombatActor actor;
        public readonly string contentId;
        public InteractionCompleted(CombatActor actor,string id){this.actor=actor;contentId=id;}
    }
    public static class InteractionEvents
    {
        public static event Action<InteractionCompleted> Completed;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()=>Completed=null;
        internal static void Report(CombatActor actor,string id)=>Completed?.Invoke(new InteractionCompleted(actor,id));
    }

    /// <summary>Interruptible world work. Consumers decide what its completion means.</summary>
    public sealed class TimedInteractionTarget : InteractionTarget
    {
        [SerializeField] string contentId,verb="Working";
        [SerializeField,Min(.1f)] float duration=3;
        CombatActor worker;
        Interactor interactor;
        Vector3 origin;
        float started;
        public string ContentId=>contentId;
        public string Verb=>verb;
        public float Duration=>duration;
        public CombatActor Worker=>worker;
        public float Progress=>worker==null?0:Mathf.Clamp01((Time.time-started)/duration);
        public override bool Available(CombatActor actor)
        {
            if(!base.Available(actor)||(worker!=null&&worker!=actor))return false;
            foreach(var component in GetComponents<MonoBehaviour>())
                if(component is IWorkAvailability gate&&!gate.WorkAvailable(actor))return false;
            return true;
        }
        public override void Interact(CombatActor actor)
        {
            var input=actor!=null?actor.GetComponent<Interactor>():null;
            if(!CanInteract(actor)||!actor.CanAct||input?.Active!=this||worker!=null)return;
            worker=actor;interactor=input;origin=actor.transform.position;started=Time.time;
            worker.Health.Damaged+=Damaged;
        }
        void Damaged(CombatActor source,DamageResult result){if(result.applied>0)CancelWork();}
        public void CancelWork()
        {
            if(worker!=null)worker.Health.Damaged-=Damaged;
            worker=null;interactor=null;
        }
        protected override void OnDisable(){CancelWork();base.OnDisable();}
        void Update()
        {
            if(worker==null)return;
            if(!worker.Alive||!worker.CanAct||interactor==null||interactor.Active!=this||!CanInteract(worker)||
                worker.Motor.Travelling||(worker.transform.position-origin).sqrMagnitude>.0625f||
                worker.GetComponent<BasicAttack>()?.Target!=null){CancelWork();return;}
            if(Progress<1)return;
            var actor=worker;var input=interactor;
            CancelWork();input.Cancel(); // Clear ownership before callbacks can start another interaction.
            InteractionEvents.Report(actor,contentId);
        }
        public void Configure(string id,string action,float seconds){contentId=id;verb=action;duration=Mathf.Max(.1f,seconds);}
    }
}
