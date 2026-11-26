using System;
using System.Linq;
using DiceFree.Combat;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DiceFree.Dungeons
{
    public sealed class CrimsonCrucibleRun : StoryDungeonRun
    {
        public static CrimsonCrucibleRun Current {get;private set;}
        public int Stage {get;private set;}
        public CrucibleArena Arena {get;private set;}
        public CrucibleBoss Opponent=>Arena!=null&&Stage<6?Arena.bosses[Stage]:null;
        protected override string DefinitionId=>CrimsonCruciblePortal.DungeonId;
        protected override string ArenaScene=>CrimsonCruciblePortal.SceneName;
        protected override string Title=>"Crimson Crucible";
        protected override Vector3 ArenaArrival=>CrimsonCruciblePortal.Origin+new Vector3(0,0,-12);
        protected override Rect ArenaBounds=>CrimsonCruciblePortal.Bounds;
        protected override DungeonRewardPool RewardPool=>Arena?.rewards;
        protected override int BonusXp=>4000;
        protected override int BonusGold=>180;
        protected override string ActiveStatus=>Opponent==null?"Six cells conquered.":$"Cell {Stage+1}/6 · {Opponent.Name}\n"+(Opponent.Armed?Opponent.Mechanic:"The gate is open. Enter the next cell to challenge its guardian.");
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]static void Reset()=>Current=null;
        public static string Enter(CombatActor actor)
        {
            if(Current!=null)return Current.Join(actor,actor.transform.position,CrimsonCruciblePortal.Origin+Vector3.right*160);
            var go=new GameObject("Crimson Crucible session");SceneManager.MoveGameObjectToScene(go,actor.gameObject.scene);var run=go.AddComponent<CrimsonCrucibleRun>();
            string error=run.Join(actor,actor.transform.position,CrimsonCruciblePortal.Origin+Vector3.right*160);
            if(error==null)Current=run;else Destroy(go);return error;
        }
        protected override void SetupArena(Scene scene)
        {
            Arena=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<CrucibleArena>(true)).Single();
            if(Arena.bosses.Length!=6||Arena.gates.Length!=5||Arena.rewards==null)throw new InvalidOperationException("Incomplete Crucible cells or treasure.");
            Stage=0;foreach(var boss in Arena.bosses)boss.gameObject.SetActive(false);foreach(var gate in Arena.gates)gate.SetOpen(false);
        }
        protected override void TickEncounter()
        {
            if(Stage>=6)return;var boss=Opponent;
            if(!boss.Armed)
            {
                if(Participants.Any(p=>p!=null&&p.Alive&&CrucibleArena.InCell(p.transform.position,Stage)))
                {boss.gameObject.SetActive(true);boss.Arm(Participants);}
                return;
            }
            if(boss.Actor.Alive)return;
            if(Stage==2&&!CreditBristlebreaker())return;
            boss.Cease();
            if(Stage<5){Arena.gates[Stage].SetOpen(true);Stage++;}
            else{Stage=6;Complete("boss.crucible.crimson-sovereign","six-cell-gauntlet",12000,650);}
        }
        public bool CanLeave(CombatActor actor)=>Phase==RunPhase.Active&&IsParticipant(actor);
        private bool CreditBristlebreaker()
        {
            var before=new System.Collections.Generic.Dictionary<CombatActor,DiceFree.Quests.QuestProgress[]>();
            foreach(var actor in Participants)
                if(actor!=null&&actor.Alive&&actor.Stats.Definition.stableId=="class.greatblade"&&actor.Stats.Level>=60&&
                    CrucibleArena.InCell(actor.transform.position,2)&&
                    actor.GetComponent<DiceFree.Quests.QuestJournal>()?.GetProgress(DiceFree.Advancement.BerserkerMentor.QuestId)?.status==DiceFree.Quests.QuestStatus.Active)
                    before.Add(actor,actor.GetComponent<DiceFree.Quests.QuestJournal>().CaptureState());
            PublishBossKill(DiceFree.Advancement.BerserkerMentor.BossId,before.ContainsKey);
            bool saved=true;
            foreach(var pair in before)
                if(pair.Key.GetComponent<DiceFree.Persistence.ManifestationPersistence>()?.Flush()!=true)
                {
                    pair.Key.GetComponent<DiceFree.Quests.QuestJournal>().RestoreState(pair.Value);saved=false;
                    Debug.LogWarning("Bristlebreaker quest credit could not be saved; retrying before opening the next cell.");
                }
            return saved;
        }
        protected override void StopEncounter(){if(Arena!=null)foreach(var boss in Arena.bosses)if(boss!=null){boss.Cease();boss.gameObject.SetActive(false);}}
        protected override void OnDestroy(){if(Current==this)Current=null;base.OnDestroy();}
    }
}
