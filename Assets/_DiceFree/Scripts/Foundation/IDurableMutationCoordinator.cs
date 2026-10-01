using System;

namespace DiceFree.Foundation
{
    /// <summary>
    /// Allows one synchronous domain mutation to defer durable snapshots until its
    /// complete state has committed or rolled back. Infrastructure owns the scope.
    /// </summary>
    public interface IDurableMutationCoordinator
    {
        IDisposable DeferDurableWrites();
    }

    /// <summary>Small UI-facing snapshot implemented by the application input adapter.</summary>
    public interface ITraversalUiState
    {
        string ModeLabel { get; }
        string Feedback { get; }
    }
}
