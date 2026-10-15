using UnityEngine;
namespace DiceFree.World
{
    /// <summary>Drives rigged village footsteps from actual displacement; never owns navigation.</summary>
    [DisallowMultipleComponent]
    public sealed class VillageWalkingAnimator : MonoBehaviour
    {
        [SerializeField] private Animator model;
        private Vector3 previous;
        private bool sampled;
        public Animator Model => model;
        public float MeasuredSpeed { get; private set; }
        public void Configure(Animator value)
        {
            model=value;
            if(model==null)return;
            model.applyRootMotion=false;
            model.cullingMode=AnimatorCullingMode.AlwaysAnimate;
            model.updateMode=AnimatorUpdateMode.Normal;
        }
        private void OnEnable(){previous=transform.position;sampled=false;}
        private void LateUpdate()
        {
            if(model==null)return;
            float moved=Vector3.ProjectOnPlane(transform.position-previous,Vector3.up).magnitude;
            previous=transform.position;
            MeasuredSpeed=sampled&&Time.deltaTime>.0001f&&moved<.5f?moved/Time.deltaTime:0;
            sampled=true;
            model.SetFloat("Speed",MeasuredSpeed,.07f,Mathf.Max(.0001f,Time.deltaTime));
            bool walking=model.GetCurrentAnimatorStateInfo(0).IsName("Walk");
            model.speed=walking&&MeasuredSpeed>.05f?Mathf.Clamp(MeasuredSpeed/1.2f,.45f,1.5f):1;
        }
        private void OnDisable(){sampled=false;MeasuredSpeed=0;if(model!=null){model.SetFloat("Speed",0);model.speed=1;}}
    }
}
