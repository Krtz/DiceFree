using DiceFree.Combat;
using DiceFree.Items;
using DiceFree.Skills;
using UnityEngine;

namespace DiceFree.UI
{
    /// <summary>Cosmetic early-class motion; gameplay owns all movement, damage and cast timing.</summary>
    public sealed class PlayerMotionPresentation : MonoBehaviour
    {
        CombatActor actor;BasicAttack basic;Equipment equipment;
        NoviceSkillCaster novice;PhysicalSkillCaster physical;MagicalSkillCaster magical;
        Animator model;string classId,state;float hold,playback=1;bool winding,alive=true,armed;
        public string State => state;
        public string ControllerClassId => classId;
        void Awake(){actor=GetComponent<CombatActor>();basic=GetComponent<BasicAttack>();equipment=GetComponent<Equipment>();novice=GetComponent<NoviceSkillCaster>();physical=GetComponent<PhysicalSkillCaster>();magical=GetComponent<MagicalSkillCaster>();}
        void OnEnable(){if(novice!=null)novice.Casted+=NoviceCast;if(physical!=null)physical.Casted+=PhysicalCast;if(magical!=null)magical.Casted+=MagicalCast;if(equipment!=null){equipment.Changed+=EquipmentChanged;EquipmentChanged();}if(actor!=null)actor.Health.Damaged+=Hurt;}
        void OnDisable(){if(novice!=null)novice.Casted-=NoviceCast;if(physical!=null)physical.Casted-=PhysicalCast;if(magical!=null)magical.Casted-=MagicalCast;if(equipment!=null)equipment.Changed-=EquipmentChanged;if(actor!=null)actor.Health.Damaged-=Hurt;}
        void EquipmentChanged(){armed=false;if(equipment!=null)foreach(var item in equipment.Slots)if(item.slotId==EquipmentSlot.MainHand.ToString()&&equipment.IsResolved(item))armed=true;}
        static string Resource(string id)=>id switch{"class.novice"=>"Novice","class.physically-blessed-novice"=>"Physical","class.magically-touched-novice"=>"Magical",_=>null};
        bool Ensure(Animator animator)
        {
            string id=actor.Stats.Definition?.stableId;string resource=Resource(id);if(resource==null||animator==null)return false;
            if(model==animator&&classId==id)return true;
            var controller=Resources.Load<RuntimeAnimatorController>("PlayerMotion/"+resource);if(controller==null)return false;
            model=animator;classId=id;state=null;hold=0;winding=false;alive=actor.Alive;model.runtimeAnimatorController=controller;model.applyRootMotion=false;model.Rebind();model.Update(0);Play("Idle");return true;
        }
        bool Current()=>Ensure(GetComponent<NovicePresentationDriver>()?.VisualAnimator);
        void Play(string next,float seconds=0,float speed=1,bool restart=false)
        {
            if(model==null||state==next&&!restart)return;state=next;hold=Time.time+seconds;playback=speed;model.speed=speed;model.CrossFadeInFixedTime(next,.07f,0,0);
        }
        public bool PlayRequestedAttack()
        {
            if(!Current())return false;Play(armed?"ArmedAttack":"Attack",.5f,1,true);return true;
        }
        public bool Drive(Animator animator,float speed)
        {
            if(!Ensure(animator))return false;
            if(!actor.Alive){if(state!="Death")Play("Death",float.PositiveInfinity);model.speed=1;alive=false;winding=false;return true;}
            if(!alive){state=null;hold=0;alive=true;Play("Idle");}
            if(actor.Effects?.Stunned==true){model.speed=0;return true;}
            bool now=basic!=null&&basic.State=="Wind-up";
            if(now&&!winding)
            {
                float windup=basic.Definition.windup/Mathf.Max(.1f,actor.Stats.AttackSpeed);float rate=.48f/Mathf.Max(.05f,windup);
                Play(armed?"ArmedAttack":"Attack",(armed? .9f:.85f)/rate,rate,true);
            }
            winding=now;
            // Movement remains responsive and cancels only the cosmetic recovery.
            if(speed>.15f&&!now&&state!="Death")hold=0;
            if(Time.time<hold){model.speed=playback;return true;}
            Play(speed>.13f?"Locomotion":"Idle");model.speed=speed>.13f?Mathf.Clamp(speed/5,.4f,1.7f):1;return true;
        }
        void NoviceCast(NoviceSkillDefinition skill)
        {
            if(!Current())return;
            Play(skill.kind==NoviceSkillKind.StrengthMeleeStun?"Heavy":skill.kind==NoviceSkillKind.SpiritHeal?"Heal":skill.kind==NoviceSkillKind.AgilityAttackSpeedBuff?"Guard":"Cast",.7f,1,true);
        }
        void PhysicalCast(PhysicalSkillDefinition skill){if(Current())Play(skill.kind==PhysicalSkillKind.HeavyStrike?"Heavy":skill.kind==PhysicalSkillKind.Guard?"Guard":"Cast",.7f,1,true);}
        void MagicalCast(MagicalSkillDefinition skill)
        {
            if(!Current())return;float rate=skill.kind==MagicalSkillKind.IceBurst? .5f/Mathf.Max(.1f,skill.iceDelaySeconds):1;
            Play(skill.kind==MagicalSkillKind.Mend?"Heal":"Cast",.85f/rate,rate,true);
        }
        void Hurt(CombatActor source,DamageResult damage){if(Current()&&actor.Alive&&(state=="Idle"||state=="Locomotion"))Play("Hit",.23f,1.3f,true);}
    }
}
