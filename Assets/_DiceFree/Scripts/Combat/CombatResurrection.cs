namespace DiceFree.Combat
{
    public interface ICombatPartyMembership
    {
        bool SameParty(CombatActor other);
    }

    public interface ICombatResurrectionTarget
    {
        bool CanReviveInPlace { get; }
        bool ReviveInPlace();
    }
}
