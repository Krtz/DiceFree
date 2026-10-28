using System;
using DiceFree.Combat;
using UnityEngine;
using UnityEngine.AI;

namespace DiceFree.Briarback
{
    /// <summary>
    /// A small, ordinary-aggro road group. Each member walks between two authored
    /// camp stops and the whole group waits 3-8 seconds at each stop. Combat stays
    /// owned by BasicAttack/AggroBehaviour; this component never shares targets or
    /// raises an alarm.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class QuilboarRoadPatrol : MonoBehaviour
    {
        [SerializeField] private CombatActor[] members = Array.Empty<CombatActor>();
        [SerializeField] private Vector3[] routeTargets = Array.Empty<Vector3>();
        [SerializeField, Range(3f, 8f)] private float minimumStopSeconds = 3f;
        [SerializeField, Range(3f, 8f)] private float maximumStopSeconds = 8f;

        private BasicAttack[] attacks = Array.Empty<BasicAttack>();
        private CombatActor[] previousTargets = Array.Empty<CombatActor>();
        private bool[] orderIssued = Array.Empty<bool>();
        private float[] retryAt = Array.Empty<float>();
        private int destinationStop;
        private float waitUntil;
        private bool waiting;
        private bool arrived;

        public CombatActor[] Members => members;
        public Vector3[] RouteTargets => routeTargets;
        public float MinimumStopSeconds => minimumStopSeconds;
        public float MaximumStopSeconds => maximumStopSeconds;
        public int CurrentStop => destinationStop;
        public bool IsWaitingAtStop => waiting && arrived;
        public float CurrentPauseSeconds { get; private set; }
        public float PauseRemaining => waiting ? Mathf.Max(0, waitUntil - Time.time) : 0;
        public int StopCount => members == null || members.Length == 0 ? 0 : routeTargets.Length / members.Length;

        public void Configure(CombatActor[] groupMembers, Vector3[] targets, float minimumPause = 3f, float maximumPause = 8f)
        {
            if (groupMembers == null || groupMembers.Length != 3)
                throw new ArgumentException("A Briarback road patrol must have exactly three members.", nameof(groupMembers));
            if (targets == null || targets.Length != groupMembers.Length * 2)
                throw new ArgumentException("A road patrol needs one navigation target per member at each of two camp stops.", nameof(targets));
            if (minimumPause < 3f || maximumPause > 8f || maximumPause < minimumPause)
                throw new ArgumentOutOfRangeException(nameof(minimumPause), "Camp pauses must stay inside the approved 3-8 second range.");

            members = groupMembers;
            routeTargets = targets;
            minimumStopSeconds = minimumPause;
            maximumStopSeconds = maximumPause;
            CacheMembers();
        }

        private void Awake() => CacheMembers();

        private void OnEnable()
        {
            CacheMembers();
            destinationStop = 0;
            waiting = false;
            arrived = false;
            Array.Clear(orderIssued, 0, orderIssued.Length);
            Array.Clear(retryAt, 0, retryAt.Length);
        }

        private void Start()
        {
            destinationStop = 0;
            waiting = false;
            arrived = false;
        }

        private void CacheMembers()
        {
            int count = members?.Length ?? 0;
            attacks = new BasicAttack[count];
            previousTargets = new CombatActor[count];
            orderIssued = new bool[count];
            retryAt = new float[count];
            for (int i = 0; i < count; i++)
                attacks[i] = members[i] != null ? members[i].GetComponent<BasicAttack>() : null;
        }

        private void Update()
        {
            if (members == null || members.Length != 3 || StopCount != 2)
                return;

            bool allAtStop = true;
            for (int i = 0; i < members.Length; i++)
            {
                var actor = members[i];
                var attack = attacks[i];
                if (actor == null || !actor.Alive || attack == null)
                    continue;

                if (attack.Target != previousTargets[i])
                {
                    previousTargets[i] = attack.Target;
                    orderIssued[i] = false;
                }
                if (attack.Target != null)
                {
                    allAtStop = false;
                    continue;
                }
                var aggro = actor.GetComponent<DiceFree.AI.AggroBehaviour>();
                if (aggro != null && aggro.Returning)
                {
                    // Let the ordinary leash own movement until it returns the
                    // member home; resume its route after that state clears.
                    orderIssued[i] = false;
                    allAtStop = false;
                    continue;
                }

                Vector3 target = routeTargets[destinationStop * members.Length + i];
                if (Vector3.Distance(actor.transform.position, target) <= 1.15f)
                {
                    if (!arrived)
                        actor.Motor.Stop();
                    continue;
                }

                allAtStop = false;
                if (orderIssued[i] || Time.time < retryAt[i])
                    continue;

                if (actor.Motor.Ready && actor.Motor.MoveTo(target))
                    orderIssued[i] = true;
                else
                    retryAt[i] = Time.time + 0.6f;
            }

            if (!allAtStop)
            {
                arrived = false;
                return;
            }

            if (!arrived)
            {
                arrived = true;
                waiting = true;
                CurrentPauseSeconds = UnityEngine.Random.Range(minimumStopSeconds, maximumStopSeconds);
                waitUntil = Time.time + CurrentPauseSeconds;
                Array.Clear(orderIssued, 0, orderIssued.Length);
                return;
            }

            if (!waiting || Time.time < waitUntil)
                return;

            waiting = false;
            arrived = false;
            destinationStop = 1 - destinationStop;
            Array.Clear(orderIssued, 0, orderIssued.Length);
            Array.Clear(retryAt, 0, retryAt.Length);
        }

        private void OnDisable()
        {
            if (members == null)
                return;
            foreach (var actor in members)
                if (actor != null && actor.Motor != null && actor.GetComponent<BasicAttack>()?.Target == null)
                    actor.Motor.Stop();
            Array.Clear(orderIssued, 0, orderIssued.Length);
        }
    }
}
