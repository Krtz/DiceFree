using System;

namespace DiceFree.Combat
{
    // One explicit grant, not actor-wide stats. No default chance or multiplier.
    public sealed class CriticalRule
    {
        public string SourceId { get; }
        public string RuleId { get; }
        public bool Permitted { get; }
        public float Chance { get; }
        public float UnclampedChance { get; }
        public int Priority { get; }
        public float RequestedMultiplier { get; }
        // Caller supplies finalized values after its explicit modifiers; their composition is not decided here.
        public CriticalRule(string sourceId, string ruleId, bool permitted, float chance, float requestedMultiplier, int priority = 0)
        {
            if (string.IsNullOrWhiteSpace(sourceId) || string.IsNullOrWhiteSpace(ruleId))
                throw new ArgumentException("Critical rule requires stable source and rule identity.");
            if (float.IsNaN(chance) || float.IsInfinity(chance))
                throw new ArgumentOutOfRangeException(nameof(chance));
            if (float.IsNaN(requestedMultiplier) || float.IsInfinity(requestedMultiplier) || requestedMultiplier <= 0)
                throw new ArgumentOutOfRangeException(nameof(requestedMultiplier));
            SourceId = sourceId; RuleId = ruleId; Permitted = permitted;
            UnclampedChance = chance; Chance = Math.Max(0, Math.Min(1, chance));
            RequestedMultiplier = requestedMultiplier; Priority = priority;
        }
    }
}
