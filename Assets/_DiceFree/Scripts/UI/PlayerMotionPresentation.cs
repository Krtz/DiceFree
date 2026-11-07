using DiceFree.Combat;
using DiceFree.Items;
using DiceFree.Skills;
using System.Collections.Generic;
using UnityEngine;

namespace DiceFree.UI
{
    /// <summary>Cosmetic Tier I/II motion; gameplay owns movement, damage and cast timing.</summary>
    public sealed class PlayerMotionPresentation : MonoBehaviour
    {
        CombatActor actor;BasicAttack basic;Equipment equipment;
        NoviceSkillCaster novice;PhysicalSkillCaster physical;MagicalSkillCaster magical;
        MageSkillCaster mage;GreatbladeSkillCaster greatblade;PriestSkillCaster priest;
        AnimatorOverrideController sessionController;RuntimeAnimatorController previousController;
        Transform spinVisual;Quaternion spinRest;float spinStarted,committedUntil;
        bool licensedSpin,spinning,committed;float armedDuration=.9f,armedImpact=.48f;
        Animator model;string classId,state;float hold,playback=1,playedAt;bool winding,alive=true,armed;
        public string State => state;
        public string ControllerClassId => classId;
        public bool SupportsCurrentClass => Resource(actor?.Stats.Definition?.stableId)!=null;
        bool controllerTwoHanded;
        public bool UsesTwoHandedStance=>equipment!=null&&equipment.UsesTwoHandedStance;
        void Awake(){actor=GetComponent<CombatActor>();basic=GetComponent<BasicAttack>();equipment=GetComponent<Equipment>();novice=GetComponent<NoviceSkillCaster>();physical=GetComponent<PhysicalSkillCaster>();magical=GetComponent<MagicalSkillCaster>();mage=GetComponent<MageSkillCaster>();greatblade=GetComponent<GreatbladeSkillCaster>();priest=GetComponent<PriestSkillCaster>();}
        void OnEnable(){if(priest!=null)priest.Casted+=PriestCast;if(novice!=null)novice.Casted+=NoviceCast;if(physical!=null)physical.Casted+=PhysicalCast;if(magical!=null)magical.Casted+=MagicalCast;if(mage!=null)mage.Casted+=MageCast;if(greatblade!=null)greatblade.CastStarted+=GreatbladeCast;if(equipment!=null){equipment.Changed+=EquipmentChanged;EquipmentChanged();}if(actor!=null)actor.Health.Damaged+=Hurt;}
        void OnDisable(){if(priest!=null)priest.Casted-=PriestCast;if(novice!=null)novice.Casted-=NoviceCast;if(physical!=null)physical.Casted-=PhysicalCast;if(magical!=null)magical.Casted-=MagicalCast;if(mage!=null)mage.Casted-=MageCast;if(greatblade!=null)greatblade.CastStarted-=GreatbladeCast;if(equipment!=null)equipment.Changed-=EquipmentChanged;if(actor!=null)actor.Health.Damaged-=Hurt;ReleaseController();}
        void EquipmentChanged(){armed=false;if(equipment!=null)foreach(var item in equipment.Slots)if(item.slotId==EquipmentSlot.MainHand.ToString()&&equipment.IsResolved(item))armed=true;}
        static string Resource(string id)=>id switch{"class.novice"=>"Novice","class.physically-blessed-novice"=>"Physical","class.magically-touched-novice" or "class.mage"=>"Magical","class.greatblade"=>"TierTwo/Greatblade","class.priest"=>"TierTwo/Priest",_=>null};
        void RestoreSpin(){if(spinning&&spinVisual!=null)spinVisual.localRotation=spinRest;spinning=false;spinVisual=null;}
        void ReleaseController()
        {
            RestoreSpin();
            if(sessionController!=null){if(model!=null&&model.runtimeAnimatorController==sessionController)model.runtimeAnimatorController=previousController;Destroy(sessionController);sessionController=null;}
            previousController=null;model=null;classId=null;
        }
        RuntimeAnimatorController GreatbladeController(RuntimeAnimatorController fallback)
        {
            armedDuration=.9f;armedImpact=.48f;licensedSpin=false;
            // Reuse optional, already-installed presentation clips at runtime only.
            // No commercial animation is copied into the tracked fallback assets.
            var source=Resources.Load<RuntimeAnimatorController>("GreatbladeLicensed/GreatbladePrototype");
            if(source==null)return fallback;
            var pairs=new List<KeyValuePair<AnimationClip,AnimationClip>>();sessionController=new AnimatorOverrideController(fallback){name="Greatblade synchronized motion"};sessionController.GetOverrides(pairs);
            for(int i=0;i<pairs.Count;i++)
            {
                string target=pairs[i].Key.name switch{"Physical Idle"=>"Greatblade armed ready","Physical Locomotion"=>"Greatblade two-handed carry walk","Physical Attack" or "Physical ArmedAttack"=>"GreatbladeSwing","Greatblade fallback Spin Dash"=>"GreatbladeSpin",_=>null};
                if(target==null)continue;
                foreach(var clip in source.animationClips)if(clip.name==target){pairs[i]=new KeyValuePair<AnimationClip,AnimationClip>(pairs[i].Key,clip);if(target=="GreatbladeSwing"){armedDuration=clip.length;armedImpact=clip.length*.52f;}if(target=="GreatbladeSpin")licensedSpin=true;break;}
            }
            sessionController.ApplyOverrides(pairs);return sessionController;
        }
        bool Ensure(Animator animator)
        {
            string id=actor.Stats.Definition?.stableId;string resource=Resource(id);if(resource==null||animator==null){if(sessionController!=null)ReleaseController();return false;}
            bool twoHanded=UsesTwoHandedStance;
            if(model==animator&&classId==id&&controllerTwoHanded==twoHanded)return true;
            var controller=Resources.Load<RuntimeAnimatorController>("PlayerMotion/"+resource);if(controller==null)return false;
            ReleaseController();model=animator;previousController=model.runtimeAnimatorController;classId=id;state=null;hold=0;winding=false;committed=false;alive=actor.Alive;armedDuration=.9f;armedImpact=.48f;
            controllerTwoHanded=twoHanded;
            model.runtimeAnimatorController=id=="class.greatblade"&&twoHanded?GreatbladeController(controller):controller;model.applyRootMotion=false;model.Rebind();model.Update(0);Play("Idle");return true;
        }
        bool Current()=>Ensure(GetComponent<NovicePresentationDriver>()?.VisualAnimator);
        public void RefreshForEquipment()=>Current();
        void Play(string next,float seconds=0,float speed=1,bool restart=false)
        {
            if(model==null||state==next&&!restart)return;state=next;playedAt=Time.time;hold=Time.time+seconds;playback=speed;model.speed=speed;model.CrossFadeInFixedTime(next,.07f,0,0);
        }
        public bool PlayRequestedAttack()
        {
            if(!Current())return false;Play(BasicState(),.5f,1,true);return true;
        }
        string BasicState()=>classId=="class.priest"?(UsesTwoHandedStance?"ArmedAttack":"Attack"):classId=="class.mage"?"Cast":armed||classId=="class.greatblade"?"ArmedAttack":"Attack";
        public bool Drive(Animator animator,float speed)
        {
            if(!Ensure(animator))return false;
            if(!actor.Alive){RestoreSpin();if(state!="Death")Play("Death",float.PositiveInfinity);model.speed=1;alive=false;winding=false;return true;}
            if(!alive){state=null;hold=0;alive=true;Play("Idle");}
            if(actor.Effects?.Stunned==true){model.speed=0;return true;}
            if(classId=="class.mage"&&mage!=null&&mage.IsChanneling){if(state!="Cast"||model.GetCurrentAnimatorStateInfo(0).normalizedTime>=.95f)Play("Cast",.85f,1,true);model.speed=1;return true;}
            if(classId=="class.priest"&&priest!=null)
            {
                if(priest.IsChanneling){Play("Revive");model.speed=1;return true;}
                if(state=="Revive")hold=0;
            }
            bool now=basic!=null&&basic.State=="Wind-up";
            if(now&&!winding)
            {
                float windup=basic.Definition.windup/Mathf.Max(.1f,actor.Stats.AttackSpeed);bool weapon=armed||classId=="class.greatblade";float rate=(weapon?armedImpact:.48f)/Mathf.Max(.05f,windup);
                Play(BasicState(),(weapon?armedDuration:.85f)/rate,rate,true);
            }
            winding=now;
            if(classId=="class.greatblade"&&greatblade!=null&&committed)
            {
                if(greatblade.Busy)
                {
                    if(state=="Spin Dash"&&!licensedSpin)
                    {
                        if(!spinning){spinVisual=GetComponent<NovicePresentationDriver>()?.VisualRoot;if(spinVisual!=null&&spinVisual!=actor.transform){spinRest=spinVisual.localRotation;spinStarted=Time.time;spinning=true;}}
                        if(spinning)spinVisual.localRotation=spinRest*Quaternion.Euler(0,(Time.time-spinStarted)*720,0);
                    }
                    model.speed=playback;return true;
                }
                if(state=="Spin Dash"||Time.time<committedUntil)hold=0;
                committed=false;RestoreSpin();
            }
            // Movement remains responsive and cancels only the cosmetic recovery.
            // Measured movement includes the frame immediately before Stop().
            // Let a new gesture cross-fade before fresh movement cancels recovery.
            if(speed>.15f&&Time.time>playedAt+.12f&&!now&&state!="Death")hold=0;
            if(Time.time<hold){model.speed=playback;return true;}
            Play(speed>.13f?"Locomotion":"Idle");model.speed=speed>.13f?Mathf.Clamp(speed/5,.4f,1.7f):1;return true;
        }
        void PriestCast(PriestSkill skill)
        {
            if(actor.Stats.Definition?.stableId!=PriestSkillProgression.PriestClassId||!Current())return;
            string gesture=skill switch
            {
                PriestSkill.IceAegis=>"Guard",
                PriestSkill.HolyLight or PriestSkill.WildGrowth or PriestSkill.Revive=>"Heal",
                _=>"Cast"
            };
            Play(gesture,.82f,1,true);
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
        void MageCast(DiceFree.Progression.MageProgressionRules.Kind skill)
        {
            if(actor.Stats.Definition?.stableId!="class.mage"||!Current())return;
            Play(skill==DiceFree.Progression.MageProgressionRules.Kind.MathBarrier?"Guard":"Cast",.8f,1,true);
        }
        void GreatbladeCast(DiceFree.Progression.GreatbladeProgressionRules.Kind skill)
        {
            if(actor.Stats.Definition?.stableId!="class.greatblade"||!Current())return;
            if(skill==DiceFree.Progression.GreatbladeProgressionRules.Kind.BattleCry){Play("Guard",.7f,1,true);return;}
            committed=true;
            if(skill==DiceFree.Progression.GreatbladeProgressionRules.Kind.SpinDash){committedUntil=Time.time+3;Play("Spin Dash",3,1,true);return;}
            bool heavy=skill==DiceFree.Progression.GreatbladeProgressionRules.Kind.OverheadBreaker;
            float windup=heavy?greatblade.Tuning.breakerWindup:greatblade.Tuning.arcWindup;committedUntil=Time.time+windup;
            float rate=(heavy?.55f:armedImpact)/Mathf.Max(.05f,windup);Play(heavy?"Heavy":"ArmedAttack",(heavy?1.05f:armedDuration)/rate,rate,true);
        }
        void Hurt(CombatActor source,DamageResult damage){if(Current()&&actor.Alive&&(state=="Idle"||state=="Locomotion"))Play("Hit",.23f,1.3f,true);}
    }
}
