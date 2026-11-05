namespace DiceFree.UI
{
    /// <summary>
    /// Read-only channel presentation contract. A spell controls its own
    /// start, completion and interruption; the HUD never changes combat state.
    /// Future channelled spells implement this interface on their caster.
    /// </summary>
    public interface IChannelingAbility : DiceFree.Foundation.IChannelingAbility
    {
    }
}
