using System;using DiceFree.Combat;using UnityEngine;
namespace DiceFree.Briarback
{
    [DefaultExecutionOrder(150)]
    public sealed class QuilboarPresentation:MonoBehaviour
    {
        public Animator model;
        CombatActor actor;BasicAttack basic;QuilboarCombat creature;BriarbackChieftainEncounter chieftain;Vector3 previous;string state;float hold;bool winding,axeThrowing;
        void Awake(){actor=GetComponent<CombatActor>();basic=GetComponent<BasicAttack>();creature=GetComponent<QuilboarCombat>();chieftain=GetComponent<BriarbackChieftainEncounter>();previous=transform.position;}
        void Play(string value,float seconds=0){if(state==value)return;state=value;hold=Time.time+seconds;model.CrossFadeInFixedTime(value,.10f,0);}
        void LateUpdate(){
            if(model==null)return;
            float speed=Vector3.Distance(transform.position,previous)/Mathf.Max(.001f,Time.deltaTime);previous=transform.position;
            if(!actor.Alive){model.speed=1;Play("Death");return;}
            if(chieftain!=null&&chieftain.Winding){model.speed=1;Play("Slam",1.5f);return;}
            if(creature.Winding){model.speed=1;Play("Slam",1.1f);return;}
            if(actor.Effects?.Stunned==true){model.speed=0;return;}
            bool throwing=chieftain!=null&&chieftain.AxeThrowStarted;
            if(throwing&&!axeThrowing){state="";model.speed=1;Play("Attack",.65f);}
            axeThrowing=throwing;
            bool now=basic.State=="Wind-up";
            if(now&&!winding){state="";model.speed=1;
                // Match the actual BasicAttack impact clock. Brute definitions
                // author 1.2 seconds before attack-speed scaling.
                float attackWindup=basic.Definition.windup/actor.Stats.AttackSpeed;
                Play("Attack",attackWindup);
                if((creature.role==QuilboarRole.SpearThrower&&!creature.SpearInMelee)||creature.IsShaman)
                    QuilboarProjectile.Launch(actor,basic.Target,basic.Definition.windup/actor.Stats.AttackSpeed);
            }
            winding=now;
            if(Time.time<hold){model.speed=1;return;}
            model.speed=speed>.2f?Mathf.Clamp(speed/2.8f,.65f,1.65f):1;
            Play(speed>.2f?"Locomotion":"Idle");
        }
    }
}
