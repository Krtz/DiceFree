namespace DiceFree.Foundation
{
    /// <summary>Small UI-facing snapshot implemented by the application input adapter.</summary>
    public interface ITraversalUiState
    {
        string ModeLabel { get; }
        string Feedback { get; }
    }
}
