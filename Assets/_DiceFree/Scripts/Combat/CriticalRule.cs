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
        public float RequestedMultiplier { get; }
        public CriticalRule(string sourceId, string ruleId, bool permitted, float chance, float requestedMultiplier)
        {
            if (string.IsNullOrWhiteSpace(sourceId) || string.IsNullOrWhiteSpace(ruleId))
                throw new ArgumentException("Critical rule requires stable source and rule identity.");
            if (float.IsNaN(chance) || float.IsInfinity(chance) || chance < 0 || chance > 1)
                throw new ArgumentOutOfRangeException(nameof(chance));
            if (float.IsNaN(requestedMultiplier) || float.IsInfinity(requestedMultiplier) || requestedMultiplier <= 0)
                throw new ArgumentOutOfRangeException(nameof(requestedMultiplier));
            SourceId = sourceId; RuleId = ruleId; Permitted = permitted;
            Chance = chance; RequestedMultiplier = requestedMultiplier;
        }
    }
}
