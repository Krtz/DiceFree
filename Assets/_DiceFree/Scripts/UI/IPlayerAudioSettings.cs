namespace DiceFree.UI
{
    public interface IPlayerAudioSettings
    {
        float SfxLevel { get; }
        float AmbientLevel { get; }
        void SetLevels(float sfx,float ambience);
    }
}
