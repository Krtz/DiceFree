using System;
using DiceFree.Combat;
using UnityEngine;

namespace DiceFree.World
{
    public readonly struct AreaEntered
    {
        public readonly long sequence;
        public readonly string areaId;
        public readonly CombatActor actor;
        public readonly Vector3 position;
        public AreaEntered(long sequence, string areaId, CombatActor actor)
        { this.sequence = sequence; this.areaId = areaId; this.actor = actor; position = actor.transform.position; }
    }
    public static class AreaEvents
    {
        private static long sequence;
        public static event Action<AreaEntered> Entered;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetSessionLifetime() { sequence = 0; Entered = null; }
        internal static void Report(string id, CombatActor actor) => Entered?.Invoke(new AreaEntered(++sequence, id, actor));
    }
}
