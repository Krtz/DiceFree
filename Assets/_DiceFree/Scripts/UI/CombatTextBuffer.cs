using System.Collections.Generic;
using UnityEngine;

namespace DiceFree.UI
{
    public enum CombatTextKind { Damage, Critical, Healing, Shield, Resistance, Miss }

    /// <summary>Small presentation-only history. All values are already resolved by combat.</summary>
    public sealed class CombatTextBuffer
    {
        public const int Capacity = 8;
        public const float Lifetime = 1.15f;
        public sealed class Entry
        {
            public CombatTextKind kind;
            public float amount, born;
            public Vector3 origin;
            public int lane;
            public string Text => kind switch
            {
                CombatTextKind.Miss => "MISS",
                CombatTextKind.Critical => "CRIT " + Number(amount),
                CombatTextKind.Healing => "+" + Number(amount),
                CombatTextKind.Shield => "Absorb " + Number(amount),
                CombatTextKind.Resistance => "Resist +" + Number(amount),
                _ => "-" + Number(amount)
            };
            private static string Number(float value) => value.ToString(value < 10 ? "0.0" : "0");
        }
        private readonly List<Entry> entries = new(Capacity);
        private int nextLane;
        public IReadOnlyList<Entry> Entries => entries;
        public void Clear() { entries.Clear(); nextLane = 0; }
        public void Tick(float now) => entries.RemoveAll(e => now - e.born >= Lifetime);
        public void Add(CombatTextKind kind, float amount, Vector3 origin, float now)
        {
            if (!float.IsFinite(now) || !float.IsFinite(amount) || amount < 0 ||
                (amount == 0 && kind != CombatTextKind.Miss)) return;
            Tick(now);
            // Coalesce only packets arriving in the same short burst and category.
            // Ordinary separate attacks keep separate rising numbers.
            if (entries.Count > 0)
            {
                var previous = entries[entries.Count - 1];
                if (previous.kind == kind && now - previous.born <= .075f)
                { previous.amount += amount; return; }
            }
            if (entries.Count == Capacity) entries.RemoveAt(0);
            entries.Add(new Entry { kind = kind, amount = amount, origin = origin, born = now, lane = nextLane++ % 4 });
        }
    }
}
