using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Input;
using DiceFree.Progression;
using DiceFree.UI;
using UnityEngine;
using Kind=DiceFree.Progression.GreatbladeProgressionRules.Kind;
namespace DiceFree.Visuals
{
    [RequireComponent(typeof(GreatbladeSkillCaster))]
    public sealed class GreatbladePresentation:MonoBehaviour
    {
        GreatbladeSkillCaster caster;
        NovicePresentationDriver driver;
        TraversalInput traversal;
        CombatInput combat;
        bool suppressed,restoreTraversal,restoreCombat;
        LineRenderer telegraph;
        Material material;
        void Awake()
        {
            caster=GetComponent<GreatbladeSkillCaster>();driver=GetComponent<NovicePresentationDriver>();
            traversal=GetComponent<TraversalInput>();combat=GetComponent<CombatInput>();
            var go=new GameObject("Greatblade committed swing outline");go.transform.SetParent(transform,false);
            telegraph=go.AddComponent<LineRenderer>();telegraph.useWorldSpace=true;telegraph.widthMultiplier=.045f;
            material=new Material(Shader.Find("Universal Render Pipeline/Unlit"));material.color=new Color(1,.68f,.22f);
            telegraph.sharedMaterial=material;telegraph.enabled=false;
        }
        void OnEnable(){caster.CastStarted+=Started;caster.CastFinished+=Finished;InputBindings.Current.SuppressionStarted+=Cancel;}
        void OnDisable(){caster.CastStarted-=Started;caster.CastFinished-=Finished;InputBindings.Current.SuppressionStarted-=Cancel;RestoreInputs();if(telegraph!=null)telegraph.enabled=false;}
        void OnDestroy(){if(material!=null)Destroy(material);}
        void Cancel()=>caster.CancelCast();
        void Started(Kind kind)
        {
            if(kind==Kind.BattleCry){EarlySkillVfx.Play(EarlySkillVfx.Cue.PhysicalQuickening,transform.position);return;}
            var animator=driver?.VisualAnimator;
            if(animator!=null){if(kind==Kind.SpinDash)animator.SetTrigger("SpinDash");else driver.PlayAttackAnimation();}
            if(kind==Kind.WideArc)
            {
                int mastery=GetComponent<DiceFree.Skills.GreatbladeSkillProgression>().Rank(Kind.GreatswordMastery);
                float angle=caster.Tuning.arcAngle+(mastery>=3?15+3*(mastery-3):0);
                float radius=GetComponent<CombatActor>().Radius+caster.Tuning.arcReach;
                var points=new Vector3[26];points[0]=transform.position+Vector3.up*.12f;points[25]=points[0];
                for(int i=0;i<24;i++)points[i+1]=points[0]+Quaternion.AngleAxis(-angle*.5f+angle*i/23f,Vector3.up)*transform.forward*radius;
                telegraph.positionCount=points.Length;telegraph.SetPositions(points);telegraph.enabled=true;
            }
        }
        void Finished(Kind kind)
        {
            if(telegraph!=null)telegraph.enabled=false;
            if(kind==Kind.OverheadBreaker&&caster.LastCastHits>0)
                EarlySkillVfx.Play(EarlySkillVfx.Cue.PhysicalHeavy,transform.position+transform.forward*caster.Tuning.breakerReach);
        }
        void Update()
        {
            if(caster.Busy&&!suppressed)
            {
                restoreTraversal=traversal!=null&&traversal.enabled;restoreCombat=combat!=null&&combat.enabled;
                if(restoreTraversal)traversal.enabled=false;if(restoreCombat)combat.enabled=false;suppressed=true;
            }
            else if(!caster.Busy)
            {
                if(suppressed)RestoreInputs();
                if(telegraph!=null)telegraph.enabled=false;
            }
        }
        void LateUpdate()
        {
            if(!caster.Busy||GetComponent<ActorStats>().Definition?.stableId!=GreatbladeProgressionRules.ClassId)return;
            var animator=driver?.VisualAnimator;if(animator==null)return;
            if(caster.Feedback==Kind.SpinDash.ToString()&&!animator.GetCurrentAnimatorStateInfo(0).IsName("Spin Dash"))
                animator.CrossFade("Spin Dash",.05f,0);
        }
        void RestoreInputs(){if(suppressed){if(restoreTraversal&&traversal!=null)traversal.enabled=true;if(restoreCombat&&combat!=null)combat.enabled=true;}suppressed=false;}
    }
}
