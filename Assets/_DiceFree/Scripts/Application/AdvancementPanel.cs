using System.Linq;
using UnityEngine;

namespace DiceFree.Advancement
{
    [DisallowMultipleComponent, RequireComponent(typeof(AdvancementController))]
    public sealed class AdvancementPanel : MonoBehaviour
    {
        [SerializeField] private AdvancementController controller;

        public void Configure(AdvancementController value) => controller = value;

        private void Awake()
        {
            if (controller == null) controller = GetComponent<AdvancementController>();
        }

        private void OnGUI()
        {
            if (controller == null || controller.CurrentClass == null) return;
            var trial = GetComponent<DiceFree.World.MountainTrialTraveller>();
            if (trial != null && !trial.InsideTrial) return;

            var available = controller.Definitions.Where(controller.CanAdvance).ToArray();
            if (available.Length == 0) return;

            const float width = 520;
            float height = 92 + available.Length * 42;
            var box = new Rect((Screen.width - width) / 2f, 28, width, height);
            GUI.Box(box, "Advancement available");
            GUI.Label(new Rect(box.x + 12, box.y + 26, box.width - 24, 22),
                controller.CurrentClass.displayName + " can choose a new Way. The current manifestation will be preserved.");

            float y = box.y + 52;
            foreach (var definition in available)
            {
                string label = string.IsNullOrWhiteSpace(definition.displayName)
                    ? definition.targetClass.displayName
                    : definition.displayName;
                if (GUI.Button(new Rect(box.x + 12, y, box.width - 24, 34), label))
                    controller.TryAdvance(definition);
                y += 42;
            }

            if (!string.IsNullOrEmpty(controller.Feedback))
                GUI.Label(new Rect(box.x + 12, box.yMax - 24, box.width - 24, 20), controller.Feedback);
        }
    }
}
