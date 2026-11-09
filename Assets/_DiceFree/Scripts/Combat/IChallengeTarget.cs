namespace DiceFree.Combat
{
    public interface IChallengeTarget
    {
        bool TryChallenge(CombatActor source, float duration, float extraThreat);
    }
}