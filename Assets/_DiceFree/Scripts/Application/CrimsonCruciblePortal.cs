using DiceFree.Combat;
using DiceFree.World;
using UnityEngine;

namespace DiceFree.Dungeons
{
    public sealed class CrimsonCruciblePortal : InteractionTarget
    {
        public const string SceneName="CrimsonCrucible",DungeonId="dungeon.crimson-crucible";
        public static readonly Vector3 Origin=new(72000,0,72000);
        public static readonly Rect Bounds=new(71976,71976,48,288);
        [SerializeField] bool exit;
        string feedback;float feedbackUntil;
        public void ConfigureAsExit()=>exit=true;
        public override bool Available(CombatActor actor)=>base.Available(actor)&&(!exit||CrimsonCrucibleRun.Current?.CanLeave(actor)==true);
        public override void Interact(CombatActor actor)
        {
            if(!CanInteract(actor))return;
            if(exit)CrimsonCrucibleRun.Current?.RequestLeave(actor);
            else{feedback=CrimsonCrucibleRun.Enter(actor);feedbackUntil=Time.unscaledTime+6;}
        }
        void OnGUI(){if(!string.IsNullOrEmpty(feedback)&&Time.unscaledTime<feedbackUntil)GUI.Box(new Rect(15,185,550,31),feedback);}
    }
}
