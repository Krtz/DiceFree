using UnityEngine;

namespace DiceFree.Advancement
{
    public sealed class DefenderGateNotice : MonoBehaviour
    {
        private static DefenderGateNotice current;
        private string message;

        internal static void Clear()
        {
            if (current == null) return;
            current.enabled = false;
            Destroy(current.gameObject);
            current = null;
        }

        internal static void Show(string value)
        {
            Clear();
            current = new GameObject("Trimery watch outcome").AddComponent<DefenderGateNotice>();
            current.message = value;
            Destroy(current.gameObject, 7);
        }

        private void OnGUI()
        {
            float width = Mathf.Min(560, Screen.width - 24);
            GUI.Box(new Rect((Screen.width - width) * .5f, 74, width, 31), message);
        }
    }
}