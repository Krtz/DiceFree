using System;
using DiceFree.Combat;
using UnityEngine;

namespace DiceFree.UI
{
    [DisallowMultipleComponent, RequireComponent(typeof(CombatActor), typeof(NovicePresentationDriver))]
    public sealed class ClassPresentationSwitcher : MonoBehaviour
    {
        [Serializable]
        public sealed class Binding
        {
            public string classId;
            public Transform visualRoot;
            public Animator animator;
        }

        [SerializeField] private Binding[] bindings = Array.Empty<Binding>();

        private CombatActor actor;
        private NovicePresentationDriver driver;
        private ActorFeedback feedback;
        private string currentClassId;

        public string CurrentClassId => currentClassId;
        public Transform ActiveVisualRoot { get; private set; }

        private void Awake()
        {
            actor = GetComponent<CombatActor>();
            driver = GetComponent<NovicePresentationDriver>();
            feedback = GetComponent<ActorFeedback>();
            Refresh(true);
        }

        private void Update() => Refresh(false);

        public void Configure(params Binding[] values)
        {
            bindings = values ?? Array.Empty<Binding>();
            currentClassId = null;
            if (Application.isPlaying) Refresh(true);
            else Apply(actor != null ? actor.Stats.Definition?.stableId : null);
        }

        public Transform Resolve(string classId)
        {
            foreach (var binding in bindings)
                if (binding != null && binding.classId == classId)
                    return binding.visualRoot;
            return null;
        }

        private void Refresh(bool force)
        {
            if (actor == null) actor = GetComponent<CombatActor>();
            string next = actor?.Stats?.Definition?.stableId;
            if (!force && next == currentClassId) return;
            Apply(next);
        }

        private void Apply(string classId)
        {
            Binding active = null;
            foreach (var binding in bindings)
            {
                if (binding?.visualRoot == null) continue;
                bool selected = binding.classId == classId;
                binding.visualRoot.gameObject.SetActive(selected);
                if (selected) active = binding;
            }

            currentClassId = classId;
            ActiveVisualRoot = active?.visualRoot;
            if (active == null) return;

            if (driver == null) driver = GetComponent<NovicePresentationDriver>();
            if (feedback == null) feedback = GetComponent<ActorFeedback>();
            var animator = active.animator != null
                ? active.animator
                : active.visualRoot.GetComponentInChildren<Animator>(true);
            driver?.Configure(active.visualRoot, animator);
            feedback?.Configure(active.visualRoot);
        }
    }
}
