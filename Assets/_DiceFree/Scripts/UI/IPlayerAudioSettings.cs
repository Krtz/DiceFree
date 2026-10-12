namespace DiceFree.UI
{
    public interface IPlayerAudioSettings
    {
        float MasterLevel { get; }
        float SfxLevel { get; }
        float AmbientLevel { get; }
        float FootstepsLevel { get; }
        float WildlifeLevel { get; }
        void SetLevels(float sfx,float ambience);
        void SetExtendedLevels(float master,float sfx,float ambience,
            float footsteps,float wildlife);
    }
}
