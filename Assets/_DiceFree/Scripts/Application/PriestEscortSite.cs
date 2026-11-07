using System;
using DiceFree.Combat;
using UnityEngine;

namespace DiceFree.Advancement
{
    public sealed class PriestEscortSite : MonoBehaviour
    {
        [Serializable]
        public sealed class Encounter
        {
            public int waypoint;
            public CombatActor[] hostiles;
        }

        public const string SceneName = "PriestEscort";
        public const string RouteId = "escort.trimery.cornberg";
        public CombatActor inspector;
        public CombatActor[] guards;
        public Vector3 playerSpawn;
        public Vector3[] route;
        public Encounter[] encounters;
        public Rect mapBounds;
    }
}
