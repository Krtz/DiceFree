using UnityEngine;
namespace DiceFree.UI
{
    /// <summary>
    /// Gameplay actor presentation seam. The HUD is intentionally unaware of
    /// individual enemy/model implementations and scene-specific assemblies.
    /// </summary>
    public interface IInspectableVisual
    {
        Transform DisplayModel { get; }
    }
}
