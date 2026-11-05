namespace DiceFree.UI
{
    // Bone adjustments must run before props follow those bones in read-only previews.
    public interface IPortraitBonePose : IPortraitPoseable { }
    public interface IPortraitPoseable { void ApplyPortraitPose(); }
}
