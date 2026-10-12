namespace DiceFree.Combat
{
    /// <summary>
    /// Exposes authored AI aggro that has not yet become a BasicAttack.Target.
    /// Travel abilities must not mistake a chasing enemy for out-of-combat.
    /// </summary>
    public interface IActiveThreatSource
    {
        bool Threatens(CombatActor target);
    }
}
