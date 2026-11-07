using DiceFree.Combat;
using UnityEngine;

namespace DiceFree.Development
{
    [RequireComponent(typeof(Health))]
    public sealed class PriestPracticeAlly : MonoBehaviour
    {
        [SerializeField, Range(0.01f, 1f)] private float startingHealth = .35f;

        private void Start() => ResetWounds();
        public void Configure(float fraction) => startingHealth = Mathf.Clamp(fraction, .01f, 1f);
        public void ResetWounds() => GetComponent<Health>().Restore(startingHealth);
    }
}
