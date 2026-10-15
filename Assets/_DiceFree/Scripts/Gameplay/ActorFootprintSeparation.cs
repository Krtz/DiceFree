using UnityEngine;
using UnityEngine.AI;

namespace DiceFree.Combat
{
    /// <summary>
    /// NavMesh avoidance is advisory: two units can still interpenetrate after
    /// simultaneous moves, close-range pathing, or manually driven movement.
    /// Resolve the mover's OWN footprint without pushing other characters.
    /// Explicit ghosting effects may bypass unit separation, not terrain.
    /// </summary>
    [DisallowMultipleComponent, DefaultExecutionOrder(10000)]
    [RequireComponent(typeof(CombatActor),typeof(NavMeshAgent))]
    public sealed class ActorFootprintSeparation : MonoBehaviour
    {
        private CombatActor actor;
        private NavMeshAgent agent;
        private Vector3 lastPosition;
        private const float Clearance=.045f;
        public int Corrections { get; private set; }
        private void Awake()
        {
            actor=GetComponent<CombatActor>();
            agent=GetComponent<NavMeshAgent>();
            lastPosition=transform.position;
        }
        private void OnEnable()=>lastPosition=transform.position;
        // A deliberate warp (including a boss's Slime Slam landing) is not
        // locomotion. Never turn its large displacement into a collision push.
        public void RebaseAfterTeleport()=>lastPosition=agent!=null && agent.enabled &&
            agent.isOnNavMesh?agent.nextPosition:transform.position;
        private void LateUpdate()
        {
            if(actor==null || !actor.Alive || actor.Ghosting || agent==null ||
                !agent.enabled || !agent.isOnNavMesh)return;
            Vector3 now=agent.nextPosition;
            Vector3 travelled=Vector3.ProjectOnPlane(now-lastPosition,Vector3.up);
            // Never displace a stationary character: players must block each
            // other, not push one another out of the way.
            bool mover=travelled.sqrMagnitude>.000025f;
            if(!mover){lastPosition=now;return;}
            for(int iteration=0;iteration<4;iteration++)
            {
                Vector3 corrective=Vector3.zero;
                foreach(var other in CombatActor.All)
                {
                    if(other==null || other==actor || !other.Alive ||
                       actor.CanPassThrough(other) || !other.gameObject.activeInHierarchy)continue;
                    Vector3 delta=now-other.transform.position;
                    if(Mathf.Abs(delta.y)>1.3f)continue; // Separate floors.
                    delta.y=0;
                    float minimum=actor.Radius+other.Radius+Clearance;
                    float d=delta.magnitude;
                    if(d>=minimum)continue;
                    Vector3 away=d>.00001f?delta/d:
                        (travelled.sqrMagnitude>.00001f?
                            -travelled.normalized:Vector3.right);
                    corrective+=away*(minimum-d);
                }
                if(corrective.sqrMagnitude<.000001f)break;
                corrective=Vector3.ClampMagnitude(corrective,actor.Radius*.65f);
                // Agent.Move respects the NavMesh edge. Do not warp through
                // trees or fences just to separate two overlapping units.
                agent.Move(corrective);
                now=agent.nextPosition;
                Corrections++;
            }
            lastPosition=now;
        }
    }
}
