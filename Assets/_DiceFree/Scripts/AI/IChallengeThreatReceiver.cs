using DiceFree.Combat;

namespace DiceFree.AI
{
    // Bosses can have encounter-specific threat selection alongside ordinary aggro.
    public interface IChallengeThreatReceiver
    {
        bool AllowsForcedChallenge { get; }
        void AddChallengeThreat(CombatActor source, float amount);
    }
}