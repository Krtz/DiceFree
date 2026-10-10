namespace DiceFree.UI
{
    /// <summary>UI bridge into application-level bag dropping and remote banking.</summary>
    public interface IInventoryItemActions
    {
        bool Perform(string instanceId,string action,out string message);
    }
}
