using DiceFree.Combat;
using UnityEngine;
namespace DiceFree.Briarback
{
    [DefaultExecutionOrder(150)]
    public sealed class QuilboarPresentation : MonoBehaviour
    {
        public Animator model;
        CombatActor actor;BasicAttack basic;QuilboarCombat creature;BriarbackChieftainEncounter chieftain;
        Vector3 previous;string state;float hold,actionSpeed=1;bool winding,smashing,axeThrowing;
        public string PresentationState => state;
        public float ActionSpeed => actionSpeed;
        void Awake(){actor=GetComponent<CombatActor>();basic=GetComponent<BasicAttack>();creature=GetComponent<QuilboarCombat>();chieftain=GetComponent<BriarbackChieftainEncounter>();previous=transform.position;}
        void Play(string value,float seconds=0,float speed=1,bool restart=false)
        {
            if(state==value&&!restart)return;
            state=value;hold=Time.time+seconds;actionSpeed=speed;model.speed=speed;
            model.CrossFadeInFixedTime(value,.065f,0,0);
        }
        void Action(string value,float windup,float clipImpact,float clipDuration)
        {
            float speed=clipImpact/Mathf.Max(.05f,windup);
            Play(value,clipDuration/speed,speed,true);
        }
        void LateUpdate()
        {
            if(model==null)return;
            float speed=Vector3.ProjectOnPlane(transform.position-previous,Vector3.up).magnitude/Mathf.Max(.001f,Time.deltaTime);previous=transform.position;
            if(!actor.Alive){winding=smashing=axeThrowing=false;Play("Death");model.speed=1;return;}
            if(state=="Death"){state=null;hold=0;}
            if(actor.Effects?.Stunned==true){model.speed=0;return;}
            bool smash=(chieftain!=null&&chieftain.Winding)||(creature!=null&&creature.Winding);
            if(smash)
            {
                if(!smashing)Action("Slam",chieftain!=null&&chieftain.Winding?1.5f:1.1f,1.2f,1.8f);
                smashing=true;model.speed=actionSpeed;return;
            }
            smashing=false;
            bool throwing=chieftain!=null&&chieftain.AxeThrowStarted;
            if(throwing&&!axeThrowing)Action("Throw",.65f,.68f,1.1f);
            axeThrowing=throwing;
            bool now=basic.State=="Wind-up";
            if(now&&!winding)
            {
                float duration=basic.Definition.windup/actor.Stats.AttackSpeed;
                bool ranged=creature!=null&&creature.role==QuilboarRole.SpearThrower&&!creature.SpearInMelee;
                bool caster=creature!=null&&creature.IsShaman;
                Action(caster?"Cast":ranged?"Throw":"Attack",duration,caster ? .75f : ranged ? .68f : .62f,caster?1.2f:ranged?1.1f:1f);
                if(ranged||caster)QuilboarProjectile.Launch(actor,basic.Target,duration);
            }
            winding=now;
            if(Time.time<hold){model.speed=actionSpeed;return;}
            float playback=speed>.2f?Mathf.Clamp(speed/2.8f,.65f,1.65f):1;
            Play(speed>.2f?"Locomotion":"Idle");model.speed=playback;
        }
    }
}
