using UnityEngine;
using UnityEngine.SceneManagement;
namespace DiceFree.World
{
    /// <summary>Authored overworld extents, shared by initial exploration and dungeon return.</summary>
    public sealed class OverworldMapBounds : MonoBehaviour
    {
        [SerializeField] Rect bounds;
        public Rect Bounds => bounds;
        public void Configure(Rect value) => bounds=value;
        public static Rect ForScene(Scene scene, Rect fallback)
        {
            foreach(var value in Object.FindObjectsByType<OverworldMapBounds>())
                if(value.gameObject.scene==scene)return value.Bounds;
            return fallback;
        }
    }
}
