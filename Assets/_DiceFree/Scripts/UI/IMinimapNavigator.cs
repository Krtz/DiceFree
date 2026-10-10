using UnityEngine;

namespace DiceFree.UI
{
    /// <summary>
    /// UI-only movement contract. Traversal belongs to the character/gameplay
    /// assembly; the HUD never reaches across into gameplay implementation types.
    /// </summary>
    public interface IMinimapNavigator
    {
        bool TryMoveFromMinimap(Vector3 worldPoint);
    }
}
