using System;
using System.Linq;
using DiceFree.Combat;
using DiceFree.Quests;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DiceFree.Dungeons
{
    public sealed class ShamanDungeonRun : StoryDungeonRun
    {
        public const string QuestId = "quest.briarback.four-elements-hunt";
        public const string DungeonId = "dungeon.fourfold-hollow";
        public const string SceneName = "FourfoldHollow";
        public static readonly Vector3 Origin = new(50000, 0, 50000);

        public static ShamanDungeonRun Current { get; private set; }
        public CombatActor Player => Initiator;
        public CombatActor Opponent => arena != null && Stage >= 0 && Stage < arena.opponents.Length
            ? arena.opponents[Stage] : null;
        public int Stage { get; private set; } = -1;
        public bool CanBegin => Phase == RunPhase.Active && Stage < 0;

        private ShamanDungeonArena arena;
        private float nextWardAt;
        private bool awakening;

        protected override string DefinitionId => DungeonId;
        protected override string ArenaScene => SceneName;
        protected override string Title => "Fourfold Hollow";
        protected override Vector3 ArenaArrival => arena.arrival.position;
        protected override Rect ArenaBounds => new(49966, 49965, 68, 76);
        protected override DungeonRewardPool RewardPool => arena?.rewards;
        protected override int BonusXp => 3500;
        protected override int BonusGold => 220;
        protected override string ActiveStatus => Stage < 0 ? "Touch the central altar to challenge the four wardens."
            : Stage >= 4 ? "Final rite · Defeat the Hollow Hierophant"
            : $"Ward {Stage + 1} of 4 · {(awakening ? "Awakening " : "Defeat ")}{Opponent.DisplayName}";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset() => Current = null;

        public static string Enter(CombatActor actor, Vector3 entrance, Vector3 outward)
        {
            if (actor == null || !actor.Alive) return "Only a living player can enter the hollow.";
            if (actor.GetComponent<QuestJournal>()?.GetProgress(QuestId)?.status != QuestStatus.Completed)
                return "Turn in The Four Elements Hunt to the elemental scholar in Trimery.";

            if (Current != null) return Current.Join(actor, entrance + outward * 2.3f, Origin + Vector3.right * 160);
            var go = new GameObject("Fourfold Hollow session");
            SceneManager.MoveGameObjectToScene(go, actor.gameObject.scene);
            var run = go.AddComponent<ShamanDungeonRun>();
            var error = run.Join(actor, entrance + outward * 2.3f, Origin + Vector3.right * 160);
            if (error == null) Current = run;
            else Destroy(go);
            return error;
        }

        protected override void SetupArena(Scene scene)
        {
            arena = scene.GetRootGameObjects().Select(root => root.GetComponent<ShamanDungeonArena>())
                .Single(component => component != null);
            if (arena.opponents.Length != 5 || arena.braziers.Length != 4 || arena.rewards == null || arena.arrival == null)
                throw new InvalidOperationException("The ritual chamber is missing its encounter or arrival.");
        }

        public bool CanUseAltar(CombatActor actor) => CanBegin && IsParticipant(actor);
        public bool CanLeave(CombatActor actor) => Phase == RunPhase.Active && IsParticipant(actor);

        public void BeginRitual() => BeginRitual(Player);

        public void BeginRitual(CombatActor actor)
        {
            if (!CanUseAltar(actor) || !actor.Alive) return;
            if (arena.startPrompt != null) arena.startPrompt.SetActive(false);
            PrepareNext(1.5f);
        }

        private void PrepareNext(float delay)
        {
            Stage++;
            awakening = true;
            nextWardAt = Time.time + delay;
        }

        protected override void TickEncounter()
        {
            if (Stage < 0) return;
            if (awakening)
            {
                if (Time.time < nextWardAt) return;
                awakening = false;
                arena.Awaken(Stage);
            }
            else if (!Opponent.Alive)
            {
                if (Stage < arena.braziers.Length)
                {
                    arena.Extinguish(Stage);
                    PrepareNext(3);
                }
                else Complete(Opponent.Stats.Definition.stableId, "ritual");
            }
        }

        protected override void StopEncounter()
        {
            if (Opponent != null && Opponent.gameObject.activeSelf) Opponent.gameObject.SetActive(false);
        }

        protected override void OnDestroy()
        {
            if (Current == this) Current = null;
            base.OnDestroy();
        }
    }
}
