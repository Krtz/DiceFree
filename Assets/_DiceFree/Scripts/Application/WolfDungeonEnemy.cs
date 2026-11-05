using System.Linq;
using DiceFree.Combat;
using DiceFree.Ironpine;
using UnityEngine;
using UnityEngine.AI;
namespace DiceFree.Dungeons
{
    public sealed class WolfDungeonEnemy : MonoBehaviour
    {
        public WolfDungeonRun Run {get;private set;}
        public WolfRank Rank {get;private set;}
        public CombatActor Actor {get;private set;}
        IronpineWildlife wildlife;float nextTarget,flankEnds;int flankSide;
        CombatActor opponent;bool attempted;Vector3? approach;
        public string Tactic {get;private set;}="Closing";
        public int FlankAttempts {get;private set;}
        public int FlanksCompleted {get;private set;}
        public Vector3? ApproachPoint=>approach;
        public void Initialize(WolfDungeonRun run,WolfRank rank,int ordinal=0)
        { Run=run;Rank=rank;flankSide=ordinal%2==0?-1:1;Actor=GetComponent<CombatActor>();wildlife=GetComponent<IronpineWildlife>();wildlife.ConfigureEncounter(); }
        void Update()
        {
            if(Run==null||!Actor.Alive||!Run.Fighting)return;
            if(Time.time<nextTarget)return;nextTarget=Time.time+.25f;
            var target=Run.Living.OrderBy(a=>(a.transform.position-transform.position).sqrMagnitude).FirstOrDefault();
            if(target==null){wildlife.SetEncounterApproach(null);return;}
            if(target!=opponent){opponent=target;attempted=false;approach=null;}
            wildlife.EngageEncounter(target);
            float distance=Vector3.Distance(transform.position,target.transform.position);
            if(Rank!=WolfRank.Strong){Tactic=Rank==WolfRank.Alpha?"Alpha pressure":"Direct pressure";wildlife.SetEncounterApproach(null);return;}
            if(!attempted&&distance<=14)
            {
                attempted=true;
                if(distance>6)
                {
                    var forward=Vector3.ProjectOnPlane(target.transform.position-transform.position,Vector3.up).normalized;
                    var side=Vector3.Cross(Vector3.up,forward)*flankSide;
                    var desired=target.transform.position+side*5+forward*1.5f;
                    var path=new NavMeshPath();
                    if(NavMesh.SamplePosition(desired,out var hit,1.25f,-1)&&NavMesh.CalculatePath(transform.position,hit.position,-1,path)&&path.status==NavMeshPathStatus.PathComplete)
                    {approach=hit.position;flankEnds=Time.time+3.5f;FlankAttempts++;}
                }
            }
            if(approach.HasValue)
            {
                bool arrived=Vector3.Distance(transform.position,approach.Value)<1.2f;
                if(arrived||Time.time>=flankEnds||distance<4){if(arrived)FlanksCompleted++;approach=null;}
            }
            Tactic=approach.HasValue?"Flanking":attempted?"Committed":"Closing";
            wildlife.SetEncounterApproach(approach);
        }
    }
}
