using System;

namespace DiceFree.Foundation
{
    /// <summary>
    /// Durable state owned by the Echo rather than one manifestation.
    /// Persistence keeps each implementation in its own optional save section.
    /// </summary>
    public interface IEchoWideDurableState
    {
        string SectionId { get; }
        int Version { get; }
        event Action Changed;

        string CaptureJson();
        void RestoreJson(int version, string json);
        void ResetToDefault();
    }
}
