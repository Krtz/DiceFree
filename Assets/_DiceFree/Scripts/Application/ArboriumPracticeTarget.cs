using DiceFree.Combat;
using UnityEngine;
using DiceFree.World;
namespace DiceFree.Visuals
{
    // An ordinary attackable practice actor, with no reward or autonomous attack authority.
    [RequireComponent(typeof(CombatActor))]
    [DefaultExecutionOrder(650)]
    public sealed class ArboriumPracticeTarget : MonoBehaviour
    {
        CombatActor actor;
        ArboriumInterior interior;
        CapsuleCollider body;
        int floor;
        void Awake()
        {
            actor=GetComponent<CombatActor>();body=GetComponent<CapsuleCollider>();
            actor.Health.Invulnerable=true;
            var attack=GetComponent<BasicAttack>();if(attack!=null)attack.enabled=false;
            interior=GetComponentInParent<ArboriumInterior>();
            if(interior!=null)floor=Mathf.FloorToInt(interior.transform.InverseTransformPoint(transform.position).y/interior.StoreyHeight);
        }
        void Start()=>actor.Motor.SetMotionBlocked("arborium.practice",true);
        // Other storeys cannot attack or display a health bar through the ceiling.
        void LateUpdate()
        {
            bool available=interior==null||interior.Inside&&interior.CurrentFloor==floor;
            actor.enabled=available;if(body!=null)body.enabled=available;
        }
        void OnDisable(){if(actor!=null)actor.enabled=true;if(body!=null)body.enabled=true;}
    }
}
