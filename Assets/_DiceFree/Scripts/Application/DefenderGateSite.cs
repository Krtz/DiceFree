using System;
using DiceFree.Combat;
using UnityEngine;

namespace DiceFree.Advancement
{
    [DisallowMultipleComponent]
    public sealed class DefenderGateSite : MonoBehaviour
    {
        [Serializable]
        public sealed class Wave
        {
            public string warning;
            public CombatActor[] raiders;
        }

        public const string SceneName = "DefenderGate";
        public const string TrialId = "defense.trimery.gate";
        public CombatActor[] workers;
        public CombatActor[] guards;
        public Wave[] waves;
        public Vector3 playerSpawn;
        public Rect mapBounds;
    }
}