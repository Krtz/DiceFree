using DiceFree.Combat;
using UnityEngine;

namespace DiceFree.Development
{
    [RequireComponent(typeof(Health))]
    public sealed class PriestPracticeAlly : MonoBehaviour
    {
        [SerializeField, Range(0.01f, 1f)] private float startingHealth = .35f;
        [SerializeField] private bool startsFallen;

        private void Start() => ResetWounds();
        public void Configure(float fraction) => startingHealth = Mathf.Clamp(fraction, .01f, 1f);
        public void ConfigureFallen() => startsFallen = true;

        public void ResetWounds()
        {
            var health = GetComponent<Health>();
            health.Restore(startingHealth);
            if (startsFallen) health.ApplyDamage(null, new DamageResult { mitigated = health.Maximum });
        }
    }
}
