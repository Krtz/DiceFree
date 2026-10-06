namespace DiceFree.Foundation
{
    /// <summary>
    /// Opt-in hook for transient runtime state that must reset when a different manifestation is loaded.
    /// </summary>
    public interface IManifestationSessionState
    {
        void ResetForManifestationLoad();
    }
}
