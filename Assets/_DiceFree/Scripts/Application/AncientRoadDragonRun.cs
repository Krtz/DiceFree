using System;
using System.Linq;
using DiceFree.Combat;
using DiceFree.Gameplay;
using DiceFree.World;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DiceFree.Dungeons
{
    public sealed class AncientRoadDragonRun : StoryDungeonRun
    {
        public static AncientRoadDragonRun Current { get; private set; }
        public AncientRoadDragonLairPortal Entrance { get; private set; }
        private AncientRoadDragonEncounter dragon;

        protected override string DefinitionId => AncientRoadDragonLairPortal.DungeonId;
        protected override string ArenaScene => AncientRoadDragonLairPortal.SceneName;
        protected override string Title => "Ancient Road Dragon's Lair";
        protected override Vector3 ArenaArrival => AncientRoadDragonLairPortal.Origin + AncientRoadDragonLairPortal.ArrivalOffset;
        protected override Rect ArenaBounds => AncientRoadDragonLairPortal.MapBounds;
        protected override DungeonRewardPool RewardPool => dragon?.Rewards;
        protected override int BonusXp => 2400;
        protected override int BonusGold => 190;
        protected override string ActiveStatus => dragon?.Caption ?? "Follow the old road to the dragon's roost.";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset() => Current = null;

        public static string Enter(AncientRoadDragonLairPortal entrance, CombatActor actor)
        {
            var stagingOrigin = AncientRoadDragonLairPortal.Origin + Vector3.right * 160;
            if (Current != null) return Current.Join(actor, actor.transform.position, stagingOrigin);
            var go = new GameObject("Ancient Road Dragon session");
            SceneManager.MoveGameObjectToScene(go, actor.gameObject.scene);
            var run = go.AddComponent<AncientRoadDragonRun>();
            run.Entrance = entrance;
            var error = run.Join(actor, actor.transform.position, stagingOrigin);
            if (error == null) Current = run;
            else Destroy(go);
            return error;
        }

        protected override void SetupArena(Scene scene)
        {
            dragon = scene.GetRootGameObjects().Select(root => root.GetComponentInChildren<AncientRoadDragonEncounter>())
                .FirstOrDefault(boss => boss != null);
            if (dragon == null || dragon.Rewards == null)
                throw new InvalidOperationException("The lair is missing the dragon encounter or its rewards.");
            dragon.Arm(Participants, OnDragonDefeated, Witness);
        }

        public bool CanLeave(CombatActor actor) => Phase == RunPhase.Active && IsParticipant(actor);
        private void Witness(string mechanic) => Publish(new BossMechanicWitnessedEvent(AncientRoadDragonLairPortal.BossId, mechanic));
        public void OnDragonDefeated() => Complete(AncientRoadDragonLairPortal.BossId, "normal", 6500, 480);
        protected override void StopEncounter() { if (dragon != null) dragon.Cease(); }

        protected override void OnDestroy()
        {
            if (Current == this) Current = null;
            base.OnDestroy();
        }
    }
}
