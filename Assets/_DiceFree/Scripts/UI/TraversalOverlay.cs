using UnityEngine;
using DiceFree.Core;
using DiceFree.World;
using DiceFree.Foundation;

namespace DiceFree.UI
{
    public sealed class TraversalOverlay : MonoBehaviour
    {
        // Keep the serialized field name used by Cornberg.unity while widening its type
        // so UI consumes the Foundation contract instead of the Application assembly.
        [SerializeField] private MonoBehaviour input;
        [SerializeField] private ExplorationCamera explorationCamera;
        private ITraversalUiState State => input as ITraversalUiState;
        private GUIStyle title, body;
        private static Rect Panel => new Rect(16, 16, 405, 166);
        public static bool CoversPointer(Vector2 screenPoint) => Panel.Contains(new Vector2(screenPoint.x, Screen.height - screenPoint.y));
        private void OnGUI()
        {
            if (DiceFree.Foundation.ControlBindings.BlockGameplay) return;
            title ??= new GUIStyle(GUI.skin.label) { fontSize = 19, fontStyle = FontStyle.Bold };
            body ??= new GUIStyle(GUI.skin.label) { fontSize = 13, wordWrap = true };
            GUI.Box(Panel, GUIContent.none);
            GUI.Label(new Rect(28, 24, 345, 28), "CORNBERG  /  World 1 blockout", title);
            GUI.Label(new Rect(28, 56, 345, 24), State?.ModeLabel ?? "Movement controls unavailable", body);
            GUI.Label(new Rect(28, 78, 385, 40), "F6: change mode   Space: stop   Wheel: zoom\nV: " +
                (explorationCamera.LookingOut ? "return to isometric view" : "look toward the Great Tree"), body);
            GUI.Label(new Rect(28, 116, 385, 40), "Arrows / middle-drag: pan   Q/E: rotate\nHome: recenter   F: follow " + (explorationCamera.Following ? "ON" : "OFF"), body);
            var surface = input != null ? input.GetComponent<SurfaceTravel>()?.Current : null;
            GUI.Label(new Rect(28, 156, 385, 22), surface != null
                ? $"{surface.Definition.displayName}: +{surface.Definition.speedBonusPercent:0.#}% movement speed"
                : State?.Feedback, body);
        }
        public void Configure(MonoBehaviour controls, ExplorationCamera camera) { input = controls; explorationCamera = camera; }
    }
}
