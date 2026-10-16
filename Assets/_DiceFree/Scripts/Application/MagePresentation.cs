using DiceFree.Combat;using DiceFree.Progression;using DiceFree.UI;using UnityEngine;
namespace DiceFree.Visuals
{
 [DisallowMultipleComponent,RequireComponent(typeof(MageSkillCaster))]
 public sealed class MagePresentation:MonoBehaviour
 {
  MageSkillCaster caster;
  void Awake()=>caster=GetComponent<MageSkillCaster>();
  void OnEnable(){caster??=GetComponent<MageSkillCaster>();caster.Casted+=OnCast;}
  void OnDisable(){if(caster!=null)caster.Casted-=OnCast;}
  void OnCast(MageProgressionRules.Kind kind){GetComponent<ReturnHomeAbility>()?.Cancel();GetComponent<NovicePresentationDriver>()?.PlayAttackAnimation();}
 }
}
