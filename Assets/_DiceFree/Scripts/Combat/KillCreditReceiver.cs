using System;
using System.Collections.Generic;
using UnityEngine;

namespace DiceFree.Combat
{
    [DisallowMultipleComponent, RequireComponent(typeof(CombatActor))]
    public sealed class KillCreditReceiver : MonoBehaviour
    {
        private CombatActor owner;
        private readonly HashSet<long> received = new();
        public event Action<ActorDefeated> Credited;
        private void Awake() => owner = GetComponent<CombatActor>();
        private void OnEnable() => DefeatEvents.Reported += Receive;
        private void OnDisable() => DefeatEvents.Reported -= Receive;
        private void Receive(ActorDefeated defeat)
        {
            if (defeat.creditOwner != owner || defeat.victim == owner || !received.Add(defeat.sequence)) return;
            Credited?.Invoke(defeat);
        }
    }
}
