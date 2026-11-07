using UnityEngine;

namespace DiceFree.Advancement
{
    public sealed class PriestEscortNotice : MonoBehaviour
    {
        private string message;
        public static void Show(string value)
        {
            var notice = new GameObject("Escort outcome").AddComponent<PriestEscortNotice>();
            notice.message = value;
            Destroy(notice.gameObject, 8);
        }

        private void OnGUI() => GUI.Box(new Rect((Screen.width - 520) / 2f, 74, 520, 31), message);
    }
}
