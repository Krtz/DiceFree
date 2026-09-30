using System;

namespace DiceFree.Combat
{
    // Generic originating action/source; independent of any crit grant or AttackDefinition.
    public sealed class ActionProvenance
    {
        public string ActionId { get; }
        public string SourceId { get; }
        public ActionProvenance(string actionId, string sourceId)
        {
            if (string.IsNullOrWhiteSpace(actionId) || string.IsNullOrWhiteSpace(sourceId))
                throw new ArgumentException("Action requires stable action and source identities.");
            ActionId = actionId; SourceId = sourceId;
        }
    }
}
