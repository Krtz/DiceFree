using System.Linq;
using DiceFree.Combat;
using DiceFree.Ironpine;
using UnityEngine;
namespace DiceFree.Dungeons
{
    public sealed class WolfDungeonEnemy : MonoBehaviour
    {
        public WolfDungeonRun Run {get;private set;}
        public WolfRank Rank {get;private set;}
        public CombatActor Actor {get;private set;}
        IronpineWildlife wildlife;float nextTarget;
        public void Initialize(WolfDungeonRun run,WolfRank rank)
        { Run=run;Rank=rank;Actor=GetComponent<CombatActor>();wildlife=GetComponent<IronpineWildlife>();wildlife.ConfigureEncounter(); }
        void Update()
        {
            if(Run==null||!Actor.Alive||!Run.Fighting)return;
            if(Time.time<nextTarget)return;nextTarget=Time.time+.4f;
            var target=Run.Living.OrderBy(a=>(a.transform.position-transform.position).sqrMagnitude).FirstOrDefault();
            if(target!=null)wildlife.EngageEncounter(target);
        }
    }
}
