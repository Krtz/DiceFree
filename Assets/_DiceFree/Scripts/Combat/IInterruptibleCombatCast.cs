namespace DiceFree.Combat
{
    // A cast opts into interruption independently of the owner's stun resistance.
    public interface IInterruptibleCombatCast
    {
        bool TryInterruptCast();
    }
}