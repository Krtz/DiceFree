namespace DiceFree.UI
{
    /// <summary>
    /// Shared player combat command surface. The UI and future class action
    /// bars can arm normal attack/attack-move without depending on the
    /// Assembly-CSharp player input implementation.
    /// </summary>
    public interface ISharedCombatCommands
    {
        void BeginAttackTargeting();
        void CancelAttackMove();
    }
}
