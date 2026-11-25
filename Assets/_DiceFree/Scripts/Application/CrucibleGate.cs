using UnityEngine;
using UnityEngine.AI;

namespace DiceFree.Dungeons
{
    public sealed class CrucibleGate : MonoBehaviour
    {
        [SerializeField] Transform bars;
        [SerializeField] Collider barrier;
        [SerializeField] NavMeshObstacle obstacle;
        float lift;
        public bool Open {get;private set;}
        public void Configure(Transform visual,Collider collision,NavMeshObstacle carve){bars=visual;barrier=collision;obstacle=carve;}
        public void SetOpen(bool value)
        {
            Open=value;if(barrier!=null)barrier.enabled=!value;if(obstacle!=null)obstacle.enabled=!value;
            if(!value){lift=0;if(bars!=null)bars.localPosition=Vector3.zero;}
        }
        void Update(){lift=Mathf.MoveTowards(lift,Open?6.5f:0,Time.deltaTime*4);if(bars!=null)bars.localPosition=Vector3.up*lift;}
    }
}
