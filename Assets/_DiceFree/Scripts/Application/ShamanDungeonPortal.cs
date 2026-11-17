using DiceFree.Combat;
using DiceFree.World;
using UnityEngine;

namespace DiceFree.Dungeons
{
    public sealed class ShamanDungeonPortal : InteractionTarget
    {
        public enum Action { Enter, BeginRitual, Leave }
        public Action action;
        public string Feedback { get; private set; }
        private float feedbackUntil;

        public override bool Available(CombatActor actor)
        {
            if (!base.Available(actor)) return false;
            var run = ShamanDungeonRun.Current;
            return action switch
            {
                Action.Enter => true,
                Action.BeginRitual => run != null && run.CanUseAltar(actor),
                _ => run != null && run.CanLeave(actor)
            };
        }

        public override void Interact(CombatActor actor)
        {
            if (!CanInteract(actor)) return;
            switch (action)
            {
                case Action.Enter:
                    Feedback = ShamanDungeonRun.Enter(actor, ApproachPosition, transform.forward);
                    feedbackUntil = Time.unscaledTime + 6;
                    break;
                case Action.BeginRitual:
                    ShamanDungeonRun.Current.BeginRitual(actor);
                    break;
                case Action.Leave:
                    ShamanDungeonRun.Current.RequestLeave(actor);
                    break;
            }
        }

        private void OnGUI()
        {
            if (!string.IsNullOrEmpty(Feedback) && Time.unscaledTime < feedbackUntil)
                GUI.Box(new Rect(15, 185, 550, 31), Feedback);
        }
    }
}

