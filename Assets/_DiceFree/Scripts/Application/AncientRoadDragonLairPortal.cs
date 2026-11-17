using DiceFree.Combat;
using DiceFree.World;
using UnityEngine;

namespace DiceFree.Dungeons
{
    public sealed class AncientRoadDragonLairPortal : InteractionTarget
    {
        public const string SceneName = "AncientRoadDragonLair";
        public const string DungeonId = "dungeon.ancient-road-dragon";
        public const string BossId = "boss.ancient-road-dragon";
        public const string QuestId = "quest.arborium.ancient-road-dragon";
        public static readonly Vector3 Origin = new(56000, 0, 56000);
        public static readonly Vector3 ArrivalOffset = new(0, 0, -27);
        public static readonly Rect MapBounds = new(55955, 55955, 90, 94);

        [SerializeField] private bool exit;
        private string feedback;
        private float feedbackUntil;

        public static AncientRoadDragonLairPortal Current => AncientRoadDragonRun.Current?.Entrance;
        public CombatActor Player => AncientRoadDragonRun.Current?.Initiator;

        public void ConfigureAsExit() => exit = true;

        public override bool Available(CombatActor actor) =>
            base.Available(actor) && (!exit || AncientRoadDragonRun.Current?.CanLeave(actor) == true);

        public override void Interact(CombatActor actor)
        {
            if (!CanInteract(actor)) return;
            if (exit) AncientRoadDragonRun.Current?.RequestLeave(actor);
            else
            {
                feedback = AncientRoadDragonRun.Enter(this, actor);
                feedbackUntil = Time.unscaledTime + 6;
            }
        }

        private void OnGUI()
        {
            if (!string.IsNullOrEmpty(feedback) && Time.unscaledTime < feedbackUntil)
                GUI.Box(new Rect(15, 185, 550, 31), feedback);
        }

        public static void ReleaseForSessionExit() => AncientRoadDragonRun.Current?.ReleaseForSessionExit();
    }
}
