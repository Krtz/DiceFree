namespace DiceFree.Foundation
{
    // Shared read-only presentation seam; gameplay owns cancellation and timing.
    public interface IChannelingAbility
    {
        bool IsChanneling { get; }
        string ChannelLabel { get; }
        float ChannelDuration { get; }
        float ChannelRemaining { get; }
    }
}
