using UnityEngine;
namespace DiceFree.Combat
{
 public interface IIncomingDamageRedirector { float RedirectDamage(float incoming); }
 [CreateAssetMenu(menuName="DiceFree/Combat/Mage tuning")]
 public sealed class MageCombatTuning:ScriptableObject
 {
  public AttackDefinition[] attacks=new AttackDefinition[6];
  public float range=18,chainRange=6,quicksandRadius=3.5f,decayDelay=8,decayInterval=1;
 }
}
