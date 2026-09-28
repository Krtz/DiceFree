using UnityEngine;
using DiceFree.Characters;
using DiceFree.Core;

namespace DiceFree.UI
{
    public sealed class TraversalOverlay : MonoBehaviour
    {
        [SerializeField] private TraversalInput input;
        [SerializeField] private ExplorationCamera explorationCamera;
        private GUIStyle title, body;
        private static Rect Panel => new Rect(16, 16, 405, 166);
        public static bool CoversPointer(Vector2 screenPoint) => Panel.Contains(new Vector2(screenPoint.x, Screen.height - screenPoint.y));
        private void OnGUI()
        {
            title ??= new GUIStyle(GUI.skin.label) { fontSize = 19, fontStyle = FontStyle.Bold };
            body ??= new GUIStyle(GUI.skin.label) { fontSize = 13, wordWrap = true };
            GUI.Box(Panel, GUIContent.none);
            GUI.Label(new Rect(28, 24, 345, 28), "CORNBERG  /  World 1 blockout", title);
            GUI.Label(new Rect(28, 56, 345, 24), input.Mode == TraversalInput.ControlMode.Classic
                ? "Classic · Right-click ground to move" : "Direct · WASD to move", body);
            GUI.Label(new Rect(28, 78, 385, 40), "F6: change mode   Space: stop   Wheel: zoom\nV: " +
                (explorationCamera.LookingOut ? "return to isometric view" : "look toward the Great Tree"), body);
            GUI.Label(new Rect(28, 116, 385, 40), "Arrows / middle-drag: pan   Q/E: rotate\nHome: recenter   F: follow " + (explorationCamera.Following ? "ON" : "OFF"), body);
            GUI.Label(new Rect(28, 156, 385, 22), input.Feedback, body);
        }
        public void Configure(TraversalInput controls, ExplorationCamera camera) { input = controls; explorationCamera = camera; }
    }
}
